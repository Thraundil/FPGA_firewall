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
        bool clockCheck { get; set; }
    }

    [TopLevelInputBus]
    public interface ITCP : IBus
    {
        [FixedArrayLength(2)]
        IFixedArray<byte> Source_Port { get; set; }

        [FixedArrayLength(2)]
        IFixedArray<byte> Dest_Port { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> Sequence_number { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> Ack_number { get; set; }
        // data_offset + reserved + control flags
        [FixedArrayLength(2)]
        IFixedArray<byte> junk { get; set; }

        [FixedArrayLength(2)]
        IFixedArray<byte> Window_size { get; set; }

        [FixedArrayLength(2)]
        IFixedArray<byte> Checksum { get; set; }

        [FixedArrayLength(2)]
        IFixedArray<byte> Urgent_pointer { get; set; }

        [FixedArrayLength(40)]
        IFixedArray<byte> Optional_data { get; set; }
    }

    // ****************************************************************************

    public class ipv4Reader : SimpleProcess
    {
        [InputBus]
        public IPv4_Simple ipv4;

        [InputBus]
        public ITCP tcp;
        //[InputBus, OutputBus]
        //public ITCP TCP;

        // Int list[4] to compare IP Source/Destination
        private int[] allowed_SourceIP = new int[4];
        //private int[] allowed_ports = new int[2];
        //        private int[] allowed_DestinationIP = new int[4];

        // ipv4Reader_Constructor
        public ipv4Reader(IPv4_Simple busIn, ITCP busTcp, int[] SourceIP)
        {
            ipv4 = busIn;
            tcp = busTcp;
            allowed_SourceIP = SourceIP;
            //allowed_ports = ports;
        }

        // int x is needed, as VHDL does not allow function calls without an argument...?
        private void sourceCompareIpv4(int[] x)
        {
            if (ipv4.SourceIP[0] == x[0])

            {
                Console.WriteLine("Ipv4 TIME");
            }
        }
        // private void sourceComparePort(int x)
        //{
        //    if (TCP.Dest_Port[0] == x[0])
        //
        //    {
        //        Console.WriteLine("Port TIME");
        //    }
        //}


        // On Tick (ipv4Readers 'main')
        protected override void OnTick()
        {

            if (ipv4.clockCheck)
            {
                sourceCompareIpv4(allowed_SourceIP);
                //sourceComparePort(allowed_ports);
            }
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
                var rules = new Rules();
                // Print class
                var print = new Print();

                var byte_input_stream = new inputSimulator();

                for (int i = 0; i < rules.accepted_sources.Length; i++)
                {
                    print.print_int_array(rules.ip_str_to_int_array(rules.accepted_sources[i]));
                }

                var ipv4Read = new ipv4Reader(byte_input_stream.ipv4, byte_input_stream.TCP, rules.ip_str_to_int_array(rules.accepted_sources[0]));

                sim.Run();
            }
        }
    }
}
