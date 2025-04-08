namespace SharedX.Core.Interfaces;
public interface IReceiverEngine<T> where T : class
{
    Task ReceiveEngine(T message, CancellationToken cancellationToken);
}