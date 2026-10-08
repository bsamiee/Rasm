using Rhino.DocObjects.Custom;
using Rhino.FileIO;
using Rhino.Runtime;

namespace Rasm.Rhino.Persistence;

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedUserData<TState>(uint typeCode, DictionaryCodec<TState> codec, Option<Func<TState, Transform, TState>> transformed) : UserData
    where TState : notnull {
    public Option<TState> State { get; internal set; }

    public sealed override string Description => GetType().Name;

    public sealed override bool ShouldWrite => State.IsSome;

    protected sealed override bool Write(BinaryArchiveWriter archive) =>
        Callbacks.Succeeded(State.Map(held => AttachedData.Write(archive, typeCode, codec, held)), static () => false, CallbackSite.Of(this));

    protected sealed override bool Read(BinaryArchiveReader archive) =>
        Callbacks.Answer(
            from read in AttachedData.Read(archive, typeCode, codec)
            from stored in IO.lift(() => { State = Some(read); })
            select true,
            static () => false, CallbackSite.Of(this));

    protected sealed override void OnTransform(Transform transform) {
        base.OnTransform(transform);
        State = (transformed, State).Apply((move, held) => move(held, transform)).As() || State;
    }

    protected sealed override void OnDuplicate(UserData source) => State = ((DefinedUserData<TState>)source).State;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AttachedData {
    // --- [FRAMES]
    private static IO<T> Faulted<T>(IO<T> call, string member) =>
        call.Catch(static error => error.HasException<BinaryArchiveException>(), error => IO.fail<T>(new ArchiveFault(member, error)));

    public static IO<Unit> Write<T>(BinaryArchiveWriter writer, uint typeCode, DictionaryCodec<T> codec, T payload) =>
        from dictionary in IO.lift(() => codec.ToDictionary(payload))
        from written in IO.lift(() => Refused.Unless(writer.BeginWrite3dmChunk(typeCode, majorVersion: 1, minorVersion: 0), nameof(BinaryArchiveWriter.BeginWrite3dmChunk))).Bracket(
            Use: _ => Faulted(IO.lift(() => writer.WriteDictionary(dictionary)), nameof(BinaryArchiveWriter.WriteDictionary)),
            Fin: _ => IO.lift(() => Refused.Unless(writer.EndWrite3dmChunk(), nameof(BinaryArchiveWriter.EndWrite3dmChunk))))
        select written;

    public static IO<T> Read<T>(BinaryArchiveReader reader, uint typeCode, DictionaryCodec<T> codec) =>
        from dictionary in IO.lift(() => Refused.Unless(reader.BeginRead3dmChunk(typeCode, out _, out _), nameof(BinaryArchiveReader.BeginRead3dmChunk))).Bracket(
            Use: _ => Faulted(IO.lift(reader.ReadDictionary), nameof(BinaryArchiveReader.ReadDictionary)),
            Fin: _ => IO.lift(() => Refused.Unless(reader.EndRead3dmChunk(suppressPartiallyReadChunkWarning: true), nameof(BinaryArchiveReader.EndRead3dmChunk))))
        from payload in IO.lift(() => codec.FromDictionary(dictionary))
        select payload;

    public static IO<Option<T>> Read<T>(File3dm archive, Guid plugIn, Func<BinaryArchiveReader, IO<T>> read) =>
        from found in IO.lift(() => Conversions.Rows(archive.PlugInData).Find(data => data.PlugInId == plugIn))
        from payload in found.Traverse((
            from data in Eff.runtime<File3dmPlugInData>()
            from captured in Callbacks.Captured<T>((
                from answer in Eff.runtime<Action<Fin<T>>>()
                from called in IO.lift(() => {
                    _ = archive.PlugInData.TryRead(data, (_, reader) => {
                        Fin<T> outcome = Try.lift(read(reader).Run).Run();
                        answer(outcome);
                        return outcome.IsSucc;
                    });
                })
                select called).RunIO, nameof(File3dmPlugInDataTable.TryRead))
            select captured).RunIO).As()
        select payload;

    // --- [USER_DATA]
    public static IO<Option<TData>> Find<TData>(CommonObject target) where TData : UserData =>
        IO.lift(() => Optional(target.UserData.Find(typeof(TData)) as TData));

    public static IO<Unit> Set<TData, TState>(CommonObject target, TState state)
        where TData : DefinedUserData<TState>, new()
        where TState : notnull =>
        from found in Find<TData>(target)
        from held in found.Match(
            Some: IO.pure,
            None:
                from data in IO.lift(static () => new TData())
                from attached in DisposalOps.OnFailure(
                    IO.lift(() => Refused.Unless(target.UserData.Add(data), data, nameof(UserDataList.Add)))
                        .Catch(static error => error.HasException<ArgumentException>(), static error => IO.fail<TData>(new NotAttachable(typeof(TData), error))),
                    IO.lift(data.Dispose))
                select attached)
        from stored in IO.lift(() => { held.State = Some(state); })
        select stored;

    public static IO<Unit> Remove<TData>(CommonObject target) where TData : UserData =>
        from found in Find<TData>(target)
        from removed in found.Traverse(held => IO.lift(() => Refused.Unless(target.UserData.Remove(held), nameof(UserDataList.Remove)))
            .Finally(IO.lift(held.Dispose))).As()
        select unit;

    public static IO<Unit> Move(CommonObject source, CommonObject destination, bool append) =>
        IO.lift(() => UserData.MoveUserDataTo(destination, UserData.MoveUserDataFrom(source), append));
}
