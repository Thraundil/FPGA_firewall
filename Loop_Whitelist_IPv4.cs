using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SME;

namespace simplePackageFilter
{
    [ClockedProcess]
    public class Loop_Whitelist_IPv4 : SimpleProcess
    {
        [InputBus]
        public IBus_Rule_Verdict_IPV4[] rule_list;

        [OutputBus]
        public Loop_Whitelist_IPv4_To_Decider to_decider = Scope.CreateBus<Loop_Whitelist_IPv4_To_Decider>();

        public Loop_Whitelist_IPv4(IBus_Rule_Verdict_IPV4[] rule_verdicts)
        {
            rule_list = rule_verdicts;

        }

        bool Rule_bool = false;

        protected override void OnTick()
        {
            to_decider.Valid = false;
            to_decider.Value = false;
            Rule_bool = false;

            if (rule_list[0].ipv4_IsSet)
            {

                for (int i = 0; i < rule_list.Length; i++)
                {
                    Rule_bool |= rule_list[i].ipv4_Accepted;
                }

                to_decider.Valid = true;
                to_decider.Value = Rule_bool;
            }
        }
    }
}
