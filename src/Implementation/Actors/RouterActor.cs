using Implementation.Interfaces;
using NetMQ;
using NetMQ.Monitoring;
using NetMQ.Sockets;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace Implementation.Actors
{
    public sealed class RouterActor : ActorBase, IMessageRouterActor
    {
        public RouterActor(IBroadcastMessage broadcastMessage)
        {
            OnMessage = broadcastMessage;
        }

        public IBroadcastMessage OnMessage { get; init; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if(_actor is not null)
                return Task.CompletedTask;

            _actor = NetMQActor.Create(new RouterActorShimHandler(ActorName, Address, OnMessage));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if(_actor is not null)
            {
                _actor.Dispose();
                _actor = null;
            }
            return Task.CompletedTask;
        }

        public Task PublishMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (_actor is null)
                throw new InvalidOperationException("Router not started");

            NetMQMessage messageToClients = new();
            messageToClients.AppendEmptyFrame();
            messageToClients.Append(message.Command);
            messageToClients.Append(message.Text);
            _bufferBlock.Post(messageToClients);
            return Task.CompletedTask;
        }

    }

    internal sealed class RouterActorShimHandler : IShimHandler
    {
        private NetMQPoller? _poller;
        private RouterSocket? _router;
        private readonly string _address;
        private readonly string _actorName;
        private readonly HashSet<NetMQFrame> _clients = new();
        private readonly IBroadcastMessage _broadcastMessage;

        public RouterActorShimHandler(string actorName, string address, IBroadcastMessage broadcastMessage)
        {
            _address = address;
            _broadcastMessage = broadcastMessage;
            _actorName = actorName;
        }

        public void Run(PairSocket shim)
        {
            try
            {
                using (_router = new())
                {
                    using (NetMQMonitor monitor = new(_router, $"inproc://#router#{_actorName}.inproc", SocketEvents.Listening))
                    {
                        monitor.Listening += Monitor_Listening;
                        _router.Options.TcpKeepalive = true;
                        _router.Options.Identity = Encoding.UTF8.GetBytes(_actorName);
                        _router.Options.RouterHandover = true;
                        _router.Options.RouterMandatory = false;
                        _router.Options.RouterRawSocket = false;
                        _router.Options.ReceiveHighWatermark = 0;
                        _router.Options.SendHighWatermark = 0;
                        _router.ReceiveReady += Router_ReceiveReady;
                        shim.ReceiveReady += Shim_ReceiveReady;
                        _poller = new() { shim, _router };
                        monitor.AttachToPoller(_poller);
                        _router.Bind(_address);
                        shim.SignalOK();
                        _poller.Run();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async void Shim_ReceiveReady(object? sender, NetMQSocketEventArgs e)
        {
            try
            {
                var msg = await e.Socket.ReceiveMultipartMessageAsync();
                if (msg.FrameCount == 3)
                {
                    foreach (var item in _clients)
                    {
                        NetMQMessage messageToClient = new();
                        messageToClient.Append(item);
                        messageToClient.AppendEmptyFrame();
                        messageToClient.Append(msg[1]);
                        messageToClient.Append(msg[2]);
                        _router?.SendMultipartMessage(messageToClient);
                    }
                }
                else if (msg.FrameCount == 1 && msg[0].ConvertToString().Equals(NetMQActor.EndShimMessage, StringComparison.OrdinalIgnoreCase))
                {
                    _poller?.Stop();
                    //TODO: notify that the server is no longer listening
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async void Router_ReceiveReady(object? sender, NetMQSocketEventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                NetMQMessage? message = null;
                if (!e.Socket.TryReceiveMultipartMessage(ref message))
                    break;

                if (message.FrameCount == 2)
                    _clients.Add(message[0]);
                else if (message.FrameCount == 4)
                    await _broadcastMessage.BroadcastMessageAsync(new(message[2].ConvertToString(), message[3].ConvertToString()), default);
            }
        }

        private void Monitor_Listening(object? sender, NetMQMonitorSocketEventArgs e)
        {
            //TODO: notify that the router is listening
        }
    }
}
