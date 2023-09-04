using Implementation.Interfaces;
using NetMQ;
using NetMQ.Monitoring;
using NetMQ.Sockets;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace Implementation.Actors
{
    public sealed class DealerActor : ActorBase, IDealerActor
    {
        public IBroadcastMessage OnMessage { get; init; }

        public DealerActor(IBroadcastMessage broadcastMessage)
        {
            OnMessage = broadcastMessage;
        }

        public Task ConnectAsync(CancellationToken cancellationToken)
        {
            if (_actor is null)
                return Task.CompletedTask;

            _actor = NetMQActor.Create(new DealerActorShimHandler(this));
            return Task.CompletedTask;
        }

        public Task DisconnectAsync(CancellationToken cancellationToken)
        {
            if(_actor is not null)
            {
                _actor.Dispose();
                _actor = null;
            }
            return Task.CompletedTask;
        }

        public Task SendMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (_actor is null)
                throw new InvalidOperationException("Dealer not started");

            NetMQMessage messageToServer = new();
            messageToServer.AppendEmptyFrame();
            messageToServer.Append(message.Command);
            messageToServer.Append(message.Text);
            _bufferBlock.Post(messageToServer);
            return Task.CompletedTask;

        }
    }

    internal sealed class DealerActorShimHandler : IShimHandler
    {
        private NetMQPoller? _poller;
        private DealerSocket? _dealer;
        private readonly IBroadcastMessage _broadcastMessage;
        private readonly string _actorName;
        private readonly string _address;
        private readonly string _identity = Guid.NewGuid().ToString();

        public DealerActorShimHandler(DealerActor dealerActor)
        {
            _address = dealerActor.Address;
            _actorName = dealerActor.ActorName;
            _broadcastMessage = dealerActor.OnMessage;
        }

        public void Run(PairSocket shim)
        {
            using (_dealer = new())
            {
                _dealer.Options.TcpKeepalive = true;
                _dealer.Options.ReconnectInterval = TimeSpan.FromSeconds(3D);
                _dealer.Options.TcpKeepaliveInterval = TimeSpan.FromSeconds(30D);
                _dealer.Options.ReceiveHighWatermark = 0;
                _dealer.Options.SendHighWatermark = 0;
                _dealer.Options.Identity = Encoding.Unicode.GetBytes(_identity);
                using (_poller = new() { shim, _dealer })
                {
                    using (NetMQMonitor monitor = new(_dealer, $"inproc://#{_actorName}#.inproc", SocketEvents.Connected | SocketEvents.ConnectRetried | SocketEvents.Disconnected))
                    {
                        monitor.Connected += Monitor_Connected;
                        monitor.ConnectRetried += Monitor_ConnectRetried;
                        monitor.Disconnected += Monitor_Disconnected;

                        monitor.AttachToPoller(_poller);
                        _dealer.Connect(_address);
                        _dealer.ReceiveReady += Dealer_ReceiveReady;
                        shim.ReceiveReady += Shim_ReceiveReady;
                        shim.SignalOK();
                        _poller.Run();
                    }
                }
            }
        }

        private async void Shim_ReceiveReady(object? sender, NetMQSocketEventArgs e)
        {
            var receivedMessage = await e.Socket.ReceiveMultipartMessageAsync();
            if (receivedMessage.FrameCount == 1)
            {
                string command = receivedMessage[0].ConvertToString();
                if(command.Equals(NetMQActor.EndShimMessage, StringComparison.OrdinalIgnoreCase))
                {
                    _poller?.Stop();
                    //TODO: notify disconnection
                }
            }
            else if(receivedMessage.FrameCount == 3)
            {
                _dealer?.SendMultipartMessage(receivedMessage);
            }
        }

        private async void Dealer_ReceiveReady(object? sender, NetMQSocketEventArgs e)
        {
            try
            {
                NetMQMessage receivedMessage = await e.Socket.ReceiveMultipartMessageAsync();
                if (receivedMessage.FrameCount == 3)
                {
                    var command = receivedMessage[1].ConvertToString();
                    var message = receivedMessage[2].ConvertToString();
                    await _broadcastMessage.BroadcastMessageAsync(new(command, message), default);
                }
            }
            catch (Exception)
            {
                //TODO: log message;
            }
        }

        private void Monitor_Disconnected(object? sender, NetMQMonitorSocketEventArgs e)
        {
            //TODO: notify disconnection
        }

        private void Monitor_Connected(object? sender, NetMQMonitorSocketEventArgs e)
        {
            _dealer?.SendFrame(_identity);
        }

        private void Monitor_ConnectRetried(object? sender, NetMQMonitorIntervalEventArgs e)
        {
            //TODO: log and update connection status
        }
    }
}
