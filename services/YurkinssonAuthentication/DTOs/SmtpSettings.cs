namespace YurkinssonAuthentication.DTOs
{
    //public record SmtpSettings
    //(string Smtp, int Port, string From, string Pass);
    public class SmtpSettings
    {
        public string Smtp { get; set; }
        public int Port { get; set; }
        public string From { get; set; }
        public string Pass { get; set; }
    }
}
