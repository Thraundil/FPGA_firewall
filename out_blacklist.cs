using SME;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simplePackageFilter
{
    public class Final_check_Blacklist : SimpleProcess
    {
        [InputBus]
        public IBus_blacklist_ruleVerdict_out[] busList;

        [OutputBus]
        public readonly IBus_blacklist_finalVerdict_out final_say = Scope.CreateBus<IBus_blacklist_finalVerdict_out>();

        // Class Variables
        bool my_bool = false;

        // Constructor         
        public Final_check_Blacklist(IBus_blacklist_ruleVerdict_out[] busList_out)
        {
            busList = busList_out;
        }

        protected override void OnTick()
        {
            if (busList[0].IsSet)
            {
                final_say.Valid = true;
                // Checks if any rule process returns TRUE.
                // my_bool = busList.Any(val => val.Accepted);
                for (int i = 0; i > busList.Length; i++)
                {
                    my_bool |= busList[i].Accepted;
                }

                // Accept the incoming package
                if (!my_bool)
                {
                    final_say.Accept_or_deny = true;
                    Console.WriteLine("The blacklist Accepted the package");
                }
                // Deny the incoming package, as the IP was not on the whitelist.
                else
                {
                    Console.WriteLine("The blacklist denied the package");
                    final_say.Accept_or_deny = false;
                }
                my_bool = true;
            }
            else
            {
                final_say.Valid = false;
                final_say.Accept_or_deny = false;
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
                    else{
                        doesItMatch = true;
                        x++;
                    }
                }
            }

            // if 'doesItMatch', it means that the given IP was in the blacklist range
            if (doesItMatch){
                // This indicates that it IS on the blacklist and should be BLOCKED
                ruleVerdict.Accepted = true;
            }
            else
            {
                ruleVerdict.Accepted = false;
            }
        }


        // On Tick (ipv4Readers 'main')
        protected override void OnTick()
        {
            if (blacklist_out.ReadyToWorkFlag)
            {
                IP_Match(dest_low, dest_high);
                ruleVerdict.IsSet = true;
                //sourceComparePort(allowed_ports);
            }
            else
            {
                ruleVerdict.Accepted = false;
                ruleVerdict.IsSet = false;
            }
        }
    }
}
