using Implementation.Interfaces;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;

namespace Implementation
{
    public sealed class BroadcastMessage : IBroadcastMessage
    {
        private readonly IDictionary<string, ActionBlock<Message>> _listeners = new ConcurrentDictionary<string, ActionBlock<Message>>(StringComparer.OrdinalIgnoreCase);
        private readonly BufferBlock<Message> _buffer;
        private readonly ActionBlock<Message> _broadcaster;

        public BroadcastMessage()
        {
            var linkOptions = new DataflowLinkOptions() { PropagateCompletion = true };
            _buffer = new BufferBlock<Message>();
            _broadcaster = new ActionBlock<Message>(async message =>
            {
                await Task.WhenAll(_listeners.Values.Select(listener => listener.SendAsync(message)));
            });
            _buffer.LinkTo(_broadcaster, linkOptions);
        }

        public bool AddListener(string blockID, ActionBlock<Message> blockAction)
        {
            if(string.IsNullOrEmpty(blockID) || blockAction is null)
                return false;

            if(_listeners.ContainsKey(blockID))
                return false;

            _listeners[blockID] = blockAction;
            return true;
        }

        public bool RemoveListener(string blockID)
        {
            if(string.IsNullOrEmpty(blockID)) 
                return false;

            return _listeners.Remove(blockID);
        }

        public async Task BroadcastMessageAsync(Message message, CancellationToken cancellationToken) =>
            await _buffer.SendAsync(message, cancellationToken);
    }
}
