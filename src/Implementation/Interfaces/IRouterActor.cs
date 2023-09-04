namespace Implementation.Interfaces
{
    public interface IRouterActor: IMessageServer, IActor
    {
        IBroadcastMessage OnMessage { get; }
        Task PublishMessageAsync(Message message, CancellationToken cancellationToken);
    }
}
