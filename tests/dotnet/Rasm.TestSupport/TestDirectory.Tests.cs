namespace Rasm.TestSupport;

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class TestDirectoryTests {
    [Fact]
    public void PathsResolveInsideTheRootAndDisposeRemovesIt() {
        string root;
        using (TestDirectory directory = TestDirectory.Create("support")) {
            root = directory.Root.FullName;
            FileInfo file = directory.File("nested/value.txt");
            Assert.StartsWith(root, file.FullName, StringComparison.Ordinal);
            Assert.True(directory.CreateDirectory("nested").Exists);
            File.WriteAllText(file.FullName, "value");
            Assert.True(File.Exists(file.FullName));
        }
        Assert.False(Directory.Exists(root));
    }

    [Theory]
    [InlineData("..")]
    [InlineData("../escape.txt")]
    [InlineData("nested/../../escape.txt")]
    public void PathsThatLeaveTheRootAreRejected(string relativePath) {
        using TestDirectory directory = TestDirectory.Create("support");
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => directory.File(relativePath));
    }

    [Fact]
    public void AbsolutePathsAreRejected() {
        using TestDirectory directory = TestDirectory.Create("support");
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => directory.File(directory.Root.Parent!.FullName));
    }
}
