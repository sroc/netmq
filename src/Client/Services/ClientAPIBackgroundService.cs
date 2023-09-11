using Client.Interfaces;

namespace Client.Services
{
    public class ClientAPIBackgroundService : BackgroundService
    {
        private readonly IStockPriceService _stockPriceService;
        public ClientAPIBackgroundService(IStockPriceService stockPriceService) { 
            _stockPriceService = stockPriceService;
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _stockPriceService.Connect(stoppingToken);
        }

        public async override Task StopAsync(CancellationToken cancellationToken)
        {
            await _stockPriceService.Disconnect(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}
