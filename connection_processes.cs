using System;
using SME;

namespace simplePackageFilter
{
    [ClockedProcess]
    public class Connection_process_IPV4_incoming : SimpleProcess
    {

        // Incoming IPV4
        [InputBus]
        public IBus_IPv4_In ipv4_in;

        // Input bus to check if we need to update state
        [InputBus]
        private readonly IBus_Update_State_tcp update_tcp = Scope.CreateOrLoadBus<IBus_Update_State_tcp>();

        [InputBus]
        private readonly IBus_Update_State_out update_out = Scope.CreateOrLoadBus<IBus_Update_State_out>();

        [OutputBus]
        public IBus_Process_Verdict_IPV4 ruleVerdict = Scope.CreateBus<IBus_Process_Verdict_IPV4>();

        // Variable declerations for the constructor
        private byte[] Ip_source { get; set; }
        private byte[] Ip_dest { get; set; }
        private readonly uint my_id;

        private bool connection_in_use;
        public Connection_process_IPV4_incoming(byte[] ip_source_in, byte[] ip_dest_in, uint ids, IBus_IPv4_In ipv4)
        {
            Ip_source = ip_source_in;
            Ip_dest   = ip_dest_in;
            my_id     = ids;
            ipv4_in   = ipv4;
        }

        protected override void OnTick()
        {
            // Set all the potential flags
            ruleVerdict.IsSet_ipv4    = ipv4_in.ClockCheck;
            ruleVerdict.Accepted_ipv4 = false;

            if (update_tcp.Flag && update_tcp.Id == my_id)
            {
                // overwrite it settings here
                connection_in_use = true;

                // Update the state process with incoming information
                for (int k = 0; k<4; k++)
                {
                    Ip_source[k] = update_tcp.SourceIP[k];
                    Ip_dest[k]   = update_tcp.DestIP[k];
                }
            }

            if (update_out.Flag && update_out.Id == my_id)
            {
                // overwrite it settings here
                connection_in_use = true;

                // Update the state process with incoming information
                for (int k = 0; k < 4; k++)
                {
                    Ip_source[k] = update_out.SourceIP[k];
                    Ip_dest[k] = update_out.DestIP[k];
                }
            }

            if (connection_in_use)
            {

                if (ipv4_in.ClockCheck)
                {
                    if(Shared_functions.ipv4_checker(Ip_source, Ip_dest, ipv4_in.SourceIP, ipv4_in.DestIP))
                    {
                        ruleVerdict.Accepted_ipv4 = true;
                    }
                }
            }
        }
    }

// ****************************************************************************

    [ClockedProcess] // ClockedProcess due to having a COUNTER (timer)
    public class Connection_process_TCP_incoming : SimpleProcess
    {

        // Input but from the stateful
        [InputBus]
        public IBus_ITCP_In stateful_in;


        // Input bus to check if we need to update state
        [InputBus]
        private readonly IBus_Update_State_tcp update_tcp = Scope.CreateOrLoadBus<IBus_Update_State_tcp>();

        [InputBus]
        private readonly IBus_Update_State_out update_out = Scope.CreateOrLoadBus<IBus_Update_State_out>();

        [InputBus]
        public IBus_outgoing_to_TCP from_out;

        [OutputBus]
        public IBus_TCP_to_outgoing to_out = Scope.CreateBus<IBus_TCP_to_outgoing>();


        [OutputBus]
        public IBus_Process_Verdict_TCP ruleVerdict = Scope.CreateBus<IBus_Process_Verdict_TCP>();

        [OutputBus]
        public IBus_Connection_In_Use in_use = Scope.CreateBus<IBus_Connection_In_Use>();

        // Needs one more input and output bus
        // It needs to "syncronize" with the respective process from outgoing 

        // Variables for the constructor
        byte[] Ip_source { get; set; }
        byte[] Ip_dest { get; set; }
        uint Port_in { get; set; }
        bool our_turn_to_send;


        private readonly uint my_id;

        private int timeout_counter = 10000;
        private int default_timeout_counter = 10000; // this variable is the variable we can change if we want to change the maximum timeout value
        private bool connection_in_use = false;
        private bool connection_is_established = false;
        uint stage; // To represent the TCP handshake stage. 0 = nothing happened yet. 1 = means syn. 2 = syn-ack. 3 = ack and we are done.
        uint syn_flag_counter; // we will acecpt 5 attemps to recieve the same syn packet before we stop accepting them

        public Connection_process_TCP_incoming(byte[] ip_source_in, byte[] ip_dest_in, uint port, uint ids, IBus_ITCP_In stateful)
        {
            Port_in     = port;
            Ip_source   = ip_source_in;
            Ip_dest     = ip_dest_in;
            my_id       = ids;
            stateful_in = stateful;
            in_use.Id = my_id;
        }

        protected override void OnTick()
        {
            // Set all the potential flags
            // If we get data then our reply is "valid"
            // Maybe change this to = false and then make it true after we check if "thatonevar...." is true
            ruleVerdict.IsSet_state = stateful_in.ThatOneVariableThatSaysIfWeAreDone;
            ruleVerdict.Accepted_state = false;
            to_out.valid = false;
            to_out.end_con = false;

            // Need to check if the imcoming packet is TCP or UDP
            // IF it is TCP then the "connection is established" has to wait for the syn -> syn-ack -> ack
            // whereas if it is UDP then it is established right away
            if (update_tcp.Flag && update_tcp.Id == my_id)
            {

                connection_in_use = true;
                in_use.In_use = true;

                // Update the state process with incoming information
                for (int k = 0; k < 4; k++)
                {
                    Ip_source[k] = update_tcp.SourceIP[k];
                    Ip_dest[k] = update_tcp.DestIP[k];
                }
                Port_in = update_tcp.Port;
                if (update_tcp.is_tcp)
                {
                    stage = 1;
                    our_turn_to_send = false; // they set the flag
                    syn_flag_counter = 5;
                }

                // Meaning it is an UDP packet
                // The connection is considered established when the first UDP packet is seen
                else
                {
                    connection_is_established = true;
                }
            }

            if (update_out.Flag && update_out.Id == my_id)
            {
                // overwrite it settings here
                connection_in_use = true;
                in_use.In_use = true;

                // Update the state process with incoming information
                for (int k = 0; k < 4; k++)
                {
                    Ip_source[k] = update_out.SourceIP[k];
                    Ip_dest[k] = update_out.DestIP[k];
                }
                Port_in = update_out.Port;
                if (update_out.Is_tcp)
                {
                    stage = 1;
                    our_turn_to_send = true;
                    syn_flag_counter = 5;
                }

                // Meaning it is an UDP packet
                // The connection is considered established when the first UDP packet is seen
                else
                {
                    connection_is_established = true;
                }

            }
            if (from_out.valid)
            {
                if (from_out.end_con)
                {
                    connection_in_use = false;
                    in_use.In_use = false;
                    stage = 0;
                }
                else
                {
                    stage = from_out.stage; // maybe just add one?
                    our_turn_to_send = true;  // Simply swap the boolean. Maybe just set to true.
                }
            }

            // If we get data
            // maybe add a check here that the type of connection. Could maybe reuse the stage variable here. 
            // If the stage is none-zero it means it is a tcp connection.
            // just need to make sure that stage get set back to 0 once a tcp connection dies.
            if (stateful_in.ThatOneVariableThatSaysIfWeAreDone && ((stateful_in.is_tcp && stage != 0) || (!stateful_in.is_tcp && stage == 0)))
            {
                // If the connection is both in use and the connecton matches the incoming "packet connection" ports, IPs ect.
                if (connection_in_use && Shared_functions.DoesConnectExist(Ip_source, Ip_dest, Port_in, stateful_in.SourceIP, stateful_in.DestIP, stateful_in.Port))
                {
                    // Convert byte to a bool array
                    var bits_bool = new bool[8];
                    for (int i = 0; i < 8; i++)
                        bits_bool[i] = (stateful_in.Flags & (1 << i)) == 0 ? false : true;
                    Array.Reverse(bits_bool);

                    // Check to see if we are timed out
                    if (timeout_counter == 0)
                    {
                        connection_in_use = false;
                        in_use.In_use = false;
                        stage = 0;
                        to_out.end_con = true;
                        to_out.valid = true;
                        // must signal to the other process that a timeout has occured and that it must reset
                    }
                    // if we are in use we are either established or not establised
                    if (connection_is_established)
                    {
                        // If it is a tcp connection and it has been established(through tcp handshake) then we are only looking for the fin flag
                        if (stateful_in.is_tcp)
                        {
                            // If the fin flag has been set. Check for other flags or nah?
                            if (bits_bool[0])
                            {
                                connection_in_use = false;
                                timeout_counter = default_timeout_counter;
                                stage = 0;
                                syn_flag_counter = 5;
                                in_use.Id = my_id;
                                in_use.In_use = false;
                                // tell the other process that the connection is dead! Dead I say!
                            }
                        }
                        ruleVerdict.Accepted_state = true;

                    }
                    else // Under establishment
                    {
                        // Meaning its our "turn" to receive
                        // Since this process handles inconing data we are only interested if we are "supposed" to receive data
                        if (our_turn_to_send)
                        {
                            // Stage = 0 means nothing happened yet
                            // Stage = 1 means we have seen the syn packet
                            // Stage = 2 means we have seen the syn-ack packet
                            // Stage = 3 means we have seen the ack packet and thus the connection is considered established
                            if (bits_bool[1] && bits_bool[3] && stage == 1) // syn-ack stage
                            {
                                stage = 2;
                                our_turn_to_send = false;
                                ruleVerdict.Accepted_state = true;
                                timeout_counter = default_timeout_counter;
                                to_out.valid = true;
                                to_out.stage = 2;
                            }
                            else if (stage == 2 && bits_bool[3] && !bits_bool[6]) // ack stage
                            {
                                stage = 3;
                                connection_is_established = true; // we have seen the syn -> syn-ack -> ack handshake
                                our_turn_to_send = false;
                                ruleVerdict.Accepted_state = true;
                                timeout_counter = default_timeout_counter;
                                to_out.valid = true;
                                to_out.stage = 3;
                            }
                            // Need a way to signal to the outgoing process that a change has been made and that it should expect a message now
                            // likewise we need for the outgoing process to be able to tell us about changes as well

                        }
                        // This should mean:
                        // We expect us the send the next packet, but we are receiving the same syn package again
                        // If they resent the same syn-package again for w/e reason
                        else if (bits_bool[6] && !bits_bool[3] && stage == 1 && syn_flag_counter > 0)
                        {
                            syn_flag_counter -= 1;
                            ruleVerdict.Accepted_state = true;
                        }
                        // Kill connection if syn counter = 0?
                        if(syn_flag_counter == 0)
                        {
                            connection_in_use = false;
                            in_use.In_use = false;
                            stage = 0;
                            to_out.end_con = true;
                            to_out.valid = true;
                        }
                    }
                    timeout_counter -= 1;
                }
            }
        }
    }


// ****************************************************************************

    [ClockedProcess]
    public class Connection_process_outgoing : SimpleProcess
    {

        [InputBus]
        public IBus_Blacklist_out dataOut;

        [InputBus]
        private readonly IBus_Update_State_tcp update_tcp = Scope.CreateOrLoadBus<IBus_Update_State_tcp>();

        [InputBus]
        private readonly IBus_Update_State_out update_out = Scope.CreateOrLoadBus<IBus_Update_State_out>();

        [InputBus]
        public IBus_TCP_to_outgoing from_tcp;

        [OutputBus]
        public IBus_outgoing_to_TCP to_TCP = Scope.CreateBus<IBus_outgoing_to_TCP>();

        [OutputBus]
        public IBus_Process_Verdict_Outgoing ruleVerdict = Scope.CreateBus<IBus_Process_Verdict_Outgoing>();

        private byte[] Ip_source { get; set; }
        private byte[] Ip_dest { get; set; }
        private uint Port_in { get; set; }

        private readonly uint my_id;

        bool our_turn_to_send;

        private bool connection_in_use = false;
        private bool connection_is_established = false;
        uint stage; // To represent the TCP handshake stage. 0 = nothing happened yet. 1 = means syn. 2 = syn-ack. 3 = ack and we are done.
        //uint syn_flag_counter; // we will acecpt 5 attemps to recieve the same syn packet before we stop accepting them

        public Connection_process_outgoing(byte[] ip_source_in, byte[] ip_dest_in, uint port, uint ids, IBus_Blacklist_out data_Out)

        {
            Port_in = port;
            Ip_source = ip_source_in;
            Ip_dest = ip_dest_in;
            my_id = ids;
            dataOut = data_Out;
        }


        protected override void OnTick()
        {
            // Set all the potential flags
            // If we get data then our reply is "valid"
            ruleVerdict.IsSet_outgoing = dataOut.ReadyToWorkFlag;
            ruleVerdict.Accepted_outgoing = false;
            to_TCP.valid = false;
            to_TCP.end_con = false;


            if (update_tcp.Flag && update_tcp.Id == my_id)
            {

                connection_in_use = true;
                // Update the state process with incoming information
                for (int k = 0; k < 4; k++)
                {
                    Ip_source[k] = update_out.DestIP[k];
                    Ip_dest[k] = update_out.SourceIP[k];

                    //Ip_source[k] = update_tcp.SourceIP[k];
                    //Ip_dest[k] = update_tcp.DestIP[k];
                }
                Port_in = update_tcp.Port;
                if (update_tcp.is_tcp)
                {
                    stage = 1;
                    our_turn_to_send = true; // they set the flag
                }

                // Meaning it is an UDP packet
                // The connection is considered established when the first UDP packet is seen
                else
                {
                    connection_is_established = true;
                }
            }

            if (update_out.Flag && update_out.Id == my_id)
            {
                // overwrite it settings here
                connection_in_use = true;

                // Update the state process with incoming information
                for (int k = 0; k < 4; k++)
                {
                    Ip_source[k] = update_out.DestIP[k];
                    Ip_dest[k] = update_out.SourceIP[k];

                    //Ip_source[k] = update_out.SourceIP[k];
                    //Ip_dest[k] = update_out.DestIP[k];
                }
                Port_in = update_out.Port;
                if (update_out.Is_tcp)
                {
                    stage = 1;
                    our_turn_to_send = false;
                }

                // Meaning it is an UDP packet
                // The connection is considered established when the first UDP packet is seen
                else
                {
                    connection_is_established = true;
                }

            }

            if (from_tcp.valid)
            {
                if (from_tcp.end_con)
                {
                    connection_in_use = false;
                    stage = 0;
                }
                else
                {
                    stage = from_tcp.stage; // maybe just add one?
                    our_turn_to_send = true;  // Simply swap the boolean. Maybe just set to true.
                }
            }

            // If we get data
            if (dataOut.ReadyToWorkFlag)
            {
                // If the connection is both in use and the connecton matches the incoming "packet connection" ports, IPs ect.
                if (connection_in_use && Shared_functions.DoesConnectExist(Ip_source, Ip_dest, Port_in, dataOut.SourceIP, dataOut.DestIP, dataOut.SourcePort))
                {
                    // Convert byte to a bool array
                    var bits_bool = new bool[8];
                    for (int i = 0; i < 8; i++)
                        bits_bool[i] = (dataOut.Flags & (1 << i)) == 0 ? false : true;
                    Array.Reverse(bits_bool);
                    // if we are in use we are either established or not establised
                    if (connection_is_established)
                    {
                        // If it is a tcp connection and it has been established(through tcp handshake) then we are only looking for the fin flag
                        if (dataOut.is_tcp)
                        {
                            // If the fin flag has been set. Check for other flags or nah?
                            if (bits_bool[7])
                            {
                                connection_in_use = false;
                                stage = 0;
                                to_TCP.valid = true;
                                to_TCP.end_con = true;
                                // tell the other process that the connection is dead! Dead I say!
                            }
                        }
                        ruleVerdict.Accepted_outgoing = true;

                    }
                    else // Under establishment
                    {
                        // Meaning its our "turn" to receive
                        // Since this process handles inconing data we are only interested if we are "supposed" to receive data
                        if (our_turn_to_send)
                        {
                            // Stage = 0 means nothing happened yet
                            // Stage = 1 means we have seen the syn packet
                            // Stage = 2 means we have seen the syn-ack packet
                            // Stage = 3 means we have seen the ack packet and thus the connection is considered established
                            if (bits_bool[6] && bits_bool[3] && stage == 1) // syn-ack stage
                            {
                                stage = 2;
                                our_turn_to_send = false; // ???
                                ruleVerdict.Accepted_outgoing = true;
                                to_TCP.valid = true;
                                to_TCP.stage = 2;
                            }
                            else if (stage == 2 && bits_bool[3] && !bits_bool[6]) // ack stage
                            {
                                stage = 3;
                                connection_is_established = true; // we have seen the syn -> syn-ack -> ack handshake
                                our_turn_to_send = false; // ???
                                ruleVerdict.Accepted_outgoing = true;
                                to_TCP.valid = true;
                                to_TCP.stage = 3;
                            }
                            // Need a way to signal to the outgoing process that a change has been made and that it should expect a message now
                            // likewise we need for the outgoing process to be able to tell us about changes as well

                        }
                        // This should mean:
                        // We expect us the send the next packet, but we are receiving the same syn package again
                        // If they resent the same syn-package again for w/e reason we accept it
                        else if (bits_bool[6] && !bits_bool[3] && stage == 1)
                        {
                            ruleVerdict.Accepted_outgoing = true;
                        }
                    }
                }
            }
        }
    }
}
