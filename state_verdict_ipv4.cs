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
        public Loop_Whitelist_IPv4_To_Decider Whitelist_from_loop;

        [InputBus]
        public IBus_IPv4_In ipv4_in;

        // stuff from the ipv4 rule processes

        [InputBus]
        public Loop_Con_IPv4_To_Decider Con_from_loop;

        // if we accept or deny the ipv4 header/packet
        [OutputBus]
        public IBus_State_Verdict_IPv4 final_say_ipv4 = Scope.CreateBus<IBus_State_Verdict_IPv4>();

        [OutputBus]
        public ipv4_verdict_to_sim to_sim = Scope.CreateOrLoadBus<ipv4_verdict_to_sim>();


        bool connection_bool = false;

        bool rule_bool = false;

        public Ipv4_state_verdict(Loop_Con_IPv4_To_Decider con_process, IBus_IPv4_In ipv4_data, Loop_Whitelist_IPv4_To_Decider rule_process)
        {
            Con_from_loop = con_process;
            ipv4_in = ipv4_data;
            Whitelist_from_loop = rule_process;
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
            while (!Con_from_loop.Valid || !Whitelist_from_loop.Valid)
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

            connection_bool = Con_from_loop.Value;

                // Accept the incoming package
                if (connection_bool)
                {
                    final_say_ipv4.Accepted = true;
                    Console.WriteLine("Incoming IPV4:     Matches (connection)");
                }
                else
                {

                rule_bool = Whitelist_from_loop.Value;

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
