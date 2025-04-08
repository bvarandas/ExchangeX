using MediatR;

namespace SharedX.Core.Bus;

public interface IMediatorHandler : ISender, IPublisher
{
}
