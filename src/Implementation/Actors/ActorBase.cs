using Implementation.Interfaces;
using NetMQ;
using System.Threading.Tasks.Dataflow;

namespace Implementation.Actors
{
    public abstract class ActorBase: IActor
    {
        protected NetMQActor? _actor;
        protected readonly BufferBlock<NetMQMessage> _bufferBlock;
        protected ActorBase()
        {
            _bufferBlock = new();
            DataflowLinkOptions linkOptions = new() { PropagateCompletion = true};
            ActionBlock<NetMQMessage> action = new(message =>
            {
                _actor?.SendMultipartMessage(message);
            });
            _bufferBlock.LinkTo(action, linkOptions);

        }

        public string HostName { get; private set; } = string.Empty;
        public int Port { get; private set; }
        public string ActorName { get; private set; } = string.Empty;
        public string Address => $"tcp://{HostName}:{Port}";
        public string Topic { get; private set; } = string.Empty;

        public IActor AddActorName(string name)
        {
            ActorName = name;
            return this;
        }

        public IActor AddHostName(string hostName)
        {
            HostName = hostName;
            return this;
        }

        public IActor AddPort(int port)
        {
            Port = port;
            return this;
        }

        public IActor AddTopic(string topic)
        {
            Topic = topic;
            return this;
        }
    }
}
