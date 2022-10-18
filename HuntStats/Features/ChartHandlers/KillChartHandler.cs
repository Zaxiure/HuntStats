using HuntStats.Data;
using MediatR;

namespace HuntStats.Features.ChartHandlers;

public class KillChartInfo
{
    public DateTime DateTime { get; set; }
    public int Kills { get; set; }
    public int YourKills { get; set; }
    public int Assists { get; set; }
    public int Deaths { get; set; }
}

public class KillChartQuery : IRequest<List<KillChartInfo>>
{
    public KillChartQuery(int amount)
    {
        Amount = amount;
    }

    public int Amount { get; set; }
}

public class KillChartQueryHandler : IRequestHandler<KillChartQuery, List<KillChartInfo>>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IMediator _mediator;

    public KillChartQueryHandler(IDbConnectionFactory connectionFactory, IMediator mediator)
    {
        _connectionFactory = connectionFactory;
        _mediator = mediator;
    }

    public async Task<List<KillChartInfo>> Handle(KillChartQuery request, CancellationToken cancellationToken)
    {
        using var con = await _connectionFactory.GetOpenConnectionAsync();
        var Matches = await _mediator.Send(new GetAllMatchCommand());
        var Settings = await _mediator.Send(new GetSettingsCommand());
        Matches = Matches.OrderByDescending(x => x.DateTime).Take(request.Amount).ToList();

        return Matches.Select(async x =>
        {
            var accolades = await _mediator.Send(new GetAccoladesByMatchIdCommand(x.Id));
            var team = x.Teams.FirstOrDefault(x => x.Players.FirstOrDefault(y => y.ProfileId == Settings.PlayerProfileId) != null);
            if (team != null)
            {
                var Assists = 0;
                if (accolades.FirstOrDefault(x => x.Category == "accolade_players_killed_assist") != null) Assists = accolades.FirstOrDefault(x => x.Category == "accolade_players_killed_assist").Hits;
                var Kills = x.Teams.Select(x => x.Players.Select(y => y.KilledByMe + y.DownedByMe + y.KilledByTeammate + y.DownedByTeammate).Sum()).Sum();
                var YourKills = x.Teams.Select(x => x.Players.Select(y => y.KilledByMe + y.DownedByMe).Sum()).Sum();
                var Deaths = x.Teams.Select(x => x.Players.Select(y => y.KilledMe + y.DownedMe).Sum()).Sum();
                return new KillChartInfo
                {
                    DateTime = x.DateTime,
                    YourKills = YourKills,
                    Deaths = Deaths,
                    Kills = Kills,
                    Assists = Assists
                };
            }

            return null;
        }).Select(x => x.Result).Where(x => x != null).OrderBy(x => x.DateTime).ToList();
    }
}