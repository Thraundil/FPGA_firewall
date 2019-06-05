using SME;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Net;

namespace simplePackageFilter
{


    // ****************************************************************************
    [ClockedProcess]
    public class Rule_Process : SimpleProcess
    {

        // Input bus from the ipv4 header check
        [InputBus]
        public IBus_IPv4_In ipv4;

        // Input but from the stateful check
        [InputBus]
        public IBus_ITCP_In stateful_in;

        [OutputBus]
        public IBus_ruleVerdict_In ruleVerdict = Scope.CreateBus<IBus_ruleVerdict_In>();

        // IP source range low/high as a byte array
        private  byte[] ip_low_source { get; set; }
        private  byte[] ip_high_source { get; set; }

        // IP destination range low/high as a byte array
        private  byte[] ip_low_dest { get;  set; }
        private  byte[] ip_high_dest { get; set; }


        // ipv4Reader_Constructor
        public Rule_Process(IBus_IPv4_In busIn, IBus_ITCP_In tcpin, byte[] ip_low_source_in, byte[] ip_high_source_in, byte[] ip_low_dest_in, byte[] ip_high_dest_in)
        {
            ipv4 = busIn;
            stateful_in = tcpin;
            ip_low_source = ip_low_source_in;
            ip_high_source = ip_high_source_in;
            ip_low_dest = ip_low_dest_in;
            ip_high_dest = ip_high_dest_in;
        }

        // Compares if incoming IP 'destination' is in the whitelisted range.
        private bool IsIPinRange(byte[] low_source, byte[] high_source, byte[] low_dest, byte[] high_dest, IFixedArray<byte> ip_source, IFixedArray<byte> ip_dest)
        {

            bool doesItMatch = false;

            // The ranges
            uint low_source_uint  = ByteArrayToUint.convert(low_source);
            uint high_source_uint = ByteArrayToUint.convert(high_source);
            uint low_dest_uint    = ByteArrayToUint.convert(low_dest);
            uint high_dest_uint   = ByteArrayToUint.convert(high_dest);

            // The incoming IP source/dest
            uint incoming_source_uint = ByteArrayToUint.convertIFixed(ip_source);
            uint incoming_dest_uint   = ByteArrayToUint.convertIFixed(ip_dest);

            if ((low_source_uint <= incoming_source_uint && incoming_source_uint <= high_source_uint) &&
                (low_dest_uint <= incoming_dest_uint && incoming_dest_uint <= high_dest_uint)) {
                doesItMatch = true;
            }

            return doesItMatch;
        }


        // On Tick (ipv4Readers 'main')
        protected override void OnTick()
        {
            ruleVerdict.tcp_Accepted = false;
            ruleVerdict.tcp_IsSet = false;
            ruleVerdict.ipv4_IsSet = false;
            ruleVerdict.ipv4_Accepted = false;
            if (ipv4.ClockCheck)
            {
                if (IsIPinRange(ip_low_source, ip_high_source, ip_low_dest, ip_high_dest, ipv4.SourceIP, ipv4.DestIP))
                {
                    ruleVerdict.ipv4_Accepted = true;
                }
                ruleVerdict.ipv4_IsSet = true;
                //sourceComparePort(allowed_ports);
            }
            if(stateful_in.ThatOneVariableThatSaysIfWeAreDone)
            {
                ruleVerdict.tcp_IsSet = true;
                if (IsIPinRange(ip_low_source, ip_high_source, ip_low_dest, ip_high_dest, stateful_in.SourceIP, stateful_in.DestIP))
                {
                    ruleVerdict.tcp_Accepted = true;
                }
            }
        }
    }

    // ****************************************************************************

    // Main
}


