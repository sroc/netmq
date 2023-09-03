namespace Implementation.Interfaces
{
    public interface IMessageServer
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}
