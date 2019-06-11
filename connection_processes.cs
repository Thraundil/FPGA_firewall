using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SME;

namespace simplePackageFilter
{
    [ClockedProcess]
    public class Connection_process_IPV4_incoming : SimpleProcess
    {

        // Incoming IPV4
        [InputBus]
        public IBus_IPv4_In ipv4_in;

        // Input bus to check if we need to update state
        [InputBus]
        private readonly IBus_Update_State_tcp update_tcp = Scope.CreateOrLoadBus<IBus_Update_State_tcp>();

        [InputBus]
        private readonly IBus_Update_State_out update_out = Scope.CreateOrLoadBus<IBus_Update_State_out>();

        [OutputBus]
        public IBus_Process_Verdict_IPV4 ruleVerdict = Scope.CreateBus<IBus_Process_Verdict_IPV4>();

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

            if (update_tcp.Flag && update_tcp.Id == my_id)
            {
                // overwrite it settings here
                connection_in_use = update_tcp.set_in_use;

                // Update the state process with incoming information
                for (int k = 0; k<4; k++)
                {
                    Ip_source[k] = update_tcp.SourceIP[k];
                    Ip_dest[k]   = update_tcp.DestIP[k];
                }
            }

            if (update_out.Flag && update_out.Id == my_id)
            {
                // overwrite it settings here
                connection_in_use = update_out.set_in_use;

                // Update the state process with incoming information
                for (int k = 0; k < 4; k++)
                {
                    Ip_source[k] = update_out.SourceIP[k];
                    Ip_dest[k] = update_out.DestIP[k];
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
        private readonly IBus_Update_State_tcp update_tcp = Scope.CreateOrLoadBus<IBus_Update_State_tcp>();

        [InputBus]
        private readonly IBus_Update_State_out update_out = Scope.CreateOrLoadBus<IBus_Update_State_out>();

        [OutputBus]
        public IBus_Process_Verdict_TCP ruleVerdict = Scope.CreateBus<IBus_Process_Verdict_TCP>();

        [OutputBus]
        public IBus_Connection_In_Use in_use = Scope.CreateBus<IBus_Connection_In_Use>();

        // Variables for the constructor
        byte[] Ip_source { get; set; }
        byte[] Ip_dest { get; set; }
        int Port_in { get; set; }

        private readonly int my_id;

        private int timeout_counter = 10000;
        private bool connection_in_use = false;

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

            if (update_tcp.Flag && update_tcp.Id == my_id)
            {
                // overwrite it settings here
                connection_in_use = update_tcp.set_in_use;
                in_use.Id = my_id;
                in_use.In_use = update_tcp.set_in_use;

                // Update the state process with incoming information
                for (int k = 0; k < 4; k++)
                {
                    Ip_source[k] = update_tcp.SourceIP[k];
                    Ip_dest[k] = update_tcp.DestIP[k];
                }
                Port_in = update_tcp.Port;
                //Console.WriteLine("tcp_in");
                //Console.WriteLine("{0} {1} {2} {3}", Ip_source[0], Ip_source[1], Ip_source[2], Ip_source[3]);
                //Console.WriteLine("{0} {1} {2} {3}", Ip_dest[0], Ip_dest[1], Ip_dest[2], Ip_dest[3]);
                //Console.WriteLine("{0}", stateful_in.Port);
            }

            if (update_out.Flag && update_out.Id == my_id)
            {
                // overwrite it settings here
                connection_in_use = update_out.set_in_use;
                in_use.Id = my_id;
                in_use.In_use = update_out.set_in_use;

                // Update the state process with incoming information
                for (int k = 0; k < 4; k++)
                {
                    Ip_source[k] = update_out.SourceIP[k];
                    Ip_dest[k] = update_out.DestIP[k];
                }
                Port_in = update_out.Port;
                //Console.WriteLine("outgoing");
                //Console.WriteLine("{0} {1} {2} {3}", Ip_source[0], Ip_source[1], Ip_source[2], Ip_source[3]);
                //Console.WriteLine("{0} {1} {2} {3}", Ip_dest[0], Ip_dest[1], Ip_dest[2], Ip_dest[3]);
                //Console.WriteLine("{0}", stateful_in.Port);
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
                        ruleVerdict.Accepted_state = true;
                    }
                }
                timeout_counter -= 1;
            }
        }
    }


// ****************************************************************************

    [ClockedProcess]
    public class Connection_process_outgoing : SimpleProcess
    {

        [InputBus]
        public IBus_Blacklist_out dataOut;

        [InputBus]
        private readonly IBus_Update_State_tcp update_tcp = Scope.CreateOrLoadBus<IBus_Update_State_tcp>();

        [InputBus]
        private readonly IBus_Update_State_out update_out = Scope.CreateOrLoadBus<IBus_Update_State_out>();

        [OutputBus]
        public IBus_Process_Verdict_Outgoing ruleVerdict = Scope.CreateBus<IBus_Process_Verdict_Outgoing>();

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


            if (update_tcp.Flag && update_tcp.Id == my_id)
            {
                // overwrite it settings here
                connection_in_use = update_tcp.set_in_use;

                // Update the state process with incoming information
                for (int k = 0; k < 4; k++)
                {
                    Ip_source[k] = update_tcp.SourceIP[k];
                    Ip_dest[k] = update_tcp.DestIP[k];
                }
                Port_in = update_tcp.Port;
            }

            if (update_out.Flag && update_out.Id == my_id)
            {
                //overwrite its settings here
                connection_in_use = update_out.set_in_use;

                //Update the state process with incoming information
                for (int k = 0; k < 4; k++)
                {
                    Ip_source[k] = update_out.SourceIP[k];
                    Ip_dest[k] = update_out.DestIP[k];
                }
                Port_in = update_out.Port;
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
