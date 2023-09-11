using Client.Interfaces;
using Client.Options;
using Implementation.Interfaces;
using Microsoft.Extensions.Options;

namespace Client.Services
{
    public sealed class StockPriceService: IStockPriceService
    {
        private readonly IDealerActor _dealerActor;
        private readonly DealerActorOptions _options;

        public StockPriceService(IDealerActor dealerActor, IOptions<DealerActorOptions> options)
        {
            _dealerActor = dealerActor;
            _options = options.Value;
            _dealerActor
                .AddActorName(_options.ActorName)
                .AddHostName(_options.HostName)
                .AddPort(_options.Port);
        }

        public async Task Connect(CancellationToken cancellationToken)
        {
            await _dealerActor.ConnectAsync(cancellationToken);
        }

        public async Task Disconnect(CancellationToken cancellationToken)
        {
            await _dealerActor.DisconnectAsync(cancellationToken);
        }
    }
}
