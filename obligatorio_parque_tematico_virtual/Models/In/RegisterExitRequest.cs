namespace Models.In;

public class RegisterExitRequest
{
    public Guid userId { get; set; }
    public DateTime exitDate { get; set; }
}