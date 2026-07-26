using System.ComponentModel.DataAnnotations;
using DataAccess.Base;

namespace DataAccess.Models;

public class UserInfo : BaseEntity
{
    [Required]
    [MaxLength(225)]
    public string GuidId { get; set; } = string.Empty;
    [Required]
    [MaxLength(256)]
    public string NickName { get; set; } = string.Empty;
    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;
    [MaxLength(512)]
    public string JwtToken { get; set; } = string.Empty;
    [MaxLength(1024)]
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiry { get; set; }
    public int HueDegrees { get; set; } = 0;
    public int MaxSettingsCount { get; set; } = 3;
    public bool IsSupporter { get; set; } = false;
    [MaxLength(50)]
    public string AccountTier { get; set; } = "Free"; // "Free", "Supporter", "Unlimited"
    public DateTime? LastSupportDate { get; set; }
    [MaxLength(100)]
    public string? SupportType { get; set; }
    // Navigation property for MetronomeSettings
    public ICollection<MetronomeSettings> MetronomeSettings { get; set; } = new List<MetronomeSettings>();
}