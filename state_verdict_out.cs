using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        [OutputBus]
        public out_verdict_to_sim out_to_sim = Scope.CreateOrLoadBus<out_verdict_to_sim>();

        [OutputBus]
        public IBus_Update_State_out update_out = Scope.CreateOrLoadBus<IBus_Update_State_out>();

        [OutputBus]
        public IBus_blacklist_finalVerdict_out final_say_out = Scope.CreateBus<IBus_blacklist_finalVerdict_out>();

        bool connection_bool_out = false;

        bool black_bool_out = false;

        uint counter_id = 5;

        public out_state_verdict(IBus_blacklist_ruleVerdict_out[] black_input, IBus_Process_Verdict_Outgoing[] con_input, IBus_Blacklist_out the_data)
        {
            blacklist_input = black_input;
            connection_list = con_input;
            dataOut = the_data;
        }

        protected override async Task OnTickAsync()
        {
            final_say_out.Accept_or_deny = false;
            final_say_out.Valid = false;
            update_out.Flag = false;
            out_to_sim.out_ready_flag = true;
            connection_bool_out = false;
            black_bool_out = false;

            while(!dataOut.ReadyToWorkFlag)
            {
                await ClockAsync();
            }

            out_to_sim.out_ready_flag = false;

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
                    Console.WriteLine("The outgoing packet is already part of a connection");
                    // and so nothing needs to be done
                }
                else
                {
                    Console.WriteLine("The outgoing packet is not part of a connection!");
                    // and so we must update the stack accordingly
                    update_out.Flag = true;
                    update_out.set_in_use = true;
                    update_out.SourceIP = dataOut.DestIP;
                    update_out.DestIP = dataOut.SourceIP;
                    update_out.Port = dataOut.SourcePort;
                    update_out.Id = counter_id; // This is not the correct way to do it
                    counter_id += 1;
                    //Console.WriteLine("{0} {1} {2} {3}", dataOut.SourceIP[0], dataOut.SourceIP[1], dataOut.SourceIP[2], dataOut.SourceIP[3]);
                    //Console.WriteLine("{0} {1} {2} {3}", dataOut.DestIP[0], dataOut.DestIP[1], dataOut.DestIP[2], dataOut.DestIP[3]);
                    //Console.WriteLine("{0}", dataOut.SourcePort);
                }

            }
            else
            {
                Console.WriteLine("blacklist denied the package");
                final_say_out.Valid = true;
                final_say_out.Accept_or_deny = false;
            }


        }
    }
}
