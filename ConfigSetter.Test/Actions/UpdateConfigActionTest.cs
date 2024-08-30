using ConfigSetter.Actions;
using ConfigSetter.Model;
using Microsoft.Extensions.Logging;
namespace ConfigSetter.Test.Actions;

public class TempFile : IDisposable
{
    private bool disposedValue;

    public FileInfo FileInfo { get; }
    public TempFile(string path)
    {
        FileInfo = new FileInfo(path);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (FileInfo.Exists)
                {
                    FileInfo.Delete();
                }
            }
            disposedValue = true;
        }
    }

    ~TempFile()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

public class UpdateConfigActionTest
{
    private readonly ILogger _logger;
    public UpdateConfigActionTest()
    {
        _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<UpdateConfigAction>();
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