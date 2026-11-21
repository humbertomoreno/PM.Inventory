namespace PM.InventoryService.Models
{
	public class StoreCatalogResponse
	{
		public List<Category> categories { get; set; }
	}

	public class Category
	{
		public string category { get; set; }
		public decimal totalInventoryValue { get; set; }
		public int availableRecordCount { get; set; }
		public Mostrecentlyupdated mostRecentlyUpdated { get; set; }
		public List<Item> items { get; set; }
	}

	public class Mostrecentlyupdated
	{
		public long id { get; set; }
		public string name { get; set; }
		public DateTime lastUpdated { get; set; }
	}

	public class Item
	{
		public long id { get; set; }
		public string name { get; set; }
		public decimal price { get; set; }
		public int quantity { get; set; }
		public DateTime lastUpdated { get; set; }
	}

}
