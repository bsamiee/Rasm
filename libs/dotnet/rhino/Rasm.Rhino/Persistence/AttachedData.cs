using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI;
using Rhino.Collections;
using Rhino.DocObjects.Custom;
using Rhino.FileIO;
using Rhino.Runtime;

namespace Rasm.Rhino.Persistence;

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedUserData<TState> : UserData where TState : notnull {
    // --- [STATE]
    private readonly uint typeCode;
    private readonly DictionaryCodec<TState> codec;
    private readonly Option<Func<TState, Transform, TState>> transformed;
    private readonly IPlugInViews owner;

    protected DefinedUserData(uint typeCode, DictionaryCodec<TState> codec, Option<Func<TState, Transform, TState>> transformed) =>
        (this.typeCode, this.codec, this.transformed, owner) = (typeCode, codec, transformed, HostedTypes.Owner(GetType()));

    public Option<TState> State { get; private set; }

    internal Option<ArchivableDictionary> Newer { get; private set; }

    public sealed override string Description => GetType().Name;

    public sealed override bool ShouldWrite => State.IsSome || Newer.IsSome;

    internal IO<Unit> Assign(TState state) =>
        Newer.Match(Some: raw => IO.fail<Unit>(codec.Refusal(raw)), None: () => IO.lift(() => { State = Some(state); }));

    protected sealed override bool Write(BinaryArchiveWriter archive) =>
        Callbacks.Succeeded(
            Newer.Map(raw => AttachedData.Write(archive, typeCode, raw)) | State.Map(held => AttachedData.Write(archive, typeCode, codec, held)),
            static () => false, CallbackSite.Of(owner, this));

    protected sealed override bool Read(BinaryArchiveReader archive) =>
        Callbacks.Answer(
            from read in AttachedData.Read(archive, typeCode, codec)
            from stored in IO.lift(() => {
                (State, Newer) = read.Switch(
                    held: static held => (Some(held.Value), Option<ArchivableDictionary>.None),
                    absent: static _ => (Option<TState>.None, Option<ArchivableDictionary>.None),
                    newer: static newer => (Option<TState>.None, Some(newer.Raw)));
            })
            select true,
            static () => false, CallbackSite.Of(owner, this));

    protected sealed override void OnTransform(Transform transform) {
        base.OnTransform(transform);
        State = (transformed, State).Apply((move, held) => move(held, transform)).As() || State;
    }

    protected sealed override void OnDuplicate(UserData source) {
        DefinedUserData<TState> held = (DefinedUserData<TState>)source;
        (State, Newer) = (held.State, held.Newer);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AttachedData {
    // --- [FRAMES]
    public static IO<Unit> Write<T>(BinaryArchiveWriter writer, uint typeCode, DictionaryCodec<T> codec, T payload) =>
        IO.lift(() => codec.ToDictionary(payload)).Bind(dictionary => Write(writer, typeCode, dictionary));

    public static IO<Unit> Write(BinaryArchiveWriter writer, uint typeCode, ArchivableDictionary dictionary) =>
        DisposalOps.Within(
            () => writer.BeginWrite3dmChunk(typeCode, majorVersion: 1, minorVersion: 0),
            begun => IO.lift(() => Refused.Unless(begun, nameof(BinaryArchiveWriter.BeginWrite3dmChunk)))
                .Bind(_ => IO.lift(() => writer.WriteDictionary(dictionary))),
            begun => when(begun, IO.lift(() => Refused.Unless(writer.EndWrite3dmChunk(), nameof(BinaryArchiveWriter.EndWrite3dmChunk)))).As());

    public static IO<StoredRecord<T>> Read<T>(BinaryArchiveReader reader, uint typeCode, DictionaryCodec<T> codec) =>
        from dictionary in DisposalOps.Within(
            () => reader.BeginRead3dmChunk(typeCode, out _, out _),
            begun => IO.lift(() => Refused.Unless(begun, nameof(BinaryArchiveReader.BeginRead3dmChunk)))
                .Bind(_ => IO.lift(reader.ReadDictionary)),
            begun => when(begun, IO.lift(() => Refused.Unless(reader.EndRead3dmChunk(suppressPartiallyReadChunkWarning: true), nameof(BinaryArchiveReader.EndRead3dmChunk)))).As())
        from payload in IO.lift(() => codec.FromDictionary(dictionary))
        select payload;

    public static IO<Option<T>> Read<T>(File3dm archive, Guid plugIn, Func<BinaryArchiveReader, IO<T>> read) =>
        from found in IO.lift(() => Conversions.Rows(archive.PlugInData).Find(data => data.PlugInId == plugIn))
        from payload in found.Traverse((
            from data in Eff.runtime<File3dmPlugInData>()
            from captured in Callbacks.Captured<T>(Answered(archive, data, read).RunIO, nameof(File3dmPlugInDataTable.TryRead))
            select captured).RunIO).As()
        select payload;

    private static Eff<Action<Fin<T>>, Unit> Answered<T>(File3dm archive, File3dmPlugInData data, Func<BinaryArchiveReader, IO<T>> read) =>
        from answer in Eff.runtime<Action<Fin<T>>>()
        from called in IO.lift(() => {
            _ = archive.PlugInData.TryRead(data, (_, reader) => {
                Fin<T> outcome = Callbacks.Settled(read(reader), CancellationToken.None);
                answer(outcome);
                return outcome.IsSucc;
            });
        })
        select called;

    // --- [USER_DATA]
    public static IO<Option<TData>> Find<TData>(CommonObject target) where TData : UserData =>
        IO.lift(() => Optional(target.UserData.Find(typeof(TData)) as TData));

    public static IO<Unit> Set<TData, TState>(CommonObject target, TState state)
        where TData : DefinedUserData<TState>, new()
        where TState : notnull =>
        from found in Find<TData>(target)
        from stored in found.Match(
            Some: held => held.Assign(state),
            None: Attached<TData, TState>(target, state))
        select stored;

    public static IO<Unit> Remove<TData>(CommonObject target) where TData : UserData =>
        from found in Find<TData>(target)
        from removed in found.Traverse(held => DisposalOps.Within(() => held, data => IO.lift(() => Refused.Unless(target.UserData.Remove(data), nameof(UserDataList.Remove))))).As()
        select unit;

    public static IO<Unit> Move(CommonObject source, CommonObject destination, bool append) =>
        IO.lift(() => UserData.MoveUserDataFrom(source)).Bind(id => DisposalOps.OnFailure(
            IO.lift(() => UserData.MoveUserDataTo(destination, id, append)),
            IO.lift(() => UserData.MoveUserDataTo(source, id, append: true)))).Uninterruptible();

    private static IO<Unit> Attached<TData, TState>(CommonObject target, TState state)
        where TData : DefinedUserData<TState>, new()
        where TState : notnull =>
        IO.lift(static () => new TData()).Bind(data => DisposalOps.OnFailure(
            data.Assign(state)
                .Bind(_ => IO.lift(() => Refused.Unless(target.UserData.Add(data), nameof(UserDataList.Add)))),
            IO.lift(data.Dispose))).Uninterruptible();
}
