using ConfigSetter.Actions;
using ConfigSetter.Binders;
using System.CommandLine;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace ConfigSetter.Commands;


public class RootCommand
{
    public int Invoke(string[] args)
    {

        var loggingVerboseOption = new Option<bool>(
            aliases: ["--verbose", "-v"],
            description: "Show verbose output",
            getDefaultValue: () => false
        )
        { IsRequired = false, Arity = ArgumentArity.Zero };

        var loggingSilentOption = new Option<bool>(
            aliases: ["--silent", "-s"],
            description: "Show no output",
            getDefaultValue: () => false
        )
        { IsRequired = false, Arity = ArgumentArity.Zero };

        var loggingToStdErrOptions = new Option<bool>(
            aliases: ["--log-to-stderr"],
            description: "Log to stderr",
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

        var outputFormatOption = new Option<string>(
            aliases: ["--output-format"],
            description: "The output format to use",
            getDefaultValue: () => "json"
        )
        { IsRequired = false, Arity = ArgumentArity.ExactlyOne };
        outputFormatOption.FromAmong("json", "yaml");
        var outputFileOption = new Option<FileInfo>(
            aliases: ["--output-file", "-o"],
            description: "The output file to write the settings to",
            parseArgument: (argResult) =>
            {
                var value = argResult.Tokens.Single().Value;
                var finfo = new FileInfo(value);
                if (finfo.Exists)
                {
                    throw new ArgumentException($"File {value} already exists");
                }
                return finfo;
            }
        )
        { AllowMultipleArgumentsPerToken = false, IsRequired = false, Arity = ArgumentArity.ExactlyOne };

        var rootCommand = new System.CommandLine.RootCommand(description: "A simple tool to parse settings files");
        rootCommand.AddOption(loggingVerboseOption);
        rootCommand.AddOption(loggingFormatOption);
        rootCommand.AddOption(loggingToStdErrOptions);
        rootCommand.AddOption(loggingSilentOption);

        rootCommand.AddOption(configurationOption);
        rootCommand.AddOption(inputSettingsOption);
        rootCommand.AddOption(prefixOption);

        rootCommand.AddOption(outputFormatOption);
        rootCommand.AddOption(outputFileOption);
        rootCommand.SetHandler((logger, parameters) =>
        {
            return new UpdateConfigAction(logger).Execute(parameters);

        },
            new LoggerBinder() { Name = "Root", VerboseOptions = loggingVerboseOption, SilentOption = loggingSilentOption, ToStdErrOption = loggingToStdErrOptions },
            new UpdateConfigBinder()
            {
                Prefix = prefixOption,
                ConfigurationOption = configurationOption,
                InputSettingsOption = inputSettingsOption,
                OutputFormatOption = outputFormatOption,
                OutputFileOption = outputFileOption
            }
        );

        return rootCommand.Invoke(args);
    }
}