namespace Implementation.Interfaces
{
    public interface IMessageDealerActor: IMessageClient
    {
        IBroadcastMessage OnMessage { get; }
        Task SendMessageAsync(string command, string text, CancellationToken cancellationToken);
    }
}
