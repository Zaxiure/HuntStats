using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using ConsoleApp1.Models;
using Dommel;
using HuntStats.Data;
using HuntStats.Models;
using HuntStats.State;
using MediatR;
using Newtonsoft.Json;
using Entry = HuntStats.Models.Entry;

namespace HuntStats.Features;

public class XmlFileQuery : IRequest<GeneralStatus>
{
    
}

public class Attributes
{
    [XmlElement(ElementName = "Attr")]
    public List<Attr> Atrributes { get; set; }

    // public Dictionary<string, string> AttributesDict { get; set; }
}


public class Attr
{
    [XmlAttribute(AttributeName = "name")]
    public string Name { get; set; }
    [XmlAttribute(AttributeName = "value")]
    public string Value { get; set; }
}

public class XmlFileQueryHandler : IRequestHandler<XmlFileQuery, GeneralStatus>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly AppState _appState;
    
    public XmlFileQueryHandler(IDbConnectionFactory connectionFactory, AppState appState)
    {
        _connectionFactory = connectionFactory;
        _appState = appState;
    }

    public bool CheckSameMatch(string filePath1, string filePath2)
    {
        XmlSerializer reader = new XmlSerializer(typeof(Attributes));  
        XmlSerializer reader2 = new XmlSerializer(typeof(Attributes));
        var fileText = File.ReadAllText(filePath1);
        var fileText2 = File.ReadAllText(filePath2);
        var file = new StringReader(fileText);
        var file2 = new StringReader(fileText2);
        Attributes overview =  (Attributes)reader.Deserialize(file);
        Attributes overview2 =  (Attributes)reader2.Deserialize(file2);
        
        var attributes = new Dictionary<string, string>();
    
        foreach (var attr in overview.Atrributes)
        {
            attributes.Add(attr.Name, attr.Value);
        }
        
        var attributes2 = new Dictionary<string, string>();
    
        foreach (var attr in overview2.Atrributes)
        {
            attributes2.Add(attr.Name, attr.Value);
        }
    
        var total = 0;
        var succes = 0;
        
        var numberOfTeams = Convert.ToInt32(attributes.FirstOrDefault(x => x.Key == "MissionBagNumTeams").Value);
        var numberOfEntries = Convert.ToInt32(attributes.FirstOrDefault(x => x.Key == "MissionBagNumEntries").Value);
        var numberOfAccolades = Convert.ToInt32(attributes.FirstOrDefault(x => x.Key == "MissionBagNumAccolades").Value);
        
        var numberOfTeams2 = Convert.ToInt32(attributes.FirstOrDefault(x => x.Key == "MissionBagNumTeams").Value);
        var numberOfEntries2 = Convert.ToInt32(attributes.FirstOrDefault(x => x.Key == "MissionBagNumEntries").Value);
        var numberOfAccolades2 = Convert.ToInt32(attributes.FirstOrDefault(x => x.Key == "MissionBagNumAccolades").Value);
    
        for (int i = 0; i < numberOfAccolades; i++)
        {
            foreach (var accoladeEntry in attributes.Where(x => x.Key.Contains("MissionAccoladeEntry_" + i)))
            {
                total += 1;
                if (attributes2[accoladeEntry.Key] == accoladeEntry.Value)
                {
                    succes += 1;
                }
            }
        }
        
        for (int i = 0; i < numberOfEntries; i++)
        {
            foreach (var missionBagEntry in attributes.Where(x => x.Key.Contains("MissionBagEntry_" + i)))
            {
                total += 1;
                if (attributes2[missionBagEntry.Key] == missionBagEntry.Value)
                {
                    succes += 1;
                }
            }
        }
        
        for (int i = 0; i < numberOfTeams; i++)
        {
            foreach (var playerEntry in attributes.Where(x => x.Key.Contains("MissionBagPlayer_" + i)))
            {
                total += 1;
                if (attributes2[playerEntry.Key] == playerEntry.Value)
                {
                    succes += 1;
                }
            }
        }
        
        var percentage = Convert.ToInt32((double)succes / total * 100);
        if (percentage > 80)
        {
            return true;
        }
    
        return false;
        //     }
        // }
    }

    public async Task CopyWhenFileUnlocked(string path, string path2)
    {
        try
        {
            File.Copy(path, path2);
        }
        catch (Exception e)
        {
            await Task.Delay(100);
            await CopyWhenFileUnlocked(path, path2);
        }
    }
    
    public async Task DeleteWhenFileUnlocked(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception e)
        {
            await DeleteWhenFileUnlocked(path);
        }
    }
    
    // This needs to be cleaned up because holy shit....
    public async Task<GeneralStatus> Handle(XmlFileQuery request, CancellationToken cancellationToken)
    {
        using var con = await _connectionFactory.GetOpenConnectionAsync();
        var settings = await con.FirstOrDefaultAsync<Settings>(x => x.Id == 1);
        var appdataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\HuntStats";
        var huntFilePath = settings.Path + @"\user\profiles\default\attributes.xml";
        var huntFileTempPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\HuntStats\attributes.xml";
        
        if(!Directory.Exists(appdataDirectory)) Directory
            .CreateDirectory(appdataDirectory);

        if (!File.Exists(huntFilePath)) return GeneralStatus.Error;

        HuntMatchTable foundMatch = null;

        bool sameFile = false;

        if (!File.Exists(huntFileTempPath))
        {
            await CopyWhenFileUnlocked(huntFilePath, huntFileTempPath);
            // await CopyWhenFileUnlocked(huntFileTempPath, huntFileTempPath + ".tmp");
        }
        else
        {
            // await CopyWhenFileUnlocked(huntFilePath, huntFileTempPath + ".tmp");
            sameFile = CheckSameMatch(huntFilePath, huntFileTempPath);
        }
        
        if(!sameFile)
        {
            File.Delete(huntFileTempPath);
            await CopyWhenFileUnlocked(huntFilePath, huntFileTempPath);
            // File.Copy(huntFilePath, huntFileTempPath);
            // File.Delete(huntFileTempPath + ".tmp");
            XmlSerializer reader = new XmlSerializer(typeof(Attributes));  
            StreamReader file = new System.IO.StreamReader(huntFileTempPath);  
            
    
            Attributes overview =  (Attributes)reader.Deserialize(file);
            
            var match = new HuntMatch()
            {
                Teams = new List<Team>(),
                DateTime = DateTime.UtcNow
            };
    
    
            var attributes = new Dictionary<string, string>();
    
            foreach (var attr in overview.Atrributes)
            {
                attributes.Add(attr.Name, attr.Value);
            }
            
            var numberOfTeams = Convert.ToInt32(attributes.FirstOrDefault(x => x.Key == "MissionBagNumTeams").Value);
            var numberOfEntries = Convert.ToInt32(attributes.FirstOrDefault(x => x.Key == "MissionBagNumEntries").Value);
            var numberOfAccolades = Convert.ToInt32(attributes.FirstOrDefault(x => x.Key == "MissionBagNumAccolades").Value);

            var entries = new List<Entry>();
            var accolades = new List<Accolade>();

            for (int i = 0; i < numberOfEntries; i++)
            {
                entries.Add(new Entry()
                {
                    Amount = Convert.ToInt32(attributes["MissionBagEntry_" + i + "_amount"]),
                    Category = attributes["MissionBagEntry_" + i + "_category"],
                    DescriptorName = attributes["MissionBagEntry_" + i + "_descriptorName"],
                    DescriptorScore = Convert.ToInt32(attributes["MissionBagEntry_" + i + "_descriptorScore"]),
                    DescriptorType = Convert.ToInt32(attributes["MissionBagEntry_" + i + "_descriptorType"]),
                    Reward = Convert.ToInt32(attributes["MissionBagEntry_" + i + "_reward"]),
                    RewardSize = Convert.ToInt32(attributes["MissionBagEntry_" + i + "_rewardSize"]),
                    UiName = attributes["MissionBagEntry_" + i + "_uiName"],
                    UiName2 = attributes["MissionBagEntry_" + i + "_uiName2"],
                });
            }

            for (int i = 0; i < numberOfAccolades; i++)
            {
                accolades.Add(new Accolade()
                {
                    BloodlineXp = Convert.ToInt32(attributes["MissionAccoladeEntry_" + i + "_bloodlineXp"]),
                    Bounty = Convert.ToInt32(attributes["MissionAccoladeEntry_" + i + "_bounty"]),
                    Category = attributes["MissionAccoladeEntry_" + i + "_category"],
                    EventPoints = Convert.ToInt32(attributes["MissionAccoladeEntry_" + i + "_eventPoints"]),
                    Gems = Convert.ToInt32(attributes["MissionAccoladeEntry_" + i + "_gems"]),
                    GeneratedGems = Convert.ToInt32(attributes["MissionAccoladeEntry_" + i + "_generatedGems"]),
                    Gold = Convert.ToInt32(attributes["MissionAccoladeEntry_" + i + "_gold"]),
                    Header = attributes["MissionAccoladeEntry_" + i + "_header"],
                    Hits = Convert.ToInt32(attributes["MissionAccoladeEntry_" + i + "_hits"]),
                    HunterPoints = Convert.ToInt32(attributes["MissionAccoladeEntry_" + i + "_hunterPoints"]),
                    HunterXp = Convert.ToInt32(attributes["MissionAccoladeEntry_" + i + "_hunterXp"]),
                    Weighting = Convert.ToInt32(attributes["MissionAccoladeEntry_" + i + "_weighting"]),
                    Xp = Convert.ToInt32(attributes["MissionAccoladeEntry_" + i + "_xp"])
                });
            }
            
            for (int i = 0; i < numberOfTeams; i++)
            {
                var playerAmount = Convert.ToInt32(attributes["MissionBagTeam_" + i + "_numplayers"]);
                var team = new Team
                {
                    Id = i,
                    Players = new List<Player>(),
                    Mmr = Convert.ToInt32(attributes["MissionBagTeam_" + i + "_mmr"]),
                };
    
                for (int j = 0; j < playerAmount; j++)
                {
                    team.Players.Add(new Player()
                    {
                        Name = attributes["MissionBagPlayer_" + i + "_" + j +"_blood_line_name"],
                        Mmr = Convert.ToInt32(attributes["MissionBagPlayer_" + i + "_" + j + "_mmr"]),
                        KilledMe = Convert.ToInt32(attributes["MissionBagPlayer_" + i + "_" + j + "_killedme"]),
                        DownedMe = Convert.ToInt32(attributes["MissionBagPlayer_" + i + "_" + j + "_downedme"]),
                        KilledTeammate = Convert.ToInt32(attributes["MissionBagPlayer_" + i + "_" + j + "_killedteammate"]),
                        DownedTeammate = Convert.ToInt32(attributes["MissionBagPlayer_" + i + "_" + j + "_downedteammate"]),
                        KilledByMe = Convert.ToInt32(attributes["MissionBagPlayer_" + i + "_" + j + "_killedbyme"]),
                        DownedByMe = Convert.ToInt32(attributes["MissionBagPlayer_" + i + "_" + j + "_downedbyme"]),
                        KilledByTeammate = Convert.ToInt32(attributes["MissionBagPlayer_" + i + "_" + j + "_killedbyteammate"]),
                        DownedByTeammate = Convert.ToInt32(attributes["MissionBagPlayer_" + i + "_" + j + "_downedbyteammate"]),
                        BountyExtracted = Convert.ToInt32(attributes["MissionBagPlayer_" + i + "_" + j + "_bountyextracted"]),
                        BountyPickedUp = Convert.ToInt32(attributes["MissionBagPlayer_" + i + "_" + j + "_bountypickedup"]),
                        Proximity = bool.Parse(attributes["MissionBagPlayer_" + i + "_" + j + "_proximitytome"]),
                        ProximityTeammate = bool.Parse(attributes["MissionBagPlayer_" + i + "_" + j + "_proximitytoteammate"]),
                    });
                }
                match.Teams.Add(team);
            }
    
            var matchId = Convert.ToInt32(await con.InsertAsync(new HuntMatchTable()
            {
                Butcher = bool.Parse(attributes["MissionBagBoss_0"]),
                Spider = bool.Parse(attributes["MissionBagBoss_1"]),
                Assassin = bool.Parse(attributes["MissionBagBoss_2"]),
                Scrapbeak = bool.Parse(attributes["MissionBagBoss_3"]),
                DateTime = DateTime.UtcNow
            }));

            foreach (var entry in entries)
            {
                entry.MatchId = matchId;
                await con.InsertAsync(entry);
            }

            foreach (var accolade in accolades)
            {
                accolade.MatchId = matchId;
                await con.InsertAsync(accolade);
            }
    
            foreach (var team in match.Teams)
            {
                await con.InsertAsync(new TeamTable()
                {
                    Mmr = team.Mmr,
                    Players = JsonConvert.SerializeObject(team.Players),
                    MatchId = matchId
                });
            }
        }
        
        
        _appState.MatchAdded();
        return GeneralStatus.Succes;
    }
}