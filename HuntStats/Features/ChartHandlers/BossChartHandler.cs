using HuntStats.Data;
using MediatR;

namespace HuntStats.Features.ChartHandlers;

public class BossChartInfo
{
    public DateTime DateTime { get; set; }
    
    public int Assassin { get; set; }
    public int Butcher { get; set; }
    public int Spider { get; set; }
    public int Scrapbeak { get; set; }
}

public class BossChartQuery : IRequest<BossChartInfo>
{
    public BossChartQuery(int amount)
    {
        Amount = amount;
    }
    
    public int Amount { get; set; }
}


public class BossChartQueryHandler : IRequestHandler<BossChartQuery, BossChartInfo>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IMediator _mediator;

    public BossChartQueryHandler(IDbConnectionFactory connectionFactory, IMediator mediator)
    {
        _connectionFactory = connectionFactory;
        _mediator = mediator;
    }

    public async Task<BossChartInfo> Handle(BossChartQuery request, CancellationToken cancellationToken)
    {
        using var con = await _connectionFactory.GetOpenConnectionAsync();
        var huntMatch = await _mediator.Send(new GetMatchCommand()
        {
            OrderType = OrderType.Descending,
            Page = 0,
            PageSize = request.Amount
        });
        var Settings = await _mediator.Send(new GetSettingsCommand());

        return new BossChartInfo()
        {
            Scrapbeak = huntMatch.Matches.Select(x => x.Scrapbeak ? 1 : 0).Sum(),
            Spider = huntMatch.Matches.Select(x => x.Spider ? 1 : 0).Sum(),
            Assassin = huntMatch.Matches.Select(x => x.Assassin ? 1 : 0).Sum(),
            Butcher = huntMatch.Matches.Select(x => x.Butcher ? 1 : 0).Sum(),
        };
    }
}