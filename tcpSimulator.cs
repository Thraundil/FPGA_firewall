using System.IO;
using SME;

namespace simplePackageFilter
{
    public class InputSimulator : SimulationProcess
    {
        [OutputBus, InputBus]
        public IBus_ITCP_In tcpBus = Scope.CreateBus<ITCP_In>();

        // Used to read input from a .txt file
        byte[] sample;

        public async override System.Threading.Tasks.Task Run()
        {
            sample = File.ReadAllBytes("../../input_data/tcp.txt");

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
            }
        }
    }
}
