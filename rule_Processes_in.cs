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

        // An argument is needed, as VHDL does not allow function calls without an argument...?!
        private bool IsIPinRange(byte[] low_source, byte[] high_source, byte[] low_dest, byte[] high_dest, IFixedArray<byte> ip_source, IFixedArray<byte> ip_dest)
        {

            bool doesItMatch = true;
            int x = 0;

            // TO BE PARALLELISED
            // Src Low
            while (x < low_source.Length) {
                if (low_source[x] < ip_source[x]){
                    x = low_source.Length;
                }
                else if (low_source[x] == ip_source[x]) {
                    x++;
                }
                else {
                    doesItMatch = false;
                    x = low_source.Length;
                }
            }

            x = 0;

            // Src High
            while (x < high_source.Length) {
                if (high_source[x] > ip_source[x]){
                    x = high_source.Length;
                }
                else if (high_source[x] == ip_source[x]) {
                    x++;
                }
                else{
                    doesItMatch = false;
                    x = high_source.Length;
                }
            }

            x = 0;

            // Dest Low
            while (x < low_dest.Length) {
                if (low_dest[x] < ip_dest[x]){
                    x = low_dest.Length;
                }
                else if (low_dest[x] == ip_dest[x]) {
                    x++;
                }
                else{
                    doesItMatch = false;
                    x = low_dest.Length;
                }
            }

            x = 0;

            // Dest High
            while (x < high_dest.Length) {
                if (high_dest[x] > ip_dest[x]){
                    x = high_dest.Length;
                }
                else if (high_dest[x] == ip_dest[x]) {
                    x++;
                }
                else{
                    doesItMatch = false;
                    x = high_dest.Length;
                }
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


