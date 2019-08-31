using System;
using SME;

namespace simplePackageFilter
{
    public class SinkTCP : SimpleProcess
    {
        [InputBus]
        public IBus_finalVerdict_tcp_In tcp_reply;


        public SinkTCP(IBus_finalVerdict_tcp_In reps)
        {
            tcp_reply = reps;
        }

        int counter = 0;
        protected override void OnTick()
        {
            if (!tcp_reply.Valid)
            {
                Console.WriteLine(counter);
            }
            counter += 1;
        }
    }
}
