using EcommerceAdminBot.Models;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EcommerceAdminBot.Utilities
{
    public class CosmosDBClient
    {
        // The Cosmos client instance
        private CosmosClient cosmosClient;

        // The database we will create
        private Database database;

        // The container we will create.
        private Container container;

        public async Task GetStartedAsync(string EndpointUri, String PrimaryKey, string databaseId, string containerId, string partitionKey)
        {
            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "CosmosDBDotnetQuickstart" });
            await this.CreateDatabaseAsync(databaseId);
            await this.CreateContainerAsync(containerId, partitionKey);
            //await this.ScaleContainerAsync();
            //await this.AddItemsToContainerAsync();
            //await this.QueryItemsAsync();
            //await this.ReplaceFamilyItemAsync();
            //await this.DeleteFamilyItemAsync();
            //await this.DeleteDatabaseAndCleanupAsync();
        }

        // <CreateDatabaseAsync>
        /// <summary>
        /// Create the database if it does not exist
        /// </summary>
        private async Task CreateDatabaseAsync(string databaseId)
        {
            // Create a new database
            this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            Console.WriteLine("Created Database: {0}\n", this.database.Id);
        }
        // </CreateDatabaseAsync>


        // <CreateContainerAsync>
        /// <summary>
        /// Create the container if it does not exist. 
        /// </summary>
        /// <returns></returns>
        private async Task CreateContainerAsync(string containerId, string partitionKey)
        {
            // Create a new container
            this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, partitionKey, 400);
            Console.WriteLine("Created Container: {0}\n", this.container.Id);
        }
        // </CreateContainerAsync>

        // <QueryItemsAsync>
        /// <summary>
        /// Run a query (using Azure Cosmos DB SQL syntax) against the container
        /// </summary>
        public async Task CreateDBConnection(string EndpointUri, string PrimaryKey, string databaseId, string containerId, string partitionKey)
        {
            await GetStartedAsync(EndpointUri, PrimaryKey, databaseId, containerId, partitionKey);
        }

        // <QueryItemsAsync>
        /// <summary>
        /// Run a query (using Azure Cosmos DB SQL syntax) against the container
        /// </summary>
        public async Task<bool> CheckProductAsync(string value, string property)
        {
            var sqlQueryText = $"SELECT c.id FROM c WHERE c.{property} = '{value}'";

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<ProductDBDetails> queryResultSetIterator = this.container.GetItemQueryIterator<ProductDBDetails>(queryDefinition);

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<ProductDBDetails> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                if (currentResultSet.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;

        }

        public async Task<int> AddItemsToContainerAsync(string productId, string productName, int productPrice, string productImageURL, string productCategory)
        {
            ProductDBDetails productDBDetails = new ProductDBDetails
            {
                Id = productId,
                ProductName = productName,
                Price = productPrice,
                Image = productImageURL,
                Category = productCategory
            };

            try
            {
                // Read the item to see if it exists.  
                ItemResponse<ProductDBDetails> productResponse = await this.container.ReadItemAsync<ProductDBDetails>(productDBDetails.Id, new PartitionKey(productDBDetails.ProductName));
                Console.WriteLine("Item in database with id: {0} already exists\n", productResponse.Resource.Id);
                return -1;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {

                ItemResponse<ProductDBDetails> productResponse = await this.container.CreateItemAsync<ProductDBDetails>(productDBDetails, new PartitionKey(productDBDetails.ProductName));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", productResponse.Resource.Id, productResponse.RequestCharge);

                return 1;
            }
            
        }
        // </AddItemsToContainerAsync>

        public async Task<List<ProductDBDetails>> QueryLatestCategoryItemsAsync(string category)
        {
            var sqlQueryText = $"SELECT TOP 1 * FROM c WHERE c.Category = '{category}' ORDER BY c.id DESC";

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<ProductDBDetails> queryResultSetIterator = this.container.GetItemQueryIterator<ProductDBDetails>(queryDefinition);

            List<ProductDBDetails> productDBDetails = new List<ProductDBDetails>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<ProductDBDetails> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (ProductDBDetails productDB in currentResultSet)
                {
                    productDBDetails.Add(productDB);
                    Console.WriteLine("\tRead {0}\n", productDB);
                }
            }
            return productDBDetails;
        }

        public async Task<List<ProductDBDetails>> QueryAllItemsAsync(string operation, string category)
        {
            var sqlQueryText = $"SELECT * FROM c";

            if (operation.Equals("All"))
            {
                sqlQueryText = $"SELECT * FROM c";
            }
            else if (operation.Equals("Category"))
            {
                sqlQueryText = $"SELECT * FROM c WHERE c.Category = '{category}'";
            }

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<ProductDBDetails> queryResultSetIterator = this.container.GetItemQueryIterator<ProductDBDetails>(queryDefinition);

            List<ProductDBDetails> productDetails = new List<ProductDBDetails>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<ProductDBDetails> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (ProductDBDetails product in currentResultSet)
                {
                    productDetails.Add(product);
                    Console.WriteLine("\tRead {0}\n", product);
                }
            }
            return productDetails;
        }

        public async Task<List<ProductDBDetails>> QueryItemWithIdAsync(string productId)
        {
            var sqlQueryText = $"SELECT * FROM c where c.id = '{productId}'";

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<ProductDBDetails> queryResultSetIterator = this.container.GetItemQueryIterator<ProductDBDetails>(queryDefinition);

            List<ProductDBDetails> productDetails = new List<ProductDBDetails>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<ProductDBDetails> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (ProductDBDetails product in currentResultSet)
                {
                    productDetails.Add(product);
                    Console.WriteLine("\tRead {0}\n", product);
                }
            }
            return productDetails;
        }

        public async Task<bool> DeleteItemAsync(string partitionKey, string id)
        {
            var partitionKeyValue = partitionKey;
            var userId = id;

            try
            {
                // Delete an item. Note we must provide the partition key value and id of the item to delete
                ItemResponse<ProductDBDetails> productDBResponse = await this.container.DeleteItemAsync<ProductDBDetails>(userId, new PartitionKey(partitionKeyValue));
                Console.WriteLine("Deleted ProductDBDetails [{0},{1}]\n", partitionKeyValue, userId);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        /// <summary>
        /// Replace an item in the container
        /// </summary>
        public async Task<bool> UpdateProductCatalog(string productID, string productName, string propertyToChange, string newValue)
        {
            ItemResponse<ProductDBDetails> productDBResponse = await this.container.ReadItemAsync<ProductDBDetails>(productID, new PartitionKey(productName));
            var itemBody = productDBResponse.Resource;

            switch (propertyToChange)
            {
                case "ProductName":
                    List<ProductDBDetails> productList = await QueryItemWithIdAsync(productID);
                    if (await DeleteItemAsync(productName, productID))
                    {
                        if (await AddItemsToContainerAsync(productList[0].Id, newValue, productList[0].Price, productList[0].Image, productList[0].Category) == -1)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }

                case "Price":
                    itemBody.Price = int.Parse(newValue);
                    break;

                case "Image":
                    itemBody.Image = newValue;
                    break;

                case "Category":
                    itemBody.Category = newValue;
                    break;

                default:
                    break;
            }

            // replace the item with the updated content
            productDBResponse = await this.container.ReplaceItemAsync<ProductDBDetails>(itemBody, itemBody.Id, new PartitionKey(productName));
            Console.WriteLine("Updated Product [{0},{1}].\n \tBody is now: {2}\n", productName, itemBody.Id, productDBResponse.Resource);

            return true;
        }
    }
}
