using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SME;

namespace simplePackageFilter
{
    public class Ipv4_state_verdict : StateProcess
    {

        // stuff from the ipv4 connection processes
        [InputBus]
        public IBus_Process_Verdict_IPV4[] connection_list;

        [InputBus]
        public IBus_IPv4_In ipv4_in;

        // stuff from the ipv4 rule processes
        [InputBus]
        public IBus_Rule_Verdict_IPV4[] rule_list;

        // if we accept or deny the ipv4 header/packet
        [OutputBus]
        public IBus_State_Verdict_IPv4 final_say_ipv4 = Scope.CreateBus<IBus_State_Verdict_IPv4>();

        [OutputBus]
        public ipv4_verdict_to_sim to_sim = Scope.CreateOrLoadBus<ipv4_verdict_to_sim>();


        bool connection_bool = false;

        bool rule_bool = false;

        public Ipv4_state_verdict(IBus_Process_Verdict_IPV4[] con_process, IBus_IPv4_In ipv4_data, IBus_Rule_Verdict_IPV4[] rule_process)
        {
            connection_list = con_process;
            ipv4_in = ipv4_data;
            rule_list = rule_process;
        }

        protected override async Task OnTickAsync()
        {
            // may need to fix this seems super sketchy
            final_say_ipv4.flag = false;
            final_say_ipv4.Accepted = false;
            connection_bool = false;
            rule_bool = false;

            // whenver we get back here we should be ready to recieve the next package
            to_sim.ipv4_ready_flag = true;

            while (!ipv4_in.ClockCheck)
            {
                to_sim.ipv4_ready_flag = true;
                final_say_ipv4.flag = false;
                final_say_ipv4.Accepted = false;
                await ClockAsync();
            }

            final_say_ipv4.flag = false;
            final_say_ipv4.Accepted = false;
            to_sim.ipv4_ready_flag = false;

            // send signal to not send another packet
            // while we dont have the msg from the state

            // may need to fix this lol
            while (!connection_list[0].IsSet_ipv4 || !rule_list[0].ipv4_IsSet)
            {
                final_say_ipv4.flag = false;
                final_say_ipv4.Accepted = false;
                to_sim.ipv4_ready_flag = false;
                await ClockAsync();
            }
            to_sim.ipv4_ready_flag = false;

            // They should all always be true :)
            //Console.WriteLine("{0} {1} {2} {3}", connection_list[0].IsSet_ipv4, connection_list[1].IsSet_ipv4, connection_list[2].IsSet_ipv4, connection_list[3].IsSet_ipv4);

            // we have recieved a msg from both the data(simulator process) and the state processes
            // so we are good to go

            final_say_ipv4.flag = true;
                // Checks if any rule process returns true
                // Need to just OR them all 
                for (int i = 0; i < connection_list.Length; i++)
                {
                    connection_bool |= connection_list[i].Accepted_ipv4;
                }

                // Accept the incoming package
                if (connection_bool)
                {
                    final_say_ipv4.Accepted = true;
                    Console.WriteLine("Incoming IPV4:     Matches (connection)");
                }
                else
                {
                    for (int i = 0; i < rule_list.Length; i++)
                    {
                        rule_bool |= rule_list[i].ipv4_Accepted;
                    }
                    if (rule_bool)
                    {
                        Console.WriteLine("Incoming IPV4:     Matches (whitelist)");
                        final_say_ipv4.Accepted = true;
                    }
                    else
                    {
                        Console.WriteLine("Incoming IPV4:     Blocked");
                        final_say_ipv4.Accepted = false;
                        //final_say_ipv4.Accepted = false;
                        // no need to set the flag to false since it will already be false
                    }
                }
        }
    }
}
