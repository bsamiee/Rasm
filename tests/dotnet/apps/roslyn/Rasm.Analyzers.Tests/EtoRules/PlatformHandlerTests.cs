using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Rasm.Rhino.Analyzers;
using Xunit;

namespace Rasm.Analyzers.Tests.EtoRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class PlatformHandlerTests : CSharpCodeFixVerifier<PlatformHandlerAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task DelegateHandlers() => VerifyAnalyzerAsync(
        """
        using System;
        using Eto;

        internal delegate bool FrameLink(Action tick);

        internal static class DelegateHandlers {
            internal static void Registered(Platform platform) => platform.{|#0:Add<FrameLink>(static () => static _ => true)|};

            internal static object? Found(Platform platform) => platform.{|#1:Find<FrameLink>()|};
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Platform.Add", "FrameLink"),
        Diagnostic().WithLocation(1).WithArguments("Platform.Find", "FrameLink"));

    [Fact]
    public Task InterfaceHandlers() => VerifyAnalyzerAsync(
        """
        using Eto;

        internal interface IDrawnPlatform {
            int Order { get; }
        }

        internal sealed class AppKitDrawn : IDrawnPlatform {
            public int Order => 1;
        }

        internal static class InterfaceHandlers {
            internal static void Registered(Platform platform) => platform.Add<IDrawnPlatform>(static () => new AppKitDrawn());
        }
        """);
}
