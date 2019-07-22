using System;
using SME;

namespace simplePackageFilter
{
    public static class ByteArrayToUint
    {
        public static uint convert(byte[] byte_array)
        {
            // Bitshifting with 24,16, and 8 respectivly
            uint UintResult =  (uint) (((byte_array[0] << 24)) |
                                    ((byte_array[1] << 16)) |
                                    ((byte_array[2] << 8)) |
                                    ((byte_array[3])));
            return UintResult;
        }

        public static uint convertIFixed(IFixedArray<byte> byte_array)
        {
            // Bitshifting with 24,16, and 8 respectivly
            uint UintResult =  (uint) (((byte_array[0] << 24)) |
                                    ((byte_array[1] << 16)) |
                                    ((byte_array[2] << 8)) |
                                    ((byte_array[3])));
            return UintResult;
        }

    }
}
