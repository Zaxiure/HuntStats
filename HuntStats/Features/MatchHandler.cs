using System.Text.RegularExpressions;
using ConsoleApp1.Models;
using Dommel;
using HuntStats.Data;
using MediatR;
using Newtonsoft.Json;

namespace HuntStats.Features;

public class GetMatchbyIdCommand : IRequest<HuntMatch>
{
    public GetMatchbyIdCommand(int id)
    {
        Id = id;
    }

    public int Id { get; set; }
}

public class GetMatchbyIdCommandHandler : IRequestHandler<GetMatchbyIdCommand, HuntMatch>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetMatchbyIdCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }


    public async Task<HuntMatch> Handle(GetMatchbyIdCommand request, CancellationToken cancellationToken)
    {
        using var con = await _connectionFactory.GetOpenConnectionAsync();

        var match = await con.SelectAsync<HuntMatchTable>(x => x.Id == request.Id);

        var mappedHuntMatch = match.Select(async x =>
        {
            var teams = await con.SelectAsync<TeamTable>(j => j.MatchId == x.Id);
            var huntMatch = new HuntMatch()
            {
                Id = x.Id,
                DateTime = x.DateTime,
                Teams = teams.Select(team => new Team()
                {
                    Id = team.Id,
                    Mmr = team.Mmr,
                    Players = JsonConvert.DeserializeObject<List<Player>>(team.Players)
                }).ToList()
            };
            return huntMatch;
        }).Select(x => x.Result);

        return mappedHuntMatch.FirstOrDefault();
    }
}

public class GetMatchCommand : IRequest<List<HuntMatch>>
{
    
}

public class GetMatchCommandHandler : IRequestHandler<GetMatchCommand, List<HuntMatch>>
{
    private readonly IDbConnectionFactory _connectionFactory;
    
    public GetMatchCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    
    public async Task<List<HuntMatch>> Handle(GetMatchCommand request, CancellationToken cancellationToken)
    {
        using var con = await _connectionFactory.GetOpenConnectionAsync();

        var matches = await con.GetAllAsync<HuntMatchTable>();

        var mappedHuntMatch = matches.Select(async x =>
        {
            var teams = await con.SelectAsync<TeamTable>(j => j.MatchId == x.Id);
            var huntMatch = new HuntMatch()
            {
                Id = x.Id,
                DateTime = x.DateTime,
                Teams = teams.Select(team => new Team()
                {
                    Id = team.Id,
                    Mmr = team.Mmr,
                    Players = JsonConvert.DeserializeObject<List<Player>>(team.Players)
                }).ToList()
            };
            huntMatch.TotalKills = huntMatch.Teams.Select(x => x.Players.Select(x => x.KilledByMe).Sum()).Sum();
            huntMatch.TotalKillsWithTeammate = huntMatch.TotalKills +
                                               huntMatch.Teams.Select(x =>
                                                   x.Players.Select(x => x.KilledByTeammate).Sum()).Sum();
            huntMatch.TotalDeaths = huntMatch.Teams.Select(x => x.Players.Select(x => x.KilledByMe).Sum()).Sum();

            return huntMatch;
        }).Select(x => x.Result);

        return mappedHuntMatch.ToList();
    }
}