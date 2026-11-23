using Microsoft.CodeAnalysis.Testing;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Members;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class PlatformNameSetTests : CSharpCodeFixVerifier<PlatformMemberAnalyzer, EmptyCodeFixProvider> {
    [Fact]
    public Task ReportsTheRunningPlatformsInvalidCharSetsAndIgnoresOtherPathMembersAndSameNamedSetsElsewhere() => VerifyAnalyzerAsync(
        """
        using System;
        using System.IO;

        internal static class Sets {
            internal static char[] FileNames => {|RASM3111:Path.GetInvalidFileNameChars()|};
            internal static Func<char[]> Paths => {|RASM3111:Path.GetInvalidPathChars|};
            internal static string Leaf(string path) => Path.GetFileName(path);
            internal static char[] Local => Names.Path.GetInvalidFileNameChars();
        }

        namespace Names {
            internal static class Path {
                internal static char[] GetInvalidFileNameChars() => [];
            }
        }
        """);
}
