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




        bool connection_bool = false;

        bool rule_bool = false;

        uint counter_id = 1;

        protected override async Task OnTickAsync()
        {

            // reset the flags
            final_say_tcp_in.Valid = false; // valid is
            final_say_tcp_in.Accept_or_deny = false; // did we 
            connection_bool = false; // see if you are in the connection set
            rule_bool = false; // see if you are in the rule set
            // need a flag to say if we are ready to recieve the next packet from the group! :)

            // i wonder if we can get away with using only the first index here
            // we wait for both the connection process the actual data and the rule process to send us their stuff
            while (!connection_list[0].IsSet_state || !stateful_in.ThatOneVariableThatSaysIfWeAreDone || !rule_list[0].tcp_IsSet)
            {
                if(connection_list[0].IsSet_state)
                {
                    // tell the connection processes to wait
                }
                if(rule_list[0].tcp_IsSet)
                {
                    // tell the rules to wait
                }
                if(stateful_in.ThatOneVariableThatSaysIfWeAreDone)
                {
                    // tell the "other group"/simulator to wait
                }
                await ClockAsync();
            }

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
            else
            {
                for (int i = 0; i < rule_list.Length; i++)
                {
                    rule_bool |= rule_list[i].tcp_Accepted;
                }
                if (rule_bool) // And some flags are correct - in parituclar the syn flag
                {
                    Console.WriteLine("The tcp connection does not exist but matches a whitelisted rule");
                    // do more stuff herel
                    final_say_tcp_in.Accept_or_deny = true;
                    update.Flag = true;
                    update.SourceIP = stateful_in.SourceIP;
                    update.DestIP = stateful_in.DestIP;
                    update.Port = stateful_in.Port;
                    update.Id = counter_id; // fix counter
                    counter_id += 1; 
                    Console.WriteLine("{0} {1} {2} {3}", stateful_in.SourceIP[0], stateful_in.SourceIP[1], stateful_in.SourceIP[2], stateful_in.SourceIP[3]);
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
