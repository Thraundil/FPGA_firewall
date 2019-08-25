using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SME;

namespace simplePackageFilter
{
    [ClockedProcess]
    public class Loop_Whitelist_TCP : SimpleProcess
    {
        [InputBus]
        public IBus_Rule_Verdict_TCP[] rule_list;

        [OutputBus]
        public Loop_Whitelist_TCP_To_Decider to_decider = Scope.CreateBus<Loop_Whitelist_TCP_To_Decider>();

        public Loop_Whitelist_TCP(IBus_Rule_Verdict_TCP[] rule_verdicts)
        {
            rule_list = rule_verdicts;

        }

        bool Rule_bool = false;

        protected override void OnTick()
        {
            to_decider.Valid = false;
            to_decider.Value = false;
            Rule_bool = false;

            if (rule_list[0].tcp_IsSet)
            {

                for (int i = 0; i < rule_list.Length; i++)
                {
                    Rule_bool |= rule_list[i].tcp_Accepted;
                }

                to_decider.Valid = true;
                to_decider.Value = Rule_bool;
            }
        }
    }
}
