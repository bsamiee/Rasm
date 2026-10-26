using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Rasm.Rhino.Document.Files;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class JsonFiles {
    public static IO<Option<TValue>> Read<TRecord, TValue>(string path, JsonTypeInfo<TRecord> info, Func<TRecord, Fin<TValue>> decode) where TRecord : class =>
        IO.lift(() => Some(File.ReadAllBytes(path)))
            .Catch(static error => error.HasException<FileNotFoundException>() || error.HasException<DirectoryNotFoundException>(), static _ => IO.pure(Option<byte[]>.None))
            .Bind(found => found.Traverse(bytes =>
                IO.lift(() => Refused.UnlessFound(JsonSerializer.Deserialize(bytes, info), nameof(JsonSerializer.Deserialize)).Bind(decode))
                    .Catch(static error => error.IsExpected || error.HasException<JsonException>(),
                        error => IO.fail<TValue>(new FileRefused(path, FileReason.Unreadable, error)))).As());

    public static IO<Unit> Write<TRecord>(OutputPath path, TRecord value, JsonTypeInfo<TRecord> info, bool overwrite = true) =>
        FilePublication.Write(path, overwrite, stream => IO.liftAsync(async environment => {
            await JsonSerializer.SerializeAsync(stream, value, info, environment.Token).ConfigureAwait(false);
            return unit;
        }));
}
