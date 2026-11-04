namespace Rasm.TestSupport;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class TestDirectory : IDisposable {
    private TestDirectory(DirectoryInfo root) => Root = root;

    public DirectoryInfo Root { get; }

    public static TestDirectory Create(string prefix) => new(Directory.CreateTempSubdirectory(prefix));

    public DirectoryInfo CreateDirectory(string relativePath) => Directory.CreateDirectory(Resolve(relativePath));

    public FileInfo File(string relativePath) => new(Resolve(relativePath));

    public void Dispose() {
        try { Root.Delete(recursive: true); } catch (DirectoryNotFoundException) { }
    }

    private string Resolve(string relativePath) {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        string resolved = Path.GetFullPath(relativePath, Root.FullName);
        string remainder = Path.GetRelativePath(Root.FullName, resolved);
        bool escapes = Path.IsPathFullyQualified(relativePath)
            || Path.IsPathRooted(remainder)
            || string.Equals(remainder, "..", StringComparison.Ordinal)
            || remainder.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
        return escapes
            ? throw new ArgumentOutOfRangeException(nameof(relativePath), relativePath, "path escapes test directory")
            : resolved;
    }
}
