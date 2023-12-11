using OTools.SiIntegrator;

var src = new SiDataSource(2);

src.StartWatching(Environment.CurrentDirectory);
src.StartRead();

Console.WriteLine("Done");

while (true) { }