using OTools.SiIntegrator;

var src = new SiDataSource(5);

src.StartWatching(Environment.CurrentDirectory);
src.StartRead();

Console.WriteLine("Done");

while (true) { }