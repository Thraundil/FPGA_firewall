using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using simplePackageFilter;
using SME;

namespace sme_example
{
    public class inputSimulator : SimulationProcess
    {
        [OutputBus]
        public IPv4_Simple ipv4 = Scope.CreateBus<IPv4_Simple>();

        [OutputBus]
        public ITCP TCP = Scope.CreateBus<ITCP>();

        // Used to read input from a .txt file
        byte[] sample;
        byte[] TCP_package;

        public async override System.Threading.Tasks.Task Run()
        {
            sample = File.ReadAllBytes("../../ipv4_bytes.txt");

            TCP_package = File.ReadAllBytes("../../tcp_bytes.txt");


            var stream = new MemoryStream(sample, 0, sample.Length);
            var reader = new BinaryReader(stream);

            var stream1 = new MemoryStream(TCP_package, 0, TCP_package.Length);
            var reader1 = new BinaryReader(stream1);

            // Reads a single byte each clockcykle, and
            // updates the Toplevel Inputbus IPv4_Simple
            ipv4.Header = reader.ReadByte();
            await ClockAsync();
            ipv4.Diff = reader.ReadByte();
            await ClockAsync();
            for (int i = 0; i < 2; i++)
            {
                ipv4.Length[i] = reader.ReadByte();
                await ClockAsync();
            }
            for (int i = 0; i < 2; i++)
            {
                ipv4.Id[i] = reader.ReadByte();
                await ClockAsync();
            }
            for (int i = 0; i < 2; i++)
            {
                ipv4.Flags[i] = reader.ReadByte();
                await ClockAsync();
            }
            ipv4.Ttl = reader.ReadByte();
            await ClockAsync();
            ipv4.Protocol = reader.ReadByte();
            await ClockAsync();
            for (int i = 0; i < 2; i++)
            {
                ipv4.Checksum[i] = reader.ReadByte();
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


            // TCP Packet begins //
            for (int i = 0; i < 2; i++)
            {
                TCP.Source_Port[i] = reader1.ReadByte();
                await ClockAsync();
            }
            for (int i = 0; i < 2; i++)
            {
                TCP.Dest_Port[i] = reader1.ReadByte();
                await ClockAsync();

            }
            for (int i = 0; i < 4; i++)
            {
                TCP.Sequence_number[i] = reader1.ReadByte();
                await ClockAsync();

            }
            for (int i = 0; i < 4; i++)
            {
                TCP.Ack_number[i] = reader1.ReadByte();
                await ClockAsync();

            }
            for (int i = 0; i < 2; i++)
            {
                TCP.junk[i] = reader1.ReadByte();
                await ClockAsync();

            }
            for (int i = 0; i < 2; i++)
            {
                TCP.Window_size[i] = reader1.ReadByte();
                await ClockAsync();

            }
            for (int i = 0; i < 2; i++)
            {
                TCP.Checksum[i] = reader1.ReadByte();
                await ClockAsync();

            }
            for (int i = 0; i < 2; i++)
            {
                TCP.Urgent_pointer[i] = reader1.ReadByte();
                await ClockAsync();

            }
            for (int i = 0; i < 20; i++)
            {
                TCP.Optional_data[i] = reader1.ReadByte();
                await ClockAsync();

            }

            ipv4.clockCheck = true;
            await ClockAsync();
            ipv4.clockCheck = false;
        }
    }
}
