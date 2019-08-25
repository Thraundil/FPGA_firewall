using System;
using System.Threading.Tasks;
using SME;

namespace simplePackageFilter
{
    public class stateful_state_verdict : StateProcess
    {
        [InputBus]
        public Loop_Con_TCP_To_Decider connection_list;

        [InputBus]
        public Loop_Whitelist_TCP_To_Decider rule_list;

        [InputBus]
        public IBus_ITCP_In stateful_in;


        // USE THIS
        [InputBus]
        public Loop_In_use_To_Decider in_use; 

        [OutputBus]
        public IBus_finalVerdict_tcp_In final_say_tcp_in = Scope.CreateBus<IBus_finalVerdict_tcp_In>();

        [OutputBus]
        public IBus_Update_State_tcp update_tcp = Scope.CreateOrLoadBus<IBus_Update_State_tcp>();

        [OutputBus]
        public tcp_verdict_to_sim to_sim = Scope.CreateOrLoadBus<tcp_verdict_to_sim>();


        bool connection_bool_tcp = false;

        bool rule_bool = false;


        bool[] bits = new bool[8];

        public stateful_state_verdict(Loop_Con_TCP_To_Decider con_process, Loop_Whitelist_TCP_To_Decider rule_process, IBus_ITCP_In tcp_data, Loop_In_use_To_Decider used)
        {
            connection_list = con_process;
            rule_list = rule_process;
            stateful_in = tcp_data;
            in_use = used;
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

            connection_bool_tcp = false;
            rule_bool = false;


            // We wait to get some actual data to inspect
            while (!stateful_in.ThatOneVariableThatSaysIfWeAreDone)
            {
                to_sim.tcp_ready_flag = true; // Latches...
                final_say_tcp_in.Valid = false; // To avoid latches...
                final_say_tcp_in.Accept_or_deny = false; // see above
                await ClockAsync();
            }

            // tell the simulator to wait
            // until the rules and the connection processes have responded
            to_sim.tcp_ready_flag = false; // more latches
            final_say_tcp_in.Valid = false; // yes yes yes
            final_say_tcp_in.Accept_or_deny = false; // ... 

            while (!connection_list.Valid || !rule_list.Valid)
            {
                to_sim.tcp_ready_flag = false; 
                final_say_tcp_in.Valid = false; 
                final_say_tcp_in.Accept_or_deny = false; 
                await ClockAsync();
            }
            to_sim.tcp_ready_flag = false;

            // Check if the state processes found a match
            // done by OR'ing them all together
            connection_bool_tcp = connection_list.Value;
            final_say_tcp_in.Valid = true;

            // Accept the incoming package
            if (connection_bool_tcp)
            {
                final_say_tcp_in.Accept_or_deny = true;
                Console.WriteLine("Incoming TCP:      Matches (connection)");
                // we do nothing if the connection already exist
            }
            // if the state processes did not find a match check the rules processes
            // Again we OR them all together
            else
            {
                rule_bool = rule_list.Value;
                if (rule_bool)
                {

                    // TCP packet
                    if (stateful_in.is_tcp)
                    {
                        // Convert byte to a bool array
                        for (int i = 0; i < 8; i++)
                            bits[i] = (stateful_in.Flags & (1 << i)) == 0 ? false : true;

                        // reverse the array to get correct order :)
                        Array.Reverse(bits);

                        if (!bits[0] && !bits[1] && !bits[2] && !bits[3] && !bits[4] && !bits[5] && bits[6] && !bits[7]) // only the syn flag is set
                        {
                            if (in_use.Valid_TCP)
                            {
                                Console.WriteLine("Incoming TCP:      Matches (whitelist)");
                                final_say_tcp_in.Accept_or_deny = true;
                                update_tcp.Flag = true;
                                update_tcp.is_tcp = stateful_in.is_tcp;
                                update_tcp.SourceIP = stateful_in.SourceIP;
                                update_tcp.DestIP = stateful_in.DestIP;
                                update_tcp.Port = stateful_in.Port;

                                update_tcp.Id = in_use.Id_TCP;
                            }
                            else
                            {
                                Console.WriteLine("Incoming ´TCP:  No available Connection!");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Incoming TCP:      Blocked");
                            final_say_tcp_in.Accept_or_deny = false;
                        }
                        //Console.WriteLine("{0} {1} {2} {3}", stateful_in.SourceIP[0], stateful_in.SourceIP[1], stateful_in.SourceIP[2], stateful_in.SourceIP[3]);
                        //Console.WriteLine("{0} {1} {2} {3}", stateful_in.DestIP[0], stateful_in.DestIP[1], stateful_in.DestIP[2], stateful_in.DestIP[3]);
                        //Console.WriteLine("{0}", stateful_in.Port);
                    }
                    // UDP packet
                    else
                    {
                        if (in_use.Valid_TCP)
                        {
                            final_say_tcp_in.Accept_or_deny = true;
                            update_tcp.Flag = true;
                            update_tcp.is_tcp = stateful_in.is_tcp;
                            update_tcp.SourceIP = stateful_in.SourceIP;
                            update_tcp.DestIP = stateful_in.DestIP;
                            update_tcp.Port = stateful_in.Port;
                            Console.WriteLine("Incoming UDP:      Matches(Whitelist)");
                            update_tcp.Id = in_use.Id_TCP;
                        }
                        else
                        {
                            Console.WriteLine("Incoming UDP:  No available Connection!");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Incoming {0}:      Blocked", stateful_in.is_tcp ? "TCP" : "UDP");
                    final_say_tcp_in.Accept_or_deny = false;
                }
            }
        }
    }
}
