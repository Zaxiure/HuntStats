using Dommel;
using HuntStats.Data;
using HuntStats.Models;
using MediatR;

namespace HuntStats.Features;

public class GetEntriesByMatchIdCommand : IRequest<List<HuntEntry>>
{
    public GetEntriesByMatchIdCommand(int matchId)
    {
        MatchId = matchId;
    }

    public int MatchId { get; set; }
}

public class GetEntriesByMatchIdCommandHandler : IRequestHandler<GetEntriesByMatchIdCommand, List<HuntEntry>>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetEntriesByMatchIdCommandHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<HuntEntry>> Handle(GetEntriesByMatchIdCommand request, CancellationToken cancellationToken)
    {
        var con = await _connectionFactory.GetOpenConnectionAsync();
        var entries = await con.SelectAsync<HuntEntry>(x => x.MatchId == request.MatchId);
        return entries.ToList();
    }
}