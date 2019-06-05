using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SME;

namespace simplePackageFilter
{


    [ClockedProcess]
    public class Connection_process : SimpleProcess
    {

        //[InputBus]
        //private readonly IBus_Controller_to_state data = Scope.CreateOrLoadBus<IBus_Controller_to_state>();

        [InputBus]
        public IBus_Blacklist_out dataOut;

        // Input bus from the ipv4 header check
        [InputBus]
        public IBus_IPv4_In ipv4_in;

        // Input but from the stateful check
        [InputBus]
        public IBus_ITCP_In stateful_in;

        // Input bus to check if we need to update state
        [InputBus]
        private readonly IBus_Update_State update = Scope.CreateOrLoadBus<IBus_Update_State>();

        [OutputBus]
        public IBus_ITCP_RuleVerdict ruleVerdict = Scope.CreateBus<IBus_ITCP_RuleVerdict>();

        [OutputBus]
        public IBus_Connection_In_Use in_use = Scope.CreateOrLoadBus<IBus_Connection_In_Use>();

        private byte[] Ip_source { get; set; }
        private byte[] Ip_dest { get; set; }
        private int Port_in { get; set; }

        private readonly int my_id;

        private int timeout_counter = 10000;

        private bool connection_in_use;
        public Connection_process(byte[] ip_source_in, byte[] ip_dest_in, int port, int ids, IBus_ITCP_In stateful, IBus_IPv4_In ipv4, 
            IBus_Blacklist_out data_Out)

        {
            Port_in = port;
            Ip_source = ip_source_in;
            Ip_dest = ip_dest_in;
            my_id = ids;
            stateful_in = stateful;
            ipv4_in = ipv4;
            dataOut = data_Out;
        }

        // For comparing INCOMING 'TCP/IP' (Src, Dst, Port)
        private bool DoesConnectExist(byte[] source, byte[] dest, int port, IFixedArray<byte> incoming_source, IFixedArray<byte> incoming_dest, int incoming_port)
        {
            int x = 0;
            bool doesItMatch = true;

            while (x < source.Length) {
                // IMPROVEMENT: remove the port-compare, as it is needlesly compared 4 times, instead of one.
                if (source[x] == incoming_source[x] && dest[x] == incoming_dest[x] && port == incoming_port) {
                    x++;
                }
                else {
                    doesItMatch = false;
                    x = source.Length;
                }
            }

            if (doesItMatch) {
                timeout_counter = 10000;
            }

            return doesItMatch;
        }

        // Compares with known "source/dest" pais in STATE
        private bool ipv4_checker(byte[] source, byte[] dest, IFixedArray<byte> incoming_source, IFixedArray<byte> incoming_dest)
        {
            bool doesItMatch = false;

            uint source_uint          = ByteArrayToUint.convert(dest);
            uint dest_uint            = ByteArrayToUint.convert(source);
            uint incoming_source_uint = ByteArrayToUint.convertIFixed(incoming_source);
            uint incoming_dest_uint   = ByteArrayToUint.convertIFixed(incoming_dest);

            if ((source_uint == incoming_source_uint) && (dest_uint == incoming_dest_uint)) {
                doesItMatch = true;
            }

            return doesItMatch;
        }

        protected override void OnTick()
        {
            // Set all the potential flags
            ruleVerdict.IsSet_ipv4 = ipv4_in.ClockCheck;
            ruleVerdict.Accepted_ipv4 = false;
            ruleVerdict.IsSet_state = stateful_in.ThatOneVariableThatSaysIfWeAreDone;
            ruleVerdict.Accepted_state = false;
            ruleVerdict.IsSet_out = dataOut.ReadyToWorkFlag;
            ruleVerdict.Accepted_out = false;

            if (connection_in_use)
            {
                if (timeout_counter == 0)
                {
                    connection_in_use = false;
                    in_use.Id = my_id;
                    in_use.In_use = false;
                }
                if (update.Flag && update.Id == my_id)
                {
                    // overwrite it settings here
                    connection_in_use = true;
                    in_use.Id = my_id;
                    in_use.In_use = true;
                }
                if (update.Flag_2 && update.Id_2 == my_id)
                {
                    // overwrite its settings here
                    connection_in_use = true;
                    in_use.Id = my_id;
                    in_use.In_use = true;
                }

                if(ipv4_in.ClockCheck)
                {
                    if(ipv4_checker(Ip_source, Ip_dest, ipv4_in.SourceIP, ipv4_in.DestIP))
                    {
                        ruleVerdict.Accepted_ipv4 = true;
                    }
                }


                // Needs to go through all the flags
                if (stateful_in.ThatOneVariableThatSaysIfWeAreDone)
                {
                    if (DoesConnectExist(Ip_source, Ip_dest, Port_in, stateful_in.SourceIP, stateful_in.DestIP, stateful_in.Port))
                    {
                        // check state for potential flags as well!!
                        ruleVerdict.Accepted_state = true;
                    }
                }
                if(dataOut.ReadyToWorkFlag)
                {
                        if (DoesConnectExist(Ip_source, Ip_dest, Port_in, dataOut.SourceIP, dataOut.DestIP, dataOut.SourcePort))
                        {
                            ruleVerdict.Accepted_out = true;
                        }
                }
                timeout_counter -= 1;
            }
        }
    }
}
