using System;
using SME;

namespace simplePackageFilter
{
    public class SinkP : SimpleProcess
    {
        [InputBus]
        public IBus_State_Verdict_IPv4 ipv4_reply;


        public SinkP(IBus_State_Verdict_IPv4 reps)
        {
            ipv4_reply = reps;
        }

        int counter = 0;
        protected override void OnTick()
        {
            if(!ipv4_reply.flag)
            {
                Console.WriteLine(counter);
            }
            counter += 1;
        }
    }
}
