namespace CantineBack.Models.Dtos
{
    public class Token
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime ExpireAt { get; set; } 
    }
}
