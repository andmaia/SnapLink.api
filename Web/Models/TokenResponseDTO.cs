namespace Web.Models
{
    public class TokenData
    {
        public string Token { get; set; }
    }

    public class TokenResponseDTO
    {
        public TokenData Data { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; }
    }

}
