using System.ComponentModel.DataAnnotations;

namespace YurkinssonAuthentication.Data
{
    public class YurkinIdentityLog
    {
        [Key]
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } // Time of Log entry
        public string Level { get; set; } // Log Level (Info, Error...)
        public string Message { get; set; }
        public string Exception { get; set; } // Stack trace if error
        public string Properties { get; set; } // Additional Log data (JSON)
    }
}
