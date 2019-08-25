using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SME;

namespace simplePackageFilter
{
    [ClockedProcess]
    public class Loop_Connection_IPv4 : SimpleProcess
    {
        [InputBus]
        public IBus_Process_Verdict_IPV4[] connection_list;

        [OutputBus]
        public Loop_Con_IPv4_To_Decider to_decider = Scope.CreateBus<Loop_Con_IPv4_To_Decider>();

        public Loop_Connection_IPv4(IBus_Process_Verdict_IPV4[] con_verdicts)
        {
            connection_list = con_verdicts;

        }

        bool connection_bool = false;

        protected override void OnTick()
        {
            to_decider.Valid = false;
            to_decider.Value = false;
            connection_bool = false;

            if (connection_list[0].IsSet_ipv4)
            {

                for (int i = 0; i < connection_list.Length; i++)
                {
                    connection_bool |= connection_list[i].Accepted_ipv4;
                }

                to_decider.Valid = true;
                to_decider.Value = connection_bool;
            }
        }
    }
}
