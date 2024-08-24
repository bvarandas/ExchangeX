using SharedX.Core.Interfaces;
using SharedX.Core.Matching.DropCopy;

namespace OrderEngineX.API.Receiver;

public class ReceiverExecutionReport : IReceiverEngine<ExecutionReport>
{

    public void ReceiveEngine(ExecutionReport message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
