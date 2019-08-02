using System;
using System.Threading.Tasks;
using SME;

namespace simplePackageFilter
{
    public class out_state_verdict : StateProcess
    {

        [InputBus]
        public IBus_blacklist_ruleVerdict_out[] blacklist_input;

        [InputBus]
        public IBus_Process_Verdict_Outgoing[] connection_list;

        [InputBus]
        public IBus_Blacklist_out dataOut;

        [InputBus]
        public IBus_Connection_In_Use[] in_use;

        [OutputBus]
        public out_verdict_to_sim out_to_sim = Scope.CreateOrLoadBus<out_verdict_to_sim>();

        [OutputBus]
        public IBus_Update_State_out update_out = Scope.CreateOrLoadBus<IBus_Update_State_out>();

        [OutputBus]
        public IBus_blacklist_finalVerdict_out final_say_out = Scope.CreateBus<IBus_blacklist_finalVerdict_out>();

        bool connection_bool_out = false;

        bool black_bool_out = false;

        uint counter_id = 5;
        bool found_out_id = false; 

        public out_state_verdict(IBus_blacklist_ruleVerdict_out[] black_input, IBus_Process_Verdict_Outgoing[] con_input, IBus_Blacklist_out the_data, IBus_Connection_In_Use[] uses)
        {
            blacklist_input = black_input;
            connection_list = con_input;
            dataOut = the_data;
            in_use = uses;
        }

        protected override async Task OnTickAsync()
        {
            // Reset every flag
            final_say_out.Accept_or_deny = false;
            final_say_out.Valid = false;
            update_out.Flag = false;
            out_to_sim.out_ready_flag = true;
            connection_bool_out = false;
            black_bool_out = false;


            // For some reason we have to wait to get a signal from
            while(!dataOut.ReadyToWorkFlag)
            {
                await ClockAsync();
            }

            out_to_sim.out_ready_flag = false;


            // Wait for the connection processes to reply back
            // It is fine to simply wait for 1 of each process to reply since
            // They are not multicloked and hence will all "finish" at the same time/clock cycle
            while (!connection_list[0].IsSet_outgoing || !blacklist_input[0].IsSet)
            {
                await ClockAsync();
            }
            // when both of them are set we both have the data and the blacklist answser

            for (int i = 0; i < blacklist_input.Length; i++)
            {
                black_bool_out |= blacklist_input[i].Accepted;
            }

            // if this is true then the blacklist denied the packet
            // if it is false then the blacklist did not block the packet
            if (!black_bool_out)
            {
                final_say_out.Accept_or_deny = true;
                final_say_out.Valid = true;
                for (int i = 0; i < connection_list.Length; i++)
                {
                    connection_bool_out |= connection_list[i].Accepted_outgoing;
                }
                if (connection_bool_out)
                {
                    Console.WriteLine("Outgoing Package:  Exists  (state) ");
                    // and so nothing needs to be done
                }
                else
                {

                    // Convert byte to a bool array
                    // If the incoming packet is a tcp packet
                    if (dataOut.tcp)
                    {
                        var bits = new bool[8];

                        for (int i = 0; i < 8; i++)
                            bits[i] = (dataOut.Flags & (1 << i)) == 0 ? false : true;

                        // reverse the array to get correct order :)
                        Array.Reverse(bits);
                        // only the syn flag is set
                        if (!bits[0] && !bits[1] && !bits[2] && !bits[3] && !bits[4] && !bits[5] && bits[6] && !bits[7])
                        {
                            Console.WriteLine("Outgoing Package:  New TCP  (outgoing)");
                            // and so we must update the stack accordingly
                            // check if the packet is tcp or not
                            update_out.Is_tcp = dataOut.tcp;
                            update_out.Flag = true;
                            update_out.SourceIP = dataOut.DestIP;
                            update_out.DestIP = dataOut.SourceIP;
                            update_out.Port = dataOut.SourcePort;

                            // This one start high and go low while the other process starts low and go high
                            // Should mean they wont try to write to the same process unless it is the only one available 
                            for (int l = 9; l >= 0; l--)
                            {
                                if (!in_use[l].In_use && !found_out_id)
                                {
                                    counter_id = (uint)l;
                                    found_out_id = true;
                                }
                            }
                            found_out_id = false;

                            update_out.Id = counter_id;
                            final_say_out.Valid = true;
                            final_say_out.Accept_or_deny = true;

                        }
                        else
                        {
                            // Do we drop it though??? 
                            // How does this even happen?
                            // How do we send out a packet that is a tcp packet without the the syn flag set and without there already being a connection established
                            Console.WriteLine("Outgoing Package: Does not have the syn flag only set so we dropped it");
                        }
                        //Console.WriteLine(counter_id);
                        //counter_id += 1;
                        //Console.WriteLine("{0} {1} {2} {3}", dataOut.SourceIP[0], dataOut.SourceIP[1], dataOut.SourceIP[2], dataOut.SourceIP[3]);
                        //Console.WriteLine("{0} {1} {2} {3}", dataOut.DestIP[0], dataOut.DestIP[1], dataOut.DestIP[2], dataOut.DestIP[3]);
                        //Console.WriteLine("{0}", dataOut.SourcePort);
                    }
                    // If the incoming packet is an UDP packet
                    else
                    {
                        Console.WriteLine("Outgoing Package: New UDP (outgoing)");
                        update_out.Is_tcp = dataOut.tcp;
                        update_out.Flag = true;
                        update_out.SourceIP = dataOut.DestIP;
                        update_out.DestIP = dataOut.SourceIP;
                        update_out.Port = dataOut.SourcePort;
                        for (int l = 9; l >= 0; l--)
                        {
                            if (!in_use[l].In_use && !found_out_id)
                            {
                                counter_id = (uint)l;
                                found_out_id = true;
                            }
                        }
                        found_out_id = false;

                        update_out.Id = counter_id;
                        final_say_out.Valid = true;
                        final_say_out.Accept_or_deny = true;
                    }
                }

            }
            else
            {
                Console.WriteLine("Outgoing Package:  Blocked (Blacklist)");
                final_say_out.Valid = true;
                final_say_out.Accept_or_deny = false;
            }
        }
    }
}