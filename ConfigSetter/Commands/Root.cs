using ConfigSetter.Actions;
using ConfigSetter.Binders;
using System.CommandLine;
using System.Text.RegularExpressions;

namespace ConfigSetter.Commands
{

    public class Root
    {
        public int Invoke(string[] args)
        {

            var verboseOption = new Option<bool>(
                aliases: ["--verbose", "-v"],
                description: "Show verbose output",
                getDefaultValue: () => false
            )
            { IsRequired = false, Arity = ArgumentArity.Zero };

            var loggingFormatOption = new Option<string>(
                aliases: ["--logging-format", "-f"],
                description: "The logging format to use",
                getDefaultValue: () => "text"
            )
            { IsRequired = false, Arity = ArgumentArity.ExactlyOne, };
            loggingFormatOption.FromAmong("text", "json");

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

            var prefixOption = new Option<string>(
                aliases: ["--prefix", "-p"],
                description: "The prefix to use for the settings",
                getDefaultValue: () => "dev"
            )
            { IsRequired = false, Arity = ArgumentArity.ExactlyOne };
            prefixOption.AddValidator((result) =>
            {
                var value = result.GetValueOrDefault<string>();
                if (string.IsNullOrWhiteSpace(value))
                {
                    result.ErrorMessage = "The prefix must be at least 1 characters long";
                    return;
                }

                if (Regex.IsMatch(value, @"[^a-zA-Z0-9]"))
                {
                    result.ErrorMessage = "The prefix must contain only alphanumeric characters";
                    return;
                }
            });

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
            rootCommand.AddOption(loggingFormatOption);
            rootCommand.SetHandler((logger, parameters) =>
            {
                return new UpdateConfig(logger).Execute(parameters);

            },
                new LoggerBinder(verboseOption, loggingFormatOption, "Root"),
                new UpdateConfigBinder(configurationOption, inputSettingsOption)
            );

            return rootCommand.Invoke(args);
        }
    }
}