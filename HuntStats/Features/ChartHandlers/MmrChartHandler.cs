using HuntStats.Data;
using MediatR;

namespace HuntStats.Features.ChartHandlers;

public class ChartInfo
{
    public DateTime DateTime { get; set; }
    public int TotalMmr { get; set; }
    public int Mmr { get; set; }
}
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
        var huntMatch = await _mediator.Send(new GetMatchCommand()
        {
            OrderType = OrderType.Descending,
            Page = 0,
            PageSize = request.Amount
        });
        var Settings = await _mediator.Send(new GetSettingsCommand());
        
        return huntMatch.Matches.Select(x =>
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
