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

        [FixedArrayLength(2)]
        IFixedArray<byte> Length { get; set; }

        [FixedArrayLength(2)]
        IFixedArray<byte> Id { get; set; }

        [FixedArrayLength(2)]
        IFixedArray<byte> Flags { get; set; }

        byte Ttl { get; set; }

        byte Protocol { get; set; }

        [FixedArrayLength(2)]
        IFixedArray<byte> Checksum { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> SourceIP { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> DestinationIP { get; set; }

        [InitialValue(false)]
        bool clockCheck {get; set; }
    }

    // Input class, which reads simple IPv4 bytes
    public class inputSimulator : SimulationProcess
    {
        [OutputBus]
        public IPv4_Simple ipv4 = Scope.CreateBus<IPv4_Simple>();

        // Used to read input from a .txt file
        byte[] sample;

        public async override System.Threading.Tasks.Task Run()
        {
            sample = File.ReadAllBytes("../../ipv4_bytes.txt");

            var stream = new MemoryStream(sample, 0, sample.Length);
            var reader = new BinaryReader(stream);

            // Reads a single byte each clockcykle, and
            // updates the Toplevel Inputbus IPv4_Simple
            ipv4.Header = reader.ReadByte();
            await ClockAsync();
            ipv4.Diff = reader.ReadByte();
            await ClockAsync();
            for (int i = 0; i < 2; i++) {
                ipv4.Length[i] = reader.ReadByte();
                await ClockAsync();
            }
            for (int i = 0; i < 2; i++) {
                ipv4.Id[i] = reader.ReadByte();
                await ClockAsync();
            }
            for (int i = 0; i < 2; i++) {
                ipv4.Flags[i] = reader.ReadByte();
                await ClockAsync();
            }
            ipv4.Ttl = reader.ReadByte();
            await ClockAsync();
            ipv4.Protocol = reader.ReadByte();
            await ClockAsync();
            for (int i = 0; i < 2; i++) {
                ipv4.Checksum[i] = reader.ReadByte();
                await ClockAsync();
            }
            for (int i = 0; i < 4; i++) {
                ipv4.SourceIP[i] = reader.ReadByte();
                await ClockAsync();
            }
            for (int i = 0; i < 4; i++) {
                ipv4.DestinationIP[i] = reader.ReadByte();
                await ClockAsync();
            }
            ipv4.clockCheck = true;
        }
    }

    public class ipv4Reader : SimpleProcess{

        [InputBus]
        public IPv4_Simple ipv4;

        public ipv4Reader(IPv4_Simple busIn)
        {
           ipv4 = busIn;
        }


        // int x is needed, as VHDL does not allow function calls without an argument...?
        private void localFunction(int x) {

            if (ipv4.SourceIP[0] == 133){
            Console.WriteLine(5);
            }

            // Console.WriteLine("{0} {1} {2} {3}", ipv4.SourceIP[0], ipv4.SourceIP[1], ipv4.SourceIP[2], ipv4.SourceIP[3]);

        }

        // On Tick (ipv4Readers 'main')
        protected override void OnTick()
        {
            if (ipv4.clockCheck) {
                localFunction(5);
            }
        }
    }


    public class IP_Rules {
        public int[] read_rules(){

            string[] strInput = File.ReadAllLines("../../IP_rules.txt");

            for (int i = 0; i < strInput.Length; i++) {
                Console.WriteLine(strInput[i]);
            }

            int[] ip_rules_array = strInput[0].Split('.').Select(Int32.Parse).ToArray();

            return ip_rules_array;
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

                var rules = new IP_Rules();
                int[] ip_rules_array = rules.read_rules();

                var some     = new inputSimulator();
                var ipv4Read = new ipv4Reader(some.ipv4);


                sim.Run();
            }
        }
    }
}
