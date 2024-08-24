namespace SharedX.Core.Interfaces;
public interface IReceiverEngine<T> where T : class
{
    public void ReceiveEngine(T message, CancellationToken cancellationToken);
}