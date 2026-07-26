namespace YurkinssonAuthentication.Services.Interfaces
{
    public interface IAudienceService
    {
        string ExtractAudiecneFromReferer(HttpRequest request);
        string ExtractAudiecneFromOrigin(HttpRequest request);
        string DetermineAudience(string? host);
        IEnumerable<string> GetAudiences();
    }
}
