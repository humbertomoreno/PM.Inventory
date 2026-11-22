using PM.Core.Implementation;
using PM.InventoryService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddLogging();

// TODO: This block might get optimized.
var buildServiceProvider = builder.Services.BuildServiceProvider();
var httpClientFactory = buildServiceProvider.GetRequiredService<IHttpClientFactory>();
ILogger loggerProvider = null; // buildServiceProvider.GetRequiredService<ILogger>();

var apiKey = builder.Configuration["InventoryService:ApiKey"];
var apiUrl = builder.Configuration["InventoryService:ApiUrl"];
if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiUrl))
{
	apiUrl = Environment.GetEnvironmentVariable("InventoryService.ApiUrl");
	apiKey = Environment.GetEnvironmentVariable("InventoryService.ApiKey");
}

var app = builder.Build();
app.MapGet("/api/store", () =>
{
	var catalogProvider = new CatalogProvider(httpClientFactory,
					loggerProvider,
					apiUrl,
					apiKey
				);
	var catalog = catalogProvider.GetCatalog().Result;
	var service = new InventorySummaryService(catalog);
	return service.GetSummary();
});
app.Run();