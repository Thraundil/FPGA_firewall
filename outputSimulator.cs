using System.IO;
using SME;

namespace simplePackageFilter
{
    public class OutputSimulator : SimulationProcess
    {
        [OutputBus, InputBus]
        public IBus_IPv4_out ipv4 = Scope.CreateBus<IBus_IPv4_In>();

        // Used to read input from a .txt file
        byte[] sample;

        public async override System.Threading.Tasks.Task Run()
        {
            sample = File.ReadAllBytes("../../input_data/ipv4_out_bytes.txt");

            var stream = new MemoryStream(sample, 0, sample.Length);
            var reader = new BinaryReader(stream);

            // Reads a single byte each clockcykle, and
            // updates the Toplevel Inputbus Bus_IPv4

            // reads every byte up until the source and destination IP's
            int length = (int)reader.BaseStream.Length;
            for (int j = 0; j < length / 10; j++)
            {
                // FROM IP
                for (int i = 0; i < 4; i++)
                {
                    ipv4.SourceIP[i] = reader.ReadByte();
                    reader.ReadByte();
                    await ClockAsync();
                }

                // TO IP
                for (int i = 0; i < 4; i++)
                {
                    ipv4.DestIP[i] = reader.ReadByte();
                    reader.ReadByte();
                    await ClockAsync();
                }

                // Port
                ipv4.SourcePort = reader.ReadByte();
                await ClockAsync();

                // Syn/Ack etc
                ipv4.Flags = reader.ReadByte();
                await ClockAsync();

                ipv4.ThatOneVariableThatSaysIfWeAreDone = true;
                await ClockAsync();
                ipv4.ThatOneVariableThatSaysIfWeAreDone = false;
            }
        }
    }
}
