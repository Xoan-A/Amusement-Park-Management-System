namespace Models.In;

public class SetStrategyRequest
{
    public string StrategyName { get; set; }
    public int? N { get; set; }
    public DateTime CurrentDate { get; set; }
}