using Autofac;
using Autofac.Extensions.DependencyInjection;
using Client.Interfaces;
using Client.Options;
using Client.Services;
using Implementation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

RegisterOptions();
RegisterDI();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

void RegisterOptions()
{
    builder.Services.Configure<DealerActorOptions>(builder.Configuration.GetSection(DealerActorOptions.DealerActor));
    builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory((containerBuilder) =>
        {
            containerBuilder.RegisterModule(new DefaultModule());
        }
    ));
}

void RegisterDI()
{
    builder.Services.AddSingleton<IStockPriceService, StockPriceService>();
    builder.Services.AddHostedService<ClientAPIBackgroundService>();
}
