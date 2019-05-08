using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SME;

namespace sme_example
{
    [TopLevelInputBus]
    public interface ITCP_In : IBus
    {
        [FixedArrayLength(4)]
        IFixedArray<byte> SourceIP { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> DestIP { get; set; }

        int Port { get; set; }

        [FixedArrayLength(9)]
        IFixedArray<byte> Flags { get; set; }

        [InitialValue(false)]
        bool ThatOneVariableThatSaysIfWeAreDone { get; set; }


    }
    [TopLevelInputBus]
    public interface ITCP_RuleVerdict : IBus
    {
        [InitialValue(false)]
        bool Accepted { get; set; }

        [InitialValue(false)]
        bool IsSet { get; set; }
    }
    public class Connection_process : SimpleProcess
    {
        [InputBus]
        public ITCP_In TCP;

        [OutputBus]
        public ITCP_RuleVerdict ruleVerdict = Scope.CreateBus<ITCP_RuleVerdict>();

        private readonly long ip_low_source = new long();
        private readonly long ip_high_source = new long();

        // IP destination range low/high as a LONG
        private readonly long ip_low_dest = new long();
        private readonly long ip_high_dest = new long();

        private readonly int port_low_in = new int();
        private readonly int port_high_in = new int();
        readonly long doubl = (65536);    // 256*256
        readonly long triple = (16777216); // 256*256*256
        public Connection_process(ITCP_In busIn, long ip_low_source_in, long ip_high_source_in, long ip_low_dest_in, long ip_high_dest_in, int port_low, int port_high)
        {
            TCP = busIn;
            port_high_in = port_high;
            port_low_in = port_low;
            ip_low_source = ip_low_source_in;
            ip_high_source = ip_high_source_in;
            ip_low_dest = ip_low_dest_in;
            ip_high_dest = ip_high_dest_in;
        }


        private bool DoesConnectExist(long low_source, long high_source, long low_dest, long high_dest, int port_low, int port_high, long tcp_source, long tcp_dest)
        {
            if (low_source == tcp_source && tcp_source == high_source)
            {
                if (low_dest == tcp_dest && tcp_dest == high_dest)
                {
                    if (port_low == TCP.Port && port_high == TCP.Port)
                    {
                        // The received packet's Source IP was accepted, as it was
                        // inside the accepted IP ranges of a specific rule.
                        return true;
                    }
                }
            }

            return false;
        }

        protected override void OnTick()
        {
            ruleVerdict.IsSet = false;
            ruleVerdict.Accepted = false;
            if (true) // FIX SHOULD NOT BE TRUE :)
            {
                long tcp_source = TCP.SourceIP[3] + (TCP.SourceIP[2] * 256) + (TCP.SourceIP[1] * doubl) + (TCP.SourceIP[0] * triple);
                long tcp_dest = TCP.DestIP[3] + (TCP.DestIP[2] * 256) + (TCP.DestIP[1] * doubl) + (TCP.DestIP[0] * triple);
                if (DoesConnectExist(ip_low_source, ip_high_source, ip_low_dest, ip_high_dest, port_low_in, port_high_in, tcp_source, tcp_dest))
                {
                    // Update/check state before accepting!!
                    ruleVerdict.Accepted = true;
                }
                ruleVerdict.IsSet = true;
            }
        }
    }
}
