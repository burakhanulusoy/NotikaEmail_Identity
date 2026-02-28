namespace NotikaEmail_Identity.Models
{
    public class AdminLogViewModel
    {
        public DateTimeOffset Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string LogColor { get; set; } // Tabloda renkli görünmesi için (Sarı, Mavi, Kırmızı)
    }
}
