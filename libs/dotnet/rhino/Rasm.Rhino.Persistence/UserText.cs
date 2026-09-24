using Rasm.Rhino.Document;
using Rasm.Rhino.Objects;
using Rhino;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DocumentTextKey {
    public abstract string Key { get; }

    public sealed record Flat : DocumentTextKey {
        private Flat(string key) => Key = key;

        public override string Key { get; }

        public static Fin<DocumentTextKey> Create(string key) =>
            Invalid.Unless<DocumentTextKey>(key.Length > 0, new Flat(key), nameof(Flat));
    }

    public sealed record Section : DocumentTextKey {
        private Section(string name, string entry) {
            Name = name;
            Entry = entry;
        }

        public string Name { get; }

        public string Entry { get; }

        public override string Key => $"{Name}\\{Entry}";

        public static Fin<DocumentTextKey> Create(string name, string entry) =>
            Invalid.Unless<DocumentTextKey>((name.Length > 0) && (entry.Length > 0) && !name.Contains('\\', StringComparison.Ordinal), new Section(name, entry), nameof(Section));
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UserStringStore {
    public sealed record AttributeStore(bool Quiet) : UserStringStore;

    public sealed record GeometryStore() : UserStringStore;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class UserTexts {
    // --- [DOCUMENT]
    public static IO<HashMap<string, string>> DocumentText(RhinoDoc doc) =>
        IO.lift(() => toHashMap(toSeq(Range(0, doc.Strings.Count)).Map(index => (doc.Strings.GetKey(index), doc.Strings.GetValue(index)))));

    public static IO<Option<string>> DocumentValue(RhinoDoc doc, DocumentTextKey key) =>
        IO.lift(() => Answers.Present(doc.Strings.GetValue(key.Key)));

    public static IO<Option<string>> SetDocumentText(RhinoDoc doc, DocumentTextKey key, string value) =>
        from filled in IO.lift(() => Invalid.Unless(value.Length > 0, nameof(StringTable.SetString)))
        from prior in IO.lift(() => Answers.Present(doc.Strings.SetString(key.Key, value)))
        from same in IO.lift(() => Mismatch.Unless(string.Equals(doc.Strings.GetValue(key.Key), value, StringComparison.Ordinal), nameof(StringTable.GetValue)))
        select prior;

    public static IO<Unit> DeleteDocumentText(RhinoDoc doc, DocumentTextKey key) =>
        from present in IO.lift(() => Answers.Present(doc.Strings.GetValue(key.Key)))
        from deleted in IO.lift(() => present.Iter(_ => doc.Strings.Delete(key.Key)))
        from gone in IO.lift(() => Mismatch.Unless(Answers.Present(doc.Strings.GetValue(key.Key)).IsNone, nameof(StringTable.GetValue)))
        select gone;

    // --- [OBJECTS]
    public static IO<Seq<Guid>> EditObjectText(RhinoDoc doc, Guid id, UserStringStore store, Seq<UserStringEdit> ops) =>
        from filled in IO.lift(() => Invalid.Unless(!ops.IsEmpty, nameof(UserStringEdit)))
        from target in IO.lift(() => Answers.NonEmpty(id, nameof(ObjectTable.FindId)))
        from ids in store.Switch(
            (Doc: doc, Id: target, Ops: ops),
            attributeStore: static (state, attributes) =>
                AttributeOps.Modify(state.Doc, new ObjectTarget.Ids(Seq(state.Id)), state.Ops.Map<AttributeEdit>(static op => new AttributeEdit.UserStrings(op)), attributes.Quiet),
            geometryStore: static (state, _) => RhinoObjects.ReplaceGeometry(
                state.Doc,
                state.Id,
                (GeometryBase copy) =>
                    from accessors in IO.pure(GeometryOps.UserStrings(copy))
                    from edited in state.Ops.TraverseM(op => GeometryOps.EditUserStrings(accessors, op)).As()
                    select unit).Map(_ => Seq(state.Id)))
        select ids;
}
