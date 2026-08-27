using System;
using System.Linq;
using OneBotSdk.Net.V11.Messages;
using Xunit;

namespace OneBotSdk.Net.Tests;

public sealed class MessageModelArchitectureTests
{
    [Fact]
    public void DirectionalSegmentCatalog_UsesIndependentConcreteClassHierarchies()
    {
        var expectedSendTypes = new[]
        {
            typeof(AnonymousSendSegment),
            typeof(AtSendSegment),
            typeof(ContactSendSegment),
            typeof(CustomForwardNodeSendSegment),
            typeof(CustomMusicSendSegment),
            typeof(CustomSendSegment),
            typeof(DiceSendSegment),
            typeof(FaceSendSegment),
            typeof(ForwardNodeSendSegment),
            typeof(ImageSendSegment),
            typeof(JsonSendSegment),
            typeof(LocationSendSegment),
            typeof(MusicSendSegment),
            typeof(PokeSendSegment),
            typeof(RecordSendSegment),
            typeof(ReplySendSegment),
            typeof(RpsSendSegment),
            typeof(ShakeSendSegment),
            typeof(ShareSendSegment),
            typeof(TextSendSegment),
            typeof(VideoSendSegment),
            typeof(XmlSendSegment)
        };
        var expectedReceivedTypes = new[]
        {
            typeof(AtReceivedSegment),
            typeof(ContactReceivedSegment),
            typeof(DiceReceivedSegment),
            typeof(FaceReceivedSegment),
            typeof(ForwardNodeReceivedSegment),
            typeof(ForwardReceivedSegment),
            typeof(ImageReceivedSegment),
            typeof(JsonReceivedSegment),
            typeof(LocationReceivedSegment),
            typeof(PokeReceivedSegment),
            typeof(RecordReceivedSegment),
            typeof(ReplyReceivedSegment),
            typeof(RpsReceivedSegment),
            typeof(ShakeReceivedSegment),
            typeof(ShareReceivedSegment),
            typeof(TextReceivedSegment),
            typeof(UnknownReceivedSegment),
            typeof(VideoReceivedSegment),
            typeof(XmlReceivedSegment)
        };

        // Discover the public catalog from the compiled assembly so future model changes must keep both directions explicit.
        // 从已编译程序集发现公开目录，使未来模型变更必须继续明确区分收发方向。
        var publicTypes = typeof(OneBot11SendSegment).Assembly.GetTypes();
        var actualSendTypes = publicTypes
            .Where(type => type.IsPublic && !type.IsAbstract && typeof(OneBot11SendSegment).IsAssignableFrom(type));
        var actualReceivedTypes = publicTypes
            .Where(type => type.IsPublic && !type.IsAbstract && typeof(OneBot11ReceivedSegment).IsAssignableFrom(type));

        Assert.Equal(Sort(expectedSendTypes), Sort(actualSendTypes));
        Assert.Equal(Sort(expectedReceivedTypes), Sort(actualReceivedTypes));
        Assert.All(actualSendTypes, type => Assert.False(typeof(OneBot11ReceivedSegment).IsAssignableFrom(type)));
        Assert.All(actualReceivedTypes, type => Assert.False(typeof(OneBot11SendSegment).IsAssignableFrom(type)));
    }

    private static Type[] Sort(System.Collections.Generic.IEnumerable<Type> types)
    {
        return types.OrderBy(type => type.FullName, StringComparer.Ordinal).ToArray();
    }
}
