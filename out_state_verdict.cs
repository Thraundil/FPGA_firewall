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

        // Need a new bus!!!!!
        [OutputBus]
        private readonly IBus_Update_State update = Scope.CreateOrLoadBus<IBus_Update_State>();

        protected override async Task OnTickAsync()
        {
            while(!connection_list[0].IsSet_outgoing || !blacklist_input[0].IsSet)
            {
                if(connection_list[0].IsSet_outgoing)
                {
                    //set flag to make this stop doing anything
                }
                else
                {
                    // set flag to make blacklist stop doing anything
                }
                await ClockAsync();
            }
            // when both of them are set we both have the data and the blacklist answser

        }
    }
}
