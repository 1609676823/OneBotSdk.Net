using System.Runtime.CompilerServices;

// Expose internal construction seams only to the SDK's multi-target xUnit suite.
// 仅向 SDK 的多目标 xUnit 测试程序集开放内部构造测试缝。
[assembly: InternalsVisibleTo("OneBotSdk.Net.Tests")]
