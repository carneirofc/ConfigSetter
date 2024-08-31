using ConfigSetter.Logging;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.IO;
using System.CommandLine.Rendering;
using System.Text.Json;
namespace ConfigSetter.Binders;

public class LoggerBinder : BinderBase<ILogger>
{
    public required Option<bool> SilentOption { get; set; }
    public required Option<bool> VerboseOptions { get; set; }
    public required Option<bool> ToStdErrOption { get; set; }
    public required string Name { get; set; }

    public LoggerBinder() : base() { }

    protected override ILogger GetBoundValue(BindingContext bindingContext) => GetLogger(bindingContext);

    ILogger GetLogger(BindingContext bindingContext)
    {
        var verbose = bindingContext.ParseResult.GetValueForOption(VerboseOptions);
        var silent = bindingContext.ParseResult.GetValueForOption(SilentOption);
        var logToStdErr = bindingContext.ParseResult.GetValueForOption(ToStdErrOption);

        var minimalLogLevel = silent ? LogLevel.None : verbose ? LogLevel.Debug : LogLevel.Information;
        var minimalErrLevel = silent ? LogLevel.None : logToStdErr ? LogLevel.Trace : LogLevel.Warning;

        var loggerFactory = new LoggerFactory().AddSimpleConsole(minimalLogLevel: minimalLogLevel, minimalErrorLevel: minimalErrLevel);
        var logger = loggerFactory.CreateLogger<Program>();
        return logger;
    }
}