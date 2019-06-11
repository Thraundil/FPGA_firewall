using SME;
using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;

namespace simplePackageFilter
{
    [ClockedProcess]
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

        private void IP_Match(byte[] dest_low, byte[] dest_high, IFixedArray<byte> outgoing_IP)
        {
            uint dest_low_uint    = ByteArrayToUint.convert(dest_low);
            uint dest_high_uint   = ByteArrayToUint.convert(dest_high);
            uint outgoing_IP_uint = ByteArrayToUint.convertIFixed(outgoing_IP);

            // if TRUE, it means that the given IP was in the blacklist range, and must be BLOCKED
            if ((dest_low_uint <= outgoing_IP_uint) && (outgoing_IP_uint <= dest_high_uint)){
                ruleVerdict.Accepted = true;
            } else {
                ruleVerdict.Accepted = false;
            }
        }


        // On Tick (ipv4Readers 'main')
        protected override void OnTick()
        {
            if (blacklist_out.ReadyToWorkFlag)
            {
                IP_Match(dest_low, dest_high, blacklist_out.DestIP);
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
