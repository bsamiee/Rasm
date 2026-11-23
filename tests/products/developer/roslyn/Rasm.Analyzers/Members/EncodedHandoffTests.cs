using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Members;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class EncodedHandoffTests : CSharpCodeFixVerifier<EncodedHandoffAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsDecodeOfPixelsTheSameCodeEncodedAndIgnoresAnUnrelatedEncode() => VerifyAnalyzerAsync(
        """
        using System.IO;
        using SkiaSharp;

        internal static class Handoffs {
            internal static SKImage? Direct(SKImage image) => {|#0:SKImage.FromEncodedData(image.Encode(SKEncodedImageFormat.Png, 100))|};

            internal static SKBitmap Held(SKBitmap bitmap) {
                using SKData data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
                return {|#1:SKBitmap.Decode(data)|};
            }

            internal static Eto.Drawing.Bitmap Bridged(Eto.Drawing.Bitmap bitmap) => {|#2:new Eto.Drawing.Bitmap(bitmap.ToByteArray(Eto.Drawing.ImageFormat.Png))|};

            internal static System.Drawing.Bitmap Streamed(System.Drawing.Bitmap bitmap) {
                using MemoryStream stream = new();
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;
                return {|#3:new System.Drawing.Bitmap(stream)|};
            }

            internal static SKImage? Loaded(SKImage image, Stream file) {
                using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
                return SKImage.FromEncodedData(file);
            }
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("SKImage.FromEncodedData", "SKImage.Encode"),
        Diagnostic().WithLocation(1).WithArguments("SKBitmap.Decode", "SKBitmap.Encode"),
        Diagnostic().WithLocation(2).WithArguments("new Bitmap", "Bitmap.ToByteArray"),
        Diagnostic().WithLocation(3).WithArguments("new Bitmap", "Image.Save"));
}
