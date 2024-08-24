namespace SharedX.Core.Interfaces;

public interface IPublisherEngine<T> where T : class
{
    public void PublishEngine(T message, CancellationToken cancellationToken = default);
}
