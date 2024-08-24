using MatchingX.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SharedX.Core.Interfaces;
using SharedX.Core.Matching.OrderEngine;
namespace MatchingX.ServerApp.Receiver;
public class ReceiverOrder : IReceiverEngine<OrderEngine>
{
    private readonly IMatchingReceiver _matchReceiver;
    private readonly ILogger<ReceiverOrder> _logger;

    public ReceiverOrder(IMatchingReceiver matchReceiver, ILogger<ReceiverOrder> logger)
    {
        _matchReceiver = matchReceiver;
        _logger = logger;
    }

    public void ReceiveEngine(OrderEngine message, CancellationToken cancellationToken)
    {
        _matchReceiver.ReceiveOrder(message);
    }
}