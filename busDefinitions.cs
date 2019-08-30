using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SME;

namespace simplePackageFilter
{
    [TopLevelInputBus]
    public interface ipv4_verdict_to_sim : IBus
    {
        [InitialValue(false)]
        bool ipv4_ready_flag { get; set; }
    }

    [TopLevelInputBus]
    public interface tcp_verdict_to_sim :IBus
    {
        [InitialValue(false)]
        bool tcp_ready_flag { get; set; }
    }

    [TopLevelInputBus]
    public interface out_verdict_to_sim : IBus
    {
        [InitialValue(false)]
        bool out_ready_flag { get; set; }
    }



    [InputBus]
    public interface IBus_Connection_In_Use : IBus
    {
        [InitialValue(false)]
        bool In_use { get; set; }

        uint Id { get; set; }

    }
    [InputBus]
    public interface IBus_Update_State_tcp : IBus
    {
        [FixedArrayLength(4)]
        IFixedArray<byte> SourceIP { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> DestIP { get; set; }

        [InitialValue(0)]
        uint Port { get; set; }

        [InitialValue(false)]
        bool Flag { get; set; }

        [InitialValue(0)]
        uint Id { get; set; }

        [InitialValue(true)]
        bool is_tcp { get; set; }
    }

    public interface IBus_Update_State_out : IBus
    {
        [FixedArrayLength(4)]
        IFixedArray<byte> SourceIP { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> DestIP { get; set; }

        [InitialValue(0)]
        uint Port { get; set; }

        [InitialValue(false)]
        bool Flag { get; set; }

        [InitialValue(0)]
        uint Id { get; set; }

        [InitialValue(true)]
        bool Is_tcp { get; set; }
    }


    [TopLevelInputBus]
    public interface IBus_ITCP_In : IBus
    {
        [FixedArrayLength(4)]
        IFixedArray<byte> SourceIP { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> DestIP { get; set; }

        uint Port { get; set; }

        byte Flags { get; set; }

        [InitialValue(true)]
        bool is_tcp { get; set; }

        [InitialValue(false)]
        bool ThatOneVariableThatSaysIfWeAreDone { get; set; }


    }
    [TopLevelInputBus]
    public interface IBus_Process_Verdict_IPV4 : IBus
    {
        [InitialValue(false)]
        bool Accepted_ipv4 { get; set; }

        [InitialValue(false)]
        bool IsSet_ipv4 { get; set; }

    }

    [TopLevelInputBus]
    public interface IBus_Process_Verdict_TCP : IBus
    {
        [InitialValue(false)]
        bool Accepted_state { get; set; }

        [InitialValue(false)]
        bool IsSet_state { get; set; }
    }

    [TopLevelInputBus]
    public interface IBus_Process_Verdict_Outgoing : IBus
    {
        [InitialValue(false)]
        bool Accepted_outgoing { get; set; }

        [InitialValue(false)]
        bool IsSet_outgoing { get; set; }
    }


    [TopLevelInputBus]
    public interface IBus_State_Verdict_IPv4 : IBus
    {
        [InitialValue(false)]
        bool Accepted { get; set; }

        [InitialValue(false)]
        bool flag { get; set; }
    }
    [TopLevelOutputBus]
    public interface IBus_finalVerdict_tcp_In : IBus
    {
        [InitialValue(false)]
        bool Accept_or_deny { get; set; }
        [InitialValue(false)]
        bool Valid { get; set; }
    }

    [TopLevelInputBus]
    public interface IBus_IPv4_In : IBus
    {
        [FixedArrayLength(4)]
        IFixedArray<byte> SourceIP { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> DestIP { get; set; }

        [InitialValue(false)]
        bool ClockCheck { get; set; }
    }

    [TopLevelInputBus]
    public interface IBus_Rule_Verdict_IPV4 : IBus
    {
        [InitialValue(false)]
        bool ipv4_Accepted { get; set; }

        [InitialValue(false)]
        bool ipv4_IsSet { get; set; }
    }

    [TopLevelInputBus]
    public interface IBus_Rule_Verdict_TCP : IBus
    {
        [InitialValue(false)]
        bool tcp_Accepted { get; set; }

        [InitialValue(false)]
        bool tcp_IsSet { get; set; }
    }

    [TopLevelInputBus]
    public interface IBus_Blacklist_out : IBus
    {
        [FixedArrayLength(4)]
        IFixedArray<byte> SourceIP { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> DestIP { get; set; }

        uint SourcePort { get; set; }

        byte Flags { get; set; }

        [InitialValue(true)]
        bool is_tcp { get; set; }

        [InitialValue(false)]
        bool ReadyToWorkFlag { get; set; }
    }

    [TopLevelInputBus]
    public interface IBus_blacklist_ruleVerdict_out : IBus
    {
        [InitialValue(false)]
        bool Accepted { get; set; }

        [InitialValue(false)]
        bool IsSet { get; set; }
    }

    [TopLevelOutputBus]
    public interface IBus_blacklist_finalVerdict_out : IBus
    {
        [InitialValue(false)]
        bool Accept_or_deny { get; set; }
        [InitialValue(false)]
        bool Valid { get; set; }
    }

    [InputBus]
    public interface IBus_TCP_to_outgoing : IBus
    {
        [InitialValue(false)]
        bool valid { get; set; }
        uint stage { get; set; }

        [InitialValue(false)]
        bool end_con { get; set; }
    }

    [InputBus]
    public interface IBus_outgoing_to_TCP : IBus
    {
        [InitialValue(false)]
        bool valid { get; set; }
        uint stage { get; set; }

        [InitialValue(false)]
        bool end_con { get; set; }
    }


    [InputBus]
    public interface Loop_Con_IPv4_To_Decider : IBus
    {
        [InitialValue(false)]
        bool Valid { get; set; }

        [InitialValue(false)]
        bool Value { get; set; } 
    }

    [InputBus]
    public interface Loop_Whitelist_IPv4_To_Decider : IBus
    {
        [InitialValue(false)]
        bool Valid { get; set; }

        [InitialValue(false)]
        bool Value { get; set; }
    }

    [InputBus]
    public interface Loop_Whitelist_TCP_To_Decider : IBus
    {
        [InitialValue(false)]
        bool Valid { get; set; }

        [InitialValue(false)]
        bool Value { get; set; }
    }

    [InputBus]
    public interface Loop_Con_TCP_To_Decider : IBus
    {
        [InitialValue(false)]
        bool Valid { get; set; }

        [InitialValue(false)]
        bool Value { get; set; }
    }

    [InputBus]
    public interface Loop_Blacklist_To_Decider : IBus
    {
        [InitialValue(false)]
        bool Valid { get; set; }

        [InitialValue(false)]
        bool Value { get; set; }
    }

    [InputBus]
    public interface Loop_Con_Outgoing_To_Decider : IBus
    {
        [InitialValue(false)]
        bool Valid { get; set; }

        [InitialValue(false)]
        bool Value { get; set; }
    }

    [InputBus]
    public interface Loop_In_use_To_Decider : IBus
    {
        [InitialValue(0)]
        uint Id_TCP { get; set; }

        [InitialValue(true)]
        bool Valid_TCP { get; set; }

        [InitialValue(0)]
        uint Id_Out { get; set; }

        [InitialValue(true)]
        bool Valid_Out { get; set; }

    }

}

