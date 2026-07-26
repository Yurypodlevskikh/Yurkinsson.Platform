namespace DataAccess.Base.Interfaces
{
    public interface IDescriptionEntity : IBaseEntity
    {
        string? Description { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
    }
}