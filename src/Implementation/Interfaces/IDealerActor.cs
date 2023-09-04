namespace Implementation.Interfaces
{
    public interface IDealerActor: IMessageClient, IActor
    {
        IBroadcastMessage OnMessage { get; }
        Task SendMessageAsync(Message message, CancellationToken cancellationToken);
    }
}
