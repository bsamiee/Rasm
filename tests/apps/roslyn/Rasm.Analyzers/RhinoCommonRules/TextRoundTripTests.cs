using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Rasm.Rhino.Analyzers;
using Xunit;

namespace Rasm.Analyzers.Tests.RhinoCommonRules;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class TextRoundTripTests : CSharpCodeFixVerifier<TextRoundTripAnalyzer, EmptyCodeFixProvider> {
    [Fact(DisplayName = "JSON, encoded-string, Base64, and Draco string conversions are reported; exemplar meziantou/Meziantou.Analyzer@27cbef6 tests/Meziantou.Analyzer.Test/Rules/UseGuidEmptyAnalyzerTests.cs ShouldReportError")]
    public Task TextConversions() => VerifyAnalyzerAsync(
        """
        using System;
        using Rhino.FileIO;
        using Rhino.Geometry;
        using Rhino.Runtime;

        internal static class TextConversions {
            internal static string Json(Mesh mesh, SerializationOptions options) => {|#0:mesh.ToJSON(options)|};

            internal static CommonObject FromJson(string text) => {|#1:CommonObject.FromJSON(text)|};

            internal static CommonObject FromBase64(string text) => {|#2:CommonObject.FromBase64String(8, 0, text)|};

            internal static string Encoded(MeshingParameters parameters) => {|#3:parameters.ToEncodedString()|};

            internal static MeshingParameters Decoded(string text) => {|#4:MeshingParameters.FromEncodedString(text)|};

            internal static string Subdivided(SubDDisplayParameters parameters) => {|#5:parameters.ToEncodedString()|};

            internal static string Compressed(DracoCompression compressed) => {|#6:compressed.ToBase64String()|};

            internal static GeometryBase Decompressed(string text) => {|#7:DracoCompression.DecompressBase64String(text)|};

            internal static Func<string, GeometryBase> Grouped => {|#8:DracoCompression.DecompressBase64String|};
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("CommonObject.ToJSON"),
        Diagnostic().WithLocation(1).WithArguments("CommonObject.FromJSON"),
        Diagnostic().WithLocation(2).WithArguments("CommonObject.FromBase64String"),
        Diagnostic().WithLocation(3).WithArguments("MeshingParameters.ToEncodedString"),
        Diagnostic().WithLocation(4).WithArguments("MeshingParameters.FromEncodedString"),
        Diagnostic().WithLocation(5).WithArguments("SubDDisplayParameters.ToEncodedString"),
        Diagnostic().WithLocation(6).WithArguments("DracoCompression.ToBase64String"),
        Diagnostic().WithLocation(7).WithArguments("DracoCompression.DecompressBase64String"),
        Diagnostic().WithLocation(8).WithArguments("DracoCompression.DecompressBase64String"));

    [Fact(DisplayName = "Held objects, byte conversions, BCL Base64, and nameof are clean; exemplar meziantou/Meziantou.Analyzer@27cbef6 tests/Meziantou.Analyzer.Test/Rules/UseGuidEmptyAnalyzerTests.cs ShouldNotReportError")]
    public Task HeldObjects() => VerifyAnalyzerAsync(
        """
        using System;
        using Rhino.FileIO;
        using Rhino.Geometry;
        using Rhino.Runtime;

        internal static class HeldObjects {
            internal static GeometryBase Copied(Mesh mesh) => mesh.Duplicate();

            internal static byte[] Bytes(DracoCompression compressed) => compressed.ToByteArray();

            internal static GeometryBase FromBytes(byte[] bytes) => DracoCompression.DecompressByteArray(bytes);

            internal static string Text(byte[] bytes) => Convert.ToBase64String(bytes);

            internal static byte[] Raw(string text) => Convert.FromBase64String(text);

            internal static string Named => nameof(CommonObject.FromJSON);
        }
        """);
}
