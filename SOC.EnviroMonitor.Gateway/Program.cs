using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Newtonsoft.Json;
using System.IO.Ports;
namespace SOC.Arduino;

internal class Program
{
    static void Main()
    {
        Monitor monitor = new();
        monitor.Start();
        Thread.Sleep(Timeout.Infinite);
    }
}

public class Monitor()
{
    private SerialPort? serialPort;
    private InfluxDBClient? client;
    private readonly string bucket = "soc";
    private readonly string org = "sensori";
    private readonly string token = "fYXVexX096Drh2q6f3CY4foEYpNrTpb0COmBuCi7ZImoVfLWnHjp03zLFh47AOpIHe05b_U-zUZIm8y_2GA6yA==";

    public void Start()
    {
        var ports = SerialPort.GetPortNames();
        if (ports != null && ports.Length > 0)
        {
            serialPort = new(ports[0], 9600);
            serialPort.Open();
            serialPort.DataReceived += DataReceived;
        }
        client = new InfluxDBClient("http://10.80.1.15:8086", token);
    }

    private void DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        var serialPort = (SerialPort)sender;
        var json = serialPort.ReadLine();
        Console.WriteLine(json);

        try
        {
            Data? data = JsonConvert.DeserializeObject<Data>(json);
            var point = PointData
              .Measurement("Arduino")
              .Tag("Device", "Arduino1")
              .Field("Temperature", data?.Temperature)
              .Field("Humidity", data?.Humidity)
              .Field("Pir", data?.Pir)
              .Field("Brightness", data?.Brightness)
              .Field("Mic", data?.Mic)
              .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

            using (var writeApi = client.GetWriteApi())
            {
                writeApi.WritePoint(point, bucket, org);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: " + ex.Message);
        }
    }
}

public class Data
{
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public int Pir { get; set; }
    public int Brightness { get; set; }
    public int Mic { get; set; }
}