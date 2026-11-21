using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging.Abstractions;
using PM.Core.Implementation;
using PM.Core.StoreModels;

namespace PM.Core.Tests;

public class CatalogProviderTests
{
	[Fact]
	public async Task GetCatalog_MapsValidProductsAndDeduplicatesNormalizedNames()
	{
		var lastUpdated = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);
		var handler = new RecordingHttpMessageHandler(_ => JsonResponse(HttpStatusCode.OK, new
		{
			records = new[]
			{
				new { id = 10, name = " Green Apple ", category = " Fruit ", price = 1.25f, quantity = 3, lastUpdated },
				new { id = 11, name = "green apple", category = "Produce", price = 1.50f, quantity = 4, lastUpdated },
				new { id = 12, name = "Pear", category = "Fruit", price = 2.50f, quantity = 2, lastUpdated },
				new { id = 0, name = "Invalid", category = "Fruit", price = 1.00f, quantity = 1, lastUpdated },
				new { id = 13, name = "No stock", category = "Fruit", price = 1.00f, quantity = 0, lastUpdated }
			},
			page = 1,
			pageSize = 25,
			totalPages = 1,
			totalRecords = 5
		}));
		var provider = CreateProvider(handler);

		var catalog = await provider.GetCatalog();

		var product = Assert.Single(catalog.Products, p => p.ID == 10);
		Assert.Equal(2, catalog.Products.Count);
		Assert.Equal("Green Apple", product.Name);
		Assert.Equal(1, product.CategoryID);
		Assert.Equal(1.25m, product.Price);
		Assert.Equal(3, product.Quantity);
		Assert.Equal(lastUpdated, product.LastUpdated);
		Assert.Equal("Fruit", Assert.Single(catalog.Categories).Name);
		Assert.Equal("https://catalog.test/api/records?page=1&pagesize=25", handler.Requests[0].RequestUri?.ToString());
		Assert.Contains(handler.Requests[0].Headers, header =>
			header.Key == "X-API-Key" && header.Value.SequenceEqual(new[] { "test-key" }));
	}

	[Fact]
	public async Task GetCatalog_StopsOnRateLimitAndKeepsProductsAlreadyFetched()
	{
		var handler = new RecordingHttpMessageHandler(request =>
		{
			if (request.RequestUri?.Query.Contains("page=2", StringComparison.Ordinal) == true)
				return new HttpResponseMessage(HttpStatusCode.TooManyRequests);

			return JsonResponse(HttpStatusCode.OK, new
			{
				records = new[]
				{
					new { id = 1, name = "Tea", category = "Drinks", price = 3.25f, quantity = 2, lastUpdated = DateTime.UnixEpoch }
				},
				page = 1,
				pageSize = 25,
				totalPages = 3,
				totalRecords = 3
			});
		});
		var provider = CreateProvider(handler);

		var catalog = await provider.GetCatalog();

		Assert.Equal(2, handler.Requests.Count);
		Assert.Equal("Tea", Assert.Single(catalog.Products).Name);
		Assert.Equal("Drinks", Assert.Single(catalog.Categories).Name);
	}

	private static CatalogProvider CreateProvider(HttpMessageHandler handler)
	{
		return new CatalogProvider(new StubHttpClientFactory(handler), NullLogger.Instance, "https://catalog.test", "test-key");
	}

	private static HttpResponseMessage JsonResponse<T>(HttpStatusCode statusCode, T body)
	{
		var json = JsonSerializer.Serialize(body);
		return new HttpResponseMessage(statusCode)
		{
			Content = new StringContent(json, Encoding.UTF8, "application/json")
		};
	}

	private sealed class StubHttpClientFactory : IHttpClientFactory
	{
		private readonly HttpMessageHandler handler;

		public StubHttpClientFactory(HttpMessageHandler handler)
		{
			this.handler = handler;
		}

		public HttpClient CreateClient(string name)
		{
			return new HttpClient(handler, disposeHandler: false);
		}
	}

	private sealed class RecordingHttpMessageHandler : HttpMessageHandler
	{
		private readonly Func<HttpRequestMessage, HttpResponseMessage> createResponse;

		public RecordingHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> createResponse)
		{
			this.createResponse = createResponse;
		}

		public List<HttpRequestMessage> Requests { get; } = new();

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			Requests.Add(request);
			return Task.FromResult(createResponse(request));
		}
	}
}
