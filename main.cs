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
            int max_number_connections = 1000;
            _ = rules.accepted_destinations.Length;

            using (var sim = new Simulation())
            {
                sim
                    .BuildCSVFile();
                // CARL/KENNETH FIX PLZ
                //                    .BuildVHDL();

                // Creates 3 classes, each with their own uses
                var ipv4_in = new InputSimulator();
                var tcp_input = new tcp_in_simulator();
                //var output = new output_simulator();

                // Bus array for each rule to write a bus to

                IBus_ruleVerdict_In[] Bus_array_sources = new IBus_ruleVerdict_In[len_sources];

                // The bus loop, in which the above array is filled
                for (int i = 0; i < len_sources; i++)
                {
                    var (low_src, high_src) = rules.Get_sources(i);
                    var (low_dest, high_dest) = rules.Get_destination(i);
                    var temptemp = new Rule_Process(ipv4_in.ipv4, low_src, high_src, low_dest, high_dest);
                    Bus_array_sources[i] = temptemp.ruleVerdict;
                }

                // Bus array for the exsisting connections

                IBus_ITCP_RuleVerdict[] Bus_array_connections = new IBus_ITCP_RuleVerdict[max_number_connections];
                for (int i = 0; i < max_number_connections; i++)
                {
                    // Inizialise every process to some default value that can never be matched
                    var temp = new Connection_process(tcp_input.tcp, 0, 0, 0, 0,0,0);
                    Bus_array_connections[i] = temp.ruleVerdict;
                }


                var Final_verdict = new Final_check(Bus_array_sources);


                // Prints a file, for testing purposes
                // print.print_file(rules.accepted_sources);
                sim.Run();
            }
        }
    }
}
