# PM Inventory API

This service fetches inventory records from the upstream API base URL and API Key entries configured by `InventoryService:ApiUrl` and `InventoryService:ApiKey`, requesting pages of 25 records and sending the configured API key in the `X-API-Key` header. It filters invalid records, removes duplicates, and returns a category-level summary with inventory value, quantity, most recently updated item, and the items in each category.

## Endpoint
When running with the local HTTP profile, call `GET http://localhost:5073/api/store`. No request body or query parameters are required.

```bash
curl http://localhost:5073/api/store
```

## Areas of Improvement
Initialization could be improved by making it asynchronous and handling errors more gracefully. For the sake of the test, it does the job.

A database could be used to store the inventory records and avoid fetching the same records repeatedly, but for the sake of the test, this was not covered in this implementation.

During the API calls to retrieve the items and categories from the supplier, the client code removes beginning and trailing spaces from category and item names. Also, the client code removes records with quantities of 0 or less, and any records with a null or empty category or item name, or with no pricing. This logic might not still be 100% perfect; it was based on the data received.
