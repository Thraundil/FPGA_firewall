using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SME;

namespace simplePackageFilter
{

// ****************************************************************************

    public static class Shared_functions
    {
        public static bool DoesConnectExist(byte[] source, byte[] dest, int port,IFixedArray<byte> incoming_source,
                                            IFixedArray<byte> incoming_dest, int incoming_port)
        {
            bool doesItMatch = false;

            uint source_uint          = ByteArrayToUint.convert(source);
            uint dest_uint            = ByteArrayToUint.convert(dest);
            uint incoming_source_uint = ByteArrayToUint.convertIFixed(incoming_source);
            uint incoming_dest_uint   = ByteArrayToUint.convertIFixed(incoming_dest);

            if (((source_uint == incoming_source_uint) && (dest_uint == incoming_dest_uint)) && (port == incoming_port)) {
                doesItMatch = true;
            }

            return doesItMatch;
        }

        // Compares with known "source/dest" pairs in STATE
        public static bool ipv4_checker(byte[] source, byte[] dest, IFixedArray<byte> incoming_source,
                                        IFixedArray<byte> incoming_dest)
        {
            bool doesItMatch = false;

            uint source_uint          = ByteArrayToUint.convert(source);
            uint dest_uint            = ByteArrayToUint.convert(dest);
            uint incoming_source_uint = ByteArrayToUint.convertIFixed(incoming_source);
            uint incoming_dest_uint   = ByteArrayToUint.convertIFixed(incoming_dest);

            if ((source_uint == incoming_source_uint) && (dest_uint == incoming_dest_uint)) {
                doesItMatch = true;
            }

            return doesItMatch;
        }
    }


// ****************************************************************************

    public class Connection_process_IPV4_incoming : SimpleProcess
    {

        // Incoming IPV4
        [InputBus]
        public IBus_IPv4_In ipv4_in;

        // Input bus to check if we need to update state
        [InputBus]
        private readonly IBus_Update_State update = Scope.CreateOrLoadBus<IBus_Update_State>();

        [OutputBus]
        public IBus_Process_Verdict_IPV4 ruleVerdict = Scope.CreateBus<IBus_Process_Verdict_IPV4>();

        [OutputBus]
        public IBus_Connection_In_Use in_use = Scope.CreateOrLoadBus<IBus_Connection_In_Use>();

        // Variable declerations for the constructor
        private byte[] Ip_source { get; set; }
        private byte[] Ip_dest { get; set; }
        private readonly int my_id;

        private bool connection_in_use;
        public Connection_process_IPV4_incoming(byte[] ip_source_in, byte[] ip_dest_in, int ids, IBus_IPv4_In ipv4)
        {
            Ip_source = ip_source_in;
            Ip_dest   = ip_dest_in;
            my_id     = ids;
            ipv4_in   = ipv4;
        }

        protected override void OnTick()
        {
            // Set all the potential flags
            ruleVerdict.IsSet_ipv4    = ipv4_in.ClockCheck;
            ruleVerdict.Accepted_ipv4 = false;

            if (update.Flag && update.Id == my_id)
            {
                // overwrite it settings here
                connection_in_use = true;
                in_use.Id = my_id;
                in_use.In_use = true;

                // Update the state process with incoming information
                for (int k = 0; k<4; k++)
                {
                    Ip_source[k] = update.SourceIP[k];
                    Ip_dest[k]   = update.DestIP[k];
                }
            }

            if (connection_in_use)
            {
                if(ipv4_in.ClockCheck)
                {
                    if(Shared_functions.ipv4_checker(Ip_source, Ip_dest, ipv4_in.SourceIP, ipv4_in.DestIP))
                    {
                        ruleVerdict.Accepted_ipv4 = true;
                    }
                }
            }
        }
    }

// ****************************************************************************

    [ClockedProcess] // ClockedProcess due to having a COUNTER (timer)
    public class Connection_process_TCP_incoming : SimpleProcess
    {

        // Input but from the stateful
        [InputBus]
        public IBus_ITCP_In stateful_in;

        // Input bus to check if we need to update state
        [InputBus]
        private readonly IBus_Update_State update = Scope.CreateOrLoadBus<IBus_Update_State>();

        [OutputBus]
        public IBus_Process_Verdict_TCP ruleVerdict = Scope.CreateBus<IBus_Process_Verdict_TCP>();

        [OutputBus]
        public IBus_Connection_In_Use in_use = Scope.CreateOrLoadBus<IBus_Connection_In_Use>();

        // Variables for the constructor
        private byte[] Ip_source { get; set; }
        private byte[] Ip_dest { get; set; }
        private int Port_in { get; set; }

        private readonly int my_id;

        private int timeout_counter = 10000;
        private bool connection_in_use;

        public Connection_process_TCP_incoming(byte[] ip_source_in, byte[] ip_dest_in, int port, int ids, IBus_ITCP_In stateful)
        {
            Port_in     = port;
            Ip_source   = ip_source_in;
            Ip_dest     = ip_dest_in;
            my_id       = ids;
            stateful_in = stateful;
        }

        protected override void OnTick()
        {
            // Set all the potential flags
            ruleVerdict.IsSet_state = stateful_in.ThatOneVariableThatSaysIfWeAreDone;
            ruleVerdict.Accepted_state = false;

            if (update.Flag && update.Id == my_id)
            {
                // overwrite it settings here
                connection_in_use = true;
                in_use.Id = my_id;
                in_use.In_use = true;
                
                // Update the state process with incoming information
                for (int k = 0; k<4; k++)
                {
                    Ip_source[k] = update.SourceIP[k];
                    Ip_dest[k]   = update.DestIP[k];
                }
                // Updates port
                Port_in = update.Port;

            }

            if (connection_in_use)
            {
                if (timeout_counter == 0)
                {
                    connection_in_use = false;
                    in_use.Id = my_id;
                    in_use.In_use = false;
                }

                // Needs to go through all the flags
                if (stateful_in.ThatOneVariableThatSaysIfWeAreDone)
                {
                    if (Shared_functions.DoesConnectExist(Ip_source, Ip_dest, Port_in, stateful_in.SourceIP, stateful_in.DestIP, stateful_in.Port))
                    {
                        // check state for potential flags as well!!
                        ruleVerdict.Accepted_state = true;
                    }
                }
                timeout_counter -= 1;
            }
        }
    }


// ****************************************************************************

    public class Connection_process_outgoing : SimpleProcess
    {

        [InputBus]
        public IBus_Blacklist_out dataOut;

        // Input bus to check if we need to update state
        [InputBus]
        private readonly IBus_Update_State update = Scope.CreateOrLoadBus<IBus_Update_State>();

        [OutputBus]
        public IBus_Process_Verdict_Outgoing ruleVerdict = Scope.CreateBus<IBus_Process_Verdict_Outgoing>();

        [OutputBus]
        public IBus_Connection_In_Use in_use = Scope.CreateOrLoadBus<IBus_Connection_In_Use>();

        private byte[] Ip_source { get; set; }
        private byte[] Ip_dest { get; set; }
        private int Port_in { get; set; }

        private readonly int my_id;

        private bool connection_in_use;
        public Connection_process_outgoing(byte[] ip_source_in, byte[] ip_dest_in, int port, int ids, IBus_Blacklist_out data_Out)

        {
            Port_in = port;
            Ip_source = ip_source_in;
            Ip_dest = ip_dest_in;
            my_id = ids;
            dataOut = data_Out;
        }


        protected override void OnTick()
        {
            // Set all the potential flags

            ruleVerdict.IsSet_outgoing = dataOut.ReadyToWorkFlag;
            ruleVerdict.Accepted_outgoing = false;


            if (update.Flag && update.Id == my_id)
            {
                // overwrite it settings here
                connection_in_use = true;
                in_use.Id = my_id;
                in_use.In_use = true;

                // Update the state process with incoming information
                for (int k = 0; k<4; k++)
                {
                    Ip_source[k] = update.SourceIP[k];
                    Ip_dest[k]   = update.DestIP[k];
                }
                // Updates port
                Port_in = update.Port;
            }

            if (connection_in_use && dataOut.ReadyToWorkFlag)
            {
                if (Shared_functions.DoesConnectExist(Ip_source, Ip_dest, Port_in, dataOut.SourceIP, dataOut.DestIP, dataOut.SourcePort))
                {
                    ruleVerdict.Accepted_outgoing = true;
                }
            }
        }
    }
}
