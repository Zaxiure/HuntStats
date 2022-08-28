using System.ComponentModel.DataAnnotations.Schema;

namespace HuntStats.Models;

[Table("Entries")]
public class Entry
{
    public int Id { get; set; }
    public int Amount { get; set; }
    public string Category { get; set; }
    public string DescriptorName { get; set; }
    public int DescriptorScore { get; set; }
    public int DescriptorType { get; set; }
    public int Reward { get; set; }
    public int RewardSize { get; set; }
    public string UiName { get; set; }
    public string UiName2 { get; set; }

    public int MatchId { get; set; }
}