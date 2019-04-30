using SME;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simplePackageFilter
{
    [TopLevelInputBus]
    public interface IBus_IPv4_out : IBus
    {
        [FixedArrayLength(4)]
        IFixedArray<byte> SourceIP { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> DestIP { get; set; }

        int SourcePort { get; set; }

        int DestPort { get; set; }

        [InitialValue(false)]
        bool flag_readyOrNot { get; set; }
    }

    [TopLevelInputBus]
    public interface IBus_ruleVerdict_out : IBus
    {
        [InitialValue(false)]
        bool Accepted { get; set; }

        [InitialValue(false)]
        bool IsSet { get; set; }
    }

    [TopLevelOutputBus]
    public interface IBus_finalVerdict_out : IBus
    {
        bool Accept_or_deny { get; set; }

        bool Valid { get; set; }
    }


    // ****************************************************************************


    public class Final_check_Blacklist : SimpleProcess
    {
        [InputBus]
        public IBus_ruleVerdict_out[] busList;

        [OutputBus]
        public IBus_finalVerdict_out final_say = Scope.CreateBus<IBus_finalVerdict_out>();

        // Class Variables
        bool my_bool = false;

        // Constructor         
        public Final_check_Blacklist(IBus_ruleVerdict_out[] busList_out)
        {
            busList = busList_out;
        }

        protected override void OnTick()
        {
            final_say.Valid = false;
            final_say.Accept_or_deny = false;
            if (busList[0].IsSet)
            {
                // Checks if any rule process returns TRUE.
                //my_bool = busList.Any(val => val.Accepted);
                my_bool = busList.AsParallel().Any(val => val.Accepted);
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

    public class Rule_Process_Blacklist : SimpleProcess
    {
        [InputBus]
        public IBus_IPv4_out ipv4_out;

        [OutputBus]
        public IBus_ruleVerdict_out ruleVerdict = Scope.CreateBus<IBus_ruleVerdict_out>();

        // IP source range low/high as a LONG
        private readonly long ip_low_source = new long();
        private readonly long ip_high_source = new long();

        // IP destination range low/high as a LONG
        private readonly long ip_low_dest = new long();
        private readonly long ip_high_dest = new long();

        // ipv4Reader_Constructor
        public Rule_Process_Blacklist(IBus_IPv4_out busIn, long ip_low_source_in, long ip_high_source_in, long ip_low_dest_in, long ip_high_dest_in)
        {
            ipv4_out = busIn;
            ip_low_source = ip_low_source_in;
            ip_high_source = ip_high_source_in;
            ip_low_dest = ip_low_dest_in;
            ip_high_dest = ip_high_dest_in;

        }

        // An argument is needed, as VHDL does not allow function calls without an argument...?!
        private void IsIPinRange(long low_source, long high_source, long low_dest, long high_dest)
        {
            // Converts the received SOURCE IP into a long for comparison
            long doubl = (65536);    // 256*256
            long triple = (16777216); // 256*256*256
            long ipv4_source = ipv4_out.SourceIP[3] + (ipv4_out.SourceIP[2] * 256) + (ipv4_out.SourceIP[1] * doubl) + (ipv4_out.SourceIP[0] * triple);
            long ipv4_dest = ipv4_out.DestIP[3] + (ipv4_out.DestIP[2] * 256) + (ipv4_out.DestIP[1] * doubl) + (ipv4_out.DestIP[0] * triple);

            // Compares a given IP range with the received Source IP
            if (low_source >= ipv4_source || ipv4_source >= high_source)
            {
                if (low_dest >= ipv4_dest || ipv4_dest >= high_dest)
                {
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
            if (ipv4_out.flag_readyOrNot)
            {
                IsIPinRange(ip_low_source, ip_high_source, ip_low_dest, ip_high_dest);
                ruleVerdict.IsSet = true;
                //sourceComparePort(allowed_ports);
            }
        }
    }
}
