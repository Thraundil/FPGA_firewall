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
            _ = new Print();

            // Number of rules, 
            int len_sources = rules.accepted_sources.Length;
            int len_blacklist = rules.blacklisted_destinations.Length;
            int max_number_connections = 10;
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
                IBus_ruleVerdict_In[] Bus_array_IP_whitelist = new IBus_ruleVerdict_In[len_sources];

                // Fills up the Whitelisted src/dest IP rules into the above array
                for (int i = 0; i < len_sources; i++)
                {
                    var (low_src, high_src) = rules.Get_sources(i);
                    var (low_dest, high_dest) = rules.Get_destination(i);
                    var temptemp = new Rule_Process(ipv4_in.ipv4, tcp_in.tcpBus, low_src, high_src, low_dest, high_dest);
                    Bus_array_IP_whitelist[i] = temptemp.ruleVerdict;
                }

                // The Blacklisted IP rules
                IBus_blacklist_ruleVerdict_out[] Bus_array_IP_blacklist = new IBus_blacklist_ruleVerdict_out[len_blacklist];

                // Fills up the Blacklisted Destination IP rules, into above array
                for (int i = 0; i < len_blacklist; i++)
                {
                    var (low_dest, high_dest) = rules.Get_blacklisted_destinations(i);
                    var temptemp = new Rule_Process_Blacklist(ipv4_out.ipv4, low_dest, high_dest);
                    Bus_array_IP_blacklist[i] = temptemp.ruleVerdict;
                }

                // Blacklist simulator
                // var Final_verdict_blacklist = new Final_check_Blacklist(Bus_array_IP_blacklist);

                //                Bus array for the exsisting connections
                IBus_ITCP_RuleVerdict[] Bus_array_connections = new IBus_ITCP_RuleVerdict[max_number_connections];
                IBus_Connection_In_Use[] process_in_use = new IBus_Connection_In_Use[max_number_connections];

                // Empty byte-array for the Connection_process
                var emptyByteArray = new byte[4];
                for (int i = 0; i < emptyByteArray.Length; i++)
                {
                    emptyByteArray[i] = 0x00;
                }


                for (int i = 0; i < max_number_connections; i++)
                {
                    // Inizialise every process to some default value that can never be matched
                    var temp = new Connection_process(emptyByteArray, emptyByteArray, 0, i, tcp_in.tcpBus, ipv4_in.ipv4, ipv4_out.ipv4);
                    process_in_use[i] = temp.in_use;
                    Bus_array_connections[i] = temp.ruleVerdict;
                }

                var final_verdict_tcP = new Final_check_state(Bus_array_connections, Bus_array_IP_whitelist, Bus_array_IP_blacklist, ipv4_out.ipv4, tcp_in.tcpBus);

                sim.Run();
            }
        }
    }
}
