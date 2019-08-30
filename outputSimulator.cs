using System.IO;
using System;
using SME;

namespace simplePackageFilter
{
    public class OutputSimulator : SimulationProcess
    {

        [InputBus]
        public out_verdict_to_sim from_out = Scope.CreateOrLoadBus<out_verdict_to_sim>();

        [OutputBus]
        public IBus_Blacklist_out ipv4 = Scope.CreateBus<IBus_Blacklist_out>();

        // Used to read input from a .txt file
        byte[] sample;

        public async override System.Threading.Tasks.Task Run()
        {
            sample = File.ReadAllBytes("../../input_data/ipv4_outgoing_bytes");

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
                    await ClockAsync();
                }

                // TO IP
                for (int i = 0; i < 4; i++)
                {
                    ipv4.DestIP[i] = reader.ReadByte();
                    await ClockAsync();
                }

                // Port
                ipv4.SourcePort = reader.ReadByte();
                await ClockAsync();

                // Syn/Ack etc
                ipv4.Flags = reader.ReadByte();
                await ClockAsync();

                ipv4.ReadyToWorkFlag = true;
                await ClockAsync();
                ipv4.ReadyToWorkFlag = false;
                await ClockAsync();
                await ClockAsync();

            }
        }
    }
}
