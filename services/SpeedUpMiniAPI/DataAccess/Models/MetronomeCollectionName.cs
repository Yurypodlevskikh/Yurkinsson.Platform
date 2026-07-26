using System.ComponentModel.DataAnnotations;
using DataAccess.Base;

namespace DataAccess.Models
{
    public class MetronomeCollectionName : DescriptionEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = "string.Empty";
        // Navigation property for MetronomeSettings
        public ICollection<MetronomeSettings> MetronomeSettings { get; set; } = new List<MetronomeSettings>();
    }
}