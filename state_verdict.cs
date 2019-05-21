using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SME;

namespace simplePackageFilter
{
    [TopLevelInputBus]
    public interface IBus_State_Verdict :  IBus
    {
        [InitialValue(false)]
        bool Accepted { get; set; }

        [InitialValue(false)]
        bool Flag { get; set; }
    }

    [TopLevelInputBus]
    public interface IBus_State_Verdict_IPv4 : IBus
    {
        [InitialValue(false)]
        bool Accepted { get; set; }

        [InitialValue(false)]
        bool flag { get; set; }
    }
    [TopLevelOutputBus]
    public interface IBus_finalVerdict_tcp_In : IBus
    {
        [InitialValue(false)]
        bool Accept_or_deny { get; set; }
        [InitialValue(false)]
        bool Valid { get; set; }


    }

    public class Final_check_state : SimpleProcess
    {
        [InputBus]
        public IBus_ITCP_RuleVerdict[] connection_list;

        [InputBus]
        public IBus_ruleVerdict_In[] rule_list;

        // input from the ipv4 check

        [InputBus]
        public IBus_blacklist_finalVerdict_out blacklist_input;

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

        // Class Variables
        bool connection_bool = false;

        bool rule_bool = false;

        // Constructor         
        public Final_check_state(IBus_ITCP_RuleVerdict[] busList_in, IBus_ruleVerdict_In[] rule_list_in, IBus_blacklist_finalVerdict_out blacklist_bool,
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
            final_say_tcp_in.Valid = false;
            final_say_tcp_in.Accept_or_deny = false;
            if (connection_list[0].IsSet_ipv4)
            {
                // Checks if any rule process returns TRUE.
                //my_bool = busList.Any(val => val.Accepted);
                // Need to just OR them all 
                connection_bool = connection_list.AsParallel().Any(val => val.Accepted_ipv4);
                final_say_tcp_in.Valid = true;

                // Accept the incoming package
                if (connection_bool)
                {
                    final_say_tcp_in.Accept_or_deny = true;
                    Console.WriteLine("The connection already exists");
                }
                else
                {
                    rule_bool = rule_list.AsParallel().Any(val => val.Accepted);
                    if (rule_bool) // And some flags are correct - in parituclar the syn flag
                    {
                        Console.WriteLine("The connection does not exist but matches a whitelisted rule");
                        // do more stuff herel
                        final_say_tcp_in.Accept_or_deny = true;
                        update.Flag = true;
                    }
                    else
                    {
                        Console.WriteLine("The connection does neither match a connection or a rule");
                        final_say_tcp_in.Accept_or_deny = false;
                    }
                }
                connection_bool = false;
                rule_bool = false;
            }
            if(connection_list[0].IsSet_state)
            {
                Console.WriteLine("hmm");
            }
        }
    }
}
