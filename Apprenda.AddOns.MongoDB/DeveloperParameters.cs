using System.Collections.Generic;

namespace Apprenda.AddOns.MongoDB
{
    using System.Linq;
    using Apprenda.SaaSGrid.Addons;

    public class DeveloperParameters
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }

        public static DeveloperParameters Parse(IEnumerable<AddonParameter> parameters, IEnumerable<IAddOnPropertyDefinition> manifestProperties)
        {
            var options = new DeveloperParameters();
            if (parameters != null)
            {
                options = parameters.Aggregate(options, (current, param) => MapToOption(current, param.Key.ToLowerInvariant(), param.Value));
            }
            if (manifestProperties != null)
            {
                options = manifestProperties.Aggregate(options, (current, prop) => MapToOption(current, prop.Key.ToLowerInvariant(), prop.Value));
            }
            return options;
        }

        private static DeveloperParameters MapToOption(DeveloperParameters options, string key, string value)
        {
            if ("username".Equals(key))
            {
                options.Username = value;
                return options;
            }
            if ("password".Equals(key))
            {
                options.Password = value;
                return options;
            }
            if ("database".Equals(key))
            {
                options.Database = value;
                return options;
            }
            return options;
        }
    }
}
