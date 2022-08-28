using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleApp1.Models;


[Table("Match")]
public class HuntMatchTable
{
    public int Id { get; set; }

    public bool Scrapbeak { get; set; }
    public bool Assassin { get; set; }
    public bool Butcher { get; set; }
    public bool Spider { get; set; }
    public DateTime DateTime { get; set; }
}
public class HuntMatch : HuntMatchTable
{
    public List<Team> Teams { get; set; }

    public int TotalKillsWithTeammate { get; set; }
    
    public int TotalKills { get; set; }
    
    public int TotalDeaths { get; set; }
}
[Table("Teams")]
public class TeamTable
{
    public int Id { get; set; }
    public int Mmr { get; set; }

    public string Players { get; set; }

    public int MatchId { get; set; }
}

public class Team
{
    public int Id { get; set; }
    public int Mmr { get; set; }
    public List<Player> Players { get; set; }
}

public class Player
{
    public int Id { get; set; }

    public int ProfileId { get; set; }
    public String Name { get; set; }
    public int Mmr { get; set; }

    public int KilledByMe { get; set; }
    
    public int KilledByTeammate { get; set; }
    
    public int KilledMe { get; set; }

    public int KilledTeammate { get; set; }
    
    public bool Proximity { get; set; }
    public bool ProximityTeammate { get; set; }

    public int BountyPickedUp { get; set; }
    
    public int BountyExtracted { get; set; }
}