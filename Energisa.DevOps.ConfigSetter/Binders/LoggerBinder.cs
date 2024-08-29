using System.CommandLine;
using System.CommandLine.Binding;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Energisa.DevOps.ConfigSetter.Binders;

public class LoggerBinder : BinderBase<ILogger>
{
    private readonly Option<string> _loggingFormatOption;
    private readonly Option<bool> _versionOption;
    private readonly string _loggerName;
    public LoggerBinder(Option<bool> versionOption, Option<string> loggingFormatOption, string loggerName) : base()
    {
        _versionOption = versionOption;
        _loggerName = loggerName;
        _loggingFormatOption = loggingFormatOption;
    }
    protected override ILogger GetBoundValue(BindingContext bindingContext) => GetLogger(bindingContext);

    ILogger GetLogger(BindingContext bindingContext)
    {
        var verbose = bindingContext.ParseResult.CommandResult.GetValueForOption(_versionOption);
        var loggingFormat = bindingContext.ParseResult.CommandResult.GetValueForOption(_loggingFormatOption);
        using ILoggerFactory loggerFactory = LoggerFactory.Create(
            builder =>
            {

                builder.SetMinimumLevel(verbose ? LogLevel.Debug : LogLevel.Information);
                if (loggingFormat == "json")
                {
                    builder.AddJsonConsole(options =>
                    {
                        options.JsonWriterOptions = new JsonWriterOptions
                        {
                            Indented = false,
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Default
                        };
                    });
                }
                if (loggingFormat == "text")
                {
                    builder.AddConsole();
                }
            }
            );
        ILogger logger = loggerFactory.CreateLogger(_loggerName);
        return logger;
    }
}