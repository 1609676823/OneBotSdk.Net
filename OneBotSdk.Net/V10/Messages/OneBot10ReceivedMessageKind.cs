namespace OneBotSdk.Net.V10.Messages;

/// <summary>
/// Identifies the wire shape retained while parsing an incoming message chain.
/// 标识解析入站消息链时保留的线协议形态。
/// </summary>
public enum OneBot10ReceivedMessageKind
{
    /// <summary>A CQ-code string. / CQ 码字符串。</summary>
    String,

    /// <summary>A tolerated single segment object used by some implementations. / 某些实现端使用的容错单消息段对象。</summary>
    Segment,

    /// <summary>An ordered message-segment array. / 有序消息段数组。</summary>
    SegmentArray
}
