using PM.Core.StoreModels;

namespace PM.Core.Tests;

public class StoreCatalogTests
{
	[Fact]
	public void Constructor_InitializesEmptyCollections()
	{
		var catalog = new StoreCatalog();

		Assert.NotNull(catalog.Categories);
		Assert.Empty(catalog.Categories);
		Assert.NotNull(catalog.Products);
		Assert.Empty(catalog.Products);
	}

	[Fact]
	public void StoreModels_ToStringReturnsName()
	{
		var category = new StoreCategory { Name = "Beverages" };
		var product = new StoreProduct { Name = "Tea" };

		Assert.Equal("Beverages", category.ToString());
		Assert.Equal("Tea", product.ToString());
	}
}
