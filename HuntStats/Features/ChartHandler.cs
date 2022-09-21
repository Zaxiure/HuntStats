using HuntStats.Data;
using MediatR;

namespace HuntStats.Features;

public class MmrChartQuery : IRequest<List<ChartInfo>>
{
    public MmrChartQuery(int amount)
    {
        Amount = amount;
    }
    
    public int Amount { get; set; }
}

public class MmrChartQueryHandler : IRequestHandler<MmrChartQuery, List<ChartInfo>>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IMediator _mediator;

    public MmrChartQueryHandler(IDbConnectionFactory connectionFactory, IMediator mediator)
    {
        _connectionFactory = connectionFactory;
        _mediator = mediator;
    }

    public async Task<List<ChartInfo>> Handle(MmrChartQuery request, CancellationToken cancellationToken)
    {
        var Matches = await _mediator.Send(new GetMatchCommand());
        var Settings = await _mediator.Send(new GetSettingsCommand());
        Matches = Matches.OrderByDescending(x => x.DateTime).Take(request.Amount).ToList();
        
        return Matches.Select(x =>
        {
            var team = x.Teams.FirstOrDefault(x => x.Players.FirstOrDefault(y => y.ProfileId == Settings.PlayerProfileId) != null);
            if (team != null)
            {
                var totalMmr = x.Teams.Select(x => x.Players.Select(x => x.Mmr).Sum() / x.Players.Count()).Sum() / x.Teams.Count;
                var mmr = team.Players.FirstOrDefault(y => y.ProfileId == Settings.PlayerProfileId).Mmr;
                return new ChartInfo
                {
                    DateTime = x.DateTime,
                    TotalMmr = totalMmr,
                    Mmr = mmr
                };
            }
            return null;
        }).Where(x => x != null).OrderBy(x => x.DateTime).ToList();
    }
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
        var Matches = await _mediator.Send(new GetMatchCommand());
        var Settings = await _mediator.Send(new GetSettingsCommand());
        Matches = Matches.OrderByDescending(x => x.DateTime).Take(request.Amount).ToList();
        
        return Matches.Select(x =>
        {
            var team = x.Teams.FirstOrDefault(x => x.Players.FirstOrDefault(y => y.ProfileId == Settings.PlayerProfileId) != null);
            if (team != null)
            {
                var Kills = x.Teams.Select(x => x.Players.Select(y => y.KilledByMe + y.DownedByMe + y.KilledByTeammate + y.DownedByTeammate).Sum()).Sum();
                var YourKills = x.Teams.Select(x => x.Players.Select(y => y.KilledByMe + y.DownedByMe).Sum()).Sum();
                var Deaths = x.Teams.Select(x => x.Players.Select(y => y.KilledMe + y.DownedMe).Sum()).Sum();
                return new KillChartInfo
                {
                    DateTime = x.DateTime,
                    YourKills = YourKills,
                    Deaths = Deaths,
                    Kills = Kills,
                };
            }
            return null;
        }).Where(x => x != null).OrderBy(x => x.DateTime).ToList();
    }
}

public class ChartInfo
{
    public DateTime DateTime { get; set; }
    public int TotalMmr { get; set; }
    public int Mmr { get; set; }
}

public class KillChartInfo
{
    public DateTime DateTime { get; set; }
    public int Kills { get; set; }
    public int YourKills { get; set; }
    public int Deaths { get; set; }
}