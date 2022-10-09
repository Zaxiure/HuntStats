using System.ComponentModel.DataAnnotations.Schema;

namespace HuntStats.Models;


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
    
    public List<Accolade> Accolades { get; set; }
    
    public List<HuntEntry> Entries { get; set; }

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

    public string ProfileId { get; set; }
    public String Name { get; set; }
    public int Mmr { get; set; }

    public int KilledByMe { get; set; }
    
    public int DownedByMe { get; set; }
    
    public int KilledByTeammate { get; set; }
    public int DownedByTeammate { get; set; }
    
    public int KilledMe { get; set; }
    
    public int DownedMe { get; set; }

    public int KilledTeammate { get; set; }
    public int DownedTeammate { get; set; }
    
    public bool Proximity { get; set; }
    public bool ProximityTeammate { get; set; }

    public int BountyPickedUp { get; set; }
    
    public int BountyExtracted { get; set; }

    public int StarRating()
    {
        if (Mmr >= 0 && Mmr < 2000)
        {
            return 1;
        }

        if (Mmr >= 2000 && Mmr < 2300)
        {
            return 2;
        }

        if (Mmr >= 2300 && Mmr < 2600)
        {
            return 3;
        }

        if (Mmr >= 2600 && Mmr < 2750)
        {
            return 4;
        }

        if (Mmr >= 2750 && Mmr < 3000)
        {
            return 5;
        }

        return 6;
    }
}