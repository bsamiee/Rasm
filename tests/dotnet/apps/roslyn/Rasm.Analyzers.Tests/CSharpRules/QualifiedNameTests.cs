using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.CSharpRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class QualifiedNameTests : CSharpCodeFixVerifier<QualifiedNameAnalyzer, EmptyCodeFixProvider> {

    // --- [RASM3009]

    [Fact]
    public Task Painter() => VerifyAnalyzerAsync(
        """
        using LanguageExt;
        using Rhino.Geometry;
        using static LanguageExt.Prelude;

        internal static class Painter {
            internal static int Bitmap(int size) => size;

            internal static System.Drawing.Bitmap Made(int size) => new(size, size);

            internal static Unit Run(int unit) => {|#0:Prelude.unit|};

            internal static System.Drawing.Point Origin => default;

            internal static Point Placed => new(Point3d.Origin);
        }
        """,
        Diagnostic("RASM3009").WithLocation(0).WithArguments("Prelude.unit", "int unit", "LanguageExt.Prelude"));

    // --- [RASM3010]

    [Fact]
    public Task Palette() => VerifyAnalyzerAsync(
        """
        using Rhino.Geometry;

        internal sealed record Color(int Value);

        internal static class Palette {
            internal static {|#0:System.Collections.Immutable.ImmutableArray<int>|} Empty => [];

            internal static System.Drawing.Bitmap Blank => new(1, 1);

            internal static System.Drawing.Point Origin => default;

            internal static Point Placed => new(Point3d.Origin);

            internal static Plane Base => Plane.WorldXY;

            internal static System.Numerics.Vector2 Offset => default;
        }
        """,
        Diagnostic("RASM3010").WithLocation(0).WithArguments("System.Collections.Immutable.ImmutableArray<int>", "System.Collections.Immutable", "ImmutableArray<int>"));

    // --- [RASM3014]

    [Fact]
    public Task StaticClassAlias() => VerifyAnalyzerAsync(
        """
        using LanguageExt;
        using LanguageExt.Traits;
        {|#0:using Narrowing = LanguageExt.OptionExtensions;|}

        internal static class NarrowedConcrete {
            internal static Fin<int> Held(Fin<int> value) => value.As();
            internal static Option<int> Mapped(Option<int> value) => value.Map(static held => held + 1).As();

            internal static Fin<int> Static(Fin<int> value) => FinExtensions.As(value);

            internal static Option<int> Aliased(Option<int> value) => Narrowing.As(value);

            internal static Option<int> Selected(bool first, Option<int> left, Option<int> right) => (first ? left : right).As().Map(static held => held + 1);

            internal static Option<int> Queried(Option<int> value) => (from held in value select held + 1).As().Map(static held => held * 2);

            internal static Option<int> Cast(K<Option, int> value) => ((Option<int>)value).As().Map(static held => held + 1);

            internal static Fin<int> Commented(Fin<int> value) => value.As(/* narrowing */);
        }

        namespace Custom {
            internal static class CustomNarrowing {
                internal static Fin<int> Preserve(Fin<int> value) => value.As();

                internal static Fin<int> As(this K<Fin, int> value) => ((Fin<int>)value).Map(static held => held + 99);
            }
        }
        """,
        Diagnostic("RASM3014").WithLocation(0).WithArguments("Narrowing", "type"));

    [Fact]
    public Task DateTimeAlias() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Collections;
        using System.Collections.Generic;
        using System.ComponentModel;
        using System.IO;
        using System.Runtime.CompilerServices;
        using LanguageExt;
        {|#0:using Calendar = System.DateTime;|}

        internal static class StaticReads {
            internal static DateTime Utc => Calendar.UtcNow;

            internal static int Maximum => int.MaxValue;
        }
        """,
        Diagnostic("RASM3014").WithLocation(0).WithArguments("Calendar", "type"));

    [Fact]
    public Task TypeAlias() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Reflection;
        using LanguageExt.Common;
        {|#0:using Alias = System.Type;|}

        internal sealed class DisplayType(Alias type) : TypeDelegator(type) {
            public new string ToString() => "display";
        }
        """,
        Diagnostic("RASM3014").WithLocation(0).WithArguments("Alias", "type"));

    [Fact]
    public Task GuidAlias() => VerifyAnalyzerAsync(
        """
        using System;
        using LanguageExt;
        using Rhino;
        using Rhino.Geometry;
        {|#0:using Id = System.Guid;|}

        internal sealed record Entry(Id Key);
        """,
        Diagnostic("RASM3014").WithLocation(0).WithArguments("Id", "type"));

    [Fact]
    public Task ConstructedAlias() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Collections;
        using System.Collections.Generic;
        using System.Collections.Immutable;
        using System.Threading.Tasks;
        using LanguageExt;
        {|#0:using HashSet = System.Collections.Generic.HashSet<int>;|}
        using static LanguageExt.Prelude;
        """,
        Diagnostic("RASM3014").WithLocation(0).WithArguments("HashSet", "type"));

    [Fact]
    public async Task ReferencedAliases() {
        CancellationToken token = TestContext.Current.CancellationToken;
        CSharpCodeFixTest<QualifiedNameAnalyzer, EmptyCodeFixProvider> test = new() {
            TestCode = """
                {|RASM3014:using Stage = Models.Stage;|}
                {|RASM3014:using Dither = Models.Dither;|}

                namespace Consumer;

                internal static class Usage {
                    internal static bool Is<TState>(TState value) where TState : Stage => value is Dither;
                }
                """,
        };
        test.TestState.AdditionalReferences.Add(AdditionalMetadataReferences.Emitted(await AdditionalMetadataReferences.CompiledAsync("Catalog", """
            namespace Models;

            public abstract record Stage;

            public sealed record Dither(int Value) : Stage;
            """, token), token));
        await test.RunAsync(token);
    }
}
