# [RASM_RHINO_PERSISTENCE_USERTEXT]

`TextMutationBatch` closes document, attribute, and geometry writes; `TextQuery` closes detached reads and wildcard search. `UserTexts.Commit` returns the mutation rail, while `UserTexts.Read` returns the query's detached result.

## [01]-[INDEX]

- [02]-[VOCABULARY]: `TextKey`, `UserTextValue`, `TextSection`, `ObjectTextStore`, `TextComparison`, `TextSearchPolicy`, `TextAddress`, `TextEdit`, `TextObjectFilter`, `TextMutationBatch`, and `TextQuery`.
- [03]-[DETACHED_RESULTS]: `DocumentTextSnapshot`, `ObjectTextSnapshot`, `TextMatch`, `UserTextAnswer` — the censuses and the answer family.
- [04]-[INTERPRETER]: `UserTexts.Commit` and `UserTexts.Read` — the seeded document fold, the planned object commit, and the host search seam.

## [02]-[VOCABULARY]

- Owner: `TextKey` admits a flat host key and `TextSection` a section/entry pair, the two forms the host's own store discriminant separates; `UserTextValue` admits the payload; `ObjectTextStore` is the two-row store vocabulary every routing decision reads as a set; `TextComparison` is the case-fold row; `TextSearchPolicy` crosses a store set with one comparison row; `TextAddress` makes document and object stores disjoint; `TextEdit` carries the intended post-state as a value; `TextObjectFilter` carries the host's two enumeration shapes; `TextMutationBatch` and `TextQuery` split mutation from reads.
- Entry: every request mints through a `public static` factory answering `Fin` — case records are `internal`, so the factory is the only ingress and the generated `Switch` the only egress (`[SEALED_ADMISSION]`). The prior nine-overload `Admit` family that re-admitted an already-constructed request deletes whole: a request that exists is a request that passed its clauses, and the interpreter never re-validates.
- Auto: host search routing DERIVES from the store set — `Stores.Admits(ObjectTextStore.Geometry)` and `Stores.Admits(ObjectTextStore.Attributes)` are the two `FindByUserString` arguments — so the per-row `(bool Geometry, bool Attributes)` tuple and the policy's fold over it both delete and a third store is one vocabulary row.
- Law: the host's own store discriminant is the BACKSLASH. `DocumentDataCount` counts keys containing one and `DocumentUserTextCount` counts keys without one, and `ReadDocument` splits on that same character — so a flat key carrying a backslash writes as a SECTION entry and reads back as one. `TextKey` refuses it and the section form goes through `TextSection`, which is why the two carriers are not one string.
- Law: `TextEdit.Result` carries the intended post-state: a set carries `Some(value)` and a delete carries `None`, so every write settles on the stored value rather than the host's returned `bool`.
- Law: `TextSearchPolicy` requires at least one store, and the requirement is a `CapabilityLaw` bar rather than a hook clause — `Forbidden` bars the empty held set alone, so the law is one declaration the type admits through and no consumer re-tests emptiness.
- Law: `UserTextValue` names the user-string payload and keeps the annotation and command-prompt text domains distinct.
- Growth: a new store is one `ObjectTextStore` row; a new comparison posture is one `TextComparison` row; a new address form is one `TextAddress` case and its factory.
- Boundary: `DocumentStream` alone observes `RhinoDoc.UserStringChanged`; this page never creates a parallel event surface.
- Packages: RhinoCommon (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-persistence.md` — `RhinoDoc.Strings`, `DocumentUserTextCount`, `DocumentDataCount`; `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-objects.md` — `ObjectTable.FindByUserString`, `ObjectEnumeratorSettings`, `ObjectType`); kernel `Domain/validation` (`ICapability`, `CapabilitySet`, `CapabilityLaw`); `Document/tables` (`ResourceId`); Thinktecture.Runtime.Extensions (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md`); LanguageExt.Core (`libs/dotnet/.api/api-languageext.md`).

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino.DocObjects;
using Thinktecture;

namespace Rasm.Rhino.Persistence;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError]
public readonly partial struct TextKey : IDisallowDefaultValue {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        Op op = Op.Of();
        value = value?.Trim() ?? string.Empty;
        string candidate = value;
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
                (candidate.Length == 0, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(TextKey) }))),
                (candidate.Contains('\\', StringComparison.Ordinal),
                    () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(TextKey), "a flat key carrying no section discriminant; use TextSection" })))));
    }
}

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError]
public readonly partial struct UserTextValue : IDisallowDefaultValue {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = value is null
            ? new ValidationError(string.Join(" | ", new object?[] { Op.Of(), nameof(UserTextValue) }))
            : null;
}

[ComplexValueObject]
[ValidationError]
public sealed partial record TextSection(string Section, string Entry) {
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string section,
        ref string entry) {
        Op op = Op.Of();
        section = section?.Trim() ?? string.Empty;
        entry = entry?.Trim() ?? string.Empty;
        (string group, string row) = (section, entry);
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
                (group.Length == 0, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Section) }))),
                (row.Length == 0, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Entry) })))));
    }

    public static Fin<TextSection> Of(string section, string entry, Op? key = null) =>
        key.OrDefault().AcceptValidated<TextSection>(Validate(section, entry, out TextSection? value), value);

    internal string Wire => $"{Section}\\{Entry}";
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ObjectTextStore : ICapability<ObjectTextStore> {
    public static readonly ObjectTextStore Attributes = new(key: "attributes");
    public static readonly ObjectTextStore Geometry = new(key: "geometry");

    public static CapabilityLaw<ObjectTextStore> Law =>
        CapabilityLaw<ObjectTextStore>.Forbidden(Seq(CapabilitySet<ObjectTextStore>.None));
}

[SmartEnum<bool>]
public sealed partial class TextComparison {
    public static readonly TextComparison Exact = new(key: true);
    public static readonly TextComparison Folded = new(key: false);
}

[ComplexValueObject]
[ValidationError]
public sealed partial record TextSearchPolicy(
    CapabilitySet<ObjectTextStore> Stores,
    TextComparison Comparison) {
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref CapabilitySet<ObjectTextStore> stores,
        ref TextComparison comparison) {
        Op op = Op.Of();
        TextComparison? posture = comparison;
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
                (posture is null, () => new ValidationClause(string.Join(" | ", new object?[] { op, nameof(Comparison) })))));
    }

    public static Fin<TextSearchPolicy> Of(CapabilitySet<ObjectTextStore> stores, TextComparison comparison, Op? key = null) {
        Op op = key.OrDefault();
        return from admitted in ObjectTextStore.Law.Admit(held: stores)
               from policy in op.AcceptValidated<TextSearchPolicy>(
                   Validate(admitted, comparison, out TextSearchPolicy? value),
                   value)
               select policy;
    }

    internal bool Searches(ObjectTextStore store) => Stores.Admits(capability: store);
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextAddress {
    private TextAddress() { }

    internal sealed record DocumentKeyCase(TextKey Key) : TextAddress;
    internal sealed record DocumentSectionCase(TextSection Address) : TextAddress;
    internal sealed record ObjectCase(ResourceId ObjectId, ObjectTextStore Store, TextKey Key) : TextAddress;

    public static Fin<TextAddress> Document(string key, Op? okey = null) =>
        okey.OrDefault().AcceptValidated<TextKey>(candidate: key)
            .Map<TextAddress>(static admitted => new DocumentKeyCase(Key: admitted));

    public static Fin<TextAddress> Document(string section, string entry, Op? key = null) =>
        TextSection.Of(section: section, entry: entry, key: key)
            .Map<TextAddress>(static admitted => new DocumentSectionCase(Address: admitted));

    public static Fin<TextAddress> Object(Guid objectId, ObjectTextStore store, string key, Op? okey = null) {
        Op op = okey.OrDefault();
        return (
                ResourceId.Admit(value: objectId, key: op).ToValidation(),
                op.Need(value: store).ToValidation(),
                op.AcceptValidated<TextKey>(candidate: key).ToValidation())
            .Apply(static (id, held, admitted) => (TextAddress)new ObjectCase(ObjectId: id, Store: held, Key: admitted))
            .As()
            .ToFin();
    }

    internal bool IsObject => this is ObjectCase;

    internal string Wire => Switch<string>(
        documentKeyCase:     static value => value.Key.Value,
        documentSectionCase: static value => value.Address.Wire,
        objectCase:          static value => $"{value.ObjectId.Value}:{value.Store.Key}:{value.Key.Value}");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextEdit {
    private TextEdit() { }

    internal sealed record SetCase(UserTextValue Value) : TextEdit;
    internal sealed record DeleteCase : TextEdit;

    public static Fin<TextEdit> Set(string value, Op? key = null) =>
        key.OrDefault().AcceptValidated<UserTextValue>(candidate: value)
            .Map<TextEdit>(static admitted => new SetCase(Value: admitted));

    public static TextEdit Delete() => new DeleteCase();

    internal Option<UserTextValue> Result => Switch<Option<UserTextValue>>(
        setCase:    static write => Some(write.Value),
        deleteCase: static _ => None);
}

public sealed record TextMutation(TextAddress Address, TextEdit Edit);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextObjectFilter {
    private TextObjectFilter() { }

    internal sealed record KindsCase(ObjectType Kinds) : TextObjectFilter;
    internal sealed record EnumeratorCase(ObjectEnumeratorSettings Settings) : TextObjectFilter;

    public static TextObjectFilter Kinds(ObjectType kinds) => new KindsCase(Kinds: kinds);

    public static Fin<TextObjectFilter> Enumerated(ObjectEnumeratorSettings settings, Op? key = null) =>
        key.OrDefault().Need(value: settings).Map<TextObjectFilter>(static admitted => new EnumeratorCase(Settings: admitted));
}

public sealed record TextSearch(TextKey Key, UserTextValue Pattern, TextSearchPolicy Policy, TextObjectFilter Filter);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextMutationBatch {
    private TextMutationBatch() { }

    internal sealed record DocumentCase(Seq<TextMutation> Mutations) : TextMutationBatch;
    internal sealed record ObjectsCase(Seq<TextMutation> Mutations) : TextMutationBatch;

    public static Fin<TextMutationBatch> Document(params ReadOnlySpan<TextMutation> mutations) =>
        Admitted(mutations: mutations, objects: false)
            .Map<TextMutationBatch>(static admitted => new DocumentCase(Mutations: admitted));

    public static Fin<TextMutationBatch> Objects(params ReadOnlySpan<TextMutation> mutations) =>
        Admitted(mutations: mutations, objects: true)
            .Map<TextMutationBatch>(static admitted => new ObjectsCase(Mutations: admitted));

    private static Fin<Seq<TextMutation>> Admitted(ReadOnlySpan<TextMutation> mutations, bool objects) {
        Op op = Op.Of();
        return from admitted in toSeq(mutations.ToArray())
                   .Traverse(value => op.Need(value: value).ToValidation())
                   .As()
                   .ToFin()
               from _nonempty in guard(!admitted.IsEmpty,
                   (Error)new KernelFault.InvalidValue(nameof(mutations), string.Join(" | ", new object?[] { op, "at least one text mutation" }))).ToFin()
               from _side in FactoryValidation.Admit(admitted
                   .Filter(value => value.Address.IsObject != objects)
                   .Map(value => new ValidationClause(string.Join(" | ", new object?[] {
                       op, nameof(TextMutationBatch), $"an address on the {(objects ? "object" : "document")} side; got '{value.Address.Wire}'" }))))
               select admitted;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextQuery {
    private TextQuery() { }

    internal sealed record ReadDocumentCase : TextQuery;
    internal sealed record ReadObjectsCase(Seq<ResourceId> ObjectIds) : TextQuery;
    internal sealed record SearchCase(TextSearch Search) : TextQuery;

    public static TextQuery Document() => new ReadDocumentCase();

    public static Fin<TextQuery> Objects(params ReadOnlySpan<Guid> objectIds) {
        Op op = Op.Of();
        return from admitted in toSeq(objectIds.ToArray())
                   .Traverse(id => ResourceId.Admit(value: id, key: op).ToValidation())
                   .As()
                   .ToFin()
               from _nonempty in guard(!admitted.IsEmpty,
                   (Error)new KernelFault.InvalidValue(nameof(objectIds), string.Join(" | ", new object?[] { op, "at least one object identity" }))).ToFin()
               select (TextQuery)new ReadObjectsCase(ObjectIds: admitted);
    }

    public static Fin<TextQuery> Search(string key, string pattern, TextSearchPolicy policy, TextObjectFilter filter, Op? okey = null) {
        Op op = okey.OrDefault();
        return (
                op.AcceptValidated<TextKey>(candidate: key).ToValidation(),
                op.AcceptValidated<UserTextValue>(candidate: pattern).ToValidation(),
                op.Need(value: policy).ToValidation(),
                op.Need(value: filter).ToValidation())
            .Apply(static (admitted, wildcard, held, shape) => (TextQuery)new SearchCase(
                Search: new TextSearch(Key: admitted, Pattern: wildcard, Policy: held, Filter: shape)))
            .As()
            .ToFin();
    }
}
```

## [03]-[DETACHED_RESULTS]

- Owner: `DocumentTextSnapshot` is the flat/section partition of the document string table; `ObjectTextSnapshot` is one object's two stores; `TextMatch` names an object and the store set that matched it; `UserTextAnswer` is the answer family.
- Law: a snapshot carries the MAPS alone. The retired form carried the host's own counts beside them with a `Consistent` predicate every caller re-read — a mirror that reports disagreement instead of refusing it. The host counts are now a POSTCONDITION inside the read: `DocumentUserTextCount` must equal the folded flat map and `DocumentDataCount` the folded section map, `UserStringCount` must equal the folded attribute map, and a disagreement is a typed `Diverged` carrying both readings.
- Law: `TextMatch.Stores` is a capability set, so a store row added to the vocabulary adds no product case and an object matched in both stores is one row rather than two.
- Law: every collection-bearing record declares its equality explicitly, because the object commit rail DECIDES on sequence equality between a planned delta run and the run the host callback reproduced — synthesized record equality over a LanguageExt carrier compares the wrong thing at exactly the site that must not be wrong.
- Growth: a new answer shape is one `UserTextAnswer` case; a new census column is one snapshot member and its postcondition.
- Packages: `Document/tables` (`ResourceId`); `Document/session` (`IDetachedDocumentResult`); Generator.Equals (`libs/dotnet/.api/api-generator-equals.md` — `[Equatable]`, `[UnorderedEquality]`); LanguageExt.Core.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Generator.Equals;
using Rasm.Domain;
using Rasm.Rhino.Document;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
[Equatable]
public sealed partial record DocumentTextSnapshot(
    [property: UnorderedEquality] HashMap<TextKey, UserTextValue> Flat,
    [property: UnorderedEquality] HashMap<TextSection, UserTextValue> Sections);

[Equatable]
public sealed partial record ObjectTextSnapshot(
    ResourceId ObjectId,
    [property: UnorderedEquality] HashMap<TextKey, UserTextValue> Attributes,
    [property: UnorderedEquality] HashMap<TextKey, UserTextValue> Geometry);

[ComplexValueObject]
[ValidationError]
public sealed partial record TextMatch(ResourceId ObjectId, CapabilitySet<ObjectTextStore> Stores) {
    public static Fin<TextMatch> Of(ResourceId objectId, CapabilitySet<ObjectTextStore> stores, Op? key = null) {
        Op op = key.OrDefault();
        return from admitted in ObjectTextStore.Law.Admit(held: stores)
               from match in op.AcceptValidated<TextMatch>(Validate(objectId, admitted, out TextMatch? value), value)
               select match;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UserTextAnswer : IDetachedDocumentResult {
    private UserTextAnswer() { }

    public sealed record DocumentCase(DocumentTextSnapshot Snapshot) : UserTextAnswer;
    public sealed record ObjectsCase(Seq<ObjectTextSnapshot> Snapshots) : UserTextAnswer;
    public sealed record MatchesCase(Seq<TextMatch> Matches) : UserTextAnswer;
}
```

## [04]-[INTERPRETER]

- Owner: `UserTexts` — `Commit` routes document and object mutations to their owning transaction rail, while `Read` routes detached reads and wildcard search.
- Entry: `UserTexts.Commit(DocumentSession, TextMutationBatch, Op?)`; `UserTexts.Read(DocumentSession, TextQuery, Op?)`.
- Auto: the document rail reads the string table TWICE regardless of batch length — once to seed the fold, once to settle it. The seeded maps ARE the document's text state, each applied mutation advances them, and the next mutation reads its prior from the fold. The retired per-probe prior read walked `Strings.Count` calling `GetKey(index)`, so every mutation cost a full table scan and every batch cost two per mutation; the closing census proves the whole batch at once, which is strictly stronger than the per-mutation re-read it replaces.
- Law: every object write settles on the STORED value. A host `true` that wrote something else is a silent divergence, and a host `false` settles ONLY when the key already holds what the request asked for — deleting an absent key, or writing the value already there. A refusal that leaves an EFFECTIVE change unapplied is a typed failure.
- Law: object mutations are planned against detached values before any host write, and the attribute callback proves its stored map equals that plan before the table mutation commits.
- Law: `TextPlan` owns each staged geometry clone through `Tables.Commit`; failed planning releases the clones already staged, and terminal commit releases the complete plan on both rails.
- Law: object mutation terminates at `Tables.Commit`; document text remains under the session's own undo bracket. Two rails because two transaction owners: the object table's program carries its own undo and interaction posture, and a document string write has no table occurrence to address.
- Boundary: Rhino's mutable text, clone, undo, and object-table calls form the platform-forced statement seam. No live `RhinoObject` and no mutable string collection escapes a projection.
- Packages: RhinoCommon (`libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-persistence.md` — `StringTable.Count`/`GetKey`/`GetValue`/`SetString`/`Delete`/`DocumentUserTextCount`/`DocumentDataCount`; `libs/dotnet/Rasm.Rhino/.api/api-rhinocommon-objects.md` — `RhinoObject.Attributes`/`Geometry`, `GetUserStrings`, `GetUserString`, `SetUserString`, `DeleteUserString`, `UserStringCount`, `ObjectTable.FindId`/`FindByUserString`); `Document/session` (`DocumentSession.Demand`, `SessionNeed`, `UndoCustody`); `Document/commit` (`DocumentCommit.Sealed`, `RedrawPolicy`, `HostInteraction`); `Document/tables` (`Tables.Commit`, `TableOp.Amend`/`Replace`, `TableTarget`, `TableTransaction.Recorded`, `AttributeChange`, `ModeRegard`); kernel `Domain/rails` (`Op.Catch`, `Custody`); LanguageExt.Core (`TraverseM`, `Choose`, `HashMap`).

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Collections.Specialized;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record TextPlan(Seq<TableOp> Operations, Seq<GeometryBase> Owned) : IDetachedDocumentResult, IDisposable {
    public void Dispose() => Owned.Rev().Iter(static geometry => geometry.Dispose());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class UserTexts {
    public static Fin<Unit> Commit(DocumentSession session, TextMutationBatch batch, Op? key = null) {
        Op op = key.OrDefault();
        return from owner in op.Need(value: session)
               from request in op.Need(value: batch)
               from completed in request.Switch<(DocumentSession Session, Op Op), Fin<Unit>>(
                   state: (owner, op),
                   documentCase: static (state, document) => state.Session.Demand(
                       use: owner => MutateDocument(document: owner, mutations: document.Mutations, key: state.Op),
                       key: state.Op,
                       needs: SessionNeed.Mutation(custody: UndoCustody.Recorded, redraw: RedrawPolicy.None).ToArray()),
                   objectsCase: static (state, objects) =>
                       from plan in state.Session.Demand(
                           use: owner => Plan(document: owner, mutations: objects.Mutations, key: state.Op),
                           key: state.Op,
                           needs: [SessionNeed.Read])
                       from _committed in CommitPlan(session: state.Session, plan: plan, key: state.Op)
                       select unit)
               select completed;
    }

    public static Fin<UserTextAnswer> Read(DocumentSession session, TextQuery query, Op? key = null) {
        Op op = key.OrDefault();
        return from owner in op.Need(value: session)
               from request in op.Need(value: query)
               from answer in request.Switch<(DocumentSession Session, Op Op), Fin<UserTextAnswer>>(
                   state: (owner, op),
                   readDocumentCase: static (state, _) => state.Session.Demand(
                       use: document => ReadDocument(document: document, key: state.Op)
                           .Map<UserTextAnswer>(static value => new UserTextAnswer.DocumentCase(Snapshot: value)),
                       key: state.Op,
                       needs: [SessionNeed.Read]),
                   readObjectsCase: static (state, read) => state.Session.Demand(
                       use: document => read.ObjectIds
                           .Traverse(id => ReadObject(document: document, objectId: id, key: state.Op).ToValidation())
                           .As()
                           .ToFin()
                           .Map<UserTextAnswer>(static values => new UserTextAnswer.ObjectsCase(Snapshots: values)),
                       key: state.Op,
                       needs: [SessionNeed.Read]),
                   searchCase: static (state, search) => state.Session.Demand(
                       use: document => Search(document: document, search: search.Search, key: state.Op)
                           .Map<UserTextAnswer>(static values => new UserTextAnswer.MatchesCase(Matches: values)),
                       key: state.Op,
                       needs: [SessionNeed.Read]))
               select answer;
    }

    private sealed record DocumentFold(
        HashMap<TextKey, UserTextValue> Flat,
        HashMap<TextSection, UserTextValue> Sections);

    private static Fin<Unit> MutateDocument(RhinoDoc document, Seq<TextMutation> mutations, Op key) =>
        DocumentCommit.Sealed(
            document: document,
            name: nameof(Commit),
            recordsUndo: true,
            redraw: RedrawPolicy.None,
            run: () =>
                from seed in ReadDocument(document: document, key: key)
                from folded in mutations.Fold(
                    Fin.Succ(value: new DocumentFold(Flat: seed.Flat, Sections: seed.Sections)),
                    (state, mutation) => state.Bind(fold => Advance(document: document, fold: fold, mutation: mutation, key: key)))
                from settled in ReadDocument(document: document, key: key)
                from _proof in guard(
                    settled.Flat == folded.Flat && settled.Sections == folded.Sections,
                    (Error)new PersistenceFault.Diverged(
                        Key: key,
                        Subject: nameof(DocumentTextSnapshot),
                        Expected: $"flat={folded.Flat.Count}:sections={folded.Sections.Count}",
                        Observed: $"flat={settled.Flat.Count}:sections={settled.Sections.Count}")).ToFin()
                select unit,
            project: Fin.Succ,
            op: key);

    private static Fin<DocumentFold> Advance(RhinoDoc document, DocumentFold fold, TextMutation mutation, Op key) =>
        mutation.Address.Switch<(RhinoDoc Document, DocumentFold Fold, TextMutation Mutation, Op Op), Fin<DocumentFold>>(
            state: (document, fold, mutation, key),
            documentKeyCase: static (state, address) => Written(
                    mutation: state.Mutation,
                    set: value => state.Document.Strings.SetString(address.Key.Value, value),
                    delete: () => state.Document.Strings.Delete(address.Key.Value),
                    key: state.Op)
                .Map(_ => state.Fold with {
                    Flat = state.Mutation.Edit.Result.Match(
                        Some: value => state.Fold.Flat.AddOrUpdate(address.Key, value),
                        None: () => state.Fold.Flat.Remove(address.Key)),
                }),
            documentSectionCase: static (state, address) => Written(
                    mutation: state.Mutation,
                    set: value => state.Document.Strings.SetString(address.Address.Section, address.Address.Entry, value),
                    delete: () => state.Document.Strings.Delete(address.Address.Section, address.Address.Entry),
                    key: state.Op)
                .Map(_ => state.Fold with {
                    Sections = state.Mutation.Edit.Result.Match(
                        Some: value => state.Fold.Sections.AddOrUpdate(address.Address, value),
                        None: () => state.Fold.Sections.Remove(address.Address)),
                }),
            objectCase: static (state, address) => Fin.Fail<DocumentFold>(
                error: new KernelFault.InvalidValue(nameof(TextMutationBatch), string.Join(" | ", new object?[] { state.Op, $"a document address in a document batch; got '{address.Key.Value}'" }))));

    private static Fin<Unit> Written(
        TextMutation mutation,
        Func<string, string> set,
        Action delete,
        Op key) =>
        key.Catch(() => mutation.Edit.Switch<Unit>(
                setCase: write => { _ = set(write.Value.Value); return unit; },
                deleteCase: _ => { delete(); return unit; }));

    private static Fin<TextPlan> Plan(RhinoDoc document, Seq<TextMutation> mutations, Op key) =>
        toSeq(ObjectTextStore.Items)
            .Bind(store => Grouped(mutations: mutations, store: store)
                .OrderBy(static group => group.Key.Value)
                .AsIterable()
                .ToSeq()
                .Map(group => (Store: store, ObjectId: group.Key, Mutations: group.Value)))
            .Fold(
                Fin.Succ(value: new TextPlan(Operations: Seq<TableOp>(), Owned: Seq<GeometryBase>())),
                (state, batch) => state.Bind(plan =>
                    (from source in Resolve(document: document, objectId: batch.ObjectId, key: key)
                     from staged in Staged(source: source, store: batch.Store, mutations: batch.Mutations, key: key)
                     select new TextPlan(
                         Operations: plan.Operations.Add(staged.Operation),
                         Owned: staged.Owned.Match(
                             Some: geometry => plan.Owned.Add(geometry),
                             None: () => plan.Owned)))
                    .Rollback(
                        held: plan.Owned,
                        release: geometry => key.Catch(geometry.Dispose),
                        key: key)));

    private static HashMap<ResourceId, Seq<TextMutation>> Grouped(Seq<TextMutation> mutations, ObjectTextStore store) =>
        mutations
            .Choose(mutation => mutation.Address is TextAddress.ObjectCase address && address.Store == store
                ? Some((address.ObjectId, mutation))
                : None)
            .Fold(
                HashMap<ResourceId, Seq<TextMutation>>(),
                static (groups, row) => groups.AddOrUpdate(
                    key: row.ObjectId,
                    Some: held => held.Add(row.mutation),
                    None: () => Seq(row.mutation)));

    private static Fin<(TableOp Operation, Option<GeometryBase> Owned)> Staged(RhinoObject source, ObjectTextStore store, Seq<TextMutation> mutations, Op key) =>
        store == ObjectTextStore.Attributes
            ? PlanAttributes(source: source, mutations: mutations, key: key)
                .Map(static operation => (Operation: operation, Owned: Option<GeometryBase>.None))
            : PlanGeometry(source: source, mutations: mutations, key: key)
                .Map(static staged => (Operation: staged.Operation, Owned: Some(staged.Owned)));

    private static Fin<TableOp> PlanAttributes(RhinoObject source, Seq<TextMutation> mutations, Op key) =>
        from detached in Freeze(read: () => source.Attributes.GetUserStrings(), key: key)
        from planned in Project(values: detached, mutations: mutations, key: key)
        from target in TableTarget.Of(source.Id)
        from change in key.AcceptValidated<AttributeChange>(
            AttributeChange.Validate(attributes =>
                from _applied in mutations
                    .TraverseM(mutation => Settle(
                        get: attributes.GetUserString,
                        set: attributes.SetUserString,
                        delete: attributes.DeleteUserString,
                        mutation: mutation,
                        key: key))
                    .As()
                from actual in Freeze(read: attributes.GetUserStrings, key: key)
                from _proof in guard(
                    actual == planned,
                    (Error)new PersistenceFault.Diverged(
                        Key: key,
                        Subject: nameof(PlanAttributes),
                        Expected: planned.Count.ToString(),
                        Observed: actual.Count.ToString())).ToFin()
                select unit,
                out AttributeChange? value),
            value)
        from operation in TableOp.Amend(target: target, change: change, interaction: HostInteraction.Quiet, key: key)
        select operation;

    private static Fin<(TableOp Operation, GeometryBase Owned)> PlanGeometry(RhinoObject source, Seq<TextMutation> mutations, Op key) =>
        from edited in key.Catch(() => Optional(source.Geometry.Duplicate()).ToFin(Fail: key.InvalidResult()))
        from operation in (
            from _applied in mutations
                .TraverseM(mutation => Settle(
                    get: edited.GetUserString,
                    set: edited.SetUserString,
                    delete: edited.DeleteUserString,
                    mutation: mutation,
                    key: key))
                .As()
            from target in TableTarget.Of(source.Id)
            from replacement in TableOp.Replace(target: target, replacement: edited, modes: ModeRegard.Respect, key: key)
            select replacement)
            .Rollback(edited)
        select (Operation: operation, Owned: edited);

    private static Fin<Unit> CommitPlan(DocumentSession session, TextPlan plan, Op key) =>
        Custody.Bracket(() => plan.Operations.IsEmpty
            ? Fin.Succ(value: unit)
            : from transaction in TableTransaction.Recorded(
                  nameof(Commit),
                  RedrawPolicy.None,
                  Seq<TableCustomUndo>(),
                  plan.Operations.ToArray())
              from _committed in Tables.Commit(session: session, transaction: transaction, key: key)
              select unit,
            plan);

    private static Fin<HashMap<TextKey, UserTextValue>> Project(
        HashMap<TextKey, UserTextValue> values,
        Seq<TextMutation> mutations,
        Op key) =>
        mutations.Fold(
            Fin.Succ(value: values),
            (state, mutation) => state.Bind(plan =>
                from address in Addressed(address: mutation.Address, key: key)
                select mutation.Edit.Result.Match(
                    Some: value => plan.AddOrUpdate(address.Key, value),
                    None: () => plan.Remove(address.Key))));

    private static Fin<Unit> Settle(
        Func<string, string?> get,
        Func<string, string, bool> set,
        Func<string, bool> delete,
        TextMutation mutation,
        Op key) =>
        from address in Addressed(address: mutation.Address, key: key)
        from _accepted in key.Catch(() => Fin.Succ(value: mutation.Edit.Switch<bool>(
            setCase: write => set(address.Key.Value, write.Value.Value),
            deleteCase: _ => delete(address.Key.Value))))
        from current in key.Catch(() => Value(source: get(address.Key.Value), key: key))
        from _settled in current == mutation.Edit.Result
            ? Fin.Succ(value: unit)
            : Fin.Fail<Unit>(error: new PersistenceFault.Diverged(
                Key: key,
                Subject: address.Key.Value,
                Expected: mutation.Edit.Result.Match(Some: static value => value.Value, None: static () => "<absent>"),
                Observed: current.Match(Some: static value => value.Value, None: static () => "<absent>")))
        select unit;

    private static Fin<TextAddress.ObjectCase> Addressed(TextAddress address, Op key) =>
        address.Switch<Op, Fin<TextAddress.ObjectCase>>(
            state: key,
            documentKeyCase: static (op, value) => Fin.Fail<TextAddress.ObjectCase>(
                error: new KernelFault.InvalidValue(nameof(TextAddress), string.Join(" | ", new object?[] { op, $"an object address; got '{value.Key.Value}'" }))),
            documentSectionCase: static (op, value) => Fin.Fail<TextAddress.ObjectCase>(
                error: new KernelFault.InvalidValue(nameof(TextAddress), string.Join(" | ", new object?[] { op, $"an object address; got '{value.Address.Wire}'" }))),
            objectCase: static (_, value) => Fin.Succ(value: value));

    private static Fin<DocumentTextSnapshot> ReadDocument(RhinoDoc document, Op key) =>
        from rows in key.Catch(() => toSeq(Range(0, document.Strings.Count))
            .Traverse(index => Row(document: document, index: index, key: key).ToValidation())
            .As()
            .ToFin())
        from maps in rows.Fold(
            Fin.Succ(value: (Flat: HashMap<TextKey, UserTextValue>(), Sections: HashMap<TextSection, UserTextValue>())),
            (state, row) => state.Bind(held => row.Address.Switch<
                    (HashMap<TextKey, UserTextValue> Flat, HashMap<TextSection, UserTextValue> Sections),
                    Fin<(HashMap<TextKey, UserTextValue> Flat, HashMap<TextSection, UserTextValue> Sections)>>(
                state: held,
                documentKeyCase: (maps, address) => maps.Flat.ContainsKey(address.Key)
                    ? Fin.Fail<(HashMap<TextKey, UserTextValue>, HashMap<TextSection, UserTextValue>)>(
                        Collision(label: nameof(TextKey), raw: row.Raw, canonical: address.Key.Value, key: key))
                    : Fin.Succ(value: (maps.Flat.Add(address.Key, row.Value), maps.Sections)),
                documentSectionCase: (maps, address) => maps.Sections.ContainsKey(address.Address)
                    ? Fin.Fail<(HashMap<TextKey, UserTextValue>, HashMap<TextSection, UserTextValue>)>(
                        Collision(label: nameof(TextSection), raw: row.Raw, canonical: address.Address.Wire, key: key))
                    : Fin.Succ(value: (maps.Flat, maps.Sections.Add(address.Address, row.Value))),
                objectCase: (_, _) => Fin.Fail<(HashMap<TextKey, UserTextValue>, HashMap<TextSection, UserTextValue>)>(
                    error: new KernelFault.InvalidValue(nameof(TextAddress), string.Join(" | ", new object?[] { key, "a document address off the string table" }))))))
        from counts in key.Catch(() => Fin.Succ(value: (Flat: document.Strings.DocumentUserTextCount, Sections: document.Strings.DocumentDataCount)))
        from _proof in Proved(
            subject: nameof(DocumentTextSnapshot),
            expected: $"flat={counts.Flat}:sections={counts.Sections}",
            observed: $"flat={maps.Flat.Count}:sections={maps.Sections.Count}",
            agreed: counts.Flat == maps.Flat.Count && counts.Sections == maps.Sections.Count,
            key: key)
        select new DocumentTextSnapshot(Flat: maps.Flat, Sections: maps.Sections);

    private static Fin<(string Raw, TextAddress Address, UserTextValue Value)> Row(RhinoDoc document, int index, Op key) =>
        from raw in key.Catch(() => Fin.Succ(value: document.Strings.GetKey(index)))
        from value in key.AcceptValidated<UserTextValue>(candidate: document.Strings.GetValue(index))
        let separator = raw.IndexOf('\\', StringComparison.Ordinal)
        from address in separator < 0
            ? TextAddress.Document(key: raw, okey: key)
            : TextAddress.Document(section: raw[..separator], entry: raw[(separator + 1)..], key: key)
        select (raw, address, value);

    private static Fin<ObjectTextSnapshot> ReadObject(RhinoDoc document, ResourceId objectId, Op key) =>
        from source in Resolve(document: document, objectId: objectId, key: key)
        from stores in (
                Freeze(read: () => source.Attributes.GetUserStrings(), key: key).ToValidation(),
                Freeze(read: () => source.Geometry.GetUserStrings(), key: key).ToValidation())
            .Apply(static (attributes, geometry) => (Attributes: attributes, Geometry: geometry))
            .As()
            .ToFin()
        from count in key.Catch(() => Fin.Succ(value: source.Attributes.UserStringCount))
        from _proof in Proved(
            subject: nameof(ObjectTextSnapshot),
            expected: count.ToString(),
            observed: stores.Attributes.Count.ToString(),
            agreed: count == stores.Attributes.Count,
            key: key)
        select new ObjectTextSnapshot(ObjectId: objectId, Attributes: stores.Attributes, Geometry: stores.Geometry);

    private static Fin<Seq<TextMatch>> Search(RhinoDoc document, TextSearch search, Op key) =>
        from found in toSeq(ObjectTextStore.Items)
            .TraverseM(store => search.Policy.Searches(store: store)
                ? Find(document: document, search: search, store: store, key: key).Map(ids => (Store: store, Ids: ids))
                : Fin.Succ(value: (Store: store, Ids: Seq<Guid>())))
            .As()
        let census = found.Bind(static row => row.Ids).Distinct().OrderBy(static id => id).AsIterable().ToSeq()
        from matches in census
            .Traverse(id => ResourceId.Admit(value: id, key: key)
                .Bind(admitted => TextMatch.Of(
                    objectId: admitted,
                    stores: CapabilitySet<ObjectTextStore>.Of(found
                        .Filter(row => row.Ids.Contains(id))
                        .Map(static row => row.Store)
                        .ToArray()),
                    key: key))
                .ToValidation())
            .As()
            .ToFin()
        select matches;

    private static Fin<Seq<Guid>> Find(RhinoDoc document, TextSearch search, ObjectTextStore store, Op key) =>
        from found in key.Catch(() => Fin.Succ(value: search.Filter.Switch<(RhinoDoc Document, TextSearch Search, ObjectTextStore Store), RhinoObject[]?>(
            state: (document, search, store),
            kindsCase: static (state, kinds) => state.Document.Objects.FindByUserString(
                state.Search.Key.Value,
                state.Search.Pattern.Value,
                state.Search.Policy.Comparison.Key,
                state.Store == ObjectTextStore.Geometry,
                state.Store == ObjectTextStore.Attributes,
                kinds.Kinds),
            enumeratorCase: static (state, enumerator) => state.Document.Objects.FindByUserString(
                state.Search.Key.Value,
                state.Search.Pattern.Value,
                state.Search.Policy.Comparison.Key,
                state.Store == ObjectTextStore.Geometry,
                state.Store == ObjectTextStore.Attributes,
                enumerator.Settings))))
        select toSeq(Optional(found).IfNone(Array.Empty<RhinoObject>())).Map(static value => value.Id);

    private static Fin<RhinoObject> Resolve(RhinoDoc document, ResourceId objectId, Op key) =>
        key.Catch(() => Optional(document.Objects.FindId(objectId.Value))
            .ToFin(Fail: new PersistenceFault.AbsentEntry(Key: key, Table: "objects", Entry: objectId.Value.ToString())));

    private static Fin<HashMap<TextKey, UserTextValue>> Freeze(Func<NameValueCollection> read, Op key) =>
        from source in key.Catch(() => Fin.Succ(value: read()))
        from rows in toSeq(source.AllKeys.OfType<string>())
            .Traverse(raw => (
                    key.AcceptValidated<TextKey>(candidate: raw).ToValidation(),
                    key.AcceptValidated<UserTextValue>(candidate: source[raw]).ToValidation())
                .Apply(static (admitted, value) => (Raw: raw, Key: admitted, Value: value))
                .As())
            .As()
            .ToFin()
        from map in rows.Fold(
            Fin.Succ(HashMap<TextKey, UserTextValue>()),
            (state, row) => state.Bind(held => held.ContainsKey(row.Key)
                ? Fin.Fail<HashMap<TextKey, UserTextValue>>(Collision(label: nameof(TextKey), raw: row.Raw, canonical: row.Key.Value, key: key))
                : Fin.Succ(value: held.Add(row.Key, row.Value))))
        select map;

    private static Fin<Unit> Proved(string subject, string expected, string observed, bool agreed, Op key) => agreed
        ? Fin.Succ(value: unit)
        : Fin.Fail<Unit>(error: new PersistenceFault.Diverged(Key: key, Subject: subject, Expected: expected, Observed: observed));

    private static Error Collision(string label, string raw, string canonical, Op key) =>
        new PersistenceFault.Diverged(Key: key, Subject: label, Expected: raw, Observed: canonical);

    private static Fin<Option<UserTextValue>> Value(string? source, Op key) => source is null
        ? Fin.Succ(value: Option<UserTextValue>.None)
        : key.AcceptValidated<UserTextValue>(candidate: source).Map(Some);
}
```
