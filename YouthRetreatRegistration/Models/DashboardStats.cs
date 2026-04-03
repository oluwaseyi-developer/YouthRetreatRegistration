namespace YouthRetreatRegistration.Models;

public class DashboardStats
{
    public int TotalRegistrations { get; set; }
    public int MaleCount { get; set; }
    public int FemaleCount { get; set; }
    public int OtherBranchCount { get; set; }
    public int AttendedCount { get; set; }
    public Dictionary<string, int> AgeRangeBreakdown { get; set; } = new();
}
