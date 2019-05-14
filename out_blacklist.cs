using SME;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simplePackageFilter
{
    [TopLevelInputBus]
    public interface IBus_Blacklist_out : IBus
    {
        [FixedArrayLength(4)]
        IFixedArray<byte> SourceIP { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> DestIP { get; set; }

        int SourcePort { get; set; }

        byte Flags { get; set; }

        [InitialValue(false)]
        bool ThatOneVariableThatSaysIfWeAreDone { get; set; }
    }

    [TopLevelInputBus]
    public interface IBus_blacklist_ruleVerdict_out : IBus
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
        public IBus_blacklist_ruleVerdict_out[] busList;

        [OutputBus]
        public IBus_finalVerdict_out final_say = Scope.CreateBus<IBus_finalVerdict_out>();

        // Class Variables
        bool my_bool = false;

        // Constructor         
        public Final_check_Blacklist(IBus_blacklist_ruleVerdict_out[] busList_out)
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
                if (!my_bool)
                {
                    final_say.Accept_or_deny = true;
                    Console.WriteLine("The outgoing package was NOT in the blacklist");
                }
                // Deny the incoming package, as the IP was not on the whitelist.
                else
                {
                    Console.WriteLine("The outgoing package was dropped (part of Blacklist)");
                    final_say.Accept_or_deny = false;
                }
                my_bool = true;
            }
        }
    }

    // ****************************************************************************

    public class Rule_Process_Blacklist : SimpleProcess
    {
        [InputBus]
        public IBus_Blacklist_out blacklist_out;

        [OutputBus]
        public IBus_blacklist_ruleVerdict_out ruleVerdict = Scope.CreateBus<IBus_blacklist_ruleVerdict_out>();

        // IP source range low/high as a LONG
        private long dest_low { get; set; }
        private long dest_high { get; set; }

        // IP destination range low/high as a LONG


        // ipv4Reader_Constructor
        public Rule_Process_Blacklist(IBus_Blacklist_out busIn, long ip_dest_low, long ip_dest_high)
        {
            blacklist_out = busIn;
            dest_low = ip_dest_low;
            dest_high = ip_dest_high;
        }

        private void IP_Match(long dest_low, long dest_high)
        {
            // Converts the received SOURCE IP into a long for comparison
            long doubl = (65536);    // 256*256
            long triple = (16777216); // 256*256*256
            long ipv4_dest = blacklist_out.DestIP[3] + (blacklist_out.DestIP[2] * 256) + (blacklist_out.DestIP[1] * doubl) + (blacklist_out.DestIP[0] * triple);

            // Compares a given IP range with the received Source IP
            if (dest_low <= ipv4_dest || dest_high <= ipv4_dest)
            {
                    // The received packet's Source IP was accepted, as it was
                    // inside the accepted IP ranges of a specific rule.
                    ruleVerdict.Accepted = true;
            }
        }


        // On Tick (ipv4Readers 'main')
        protected override void OnTick()
        {
            ruleVerdict.Accepted = false;
            ruleVerdict.IsSet = false;
            if (blacklist_out.ThatOneVariableThatSaysIfWeAreDone)
            {
                IP_Match(dest_low, dest_high);
                ruleVerdict.IsSet = true;
                //sourceComparePort(allowed_ports);
            }
        }
    }
}
