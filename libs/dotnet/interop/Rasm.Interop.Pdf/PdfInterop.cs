using PdfSharp.Fonts;
using PdfSharp.Snippets.Font;

namespace Rasm.Interop.Pdf;

/// <summary>Provides the PDFsharp font-resolver registration that executables run once at the composition root</summary>
/// <remarks>
/// <para><see cref="PdfSharp.Drawing.XFont"/> construction throws while <see cref="GlobalFontSettings.FontResolver"/> holds no resolver, and <see cref="FailsafeFontResolver"/> maps every request to a bundled SegoeWP face</para>
/// <para>The setter ignores a repeated assignment of the same resolver type and throws for a different type after the first font use</para>
/// </remarks>
public static class PdfInterop {
    /// <summary>Assigns the PDFsharp failsafe font resolver</summary>
    public static void Initialize() => GlobalFontSettings.FontResolver = new FailsafeFontResolver();
}
