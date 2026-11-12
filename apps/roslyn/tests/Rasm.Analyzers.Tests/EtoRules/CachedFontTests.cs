using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Rasm.Rhino.Analyzers;
using Xunit;

namespace Rasm.Analyzers.Tests.EtoRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class CachedFontTests : CSharpCodeFixVerifier<CachedFontAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task AcquiredMeasure() => VerifyAnalyzerAsync(
        """
        using System;
        using Eto.Drawing;
        using LanguageExt;
        using LanguageExt.Common;
        using Rasm.Rhino;
        using static LanguageExt.Prelude;

        namespace Rasm.Rhino {
            public sealed record Owned<T>(T Value, EnvIO Scope);

            public static class Products {
                public static IO<T> Of<T>(Func<T?> call) where T : class => IO.lift(() => Optional(call()).ToFin(Error.New("missing")));
                public static IO<TOut> Transfer<T, TOut>(Func<T?> make, Func<T, IO<TOut>> to) where T : class, IDisposable => IO.lift(() => make()!).Bind(to);
                public static IO<Unit> Released(Action release) => IO.lift(release);
                public static IO<T> Replace<T>(Ref<Option<Owned<T>>> slot, IO<T> body) => body;
            }
        }

        internal static class AcquiredMeasure {
            internal static IO<float> Measured() =>
                from regular in Products.Of(static () => {|#0:SystemFonts.Label()|})
                from emphasis in Products.Of(() => {|#1:SystemFonts.Bold(regular.Size)|})
                select emphasis.MeasureString("caption").Width;
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Label", "Products.Of"),
        Diagnostic().WithLocation(1).WithArguments("Bold", "Products.Of"));

    [Fact]
    public Task RangedFont() => VerifyAnalyzerAsync(
        """
        using System;
        using Eto.Drawing;
        using LanguageExt;
        using LanguageExt.Common;
        using Rasm.Rhino;
        using static LanguageExt.Prelude;

        namespace Rasm.Rhino {
            public sealed record Owned<T>(T Value, EnvIO Scope);

            public static class Products {
                public static IO<T> Of<T>(Func<T?> call) where T : class => IO.lift(() => Optional(call()).ToFin(Error.New("missing")));
                public static IO<TOut> Transfer<T, TOut>(Func<T?> make, Func<T, IO<TOut>> to) where T : class, IDisposable => IO.lift(() => make()!).Bind(to);
                public static IO<Unit> Released(Action release) => IO.lift(release);
                public static IO<T> Replace<T>(Ref<Option<Owned<T>>> slot, IO<T> body) => body;
            }
        }

        internal static class RangedFont {
            internal static IO<float> Lifted() =>
                from regular in IO.lift(() => {|#0:SystemFonts.Label()|})
                from size in Products.Transfer(() => regular, static held => IO.pure(held.Size))
                select size;

            internal static IO<Unit> Let() =>
                from size in IO.pure(1f)
                let shared = {|#1:SystemFonts.Palette(size)|}
                from released in release(shared)
                select released;

            internal static IO<Font> Bound() =>
                IO.lift(() => {|#2:SystemFonts.Message()|}).Bind(static font => use(() => font));

            internal static IO<float> Scoped() => Products.Transfer(() => {|#3:SystemFonts.User()|}, static font => IO.pure(font.Size));
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Label", "Products.Transfer"),
        Diagnostic().WithLocation(1).WithArguments("Palette", "Prelude.release"),
        Diagnostic().WithLocation(2).WithArguments("Message", "Prelude.use"),
        Diagnostic().WithLocation(3).WithArguments("User", "Products.Transfer"));

    [Fact]
    public Task DisposedFont() => VerifyAnalyzerAsync(
        """
        using System;
        using Eto.Drawing;
        using LanguageExt;
        using LanguageExt.Common;
        using Rasm.Rhino;
        using static LanguageExt.Prelude;

        namespace Rasm.Rhino {
            public sealed record Owned<T>(T Value, EnvIO Scope);

            public static class Products {
                public static IO<T> Of<T>(Func<T?> call) where T : class => IO.lift(() => Optional(call()).ToFin(Error.New("missing")));
                public static IO<TOut> Transfer<T, TOut>(Func<T?> make, Func<T, IO<TOut>> to) where T : class, IDisposable => IO.lift(() => make()!).Bind(to);
                public static IO<Unit> Released(Action release) => IO.lift(release);
                public static IO<T> Replace<T>(Ref<Option<Owned<T>>> slot, IO<T> body) => body;
            }
        }

        internal static class DisposedFont {
            internal static float Used() {
                using Font font = {|#0:SystemFonts.Default()|};
                return font.Size;
            }

            internal static void Direct() => {|#1:SystemFonts.Bold()|}.Dispose();

            internal static void Local() {
                Font font = {|#2:SystemFonts.StatusBar()|};
                _ = Products.Released(font.Dispose).Run();
            }

            internal static IO<Font> Held(Ref<Option<Owned<Font>>> slot) => Products.Replace(slot, IO.pure({|#3:SystemFonts.Menu()|}));

            internal static void Retained(Action<IDisposable> retain) => retain({|#4:SystemFonts.ToolTip()|});

            internal static IO<Unit> Deferred() => IO.lift({|#5:SystemFonts.TitleBar()|}.Dispose);

            internal static void Casted() => ((IDisposable){|#6:SystemFonts.Label()|}).Dispose();

            internal static void Conditional() => {|#7:SystemFonts.Message()|}?.Dispose();

            internal static void Tuple() {
                (Font Item, int Count) held = ({|#8:SystemFonts.Label()|}, 1);
                held.Item.Dispose();
            }

            internal static void Deconstructed() {
                Font font;
                int count;
                (font, count) = ({|#9:SystemFonts.Message()|}, 1);
                font.Dispose();
            }

            internal static void Other() {
                (Font First, Font Second) held = (SystemFonts.Label(), {|#10:SystemFonts.Bold()|});
                held.Second.Dispose();
            }
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("Default", "using"),
        Diagnostic().WithLocation(1).WithArguments("Bold", "Dispose"),
        Diagnostic().WithLocation(2).WithArguments("StatusBar", "Dispose"),
        Diagnostic().WithLocation(3).WithArguments("Menu", "Products.Replace"),
        Diagnostic().WithLocation(4).WithArguments("ToolTip", "retain"),
        Diagnostic().WithLocation(5).WithArguments("TitleBar", "Dispose"),
        Diagnostic().WithLocation(6).WithArguments("Label", "Dispose"),
        Diagnostic().WithLocation(7).WithArguments("Message", "Dispose"),
        Diagnostic().WithLocation(8).WithArguments("Label", "Dispose"),
        Diagnostic().WithLocation(9).WithArguments("Message", "Dispose"),
        Diagnostic().WithLocation(10).WithArguments("Bold", "Dispose"));

    [Fact]
    public Task ReadFont() => VerifyAnalyzerAsync(
        """
        using System;
        using Eto.Drawing;
        using LanguageExt;
        using LanguageExt.Common;
        using Rasm.Rhino;
        using static LanguageExt.Prelude;

        namespace Rasm.Rhino {
            public sealed record Owned<T>(T Value, EnvIO Scope);

            public static class Products {
                public static IO<T> Of<T>(Func<T?> call) where T : class => IO.lift(() => Optional(call()).ToFin(Error.New("missing")));
                public static IO<TOut> Transfer<T, TOut>(Func<T?> make, Func<T, IO<TOut>> to) where T : class, IDisposable => IO.lift(() => make()!).Bind(to);
                public static IO<Unit> Released(Action release) => IO.lift(release);
                public static IO<T> Replace<T>(Ref<Option<Owned<T>>> slot, IO<T> body) => body;
            }
        }

        internal static class ReadFont {
            internal static IO<float> Measured() =>
                from regular in IO.lift(static () => SystemFonts.Label())
                from emphasis in IO.lift(() => SystemFonts.Bold(regular.Size))
                select emphasis.MeasureString("caption").Width;

            internal static float Cached() {
                Font shared = SystemFonts.Cached(SystemFont.Bold);
                return shared.Size;
            }

            internal static IO<float> Owned() =>
                from owned in Products.Of(static () => new Font(SystemFont.Label))
                select owned.Size;

            internal static void Converted() {
                using CopiedFont font = SystemFonts.Label();
            }
        }

        internal sealed class CopiedFont(Font font) : IDisposable {
            public static implicit operator CopiedFont(Font shared) => new(new Font(SystemFont.Label));

            public void Dispose() => font.Dispose();
        }
        """);
}
