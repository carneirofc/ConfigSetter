
using Microsoft.Extensions.Logging;
using ConfigSetter.Model;
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ConfigSetter.Actions
{
    public class UpdateConfig
    {
        private readonly ILogger _logger;
        public UpdateConfig(ILogger logger)
        {
            _logger = logger;
        }

        public Task<int> Execute(UpdateConfigParameters parameters)
        {
            _logger.LogInformation("Updating configuration file {0} with input settings file {1}", parameters.Configuration.FullName, parameters.InputSettings.FullName);
            var config = LoadYamlConfig<Config>(parameters.Configuration);
            _logger.LogInformation("Loaded configuration file {0}", JsonConvert.SerializeObject(config));
            var settings = LoadYamlConfig<dynamic>(parameters.InputSettings);
            _logger.LogInformation("Loaded settings file {0}", JsonConvert.SerializeObject(settings as object));

            if (settings == null)
            {
                _logger.LogError("Settings file is empty");
                return Task.FromResult(1);
            }

            if (settings.variables == null)
            {
                _logger.LogError("Settings file does not contain a 'variables' key");
                return Task.FromResult(1);
            }

            // Cast settings.variables to IDictionary<string, object>
            var variables = settings.variables as IDictionary<string, object>;
            if (variables != null)
            {
                foreach (var kvp in variables)
                {
                    _logger.LogInformation("Key: {0}, Value: {1}", kvp.Key, kvp.Value);
                }
            }
            else
            {
                _logger.LogWarning("settings['variables'] is not a dictionary");
            }


            return Task.FromResult(0);
        }

        private T? LoadYamlConfig<T>(FileInfo configuration)
        {
            if (!configuration.Exists)
            {
                throw new ArgumentException($"File {configuration.FullName} does not exist");
            }
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            var data = File.ReadAllText(configuration.FullName);
            _logger.LogDebug("Read file {0} with content:\n{1}", configuration.FullName, data);
            var obj = deserializer.Deserialize<T>(data);
            return obj;
        }
    }
}