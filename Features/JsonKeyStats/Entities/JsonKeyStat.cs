namespace cesar.Features.JsonKeyStats.Entities;

public class JsonKeyStat
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public int Occurrences { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}
