using PdfSharp.Fonts;
using PdfSharp.Snippets.Font;

namespace Rasm.Interop.Pdf;

/// <summary>PDFsharp font-resolver registration; executables call <see cref="Initialize"/> once at the composition root</summary>
/// <remarks>
/// The first <see cref="PdfSharp.Drawing.XFont"/> construction throws while <see cref="GlobalFontSettings.FontResolver"/> holds no resolver;
/// <see cref="FailsafeFontResolver"/> substitutes a bundled face for every request the platform cannot resolve, and the setter tolerates repeated assignment
/// </remarks>
public static class PdfInterop {
    /// <summary>Assigns the PDFsharp failsafe font resolver</summary>
    public static void Initialize() => GlobalFontSettings.FontResolver = new FailsafeFontResolver();
}
