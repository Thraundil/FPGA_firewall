using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SME;

namespace simplePackageFilter
{  
    [ClockedProcess]
    public class Loop_Con_TCP : SimpleProcess
    {
        [InputBus]
        public IBus_Process_Verdict_TCP[] connection_list;

        [OutputBus]
        public Loop_Con_TCP_To_Decider to_decider = Scope.CreateBus<Loop_Con_TCP_To_Decider>();

        public Loop_Con_TCP(IBus_Process_Verdict_TCP[] con_verdicts)
        {
            connection_list = con_verdicts;

        }

        bool Connection_bool = false;

        protected override void OnTick()
        {
            to_decider.Valid = false;
            to_decider.Value = false;
            Connection_bool = false;

            if (connection_list[0].IsSet_state)
            {
                for (int i = 0; i < connection_list.Length; i++)
                {
                    Connection_bool |= connection_list[i].Accepted_state;
                }

                to_decider.Valid = true;
                to_decider.Value = Connection_bool;
            }
        }
    }
}
