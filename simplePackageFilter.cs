using SME;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Net;

namespace simplePackageFilter
{

    [TopLevelInputBus]
    public interface IPv4_Simple : IBus
    {
        byte Header { get; set; }

        byte Diff { get; set; }

        byte[] length { get; set; }

        byte[] id { get; set; }

        byte[] flags { get; set; }

        byte ttl { get; set; }

        byte protocol { get; set; }

        byte[] checksum { get; set; }

        byte[] SourceIP { get; set; }

        byte[] DestinationIP { get; set; }
    }

    // Input class, which reads simple IPv4 bytes
    public class inputSimulator : SimulationProcess
    {

        [InputBus]
        public IPv4_Simple ipv4;

        byte[] sample;

        public async override System.Threading.Tasks.Task Run()
        {
            sample = File.ReadAllBytes("C:/Users/patri/OneDrive/Dokumenter/GitHub/FPGA_firewall/ipv4_bytes.txt");
                
            //foreach (var item in sample)
            //{
            //    Console.WriteLine(item);
            //}

           // byte[] arr = { 0x45, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x82, 0xe2, 0xed, 0xad, 0x5b, 0xe0, 0xd3, 0x47 };
            var stream = new MemoryStream(sample, 0, sample.Length);
            var reader = new BinaryReader(stream);
            //Console.WriteLine(sample.Length);

            for (int i = 0; i < (sample.Length) / 20; i++)
            {
                // UPDATE IPv4_Simple INPUTBUS HERE
                Console.WriteLine("Version and header length {0}", reader.ReadByte());
                Console.WriteLine("Diff services {0}", reader.ReadByte());

                Console.WriteLine("Total length {0}", IPAddress.NetworkToHostOrder(reader.ReadInt16()));
                Console.WriteLine("ID {0}", IPAddress.NetworkToHostOrder(reader.ReadInt16()));
                Console.WriteLine("Flags and offset {0}", IPAddress.NetworkToHostOrder(reader.ReadInt16()));

                Console.WriteLine("TTL {0}", reader.ReadByte());
                Console.WriteLine("Protocol {0}", reader.ReadByte());
                Console.WriteLine("Checksum {0}", reader.ReadInt16());

                Console.WriteLine("Source IP {0}", new IPAddress(reader.ReadUInt32()));
                Console.WriteLine("Destination IP {0}", new IPAddress(reader.ReadUInt32()));

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
                sim
                    .BuildCSVFile()
                    .BuildVHDL();

                var some = new inputSimulator();

                sim.Run();
            }
        }
    }
}
