using MatchingX.Core.Repositories;
using MediatR;
using SharedX.Core.Matching;

namespace MacthingX.Application.Querys;
public class GetTradeQueryHandler : IRequestHandler<GetTradeIdQuery, Trade>
{
    private readonly ITradeRepository _repository;
    public GetTradeQueryHandler(ITradeRepository repository)
    {
        _repository = repository;
    }
    public async Task<Trade> Handle(GetTradeIdQuery request, CancellationToken cancellationToken)
    {
        var trade = await _repository.GetTradeIdAsync(cancellationToken);

        return trade;
    }
}