using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.ThinktectureRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class DataFreeSmartEnumTests : CSharpCodeFixVerifier<DataFreeSmartEnumAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task Modifier() => VerifyAnalyzerAsync(
        """
        using Thinktecture;

        [SmartEnum]
        internal sealed partial class {|#0:Modifier|} {
            public static readonly Modifier Common = new();
            public static readonly Modifier Shift = new();
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Modifier", "holds data-free items"));

    [Fact]
    public Task Glass() => VerifyAnalyzerAsync(
        """
        using Thinktecture;

        [SmartEnum<string>]
        internal sealed partial class {|#0:Glass|} {
            public static readonly Glass Regular = new("regular");
            public static readonly Glass ClearFrosted = new("clear-frosted");
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Glass", "keys items by unread member names"));

    [Fact]
    public Task Discipline() => VerifyAnalyzerAsync(
        """
        using Thinktecture;

        [SmartEnum<string>]
        internal sealed partial class Discipline {
            public static readonly Discipline General = new("G");
            public static readonly Discipline Survey = new("V");
        }
        """);

    [Fact]
    public Task Channel() => VerifyAnalyzerAsync(
        """
        using Thinktecture;

        [SmartEnum<string>]
        internal sealed partial class {|#0:Channel|} {
            public static readonly Channel Red = new("red");
            public static readonly Channel Green = new("green");
        }

        [SmartEnum<string>]
        internal sealed partial class Parsed {
            public static readonly Parsed First = new("first");
            public static readonly Parsed Second = new("second");
        }

        internal static class Channels {
            internal static Channel Get(this Channel channel) => channel;

            internal static Channel Checked(Channel channel) => channel.Get();

            internal static Parsed Read() => Parsed.Parse("first", null);
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Channel", "keys items by unread member names"));

    [Fact]
    public Task Weight() => VerifyAnalyzerAsync(
        """
        using Thinktecture;

        [SmartEnum]
        internal sealed partial class Weight {
            public static readonly Weight Light = new(300);
            public static readonly Weight Bold = new(700);

            public int Css { get; }
        }
        """);
}
