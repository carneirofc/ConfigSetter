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