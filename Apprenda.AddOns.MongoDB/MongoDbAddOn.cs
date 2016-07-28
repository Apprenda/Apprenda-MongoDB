using Apprenda.SaaSGrid.Addons;
using Apprenda.Services.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System;

namespace Apprenda.AddOns.MongoDB
{
    public class MongoDbAddOn : AddonBase
    {
        private static readonly ILogger Logger = LogManager.Instance().GetLogger(typeof(MongoDbAddOn));

        /// <summary>
        /// Provisions an instance of this add-on.
        /// </summary>
        /// <param name="_request">A request object encapsulating all the parameters available to provision an add-on.</param>
        /// <returns>
        /// A <see cref="T:Apprenda.SaaSGrid.Addons.ProvisionAddOnResult"/> object containing the results of the operation as well as the data needed to connect to / use the newly provisioned instance.
        /// </returns>
        public override ProvisionAddOnResult Provision(AddonProvisionRequest _request)
        {
            if (_request == null)
            {
                throw new ArgumentNullException("_request");
            }
            // developerOptions is a string of arbitrary arguments used at provisioning time.
            // In this case, we need to know the username and password of the user to be assigned to this DB.
            // The expected format is: username=<username>,password=<password>
            //
            // NOTE: In the real world it may not be the best idea to pass in the username and password.
            //       Instead they could be derived and guaranteed unique from data in the manifest.
            //       However, this illustrates how one might use the developerOptions parameter.

            var parameters = DeveloperParameters.Parse(_request.DeveloperParameters, _request.Manifest.Properties);
            // since there is no connection data yet, this constructor doesn't make sense. but just throw an empty string for now.
            var result = new ProvisionAddOnResult("", false, "");
            try
            {
                string port, connectionString;
                try
                {
                    port = _request.Manifest.Properties.Find(_x => _x.Key.Equals("port")).Value;
                }
                catch(ArgumentNullException)
                {
                    port = "27017";
                }
                if (_request.Manifest.ProvisioningUsername.Equals("admin") && !(_request.Manifest.ProvisioningPasswordHasValue))
                {
                    connectionString = string.Format("mongodb://{0}:{1}", _request.Manifest.ProvisioningLocation, port);
                }
                else
                {
                    connectionString = string.Format(
                        "mongodb://{0}:{1}@{2}:{3}", _request.Manifest.ProvisioningUsername,
                        _request.Manifest.ProvisioningPassword,
                        _request.Manifest.ProvisioningLocation,
                        port);
                }
                var client = new MongoClient(connectionString);
                var databaseName = GetDatabaseName(_request.Manifest, parameters);
                var cred = MongoCredential.CreateMongoCRCredential(databaseName, _request.Manifest.ProvisioningUsername, _request.Manifest.ProvisioningPassword);
                var database = client.GetDatabase(databaseName);
                var document = CreateUserAdd(parameters.Username, parameters.Password, databaseName);
                database.RunCommand<BsonDocument>(document);
                // creates a new database. note - the database will not be created until something is written to it
                var newCollection = database.GetCollection<ProvisioningData>("__provisioningData");
                newCollection.InsertOne(new ProvisioningData());

                // Set the connection string that the app will use.
                // This connection string includes the username and password given for this instance.
                //result.ConnectionData = string.Format("mongodb://{0}:{1}@{2}:{3}/{4}", _request.Manifest.ProvisioningUsername, _request.Manifest.ProvisioningPassword, _request.Manifest.ProvisioningLocation, port, databaseName);
                result.ConnectionData = string.Format("mongodb://{0}:{1}/{2}", _request.Manifest.ProvisioningLocation, port, databaseName);
                result.IsSuccess = true;
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

        public override OperationResult Deprovision(AddonDeprovisionRequest _request)
        {
            var result = new OperationResult() { IsSuccess = false };
            string port;
            try
            {
                port = _request.Manifest.Properties.Find(_x => _x.Key.Equals("port")).Value;
            }
            catch (ArgumentNullException)
            {
                port = "27017";
            }
            try
            {
                var parameters = DeveloperParameters.Parse(_request.DeveloperParameters, _request.Manifest.Properties);
                var connectionString = string.Format(
                    "mongodb://{0}:{1}",
                    _request.Manifest.ProvisioningLocation,
                    port);
                var client = new MongoClient(connectionString);
                var name = GetDatabaseName(_request.Manifest, parameters);
                var db = client.GetDatabase(name);
                var drop = DropUser(parameters.Username);
                db.RunCommand<BsonDocument>(drop);
                client.DropDatabase(name);
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

        public override OperationResult Test(AddonTestRequest _request)
        {
            // NOTE: Any exceptions thrown out of any add-on method will result in a failure of the operation.
            //       The full-depth exception from inside the add-on will get logged in the SOC, but if you
            //       wish to display a more user-friendly message in the report card format then you need to
            //       handle exceptions manually inside the add-on implementation.

            var result = new OperationResult() { IsSuccess = false };
            string port;
            try
            {
                port = _request.Manifest.Properties.Find(_x => _x.Key.Equals("port")).Value;
            }
            catch (ArgumentNullException)
            {
                port = "27017";
            }
            try
            {
                var parameters = DeveloperParameters.Parse(_request.DeveloperParameters, _request.Manifest.Properties);
                var connectionString = string.Format(
                    "mongodb://{0}:{1}",
                    _request.Manifest.ProvisioningLocation,
                    port);
                var client = new MongoClient(connectionString);

                // Create a DB and add a collection to make sure the MongoDB instance is configured correctly.
                var database = client.GetDatabase("test");
                var collection = database.GetCollection<TestObject>("testObjects");
                collection.InsertOne(new TestObject { Value = "test" });
                client.DropDatabase("test");
                result.IsSuccess = true;
            }
            catch (MongoException mongoException)
            {
                Logger.ErrorFormat("Error occurred during testing: {0} \n {1}", mongoException.Message, mongoException.StackTrace);
                result.EndUserMessage = mongoException.Message;
            }
            catch (Exception mongoException)
            {
                Logger.ErrorFormat("Error occurred during testing: {0} \n {1}", mongoException.Message, mongoException.StackTrace);
                result.EndUserMessage = mongoException.Message;
            }

            return result;
        }

        private static string GetDatabaseName(AddonManifest manifest, DeveloperParameters p)
        {
            return p.Database != null ? string.Format("{0}", p.Database) : string.Format("{0}-{1}", manifest.CallingDeveloperAlias, manifest.InstanceAlias);
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

        private BsonDocument CreateUserAdd(string username, string password, string db)
        {
            // Construct the createUser command.
            var writeConcern = WriteConcern.WMajority
                .With(wTimeout: TimeSpan.FromMilliseconds(5000));
            var command = new BsonDocument
            {
                { "createUser", username },
                { "pwd", password },
                { "roles", new BsonArray
                    {
                        new BsonDocument
                        {
                            { "role", "dbOwner" },
                            { "db", db }   
                        },
                        "readWrite"
                    }},
                { "writeConcern", writeConcern.ToBsonDocument() }
            };
            return command;
        }

        private BsonDocument DropUser(string username)
        {
            var writeConcern = WriteConcern.WMajority
                .With(wTimeout: TimeSpan.FromMilliseconds(5000));
            var command = new BsonDocument
            {
                { "dropUser", username },
                { "writeConcern", writeConcern.ToBsonDocument() }
            };
            return command;
        }
    }
}