using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SME;

namespace simplePackageFilter
{
    [ClockedProcess]
    public class Loop_In_use : SimpleProcess
    {
        [InputBus]
        public IBus_Connection_In_Use[] in_use;

        [OutputBus]
        public Loop_In_use_To_Decider to_decider = Scope.CreateBus<Loop_In_use_To_Decider>();

        public Loop_In_use(IBus_Connection_In_Use[] Con_verdict)
        {
            in_use = Con_verdict;

        }

        bool found_out_id_out = false;
        bool found_out_id_tcp = false;

        protected override void OnTick()
        {
            found_out_id_out = false;
            found_out_id_tcp = false;

            // If there was any available conncetions to be written to
            to_decider.Valid_Out = false;
            to_decider.Valid_TCP = false;

            for (int l = 9; l >= 0; l--)
            {
                if (!in_use[l].In_use && !found_out_id_tcp)
                {
                    to_decider.Id_TCP = (uint)l;
                    to_decider.Valid_TCP = true;
                    found_out_id_tcp = true;
                }
                else
                {
                    if(!in_use[l].In_use && !found_out_id_out)
                    {
                        to_decider.Id_Out = (uint)l;
                        to_decider.Valid_Out = true;
                        found_out_id_out = true;
                    }
                }
            }
        }
    }
}
