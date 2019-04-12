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

        [FixedArrayLength(4)]
        IFixedArray<byte> SourceIP { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> DestinationIP { get; set; }

        [InitialValue(false)]
        bool clockCheck { get; set; }
    }
    [TopLevelInputBus]
    public interface Itemp_name : IBus
    {
        [FixedArrayLength(2)]
        IFixedArray<bool> good_or_bad { get; set; }
    }

    [TopLevelOutputBus]
    public interface Iprocess_bools : IBus
    {
        bool accept_or_deny { get; set; }
    }

    public class final_check : SimpleProcess
    {
        [InputBus]
        public Itemp_name yes_or_no;

        [OutputBus]
        public Iprocess_bools final_say;

        public final_check(Itemp_name busIn)
        {
            yes_or_no = busIn;
        }

        // Deicdes to throw away or keep the package
        private bool keep_track = false;
        protected override void OnTick()
        {
            keep_track = false;
            for(int i = 0; i < yes_or_no.good_or_bad.Length; i++)
            {
                Console.WriteLine(yes_or_no.good_or_bad[i]);
                if(yes_or_no.good_or_bad[i])
                {
                    keep_track = true;
                    Console.WriteLine("TRUE DAT");
                    break;
                }
            }
        }
    }

    // ****************************************************************************
    public class ipv4Reader : SimpleProcess
    {
        [InputBus]
        public IPv4_Simple ipv4;

        [OutputBus]
        public Itemp_name TCP = Scope.CreateBus<Itemp_name>();

        // Int list[4] to compare IP Source/Destination
        private readonly int[] allowed_SourceIP = new int[4];
        //private int[] allowed_ports = new int[2];
        //        private int[] allowed_DestinationIP = new int[4];

        private readonly int my_id = new int();

        // ipv4Reader_Constructor
        public ipv4Reader(IPv4_Simple busIn, int[] SourceIP, int id)
        {
            ipv4 = busIn;
            allowed_SourceIP = SourceIP;
            my_id = id;
            //allowed_ports = ports;
        }

        // int x is needed, as VHDL does not allow function calls without an argument...?
        private void sourceCompareIpv4(int[] x)
        {
            if (ipv4.SourceIP[0] == x[0])
            {
                TCP.good_or_bad[my_id] = true;
                Console.WriteLine("The packet was accepted");
            }
            else
            {
                TCP.good_or_bad[my_id] = false;
                Console.WriteLine("The packet was denied");
            }
        }


        // On Tick (ipv4Readers 'main')
        protected override void OnTick()
        {
            TCP.good_or_bad[my_id] = false;
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

                // Creates 3 classes, each with their own uses
                var rules = new Rules();
                var print = new Print();
                var byte_input = new inputSimulator();

                // Prints a file, for testing purposes
                // print.print_file(rules.accepted_sources);

                // start one process for each rule
                for (int i = 0; i < (rules.accepted_sources).Length; i++)
                {
                    Console.WriteLine(i);
                    var ipv4Read = new ipv4Reader(byte_input.ipv4,
                                                  rules.ip_str_to_int_array(rules.accepted_sources[i]),
                                                  i);
                    var final_say = new final_check(ipv4Read.TCP);
                }
                sim.Run();
            }
        }
    }
}
