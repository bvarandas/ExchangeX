using MatchingX.Core.Repositories;
using MediatR;
using SharedX.Core.Matching;
using MatchingX.Core.Filters;
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
        var result = await _repository.GetOrdersOnRestartAsync(new OrderParams(), cancellationToken);

        return result;
    }
}