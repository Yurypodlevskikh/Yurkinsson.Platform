namespace BusinessLogic.Models
{
    public class IdentityApiSettings
    {
        public string BaseUrl { get; set; } = string.Empty;	
        public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(5);
    }
}