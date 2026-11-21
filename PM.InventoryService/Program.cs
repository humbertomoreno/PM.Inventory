using PM.Core.Implementation;
using PM.InventoryService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddLogging();

// TODO: This block might get optimized.
var buildServiceProvider = builder.Services.BuildServiceProvider();
var httpClientFactory = buildServiceProvider.GetRequiredService<IHttpClientFactory>();
ILogger loggerProvider = null; // buildServiceProvider.GetRequiredService<ILogger>();

var app = builder.Build();
app.MapGet("/api/store", () =>
{
	var catalogProvider = new CatalogProvider(httpClientFactory,
					loggerProvider,
					builder.Configuration["InventoryService:ApiUrl"],
					builder.Configuration["InventoryService:ApiKey"]
				);
	var catalog = catalogProvider.GetCatalog().Result;
	var service = new InventorySummaryService(catalog);
	return service.GetSummary();
});
app.Run();