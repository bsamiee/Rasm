using Mono.Unix;
using Mono.Unix.Native;

namespace Rasm.Rhino.Document.Files;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FilePublication {
    private const int FileExists = unchecked((int)0x80070050);
    private const int AlreadyExists = unchecked((int)0x800700B7);

    public static IO<Unit> Write(OutputPath path, bool overwrite, Func<FileStream, IO<Unit>> encode) =>
        (from stream in Refusing(path, RhinoReason.Unwritable, use(() => {
            _ = Directory.CreateDirectory(path.Folder);
            return new FileStream(Path.Join(path.Folder, $"{Path.GetRandomFileName()}.tmp"), new FileStreamOptions {
                Mode = FileMode.CreateNew, Access = FileAccess.ReadWrite, Share = FileShare.None, Options = FileOptions.Asynchronous,
            });
        }))
         from published in (from encoded in Refusing(path, RhinoReason.Unwritable, encode(stream).Bracket().Finally(release(stream)))
                            from linked in Linked(stream.Name, path, overwrite)
                            select linked)
             .Finally(Refusing(path, RhinoReason.Unwritable, IO.lift(() => File.Delete(stream.Name))))
         select published).Bracket().Uninterruptible();

    internal static IO<T> Refusing<T>(string path, RhinoReason reason, IO<T> access) =>
        access.MapFail(error => error.HasException<IOException>() || error.HasException<UnauthorizedAccessException>() ? new RhinoFault(path, reason, Inner: error) : error);

    private static IO<Unit> Linked(string staged, OutputPath path, bool overwrite) =>
        Refusing(path, RhinoReason.Unwritable, cancelToken.Bind(token => IO.lift(() => {
            token.ThrowIfCancellationRequested();
            if (overwrite || OperatingSystem.IsWindows())
                File.Move(staged, path, overwrite);
            else
                UnixMarshal.ThrowExceptionForLastErrorIf(Syscall.link(staged, path));
            return unit;
        })).MapFail(error => !overwrite && error.Exception.Case is UnixIOException { ErrorCode: Errno.EEXIST } or IOException { HResult: FileExists or AlreadyExists }
            ? new RhinoFault(path, RhinoReason.Taken, Inner: error)
            : error));
}
