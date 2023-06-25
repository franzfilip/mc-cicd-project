using ConsumerSimulator;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

public class Program
{
    public static void Main(string[] args)
    {
        Simulator simulator = new Simulator(5);

        simulator.StartSimulation();

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
}
