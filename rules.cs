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

        public (long, long) Ip_str_to_long_tuple(string ip)
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

            // Converts the IP address into a Long, for easier comparisons used later on
            long doubl     = (65536);    // 256*256
            long triple    = (16777216); // 256*256*256
            long low_long  = low[3] + (low[2] * 256) + (low[1] * doubl) + (low[0] * triple);
            long high_long = high[3] + (high[2] * 256) + (high[1] * doubl) + (high[0] * triple);

            return (low_long, high_long);
        }

        public (long, long) Get_sources(int x)
        {
            return (Ip_str_to_long_tuple(this.accepted_sources[x]));
        }

    }
}
