namespace Implementation.Interfaces
{
    public interface IMessageDealerActor: IMessageClient
    {
        IBroadcastMessage OnMessage { get; }
        Task SendMessageAsync(Message message, CancellationToken cancellationToken);
    }
}
