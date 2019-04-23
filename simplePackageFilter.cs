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
        bool ClockCheck { get; set; }
    }

    [TopLevelInputBus]
    public interface IBus_ruleVerdict : IBus
    {
        [InitialValue(false)]
        bool Accepted { get; set; }

        [InitialValue(false)]
        bool IsSet { get; set; }
    }

    [TopLevelOutputBus]
    public interface IBus_finalVerdict : IBus
    {
        bool Accept_or_deny { get; set; }

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
            final_say.Accept_or_deny = false;
            if (busList[0].IsSet)
            {
                for (int i = 0; i < busList.Length; i++)
                {
                    if (busList[i].Accepted)
                        my_bool = true;
                }

                final_say.Valid = true;
                if (my_bool)
                {
                    final_say.Accept_or_deny = true;
                    Console.WriteLine("The package was Accepted");
                }
                else
                {
                    Console.WriteLine("Access Denied");
                    final_say.Accept_or_deny = false;
                }
                my_bool = false;
            }
        }
    }

    // ****************************************************************************

    public class Ipv4Reader : SimpleProcess
    {
        [InputBus]
        public IBus_IPv4 ipv4;

        [OutputBus]
        public IBus_ruleVerdict ruleVerdict = Scope.CreateBus<IBus_ruleVerdict>();

        // Int list[4] to compare IP Source/Destination
        private readonly byte[] ip_low = new byte[4];
        private readonly byte[] ip_high = new byte[4];

        // ipv4Reader_Constructor
        public Ipv4Reader(IBus_IPv4 busIn, byte[] ip_low_in, byte[] ip_high_in)
        {
            ipv4 = busIn;
            ip_low = ip_low_in;
            ip_high = ip_high_in;
        }

        // An argument is needed, as VHDL does not allow function calls without an argument...?!
        private void SourceCompareIpv4(byte[] low, byte[] high)
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
                ruleVerdict.Accepted = true;
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
            ruleVerdict.Accepted = false;
            ruleVerdict.IsSet = false;
            if (ipv4.ClockCheck)
            {
                SourceCompareIpv4(ip_low, ip_high);
                ruleVerdict.IsSet = true;
                //sourceComparePort(allowed_ports);
            }
        }
    }

    // ****************************************************************************


    // Main
    public class Program
    {
        static void Main()
        {

            // General classes, compiled before simulation
            var rules = new Rules();
            _ = new Print();

            // Number of rules, 
            int len_sources = rules.accepted_sources.Length;
            _ = rules.accepted_destinations.Length;

            using (var sim = new Simulation())
            {
                sim
                    .BuildCSVFile();
                //                    .BuildVHDL();

                // Creates 3 classes, each with their own uses
                var byte_input = new InputSimulator();


                // Bus array for each rule to write a bus to
                IBus_ruleVerdict[] newnew_array = new IBus_ruleVerdict[len_sources];

                // The bus loop, in which the above array is filled
                for (int i = 0; i < len_sources; i++)
                {
                    var (low, high) = rules.Get_sources(i);
                    var temptemp = new Ipv4Reader(byte_input.ipv4, low, high);
                    newnew_array[i] = temptemp.ruleVerdict;
                }

                // TEST
                var teststuff = new Final_check(newnew_array);


                // Prints a file, for testing purposes
                // print.print_file(rules.accepted_sources);
                sim.Run();
            }
        }
    }
}
