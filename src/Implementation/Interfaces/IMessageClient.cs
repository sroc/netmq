namespace Implementation.Interfaces
{
    public interface IMessageClient
    {
        Task ConnectAsync(CancellationToken cancellationToken);
        Task DisconnectAsync(CancellationToken cancellationToken);
    }
}
