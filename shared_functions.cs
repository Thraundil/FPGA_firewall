using System;


namespace simplePackageFilter
{
    public static class Shared_Functions
    {
        // Compares if incoming IP 'destination' is in the whitelisted range.
        public static bool IsIPinRange(byte[] low_source, byte[] high_source, byte[] low_dest,
                                 byte[] high_dest, IFixedArray<byte> ip_source, IFixedArray<byte> ip_dest)
        {
            // The return boolean starts false
            bool doesItMatch = false;

            // The whitelisted source/dest ranges
            uint low_source_uint  = ByteArrayToUint.convert(low_source);
            uint high_source_uint = ByteArrayToUint.convert(high_source);
            uint low_dest_uint    = ByteArrayToUint.convert(low_dest);
            uint high_dest_uint   = ByteArrayToUint.convert(high_dest);

            // The incoming IP source/dest
            uint incoming_source_uint = ByteArrayToUint.convertIFixed(ip_source);
            uint incoming_dest_uint   = ByteArrayToUint.convertIFixed(ip_dest);

            // Comparing if the incoming Source/Dest is WITHIN the legal whitelist range
            if ((low_source_uint <= incoming_source_uint && incoming_source_uint <= high_source_uint) &&
                (low_dest_uint <= incoming_dest_uint && incoming_dest_uint <= high_dest_uint)) {
                doesItMatch = true;
            }

            // Returns TRUE if the incoming src/dst was in the whitelisted range
            return doesItMatch;
        }

    // ****************************************************************************

        public static bool DoesConnectExist(byte[] source, byte[] dest, int port,IFixedArray<byte> incoming_source,
                                            IFixedArray<byte> incoming_dest, int incoming_port)
        {
            bool doesItMatch = false;

            uint source_uint          = ByteArrayToUint.convert(source);
            uint dest_uint            = ByteArrayToUint.convert(dest);
            uint incoming_source_uint = ByteArrayToUint.convertIFixed(incoming_source);
            uint incoming_dest_uint   = ByteArrayToUint.convertIFixed(incoming_dest);

            if (((source_uint == incoming_source_uint) && (dest_uint == incoming_dest_uint)) && (port == incoming_port)) {
                doesItMatch = true;
            }

            return doesItMatch;
        }

    // ****************************************************************************

        public static bool ipv4_checker(byte[] source, byte[] dest, IFixedArray<byte> incoming_source,
                                        IFixedArray<byte> incoming_dest)
        {
            bool doesItMatch = false;

            uint source_uint          = ByteArrayToUint.convert(source);
            uint dest_uint            = ByteArrayToUint.convert(dest);
            uint incoming_source_uint = ByteArrayToUint.convertIFixed(incoming_source);
            uint incoming_dest_uint   = ByteArrayToUint.convertIFixed(incoming_dest);

            if ((source_uint == incoming_source_uint) && (dest_uint == incoming_dest_uint)) {
                doesItMatch = true;
            }

            return doesItMatch;
        }
    }
}
