using OTools.Common;
using System.Diagnostics;

namespace OTools.SiIntegrator;

public class SiDataSource : ISiDataSource
{
    private uint _comPort;
    private FileSystemWatcher _watcher;

    public event EventHandler<SiCard>? SiCardRead;

    public SiDataSource(uint? comPort)
    {
        _comPort = comPort ?? 0;
    }

    public void StartRead()
    {
        Process p = new()
        {
            StartInfo = new("OTools.SiBackend.exe"/*, $"read {_comPort}"*/)
            {
                UseShellExecute = true,
                CreateNoWindow = false,
                WindowStyle = ProcessWindowStyle.Normal,
            }
        };
        p.Exited += (sender, e) => Console.WriteLine("Exited");

        p.Start();
        p.WaitForExitAsync();

        //SiBackend.Program.Main(new[] { "read", _comPort.ToString() });
    }

    public void StartWatching(string filePath)
    {
        _watcher = new()
        {
            Path = filePath,

            NotifyFilter = NotifyFilters.LastWrite,
            Filter = "*.card"
        };

        _watcher.Changed += _watcher_Changed;

        _watcher.EnableRaisingEvents = true;
    }

    private void _watcher_Changed(object sender, FileSystemEventArgs e)
    {
        string xml = File.ReadAllText(e.FullPath);
        XMLDocument doc = XMLDocument.Deserialize(xml);

        SiCard card = SiCard.Parse(doc);
        Console.WriteLine(card.Siid);
        
        SiCardRead?.Invoke(this, card);
    }
}

public interface ISiDataSource
{
    public event EventHandler<SiCard>? SiCardRead;
}