namespace cesar.Features.JsonKeyStats.Models;

public class JsonKeyStatViewModel
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public int Occurrences { get; set; }
    public DateTime ValidFrom { get; set; }
}
