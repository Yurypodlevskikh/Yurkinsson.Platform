using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAccess.Base;

namespace DataAccess.Models;

public class MetronomeSettings : DescriptionEntity
{
    [Required]
    public int UserInfoId { get; set; }
    public virtual UserInfo? User { get; set; }
    // Optional foreign key to MetronomeCollectionName
    public int? MetronomeCollectionId { get; set; }
    public virtual MetronomeCollectionName? MetronomeCollection { get; set; }
    [Required]
    [MaxLength(100)]
    public string? Title { get; set; } // Name of the metronome settings
    [Required]
    public int SourceBpm { get; set; } // Source BPM of the metronome settings
    [Required]
    public int TargetBpm { get; set; } // Target BPM of the metronome settings
    public int BarsPerPattern { get; set; } = 0; // Number of bars per pattern
    public int? TempoPeriods { get; set; } // Number of tempo periods
    public bool IsReversed { get; set; } = false; // If the metronome is counting the target value back to the initial value
    public bool IsRepeatPattern { get; set; } = false; // If the metronome is repeating the pattern
    public int BeatsPerBar { get; set; } = 4; // Number of beats per bar
    public bool IsTempoIncreace { get; set; } = true; // If the metronome is increasing the tempo
}