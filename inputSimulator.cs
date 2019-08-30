using System.IO;
using SME;

namespace simplePackageFilter
{
    public class InputSimulator : SimulationProcess
    {
        [InputBus]
        public ipv4_verdict_to_sim can_we_send = Scope.CreateOrLoadBus<ipv4_verdict_to_sim>();

        [OutputBus]
        public IBus_IPv4_In ipv4 = Scope.CreateBus<IBus_IPv4_In>();

        // Used to read input from a .txt file
        byte[] sample;

        public async override System.Threading.Tasks.Task Run()
        {
            sample = File.ReadAllBytes("../../input_data/ipv4_incoming_bytes");

            var stream = new MemoryStream(sample, 0, sample.Length);
            var reader = new BinaryReader(stream);

            // Reads a single byte each clockcykle, and
            // updates the Toplevel Inputbus Bus_IPv4

            // reads every byte up until the source and destination IP's

            // We collect the entire packet, we wait until we can send and then we send it
            int length = (int)reader.BaseStream.Length;
            for (int j = 0; j < length / 8; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    ipv4.SourceIP[i] = reader.ReadByte();
                    await ClockAsync();
                }
                for (int i = 0; i < 4; i++)
                {
                    ipv4.DestIP[i] = reader.ReadByte();
                    await ClockAsync();
                }

                // We have to wait for the ipv4 to be ready to recieve the next package
                while(!can_we_send.ipv4_ready_flag)
                {
                    await ClockAsync();
                }

                ipv4.ClockCheck = true;
                await ClockAsync();

                ipv4.ClockCheck = false;
                await ClockAsync();
                await ClockAsync();
            }
        }
    }
}
