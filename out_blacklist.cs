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
        bool ReadyToWorkFlag { get; set; }
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
    public interface IBus_blacklist_finalVerdict_out : IBus
    {
        [InitialValue(false)]
        bool Accept_or_deny { get; set; }
        [InitialValue(false)]
        bool Valid { get; set; }
    }


    // ****************************************************************************


    public class Final_check_Blacklist : SimpleProcess
    {
        [InputBus]
        public IBus_blacklist_ruleVerdict_out[] busList;

        [OutputBus]
        public readonly IBus_blacklist_finalVerdict_out final_say = Scope.CreateOrLoadBus<IBus_blacklist_finalVerdict_out>();

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
                    Console.WriteLine("The package was Accepted");
                }
                // Deny the incoming package, as the IP was not on the whitelist.
                else
                {
                    Console.WriteLine("The package was Denied");
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

        // IP source range low/high as a byte array
        private byte[] dest_low { get; set; }
        private byte[] dest_high { get; set; }


        // ipv4Reader_Constructor
        public Rule_Process_Blacklist(IBus_Blacklist_out busIn, byte[] ip_dest_low, byte[] ip_dest_high)
        {
            blacklist_out = busIn;
            dest_low = ip_dest_low;
            dest_high = ip_dest_high;
        }

        private void IP_Match(byte[] dest_low, byte[] dest_high)
        {

            int x = 0;
            bool doesItMatch = true;

            while (x < dest_low.Length) {

                if (dest_low[x] == blacklist_out.DestIP[x] || dest_high[x] == blacklist_out.DestIP[x]){
                    x++;
                }
                else{
                    if ((dest_low[x] > blacklist_out.DestIP[x]) || (dest_high[x] < blacklist_out.DestIP[x])){
                        doesItMatch = false;
                        x = dest_low.Length;
                    }
                }
            }

            if (doesItMatch){
                ruleVerdict.Accepted = true;
            }
        }


        // On Tick (ipv4Readers 'main')
        protected override void OnTick()
        {
            ruleVerdict.Accepted = false;
            ruleVerdict.IsSet = false;
            if (blacklist_out.ReadyToWorkFlag)
            {
                IP_Match(dest_low, dest_high);
                ruleVerdict.IsSet = true;
                //sourceComparePort(allowed_ports);
            }
        }
    }
}
