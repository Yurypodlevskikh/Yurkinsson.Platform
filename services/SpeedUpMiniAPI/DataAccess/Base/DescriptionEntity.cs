using System.ComponentModel.DataAnnotations;
using DataAccess.Base.Interfaces;

namespace DataAccess.Base
{
    public abstract class DescriptionEntity : BaseEntity, IDescriptionEntity
    {
        [MaxLength(255)]
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}