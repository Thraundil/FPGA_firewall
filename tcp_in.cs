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

        int Port { get; set; }

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

        private long ip_source { get; set; }
        private long ip_dest { get; set; }

        private int port_in { get; set; }

        readonly long doubl = (65536);    // 256*256
        readonly long triple = (16777216); // 256*256*256
        public Connection_process(IBus_ITCP_In busIn, long ip_source_in, long ip_dest_in, int port)
        {
            TCP = busIn;
            port_in = port;
            ip_source = ip_source_in;
            ip_dest = ip_dest_in;
        }


        private bool DoesConnectExist(long source, long dest, int port, long tcp_source, long tcp_dest)
        {
            if (source == tcp_source)
            {
                if (dest == tcp_dest)
                {
                    if (port == TCP.Port)
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
                if (DoesConnectExist(ip_source, ip_dest, port_in, tcp_source, tcp_dest))
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
        public IBus_ITCP_RuleVerdict[] connection_list;

        [InputBus]
        public IBus_ruleVerdict_In[] rule_list;

        [OutputBus]
        public IBus_finalVerdict_tcp_In final_say = Scope.CreateBus<IBus_finalVerdict_tcp_In>();

        // Class Variables
        bool connection_bool = false;

        bool rule_bool = false;

        // Constructor         
        public Final_check_Tcp(IBus_ITCP_RuleVerdict[] busList_in, IBus_ruleVerdict_In[] rule_list_in )
        {
            connection_list = busList_in;
            rule_list = rule_list_in;

        }

        protected override void OnTick()
        {
            final_say.Valid = false;
            final_say.Accept_or_deny = false;
            if (connection_list[0].IsSet)
            {
                // Checks if any rule process returns TRUE.
                //my_bool = busList.Any(val => val.Accepted);
                connection_bool = connection_list.AsParallel().Any(val => val.Accepted);
                final_say.Valid = true;

                // Accept the incoming package
                if (connection_bool)
                {
                    final_say.Accept_or_deny = true;
                    Console.WriteLine("The connection already exists");
                }

                else
                {
                    rule_bool = rule_list.AsParallel().Any(val => val.Accepted);
                    if (rule_bool) // And some flags are correct - in parituclar the syn flag
                    {
                        Console.WriteLine("The connection does not exist but matches a whitelisted rule");
                        final_say.Accept_or_deny = true;
                    }
                    else
                    {
                        Console.WriteLine("The connection does neither match a connection or a rule");
                        final_say.Accept_or_deny = false;
                    }
                }
                connection_bool = false;
                rule_bool = false;
            }
        }
    }
}
