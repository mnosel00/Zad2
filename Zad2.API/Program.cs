using Zad2.Core.Interfaces;
using Zad2.Core.Services;
using Zad2.Infrastructure.Clients;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Rick and Morty Aggregator API",
        Version = "v1",
        Description = "An API that aggregates data from the Rick and Morty API"
    });
});

// Register HttpClient for Rick and Morty API
builder.Services.AddHttpClient<IRickAndMortyClient, RickAndMortyHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://rickandmortyapi.com/api/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register Core services
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<ITopPairsService, TopPairsService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Rick and Morty Aggregator API v1");
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
