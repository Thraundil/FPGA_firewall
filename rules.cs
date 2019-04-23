using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simplePackageFilter
{
    public class Rules
    {

        public string[] accepted_ports = File.ReadAllLines("../../input_data/accepted_ports.txt");
        public string[] accepted_sources = File.ReadAllLines("../../input_data/accepted_sourceIP.txt");
        public string[] accepted_destinations = File.ReadAllLines("../../input_data/accepted_destinationIP.txt");

        public int[] Ip_str_to_int_array(string ip)
        {
            int[] ip_array = new int[4];
            ip_array = ip.Split('.').Select(Int32.Parse).ToArray();
            return ip_array;
        }

        public (int[], int[]) Ip_str_to_byte_array(string ip)
        {
            string[] ip_array = new string[2];
            int[] low = new int[4];
            int[] high = new int[4];
            byte[] byte_low = new byte[4];
            byte[] byte_high = new byte[4];

            // Splits 'x.x.x.x-y.y.y.y' in two
            ip_array = ip.Split('-');
            // Splits both strings by '.' into lists, converts to int
            low = ip_array[0].Split('.').Select(Int32.Parse).ToArray();
            high = ip_array[1].Split('.').Select(Int32.Parse).ToArray();
            // Converts int lists to byte lists
            byte_low = low.Select(i => (byte)i).ToArray();
            byte_high = high.Select(i => (byte)i).ToArray();

            return (low, high);
        }

        public (int[], int[]) Get_sources(int x)
        {
            return (Ip_str_to_byte_array(this.accepted_sources[x]));
        }

    }
}
