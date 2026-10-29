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
        (from stream in Faulted(path, RhinoReason.Unwritable, use(() => {
            _ = Directory.CreateDirectory(path.Folder);
            return new FileStream(Path.Join(path.Folder, $"{Path.GetRandomFileName()}.tmp"), new FileStreamOptions {
                Mode = FileMode.CreateNew, Access = FileAccess.ReadWrite, Share = FileShare.None, Options = FileOptions.Asynchronous,
            });
        }))
         from published in (from encoded in Faulted(path, RhinoReason.Unwritable, encode(stream).Bracket().Finally(release(stream)))
                            from linked in Published(stream.Name, path, overwrite)
                            select linked)
             .Finally(Faulted(path, RhinoReason.Unwritable, IO.lift(() => File.Delete(stream.Name))))
         select published).Bracket().Uninterruptible();

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

    // --- [JSON]
    public static IO<Option<TValue>> Read<TRecord, TValue>(string path, JsonTypeInfo<TRecord> info, Func<TRecord, Fin<TValue>> decode) where TRecord : class =>
        IO.lift(() => Some(File.ReadAllBytes(path)))
            .Catch(static error => error.HasException<FileNotFoundException>() || error.HasException<DirectoryNotFoundException>(), static _ => IO.pure(Option<byte[]>.None))
            .Bind(found => found.Traverse(bytes =>
                IO.lift(() => Conversions.Required(JsonSerializer.Deserialize(bytes, info)).Bind(decode))
                    .Catch(static error => error.IsExpected || error.HasException<JsonException>(),
                        error => IO.fail<TValue>(new RhinoFault(path, RhinoReason.Unreadable, Inner: error)))).As());

    public static IO<Unit> Write<TRecord>(OutputPath path, TRecord value, JsonTypeInfo<TRecord> info, bool overwrite = true) =>
        Write(path, overwrite, stream => IO.liftAsync(async environment => {
            await JsonSerializer.SerializeAsync(stream, value, info, environment.Token).ConfigureAwait(false);
            return unit;
        }));
}
