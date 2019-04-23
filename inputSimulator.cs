using System.IO;
using SME;

namespace simplePackageFilter
{
    public class inputSimulator : SimulationProcess
    {
        [OutputBus]
        public IBus_IPv4 ipv4 = Scope.CreateBus<IBus_IPv4>();

        // Used to read input from a .txt file
        byte[] sample;

        public async override System.Threading.Tasks.Task Run()
        {
            sample = File.ReadAllBytes("../../input_data/ipv4_bytes.txt");

            var stream = new MemoryStream(sample, 0, sample.Length);
            var reader = new BinaryReader(stream);

            // Reads a single byte each clockcykle, and
            // updates the Toplevel Inputbus Bus_IPv4

            // reads every byte up until the source and destination IP's
            for (int i = 0; i < 12; i++)
            {
                reader.ReadByte();
                await ClockAsync();
            }
            for (int i = 0; i < 4; i++)
            {
                ipv4.SourceIP[i] = reader.ReadByte();
                await ClockAsync();
            }
            for (int i = 0; i < 4; i++)
            {
                ipv4.DestinationIP[i] = reader.ReadByte();
                await ClockAsync();
            }


            ipv4.clockCheck = true;
            await ClockAsync();
            ipv4.clockCheck = false;
        }
    }
}
