namespace BusinessLogic.DTOs;

public class MetronomeSettingsReadDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int SourceBpm { get; set; }
    public int TargetBpm { get; set; }
    public int BarsPerPattern { get; set; }
    public int? TempoPeriods { get; set; }
    public bool IsReversed { get; set; }
    public bool IsRepeatPattern { get; set; }
    public int BeatsPerBar { get; set; }
    public bool IsTempoIncreace { get; set; }
    public int? MetronomeCollectionId { get; set; }
    public string? Description { get; set; }
}