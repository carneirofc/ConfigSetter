using System.CommandLine;
using System.CommandLine.Binding;
using ConfigSetter.Model;

namespace ConfigSetter.Binders;

public class UpdateConfigBinder : BinderBase<UpdateConfigParameters>
{
    private readonly Option<FileInfo> _configurationOption;
    private readonly Option<FileInfo> _inputSettingsOption;

    public UpdateConfigBinder(Option<FileInfo> configurationOption, Option<FileInfo> inputSettingsOption) : base()
    {
        _configurationOption = configurationOption;
        _inputSettingsOption = inputSettingsOption;
    }

    protected override UpdateConfigParameters GetBoundValue(BindingContext bindingContext)
    {
        return new UpdateConfigParameters()
        {
            Configuration = bindingContext.ParseResult.CommandResult.GetValueForOption(_configurationOption) ?? throw new ArgumentException("Configuration file is required"),
            InputSettings = bindingContext.ParseResult.CommandResult.GetValueForOption(_inputSettingsOption) ?? throw new ArgumentException("Input settings file is required")
        };
    }
}
