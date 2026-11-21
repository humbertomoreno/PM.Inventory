using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PM.Core.StoreModels;

namespace PM.Core.Implementation
{
	public class CatalogProvider
	{
		#region Class Variables
		protected IHttpClientFactory httpClientFactory;
		protected ILogger logger; // TODO: Consider using a more specific logger type, such as ILogger<CatalogProvider>
		private string apiUrl;  // TODO: Make this configurable via appsettings.json or environment variable
		private string apiKey;  // TODO: Make this configurable via appsettings.json or environment variable
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="CatalogProvider" /> class.
		/// </summary>
		/// <param name="httpClientFactory">The HTTP client factory.</param>
		/// <param name="logger">The logger.</param>
		/// <param name="apiUrl">The API URL.</param>
		/// <param name="apiKey">The API key.</param>
		public CatalogProvider(IHttpClientFactory httpClientFactory, 
			ILogger logger, string apiUrl, string apiKey)
		{
			this.httpClientFactory = httpClientFactory;
			this.logger = logger;
			this.apiUrl = apiUrl;
			this.apiKey = apiKey;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Gets the catalog.
		/// </summary>
		public async Task<StoreCatalog> GetCatalog()
		{
			var result = new StoreCatalog();

			using (var client = httpClientFactory.CreateClient())
			{
				client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
				var totalPages = 9999;
				for (var page = 1; page <= totalPages; page++)
				{
					var response = default(HttpResponseMessage);
					try
					{
						var url = $"{apiUrl}/api/records?page={page}&pagesize=25";
						response = await client.GetAsync(url);
					}
					catch (Exception ex)
					{
						logger?.LogWarning(ex, "An error occurred while fetching catalog data.");
						break;
					}
					if (response != null)
					{
						if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
							break;
						else if (response.IsSuccessStatusCode)
						{
							var responseData = await response.Content.ReadAsAsync<RemoteAPIResponse>();
							if (responseData?.totalPages > 0)
								totalPages = responseData.totalPages;

							var responseRecords = responseData?.records;
							if (responseRecords?.Any() == true)
							{
								foreach (var record in responseRecords)
									AddProductIfNotExists(record, result);
							}
							else
								break; // No more records to process
						}
					}
				}
			}
			return result;
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Adds the product if not exists.
		/// </summary>
		/// <param name="record">The record.</param>
		/// <param name="result">The result.</param>
		private void AddProductIfNotExists(RemoteAPIRecord record, StoreCatalog result)
		{
			if (record != null)
			{
				var isValidRecord = (record.id > 0) &&
					(!string.IsNullOrWhiteSpace(record.name)) &&
					(!string.IsNullOrWhiteSpace(record.category)) &&
					(record.quantity > 0) && (record.price > 0);

				if (isValidRecord)
				{
					var existingProduct = result.Products.FirstOrDefault(p => p.ID == record.id);
					if (existingProduct == null)
						existingProduct = result.Products.FirstOrDefault(p => AreStringsLoselyEqual(p.Name, record.name));
					if (existingProduct == null)
					{
						var newProduct = new StoreProduct()
						{
							ID = record.id,
							Name = ReduceStringForComparison(record.name),
							CategoryID = AddCategoryIfNotExists(record.category, result),
							Price = (decimal)record.price,
							Quantity = record.quantity,
							LastUpdated = record.lastUpdated
						};
						result.Products.Add(newProduct);
					}
				}
			}
		}

		/// <summary>
		/// Adds the category if not exists.
		/// </summary>
		/// <param name="category">The category.</param>
		/// <param name="catalog">The catalog.</param>
		private long AddCategoryIfNotExists(string category, StoreCatalog catalog)
		{
			var result = default(long);
			var categoryToUse = ReduceStringForComparison(category);
			var existingCategory = catalog.Categories.FirstOrDefault(c => AreStringsLoselyEqual(c.Name, categoryToUse));
			if (existingCategory == null)
			{
				var newCategory = new StoreCategory()
				{
					ID = catalog.Categories.Count + 1,
					Name = categoryToUse
				};
				catalog.Categories.Add(newCategory);
				result = newCategory.ID;
			}
			else
				result = existingCategory.ID;

			return result;
		}

		/// <summary>
		/// Reduces the string for comparison.
		/// </summary>
		/// <param name="stringValue">The string value.</param>
		protected static string ReduceStringForComparison(string stringValue)
		{
			var result = (stringValue ?? string.Empty).Trim();
			return result;
		}

		/// <summary>
		/// Determines whether the two strings are the same. Extra spaces, upper/lower case are ignored.
		/// </summary>
		/// <param name="str1">The first string.</param>
		/// <param name="str2">The second string.</param>
		protected static bool AreStringsLoselyEqual(string str1, string str2)
		{
			var str1Value = ReduceStringForComparison(str1);
			var str2Value = ReduceStringForComparison(str2);

			var result = string.Compare(str1Value, str2Value, true) == 0;
			return result;
		}
		#endregion
	}
}