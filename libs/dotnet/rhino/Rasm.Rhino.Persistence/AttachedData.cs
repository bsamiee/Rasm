using Rasm.Rhino.Document;
using Rhino.Collections;
using Rhino.DocObjects.Custom;
using Rhino.FileIO;
using Rhino.PlugIns;
using Rhino.Runtime;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ChunkFrame(uint TypeCode, int Major, int Minor);

public sealed record UserDataCallbacks(
    Action<Error> Reject,
    ChunkFrame Frame,
    Func<int, int, bool> SupportsVersion,
    string Description,
    bool ShouldWrite,
    IO<ArchivableDictionary> Write,
    Func<ChunkFrame, ArchivableDictionary, IO<Unit>> Read,
    Option<Func<UserData, IO<Unit>>> OnDuplicate,
    Option<Func<Transform, IO<Unit>>> OnTransform);

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class CallbackUserData : UserData {
    protected abstract UserDataCallbacks Callbacks { get; }

    public sealed override string Description => Callbacks.Description;

    public sealed override bool ShouldWrite => Callbacks.ShouldWrite;

    protected sealed override bool Write(BinaryArchiveWriter archive) =>
        Answers.Succeeded(Callbacks.Write.Bind(dictionary => AttachedData.WriteChunk(archive, Callbacks.Frame, dictionary)), Callbacks.Reject);

    protected sealed override bool Read(BinaryArchiveReader archive) =>
        Answers.Succeeded(
            AttachedData.ReadChunk(archive, Callbacks.Frame.TypeCode, Callbacks.SupportsVersion).Bind(read => Callbacks.Read(read.Frame, read.Dictionary)),
            Callbacks.Reject);

    protected sealed override void OnDuplicate(UserData source) =>
        _ = Answers.Answer(Callbacks.OnDuplicate.Map(duplicated => duplicated(source)), Callbacks.Reject, static () => unit);

    protected sealed override void OnTransform(Transform transform) {
        base.OnTransform(transform);
        _ = Answers.Answer(Callbacks.OnTransform.Map(transformed => transformed(transform)), Callbacks.Reject, static () => unit);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AttachedData {
    // --- [CHUNKS]
    public static IO<Unit> WriteChunk(BinaryArchiveWriter writer, ChunkFrame frame, ArchivableDictionary dictionary) =>
        from opened in IO.lift(() => Refused.Unless(writer.BeginWrite3dmChunk(frame.TypeCode, frame.Major, frame.Minor), nameof(BinaryArchiveWriter.BeginWrite3dmChunk)))
        from closed in Disposal.Bracketed(
            IO.lift(() => writer.EnableCRCCalculation(enable: true)),
            prior => Released(() => writer.EnableCRCCalculation(prior), writer.EndWrite3dmChunk, nameof(BinaryArchiveWriter.EndWrite3dmChunk)),
            _ => Guarded(IO.lift(() => writer.WriteDictionary(dictionary)), nameof(BinaryArchiveWriter.WriteDictionary), () => writer.WriteErrorOccured))
        select closed;

    public static IO<(ChunkFrame Frame, ArchivableDictionary Dictionary)> ReadChunk(BinaryArchiveReader reader, uint typeCode, Func<int, int, bool> readable) =>
        from frame in IO.lift(() => Refused.Unless(reader.BeginRead3dmChunk(typeCode, out int major, out int minor), new ChunkFrame(typeCode, major, minor), nameof(BinaryArchiveReader.BeginRead3dmChunk)))
        from supported in unless(
            readable(frame.Major, frame.Minor),
            IO.lift(() => reader.EndRead3dmChunk(suppressPartiallyReadChunkWarning: true)).Bind(_ => IO.fail<Unit>(new UnreadableVersion(typeCode, frame.Major, frame.Minor)))).As()
        from dictionary in Disposal.Bracketed(
            IO.lift(() => reader.EnableCRCCalculation(enable: true)),
            prior => Released(() => reader.EnableCRCCalculation(prior), () => reader.EndRead3dmChunk(suppressPartiallyReadChunkWarning: false), nameof(BinaryArchiveReader.EndRead3dmChunk)),
            _ => Guarded(IO.lift(reader.ReadDictionary), nameof(BinaryArchiveReader.ReadDictionary), () => reader.ReadErrorOccured))
        select (frame, dictionary);

    private static IO<A> Guarded<A>(IO<A> call, string member, Func<bool> errorOccurred) =>
        call.IfFail(error => IO.fail<A>(error.HasException<BinaryArchiveException>() ? new ArchiveFault(member, errorOccurred()) : error));

    private static IO<Unit> Released(Action restore, Func<bool> end, string member) =>
        IO.lift(restore).Bind(_ => IO.lift(() => Refused.Unless(end(), member)));

    // --- [USER_DATA]
    public static IO<Seq<UserData>> Rows(CommonObject target) =>
        IO.lift(() => Answers.Present(target.UserData));

    public static IO<Option<T>> Find<T>(CommonObject target) where T : UserData =>
        IO.lift(() => Optional(target.UserData.Find(typeof(T))).Map(static found => (T)found));

    public static IO<Unit> Attach(CommonObject target, UserData value) =>
        from plugged in IO.lift(() => Missing.Unless(PlugIn.Find(value.GetType().Assembly) is not null, nameof(PlugIn.Find)))
        from added in IO.lift(() => Refused.Unless(target.UserData.Add(value), nameof(UserDataList.Add)))
            .Catch(static error => error.HasException<ArgumentException>(), _ => IO.fail<Unit>(new NotAttachable(value.GetType())))
        select added;

    public static IO<UserData> Detach(CommonObject target, UserData value) =>
        IO.lift(() => Refused.Unless(target.UserData.Remove(value), value, nameof(UserDataList.Remove)));

    public static IO<Unit> Move(CommonObject source, CommonObject destination, bool append) =>
        from id in IO.lift(() => Answers.NonEmpty(UserData.MoveUserDataFrom(source), nameof(UserData.MoveUserDataFrom)))
        from moved in IO.lift(() => UserData.MoveUserDataTo(destination, id, append))
        select moved;

    public static IO<ArchivableDictionary> UserDictionary(CommonObject target) =>
        IO.lift(() => Missing.Unless(target.UserDictionary, nameof(CommonObject.UserDictionary)));
}
