namespace PersonalWebApi.Models.System
{
    public class ResponseLoginDto
    {
        public string Token { get; set; }
        public DateTime TokenExpirationDate { get; set; }
        public int ClientId { get; set; }
    }
}
