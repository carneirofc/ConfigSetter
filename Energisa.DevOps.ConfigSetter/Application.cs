using System.CommandLine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Energisa.DevOps.SettingsParser
{

    public class DoStuffCommandOptions
    {
        public FileInfo Configuration { get; set; }
        public FileInfo InputSettings { get; set; }
    }
    public class Application
    {
        public async Task<int> Run(string[] args)
        {

            var statusCode = -1;
            var verboseOption = new Option<bool>(
                aliases: ["--verbose", "-v"],
                description: "Show verbose output",
                getDefaultValue: () => false
            )
            { IsRequired = false, Arity = ArgumentArity.Zero };

            var configurationOption = new Option<FileInfo>(
                aliases: ["--configuration", "-c"],
                description: "The configuration file to parse",
                parseArgument: (argResult) =>
                {
                    var value = argResult.Tokens.Single().Value;
                    var finfo = new FileInfo(value);
                    if (!finfo.Exists)
                    {
                        throw new ArgumentException($"File {value} does not exist");
                    }
                    return finfo;
                }
            )
            { AllowMultipleArgumentsPerToken = false, IsRequired = true, Arity = ArgumentArity.ExactlyOne };

            var inputSettingsOption = new Option<FileInfo>(
                aliases: ["--input-settings", "-i"],
                description: "The input settings file to parse",
                parseArgument: (argResult) =>
                {
                    var value = argResult.Tokens.Single().Value;
                    var finfo = new FileInfo(value);
                    if (!finfo.Exists)
                    {
                        throw new ArgumentException($"File {value} does not exist");
                    }
                    return finfo;
                }
            )
            { AllowMultipleArgumentsPerToken = false, IsRequired = true, Arity = ArgumentArity.ExactlyOne };

            var rootCommand = new RootCommand(description: "A simple tool to parse settings files");
            rootCommand.AddOption(verboseOption);
            rootCommand.AddOption(configurationOption);
            rootCommand.AddOption(inputSettingsOption);
            rootCommand.SetHandler(async (context) =>
            {
                var configuration = context.ParseResult.CommandResult.GetValueForOption(configurationOption) ?? throw new ArgumentException("Configuration file is required");
                var verbose = context.ParseResult.CommandResult.GetValueForOption(verboseOption);
                var inputSettings = context.ParseResult.CommandResult.GetValueForOption(inputSettingsOption) ?? throw new ArgumentException("Input settings file is required");
                statusCode = await DoStuff(new DoStuffCommandOptions
                {
                    Configuration = configuration,
                    InputSettings = inputSettings
                });
            });

            var parsed = await rootCommand.InvokeAsync(args);
            return 0;
        }

        public Task<int> DoStuff(DoStuffCommandOptions options)
        {
            _logger.LogInformation($"Configuration file: {options.Configuration.FullName}");

            var configuration = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build()
                .Deserialize(File.ReadAllText(options.Configuration.FullName));
            // _logger.LogInformation(JsonConvert.SerializeObject(configuration, Formatting.Indented));
            Console.WriteLine(JsonConvert.SerializeObject(configuration, Formatting.Indented));

            _logger.LogInformation($"Input settings file: {options.InputSettings.FullName}");
            return Task.FromResult(0);
        }
    }
}