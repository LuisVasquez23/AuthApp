namespace AuthApp.Api.Configurations
{
    public class EmailSettings
    {
        public const string SectionName = "EmailSettings";
        public string Email { get; set; } = string.Empty;   
        public string Password { get; set; } = string.Empty;
    }
}
