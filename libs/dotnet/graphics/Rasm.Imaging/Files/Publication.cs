using System.Text;
using Mono.Unix;
using Mono.Unix.Native;

namespace Rasm.Imaging.Files;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OutputPath {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = Path.IsPathFullyQualified(value) && Path.GetFileName(value).Length > 0 ? null : new ValidationError($"Output path '{value}' must be fully qualified and name a file");
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FilePublication {
    public static Error Refusal(string path, Error error) => error switch {
        ManyErrors many => Error.Many(many.Errors.Map(leaf => Refusal(path, leaf))),
        _ => error.HasException<IOException>() || error.HasException<UnauthorizedAccessException>() ? Error.New($"Access to {path} refused", error) : error,
    };

    public static IO<Unit> Write(OutputPath path, bool overwrite, Func<FileStream, IO<Unit>> write) {
        const int FileExists = unchecked((int)0x80070050);
        const int AlreadyExists = unchecked((int)0x800700B7);
        Error Refused(Error error) => Refusal(path, error);
        return
            from environment in IO.env
            from written in (
                from stream in IO.lift(() => {
                    string folder = Path.GetDirectoryName((string)path)!;
                    _ = Directory.CreateDirectory(folder);
                    return new FileStream(Path.Combine(folder, $"{Path.GetRandomFileName()}.tmp"), new FileStreamOptions {
                        Mode = FileMode.CreateNew, Access = FileAccess.ReadWrite, Share = FileShare.None, Options = FileOptions.Asynchronous,
                    });
                }).MapFail(Refused)
                from encoded in Seq<K<IO, Unit>>(
                    IO.liftVAsync(() => write(stream).Bracket().RunAsync(environment)).MapFail(Refused),
                    IO.liftAsync(async () => {
                        await stream.DisposeAsync().ConfigureAwait(false);
                        return unit;
                    }).MapFail(Refused)).Fails().As()
                from published in Seq<K<IO, Unit>>(
                    encoded.IsEmpty
                        ? IO.lift(() => {
                            environment.Token.ThrowIfCancellationRequested();
                            if (overwrite || OperatingSystem.IsWindows())
                                File.Move(stream.Name, path, overwrite);
                            else
                                UnixMarshal.ThrowExceptionForLastErrorIf(Syscall.link(stream.Name, path));
                            return unit;
                        }).MapFail(error => !overwrite && error.Exception.Case is UnixIOException { ErrorCode: Errno.EEXIST } or IOException { HResult: FileExists or AlreadyExists }
                            ? new DestinationTaken(path)
                            : Refused(error))
                        : IO.fail<Unit>(Error.Many(encoded)),
                    IO.lift(() => File.Delete(stream.Name)).MapFail(Refused)).Fails().As()
                from result in IO.lift(guard<Error>(published.IsEmpty, () => Error.Many(published)).ToFin())
                select result).Uninterruptible()
            select written;
    }

    public static IO<Unit> Write(OutputPath path, string text, bool overwrite = true) =>
        Write(path, overwrite, stream => IO.liftAsync(async environment => {
            await stream.WriteAsync(Encoding.UTF8.GetBytes(text), environment.Token).ConfigureAwait(false);
            return unit;
        }));
}
