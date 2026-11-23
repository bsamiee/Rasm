using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Thinktecture;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class DataFreeSmartEnumTests : CSharpCodeFixVerifier<DataFreeSmartEnumAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task KeylessSmartEnumOfDataFreeItemsIsReportedWhileItemsHoldingDataAreSilent() => VerifyAnalyzerAsync(
        """
        using Thinktecture;

        [SmartEnum]
        internal sealed partial class {|#0:Modifier|} {
            public static readonly Modifier Common = new();
            public static readonly Modifier Shift = new();
        }

        [SmartEnum]
        internal sealed partial class Weight {
            public static readonly Weight Light = new(300);
            public static readonly Weight Bold = new(700);

            public int Css { get; }
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Modifier", "holds data-free items"));

    [Fact]
    public Task InternalStringKeysSpellingMemberNamesAreReportedUnlessAGeneratedReaderOrKeyReadsThemOrTheyDiffer() => VerifyAnalyzerAsync(
        """
        using Thinktecture;

        [SmartEnum<string>]
        internal sealed partial class {|#0:Channel|} {
            public static readonly Channel Red = new("red");
            public static readonly Channel DeepBlue = new("deep-blue");
        }

        [SmartEnum<string>]
        internal sealed partial class Parsed {
            public static readonly Parsed First = new("first");
            public static readonly Parsed Second = new("second");
        }

        [SmartEnum<string>]
        public sealed partial class Shared {
            public static readonly Shared Alpha = new("alpha");
            public static readonly Shared Beta = new("beta");
        }

        [SmartEnum<string>]
        internal sealed partial class Spelled {
            public static readonly Spelled North = new("north");
            public static readonly Spelled South = new("south");
        }

        [SmartEnum<string>]
        internal sealed partial class Discipline {
            public static readonly Discipline General = new("G");
            public static readonly Discipline Survey = new("V");
        }

        internal static class Readers {
            internal static Channel Get(this Channel channel) => channel;

            internal static Channel Lookalike(Channel channel) => channel.Get();

            internal static Parsed Read() => Parsed.Parse("first", null);

            internal static string Key(Spelled spelled) => spelled.Key;
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Channel", "keys items by unread member names"));
}
