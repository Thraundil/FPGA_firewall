using System;
using System.IO;
using SME;

namespace simplePackageFilter
{
    public class TcpSimulator : SimulationProcess
    {
        [InputBus]
        public tcp_verdict_to_sim ready = Scope.CreateOrLoadBus<tcp_verdict_to_sim>();

        [OutputBus]
        public IBus_ITCP_In tcpBus = Scope.CreateBus<IBus_ITCP_In>();

        // Used to read input from a .txt file
        byte[] sample;

        public async override System.Threading.Tasks.Task Run()
        {
            sample = File.ReadAllBytes("../../input_data/tcp_incoming_bytes");

            var stream = new MemoryStream(sample, 0, sample.Length);
            var reader = new BinaryReader(stream);

            // reads every byte up until the source and destination IP's
            int length = (int)reader.BaseStream.Length;
            for (int j = 0; j < length / 10; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    tcpBus.SourceIP[i] = reader.ReadByte();
                    await ClockAsync();
                }
                for (int i = 0; i < 4; i++)
                {
                    tcpBus.DestIP[i] = reader.ReadByte();
                    await ClockAsync();
                }

                // Port
                tcpBus.Port = reader.ReadByte();
                await ClockAsync();

                // Tcp status (syn, ack, etc..)
                tcpBus.Flags = reader.ReadByte();
                await ClockAsync();

                tcpBus.ThatOneVariableThatSaysIfWeAreDone = true;
                await ClockAsync();
                tcpBus.ThatOneVariableThatSaysIfWeAreDone = false;

                await ClockAsync();
                await ClockAsync();
            }
        }
    }
}
