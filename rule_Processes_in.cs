using SME;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Net;

namespace simplePackageFilter
{
    public class Rule_Process_IPV4 : SimpleProcess
    {

        // Input bus from the ipv4 header check
        [InputBus]
        public IBus_IPv4_In ipv4;

        [OutputBus]
        public IBus_Rule_Verdict_IPV4 ruleVerdict = Scope.CreateBus<IBus_Rule_Verdict_IPV4>();

        // IP source range low/high as a byte array
        private  byte[] ip_low_source { get; set; }
        private  byte[] ip_high_source { get; set; }

        // IP destination range low/high as a byte array
        private  byte[] ip_low_dest { get;  set; }
        private  byte[] ip_high_dest { get; set; }

        public Rule_Process_IPV4(IBus_IPv4_In busIn, byte[] ip_low_source_in, byte[] ip_high_source_in,
                                 byte[] ip_low_dest_in, byte[] ip_high_dest_in)
        {
            ipv4           = busIn;
            ip_low_source  = ip_low_source_in;
            ip_high_source = ip_high_source_in;
            ip_low_dest    = ip_low_dest_in;
            ip_high_dest   = ip_high_dest_in;
        }

        protected override void OnTick()
        {
            ruleVerdict.ipv4_IsSet = false;

            if (ipv4.ClockCheck)
            {
                if (Shared_functions.IsIPinRange(ip_low_source, ip_high_source,ip_low_dest,
                                ip_high_dest, ipv4.SourceIP, ipv4.DestIP))
                {
                    ruleVerdict.ipv4_Accepted = true;
                }
                else
                {
                    ruleVerdict.ipv4_Accepted = false;
                }

                ruleVerdict.ipv4_IsSet = true;
            }
        }
    }

    // ****************************************************************************
    [ClockedProcess]
    public class Rule_Process_TCP : SimpleProcess
    {

        // Input but from the stateful check
        [InputBus]
        public IBus_ITCP_In stateful_in;

        [OutputBus]
        public IBus_Rule_Verdict_TCP ruleVerdict = Scope.CreateBus<IBus_Rule_Verdict_TCP>();

        // IP source range low/high as a byte array
        private  byte[] ip_low_source { get; set; }
        private  byte[] ip_high_source { get; set; }

        // IP destination range low/high as a byte array
        private  byte[] ip_low_dest { get;  set; }
        private  byte[] ip_high_dest { get; set; }


        // ipv4Reader_Constructor
        public Rule_Process_TCP(IBus_ITCP_In tcpin, byte[] ip_low_source_in, byte[] ip_high_source_in,
                                byte[] ip_low_dest_in, byte[] ip_high_dest_in)
        {
            stateful_in    = tcpin;
            ip_low_source  = ip_low_source_in;
            ip_high_source = ip_high_source_in;
            ip_low_dest    = ip_low_dest_in;
            ip_high_dest   = ip_high_dest_in;
        }

        protected override void OnTick()
        {
            ruleVerdict.tcp_IsSet = false;

            if (stateful_in.ThatOneVariableThatSaysIfWeAreDone)
            {
                if (Shared_functions.IsIPinRange(ip_low_source, ip_high_source, ip_low_dest, ip_high_dest,
                                stateful_in.SourceIP, stateful_in.DestIP))
                {
                    ruleVerdict.tcp_Accepted = true;
                }
                else
                {
                    ruleVerdict.tcp_Accepted = false;
                }
                ruleVerdict.tcp_IsSet = true;
            }
        }
    }
}


