
using ConfigSetter.Model;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Dynamic;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ConfigSetter.Actions;

public class UpdateConfigAction
{
    private readonly ILogger _logger;
    public UpdateConfigAction(ILogger logger)
    {
        _logger = logger;
    }

    public Task<int> Execute(UpdateConfigParameters parameters)
    {
        _logger.LogInformation("Configuration file {0}", parameters.Configuration.FullName);
        _logger.LogInformation("Input settings file {1}", parameters.InputSettings.FullName);

        var config = LoadYamlConfig<Config>(parameters.Configuration);
        _logger.LogDebug("Loaded configuration file\n{0}", JsonConvert.SerializeObject(config));
        if (config == null)
        {
            _logger.LogError("Configuration file is empty");
            return Task.FromResult(1);
        }

        var settings = LoadYamlConfig<object>(parameters.InputSettings);
        _logger.LogDebug("Loaded settings file\n{0}", JsonConvert.SerializeObject(settings));


        if (settings == null)
        {
            _logger.LogError("Settings file is empty");
            return Task.FromResult(1);
        }
        if (settings is not Dictionary<object, object>)
        {
            _logger.LogError("Settings file is not a dictionary");
            return Task.FromResult(1);
        }
        var dict = settings as Dictionary<object, object>;
        if (dict == null)
        {
            _logger.LogError("Settings file is not a dictionary");
            return Task.FromResult(1);
        }
        var variables = dict.GetValueOrDefault("variables") as Dictionary<object, object>;
        if (variables == null)
        {
            _logger.LogError("Settings file does not contain a 'variables' key");
            return Task.FromResult(1);
        }

        ExpandoObject newDocument = new();
        JsonPatchDocument setupPatch = new();
        foreach (var patch in config.Setup.JsonPatch)
        {
            setupPatch.Operations.Add(new Operation() { op = patch.Op.ToLower(), path = patch.Path, value = patch.Value });
        }
        setupPatch.ApplyTo(newDocument);
        _logger.LogDebug("New document after setup:\n{0}", JsonConvert.SerializeObject(newDocument));


        foreach (var kvp in variables)
        {
            _logger.LogDebug("Key: {0}, Value: {1}", kvp.Key, kvp.Value);
            _logger.LogDebug("Key: {0}, Value: {1}", kvp.Key.GetType(), kvp.Value.GetType());

            if (kvp.Key is string settingName && settingName.StartsWith(parameters.Prefix))
            {
                _logger.LogDebug("Setting key: {0}", settingName);
                var setting = config.Settings.FirstOrDefault(s => $"{parameters.Prefix}_{s.Name}" == settingName);
                if (setting == null)
                {
                    _logger.LogWarning("Setting with name {0} not found in configuration", settingName);
                    continue;
                }
                foreach (var patch in setting.JsonPatch)
                {
                    var patchDoc = new JsonPatchDocument();
                    var operation = new Operation() { op = patch.Op.ToLower(), path = patch.Path, value = kvp.Value };
                    _logger.LogDebug("Adding patch {0}", JsonConvert.SerializeObject(operation));
                    patchDoc.Operations.Add(operation);
                    patchDoc.ApplyTo(newDocument);

                    _logger.LogDebug("New document: {0}", JsonConvert.SerializeObject(newDocument));
                }
            }
        }

        var serialized = Serialize(parameters.OutputFormat, newDocument);
        if (string.IsNullOrEmpty(serialized))
        {
            _logger.LogError("Failed to serialize document");
            return Task.FromResult(1);
        }

        if (parameters.OutputFile != null)
        {
            if (parameters.OutputFile.Exists)
            {
                _logger.LogWarning("Output file {0} already exists, overwriting", parameters.OutputFile.FullName);
                parameters.OutputFile.Delete();
            }
            File.WriteAllText(parameters.OutputFile.FullName, serialized, Encoding.Default);
            _logger.LogDebug("Wrote new document to {0} using encoding {1}", parameters.OutputFile.FullName, Encoding.Default.EncodingName);
        }
        else
        {
            Console.WriteLine(serialized);
        }

        return Task.FromResult(0);
    }

    private string Serialize(string format, object obj)
    {
        if (format == "json")
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }
        if (format == "yaml")
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .WithQuotingNecessaryStrings()
                .Build();
            return serializer.Serialize(obj);
        }
        _logger.LogError("Unsupported format \"{0}\"", format);
        return string.Empty;
    }

    private T? LoadYamlConfig<T>(FileInfo configuration)
    {
        if (!configuration.Exists)
        {
            throw new ArgumentException($"File {configuration.FullName} does not exist");
        }
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .WithAttemptingUnquotedStringTypeDeserialization()
            .Build();
        var data = File.ReadAllText(configuration.FullName);
        _logger.LogDebug("Read file {0} with content:\n{1}", configuration.FullName, data);
        var obj = deserializer.Deserialize<T>(data);
        return obj;
    }
}