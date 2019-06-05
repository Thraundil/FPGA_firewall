using System;
using SME;

namespace simplePackageFilter
{
    public static class ByteArrayToUint
    {
        public static uint convert(byte[] byte_array)
        {
            uint UintResult =  (uint) (((byte_array[0] * 0x1000000)) +
                                    ((byte_array[1] * 0x10000)) +
                                    ((byte_array[2] * 0x100)) +
                                    ((byte_array[3])));
            return UintResult;
        }

        public static uint convertIFixed(IFixedArray<byte> byte_array)
        {
            uint UintResult =  (uint) (((byte_array[0] * 0x1000000)) +
                                    ((byte_array[1] * 0x10000)) +
                                    ((byte_array[2] * 0x100)) +
                                    ((byte_array[3])));
            return UintResult;
        }

    }
}
