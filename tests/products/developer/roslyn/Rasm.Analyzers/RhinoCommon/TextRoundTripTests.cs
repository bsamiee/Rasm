using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.RhinoCommon;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class TextRoundTripTests : CSharpCodeFixVerifier<TextRoundTripAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task RhinoCommonTextConversionsAreReportedWhileSameNamedBclAndByteConversionsAreSilent() => VerifyAnalyzerAsync(
        """
        using System;
        using Rhino.FileIO;
        using Rhino.Geometry;
        using Rhino.Runtime;

        internal static class Payloads {
            internal static string Json(Mesh mesh, SerializationOptions options) => {|#0:mesh.ToJSON(options)|};

            internal static MeshingParameters Meshing(string text) => {|#1:MeshingParameters.FromEncodedString(text)|};

            internal static string Subdivided(SubDDisplayParameters parameters) => {|#2:parameters.ToEncodedString()|};

            internal static Func<string, GeometryBase> Decompressed => {|#3:DracoCompression.DecompressBase64String|};

            internal static string Compressed(DracoCompression compressed) => {|#4:compressed.ToBase64String()|};

            internal static string Text(byte[] bytes) => Convert.ToBase64String(bytes);

            internal static GeometryBase Bytes(byte[] bytes) => DracoCompression.DecompressByteArray(bytes);
        }
        """,
        Diagnostic().WithLocation(0).WithArguments("CommonObject.ToJSON"),
        Diagnostic().WithLocation(1).WithArguments("MeshingParameters.FromEncodedString"),
        Diagnostic().WithLocation(2).WithArguments("SubDDisplayParameters.ToEncodedString"),
        Diagnostic().WithLocation(3).WithArguments("DracoCompression.DecompressBase64String"),
        Diagnostic().WithLocation(4).WithArguments("DracoCompression.ToBase64String"));
}
