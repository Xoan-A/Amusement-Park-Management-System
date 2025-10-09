namespace Models.Out
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string[] Roles { get; set; }
        public string Name { get; set; }
    }
}