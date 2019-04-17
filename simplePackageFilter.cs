using SME;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Net;

namespace simplePackageFilter
{
    [TopLevelInputBus]
    public interface Bus_IPv4 : IBus
    {
        [FixedArrayLength(4)]
        IFixedArray<byte> SourceIP { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> DestinationIP { get; set; }

        [InitialValue(false)]
        bool clockCheck { get; set; }
    }

    [TopLevelInputBus]
    public interface Bus_ruleVerdict : IBus
    {
        [InitialValue(false)]
        bool accepted { get; set; }

        [InitialValue(false)]
        bool isSet { get; set; }
    }

    [TopLevelOutputBus]
    public interface Bus_finalVerdict : IBus
    {
        bool accept_or_deny { get; set; }

        bool Valid { get; set; }
    }


// ****************************************************************************

    public class testing : SimpleProcess
        {
        [InputBus]
        public Bus_ruleVerdict[] busList;

        [OutputBus]
        public Bus_finalVerdict final_say = Scope.CreateBus<Bus_finalVerdict>();

        // Constructor         
        public testing(Bus_ruleVerdict[] busList_in)
        {
                busList = busList_in;
        }

        // Test
        public bool resultTest(){
            return busList[0].accepted;
        }

        // OnTick()
        protected override void OnTick()
        {
            final_say.Valid = false;
            final_say.accept_or_deny = false;
            bool my_bool = busList[0].accepted;
            Console.WriteLine(my_bool);
        }
    }

// ****************************************************************************

//    public class Final_check : SimpleProcess
//        {
//        [InputBus]
//        public Bus_ruleVerdict yes_or_no0;
//
//        [InputBus]
//        public Bus_ruleVerdict yes_or_no1;
//
//        [InputBus]
//        public Bus_ruleVerdict yes_or_no2;
//
//        [OutputBus]
//        public Bus_finalVerdict final_say = Scope.CreateBus<Bus_finalVerdict>();
//
//        public Final_check(Bus_ruleVerdict busIn0, Bus_ruleVerdict busIn1, Bus_ruleVerdict busIn2)
//        {
//            yes_or_no0 = busIn0;
//            yes_or_no1 = busIn1;
//            yes_or_no2 = busIn2;
//        }
//
//        // Deicdes to throw away or keep the package
//        protected override void OnTick()
//        {
//            final_say.Valid = false;
//            final_say.accept_or_deny = false;
//            if (yes_or_no0.isSet)
//            {
//                final_say.Valid = true;
//                if (yes_or_no0.accepted)
//                {
//                    final_say.accept_or_deny = true;
//                    Console.WriteLine("Accept");
//                }
//                else if (yes_or_no1.accepted)
//                {
//                    Console.WriteLine("Accept");
//                    final_say.accept_or_deny = true;
//                }
//                else if (yes_or_no2.accepted)
//                {
//                    final_say.accept_or_deny = true;
//                    Console.WriteLine("Accept");
//                }
//                else
//                {
//                    Console.WriteLine("Denied");
//                    final_say.accept_or_deny = false;
//                }
//            }
//
//        }
//    }

// ****************************************************************************
    public class ipv4Reader : SimpleProcess
    {
        [InputBus]
        public Bus_IPv4 ipv4;

        [OutputBus]
        public Bus_ruleVerdict ruleVerdict = Scope.CreateBus<Bus_ruleVerdict>();

        // Int list[4] to compare IP Source/Destination
        private readonly int[] allowed_SourceIP = new int[4];
        //private int[] allowed_ports = new int[2];
        //        private int[] allowed_DestinationIP = new int[4];

        private readonly int my_id = new int();

        // ipv4Reader_Constructor
        public ipv4Reader(Bus_IPv4 busIn, int[] SourceIP, int id)
        {
            ipv4             = busIn;
            allowed_SourceIP = SourceIP;
            my_id            = id;
        }

        // An argument is needed, as VHDL does not allow function calls without an argument...?!
        private void sourceCompareIpv4(int[] allowed_SourceIP)
        {
            if (ipv4.SourceIP[0] == allowed_SourceIP[0])
            {
                ruleVerdict.accepted = true;
                ruleVerdict.isSet    = true;
                Console.WriteLine("The packet was accepted");
            }
            else
            {
                Console.WriteLine("The packet was NOT accepted");
            }
        }


        // On Tick (ipv4Readers 'main')
        protected override void OnTick()
        {
            ruleVerdict.accepted = false;
            ruleVerdict.isSet    = false;
            if (ipv4.clockCheck)
            {
                sourceCompareIpv4(allowed_SourceIP);
                //sourceComparePort(allowed_ports);
            }
            else
            {
                ruleVerdict.isSet = false;
            }
        }
    }

// ****************************************************************************

//    public class Make_arrays
//    {
//        public string[] len_array;
//        public string[] int_to_array(int len)
//        {
//            for (int i = 0; i < len; i++)
//            {
//                len_array[i] = "rules" + i.ToString();
//            }
//            return len_array;
//        }
//    }

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

                // Number of 'sources' rules
                int len_rules = rules.accepted_sources.Length;

                // Bus array for each rule to write a bus to
                Bus_ruleVerdict[] newnew_array = new Bus_ruleVerdict[len_rules];

                // The bus loop, in which the above array is filled
                for (int i = 0; i < len_rules; i++)
                {
                    var temptemp = new ipv4Reader(byte_input.ipv4, rules.ip_str_to_int_array(rules.accepted_sources[i]), 0);
                    newnew_array[i] = temptemp.ruleVerdict;
                }

                // TEST
                var teststuff = new testing(newnew_array);


                // Prints a file, for testing purposes
                // print.print_file(rules.accepted_sources);

                // start one process for each rule
                // Make an array of the rule names and then use a for each over the array creating 
                // a process from each name in the array
//                var ipv4Read0 = new ipv4Reader(byte_input.ipv4,
//                                                  rules.ip_str_to_int_array(rules.accepted_sources[0]),
//                                                  0);
//                var ipv4Read1 = new ipv4Reader(byte_input.ipv4,
//                                  rules.ip_str_to_int_array(rules.accepted_sources[1]),
//                                  1);
//                var ipv4Read2 = new ipv4Reader(byte_input.ipv4,
//                                 rules.ip_str_to_int_array(rules.accepted_sources[1]),
//                                  2);

                //var final_say = new Final_check(newnew_array);
                sim.Run();
            }
        }
    }
}
