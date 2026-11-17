using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Members;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class PlatformNameSetTests : CSharpCodeFixVerifier<PlatformMemberAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsPlatformDependentInvalidCharSets() => VerifyAnalyzerAsync(
        """
        using System;
        using System.IO;
        using static System.IO.Path;

        internal static class RunningPlatformSets {
            internal static char[] Names => {|#0:Path.GetInvalidFileNameChars()|};

            internal static char[] Paths => {|#1:System.IO.Path.GetInvalidPathChars()|};

            internal static char[] Imported => {|#2:GetInvalidFileNameChars()|};

            internal static Func<char[]> Grouped => {|#3:Path.GetInvalidPathChars|};
        }
        """,
        Diagnostic("RASM3111").WithLocation(0).WithArguments("Path.GetInvalidFileNameChars()"),
        Diagnostic("RASM3111").WithLocation(1).WithArguments("System.IO.Path.GetInvalidPathChars()"),
        Diagnostic("RASM3111").WithLocation(2).WithArguments("GetInvalidFileNameChars()"),
        Diagnostic("RASM3111").WithLocation(3).WithArguments("Path.GetInvalidPathChars"));

    [Fact]
    public Task IgnoresDeclaredSearchValuesSetAndOtherPathMembers() => VerifyAnalyzerAsync(
        """
        using System;
        using System.Buffers;
        using System.IO;

        internal static class PortableSet {
            private static readonly SearchValues<char> Reserved = SearchValues.Create("<>:\"/\\|?*");

            internal static bool Portable(string name) => !name.AsSpan().ContainsAny(Reserved);

            internal static string Leaf(string path) => Path.GetFileName(path);

            internal static char Separator => Path.DirectorySeparatorChar;

            internal static string Named => nameof(Path.GetInvalidFileNameChars);

            internal static char[] Local => Names.Path.GetInvalidFileNameChars();
        }

        namespace Names {
            internal static class Path {
                internal static char[] GetInvalidFileNameChars() => [];
            }
        }
        """);
}
