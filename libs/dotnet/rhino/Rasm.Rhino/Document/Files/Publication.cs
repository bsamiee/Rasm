using Mono.Unix;
using Mono.Unix.Native;

namespace Rasm.Rhino.Document.Files;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FilePublication {
    private const int FileExists = unchecked((int)0x80070050);
    private const int AlreadyExists = unchecked((int)0x800700B7);

    public static IO<Unit> Write(OutputPath path, bool overwrite, Func<FileStream, IO<Unit>> encode) =>
        from environment in IO.env
        from written in (
            from stream in Refusing(path, FileReason.Unwritable, IO.lift(() => {
                _ = Directory.CreateDirectory(path.Folder);
                return new FileStream(Path.Join(path.Folder, $"{Path.GetRandomFileName()}.tmp"), new FileStreamOptions {
                    Mode = FileMode.CreateNew, Access = FileAccess.ReadWrite, Share = FileShare.None, Options = FileOptions.Asynchronous,
                });
            }))
            from published in Callbacks.Each(Seq(
                Callbacks.Each(Seq(
                        Refusing(path, FileReason.Unwritable, IO.liftVAsync(() => encode(stream).Bracket().RunAsync(environment))),
                        Refusing(path, FileReason.Unwritable, IO.liftAsync(async () => {
                            await stream.DisposeAsync().ConfigureAwait(false);
                            return unit;
                        }))))
                    .Bind(_ => Linked(stream.Name, path, overwrite, environment.Token)),
                Refusing(path, FileReason.Unwritable, IO.lift(() => File.Delete(stream.Name)))))
            select unit).Uninterruptible()
        select written;

    internal static IO<T> Refusing<T>(string path, FileReason reason, IO<T> access) =>
        access.MapFail(error => error.HasException<IOException>() || error.HasException<UnauthorizedAccessException>() ? new FileRefused(path, reason, error) : error);

    private static IO<Unit> Linked(string staged, OutputPath path, bool overwrite, CancellationToken token) =>
        Refusing(path, FileReason.Unwritable, IO.lift(() => {
            token.ThrowIfCancellationRequested();
            if (overwrite || OperatingSystem.IsWindows())
                File.Move(staged, path, overwrite);
            else
                UnixMarshal.ThrowExceptionForLastErrorIf(Syscall.link(staged, path));
            return unit;
        }).MapFail(error => !overwrite && error.Exception.Case is UnixIOException { ErrorCode: Errno.EEXIST } or IOException { HResult: FileExists or AlreadyExists }
            ? new FileRefused(path, FileReason.Taken, error)
            : error));
}
