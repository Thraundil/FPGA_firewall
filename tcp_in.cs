using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SME;

namespace simplePackageFilter
{
    [TopLevelInputBus]
    public interface IBus_ITCP_In : IBus
    {
        [FixedArrayLength(4)]
        IFixedArray<byte> SourceIP { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> DestIP { get; set; }

        byte Port { get; set; }

        byte Flags { get; set; }

        [InitialValue(false)]
        bool ThatOneVariableThatSaysIfWeAreDone { get; set; }


    }
    [TopLevelInputBus]
    public interface IBus_ITCP_RuleVerdict : IBus
    {
        [InitialValue(false)]
        bool Accepted { get; set; }

        [InitialValue(false)]
        bool IsSet { get; set; }
    }
    [TopLevelOutputBus]
    public interface IBus_finalVerdict_tcp_In : IBus
    {
        bool Accept_or_deny { get; set; }

        bool Valid { get; set; }
    }
    public class Connection_process : SimpleProcess
    {
        [InputBus]
        public IBus_ITCP_In TCP;

        [OutputBus]
        public IBus_ITCP_RuleVerdict ruleVerdict = Scope.CreateBus<IBus_ITCP_RuleVerdict>();

        private long ip_low_source { get; set; }
        private long ip_high_source { get; set; }

        // IP destination range low/high as a LONG
        private long ip_low_dest { get; set; }
        private long ip_high_dest { get; set; }

        private int port_low_in { get; set; }
        private int port_high_in { get; set; }

        readonly long doubl = (65536);    // 256*256
        readonly long triple = (16777216); // 256*256*256
        public Connection_process(IBus_ITCP_In busIn, long ip_low_source_in, long ip_high_source_in, long ip_low_dest_in, long ip_high_dest_in, int port_low, int port_high)
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
            if (TCP.ThatOneVariableThatSaysIfWeAreDone)
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

    public class Final_check_Tcp : SimpleProcess
    {
        [InputBus]
        public IBus_ITCP_RuleVerdict[] busList;

        [OutputBus]
        public IBus_finalVerdict_tcp_In final_say = Scope.CreateBus<IBus_finalVerdict_tcp_In>();

        // Class Variables
        bool my_bool = false;

        // Constructor         
        public Final_check_Tcp(IBus_ITCP_RuleVerdict[] busList_in)
        {
            busList = busList_in;
        }

        protected override void OnTick()
        {
            final_say.Valid = false;
            final_say.Accept_or_deny = false;
            if (busList[0].IsSet)
            {
                // Checks if any rule process returns TRUE.
                //my_bool = busList.Any(val => val.Accepted);
                my_bool = busList.AsParallel().Any(val => val.Accepted);
                final_say.Valid = true;

                // Accept the incoming package
                if (my_bool)
                {
                    final_say.Accept_or_deny = true;
                    Console.WriteLine("The package was Accepted");
                }
                // Deny the incoming package, as the IP was not on the whitelist.
                else
                {
                    Console.WriteLine("The package was Denied");
                    final_say.Accept_or_deny = false;
                }
                my_bool = false;
            }
        }
    }
}
