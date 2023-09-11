using Autofac;
using Implementation;
using Implementation.Interfaces;
using System.Threading.Tasks.Dataflow;

namespace Server
{
    class Program
    {
        const string _hostName = "localhost";
        const int _port = 11080;
        static IContainer? _container;
        static readonly CancellationTokenSource _tokenSource = new();

        async static Task Main(string[] args)
        {
            _container = ConfigureContainer();
            var cancellationToken = _tokenSource.Token;
            var routerServer = _container.Resolve<IRouterActor>();
            routerServer
                .AddActorName("Router Server")
                .AddHostName(_hostName)
                .AddPort(_port);
             
            routerServer.OnMessage.AddListener("Message Listener 1", new ActionBlock<Message>(message =>
            {
                Console.WriteLine($"Message received from dealer {message}");
            }));
            await routerServer.StartAsync(cancellationToken);
            Console.WriteLine("Router Server Started! Hit enter to stop the server");
            Console.ReadLine();
            Console.WriteLine("Stopping Route Server...");
            await routerServer.StopAsync(cancellationToken);
            await Task.Delay(2000);
            Console.WriteLine("Router Server Stopped!");
        }

        private static IContainer ConfigureContainer()
        {
            ContainerBuilder builder = new();
            builder.RegisterModule(new DefaultModule());
            return builder.Build();
        }
    }
}


