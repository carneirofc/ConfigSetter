
using ConfigSetter.Model;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Dynamic;
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
        _logger.LogInformation("Updating configuration file {0} with input settings file {1}", parameters.Configuration.FullName, parameters.InputSettings.FullName);
        var config = LoadYamlConfig<Config>(parameters.Configuration);
        _logger.LogInformation("Loaded configuration file {0}", JsonConvert.SerializeObject(config));
        if (config == null)
        {
            _logger.LogError("Configuration file is empty");
            return Task.FromResult(1);
        }

        var settings = LoadYamlConfig<object>(parameters.InputSettings);
        _logger.LogInformation("Loaded settings file {0} type {1}", JsonConvert.SerializeObject(settings as object), settings?.GetType());


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
        _logger.LogDebug("New document after setup: {0}", JsonConvert.SerializeObject(newDocument));


        // var patchDoc = new JsonPatchDocument();
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

        var serializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .WithQuotingNecessaryStrings()
            .Build();

        _logger.LogInformation("New document: {0}", JsonConvert.SerializeObject(newDocument));
        _logger.LogInformation("New document as YAML: {0}", serializer.Serialize(newDocument));

        if (parameters.OutputFile != null)
        {
            File.WriteAllText(parameters.OutputFile.FullName, serializer.Serialize(newDocument));
        }
        else
        {
            Console.WriteLine(serializer.Serialize(newDocument));
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
            .WithAttemptingUnquotedStringTypeDeserialization()
            .Build();
        var data = File.ReadAllText(configuration.FullName);
        _logger.LogDebug("Read file {0} with content:\n{1}", configuration.FullName, data);
        var obj = deserializer.Deserialize<T>(data);
        return obj;
    }
}