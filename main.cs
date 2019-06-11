using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SME;

namespace simplePackageFilter
{
    public class Program
    {
        static void Main()
        {
            // General classes, compiled before simulation
            var rules = new Rules();
//            _ = new Print();

            // Number of rules, 
            int len_sources = rules.accepted_sources.Length;
            int len_blacklist = rules.blacklisted_destinations.Length;
            int max_number_connections = 20;
            _ = rules.accepted_destinations.Length;


            using (var sim = new Simulation())
            {
                sim
                    .BuildCSVFile();
                // CARL/KENNETH FIX PLZ
                //                    .BuildVHDL();

                // The input-simulators (for simulating actual intput/output for tests)
                var ipv4_in  = new InputSimulator();
                var ipv4_out = new OutputSimulator();
                var tcp_in   = new TcpSimulator();

                // The Whitelisted IP rules 
                IBus_Rule_Verdict_IPV4[] Bus_array_IP_whitelist_ipv4 = new IBus_Rule_Verdict_IPV4[len_sources];
                IBus_Rule_Verdict_TCP[] Bus_array_IP_whitelist_tcp = new IBus_Rule_Verdict_TCP[len_sources];
                IBus_blacklist_ruleVerdict_out[] Bus_array_IP_blacklist = new IBus_blacklist_ruleVerdict_out[len_blacklist];


                // Fills up the Whitelisted src/dest IP rules into the above array
                for (int i = 0; i < len_sources; i++)
                {
                    var (low_src, high_src) = rules.Get_sources(i);
                    var (low_dest, high_dest) = rules.Get_destination(i);

                    var temptemp = new Rule_Process_IPV4(ipv4_in.ipv4, low_src, high_src, low_dest, high_dest);
                    var temptemp1 = new Rule_Process_TCP(tcp_in.tcpBus, low_src, high_src, low_dest, high_dest);

                    Bus_array_IP_whitelist_ipv4[i] = temptemp.ruleVerdict;
                    Bus_array_IP_whitelist_tcp[i] = temptemp1.ruleVerdict;
                }


                // Fills up the Blacklisted Destination IP rules, into above array
                for (int i = 0; i < len_blacklist; i++)
                {
                    var (low_dest, high_dest) = rules.Get_blacklisted_destinations(i);
                    var temptemp = new Rule_Process_Blacklist(ipv4_out.ipv4, low_dest, high_dest);
                    Bus_array_IP_blacklist[i] = temptemp.ruleVerdict;
                }

                // Bus array for the exsisting connections

                IBus_Process_Verdict_IPV4[] ipv4_connections = new IBus_Process_Verdict_IPV4[max_number_connections];
                IBus_Process_Verdict_TCP[] tcp_connections = new IBus_Process_Verdict_TCP[max_number_connections];
                IBus_Process_Verdict_Outgoing[] outgoing_connections = new IBus_Process_Verdict_Outgoing[max_number_connections];


                IBus_Connection_In_Use[] process_in_use = new IBus_Connection_In_Use[max_number_connections];

                for (int i = 0; i < max_number_connections; i++)
                {
                    // Inizialise every process to some default value that can never be matched
                    var temp_ipv4 = new Connection_process_IPV4_incoming(new byte[4] { 0x00, 0x00, 0x00, 0x00 }, new byte[4] { 0x00, 0x00, 0x00, 0x00 },i,ipv4_in.ipv4);
                    var temp_tcp = new Connection_process_TCP_incoming(new byte[4] { 0x00, 0x00, 0x00, 0x00 }, new byte[4] { 0x00, 0x00, 0x00, 0x00 }, 0, i, tcp_in.tcpBus);
                    var temp_out = new Connection_process_outgoing(new byte[4] { 0x00, 0x00, 0x00, 0x00 }, new byte[4] { 0x00, 0x00, 0x00, 0x00 }, 0, i,ipv4_out.ipv4);

                    process_in_use[i] = temp_tcp.in_use;
                    tcp_connections[i] = temp_tcp.ruleVerdict;
                    ipv4_connections[i] = temp_ipv4.ruleVerdict;
                    outgoing_connections[i] = temp_out.ruleVerdict;

                }

                var final_verdict_tcP = new stateful_state_verdict(tcp_connections, Bus_array_IP_whitelist_tcp, tcp_in.tcpBus);
                var Ipv4_state_verdict = new Ipv4_state_verdict(ipv4_connections, ipv4_in.ipv4, Bus_array_IP_whitelist_ipv4);
                var final_verdict_outgoing = new out_state_verdict(Bus_array_IP_blacklist, outgoing_connections,ipv4_out.ipv4);
                sim.Run();
            }
        }
    }
}
