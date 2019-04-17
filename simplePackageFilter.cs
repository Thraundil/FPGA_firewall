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
    public interface ITemp_name : IBus
    {
        bool good_or_bad { get; set; }

        bool ready_or_not { get; set; }
    }

    [TopLevelOutputBus]
    public interface IProcess_bools : IBus
    {
        bool accept_or_deny { get; set; }

        bool Valid { get; set; }
    }


    public class Final_check : SimpleProcess
    {
        [InputBus]
        public ITemp_name yes_or_no0;

        [InputBus]
        public ITemp_name yes_or_no1;

        [InputBus]
        public ITemp_name yes_or_no2;

        [OutputBus]
        public IProcess_bools final_say = Scope.CreateBus<IProcess_bools>();

        public Final_check(ITemp_name busIn0, ITemp_name busIn1, ITemp_name busIn2)
        {
            yes_or_no0 = busIn0;
            yes_or_no1 = busIn1;
            yes_or_no2 = busIn2;
        }

        // Deicdes to throw away or keep the package
        protected override void OnTick()
        {
            final_say.Valid = false;
            final_say.accept_or_deny = false;
            if (yes_or_no0.ready_or_not)
            {
                final_say.Valid = true;
                if (yes_or_no0.good_or_bad)
                {
                    final_say.accept_or_deny = true;
                    Console.WriteLine("Accept");
                }
                else if (yes_or_no1.good_or_bad)
                {
                    Console.WriteLine("Accept");
                    final_say.accept_or_deny = true;
                }
                else if (yes_or_no2.good_or_bad)
                {
                    final_say.accept_or_deny = true;
                    Console.WriteLine("Accept");
                }
                else
                {
                    Console.WriteLine("Denied");
                    final_say.accept_or_deny = false;
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
        public ITemp_name procArray = Scope.CreateBus<ITemp_name>();

        // Int list[4] to compare IP Source/Destination
        private readonly int[] allowed_SourceIP = new int[4];
        //private int[] allowed_ports = new int[2];
        //        private int[] allowed_DestinationIP = new int[4];

        private readonly int my_id = new int();

        // ipv4Reader_Constructor
        public ipv4Reader(IPv4_Simple busIn1, int[] SourceIP, int id)
        {
            ipv4 = busIn1;
            allowed_SourceIP = SourceIP;
            my_id = id;
            //allowed_ports = ports;
        }

        // int x is needed, as VHDL does not allow function calls without an argument...?
        private void sourceCompareIpv4(int[] x)
        {
            if (ipv4.SourceIP[0] == x[0])
            {
                procArray.good_or_bad = true;
                procArray.ready_or_not = true;
                //Console.WriteLine("The packet was accepted");
            }
        }


        // On Tick (ipv4Readers 'main')
        protected override void OnTick()
        {
            procArray.good_or_bad = false;
            procArray.ready_or_not = false;
            if (ipv4.clockCheck)
            {
                sourceCompareIpv4(allowed_SourceIP);
                //sourceComparePort(allowed_ports);
            }
            else
            {
                procArray.ready_or_not = false;
            }
        }
    }

    // ****************************************************************************

    public class Make_arrays
    {
        public string[] len_array;
        public string[] int_to_array(int len)
        {
            for (int i = 0; i < len; i++)
            {
                len_array[i] = "rules" + i.ToString();
            }
            return len_array;
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

                // Creates 3 classes, each with their own uses
                var rules = new Rules();
                var print = new Print();
                var byte_input = new inputSimulator();

                int len_rules = rules.accepted_sources.Length;

                var process_array = new Make_arrays();

                foreach (string my_array in process_array.int_to_array(len_rules))
                {

                    new ipv4Reader(byte_input.ipv4,rules.ip_str_to_int_array(rules.accepted_sources[0]),0);
                }

                

                // Prints a file, for testing purposes
                // print.print_file(rules.accepted_sources);

                // start one process for each rule
                // Make an array of the rule names and then use a for each over the array creating 
                // a process from each name in the array
                var ipv4Read0 = new ipv4Reader(byte_input.ipv4,
                                                  rules.ip_str_to_int_array(rules.accepted_sources[0]),
                                                  0);
                var ipv4Read1 = new ipv4Reader(byte_input.ipv4,
                                  rules.ip_str_to_int_array(rules.accepted_sources[1]),
                                  1);
                var ipv4Read2 = new ipv4Reader(byte_input.ipv4,
                                  rules.ip_str_to_int_array(rules.accepted_sources[1]),
                                  2);

                var final_say = new Final_check(ipv4Read0.procArray, ipv4Read1.procArray, ipv4Read2.procArray);
                sim.Run();
            }
        }
    }
}
