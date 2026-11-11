using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Rasm.Rhino.Analyzers;
using Xunit;

namespace Rasm.Analyzers.Tests.RhinoCommonRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class GrayscaleCaptureTests : CSharpCodeFixVerifier<GrayscaleCaptureAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task TrustedGrayscale() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Drawing;
        using LanguageExt;
        using LanguageExt.Common;
        using Rhino.Display;
        using static LanguageExt.Prelude;

        namespace Rasm.Rhino {
            public static class Products {
                public static IO<T> Of<T>(Func<T?> call) where T : class => IO.lift(() => Optional(call()).ToFin(Error.New("missing")));
            }

            public static class Conversions {
                public static Fin<T> Required<T>(T? value) where T : class => Optional(value).ToFin(Error.New("missing"));
            }
        }

        internal static class TrustedGrayscale {
            internal static IO<Size> Sized(ZBufferCapture capture) => IO.lift(() => {|#0:capture.GrayscaleDib()|}.Size);

            internal static IO<Bitmap> Kept(ZBufferCapture capture) => IO.lift(() => {|#1:capture.GrayscaleDib()|});

            internal static Bitmap Held(ZBufferCapture capture) {
                Bitmap gray = {|#2:capture.GrayscaleDib()|};
                return gray;
            }
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("capture.GrayscaleDib()"),
        Diagnostic().WithLocation(1).WithArguments("capture.GrayscaleDib()"),
        Diagnostic().WithLocation(2).WithArguments("capture.GrayscaleDib()"));

    [Fact]
    public Task CheckedGrayscale() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Drawing;
        using LanguageExt;
        using LanguageExt.Common;
        using Rasm.Rhino;
        using Rhino.Display;
        using static LanguageExt.Prelude;

        namespace Rasm.Rhino {
            public static class Products {
                public static IO<T> Of<T>(Func<T?> call) where T : class => IO.lift(() => Optional(call()).ToFin(Error.New("missing")));
            }

            public static class Conversions {
                public static Fin<T> Required<T>(T? value) where T : class => Optional(value).ToFin(Error.New("missing"));
            }
        }

        internal static class CheckedGrayscale {
            internal static Fin<Bitmap> Required(ZBufferCapture capture) => Conversions.Required(capture.GrayscaleDib());

            internal static Option<Bitmap> Wrapped(ZBufferCapture capture) => Optional(capture.GrayscaleDib());

            internal static IO<Size> Acquired(ZBufferCapture capture) => Products.Of(() => capture.GrayscaleDib()).Map(static gray => gray.Size);
        }
        """);
}
