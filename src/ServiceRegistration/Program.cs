using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Internal;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache(options =>
{
    options.Clock = new SystemClock();
    options.ExpirationScanFrequency = TimeSpan.FromSeconds(1);
});

builder.Services.AddSingleton<HostCache>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/silos", ([FromServices] HostCache cache) => cache.GetHosts()).WithName("GetSilos");
app.MapPost("/silo/{host}/{port}",
    ([FromServices] HostCache cache, string host, int port) => { cache.Register(host, port); });

app.Run();