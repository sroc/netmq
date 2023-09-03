namespace Implementation.Interfaces
{
    public interface IMessageRouterActor: IMessageServer
    {
        IBroadcastMessage OnMessage { get; }
        Task PublishMessageAsync(Message message, CancellationToken cancellationToken);
    }
}
