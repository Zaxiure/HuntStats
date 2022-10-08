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
        var Matches = await _mediator.Send(new GetAllMatchCommand());
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
        }).Where(x => x != null).Select(x => x.Result).OrderBy(x => x.DateTime).ToList();
    }
}

public class MoneyChartQuery : IRequest<List<MoneyChartInfo>>
{
    public MoneyChartQuery(int amount)
    {
        Amount = amount;
    }
    
    public int Amount { get; set; }
}

public class MoneyChartQueryHandler : IRequestHandler<MoneyChartQuery, List<MoneyChartInfo>>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IMediator _mediator;

    public MoneyChartQueryHandler(IDbConnectionFactory connectionFactory, IMediator mediator)
    {
        _connectionFactory = connectionFactory;
        _mediator = mediator;
    }

    public async Task<List<MoneyChartInfo>> Handle(MoneyChartQuery request, CancellationToken cancellationToken)
    {
        using var con = await _connectionFactory.GetOpenConnectionAsync();
        var Matches = await _mediator.Send(new GetAllMatchCommand());
        var Settings = await _mediator.Send(new GetSettingsCommand());
        Matches = Matches.OrderByDescending(x => x.DateTime).Take(request.Amount).ToList();
        
        return Matches.Select(async x =>
        {
            var accolades = await _mediator.Send(new GetAccoladesByMatchIdCommand(x.Id));
            var entries = await _mediator.Send(new GetEntriesByMatchIdCommand(x.Id));
            var team = x.Teams.FirstOrDefault(x => x.Players.FirstOrDefault(y => y.ProfileId == Settings.PlayerProfileId) != null);
            if (team != null)
            {
                var HuntDollars = 0;
                HuntDollars += accolades.Select(x => x.Bounty).Sum();
                var entry = entries.FirstOrDefault(x => x.Category == "accolade_found_gold");
                if(entry != null)
                {
                    HuntDollars += entry.RewardSize;
                }
                
                return new MoneyChartInfo()
                {
                    DateTime = x.DateTime,
                    HuntDollars = HuntDollars
                };
            }
            return null;
        }).Select(x => x.Result).Where(x => x != null).OrderBy(x => x.DateTime).ToList();
    }
}

public class XpChartQuery : IRequest<List<XpChartInfo>>
{
    public XpChartQuery(int amount)
    {
        Amount = amount;
    }
    
    public int Amount { get; set; }
}

public class XpChartQueryHandler : IRequestHandler<XpChartQuery, List<XpChartInfo>>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IMediator _mediator;

    public XpChartQueryHandler(IDbConnectionFactory connectionFactory, IMediator mediator)
    {
        _connectionFactory = connectionFactory;
        _mediator = mediator;
    }

    public async Task<List<XpChartInfo>> Handle(XpChartQuery request, CancellationToken cancellationToken)
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
                var Xp = 0;
                Xp += accolades.Select(x => x.Bounty).Sum() * 4;
                Xp += accolades.Select(x => x.Xp).Sum();

                if (accolades.FirstOrDefault(x => x.Category == "accolade_extraction") == null) Xp /= 2;

                return new XpChartInfo()
                {
                    DateTime = x.DateTime,
                    Xp = Xp
                };
            }
            return null;
        }).Select(x => x.Result).Where(x => x != null).OrderBy(x => x.DateTime).ToList();
    }
}

public class XpChartInfo
{
    public DateTime DateTime { get; set; }
    public int Xp { get; set; }
}


public class MoneyChartInfo
{
    public DateTime DateTime { get; set; }
    public int HuntDollars { get; set; }
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
    public int Assists { get; set; }
    public int Deaths { get; set; }
}