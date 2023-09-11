namespace Client.Interfaces
{
    public interface IStockPriceService
    {
        Task Connect(CancellationToken cancellationToken);
        Task Disconnect(CancellationToken cancellationToken);
    }
}
