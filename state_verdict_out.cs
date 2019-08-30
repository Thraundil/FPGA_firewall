using System;
using System.Threading.Tasks;
using SME;

namespace simplePackageFilter
{
    public class out_state_verdict : SimpleProcess
    {

        [InputBus]
        public Loop_Blacklist_To_Decider blacklist_input;

        [InputBus]
        public Loop_Con_Outgoing_To_Decider connection_list;

        [InputBus]
        public IBus_Blacklist_out dataOut;

        [InputBus]
        public Loop_In_use_To_Decider in_use;

        [OutputBus]
        public out_verdict_to_sim out_to_sim = Scope.CreateOrLoadBus<out_verdict_to_sim>();

        [OutputBus]
        public IBus_Update_State_out update_out = Scope.CreateOrLoadBus<IBus_Update_State_out>();

        [OutputBus]
        public IBus_blacklist_finalVerdict_out final_say_out = Scope.CreateBus<IBus_blacklist_finalVerdict_out>();

        bool connection_bool_out = false;

        bool black_bool_out = false;

        bool[] bits = new bool[8];

        byte[] Source_data_1 = new byte[4] { 0x00, 0x00, 0x00, 0x00 };
        byte[] Dest_data_1 = new byte[4] { 0x00, 0x00, 0x00, 0x00 };
        uint port_data_1 = 0;
        byte flags_data_1 = 0x00;
        bool is_tcp_data_1 = false;

        byte[] Source_data_2 = new byte[4] { 0x00, 0x00, 0x00, 0x00 };
        byte[] Dest_data_2 = new byte[4] { 0x00, 0x00, 0x00, 0x00 };
        uint port_data_2 = 0;
        byte flags_data_2 = 0x00;
        bool is_tcp_data_2 = false;

        public out_state_verdict(Loop_Blacklist_To_Decider black_input, Loop_Con_Outgoing_To_Decider con_input, IBus_Blacklist_out the_data, Loop_In_use_To_Decider uses)
        {
            blacklist_input = black_input;
            connection_list = con_input;
            dataOut = the_data;
            in_use = uses;
        }

        protected override void OnTick()
        {
            // Reset every flag
            final_say_out.Accept_or_deny = false;
            final_say_out.Valid = false;
            update_out.Flag = false;
            connection_bool_out = false;
            black_bool_out = false;



            // Wait for the connection processes to reply back
            // It is fine to simply wait for 1 of each process to reply since
            // They are not multicloked and hence will all "finish" at the same time/clock cycle
            if (blacklist_input.Valid || connection_list.Valid)
            {
                out_to_sim.out_ready_flag = false;
                // when both of them are set we both have the data and the blacklist answser

                black_bool_out = blacklist_input.Value;

                // if this is true then the blacklist denied the packet
                // if it is false then the blacklist did not block the packet
                if (!black_bool_out)
                {
                    final_say_out.Accept_or_deny = true;
                    final_say_out.Valid = true;
                    connection_bool_out = connection_list.Value;

                    if (connection_bool_out)
                    {
                        Console.WriteLine("Outgoing Package:  Exists  (state) ");
                        // and so nothing needs to be done
                    }
                    else
                    {
                        // Convert byte to a bool array
                        // If the incoming packet is a tcp packet
                        if (dataOut.is_tcp)
                        {

                            for (int i = 0; i < 8; i++)
                                bits[i] = (flags_data_1 & (1 << i)) == 0 ? false : true;

                            // reverse the array to get correct order :)
                            Array.Reverse(bits);
                            // only the syn flag is set
                            if (!bits[0] && !bits[1] && !bits[2] && !bits[3] && !bits[4] && !bits[5] && bits[6] && !bits[7])
                            {
                                if (in_use.Valid_Out)
                                {
                                    Console.WriteLine("Outgoing Package:  New TCP  (outgoing)");



                                    // and so we must update the stack accordingly
                                    // check if the packet is tcp or not
                                    update_out.Is_tcp = is_tcp_data_1;
                                    update_out.Flag = true;

                                    for (int i = 0; i < 4; i++)
                                    {
                                        update_out.SourceIP[i] = Dest_data_1[i];
                                        update_out.DestIP[i] = Source_data_1[i];
                                    }

                                    update_out.Port = port_data_1;

                                    // This one start high and go low while the other process starts low and go high
                                    // Should mean they wont try to write to the same process unless it is the only one available 

                                    update_out.Id = in_use.Id_Out;

                                    Console.WriteLine("update out", update_out.Id);

                                    final_say_out.Valid = true;
                                    final_say_out.Accept_or_deny = true;
                                }
                                else
                                {
                                    Console.WriteLine("Outgoing Package:  No available Connection!");
                                }

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
                            if (in_use.Valid_Out)
                            {
                                Console.WriteLine("Outgoing Package: New UDP (outgoing)");

                                update_out.Is_tcp = is_tcp_data_1;
                                update_out.Flag = true;

                                for (int i = 0; i < 4; i++)
                                {
                                    update_out.SourceIP[i] = Dest_data_1[i];
                                    update_out.DestIP[i] = Source_data_1[i];
                                }

                                update_out.Port = port_data_1;

                                update_out.Id = in_use.Id_Out;

                                final_say_out.Valid = true;
                                final_say_out.Accept_or_deny = true;
                            }
                            else
                            {
                                Console.WriteLine("Outgoing Package:  No available Connection!");
                            }
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

            Source_data_1 = Source_data_2;
            Dest_data_1 = Dest_data_2;
            port_data_1 = port_data_2;
            flags_data_1 = flags_data_2;
            is_tcp_data_1 = is_tcp_data_2;


            if (dataOut.ReadyToWorkFlag)
            {
                for (int i = 0; i < 4; i++)
                {
                    Source_data_2[i] = dataOut.SourceIP[i];
                    Dest_data_2[i] = dataOut.DestIP[i];

                }
                flags_data_2 = dataOut.Flags;
                port_data_2 = dataOut.SourcePort;
                is_tcp_data_2 = dataOut.is_tcp;
            }
        }
    }
}