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
        bool ipv4_ready_flag { get; set; }
    }

    [InputBus]
    public interface IBus_Connection_In_Use : IBus
    {
        bool In_use { get; set; }

        int Id { get; set; }
    }
    [InputBus]
    public interface IBus_Update_State : IBus
    {
        [FixedArrayLength(4)]
        IFixedArray<byte> SourceIP { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> DestIP { get; set; }

        int Port { get; set; }

        [InitialValue(false)]
        bool Flag { get; set; }

        uint Id { get; set; }

        // We can need to update 2 entries in the state at the same time

        [FixedArrayLength(4)]
        IFixedArray<byte> SourceIP_2 { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> DestIP_2 { get; set; }

        int Port_2 { get; set; }
        [InitialValue(false)]
        bool Flag_2 { get; set; }

        int Id_2 { get; set; }
    }
    [TopLevelInputBus]
    public interface IBus_ITCP_In : IBus
    {
        [FixedArrayLength(4)]
        IFixedArray<byte> SourceIP { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> DestIP { get; set; }

        int Port { get; set; }

        byte Flags { get; set; }

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
        bool Accepted_ipv4 { get; set; }

        [InitialValue(false)]
        bool IsSet_ipv4 { get; set; }
    }

    [TopLevelInputBus]
    public interface IBus_State_Verdict : IBus
    {
        [InitialValue(false)]
        bool Accepted { get; set; }

        [InitialValue(false)]
        bool Flag { get; set; }
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

    [TopLevelOutputBus]
    public interface IBus_finalVerdict_In : IBus
    {
        [InitialValue(false)]
        bool ipv4_Accept_or_deny { get; set; }
        [InitialValue(false)]
        bool ipv4_Valid { get; set; }

        [InitialValue(false)]
        bool tcp_Accept_or_deny { get; set; }
        [InitialValue(false)]
        bool tcp_Valid { get; set; }
    }
    [TopLevelInputBus]
    public interface IBus_Blacklist_out : IBus
    {
        [FixedArrayLength(4)]
        IFixedArray<byte> SourceIP { get; set; }

        [FixedArrayLength(4)]
        IFixedArray<byte> DestIP { get; set; }

        int SourcePort { get; set; }

        byte Flags { get; set; }

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

}
