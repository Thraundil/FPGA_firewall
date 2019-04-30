using SME;

namespace simplePackageFilter
{
    public class Program
    {
        // Main
        static void Main()
        {
            // General classes, compiled before simulation
            var rules = new Rules();
            _ = new Print();

            // Number of rules, 
            int len_sources = rules.accepted_sources.Length;
            _ = rules.accepted_destinations.Length;

            using (var sim = new Simulation())
            {
                sim
                    .BuildCSVFile();
                // CARL/KENNETH FIX PLZ
                //                    .BuildVHDL();

                // Creates 3 classes, each with their own uses
                var byte_input = new InputSimulator();

                // Bus array for each rule to write a bus to
                IBus_ruleVerdict[] Bus_array_sources = new IBus_ruleVerdict[len_sources];

                // The bus loop, in which the above array is filled
                for (int i = 0; i < len_sources; i++)
                {
                    var (low_src, high_src) = rules.Get_sources(i);
                    var (low_dest, high_dest) = rules.Get_destination(i);
                    var temptemp = new Rule_Process(byte_input.ipv4, low_src, high_src, low_dest, high_dest);
                    Bus_array_sources[i] = temptemp.ruleVerdict;
                }

                // The final verdict, checking if any rule process accepted the IPV4 source.
                var Final_verdict = new Final_check(Bus_array_sources);


                sim.Run();
            }
        }
    }
}