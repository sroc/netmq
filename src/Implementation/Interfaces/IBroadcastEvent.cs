using System.Threading.Tasks.Dataflow;

namespace Implementation.Interfaces
{
    public interface IBroadcastMessage
    {
        bool AddListener(string blockID, ActionBlock<Message> blockAction);
        bool RemoveListener(string blockID);
        Task BroadcastMessageAsync(Message message, CancellationToken cancellationToken);
    }
}
