using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SME;

namespace simplePackageFilter
{
    [TopLevelInputBus]
    public interface IBus_State_Verdict :  IBus
    {
        bool Accepted { get; set; }

        bool Flag { get; set; }
    }

    class State_verdict : SimpleProcess
    {
        [InputBus]
        public IBus_finalVerdict_In Rule_vote;

        [InputBus]
        public IBus_finalVerdict_tcp_In Connection_vote;

        [OutputBus]
        public IBus_State_Verdict State_accept = Scope.CreateBus<IBus_State_Verdict>();
        protected override void OnTick()
        {
            State_accept.Flag = false;
            State_accept.Accepted = false;
            if(Rule_vote.Valid && Connection_vote.Valid)
            {
                if(Rule_vote.Accept_or_deny || Connection_vote.Accept_or_deny)
                {
                    State_accept.Accepted = true;
                }
                State_accept.Flag = true;
            }
        }
    }
}
