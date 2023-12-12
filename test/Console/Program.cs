using Newtonsoft.Json;
using OTools.StartTimeDistributor;
using Sunley.Mathematics;
using System.Text.Json.Serialization;

var json = JsonConvert.DeserializeObject<StartTimeParameters>(File.ReadAllText(@"C:\Users\Joe\Downloads\starttimeoptions.json"));

Console.ReadLine();