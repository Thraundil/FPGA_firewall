using System;


namespace simplePackageFilter
{
    public static class Print
    {
        public static void print_int_array(int[] int_array)
        {
            for (int i = 0; i < int_array.Length; i++)
            {
                Console.Write(int_array[i] + " ");
            }
            Console.WriteLine("");
        }

        public static void print_byte_array(byte[] byte_array)
        {
            for (int i = 0; i < byte_array.Length; i++)
            {
                Console.Write(byte_array[i] + " ");
            }
            Console.WriteLine("");
        }

    }
}
