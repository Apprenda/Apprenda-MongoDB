namespace Apprenda.Addons.MongoDB.Test
{
    using System.Collections.Generic;
    using System.Configuration;
    using Apprenda.AddOns.MongoDB;
    using Apprenda.SaaSGrid.Addons;
    using NUnit.Framework;

    [TestFixture]
    public class MongoTest
    {
        private AddonProvisionRequest ProvisionRequest { get; set; }
        private AddonDeprovisionRequest DeprovisionRequest { get; set; }
        private AddonTestRequest TestRequest { get; set; }

        [SetUp]
        public void SetupManifest()
        {
            this.ProvisionRequest = new AddonProvisionRequest
                                        {
                                            Manifest = SetupPropertiesAndParameters(),
                                            DeveloperParameters = SetUpParameters()
                                        };
            this.DeprovisionRequest = new AddonDeprovisionRequest { Manifest = SetupPropertiesAndParameters() };
            this.TestRequest = new AddonTestRequest { Manifest = SetupPropertiesAndParameters() };
        }

        private static List<AddonParameter> SetUpParameters()
        {
            var paramConstructor = new List<AddonParameter>
            {
                new AddonParameter
                {
                    Key = "username",
                    Value = ConfigurationManager.AppSettings["username"]
                },
                new AddonParameter
                {
                    Key = "password",
                    Value = ConfigurationManager.AppSettings["password"]
                },
                new AddonParameter
                {
                    Key = "database",
                    Value = ConfigurationManager.AppSettings["database"]
                }
            };
            return paramConstructor;
        }

        private static AddonManifest SetupPropertiesAndParameters()
        {
            var plist = new List<DevParameter>();
            plist.Add(new DevParameter()
                          {
                              Key = "username",
                              DisplayName = "Username"
                          });
            plist.Add(new DevParameter()
                          {
                              Key = "password",
                              DisplayName = "Password",
                              IsEncrypted = true
                          });
            plist.Add(new DevParameter()
                          {
                              Key = "database",
                              DisplayName = "Database"
                          });
            var port = new AddonProperty
                            {
                                Key = "port",
                                Value = "32772"
                            };
            var manifest = new AddonManifest
            {
                AllowUserDefinedParameters = true,
                Author = "Chris Dutra",
                DeploymentNotes = "",
                Description = "",
                DeveloperHelp = "",
                IsEnabled = true,
                ManifestVersionString = "2.0",
                Name = "MongoDB",
                
                // we'll handle parameters below.
                Parameters = new ParameterList
                {
                    AllowUserDefinedParameters = "true",
                    Items = plist.ToArray()
                },
                Properties = new List<AddonProperty>(),                                 
                ProvisioningLocation = "docker",
                ProvisioningPassword = "admin",
                ProvisioningPasswordHasValue = false,
                ProvisioningUsername = "admin",
                Vendor = "Apprenda",
                Version = "3.1"
            };
            manifest.Properties.Add(port);

            return manifest;
        }

        [Test]
        public void ClientTest()
        {

        }

        [Test]
        public void ParseDeveloperParametersTest()
        {
            // covers the provision method
            var provisionDevParameters = DeveloperParameters.Parse(this.ProvisionRequest.DeveloperParameters,
                this.ProvisionRequest.Manifest.GetProperties());
            Assert.IsNotNull(provisionDevParameters);
            Assert.That(provisionDevParameters, Is.TypeOf(typeof(DeveloperParameters)));
            // coverts the deprovision method
            var deprovisionDevParameters = DeveloperParameters.Parse(this.DeprovisionRequest.DeveloperParameters,
                this.ProvisionRequest.Manifest.GetProperties());
            Assert.IsNotNull(deprovisionDevParameters);
            Assert.That(deprovisionDevParameters, Is.TypeOf(typeof(DeveloperParameters)));
            // covers the test method
            var testDevParameters = DeveloperParameters.Parse(this.TestRequest.DeveloperParameters,
                this.ProvisionRequest.Manifest.GetProperties());
            Assert.IsNotNull(testDevParameters);
            Assert.That(testDevParameters, Is.TypeOf(typeof(DeveloperParameters)));
        }

        [Test]
        public void ProvisionTest()
        {
            this.ProvisionRequest = new AddonProvisionRequest { Manifest = SetupPropertiesAndParameters(), DeveloperParameters = SetUpParameters()};
            var output = new MongoDbAddOn().Provision(this.ProvisionRequest);
            Assert.That(output, Is.TypeOf<ProvisionAddOnResult>());
            Assert.That(output.IsSuccess, Is.EqualTo(true));
            Assert.That(output.ConnectionData.Length, Is.GreaterThan(0));
        }

        [Test]
        public void DeProvisionTest()
        {
            this.DeprovisionRequest = new AddonDeprovisionRequest()
                                          {
                                              Manifest = SetupPropertiesAndParameters(),
                                              DeveloperParameters = SetUpParameters()
                                          };
            var output = new MongoDbAddOn().Deprovision(this.DeprovisionRequest);
            Assert.That(output, Is.TypeOf<OperationResult>());
            Assert.That(output.IsSuccess, Is.EqualTo(true));
        }

        [Test]
        public void SocTest()
        {
            this.TestRequest = new AddonTestRequest()
                                   {
                                       Manifest = SetupPropertiesAndParameters(),
                                       DeveloperParameters = SetUpParameters()
                                   };
            var output = new MongoDbAddOn().Test(this.TestRequest);
            Assert.That(output, Is.TypeOf<OperationResult>());
            Assert.That(output.IsSuccess, Is.EqualTo(true));
        }
    }

    public class DevParameter : IAddOnParameterDefinition
    {
        public string Key { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public bool IsEncrypted { get; set; }

        public bool IsRequired { get; set; }

        public bool HasValue { get; set; }

        public string DefaultValue { get; set; }
    }
}
