using System.ComponentModel.DataAnnotations.Schema;

namespace HuntStats.Models;

[Table("Accolades")]
public class Accolade
{
    public int Id { get; set; }

    public int BloodlineXp { get; set; }
    public int Bounty { get; set; }
    public string Category { get; set; }
    public string Header { get; set; }
    public int EventPoints { get; set; }
    public int Gems { get; set; }
    public int GeneratedGems { get; set; }
    public int Gold { get; set; }
    public int Hits { get; set; }
    public int HunterPoints { get; set; }
    public int HunterXp { get; set; }
    public int Weighting { get; set; }
    public int Xp { get; set; }
    
    public int MatchId { get; set; }
}