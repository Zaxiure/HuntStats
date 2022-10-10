using HuntStats.Data;
using MediatR;

namespace HuntStats.Features.ChartHandlers;

public class MoneyChartInfo
{
    public DateTime DateTime { get; set; }
    public int HuntDollars { get; set; }
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
        var matches = await _mediator.Send(new GetAllMatchCommand());
        var settings = await _mediator.Send(new GetSettingsCommand());
        matches = matches.OrderByDescending(x => x.DateTime).Take(request.Amount).ToList();
        
        return matches.Select(async x =>
        {
            var accolades = await _mediator.Send(new GetAccoladesByMatchIdCommand(x.Id));
            var entries = await _mediator.Send(new GetEntriesByMatchIdCommand(x.Id));
            var team = x.Teams.FirstOrDefault(x => x.Players.FirstOrDefault(y => y.ProfileId == settings.PlayerProfileId) != null);
            if (team != null)
            {
                var huntDollars = 0;
                huntDollars += accolades.Select(x => x.Bounty).Sum();
                var entry = entries.FirstOrDefault(x => x.Category == "accolade_found_gold");
                if(entry != null)
                {
                    huntDollars += entry.RewardSize;
                }
                
                return new MoneyChartInfo()
                {
                    DateTime = x.DateTime,
                    HuntDollars = huntDollars
                };
            }
            return null;
        }).Select(x => x.Result).Where(x => x != null).OrderBy(x => x.DateTime).ToList();
    }
}