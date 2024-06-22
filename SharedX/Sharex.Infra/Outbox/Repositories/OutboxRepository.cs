using Microsoft.Extensions.Logging;
using Sharedx.Infra.Outbox.Cache;
using SharedX.Core.Interfaces;
namespace Sharedx.Infra.Outbox.Repositories;
public class OutboxRepository<T> where T : class , IOutboxRepository<T>
{
    private readonly ILogger<OutboxRepository<T>> _logger = null!;

    public OutboxRepository()
    {

    }


}
