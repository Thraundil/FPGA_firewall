using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simplePackageFilter
{
    public class Print
    {
        public void print_int_array(int[] int_array)
        {
            for (int i = 0; i < int_array.Length; i++)
            {
                Console.Write(int_array[i] + " ");
            }
            Console.WriteLine("");
        }
    }
}
