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

        public int[] ip_str_to_int_array(string ip)
        {
            int[] ip_array = new int[4];
            ip_array = ip.Split('.').Select(Int32.Parse).ToArray();
            return ip_array;
        }
    }
}
