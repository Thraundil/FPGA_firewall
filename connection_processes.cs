using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SME;

namespace simplePackageFilter
{
    [InputBus]
    public interface IBus_Connection_In_Use : IBus
    {
        bool In_use { get; set; }

        int Id { get; set; }
    }
    [InputBus]
    public interface IBus_Update_State : IBus
    {
        [FixedArrayLength(4)]
        IFixedArray<byte> SourceIP { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> DestIP { get; set; }

        int Port { get; set; }

        [InitialValue(false)]
        bool Flag { get; set; }

        int Id { get; set; }

        // We can need to update 2 entries in the state at the same time

        [FixedArrayLength(4)]
        IFixedArray<byte> SourceIP_2 { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> DestIP_2 { get; set; }

        int Port_2 { get; set; }
        [InitialValue(false)]
        bool Flag_2 { get; set; }
        
        int Id_2 { get; set; }
    }
    [TopLevelInputBus]
    public interface IBus_ITCP_In : IBus
    {
        [FixedArrayLength(4)]
        IFixedArray<byte> SourceIP { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> DestIP { get; set; }

        int Port { get; set; }

        byte Flags { get; set; }

        [InitialValue(false)]
        bool ThatOneVariableThatSaysIfWeAreDone { get; set; }


    }
    [TopLevelInputBus]
    public interface IBus_ITCP_RuleVerdict : IBus
    {
        [InitialValue(false)]
        bool Accepted_ipv4 { get; set; }

        [InitialValue(false)]
        bool IsSet_ipv4 { get; set; }

        [InitialValue(false)]
        bool Accepted_state { get; set; }

        [InitialValue(false)]
        bool IsSet_state { get; set; }

        [InitialValue(false)]
        bool Accepted_out { get; set; }

        [InitialValue(false)]
        bool IsSet_out { get; set; }

    }

    [ClockedProcess]
    public class Connection_process : SimpleProcess
    {

        //[InputBus]
        //private readonly IBus_Controller_to_state data = Scope.CreateOrLoadBus<IBus_Controller_to_state>();


        [InputBus]
        public IBus_blacklist_finalVerdict_out blacklist_input;

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

        private long Ip_source { get; set; }
        private long Ip_dest { get; set; }
        private int Port_in { get; set; }

        readonly long doubl = (65536);    // 256*256
        readonly long triple = (16777216); // 256*256*256

        private readonly int my_id;

        private int timeout_counter = 10000;

        private bool connection_in_use;
        public Connection_process(long ip_source_in, long ip_dest_in, int port, int ids, IBus_ITCP_In stateful, IBus_IPv4_In ipv4, 
            IBus_Blacklist_out data_Out, IBus_blacklist_finalVerdict_out blacklistinput)

        {
            Port_in = port;
            Ip_source = ip_source_in;
            Ip_dest = ip_dest_in;
            my_id = ids;
            stateful_in = stateful;
            ipv4_in = ipv4;
            dataOut = data_Out;
            blacklist_input = blacklistinput;
        }


        private bool DoesConnectExist(long source, long dest, int port, long tcp_source, long tcp_dest)
        {
            if (source == tcp_source && dest == tcp_dest && port == stateful_in.Port)
            {

                // The received packet's Source IP was accepted, as it was
                // inside the accepted IP ranges of a specific rule.
                timeout_counter = 10000;
                return true;
            }

            return false;
        }

        private bool ipv4_checker(long source, long dest)
        {
            long doubl = (65536);    // 256*256
            long triple = (16777216); // 256*256*256
            long ipv4_source = ipv4_in.SourceIP[3] + (ipv4_in.SourceIP[2] * 256) + (ipv4_in.SourceIP[1] * doubl) + (ipv4_in.SourceIP[0] * triple);
            long ipv4_dest = ipv4_in.DestIP[3] + (ipv4_in.DestIP[2] * 256) + (ipv4_in.DestIP[1] * doubl) + (ipv4_in.DestIP[0] * triple);
            if (source == ipv4_source && dest == ipv4_dest)
            {

                // The received packet's Source IP was accepted, as it was
                // inside the accepted IP ranges of a specific rule.
                timeout_counter = 10000;
                return true;
            }

            return false;
        }

        protected override void OnTick()
        {
            // Set all the potential flags
            ruleVerdict.IsSet_ipv4 = false;
            ruleVerdict.Accepted_ipv4 = false;
            ruleVerdict.IsSet_state = false;
            ruleVerdict.Accepted_state = false;
            ruleVerdict.IsSet_out = false;
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
                    if(ipv4_checker(Ip_source, Ip_dest))
                    {
                        ruleVerdict.Accepted_ipv4 = true;
                    }
                    ruleVerdict.IsSet_ipv4 = true;
                }


                // Needs to go through all the flags
                if (stateful_in.ThatOneVariableThatSaysIfWeAreDone)
                {
                    long tcp_source = stateful_in.SourceIP[3] + (stateful_in.SourceIP[2] * 256) + (stateful_in.SourceIP[1] * doubl) + (stateful_in.SourceIP[0] * triple);
                    long tcp_dest = stateful_in.DestIP[3] + (stateful_in.DestIP[2] * 256) + (stateful_in.DestIP[1] * doubl) + (stateful_in.DestIP[0] * triple);
                    if (DoesConnectExist(Ip_source, Ip_dest, Port_in, tcp_source, tcp_dest))
                    {
                        // check state for potential flags as well!!
                        ruleVerdict.Accepted_state = true;
                    }
                    ruleVerdict.IsSet_state = true;
                }
                timeout_counter = -1;
            }
        }
    }
}
