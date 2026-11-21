using System;

namespace PM.Core.Implementation
{
	internal class RemoteAPIResponse
	{
		public RemoteAPIRecord[] records { get; set; }
		public int page { get; set; }
		public int pageSize { get; set; }
		public int totalPages { get; set; }
		public int totalRecords { get; set; }
	}

	internal class RemoteAPIRecord
	{
		public int id { get; set; }
		public string name { get; set; }
		public string category { get; set; }
		public float price { get; set; }
		public int quantity { get; set; }
		public DateTime lastUpdated { get; set; }
	}

}
