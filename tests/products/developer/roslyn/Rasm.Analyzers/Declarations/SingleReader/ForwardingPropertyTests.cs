using Rasm.Analyzers.CodeFixes;
using Rasm.Analyzers.Tests.Verifiers;
using Xunit;

namespace Rasm.Analyzers.Tests.Declarations.SingleReader;

// --- [OPERATIONS] ----------------------------------------------------------------------

public sealed class ForwardingPropertyTests : CSharpCodeFixVerifier<SingleReaderAnalyzer, SingleReaderCodeFix> {
    [Fact]
    public Task ReportsPropertyForwardingStaticFieldButNotConstructedValue() => VerifyAnalyzerAsync(
        """
        using Rhino.Geometry;

        internal static class Palette {
            internal static System.Collections.Immutable.ImmutableArray<int> Empty => [];

            internal static System.Drawing.Bitmap Blank => new(1, 1);

            internal static System.Drawing.Point Origin => default;

            internal static Point Placed => new(Point3d.Origin);

            internal static Plane {|#0:Base|} => Plane.WorldXY;

            internal static System.Numerics.Vector2 Offset => default;
        }
        """,
        Diagnostic("RASM3012").WithLocation(0).WithArguments("Base", "Plane.WorldXY"));

    [Fact]
    public Task IgnoresPropertiesReadingPrimaryConstructorParameter() => VerifyAnalyzerAsync(
        """
        using System.ComponentModel;
        using System.IO;
        using System.Runtime.CompilerServices;

        internal sealed class FileReads(FileInfo file, FileInfo? optional) {
            internal string Name => file.Name;

            internal long Length {
                get { return file.Length; }
            }

            internal string? Optional => optional?.Name;

            internal string Upper => file.Name.ToUpperInvariant();

            internal double Converted => file.Length;

            [DisplayName("File name")]
            internal string Displayed => file.Name;

            internal long Measured {
                [MethodImpl(MethodImplOptions.NoInlining)]
                get => file.Length;
            }
        }
        """);

    [Fact]
    public Task ReportsPropertiesForwardingPublicPropertyMembers() => VerifyAnalyzerAsync(
        """
        using System.IO;

        internal sealed class PublicFileReads(FileInfo file, FileInfo? optional) {
            public FileInfo File { get; } = file;

            public FileInfo? OptionalFile { get; } = optional;

            internal string {|#0:Name|} => File.Name;

            internal long {|#1:Length|} {
                get { return File.Length; }
            }

            internal string? {|#2:Optional|} => OptionalFile?.Name;
        }
        """,
        Diagnostic("RASM3012").WithLocation(0).WithArguments("Name", "File.Name"),
        Diagnostic("RASM3012").WithLocation(1).WithArguments("Length", "File.Length"),
        Diagnostic("RASM3012").WithLocation(2).WithArguments("Optional", "OptionalFile?.Name"));

    [Fact]
    public Task ReportsForwardOfPublicPropertyButNotPrivateField() => VerifyAnalyzerAsync(
        """
        using System.IO;

        internal sealed class OwnedReads(FileInfo file) {
            private readonly FileInfo Source = file;

            public FileInfo File { get; } = file;

            internal string {|#0:Owned|} => File.Name;

            internal string Uppercase => Source.Name;

            internal string Explicit => this.Source.Name;
        }
        """,
        Diagnostic("RASM3012").WithLocation(0).WithArguments("Owned", "File.Name"));

    [Fact]
    public Task ReportsPropertiesForwardingStaticMembers() => VerifyAnalyzerAsync(
        """
        using System;
        using Calendar = System.DateTime;

        internal static class StaticReads {
            internal static DateTime {|#0:Utc|} => Calendar.UtcNow;

            internal static int {|#1:Maximum|} => int.MaxValue;
        }
        """,
        Diagnostic("RASM3012").WithLocation(0).WithArguments("Utc", "Calendar.UtcNow"),
        Diagnostic("RASM3012").WithLocation(1).WithArguments("Maximum", "int.MaxValue"));

    [Fact]
    public Task IgnoresOverrideOfAbstractProperty() => VerifyAnalyzerAsync(
        """
        using System.IO;

        internal abstract class LengthContract {
            internal abstract long Length { get; }
        }

        internal sealed class OverriddenRead(FileInfo file) : LengthContract {
            internal override long Length => file.Length;
        }
        """);

    [Fact]
    public Task IgnoresVirtualProperty() => VerifyAnalyzerAsync(
        """
        using System.Collections.Generic;

        internal class ExtensibleRead(List<int> values) {
            internal virtual int Count => values.Count;
        }
        """);

    [Fact]
    public Task IgnoresPropertyHidingBaseMember() => VerifyAnalyzerAsync(
        """
        using System.Collections.Generic;

        internal class CountContract {
            internal int Count => 1;
        }

        internal sealed class HiddenRead(List<int> values) : CountContract {
            internal new int Count => values.Count;
        }
        """);

    [Fact]
    public Task ReportsPrivateAliasButNotWhenShadowedOrReadInNameof() => VerifyAnalyzerAsync(
        """
        using System.IO;

        internal sealed class PrivateAliases(FileInfo file) {
            private readonly FileInfo Source = file;

            private string {|#0:Name|} => Source.Name;

            private long {|#1:Length|} => this.Source.Length;

            private string ShadowedName => Source.Name;

            private string NamedName => Source.Name;

            internal string ReadName() => Name;

            internal long ReadLength() => Length;

            internal string ReadShadowed(FileInfo Source) => ShadowedName + Source.Name;

            internal string ReadNamed() => NamedName + nameof(NamedName);
        }
        """,
        Diagnostic("RASM3012").WithLocation(0).WithArguments("Name", "Source.Name"),
        Diagnostic("RASM3012").WithLocation(1).WithArguments("Length", "this.Source.Length"));

    [Fact]
    public Task IgnoresForwardOfInternalProtectedOrPrivateGetterMember() => VerifyAnalyzerAsync(
        """
        using System.IO;

        internal class RestrictedReads(FileInfo file) {
            internal FileInfo Shared { get; } = file;

            protected FileInfo Protected { get; } = file;

            public FileInfo Written { private get; set; } = file;

            internal string SharedName => Shared.Name;

            internal string ProtectedName => Protected.Name;

            internal string WrittenName => Written.Name;
        }
        """);

    [Fact]
    public Task ReportsForwardOfGenericMemberButNotOverInternalTypeArgument() => VerifyAnalyzerAsync(
        """
        public static class GenericReads<T> {
            public static int Count => typeof(T).GetFields().Length;
        }

        internal sealed class HiddenType;

        internal static class ConstructedReads {
            internal static int {|#0:Public|} => GenericReads<string>.Count;

            internal static int Hidden => GenericReads<HiddenType>.Count;
        }
        """,
        Diagnostic("RASM3012").WithLocation(0).WithArguments("Public", "GenericReads<string>.Count"));

    [Fact]
    public Task ReportsPrivatePropertyForwardingStaticMemberOfAnotherType() => VerifyAnalyzerAsync(
        """
        using Rhino;

        internal static class Entries {
            private static double {|#0:MissingNumber|} => RhinoMath.UnsetValue;

            internal static double Number() => MissingNumber;
        }
        """,
        Diagnostic("RASM3012").WithLocation(0).WithArguments("MissingNumber", "RhinoMath.UnsetValue"));
}
