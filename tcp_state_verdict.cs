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

        [OutputBus]
        public IBus_finalVerdict_tcp_In final_say_tcp_in = Scope.CreateBus<IBus_finalVerdict_tcp_In>();

        [OutputBus]
        private readonly IBus_Update_State update = Scope.CreateOrLoadBus<IBus_Update_State>();

        [OutputBus]
        public tcp_verdict_to_sim to_sim = Scope.CreateOrLoadBus<tcp_verdict_to_sim>();

        bool connection_bool = false;

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
            connection_bool = false; // see if you are in the connection set
            rule_bool = false; // see if you are in the rule set
            to_sim.tcp_ready_flag = true; // We are ready to receive here! Right?
            update.Flag = false;
            // need a flag to say if we are ready to recieve the next packet from the group! :)


            // We wait to get some actual data to inspect
            while (!stateful_in.ThatOneVariableThatSaysIfWeAreDone)
                await ClockAsync();

            // tell the simulator to wait
            // until the rules and the connection processes have responded
            to_sim.tcp_ready_flag = false;

            while (!rule_list[0].tcp_IsSet)
            {
                await ClockAsync();
            }

            // we wait for both of them before progressing
            while (!connection_list[0].IsSet_state)
            {
                await ClockAsync();
            }
            // we have gotten a msg from everybody and are ready to work

            // Check if the state processes found a match
            for (int i = 0; i < connection_list.Length; i++)
            {
                connection_bool |= connection_list[i].Accepted_state;
            }
            final_say_tcp_in.Valid = true;

            // Accept the incoming package
            if (connection_bool)
            {
                final_say_tcp_in.Accept_or_deny = true;
                Console.WriteLine("The tcp connection already exists");
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
                    Console.WriteLine("The tcp connection does not exist but matches a whitelisted rule");
                    final_say_tcp_in.Accept_or_deny = true;
                    update.Flag = true;
                    update.SourceIP = stateful_in.SourceIP;
                    update.DestIP = stateful_in.DestIP;
                    update.Port = stateful_in.Port;
                    update.Id = counter_id; // This is not the correct way to do it
                    counter_id += 1; 
                    //Console.WriteLine("{0} {1} {2} {3}", stateful_in.SourceIP[0], stateful_in.SourceIP[1], stateful_in.SourceIP[2], stateful_in.SourceIP[3]);
                    //Console.WriteLine("{0} {1} {2} {3}", stateful_in.DestIP[0], stateful_in.DestIP[1], stateful_in.DestIP[2], stateful_in.DestIP[3]);
                    //Console.WriteLine("{0}", stateful_in.Port);
                }
                else
                {
                    Console.WriteLine("The tcp connection does neither match a connection or a rule");
                    final_say_tcp_in.Accept_or_deny = false;
                }
            }
            connection_bool = false;
            rule_bool = false;
        }
    }
}
