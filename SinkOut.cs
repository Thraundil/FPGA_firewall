using System;
using SME;

namespace simplePackageFilter
{
    public class SinkOut : SimpleProcess
    {
        [InputBus]
        public IBus_blacklist_finalVerdict_out out_reply;


        public SinkOut(IBus_blacklist_finalVerdict_out reps)
        {
            out_reply = reps;
        }

        int counter = 0;
        protected override void OnTick()
        {
            if (!out_reply.Valid)
            {
                Console.WriteLine(counter);
            }
            counter += 1;
        }
    }
}