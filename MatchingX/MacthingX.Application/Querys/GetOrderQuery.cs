using MediatR;
using SharedX.Core.Matching;
using SharedX.Core.Matching.OrderEngine;
using SharedX.Core.Querys;
namespace MacthingX.Application.Querys;
public class GetOrderQuery: Query, IRequest<IEnumerable<OrderEngine>>
{
}
