using SharedX.Core.Entities;
using SharedX.Core.Interfaces;

namespace Security.API.Receiver;

public class ReceiverSecurity : IReceiverEngine<SecurityEngine>
{
    public void ReceiveEngine(SecurityEngine message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
