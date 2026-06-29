using cesar.Extensions;
using cesar.Features.AiTemplateGenerator.Models;
using cesar.Features.DesignTemplates;
using cesar.Features.Identity;
using cesar.Features.RawLead;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using RawLeadEntity = cesar.Features.RawLead.Entities.RawLead;

namespace cesar.Features.AiTemplateGenerator;

[Route("ai-template-generator")]
[Authorize(Roles = AppRoles.AdminOrManager)]
public class AiTemplateGeneratorController : Controller
{
    private readonly IRawLeadService _rawLeadService;
    private readonly IDesignTemplateService _designTemplateService;
    private readonly IAiTemplateGeneratorService _generatorService;

    public AiTemplateGeneratorController(
        IRawLeadService rawLeadService,
        IDesignTemplateService designTemplateService,
        IAiTemplateGeneratorService generatorService)
    {
        _rawLeadService = rawLeadService;
        _designTemplateService = designTemplateService;
        _generatorService = generatorService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        this.SetCurrentPage("AI Template Generator");
        return View(await BuildViewModelAsync(new AiTemplateGeneratorViewModel()));
    }

    [HttpPost("generate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(
        AiTemplateGeneratorViewModel model,
        CancellationToken cancellationToken)
    {
        this.SetCurrentPage("AI Template Generator");
        NormalizeSelection(model);

        var selectedLeads = await LoadSelectedLeadsAsync(model.SelectedRawLeadIds);
        ValidateSelectedLeads(model, selectedLeads);

        if (ModelState.IsValid)
        {
            try
            {
                model.Result = await _generatorService.GenerateAsync(
                    new AiTemplateGenerationRequest(
                        model.GenerationPrompt,
                        model.ContentType,
                        model.OutputLanguage,
                        model.Tone,
                        selectedLeads,
                        model.SelectedJsonPaths),
                    cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        model.RawLeadOptions = await BuildRawLeadOptionsAsync();
        return View(nameof(Index), model);
    }

    [HttpPost("save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(SaveGeneratedTemplateModel model)
    {
        this.SetCurrentPage("AI Template Generator");

        if (!IsValidJson(model.PlaceholderSchema))
        {
            ModelState.AddModelError(nameof(model.PlaceholderSchema), "Placeholder schema must be valid JSON.");
        }

        if (!ModelState.IsValid)
        {
            var viewModel = await BuildViewModelAsync(new AiTemplateGeneratorViewModel
            {
                ContentType = model.ContentType,
                Result = new AiGeneratedTemplateResult
                {
                    Name = model.Name,
                    ContentType = model.ContentType,
                    Content = model.HtmlMarkup,
                    PlaceholderSchema = model.PlaceholderSchema
                }
            });

            return View(nameof(Index), viewModel);
        }

        var template = await _designTemplateService.CreateAsync(
            model.Name.Trim(),
            model.ContentType,
            model.HtmlMarkup,
            PrettyPrint(model.PlaceholderSchema));

        return RedirectToAction("Preview", "DesignTemplate", new { id = template.Id });
    }

    private async Task<AiTemplateGeneratorViewModel> BuildViewModelAsync(AiTemplateGeneratorViewModel model)
    {
        model.RawLeadOptions = await BuildRawLeadOptionsAsync();
        return model;
    }

    private async Task<List<AiRawLeadOptionModel>> BuildRawLeadOptionsAsync()
    {
        var leads = await _rawLeadService.GetAllActiveAsync();
        return leads
            .OrderByDescending(lead => lead.IngestedAt)
            .Select(lead => new AiRawLeadOptionModel
            {
                Id = lead.Id,
                Label = $"#{lead.Id} {lead.SourceSystem} - {lead.ExternalId}".Replace("_", " "),
                RawJsonData = lead.RawJsonData
            })
            .ToList();
    }

    private async Task<List<RawLeadEntity>> LoadSelectedLeadsAsync(IEnumerable<int> selectedRawLeadIds)
    {
        var leads = new List<RawLeadEntity>();

        foreach (var leadId in selectedRawLeadIds.Distinct())
        {
            var lead = await _rawLeadService.GetByIdAsync(leadId);
            if (lead is not null && lead.ValidTo is null)
            {
                leads.Add(lead);
            }
        }

        return leads;
    }

    private void ValidateSelectedLeads(
        AiTemplateGeneratorViewModel model,
        IReadOnlyCollection<RawLeadEntity> selectedLeads)
    {
        if (model.SelectedRawLeadIds.Count == 0)
        {
            ModelState.AddModelError(nameof(model.SelectedRawLeadIds), "Select at least one raw lead.");
            return;
        }

        if (selectedLeads.Count != model.SelectedRawLeadIds.Distinct().Count())
        {
            ModelState.AddModelError(nameof(model.SelectedRawLeadIds), "One or more selected raw leads are no longer active.");
        }
    }

    private static void NormalizeSelection(AiTemplateGeneratorViewModel model)
    {
        model.SelectedRawLeadIds = model.SelectedRawLeadIds
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        model.SelectedJsonPaths = model.SelectedJsonPaths
            .Select(path => path.Trim())
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool IsValidJson(string json)
    {
        try
        {
            JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string PrettyPrint(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return json;
        }
    }
}
