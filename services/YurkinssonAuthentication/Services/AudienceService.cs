using YurkinssonAuthentication.Services.Interfaces;

namespace YurkinssonAuthentication.Services
{
    public class AudienceService : IAudienceService
    {
        private readonly IEnumerable<string> _audiences;

        public AudienceService(IEnumerable<string> audiences)
        {
            _audiences = audiences ?? throw new ArgumentNullException(nameof(audiences));
        }

        public string ExtractAudiecneFromReferer(HttpRequest request)
        {
            var referer = request.Headers["Referer"].FirstOrDefault();
            var audArray = _audiences;
            if (string.IsNullOrEmpty(referer))
                return "fake-audience";

            try
            {
                var uri = new Uri(referer);
                return DetermineAudience(uri.Host);
            }
            catch (UriFormatException)
            {
                return "fake-audience";
            }
        }

        public string ExtractAudiecneFromOrigin(HttpRequest request)
        {
            string? origin;

            if(request.Headers.TryGetValue("Origin", out var origins))
            {
                origin = origins.FirstOrDefault();
            }
            else
                return "fake-audience";

            if (string.IsNullOrEmpty(origin))
                return "fake-audience";

            var audArray = _audiences;

            try
            {
                var uri = new Uri(origin);
                return DetermineAudience(uri.Host);
            }
            catch (UriFormatException)
            {
                return "fake-audience";
            }
        }

        public string DetermineAudience(string? host)
        {
            if (host == null)
                return "fake-audience";

            var audience = _audiences.FirstOrDefault(a => new Uri(a).Host == host);

            return audience ?? "fake-audience";
        }
        public IEnumerable<string> GetAudiences()
        {
            return _audiences;
        }
    }
}
