using HuntStats.Data;
using MediatR;

namespace HuntStats.Features.ChartHandlers;

public class XpChartInfo
{
    public DateTime DateTime { get; set; }
    public int Xp { get; set; }
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