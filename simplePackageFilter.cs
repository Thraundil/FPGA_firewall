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

// ****************************************************************************

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

// ****************************************************************************

    public class ipv4Reader : SimpleProcess{

        [InputBus]
        public IPv4_Simple ipv4;

// TODO: Find a way to get the IP inputs into a LIST/Array, instead of
// some stupid int variables.
//        private readonly int[] allowed_SourceIP; = new int[4];
//        IFixedArray<int> allowed_SourceIP;

        private int ip0 = 0;
        private int ip1 = 0;
        private int ip2 = 0;
        private int ip3 = 0;
        int TEST = 130;

        // ipv4Reader_Constructor
        public ipv4Reader(IPv4_Simple busIn, int[] SourceIP)
        {
           ipv4 = busIn;
           ip0 = SourceIP[0];
           ip1 = SourceIP[1];
           ip2 = SourceIP[2];
           ip3 = SourceIP[3];
//           allowed_SourceIP = SourceIP;
        }

        // int x is needed, as VHDL does not allow function calls without an argument...?
        private void localFunction(int x) {

            if (ipv4.SourceIP[0] == TEST){
            Console.WriteLine("PARTY TIME");
            }
        }

        // On Tick (ipv4Readers 'main')
        protected override void OnTick()
        {
            if (ipv4.clockCheck) {
                localFunction(5);
            }
        }
    }

// ****************************************************************************

    public class IP_Rules {
        public int[] convert_to_int_list(string ip){
            int[] ip_array  = new int[4];
            ip_array        = ip.Split('.').Select(Int32.Parse).ToArray();
            return ip_array;
        }
    }

// ****************************************************************************

    public class Print {
        public void print_int_array(int[] int_array){
            for (int i=0; i < int_array.Length; i++) {
                Console.Write(int_array[i] + " ");
            }
            Console.WriteLine("");
        }
    }

// ****************************************************************************

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

                // Reads which IP sources are allowed
                string[] strInput = File.ReadAllLines("../../IP_rules.txt");
                var rules = new IP_Rules();
                var print = new Print();
                var byte_input_stream  = new inputSimulator();

                for (int i=0; i < strInput.Length; i++) {
                    print.print_int_array(rules.convert_to_int_list(strInput[i]));
                }

                var ipv4Read = new ipv4Reader(byte_input_stream.ipv4,rules.convert_to_int_list(strInput[0]));

                sim.Run();
            }
        }
    }
}
