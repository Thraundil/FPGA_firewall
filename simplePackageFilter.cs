using SME;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace simplePackageFilter
{

    [TopLevelInputBus]
    public interface IPv4_Simple : IBus {   
        byte Header { get; set; }

        byte[] SourceIP { get; set; }

        byte[] DestinationIP { get; set; }
    }

    // Input class, which reads simple IPv4 bytes
    public class inputSimulator : SimulationProcess {

        [InputBus]
        public IPv4_Simple ipv4;

        short[] sample;

        public async override System.Threading.Tasks.Task Run() {
            sample = File.ReadAllLines("ipv4_example.txt")
                .Select(line => short.Parse(line))
                .ToArray();

            foreach (short s in sample) {

                // UPDATE IPv4_Simple INPUTBUS HERE

                await ClockAsync();
            }
        }
    }



    // Main
    public class Program
    {
        static void Main(string[] args)
        {
            using (var sim = new Simulation())
            {
//                sim
//                  .BuildCSVFile()
//                  .BuildVHDL();

                byte[] arr = { 0, 48, 136, 21, 69, 131, 0, 24, 231, 253, 174, 161, 8, 0, 69, 0, 0, 52, 2, 31, 64, 0, 128, 6, 230, 22, 212, 25, 99, 74, 202, 177, 16, 121, 194, 156, 0, 119, 160, 128, 75, 200, 249, 141, 210, 78, 80, 24, 64, 252, 130, 182, 0, 0, 65, 82, 84, 73, 67, 76, 69, 32, 51, 52, 13, 10 };
                var stream = new MemoryStream(arr, 0, arr.Length);
                var reader = new BinaryReader(stream);

                Console.WriteLine("Version and header length {0}", reader.ReadByte());
                Console.WriteLine("Diff services{0}", reader.ReadByte());

                Console.WriteLine("Total length {0}", IPAddress.NetworkToHostOrder(reader.ReadInt16()));
                Console.WriteLine("ID {0}", IPAddress.NetworkToHostOrder(reader.ReadInt16()));
                Console.WriteLine("Flags and offset {0}", IPAddress.NetworkToHostOrder(reader.ReadInt16()));

                Console.WriteLine("TTL {0}", reader.ReadByte());
                Console.WriteLine("Protocol {0}", reader.ReadByte());
                Console.WriteLine("Checksum {0}", reader.ReadInt16());

                Console.WriteLine("Source IP {0}", new IPAddress((int)reader.ReadInt32()));
                Console.WriteLine("Destination IP {0}", new IPAddress((int)reader.ReadInt32()));

//                var programSimulator = new programPinSim();
//                var keypadSimulator  = new inputSimulator(programSimulator.keyControl);
//
//                var control = new controlProcess(keypadSimulator.keyPressed, keypadSimulator.keyControl);

//                sim.Run();
            }
        }
    }
}





