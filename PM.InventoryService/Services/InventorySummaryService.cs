using PM.Core.StoreModels;
using PM.InventoryService.Models;

namespace PM.InventoryService.Services
{
	public class InventorySummaryService
	{
		#region Class Variables
		protected readonly StoreCatalog catalog;
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="InventorySummaryService"/> class.
		/// </summary>
		/// <param name="catalog">The catalog.</param>
		public InventorySummaryService(StoreCatalog catalog)
		{
			this.catalog = catalog;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Gets the summary.
		/// </summary>
		public StoreCatalogResponse GetSummary()
		{
			var result = new StoreCatalogResponse()
			{
				categories = (from c in catalog.Categories
							  let productsInCategory = catalog.Products.Where(p => p.CategoryID == c.ID)
							  let productMostRecentlyUpdated = productsInCategory.OrderByDescending(p => p.LastUpdated).FirstOrDefault()
							  select new Category()
							  {								  
								  category = c.Name,
								  totalInventoryValue = productsInCategory.Sum(i => i.Price * i.Quantity),
								  availableRecordCount = productsInCategory.Sum(i=> i.Quantity),
								  mostRecentlyUpdated = new Mostrecentlyupdated()
											{
												id = productMostRecentlyUpdated.ID,
												name = productMostRecentlyUpdated.Name,
												lastUpdated = productMostRecentlyUpdated.LastUpdated
											},
								  items = (
									from i in productsInCategory
									  select new Item()
									  {
										  id = i.ID,
										  name = i.Name,
										  price = i.Price,
										  quantity = i.Quantity,
										  lastUpdated = i.LastUpdated
									  }).ToList()
							  }).ToList()
			};
			return result;
		}
		#endregion
	}
}
