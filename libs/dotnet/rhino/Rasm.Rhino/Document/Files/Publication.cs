using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Mono.Unix;
using Mono.Unix.Native;

namespace Rasm.Rhino.Document.Files;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class FilePublication {

    // --- [WRITES]

    private const int FileExists = unchecked((int)0x80070050);
    private const int AlreadyExists = unchecked((int)0x800700B7);

    public static IO<Unit> Write(OutputPath path, bool overwrite, Func<FileStream, IO<Unit>> encode) =>
        (from staged in Faulted(path, RhinoReason.Unwritable, IO.lift(() => Path.Join(Directory.CreateDirectory(path.Folder).FullName, $"{Path.GetRandomFileName()}.tmp")))
         from published in (from encoded in Faulted(path, RhinoReason.Unwritable, Encoded(staged, encode))
                            from linked in Published(staged, path, overwrite)
                            select linked)
             .Finally(Faulted(path, RhinoReason.Unwritable, IO.lift(() => File.Delete(staged))))
         select published).Uninterruptible();

    internal static IO<T> Faulted<T>(string path, RhinoReason reason, IO<T> access) =>
        access.MapFail(error => error.HasException<IOException>() || error.HasException<UnauthorizedAccessException>() ? new RhinoFault(path, reason, Inner: error) : error);

    private static IO<Unit> Published(string staged, OutputPath path, bool overwrite) =>
        Faulted(path, RhinoReason.Unwritable, cancelToken.Bind(token => IO.lift(() => {
            token.ThrowIfCancellationRequested();
            if (overwrite || OperatingSystem.IsWindows())
                File.Move(staged, path, overwrite);
            else
                UnixMarshal.ThrowExceptionForLastErrorIf(Syscall.link(staged, path));
            return unit;
        })).MapFail(error => !overwrite && error.Exception.Case is UnixIOException { ErrorCode: Errno.EEXIST } or IOException { HResult: FileExists or AlreadyExists }
            ? new RhinoFault(path, RhinoReason.Taken, Inner: error)
            : error));

    private static IO<Unit> Encoded(string staged, Func<FileStream, IO<Unit>> encode) =>
        IO.lift(() => new FileStream(staged, new FileStreamOptions { Mode = FileMode.CreateNew, Access = FileAccess.ReadWrite, Share = FileShare.None, Options = FileOptions.Asynchronous }))
            .Bracket(Use: stream => encode(stream).Bracket(), Fin: static stream => IO.liftVAsync(() => stream.DisposeAsync().ToUnit()));

    // --- [JSON]

    public static IO<Option<TValue>> Read<TRecord, TValue>(string path, JsonTypeInfo<TRecord> info, Func<TRecord, Fin<TValue>> decode) where TRecord : class =>
        IO.lift(() => {
            using FileStream stream = File.OpenRead(path);
            return Conversions.Required(JsonSerializer.Deserialize(stream, info)).Map(Some);
        })
            .Catch(static error => error.HasException<FileNotFoundException>() || error.HasException<DirectoryNotFoundException>(), static _ => IO.pure(Option<TRecord>.None))
            .Bind(found => found.Traverse(record => IO.lift(() => decode(record))).As())
            .Catch(static error => error.IsExpected || error.HasException<JsonException>(),
                error => IO.fail<Option<TValue>>(new RhinoFault(path, RhinoReason.Unreadable, Inner: error)));

    public static IO<Unit> Write<TRecord>(OutputPath path, TRecord value, JsonTypeInfo<TRecord> info, bool overwrite = true) =>
        Write(path, overwrite, stream => IO.liftAsync(async environment => {
            await JsonSerializer.SerializeAsync(stream, value, info, environment.Token).ConfigureAwait(false);
            return unit;
        }));
}
