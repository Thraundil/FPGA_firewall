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
        IFixedArray<byte> DestIP { get; set; }

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
                // Checks if any rule process returns TRUE.
                my_bool = busList.Any(val => val.Accepted);
                final_say.Valid = true;

                // Accept the incoming package
                if (my_bool)
                {
                    final_say.Accept_or_deny = true;
                    Console.WriteLine("The package was Accepted");
                }
                // Deny the incoming package, as the IP was not on the whitelist.
                else
                {
                    Console.WriteLine("The package was Denied");
                    final_say.Accept_or_deny = false;
                }
                my_bool = false;
            }
        }
    }

    // ****************************************************************************

    public class Rule_Process : SimpleProcess
    {
        [InputBus]
        public IBus_IPv4 ipv4;

        [OutputBus]
        public IBus_ruleVerdict ruleVerdict = Scope.CreateBus<IBus_ruleVerdict>();

        // IP source range low/high as a LONG
        private readonly long ip_low_source = new long();
        private readonly long ip_high_source = new long();

        // IP destination range low/high as a LONG
        private readonly long ip_low_dest = new long();
        private readonly long ip_high_dest = new long();

        // ipv4Reader_Constructor
        public Rule_Process(IBus_IPv4 busIn, long ip_low_source_in, long ip_high_source_in, long ip_low_dest_in, long ip_high_dest_in)
        {
            ipv4 = busIn;
            ip_low_source  = ip_low_source_in;
            ip_high_source = ip_high_source_in;
            ip_low_dest    = ip_low_dest_in;
            ip_high_dest   = ip_high_dest_in;

        }

        // An argument is needed, as VHDL does not allow function calls without an argument...?!
        private void isIPinRange(long low_source, long high_source, long low_dest, long high_dest)
        {
            // Converts the received SOURCE IP into a long for comparison
            long doubl = (65536);    // 256*256
            long triple = (16777216); // 256*256*256
            long ipv4_source = ipv4.SourceIP[3] + (ipv4.SourceIP[2] * 256) + (ipv4.SourceIP[1] * doubl) + (ipv4.SourceIP[0] * triple);
            long ipv4_dest = ipv4.DestIP[3] + (ipv4.DestIP[2] * 256) + (ipv4.DestIP[1] * doubl) + (ipv4.DestIP[0] * triple);

            // Compares a given IP range with the received Source IP
            if (low_source <= ipv4_source && ipv4_source <= high_source)
            {
                if (low_dest <= ipv4_dest && ipv4_dest <= high_dest){
                    // The received packet's Source IP was accepted, as it was
                    // inside the accepted IP ranges of a specific rule.
                    ruleVerdict.Accepted = true;
                }
            }
        }


        // On Tick (ipv4Readers 'main')
        protected override void OnTick()
        {
            ruleVerdict.Accepted = false;
            ruleVerdict.IsSet = false;
            if (ipv4.ClockCheck)
            {
                isIPinRange(ip_low_source, ip_high_source, ip_low_dest, ip_high_dest);
                ruleVerdict.IsSet = true;
                //sourceComparePort(allowed_ports);
            }
        }
    }
    // ****************************************************************************
}

