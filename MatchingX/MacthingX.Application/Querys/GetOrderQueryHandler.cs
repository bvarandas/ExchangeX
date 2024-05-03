using MatchingX.Core.Repositories;
using MediatR;
using SharedX.Core.Matching;
using MatchingX.Core.Filters;
using SharedX.Core.Interfaces;

namespace MacthingX.Application.Querys;
public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, IEnumerable<Order>>
{
    private readonly IOrderRepository _repository;
    public GetOrderQueryHandler(IOrderRepository repository)
    {
        _repository = repository;
    }
    public async Task<IEnumerable<Order>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetOrdersOnRestartAsync(cancellationToken);

        return result;
    }
}