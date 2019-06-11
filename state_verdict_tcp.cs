using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SME;

namespace simplePackageFilter
{
    public class stateful_state_verdict : StateProcess
    {
        [InputBus]
        public IBus_Process_Verdict_TCP[] connection_list;

        [InputBus]
        public IBus_Rule_Verdict_TCP[] rule_list;

        [InputBus]
        public IBus_ITCP_In stateful_in;

        [InputBus]
        IBus_Connection_In_Use in_use; 

        [OutputBus]
        public IBus_finalVerdict_tcp_In final_say_tcp_in = Scope.CreateBus<IBus_finalVerdict_tcp_In>();

        [OutputBus]
        public IBus_Update_State_tcp update_tcp = Scope.CreateOrLoadBus<IBus_Update_State_tcp>();

        [OutputBus]
        public tcp_verdict_to_sim to_sim = Scope.CreateOrLoadBus<tcp_verdict_to_sim>();

        bool connection_bool_tcp = false;

        bool rule_bool = false;

        uint counter_id = 1;

        public stateful_state_verdict(IBus_Process_Verdict_TCP[] con_process, IBus_Rule_Verdict_TCP[] rule_process, IBus_ITCP_In tcp_data)
        {
            connection_list = con_process;
            rule_list = rule_process;
            stateful_in = tcp_data;
        } 


        protected override async Task OnTickAsync()
        {

            // reset the flags
            final_say_tcp_in.Valid = false; // valid is
            final_say_tcp_in.Accept_or_deny = false; // did we 
            connection_bool_tcp = false; // see if you are in the connection set
            rule_bool = false; // see if you are in the rule set
            to_sim.tcp_ready_flag = true; // We are ready to receive here! Right?
            update_tcp.Flag = false;
            // need a flag to say if we are ready to recieve the next packet from the group! :)


            // We wait to get some actual data to inspect
            while (!stateful_in.ThatOneVariableThatSaysIfWeAreDone)
                await ClockAsync();

            // tell the simulator to wait
            // until the rules and the connection processes have responded
            to_sim.tcp_ready_flag = false;

            while(!rule_list[0].tcp_IsSet || !connection_list[0].IsSet_state)
            {
                await ClockAsync();
            }

            // Check if the state processes found a match
            for (int i = 0; i < connection_list.Length; i++)
            {
                connection_bool_tcp |= connection_list[i].Accepted_state;
            }
            final_say_tcp_in.Valid = true;

            // Accept the incoming package
            if (connection_bool_tcp)
            {
                final_say_tcp_in.Accept_or_deny = true;
                Console.WriteLine("Incoming TCP:      Matches (connection)");
                // we do nothing if the connection already exist
            }
            // if the state processes did not find a match check the rules processes
            else
            {
                for (int i = 0; i < rule_list.Length; i++)
                {
                    rule_bool |= rule_list[i].tcp_Accepted;
                }
                if (rule_bool) // And some flags are correct - in parituclar the syn flag
                {
                    Console.WriteLine("Incoming TCP:      Matches (whitelist)");
                    final_say_tcp_in.Accept_or_deny = true;
                    update_tcp.Flag = true;
                    update_tcp.set_in_use = true;
                    update_tcp.SourceIP = stateful_in.SourceIP;
                    update_tcp.DestIP = stateful_in.DestIP;
                    update_tcp.Port = stateful_in.Port;
                    update_tcp.Id = counter_id; // This is not the correct way to do it
                    counter_id += 1;
                    //Console.WriteLine("{0} {1} {2} {3}", stateful_in.SourceIP[0], stateful_in.SourceIP[1], stateful_in.SourceIP[2], stateful_in.SourceIP[3]);
                    //Console.WriteLine("{0} {1} {2} {3}", stateful_in.DestIP[0], stateful_in.DestIP[1], stateful_in.DestIP[2], stateful_in.DestIP[3]);
                    //Console.WriteLine("{0}", stateful_in.Port);
                }
                else
                {
                    Console.WriteLine("Incoming TCP:      Blocked");
                    final_say_tcp_in.Accept_or_deny = false;
                }
            }
            connection_bool_tcp = false;
            rule_bool = false;
        }
    }
}
