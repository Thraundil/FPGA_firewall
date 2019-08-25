using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SME;

namespace simplePackageFilter
{
    [ClockedProcess]
    public class Loop_Con_Outgoing : SimpleProcess
    {
        [InputBus]
        public IBus_Process_Verdict_Outgoing[] connection_list;

        [OutputBus]
        public Loop_Con_Outgoing_To_Decider to_decider = Scope.CreateBus<Loop_Con_Outgoing_To_Decider>();

        public Loop_Con_Outgoing(IBus_Process_Verdict_Outgoing[] Con_verdict)
        {
            connection_list = Con_verdict;

        }

        bool Connection_bool = false;

        protected override void OnTick()
        {
            to_decider.Valid = false;
            to_decider.Value = false;
            Connection_bool = false;

            if (connection_list[0].IsSet_outgoing)
            {

                for (int i = 0; i < connection_list.Length; i++)
                {
                    Connection_bool |= connection_list[i].Accepted_outgoing;
                }

                to_decider.Valid = true;
                to_decider.Value = Connection_bool;
            }
        }
    }
}
