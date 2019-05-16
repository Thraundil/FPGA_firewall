using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SME;

namespace simplePackageFilter
{

    public class out_connections : SimulationProcess
    {
        // The bus from the blacklist. If it is blacklisted then should set the valid flag to false and true otherwise.
        [InputBus]
        public IBus_blacklist_finalVerdict_out blacklist_input;

        // Input bus containing the data, eg port's and ip's
        [InputBus, OutputBus]
        public IBus_Blacklist_out data;

        // This is the input bus from the final_thing that says if we need to add a new connection to the state
        //[InputBus]
        //public 

        // This is the output bus to the processes(state) 
        //[OutputBus]

        // This is the output bus to create a new connection. Maybe it can be combined with the other output bus
        //[OutputBus]

        public async override System.Threading.Tasks.Task Run()
        {
            while(true)
            {
                while (!blacklist_input.Valid)
                {
                    await ClockAsync();
                }
                if(blacklist_input.Valid)
                {
                    if(blacklist_input.Accept_or_deny)
                    {

                    }
                }

            }
        }
    }
}
