using System;
using System.Collections.Generic;

namespace PM.Core.StoreModels
{
	public class StoreCatalog
	{
		public List<StoreCategory> Categories { get; set; }

		public List<StoreProduct> Products { get; set; }

		public StoreCatalog()
		{
			Categories = new List<StoreCategory>();
			Products = new List<StoreProduct>();
		}
	}

	public class StoreCategory
	{
		public long ID { get; set;  }

		public string Name { get; set; }

		override public string ToString() => Name;
	}

	public class StoreProduct
	{
		public long ID { get; set; }

		public string Name { get; set; }

		public long CategoryID { get; set; }

		public decimal Price { get; set; }

		public int Quantity { get; set; }

		public DateTime LastUpdated { get; set; }

		override public string ToString() => Name;
	}
}
