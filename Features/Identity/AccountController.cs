using System.Security.Claims;
using cesar.Features.Identity.Entities;
using cesar.Features.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace cesar.Features.Identity;

public class AccountController : Controller
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;

    public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(GetSafeReturnUrl(returnUrl));
        }

        return View(new LoginModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return LocalRedirect(GetSafeReturnUrl(model.ReturnUrl));
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(GetSafeReturnUrl(returnUrl));
        }

        return View(new RegisterModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var isFirstUser = !_userManager.Users.Any();
        var user = new AppUser
        {
            UserName = model.Email,
            Email = model.Email,
            OIB = model.OIB,
            JMBG = model.JMBG
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, isFirstUser ? AppRoles.Admin : AppRoles.Manager);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(GetSafeReturnUrl(model.ReturnUrl));
        }

        AddErrors(result);
        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError is not null)
        {
            ModelState.AddModelError(string.Empty, $"External provider error: {remoteError}");
            return View(nameof(Login), new LoginModel { ReturnUrl = returnUrl });
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var signInResult = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: true);

        if (signInResult.Succeeded)
        {
            return LocalRedirect(GetSafeReturnUrl(returnUrl));
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        return View("ExternalLoginConfirmation", new ExternalLoginConfirmationModel
        {
            Email = email,
            LoginProvider = info.ProviderDisplayName ?? info.LoginProvider,
            ReturnUrl = returnUrl
        });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationModel model)
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            return RedirectToAction(nameof(Login), new { model.ReturnUrl });
        }

        if (!ModelState.IsValid)
        {
            model.LoginProvider = info.ProviderDisplayName ?? info.LoginProvider;
            return View(model);
        }

        var isFirstUser = !_userManager.Users.Any();
        var user = new AppUser
        {
            UserName = model.Email,
            Email = model.Email,
            OIB = model.OIB,
            JMBG = model.JMBG
        };

        var createResult = await _userManager.CreateAsync(user);
        if (createResult.Succeeded)
        {
            var loginResult = await _userManager.AddLoginAsync(user, info);
            if (loginResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, isFirstUser ? AppRoles.Admin : AppRoles.Manager);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(GetSafeReturnUrl(model.ReturnUrl));
            }

            AddErrors(loginResult);
            return View(model);
        }

        AddErrors(createResult);
        model.LoginProvider = info.ProviderDisplayName ?? info.LoginProvider;
        return View(model);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied() => View();

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private string GetSafeReturnUrl(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : Url.Action("Index", "Home") ?? "/";
}
