using Apprenda.SaaSGrid.Addons;
using Apprenda.Services.Logging;
using MongoDB.Driver;
using System;
using System.Linq;

namespace Apprenda.AddOns.MongoDB
{
    public class MongoDbAddOn : AddonBase
    {
        private static readonly ILogger Logger = LogManager.Instance().GetLogger(typeof(MongoDbAddOn));

        public ProvisionAddOnResult Provision(AddonManifest manifest, string developerOptions)
        {
            // developerOptions is a string of arbitrary arguments used at provisioning time.
            // In this case, we need to know the username and password of the user to be assigned to this DB.
            // The expected format is: username=<username>,password=<password>
            //
            // NOTE: In the real world it may not be the best idea to pass in the username and password.
            //       Instead they could be derived and guaranteed unique from data in the manifest.
            //       However, this illustrates how one might use the developerOptions parameter.
            string username = null;
            string password = null;
            // since there is no connection data yet, this constructor doesn't make sense. but just throw an empty string for now.
            var result = new ProvisionAddOnResult("", false, "");
            developerOptions = developerOptions ?? string.Empty; //ensure we handle null developer options

            foreach (var innerPair in developerOptions.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(pair => pair.Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)).Where(innerPair => innerPair.Length >= 2))
            {
                switch (innerPair[0])
                {
                    case "username":
                        username = innerPair[1];
                        break;

                    case "password":
                        password = innerPair[1];
                        break;
                }
            }

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                result.EndUserMessage = "The developerOptions parameter must contain the username and password to assign to this developer's MongoDB instance. The correct format is: username=<username>,password=<password>";
                return result;
            }

            try
            {
                var server = GetServer(manifest);
                var databaseName = GetDatabaseName(manifest);

                if (server.DatabaseExists(databaseName))
                {
                    result.EndUserMessage = string.Format("A MongoDB instance with the name '{0}' already exists. Use a different instance alias.", databaseName);
                    return result;
                }

                var database = server.GetDatabase(databaseName);
                //MongoDatabaseSettings settings = new MongoDatabaseSettings(server, databaseName);

                // creates a new database.
                //MongoDatabase newDatabase = server.GetDatabase(settings);
                // adds user to the admin database, thus should give it read/write all over. (probably bad for now but should work)
                //newDatabase.AddUser(new MongoCredentials(username, password), false);

                //MongoUser user = database.FindUser(username);
                // MongoDB does not actually create the DB until you put data into it.
                // So we store the time the DB was created to force creation.
                //var collection = database.GetCollection<ProvisioningData>("__provisioningData");
                //var insertResult = collection.Insert(new ProvisioningData());

                var newCollection = database.GetCollection<ProvisioningData>("__provisioningData");
                var newInsertResult = newCollection.Insert(new ProvisioningData());

                if (newInsertResult.Ok)
                {
                    // Set the connection string that the app will use.
                    // This connection string includes the username and password given for this instance.
                    result.ConnectionData = string.Format("mongodb://{0}:{1}@{2}/{3}", manifest.ProvisioningUsername, manifest.ProvisioningPassword, manifest.ProvisioningLocation, databaseName);
                    result.IsSuccess = true;
                }
                else
                {
                    result.IsSuccess = false;
                    result.EndUserMessage = string.Format("There was an error provisioning the database:{0}{1}", Environment.NewLine, newInsertResult.ErrorMessage);
                }
            }
            catch (MongoException mongoException)
            {
                Logger.ErrorFormat("Error occurred during provisioning: {0} \n {1}", mongoException.Message, mongoException.StackTrace);
                result.EndUserMessage = mongoException.Message;
                result.IsSuccess = false;
            }
            catch (Exception e)
            {
                result.IsSuccess = false;
                Logger.ErrorFormat("General error occurred during provisioning: {0} \n {1}", e.Message, e.StackTrace);
            }

            return result;
        }

        public OperationResult Deprovision(AddonManifest manifest, string connectionData)
        {
            var result = new OperationResult() { IsSuccess = false };

            try
            {
                var server = GetServer(manifest);
                var database = server.GetDatabase(GetDatabaseName(manifest));
                database.Drop();
                result.IsSuccess = true;
            }
            catch (MongoException mongoException)
            {
                Logger.ErrorFormat("Error occurred during deprovisioning: {0} \n {1}", mongoException.Message, mongoException.StackTrace);
                result.EndUserMessage = mongoException.Message;
            }
            catch (Exception mongoException)
            {
                Logger.ErrorFormat("Error occurred during deprovisioning: {0} \n {1}", mongoException.Message, mongoException.StackTrace);
                result.EndUserMessage = mongoException.Message;
            }

            return result;
        }

        public OperationResult Test(AddonManifest manifest, string developerOptions)
        {
            // NOTE: Any exceptions thrown out of any add-on method will result in a failure of the operation.
            //       The full-depth exception from inside the add-on will get logged in the SOC, but if you
            //       wish to display a more user-friendly message in the report card format then you need to
            //       handle exceptions manually inside the add-on implementation.

            var results = new OperationResult();

            try
            {
                var server = GetServer(manifest);

                if (server == null)
                {
                    results.EndUserMessage = "Unable to connect to the server using the information from the add-on manifest.";
                    return results;
                }

                // Create a DB and add a collection to make sure the MongoDB instance is configured correctly.
                var database = server.GetDatabase("test");
                var collection = database.GetCollection<TestObject>("testObjects");
                var insertResult = collection.Insert(new TestObject { Value = "test" });
                database.Drop();

                if (insertResult.Ok)
                {
                    results.IsSuccess = true;
                }
                else
                {
                    results.EndUserMessage = insertResult.ErrorMessage;
                }
            }
            catch (MongoException mongoException)
            {
                Logger.ErrorFormat("Error occurred during testing: {0} \n {1}", mongoException.Message, mongoException.StackTrace);
                results.EndUserMessage = mongoException.Message;
            }
            catch (Exception mongoException)
            {
                Logger.ErrorFormat("Error occurred during testing: {0} \n {1}", mongoException.Message, mongoException.StackTrace);
                results.EndUserMessage = mongoException.Message;
            }

            return results;
        }

        private static MongoServer GetServer(AddonManifest manifest)
        {
            var connectionString = string.Format("mongodb://{0}(admin):{1}@{2}", manifest.ProvisioningUsername, manifest.ProvisioningPassword, manifest.ProvisioningLocation);
            var client = new MongoClient(connectionString);
            return client.GetServer();
        }

        private static string GetDatabaseName(AddonManifest manifest)
        {
            return string.Format("{0}-{1}", manifest.CallingDeveloperAlias, manifest.InstanceAlias);
        }

        private class TestObject
        {
            public string Value { get; set; }
        }

        private class ProvisioningData
        {
            public ProvisioningData()
            {
                ProvisionTime = DateTime.Now;
            }

            public DateTime ProvisionTime { get; set; }
        }

        public override OperationResult Deprovision(AddonDeprovisionRequest request)
        {
            return Deprovision(request.Manifest, request.ConnectionData);
        }

        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            return Provision(request.Manifest, request.DeveloperOptions);
        }

        public override OperationResult Test(AddonTestRequest request)
        {
            return Test(request.Manifest, request.DeveloperOptions);
        }
    }
}