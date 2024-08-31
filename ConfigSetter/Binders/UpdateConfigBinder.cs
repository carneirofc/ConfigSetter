using ConfigSetter.Model;
using System.CommandLine;
using System.CommandLine.Binding;

namespace ConfigSetter.Binders;

public class UpdateConfigBinder : BinderBase<UpdateConfigParameters>
{
    public required Option<FileInfo> ConfigurationOption { get; set; }
    public required Option<FileInfo> InputSettingsOption { get; set; }
    public required Option<string> Prefix { get; set; }
    public required Option<string> OutputFormatOption { get; set; }
    public required Option<FileInfo> OutputFileOption { get; set; }

    protected override UpdateConfigParameters GetBoundValue(BindingContext bindingContext)
    {
        return new UpdateConfigParameters()
        {
            Configuration = bindingContext.ParseResult.CommandResult.GetValueForOption(ConfigurationOption) ?? throw new ArgumentException("Configuration file is required"),
            InputSettings = bindingContext.ParseResult.CommandResult.GetValueForOption(InputSettingsOption) ?? throw new ArgumentException("Input settings file is required"),
            Prefix = bindingContext.ParseResult.CommandResult.GetValueForOption(Prefix) ?? throw new ArgumentException("Prefix is required"),
            OutputFormat = bindingContext.ParseResult.CommandResult.GetValueForOption(OutputFormatOption) ?? "yaml",
            OutputFile = bindingContext.ParseResult.CommandResult.GetValueForOption(OutputFileOption) ?? null
        };
    }
}