using SME;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Net;

namespace simplePackageFilter
{
    [TopLevelInputBus]
    public interface IBus_IPv4 : IBus
    {
        [FixedArrayLength(4)]
        IFixedArray<byte> SourceIP { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> DestinationIP { get; set; }

        [InitialValue(false)]
        bool clockCheck { get; set; }
    }

    [TopLevelInputBus]
    public interface IBus_ruleVerdict : IBus
    {
        [InitialValue(false)]
        bool accepted { get; set; }

        [InitialValue(false)]
        bool isSet { get; set; }
    }

    [TopLevelOutputBus]
    public interface IBus_finalVerdict : IBus
    {
        bool accept_or_deny { get; set; }

        bool Valid { get; set; }
    }


    // ****************************************************************************


    public class Final_check : SimpleProcess
    {
        [InputBus]
        public IBus_ruleVerdict[] busList;

        [OutputBus]
        public IBus_finalVerdict final_say = Scope.CreateBus<IBus_finalVerdict>();

        // Class Variables
        bool my_bool = false;

        // Constructor         
        public Final_check(IBus_ruleVerdict[] busList_in)
        {
            busList = busList_in;
        }

        protected override void OnTick()
        {
            final_say.Valid = false;
            final_say.accept_or_deny = false;
            if (busList[1].isSet)
            {
                for (int i = 0; i < busList.Length; i++)
                {
                    if (busList[i].accepted)
                        my_bool = true;
                }

                final_say.Valid = true;
                if (my_bool)
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
        public IBus_IPv4 ipv4;

        [OutputBus]
        public IBus_ruleVerdict ruleVerdict = Scope.CreateBus<IBus_ruleVerdict>();

        // Int list[4] to compare IP Source/Destination
        private readonly byte[] ip_low = new byte[4];
        private readonly byte[] ip_high = new byte[4];

        private readonly int my_id = new int();

        // ipv4Reader_Constructor
        public ipv4Reader(IBus_IPv4 busIn, byte[] ip_low_in, byte[] ip_high_in, int id)
        {
            ipv4 = busIn;
            ip_low = ip_low_in;
            ip_high = ip_high_in;
            my_id = id;
        }

        // An argument is needed, as VHDL does not allow function calls without an argument...?!
        private void sourceCompareIpv4(byte[] low, byte[] high)
        {

            // Potential iFixedArray-to-byteArray
            //             byte[] fromIfixedArray = new byte[4];
            // 
            //             for(int i = 0; i < 4; i++) {
            //                 fromIfixedArray[i] = ipv4.SourceIP[i];
            //             }


            if ((low[0] <= ipv4.SourceIP[0] && ipv4.SourceIP[0] <= high[0]) &&
               (low[1] <= ipv4.SourceIP[1] && ipv4.SourceIP[1] <= high[1]) &&
               (low[2] <= ipv4.SourceIP[2] && ipv4.SourceIP[2] <= high[2]) &&
               (low[3] <= ipv4.SourceIP[3] && ipv4.SourceIP[3] <= high[3]))
            {
                ruleVerdict.accepted = true;
                // Console.WriteLine("The packet was accepted");
            }
            else
            {
                // Console.WriteLine("The packet was NOT accepted");
            }
        }


        // On Tick (ipv4Readers 'main')
        protected override void OnTick()
        {
            ruleVerdict.accepted = false;
            ruleVerdict.isSet = false;
            if (ipv4.clockCheck)
            {
                sourceCompareIpv4(ip_low, ip_high);
                ruleVerdict.isSet = true;
                //sourceComparePort(allowed_ports);
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

            // General classes, compiled before simulation
            var rules = new Rules();
            var print = new Print();

            // Number of rules, 
            int len_sources = rules.accepted_sources.Length;
            int len_destination = rules.accepted_destinations.Length;

            using (var sim = new Simulation())
            {
                sim
                    .BuildCSVFile();
                //                    .BuildVHDL();

                // Creates 3 classes, each with their own uses
                var byte_input = new inputSimulator();


                // Bus array for each rule to write a bus to
                IBus_ruleVerdict[] newnew_array = new IBus_ruleVerdict[len_sources];

                // The bus loop, in which the above array is filled
                for (int i = 0; i < len_sources; i++)
                {
                    var (low, high) = rules.get_sources(i);
                    var temptemp = new ipv4Reader(byte_input.ipv4, low, high, 0);
                    newnew_array[i] = temptemp.ruleVerdict;
                }

                // TEST
                var teststuff = new Final_check(newnew_array);


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
