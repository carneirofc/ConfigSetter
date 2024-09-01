using ConfigSetter.Actions;
using ConfigSetter.Logging;
using ConfigSetter.Model;
using Microsoft.Extensions.Logging;


namespace ConfigSetter.Test.Actions;

public class UpdateConfigActionTest
{
    private readonly ILogger _logger;
    public UpdateConfigActionTest()
    {
        var loggerFactory = new LoggerFactory().AddSimpleConsole(LogLevel.Debug, LogLevel.Warning);
        _logger = loggerFactory.CreateLogger<UpdateConfigAction>();
    }

    [Fact]
    async public Task TestUpdateConfig()
    {

        var action = new UpdateConfigAction(_logger);
        var tmpDir = Path.GetTempPath();
        var output = new TempFile(Path.Combine(tmpDir, "output.yaml"));
        var result = await action.Execute(new UpdateConfigParameters
        {
            Configuration = new FileInfo("Resources/config.yaml"),
            InputSettings = new FileInfo("Resources/settings.yaml"),
            OutputFile = output.FileInfo,
            Prefix = "DEV"
        });

        var outputContent = File.ReadAllText(output.FileInfo.FullName);
        Assert.Equal(0, result);
        Assert.Contains(@"resources:
  requests: {}
  limits:
    cpu: 15m
    memory: 128", outputContent);
    }

    [Fact]
    public void TestExceptionIfNotExists()
    {
        var tmpDir = Path.GetTempPath();
        var config = new TempFile(Path.Combine(tmpDir, "config.yaml"));
        var settings = new TempFile(Path.Combine(tmpDir, "settings.yaml"));

        var action = new UpdateConfigAction(_logger);
        Assert.ThrowsAsync<ArgumentException>(() => action.Execute(new UpdateConfigParameters
        {
            Configuration = new FileInfo(Path.Combine(tmpDir, "nonexistent.yaml")),
            InputSettings = settings.FileInfo
        }));
    }
}