using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SME;

namespace simplePackageFilter
{


    [InputBus]
    public interface IBus_Controller_to_state : IBus
    {
        [FixedArrayLength(4)]
        IFixedArray<byte> Ipv4_SourceIP { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> Ipv4_DestIP { get; set; }

        [InitialValue(false)]
        bool Ipv4_Flag { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> Stateful_SourceIP { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> Stateful_DestIP { get; set; }

        int Stateful_Port { get; set; }

        byte Stateful_header_Flag { get; set; }

        [InitialValue(false)]
        bool Stateful_Flag { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> Out_SourceIP { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> Out_DestIP { get; set; }

        int Out_SourcePort { get; set; }

        byte Out_Header_Flags { get; set; }

        [InitialValue(false)]
        bool Out_Flag { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> Update_SourceIP { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> Update_DestIP { get; set; }

        int Update_Port { get; set; }

        int Update_Id { get; set; }

        [InitialValue(false)]
        bool Update_Flag { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> Update_SourceIP_2 { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> Update_DestIP_2 { get; set; }

        int Update_Port_2 { get; set; }

        int Update_Id_2 { get; set; }

        [InitialValue(false)]
        bool Update_Flag_2 { get; set; }

    }

    public class Controller : StateProcess
    {
        // The bus from the blacklist. If it is blacklisted then should set the valid flag to false and true otherwise.
        [InputBus]
        private readonly IBus_blacklist_finalVerdict_out blacklist_input = Scope.CreateOrLoadBus<IBus_blacklist_finalVerdict_out>();
        // Input bus containing the data, eg port's and ip's
        [InputBus]
        private readonly IBus_Blacklist_out dataOut = Scope.CreateOrLoadBus<IBus_Blacklist_out>();

        // Input bus from the ipv4 header check
        [InputBus]
        private readonly IBus_IPv4_In ipv4_in = Scope.CreateOrLoadBus<IBus_IPv4_In>();

        // Input but from the stateful check
        [InputBus]
        private readonly IBus_ITCP_In stateful_in = Scope.CreateOrLoadBus<IBus_ITCP_In>();

        // Input bus to check if we need to update state
        [InputBus]
        private readonly IBus_Update_State update = Scope.CreateOrLoadBus<IBus_Update_State>();

        [InputBus]
        //private readonly IBus_Connection_In_Use in_use = Scope.CreateOrLoadBus<IBus_Connection_In_Use>();
        public IBus_Connection_In_Use[] in_use;

        // This is the output bus to the processes(state) 
        [OutputBus]
        private readonly IBus_Controller_to_state to_state = Scope.CreateOrLoadBus<IBus_Controller_to_state>();



        // old constructor
        //public Controller(IBus_blacklist_finalVerdict_out BUS_blacklist_input, IBus_Blacklist_out BUS_dataOut,
        //          IBus_IPv4_In BUS_ipv4_in, IBus_ITCP_In BUS_stateful_in,
        //          IBus_Update_State BUS_update, IBus_Connection_In_Use[] BUS_in_use)
        // CONSTRUCTOR
        public Controller(IBus_Connection_In_Use[] BUS_in_use)

        {
        //    update = BUS_update;
            in_use = BUS_in_use;
        //    ipv4_in = BUS_ipv4_in;
        //    dataOut = BUS_dataOut;
        //    stateful_in = BUS_stateful_in;
        //    blacklist_input = BUS_blacklist_input;
        }


        protected async override System.Threading.Tasks.Task OnTickAsync()
        {
            // While the blacklist, the statefull check and the ip check didnt send us anything wait
            // if just one of them did go on
            while (!blacklist_input.Valid) //|| ipv4_in.ClockCheck || stateful_in.ThatOneVariableThatSaysIfWeAreDone || update.Flag || update.Flag_2)
            {
                await ClockAsync();
            }
           
            // Set all the flags to false, if the flag is set then it will become true again
            to_state.Update_Flag = false;
            to_state.Update_Flag_2 = false;
            to_state.Ipv4_Flag = false;
            to_state.Out_Flag = false;
            to_state.Stateful_Flag = false;


            // Find which flags are set
            if (update.Flag)
            {
                to_state.Update_DestIP = update.DestIP;
                to_state.Update_SourceIP = update.SourceIP;
                to_state.Update_Port = update.Port;
                to_state.Update_Flag = true;

            //    to_state.Update_Id = find first available id
            }
            if (update.Flag_2)
            {
                to_state.Update_DestIP_2 = update.DestIP_2;
                to_state.Update_SourceIP_2 = update.SourceIP_2;
                to_state.Update_Port_2 = update.Port_2;
                to_state.Update_Flag_2 = true;

                //    to_state.Update_Id = find first available id
            }

            if (blacklist_input.Valid)
            { 
                if(blacklist_input.Accept_or_deny)
                {
                    to_state.Out_DestIP = dataOut.DestIP;
                    to_state.Out_Header_Flags = dataOut.Flags;
                    to_state.Out_SourceIP = dataOut.SourceIP;
                    to_state.Out_SourcePort = dataOut.SourcePort;
                    to_state.Out_Flag = true;
                }
                // Needs to send a flag back signaling if they packet was accepted or denied. Not sure where to send it to yet
            }

            if(ipv4_in.ClockCheck)
            {
                to_state.Ipv4_DestIP = ipv4_in.DestIP;
                to_state.Ipv4_SourceIP = ipv4_in.SourceIP;
                to_state.Ipv4_Flag = true;
            }

            if(stateful_in.ThatOneVariableThatSaysIfWeAreDone)
            {
                to_state.Stateful_DestIP = stateful_in.DestIP;
                to_state.Stateful_header_Flag = stateful_in.Flags;
                to_state.Stateful_SourceIP = stateful_in.SourceIP;
                to_state.Stateful_Port = stateful_in.Port;
                to_state.Stateful_Flag = true;
            }
            await ClockAsync();
        }
    }
}
