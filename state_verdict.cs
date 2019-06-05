using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SME;

namespace simplePackageFilter
{
    public class Final_check_state : SimpleProcess
    {
        [InputBus]
        public IBus_ITCP_RuleVerdict[] connection_list;

        [InputBus]
        public IBus_ruleVerdict_In[] rule_list;

        [InputBus]
        public IBus_blacklist_ruleVerdict_out[] blacklist_input;

        //[InputBus]
        //public IBus_blacklist_finalVerdict_out blacklist_input;

        [InputBus]
        public IBus_Blacklist_out dataOut;

        // Input but from the transportation layer check / stateful check
        [InputBus]
        public IBus_ITCP_In stateful_in;

        [OutputBus]
        public IBus_State_Verdict_IPv4 final_say_ipv4 = Scope.CreateBus<IBus_State_Verdict_IPv4>();

        [OutputBus]
        public IBus_finalVerdict_tcp_In final_say_tcp_in = Scope.CreateBus<IBus_finalVerdict_tcp_In>();

        [OutputBus]
        private readonly IBus_Update_State update = Scope.CreateOrLoadBus<IBus_Update_State>();

        [OutputBus]
        public readonly IBus_blacklist_finalVerdict_out final_say_out = Scope.CreateBus<IBus_blacklist_finalVerdict_out>();

        // Class Variables
        bool connection_bool = false;

        bool rule_bool = false;

        bool out_bool = true;

        // Constructor         
        public Final_check_state(IBus_ITCP_RuleVerdict[] busList_in, IBus_ruleVerdict_In[] rule_list_in, IBus_blacklist_ruleVerdict_out[] blacklist_bool,
            IBus_Blacklist_out data_Out, IBus_ITCP_In state)
        {
            connection_list = busList_in;
            rule_list = rule_list_in;
            blacklist_input = blacklist_bool;
            dataOut = data_Out;
            stateful_in = state;
        }

        protected override void OnTick()
        {
            final_say_ipv4.flag = false;
            final_say_ipv4.Accepted = false;
            final_say_tcp_in.Valid = false;
            final_say_tcp_in.Accept_or_deny = false;
            update.Flag = false;
            update.Flag_2 = false;


            if (connection_list[0].IsSet_ipv4)
            {
                // Checks if any rule process returns TRUE.
                //my_bool = busList.Any(val => val.Accepted);
                // Need to just OR them all 
                for (int i = 0; i < connection_list.Length; i++)
                {
                    connection_bool |= connection_list[i].Accepted_ipv4;
                }
                final_say_ipv4.Accepted = true;

                // Accept the incoming package
                if (connection_bool)
                {
                    final_say_ipv4.Accepted = true;
                    Console.WriteLine("The ipv4 header connection already exists");
                }
                else
                {
                    for (int i = 0; i < rule_list.Length; i++)
                    {
                        rule_bool |= rule_list[i].ipv4_Accepted;
                    }
                    if (rule_bool) // And some flags are correct - in parituclar the syn flag
                    {
                        Console.WriteLine("The ipv4 header matches a rule or a connection");
                        // do more stuff herel
                        final_say_ipv4.Accepted = true;
                    }
                    else
                    {
                        Console.WriteLine("The ipv4 header does not match a rule or connection");
                        final_say_ipv4.Accepted = false;
                    }
                }
                connection_bool = false;
                rule_bool = false;
            }
            if(connection_list[0].IsSet_state)
            {
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
                        update.Id = 1; // Fix this!!!l
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
            if(connection_list[0].IsSet_out)
            {
                Console.WriteLine("damn");
                Console.WriteLine(blacklist_input[0].IsSet);
                if(blacklist_input[0].IsSet)
                {
                    // Checks if any rule process returns TRUE.
                    // my_bool = busList.Any(val => val.Accepted);
                    for (int i = 0; i > blacklist_input.Length; i++)
                    {
                        out_bool |= blacklist_input[i].Accepted;
                    }

                    // Accept the incoming package
                    if (!out_bool)
                    {
                        final_say_out.Accept_or_deny = true;
                        Console.WriteLine("The blacklist Accepted the package");
                        for (int i = 0; i < connection_list.Length; i++)
                        {
                            connection_bool |= connection_list[i].Accepted_out;
                        }
                        final_say_tcp_in.Valid = true;

                        if (connection_bool)
                        {
                            // If it already exist we do do not have to update the state
                            Console.WriteLine("The outgoing connection already exists");
                        }
                        else
                        {
                            Console.WriteLine("The outgoing connection does not already exist. Into the state it goes!");
                            update.Flag_2 = true;
                            update.SourceIP_2 = stateful_in.SourceIP;
                            update.DestIP_2 = stateful_in.DestIP;
                            update.Port_2 = stateful_in.Port;
                            update.Id_2 = 2; // Fix this!!!l
                        }
                        connection_bool = false;
                    }
                    // Deny the incoming package, as the IP was not on the whitelist.
                    else
                    {
                        Console.WriteLine("The blacklist denied the package");
                        final_say_out.Accept_or_deny = false;
                    }
                    out_bool = true;
                }
                else
                {
                    final_say_out.Accept_or_deny = false;
                }
            }
        }

    }
}
