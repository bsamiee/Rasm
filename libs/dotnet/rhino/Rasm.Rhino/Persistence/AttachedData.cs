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
        Callbacks.Succeeded(AttachedData.Read(archive, typeCode, codec).Bind(read => IO.lift(() => { State = Some(read); })), CallbackSite.Of(this));

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
        call | @catch(static error => error.HasException<BinaryArchiveException>(), error => IO.fail<T>(new ArchiveFault(member, error)));

    public static IO<Unit> Write<T>(BinaryArchiveWriter writer, uint typeCode, DictionaryCodec<T> codec, T payload) =>
        IO.lift(() => codec.ToDictionary(payload)).Bind(dictionary =>
            IO.lift(() => Refused.Unless(writer.BeginWrite3dmChunk(typeCode, majorVersion: 1, minorVersion: 0), nameof(BinaryArchiveWriter.BeginWrite3dmChunk))).Bracket(
                Use: _ => Faulted(IO.lift(() => writer.WriteDictionary(dictionary)), nameof(BinaryArchiveWriter.WriteDictionary)),
                Fin: _ => IO.lift(() => Refused.Unless(writer.EndWrite3dmChunk(), nameof(BinaryArchiveWriter.EndWrite3dmChunk)))));

    public static IO<T> Read<T>(BinaryArchiveReader reader, uint typeCode, DictionaryCodec<T> codec) =>
        IO.lift(() => Refused.Unless(reader.BeginRead3dmChunk(typeCode, out _, out _), nameof(BinaryArchiveReader.BeginRead3dmChunk))).Bracket(
            Use: _ => Faulted(IO.lift(reader.ReadDictionary), nameof(BinaryArchiveReader.ReadDictionary)).Bind(dictionary => IO.lift(codec.FromDictionary(dictionary))),
            Fin: _ => IO.lift(() => Refused.Unless(reader.EndRead3dmChunk(suppressPartiallyReadChunkWarning: true), nameof(BinaryArchiveReader.EndRead3dmChunk))));

    public static IO<Option<T>> Read<T>(File3dm archive, Guid plugIn, Func<BinaryArchiveReader, IO<T>> read) =>
        from found in IO.lift(() => Conversions.Rows(archive.PlugInData).Find(data => data.PlugInId == plugIn))
        from payload in found.Traverse(data => Callbacks.Captured<T>(
            answer => IO.lift(() => {
                _ = archive.PlugInData.TryRead(data, (_, reader) => {
                    Fin<T> outcome = Try.lift(read(reader).Run).Run();
                    answer(outcome);
                    return outcome.IsSucc;
                });
            }),
            nameof(File3dmPlugInDataTable.TryRead))).As()
        select payload;

    // --- [USER_DATA]
    public static IO<Option<TData>> Find<TData>(CommonObject target) where TData : UserData =>
        IO.lift(() => Optional(target.UserData.Find(typeof(TData)) as TData));

    public static IO<Unit> Set<TData, TState>(CommonObject target, TState state)
        where TData : DefinedUserData<TState>, new()
        where TState : notnull =>
        from found in Find<TData>(target)
        from held in found.Match(
            Some: static data => IO.pure(data),
            None: () => IO.lift(static () => new TData()).Bind(data => DisposalOps.OnFailure(
                IO.lift(() => Refused.Unless(target.UserData.Add(data), data, nameof(UserDataList.Add)))
                    | @catch(static error => error.HasException<ArgumentException>(), static error => IO.fail<TData>(new NotAttachable(typeof(TData), error))),
                IO.lift(data.Dispose))))
        from stored in IO.lift(() => { held.State = Some(state); })
        select stored;

    public static IO<Unit> Remove<TData>(CommonObject target) where TData : UserData =>
        Find<TData>(target).Bind(found => found.Match(
            Some: held => use(() => held).Bind(owned => IO.lift(() => Refused.Unless(target.UserData.Remove(owned), nameof(UserDataList.Remove)))).Bracket(),
            None: static () => IO.pure(unit)));

    public static IO<Unit> Move(CommonObject source, CommonObject destination, bool append) =>
        IO.lift(() => UserData.MoveUserDataTo(destination, UserData.MoveUserDataFrom(source), append));
}
