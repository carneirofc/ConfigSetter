using System.CommandLine.Binding;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Energisa.DevOps.SettingsParser.Binders;

public class ApplicationBinder : BinderBase<ILogger>
{
    protected override ILogger GetBoundValue(
        BindingContext bindingContext) => GetLogger(bindingContext);

    ILogger GetLogger(BindingContext bindingContext)
    {
        using ILoggerFactory loggerFactory = LoggerFactory.Create(
            builder => builder.AddJsonConsole(options =>
            {
                options.JsonWriterOptions = new JsonWriterOptions
                {
                    Indented = false,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Default
                };
            }));
        ILogger logger = loggerFactory.CreateLogger("LoggerCategory");
        return logger;
    }
}