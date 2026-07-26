namespace BusinessLogic.Models
{
    public class ConfirmEmail
    {
        public required string userId { get; set; }
        public required string token { get; set; }
    }
}