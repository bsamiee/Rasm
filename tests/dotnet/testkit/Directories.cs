namespace Rasm.TestKit;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class TestDirectory : IDisposable {
    private TestDirectory(DirectoryInfo root) => Root = root;

    public DirectoryInfo Root { get; }

    public static TestDirectory Create(string prefix) {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        return new TestDirectory(Directory.CreateTempSubdirectory(prefix));
    }

    public DirectoryInfo CreateDirectory(string relativePath) => Directory.CreateDirectory(Resolve(relativePath));

    public FileInfo File(string relativePath) => new(Resolve(relativePath));

    public void Dispose() {
        Root.Refresh();
        if (Root.Exists) {
            Root.Delete(recursive: true);
        }
    }

    private string Resolve(string relativePath) {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        string resolved = Path.GetFullPath(relativePath, Root.FullName);
        string remainder = Path.GetRelativePath(Root.FullName, resolved);
        bool escapes = Path.IsPathFullyQualified(relativePath)
            || string.Equals(remainder, "..", StringComparison.Ordinal)
            || remainder.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
        return escapes
            ? throw new ArgumentOutOfRangeException(nameof(relativePath), relativePath, "test path must stay inside its test directory")
            : resolved;
    }
}
