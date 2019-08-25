using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SME;

namespace simplePackageFilter
{
    [ClockedProcess]
    public class Loop_Blacklist : SimpleProcess
    {
        [InputBus]
        public IBus_blacklist_ruleVerdict_out[] blacklist_input;

        [OutputBus]
        public Loop_Blacklist_To_Decider to_decider = Scope.CreateBus<Loop_Blacklist_To_Decider>();

        public Loop_Blacklist(IBus_blacklist_ruleVerdict_out[] blacklist_verdict)
        {
            blacklist_input = blacklist_verdict;

        }

        bool Blacklist_bool = false;

        protected override void OnTick()
        {
            to_decider.Valid = false;
            to_decider.Value = false;
            Blacklist_bool = false;

            if (blacklist_input[0].IsSet)
            {

                for (int i = 0; i < blacklist_input.Length; i++)
                {
                    Blacklist_bool |= blacklist_input[i].Accepted;
                }

                to_decider.Valid = true;
                to_decider.Value = Blacklist_bool;
            }
        }
    }
}

