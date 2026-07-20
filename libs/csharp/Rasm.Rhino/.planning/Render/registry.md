# [RASM_RHINO_RENDER_REGISTRY]

`ContentUuidCatalog` owns built-in type, instance, and CCI seed data, `ContentSerializer` owns explicit read transfer and multi-load reporting, and `Registry.Run` closes registration and mutation. `Registry.Read` preserves typed query correlation, icons retain bitmap custody across every verified icon modality, and static content events fold into detached facts with no live `RenderContent` escape.

## [01]-[INDEX]

- [02]-[FACTORY_REGISTRY]: `ContentTypeInfo`, plug-in registration, and the `ContentSerializer` adapter.
- [03]-[OPERATION_FAMILY]: `ContentAdmission`, `ContentMutation`, and identity-discriminated `ContentOp` dispatch.
- [04]-[COMMIT_AND_QUERY]: `ContentTransaction`, typed query programs, and the `Registry` rails.
- [05]-[RECEIPTS]: `ContentSlot`, `ContentBody`, and the `ContentReceipt` monoid.
- [06]-[EVENTS]: `ContentPulse`, `ContentSignal`, `ContentFact`, and the `ContentStream` observation capsule.
- [07]-[SURFACE_LEDGER]: page owner table.

## [02]-[FACTORY_REGISTRY]

- Owner: `ContentUuidCatalog` reflects every public static `Guid` on `ContentUuids`, derives kind and role from its fail-closed naming grammar, and refuses a duplicate seed id so `Find` only ever reads a validated census; `ContentTypeInfo` detaches registered factory descriptors; `ContentTypeCensus` returns both tiers without confusing type, default-instance, or CCI identifiers.
- Owner: `SerializerProgram` admits a generated extension, content kind, optional single-file programs, typed multi-load reports, and a `RetentionPolicy`; `ContentSerializer` adapts the host, folding every failure into one `RetentionPolicy`-bounded `FailureLedger` that surfaces typed `RetentionOverflow` evidence.
- Owner: `RetentionPolicy` admits a non-default `Dimension`, carries the parameterized capacity, and owns eviction; `FailureLedger<T>` folds an admission into a retained `Seq<T>` plus a `RetentionOverflow` count-and-fault accumulator, returning evicted rows for release. Serializer and event-stream ledgers ride it, so bounded custody drops resources but never diagnostics.
- Law: serializer reads accept only `ContentTransfer` over `Lease<RenderContent>.Owned`; `Take` transfers custody exactly once, and no borrowed lease can masquerade as host-owned output.
- Law: `SerializerDisposition` dispatches to `ReportContentAndFile` or `ReportDeferredContentAndFile`; load policy and kind cross generated correspondence owners before the program runs. A multi-load drains every report — a failed emit never strands later reports undisposed — and a content the host report refuses after `Take` is disposed before the fault leaves.
- Law: plug-in classes and serializers register through `Registry.Run`; registration returns typed evidence and rejects missing assemblies, serializers, or plug-in identities.
- Boundary: the host also discovers serializers through `RenderPlugIn.RenderContentSerializers()`; the adapter shape is this page's, the plug-in override that returns it is the plug-in's.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.DocObjects;
using Rhino.Render;

namespace Rasm.Rhino.Render;

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record ContentTypeInfo(Guid TypeId, string InternalName, Guid RenderEngineId, Guid PlugInId) : IDetachedDocumentResult {
    internal static Fin<Seq<ContentTypeInfo>> Census(Op key) =>
        key.Catch(() => toSeq(RenderContentType.GetAllAvailableTypes()).TraverseM(descriptor => key.Catch(() => {
            using (descriptor) {
                return Fin.Succ(value: new ContentTypeInfo(
                    TypeId: descriptor.Id, InternalName: descriptor.InternalName,
                    RenderEngineId: descriptor.RenderEngineId, PlugInId: descriptor.PlugInId));
            }
        })).As());
}

[SmartEnum<string>]
public sealed partial class ContentUuidRole {
    public static readonly ContentUuidRole Type = new("type");
    public static readonly ContentUuidRole DefaultInstance = new("default-instance");
    public static readonly ContentUuidRole Cci = new("cci");
}

public sealed record ContentUuidSeed(string Name, ContentKind Kind, ContentUuidRole Role, Guid Id)
    : IDetachedDocumentResult;

public static class ContentUuidCatalog {
    public static Fin<Seq<ContentUuidSeed>> Census(Op? key = null) {
        Op op = key.OrDefault();
        return from properties in op.Catch(() => Fin.Succ(toSeq(typeof(ContentUuids)
                   .GetProperties(BindingFlags.Public | BindingFlags.Static)
                   .Where(static property => property.PropertyType == typeof(Guid) && property.GetMethod is not null)
                   .OrderBy(static property => property.Name, StringComparer.Ordinal))))
               from seeds in properties.TraverseM(property => Seed(property, op)).As()
               from _ in guard(seeds.Map(static seed => seed.Id).Distinct().Count == seeds.Count, op.InvalidResult())
               select seeds;
    }

    public static Fin<Option<ContentUuidSeed>> Find(Guid id, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in guard(id != Guid.Empty, op.InvalidInput()).ToFin()
               from seeds in Census(op)
               select seeds.Find(seed => seed.Id == id);
    }

    private static Fin<ContentUuidSeed> Seed(PropertyInfo property, Op op) =>
        from role in Role(property.Name, op)
        from kind in Kind(property.Name, op)
        from id in op.Catch(() => property.GetValue(obj: null) is Guid value && value != Guid.Empty
            ? Fin.Succ(value)
            : Fin.Fail<Guid>(op.InvalidResult()))
        select new ContentUuidSeed(Name: property.Name, Kind: kind, Role: role, Id: id);

    private static Fin<ContentUuidRole> Role(string name, Op op) =>
        name.EndsWith("CCI", StringComparison.Ordinal) ? Fin.Succ(ContentUuidRole.Cci)
        : name.EndsWith("Instance", StringComparison.Ordinal) ? Fin.Succ(ContentUuidRole.DefaultInstance)
        : name.EndsWith("Type", StringComparison.Ordinal)
            || name.EndsWith("Texture", StringComparison.Ordinal) ? Fin.Succ(ContentUuidRole.Type)
        : Fin.Fail<ContentUuidRole>(op.InvalidResult());

    private static Fin<ContentKind> Kind(string name, Op op) =>
        name.Contains("Material", StringComparison.Ordinal) ? Fin.Succ(ContentKind.Material)
        : name.Contains("Environment", StringComparison.Ordinal) ? Fin.Succ(ContentKind.Environment)
        : name.Contains("Texture", StringComparison.Ordinal) ? Fin.Succ(ContentKind.Texture)
        : Fin.Fail<ContentKind>(op.InvalidResult());
}

[ValueObject<string>]
public sealed partial class ContentExtension {
    internal static Fin<ContentExtension> Of(string value, Op key) =>
        Validate(value, null, out ContentExtension? admitted) is null
            ? Fin.Succ(value: admitted!)
            : Fin.Fail<ContentExtension>(error: key.InvalidInput());

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value)) {
            validationError = new ValidationError("serializer extension is empty");
            return;
        }
        value = value.Trim();
        validationError = value.StartsWith('.', StringComparison.Ordinal)
            && value.Length > 1
            && value.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) < 0
                ? validationError
                : new ValidationError("serializer extension is invalid");
    }
}

[SmartEnum<int>]
public sealed partial class SerializerStage {
    public static readonly SerializerStage Read = new(0);
    public static readonly SerializerStage Write = new(1);
    public static readonly SerializerStage Load = new(2);
    public static readonly SerializerStage Register = new(3);
}

[SmartEnum<string>]
public sealed partial class LoadPolicy {
    public static readonly LoadPolicy Normal = new("normal", RenderContentSerializer.LoadMultipleFlags.Normal);
    public static readonly LoadPolicy Preload = new("preload", RenderContentSerializer.LoadMultipleFlags.Preload);

    internal RenderContentSerializer.LoadMultipleFlags Native { get; }

    internal static Fin<LoadPolicy> Of(RenderContentSerializer.LoadMultipleFlags native, Op key) =>
        key.Row(Items, native, static item => item.Native);
}

public sealed class ContentTransfer : IDisposable, IDetachedDocumentResult {
    private Lease<RenderContent>.Owned? owned;

    public ContentTransfer(Lease<RenderContent>.Owned owned) => this.owned = owned;

    internal Fin<RenderContent> Take(Op key) =>
        Optional(Interlocked.Exchange(ref owned, null)).ToFin(Fail: key.MissingContext())
            .Map(static lease => lease.Value);

    public void Dispose() => Interlocked.Exchange(ref owned, null)?.Dispose();
}

[SmartEnum<bool>]
public sealed partial class SerializerDisposition {
    public static readonly SerializerDisposition Loaded = new(false, static (loaded, _) => loaded());
    public static readonly SerializerDisposition Deferred = new(true, static (_, deferred) => deferred());

    [UseDelegateFromConstructor]
    internal partial Unit Fold(Func<Unit> loaded, Func<Unit> deferred);
}

[ComplexValueObject]
public sealed partial class SerializerReport : IDisposable, IDetachedDocumentResult {
    public SerializerDisposition Disposition { get; }
    public ContentTransfer Content { get; }
    public string Path { get; }
    public int Index { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref SerializerDisposition disposition,
        ref ContentTransfer content,
        ref string path,
        ref int index) {
        path = path?.Trim() ?? string.Empty;
        validationError = disposition is not null && content is not null
            && !string.IsNullOrWhiteSpace(path) && index >= 0
            ? validationError
            : new ValidationError(message: "serializer report is invalid");
    }

    public void Dispose() => Content.Dispose();
}

public readonly record struct RetentionOverflow(int Dropped, Error Evidence) : IDetachedDocumentResult {
    public static RetentionOverflow Empty { get; } = new(Dropped: 0, Evidence: Errors.None);

    public bool Any => Dropped > 0;

    internal RetentionOverflow Absorb(Error fault) =>
        new(Dropped: Dropped + 1, Evidence: Evidence + fault);
}

public sealed record RetentionPolicy {
    private RetentionPolicy(Dimension capacity) => Capacity = capacity;

    public Dimension Capacity { get; }

    public static Fin<RetentionPolicy> Of(Dimension capacity, Op? key = null) {
        Op op = key.OrDefault();
        return guard(capacity != default, op.InvalidInput()).ToFin()
            .Map(_ => new RetentionPolicy(capacity: capacity));
    }

    internal (Seq<T> Kept, Seq<T> Evicted) Admit<T>(Seq<T> held, T incoming) {
        Seq<T> grown = held.Add(incoming);
        int excess = grown.Count - Capacity.Value;
        return excess <= 0
            ? (Kept: grown, Evicted: Seq<T>())
            : (Kept: grown.Skip(excess), Evicted: grown.Take(excess));
    }
}

public readonly record struct FailureLedger<T>(Seq<T> Retained, RetentionOverflow Overflow) {
    public static FailureLedger<T> Empty { get; } = new(Retained: Seq<T>(), Overflow: RetentionOverflow.Empty);

    internal (FailureLedger<T> Ledger, Seq<T> Evicted) Admit(RetentionPolicy policy, T incoming, Func<T, Error> fault) {
        (Seq<T> kept, Seq<T> evicted) = policy.Admit(held: Retained, incoming: incoming);
        RetentionOverflow overflowed = evicted.Fold(Overflow, (state, dropped) => state.Absorb(fault(dropped)));
        return (Ledger: new FailureLedger<T>(Retained: kept, Overflow: overflowed), Evicted: evicted);
    }
}

public sealed record SerializerFailure(SerializerStage Stage, string Path, Error Fault) : IDetachedDocumentResult;

public sealed record SerializerProgram(
    ContentExtension FileExtension,
    ContentKind Kind,
    Option<Func<string, Fin<ContentTransfer>>> Read,
    Option<Func<string, RenderContent, CreatePreviewEventArgs, Fin<Unit>>> Write,
    Option<Func<RhinoDoc, Seq<string>, ContentKind, LoadPolicy, Fin<Seq<SerializerReport>>>> LoadMultiple,
    RetentionPolicy Retention,
    string EnglishDescription,
    string LocalDescription);

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class ContentSerializer : RenderContentSerializer {
    private readonly SerializerProgram program;
    private readonly Atom<FailureLedger<SerializerFailure>> ledger = Atom(FailureLedger<SerializerFailure>.Empty);

    private ContentSerializer(SerializerProgram program)
        : base(fileExtension: program.FileExtension.Value, contentKind: (RenderContentKind)program.Kind.Key,
               canRead: program.Read.IsSome, canWrite: program.Write.IsSome) =>
        this.program = program;

    public static Fin<ContentSerializer> Of(SerializerProgram program, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(program).ToFin(Fail: op.InvalidInput())
               from extension in Optional(active.FileExtension).ToFin(Fail: op.InvalidInput())
               from kind in Optional(active.Kind).ToFin(Fail: op.InvalidInput())
               from english in op.AcceptText(active.EnglishDescription)
               from local in op.AcceptText(active.LocalDescription)
               from retention in Optional(active.Retention).ToFin(Fail: op.InvalidInput())
               from _ in guard(active.Read.IsSome || active.Write.IsSome || active.LoadMultiple.IsSome, op.InvalidInput())
               select new ContentSerializer(active with {
                   FileExtension = extension,
                   Kind = kind,
                   EnglishDescription = english,
                   LocalDescription = local,
               });
    }

    public override string EnglishDescription => program.EnglishDescription;
    public override string LocalDescription => program.LocalDescription;
    public Seq<SerializerFailure> Failures => ledger.Value.Retained;
    public RetentionOverflow Overflow => ledger.Value.Overflow;

    [return: MaybeNull]
    public override RenderContent Read(string pathToFile) {
        Op op = Op.Of(name: nameof(Read));
        return (from path in op.AcceptText(pathToFile)
                from read in program.Read.ToFin(Fail: op.InvalidInput())
                from transfer in op.Catch(() => read(path))
                from active in Optional(transfer).ToFin(Fail: op.InvalidResult())
                from content in active.Take(op)
                select content).Match(
                    Succ: static content => content,
                    Fail: fault => Reject<RenderContent>(SerializerStage.Read, pathToFile, fault));
    }

    public override bool Write(string pathToFile, RenderContent renderContent, CreatePreviewEventArgs previewArgs) {
        Op op = Op.Of(name: nameof(Write));
        return (from path in op.AcceptText(pathToFile)
                from content in Optional(renderContent).ToFin(Fail: op.InvalidInput())
                from preview in Optional(previewArgs).ToFin(Fail: op.InvalidInput())
                from write in program.Write.ToFin(Fail: op.InvalidInput())
                from _ in op.Catch(() => write(path, content, preview))
                select unit).Match(
                    Succ: static _ => true,
                    Fail: fault => Reject(SerializerStage.Write, pathToFile, fault));
    }

    public override bool CanLoadMultiple() => program.LoadMultiple.IsSome;

    public override bool LoadMultiple(
        RhinoDoc document, IEnumerable<string> paths, RenderContentKind kind, RenderContentSerializer.LoadMultipleFlags flags) {
        Op op = Op.Of(name: nameof(LoadMultiple));
        return (from activeDocument in Optional(document).ToFin(Fail: op.InvalidInput())
                from activePaths in Optional(paths).ToFin(Fail: op.InvalidInput())
                from files in op.Catch(() => Fin.Succ(toSeq(activePaths)))
                from _0 in guard(!files.IsEmpty && files.ForAll(static path => !string.IsNullOrWhiteSpace(path)), op.InvalidInput())
                from load in program.LoadMultiple.ToFin(Fail: op.InvalidInput())
                from admittedKind in ContentKind.Of(kind, op)
                from policy in LoadPolicy.Of(flags, op)
                from reports in op.Catch(() => load(activeDocument, files, admittedKind, policy))
                from _ in reports.Map(report => Emit(report, op)).Strict()
                    .Fold(Fin.Succ(value: unit), static (state, outcome) => state.Bind(_ => outcome))
                select unit).Match(
                    Succ: static _ => true,
                    Fail: fault => Reject(SerializerStage.Load, string.Empty, fault));
    }

    internal Fin<Unit> Register(Guid pluginId) {
        Op op = Op.Of(name: nameof(ContentSerializer));
        Fin<Unit> registered =
            from _ in guard(pluginId != Guid.Empty, op.InvalidInput()).ToFin()
            from result in op.Catch(() => op.Confirm(success: RegisterSerializer(id: pluginId)))
            select result;
        return registered.Match(
            Succ: static value => Fin.Succ(value),
            Fail: fault => {
                _ = Retain(stage: SerializerStage.Register, path: string.Empty, fault: fault);
                return Fin.Fail<Unit>(fault);
            });
    }

    private Fin<Unit> Emit(SerializerReport report, Op op) {
        using (report) {
            return from active in Optional(report).ToFin(Fail: op.InvalidResult())
                   from transfer in Optional(active.Content).ToFin(Fail: op.InvalidResult())
                   from path in op.AcceptText(active.Path)
                   from content in transfer.Take(op)
                   from _ in op.Catch(() => {
                           _ = active.Disposition.Fold(
                               loaded: () => { ReportContentAndFile(content, path, active.Index); return unit; },
                               deferred: () => { ReportDeferredContentAndFile(content, path, active.Index); return unit; });
                           return Fin.Succ(unit);
                       })
                       .MapFail(fault => { content.Dispose(); return fault; })
                   select unit;
        }
    }

    private bool Reject(SerializerStage stage, string path, Error error) {
        _ = Retain(stage: stage, path: path, fault: error);
        return false;
    }

    private Unit Retain(SerializerStage stage, string path, Error fault) {
        _ = ledger.Swap(state => state.Admit(
            policy: program.Retention,
            incoming: new SerializerFailure(stage, path, fault),
            fault: static failure => failure.Fault).Ledger);
        return unit;
    }

    [return: MaybeNull]
    private T Reject<T>(SerializerStage stage, string path, Error error) where T : class {
        _ = Reject(stage, path, error);
        return default;
    }
}
```

## [03]-[OPERATION_FAMILY]

- Owner: `ContentOp` `[Union]` derives from target identity: `Admit(ContentAdmission)` has no existing target, and `Mutate(ContentRef, ContentMutation)` resolves one target once. `ContentAdmission` closes each mint path behind one owned-lease rail. `ContentMutation` carries catalogued host concerns; `TreeMutation` and `Grouping` close their bounded subspaces without boolean modes.
- Law: admission internalizes custody — every factory, IO, material, texture, and environment mint becomes an owned lease; top-level results transfer through the expected kind table, parented factory results transfer through the parent slot, and every refused transfer disposes the lease.
- Law: transaction kind is a verified table-scope key — each admission exposes its expected kind, each target mutation derives its live kind, and either must equal the plan kind before mutation.
- Law: graph surgery is one target mutation — `TreeMutation` discriminates graft, prune, and slot state under its own `ChangeReason`; graft and parented admission prove `IsContentTypeAcceptableAsChild` before `SetChild`, and slot-state admission rejects an empty patch.
- Law: field, parameter, and texture writes compose their owners; material assignment resolves `TableTarget`, contains every `ObjRef` lifetime, and carries native assignment choices.
- Growth: a new admission path is one `ContentAdmission` case; a new target concern is one `ContentMutation` case; `ContentOp` keeps its identity-derived cases.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentAdmission {
    private ContentAdmission() { }
    public sealed record Factory(ContentKind Kind, Guid TypeId, Option<(ContentRef Parent, string Slot)> Into) : ContentAdmission;
    public sealed record Serialized(ContentKind Kind, ContentIo Source) : ContentAdmission;
    public sealed record Material(MaterialMint Source) : ContentAdmission;
    public sealed record Texture(TextureMint Source) : ContentAdmission;
    public sealed record Environment(EnvironmentState State) : ContentAdmission;

    internal ContentKind Expected => Switch(
        factory: static row => row.Kind,
        serialized: static row => row.Kind,
        material: static _ => ContentKind.Material,
        texture: static _ => ContentKind.Texture,
        environment: static _ => ContentKind.Environment);

    internal Fin<ContentReceipt> Apply(RhinoDoc document, ChangeReason reason, Op op) =>
        Switch(
            context: (Document: document, Reason: reason, Op: op),
            factory: static (context, source) =>
                from kind in Optional(source.Kind).ToFin(Fail: context.Op.InvalidInput())
                from _ in guard(source.TypeId != Guid.Empty, context.Op.InvalidInput())
                from parent in source.Into.Traverse(into =>
                    from slot in context.Op.AcceptText(value: into.Slot)
                    from target in Optional(into.Parent).ToFin(Fail: context.Op.InvalidInput())
                    from live in target.Resolve(document: context.Document, key: context.Op)
                    select (Content: live, Slot: slot)).As()
                from minted in context.Op.Catch(() => Optional(RenderContent.Create(context.Document, source.TypeId))
                    .ToFin(Fail: context.Op.InvalidResult()))
                from receipt in Transfer(
                    expected: kind,
                    lease: new Lease<RenderContent>.Owned(Value: minted),
                    document: context.Document,
                    parent: parent,
                    reason: context.Reason,
                    op: context.Op)
                select receipt,
            serialized: static (context, source) =>
                from kind in Optional(source.Kind).ToFin(Fail: context.Op.InvalidInput())
                from receipt in Adopted(kind, source.Source, static (io, ctx) => io.Mint(document: ctx.Document, key: ctx.Op), context)
                select receipt,
            material: static (context, source) =>
                Adopted(ContentKind.Material, source.Source, static (mint, ctx) => mint.Mint(document: ctx.Document, key: ctx.Op), context),
            texture: static (context, source) =>
                Adopted(ContentKind.Texture, source.Source, static (mint, ctx) => mint.Mint(document: ctx.Document, key: ctx.Op), context),
            environment: static (context, source) =>
                Adopted(ContentKind.Environment, source.State, static (state, ctx) => state.Mint(document: ctx.Document, key: ctx.Op), context));

    private static Fin<ContentReceipt> Adopted<TSource>(
        ContentKind expected,
        TSource? source,
        Func<TSource, (RhinoDoc Document, ChangeReason Reason, Op Op), Fin<Lease<RenderContent>>> mint,
        (RhinoDoc Document, ChangeReason Reason, Op Op) context) where TSource : class =>
        from active in Optional(source).ToFin(Fail: context.Op.InvalidInput())
        from lease in mint(active, context)
        from receipt in Transfer(
            expected: expected, lease: lease, document: context.Document,
            parent: Option<(RenderContent, string)>.None, reason: context.Reason, op: context.Op)
        select receipt;

    private static Fin<ContentReceipt> Transfer(
        ContentKind expected, Lease<RenderContent> lease, RhinoDoc document,
        Option<(RenderContent Content, string Slot)> parent, ChangeReason reason, Op op) {
        Fin<ContentReceipt> outcome =
            from actual in ContentKind.Of(lease.Resource, op)
            from _ in guard(actual == expected, op.InvalidInput())
            from __ in parent.Case switch {
                (RenderContent content, string slot) =>
                    from _acceptable in TreeMutation.Accepts(
                        parent: content, child: lease.Resource, slot: slot, op: op)
                    from _written in ChangeScope.Write(
                        content: content, reason: reason, key: op,
                        body: live => op.Catch(() => op.Confirm(success: live.SetChild(lease.Resource, slot))))
                    select unit,
                _ => op.Catch(() => op.Confirm(success: expected.Attach(document: document, content: lease.Resource))),
            }
            select ContentReceipt.Content(slot: ContentSlot.Minted, id: lease.Resource.Id)
                + ContentReceipt.Content(slot: ContentSlot.Adopted, id: lease.Resource.Id);
        return outcome.Match(
            Succ: static receipt => Fin.Succ(value: receipt),
            Fail: error => { lease.Dispose(); return Fin.Fail<ContentReceipt>(error: error); });
    }
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TreeMutation {
    private TreeMutation() { }
    public sealed record Graft(string Slot, ContentRef Child, ChangeReason Reason) : TreeMutation;
    public sealed record Prune(Option<string> Slot, ChangeReason Reason) : TreeMutation;
    public sealed record Slot(string Name, Option<bool> On, Option<double> Amount, ChangeReason Reason) : TreeMutation;

    internal Fin<ContentReceipt> Apply(RenderContent parent, RhinoDoc document, Op op) =>
        Switch(
            context: (Parent: parent, Document: document, Op: op),
            graft: static (ctx, edit) =>
                from slot in ctx.Op.AcceptText(value: edit.Slot)
                from target in Optional(edit.Child).ToFin(Fail: ctx.Op.InvalidInput())
                from reason in Optional(edit.Reason).ToFin(Fail: ctx.Op.InvalidInput())
                from child in target.Resolve(document: ctx.Document, key: ctx.Op)
                from _acceptable in Accepts(parent: ctx.Parent, child: child, slot: slot, op: ctx.Op)
                from _ in ChangeScope.Write(content: ctx.Parent, reason: reason, key: ctx.Op,
                    body: live => ctx.Op.Catch(() => ctx.Op.Confirm(success: live.SetChild(renderContent: child, childSlotName: slot))))
                select ContentReceipt.Content(slot: ContentSlot.Grafted, id: ctx.Parent.Id),
            prune: static (ctx, edit) =>
                from reason in Optional(edit.Reason).ToFin(Fail: ctx.Op.InvalidInput())
                from slot in edit.Slot.Traverse(value => ctx.Op.AcceptText(value: value)).As()
                from _ in ChangeScope.Write(content: ctx.Parent, reason: reason, key: ctx.Op,
                    body: live => slot.Case switch {
                        string name => ctx.Op.Catch(() => ctx.Op.Confirm(success: live.DeleteChild(name, reason.Native))),
                        _ => ctx.Op.Catch(() => { live.DeleteAllChildren(reason.Native); return Fin.Succ(value: unit); }),
                    })
                select ContentReceipt.Content(slot: ContentSlot.Pruned, id: ctx.Parent.Id),
            slot: static (ctx, edit) =>
                from name in ctx.Op.AcceptText(value: edit.Name)
                from reason in Optional(edit.Reason).ToFin(Fail: ctx.Op.InvalidInput())
                from _ in guard(
                    (edit.On.IsSome || edit.Amount.IsSome)
                    && edit.Amount.Map(static amount => double.IsFinite(amount)).IfNone(true),
                    ctx.Op.InvalidInput())
                from __ in ChangeScope.Write(content: ctx.Parent, reason: reason, key: ctx.Op, body: live => ctx.Op.Catch(() => {
                    _ = edit.On.Iter(on => live.SetChildSlotOn(name, on, reason.Native));
                    _ = edit.Amount.Iter(amount => live.SetChildSlotAmount(name, amount, reason.Native));
                    return Fin.Succ(value: unit);
                }))
                select ContentReceipt.Content(slot: ContentSlot.SlotSet, id: ctx.Parent.Id));

    internal static Fin<Unit> Accepts(RenderContent parent, RenderContent child, string slot, Op op) =>
        op.Catch(() => op.Confirm(success: parent.IsContentTypeAcceptableAsChild(
            type: child.TypeId,
            childSlotName: slot)));
}

[SmartEnum<string>]
public sealed partial class Grouping {
    public static readonly Grouping Make = new("make", static (content, op) =>
        op.Catch(() => Optional(content.MakeGroupInstance()).ToFin(Fail: op.InvalidResult())
            .Map(grouped => ContentReceipt.Content(slot: ContentSlot.Grouped, id: grouped.Id))));
    public static readonly Grouping Ungroup = new("ungroup", Undone(static content => content.Ungroup()));
    public static readonly Grouping Recursive = new("recursive", Undone(static content => content.UngroupRecursive()));
    public static readonly Grouping Smart = new("smart", Undone(static content => content.SmartUngroupRecursive()));

    [UseDelegateFromConstructor]
    internal partial Fin<ContentReceipt> Apply(RenderContent content, Op op);

    private static Func<RenderContent, Op, Fin<ContentReceipt>> Undone(Func<RenderContent, bool> route) =>
        (content, op) => op.Catch(() => op.Confirm(success: route(content)))
            .Map(_ => ContentReceipt.Content(slot: ContentSlot.Ungrouped, id: content.Id));
}

[SmartEnum<bool>]
public sealed partial class RenamePolicy {
    public static readonly RenamePolicy Exact = new(false);
    public static readonly RenamePolicy Unique = new(true);

    internal bool EnsuresUnique => Key;
}

[SmartEnum<string>]
public sealed partial class ExtraRequirementReason {
    public static readonly ExtraRequirementReason Ui = new("ui", RenderContent.ExtraRequirementsSetContexts.UI);
    public static readonly ExtraRequirementReason Drop = new("drop", RenderContent.ExtraRequirementsSetContexts.Drop);
    public static readonly ExtraRequirementReason Program = new("program", RenderContent.ExtraRequirementsSetContexts.Program);

    internal RenderContent.ExtraRequirementsSetContexts Native { get; }
}

[SmartEnum<string>]
public sealed partial class SubFaceAssignment {
    public static readonly SubFaceAssignment Keep = new("keep", RenderMaterial.AssignToSubFaceChoices.Keep);
    public static readonly SubFaceAssignment Remove = new("remove", RenderMaterial.AssignToSubFaceChoices.Remove);
    public static readonly SubFaceAssignment Ask = new("ask", RenderMaterial.AssignToSubFaceChoices.Ask);

    internal RenderMaterial.AssignToSubFaceChoices Native { get; }
}

[SmartEnum<string>]
public sealed partial class BlockAssignment {
    public static readonly BlockAssignment Always = new("always", RenderMaterial.AssignToBlockChoices.Always);
    public static readonly BlockAssignment Never = new("never", RenderMaterial.AssignToBlockChoices.Never);
    public static readonly BlockAssignment Ask = new("ask", RenderMaterial.AssignToBlockChoices.Ask);

    internal RenderMaterial.AssignToBlockChoices Native { get; }
}

[SmartEnum<string>]
public sealed partial class EmbedPolicy {
    public static readonly EmbedPolicy Never = new("never", RenderContent.EmbedFilesChoice.NeverEmbed);
    public static readonly EmbedPolicy Always = new("always", RenderContent.EmbedFilesChoice.AlwaysEmbed);
    public static readonly EmbedPolicy Ask = new("ask", RenderContent.EmbedFilesChoice.AskUser);

    internal RenderContent.EmbedFilesChoice Native { get; }
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentMutation {
    private ContentMutation() { }
    public sealed record Detach : ContentMutation;
    public sealed record Rename(string Name, ChangeReason Reason, RenamePolicy Policy) : ContentMutation;
    public sealed record Tree(TreeMutation Edit) : ContentMutation;
    public sealed record Field(string Name, FieldValue Value, ChangeReason Reason) : ContentMutation;
    public sealed record Param(
        ParamScope Scope, FieldValue Value, ChangeReason Reason,
        ExtraRequirementReason Context) : ContentMutation;
    public sealed record Texture(TextureConfig Config, ChangeReason Reason) : ContentMutation;
    public sealed record Assign(TableTarget Objects, SubFaceAssignment SubFaces, BlockAssignment Blocks) : ContentMutation;
    public sealed record Replace(ContentIo Source) : ContentMutation;
    public sealed record Group(Grouping Mode) : ContentMutation;
    public sealed record Export(ContentExport Output) : ContentMutation;

    internal bool RecordsUndo => this is not Export;

    internal Fin<ContentReceipt> Apply(RenderContent content, RhinoDoc document, Op op) =>
        Switch(
            context: (Content: content, Document: document, Op: op),
            detach: static (ctx, _) =>
                from kind in ContentKind.Of(ctx.Content, ctx.Op)
                from _ in ctx.Op.Catch(() => ctx.Op.Confirm(success: kind.Detach(ctx.Document, ctx.Content)))
                select ContentReceipt.Content(slot: ContentSlot.Detached, id: ctx.Content.Id),
            rename: static (ctx, edit) =>
                from name in ctx.Op.AcceptText(value: edit.Name)
                from reason in Optional(edit.Reason).ToFin(Fail: ctx.Op.InvalidInput())
                from policy in Optional(edit.Policy).ToFin(Fail: ctx.Op.InvalidInput())
                from _ in ChangeScope.Write(ctx.Content, reason, live => ctx.Op.Catch(() => {
                    live.SetName(name, renameEvents: true, ensureNameUnique: policy.EnsuresUnique);
                    return Fin.Succ(value: unit);
                }), ctx.Op)
                select ContentReceipt.Content(slot: ContentSlot.Renamed, id: ctx.Content.Id),
            tree: static (ctx, edit) =>
                from change in Optional(edit.Edit).ToFin(Fail: ctx.Op.InvalidInput())
                from receipt in change.Apply(parent: ctx.Content, document: ctx.Document, op: ctx.Op)
                select receipt,
            field: static (ctx, edit) =>
                from name in ctx.Op.AcceptText(value: edit.Name)
                from value in Optional(edit.Value).ToFin(Fail: ctx.Op.InvalidInput())
                from reason in Optional(edit.Reason).ToFin(Fail: ctx.Op.InvalidInput())
                from _ in ChangeScope.Write(ctx.Content, reason, live => value.Write(live.Fields, name, ctx.Op), ctx.Op)
                select ContentReceipt.Content(slot: ContentSlot.FieldSet, id: ctx.Content.Id),
            param: static (ctx, edit) =>
                from scope in Optional(edit.Scope).ToFin(Fail: ctx.Op.InvalidInput())
                from value in Optional(edit.Value).ToFin(Fail: ctx.Op.InvalidInput())
                from reason in Optional(edit.Reason).ToFin(Fail: ctx.Op.InvalidInput())
                from context in Optional(edit.Context).ToFin(Fail: ctx.Op.InvalidInput())
                from _ in scope.Write(ctx.Content, value, reason, context.Native, ctx.Op)
                select ContentReceipt.Content(slot: ContentSlot.FieldSet, id: ctx.Content.Id),
            texture: static (ctx, edit) =>
                from texture in Optional(ctx.Content as RenderTexture).ToFin(Fail: ctx.Op.InvalidInput())
                from config in Optional(edit.Config).ToFin(Fail: ctx.Op.InvalidInput())
                from reason in Optional(edit.Reason).ToFin(Fail: ctx.Op.InvalidInput())
                from _ in config.Apply(texture, reason, ctx.Op)
                select ContentReceipt.Content(slot: ContentSlot.Configured, id: ctx.Content.Id),
            assign: static (ctx, edit) =>
                from material in Optional(ctx.Content as RenderMaterial).ToFin(Fail: ctx.Op.InvalidInput())
                from objects in Optional(edit.Objects).ToFin(Fail: ctx.Op.InvalidInput())
                from subFaces in Optional(edit.SubFaces).ToFin(Fail: ctx.Op.InvalidInput())
                from blocks in Optional(edit.Blocks).ToFin(Fail: ctx.Op.InvalidInput())
                from ids in objects.Resolve(document: ctx.Document, key: ctx.Op)
                from _ in ctx.Op.Catch(() => {
                    ObjRef[] references = new ObjRef[ids.Count];
                    int minted = 0;
                    try {
                        foreach (Guid id in ids) {
                            references[minted] = new ObjRef(ctx.Document, id);
                            minted++;
                        }
                        return ctx.Op.Confirm(success: material.AssignTo(
                            references, subFaces.Native, blocks.Native, bInteractive: false));
                    } finally {
                        for (int index = 0; index < minted; index++) {
                            references[index].Dispose();
                        }
                    }
                })
                select ContentReceipt.Objects(slot: ContentSlot.Assigned, ids: ids),
            replace: static (ctx, edit) =>
                from source in Optional(edit.Source).ToFin(Fail: ctx.Op.InvalidInput())
                from lease in source.Mint(document: ctx.Document, key: ctx.Op)
                from receipt in ReplaceWith(target: ctx.Content, lease: lease, op: ctx.Op)
                select receipt,
            group: static (ctx, edit) =>
                from mode in Optional(edit.Mode).ToFin(Fail: ctx.Op.InvalidInput())
                from receipt in mode.Apply(content: ctx.Content, op: ctx.Op)
                select receipt,
            export: static (ctx, edit) =>
                from output in Optional(edit.Output).ToFin(Fail: ctx.Op.InvalidInput())
                from receipt in output.Switch(
                    context: (Content: ctx.Content, Op: ctx.Op),
                    archive: static (state, archive) =>
                        from embed in Optional(archive.Embed).ToFin(Fail: state.Op.InvalidInput())
                        from path in state.Op.AcceptText(value: archive.Path)
                        from _ in state.Op.Catch(() => state.Op.Confirm(success: state.Content.SaveToFile(path, embed.Native)))
                        select ContentReceipt.Path(slot: ContentSlot.Exported, path: path),
                    textureImage: static (state, image) =>
                        from texture in Optional(state.Content as RenderTexture).ToFin(Fail: state.Op.InvalidInput())
                        from path in state.Op.AcceptText(value: image.Path)
                        from _ in TextureExport.Export(
                            texture: texture, path: path,
                            width: image.Width, height: image.Height, depth: image.Depth, key: state.Op)
                        select ContentReceipt.Path(slot: ContentSlot.Exported, path: path))
                select receipt);

    private static Fin<ContentReceipt> ReplaceWith(RenderContent target, Lease<RenderContent> lease, Op op) {
        Fin<ContentReceipt> outcome =
            from targetKind in ContentKind.Of(target, op)
            from replacementKind in ContentKind.Of(lease.Resource, op)
            from _ in guard(targetKind == replacementKind, op.InvalidInput())
            from __ in op.Catch(() => op.Confirm(success: target.Replace(newcontent: lease.Resource)))
            select ContentReceipt.Content(slot: ContentSlot.Swapped, id: lease.Resource.Id);
        return outcome.Match(
            Succ: static receipt => Fin.Succ(value: receipt),
            Fail: error => { lease.Dispose(); return Fin.Fail<ContentReceipt>(error: error); });
    }
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentExport {
    private ContentExport() { }
    public sealed record Archive(string Path, EmbedPolicy Embed) : ContentExport;
    public sealed record TextureImage(string Path, int Width, int Height, int Depth) : ContentExport;
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentOp {
    private ContentOp() { }
    public sealed record Admit(ContentAdmission Source) : ContentOp;
    public sealed record Mutate(ContentRef Target, ContentMutation Change) : ContentOp;

    internal bool RecordsUndo => Switch(
        admit: static _ => true,
        mutate: static edit => Optional(edit.Change).Map(static change => change.RecordsUndo).IfNone(false));

    internal Fin<ContentReceipt> Apply(RhinoDoc document, ContentKind scope, ChangeReason reason, Op op) =>
        Switch(
            context: (Document: document, Scope: scope, Reason: reason, Op: op),
            admit: static (ctx, edit) =>
                from source in Optional(edit.Source).ToFin(Fail: ctx.Op.InvalidInput())
                from _ in guard(source.Expected == ctx.Scope, ctx.Op.InvalidInput())
                from receipt in source.Apply(document: ctx.Document, reason: ctx.Reason, op: ctx.Op)
                select receipt,
            mutate: static (ctx, edit) =>
                from target in Optional(edit.Target).ToFin(Fail: ctx.Op.InvalidInput())
                from change in Optional(edit.Change).ToFin(Fail: ctx.Op.InvalidInput())
                from content in target.Resolve(document: ctx.Document, key: ctx.Op)
                from kind in ContentKind.Of(content, ctx.Op)
                from _ in guard(kind == ctx.Scope, ctx.Op.InvalidInput())
                from receipt in change.Apply(content: content, document: ctx.Document, op: ctx.Op)
                select receipt);
}
```

## [04]-[COMMIT_AND_QUERY]

- Owner: `RegistryCommand` closes content registration, serializer registration, and document mutation; `RegistryResult` keeps each receipt distinct; `Registry.Run` is the sole change entry.
- Owner: `RegistryQuery<T>` closes target reads, rosters, current environments, and the two-tier factory census; `Registry.Read<T>` preserves result correlation through `IDetachedDocumentResult`.
- Law: the spine is the one bracket owner — the whole mutation runs inside one `Demand` window, the undo record opens through the document `UndoBracket` only when the plan records, the plan's kind opens its table change scope around the fold and closes it on every exit, redraw suppression restores prior state, and the bracket's `Seal` rolls a failed owned record back before the fault leaves.
- Law: grants are proven per plan shape against one snapshot — `Mutate` always, `Undo` when the plan records, `Redraw` when the plan redraws — and the session is the only document ingress; the redraw vocabulary is the document `RedrawPolicy` rows, shared with the table and block rails.
- Law: reads never open an undo record; every answer is a detached fact or self-disposing capsule, and `IconRequest` closes standard, virtual, and dynamic bitmap generation under one `ContentIcon` lease.
- Law: `ContentCollectionProbe` admits collection and kind-list leases once; owned cases dispose after the query and borrowed cases remain host-owned, while the answer detaches usage, members, editor state, thumbnail need, and kind evidence.
- Law: the workflow-corrected hash resolves the document's own `LinearWorkflow` inside the query window off the probe's `DocumentWorkflow` posture; a live sub-owner never enters or leaves a query value.
- Boundary: the current-environment triple reads through `RhinoDoc.CurrentEnvironment`; the settings-side per-usage binding is the settings page's edit rail, and the two never merge.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
[SmartEnum<bool>]
public sealed partial class UndoPolicy {
    public static readonly UndoPolicy Skip = new(false);
    public static readonly UndoPolicy Record = new(true);

    internal bool Enabled => Key;
}

public sealed record ContentTransaction(
    string Name,
    ContentKind Kind,
    Seq<ContentOp> Operations,
    ChangeReason Reason,
    RedrawPolicy Redraw,
    UndoPolicy Undo) {
    public static ContentTransaction Batch(string name, ContentKind kind, ChangeReason reason, params ReadOnlySpan<ContentOp> operations) =>
        new(
            Name: name,
            Kind: kind,
            Operations: toSeq(operations.ToArray()),
            Reason: reason,
            Redraw: RedrawPolicy.Deferred,
            Undo: UndoPolicy.Record);
}

public sealed record EnvironmentBindings(
    Option<Guid> Background,
    Option<Guid> Reflection,
    Option<Guid> Lighting) : IDetachedDocumentResult;

public sealed record ContentArchive(string Xml, Seq<string> EmbeddedFiles) : IDetachedDocumentResult;

public sealed record ContentRoster(ContentKind Kind, Seq<Guid> Ids) : IDetachedDocumentResult;

[SmartEnum<string>]
public sealed partial class ContentUsageFilter {
    public static readonly ContentUsageFilter None = new("none", FilterContentByUsage.None);
    public static readonly ContentUsageFilter Used = new("used", FilterContentByUsage.Used);
    public static readonly ContentUsageFilter Unused = new("unused", FilterContentByUsage.Unused);
    public static readonly ContentUsageFilter UsedSelected = new("used-selected", FilterContentByUsage.UsedSelected);

    internal FilterContentByUsage Native { get; }

    internal static Fin<ContentUsageFilter> Of(FilterContentByUsage native, Op key) =>
        key.Row(Items, native, static item => item.Native);
}

public sealed record ContentCollectionProbe(
    Lease<RenderContentCollection> Collection,
    Lease<RenderContentKindList> Kinds);

public sealed record ContentCollectionEvidence(
    ContentUsageFilter Usage,
    Seq<Guid> Members,
    bool ForcedVaries,
    Option<string> SearchPattern,
    bool NeedsPreview,
    int KindCount,
    bool ContainsContentKind,
    Option<ContentKind> SingleKind) : IDetachedDocumentResult;

[SmartEnum<string>]
public sealed partial class MatchVerdict {
    public static readonly MatchVerdict None = new("none", RenderContent.MatchDataResult.None);
    public static readonly MatchVerdict Some = new("some", RenderContent.MatchDataResult.Some);
    public static readonly MatchVerdict All = new("all", RenderContent.MatchDataResult.All);

    internal RenderContent.MatchDataResult Native { get; }

    internal static Fin<MatchVerdict> Of(RenderContent.MatchDataResult native, Op key) =>
        key.Row(Items, native, static item => item.Native);
}

[SmartEnum<string>]
public sealed partial class DynamicIconPolicy {
    public static readonly DynamicIconPolicy Tree = new("tree", DynamicIconUsage.TreeControl);
    public static readonly DynamicIconPolicy Subnode = new("subnode", DynamicIconUsage.SubnodeControl);
    public static readonly DynamicIconPolicy Content = new("content", DynamicIconUsage.ContentControl);
    public static readonly DynamicIconPolicy General = new("general", DynamicIconUsage.General);

    internal DynamicIconUsage Native { get; }
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IconRequest {
    private IconRequest() { }
    public sealed record Standard(Size2i Extent) : IconRequest;
    public sealed record Virtual(Size2i Extent) : IconRequest;
    public sealed record Dynamic(Size2i Extent, DynamicIconPolicy Policy) : IconRequest;
}

public readonly record struct MatchEvidence(MatchVerdict Verdict) : IDetachedDocumentResult;

public readonly record struct CompatibilityEvidence(Guid RenderEngineId, bool Compatible) : IDetachedDocumentResult;

public sealed record ContentIcon(Lease<System.Drawing.Bitmap> Image) : IDetachedDocumentResult, IDisposable {
    public void Dispose() => Image.Dispose();
}

public sealed class ContentQuery<T> where T : IDetachedDocumentResult {
    private readonly Func<RhinoDoc, RenderContent, Op, Fin<T>> read;

    internal ContentQuery(Func<RhinoDoc, RenderContent, Op, Fin<T>> read) => this.read = read;

    internal Fin<T> Run(RhinoDoc document, RenderContent content, Op op) => read(document, content, op);
}

public static class ContentQuery {
    public static ContentQuery<ContentSnapshot> Snapshot { get; } =
        new(read: static (_, content, op) => ContentSnapshot.Of(content: content, key: op));

    public static ContentQuery<ContentArchive> Archive { get; } =
        new(read: static (_, content, op) =>
            from xml in op.AcceptText(value: content.Xml)
            from embedded in op.Catch(() => Fin.Succ(value: toSeq(content.GetEmbeddedFilesList())))
            select new ContentArchive(Xml: xml, EmbeddedFiles: embedded));

    public static ContentQuery<FieldCensus> Fields { get; } =
        new(read: static (_, content, op) => FieldCensus.Of(fields: content.Fields, key: op));

    public static ContentQuery<ScentCensus> Scents { get; } =
        As<RenderMaterial, ScentCensus>(static (material, _) => Fin.Succ(value: MaterialScent.CensusOf(material: material)));

    public static ContentQuery<TextureConfig> Config { get; } =
        As<RenderTexture, TextureConfig>(static (texture, op) => TextureConfig.Of(texture: texture, key: op));

    public static ContentQuery<TextureTraits> Traits { get; } =
        As<RenderTexture, TextureTraits>(static (texture, op) => TextureTraits.Of(texture: texture, key: op));

    public static ContentQuery<HashWitness> Hash(HashProbe probe) =>
        new(read: (document, content, op) =>
            from active in Optional(probe).ToFin(Fail: op.InvalidInput())
            from value in active.DocumentWorkflow
                ? active.Read(content: content, workflow: document.RenderSettings.LinearWorkflow, key: op)
                : active.Read(content: content, key: op)
            select new HashWitness(
                Flags: active.Flags, Excluded: active.ExcludedParameters,
                DocumentWorkflow: active.DocumentWorkflow, Value: value));

    public static ContentQuery<FieldValue> Param(ParamScope scope) =>
        new(read: (_, content, op) =>
            from active in Optional(scope).ToFin(Fail: op.InvalidInput())
            from value in active.Read(content: content, key: op)
            select value);

    public static ContentQuery<SlotUsage> Usage(RenderMaterial.StandardChildSlots slot) =>
        As<RenderMaterial, SlotUsage>((material, op) =>
            from _ in guard(Enum.IsDefined(slot), op.InvalidInput()).ToFin()
            from usage in MaterialBridge.Usage(material: material, slot: slot, key: op)
            select usage);

    public static ContentQuery<TOut> Bake<TOut>(
        RenderTexture.TextureGeneration generation, Func<Material, Fin<TOut>> borrow)
        where TOut : IDetachedDocumentResult =>
        As<RenderMaterial, TOut>((material, op) =>
            from activeBorrow in Optional(borrow).ToFin(Fail: op.InvalidInput())
            from _ in guard(Enum.IsDefined(generation), op.InvalidInput()).ToFin()
            from result in MaterialBridge.Bake(
                material: material, generation: generation, borrow: activeBorrow, key: op)
            select result);

    public static ContentQuery<TOut> Pbr<TOut>(
        RenderTexture.TextureGeneration generation, Func<PhysicallyBasedMaterial, Fin<TOut>> borrow)
        where TOut : IDetachedDocumentResult =>
        As<RenderMaterial, TOut>((material, op) =>
            from activeBorrow in Optional(borrow).ToFin(Fail: op.InvalidInput())
            from _ in guard(Enum.IsDefined(generation), op.InvalidInput()).ToFin()
            from result in MaterialBridge.Pbr(
                material: material, generation: generation, borrow: activeBorrow, key: op)
            select result);

    public static ContentQuery<EnvironmentState> Environment(bool dataOnly) =>
        As<RenderEnvironment, EnvironmentState>((environment, op) =>
            EnvironmentState.Bake(environment: environment, isForDataOnly: dataOnly, key: op));

    public static ContentQuery<ContentIcon> Icon(IconRequest request) =>
        new(read: (_, content, op) =>
            from active in Optional(request).ToFin(Fail: op.InvalidInput())
            from icon in op.Catch(() => active.Switch(
                context: (Content: content, Op: op),
                standard: static (state, query) => Own(
                    state.Content.Icon(query.Extent.Native, out System.Drawing.Bitmap rendered), rendered, state.Op),
                @virtual: static (state, query) => Own(
                    state.Content.VirtualIcon(query.Extent.Native, out System.Drawing.Bitmap rendered), rendered, state.Op),
                dynamic: static (state, query) =>
                    from policy in Optional(query.Policy).ToFin(Fail: state.Op.InvalidInput())
                    from icon in Own(
                        state.Content.DynamicIcon(query.Extent.Native, out System.Drawing.Bitmap rendered, policy.Native),
                        rendered,
                        state.Op)
                    select icon))
            select icon);

    public static ContentQuery<MatchEvidence> Match(ContentRef old) =>
        new(read: (document, content, op) =>
            from reference in Optional(old).ToFin(Fail: op.InvalidInput())
            from prior in reference.Resolve(document: document, key: op)
            from native in op.Catch(() => Fin.Succ(content.MatchData(oldContent: prior)))
            from verdict in MatchVerdict.Of(native, op)
            select new MatchEvidence(Verdict: verdict));

    public static ContentQuery<CompatibilityEvidence> Compatible(Guid renderEngineId) =>
        new(read: (_, content, op) =>
            from _ in guard(renderEngineId != Guid.Empty, op.InvalidInput()).ToFin()
            from compatible in op.Catch(() => Fin.Succ(value: content.IsCompatible(renderEngineId)))
            select new CompatibilityEvidence(RenderEngineId: renderEngineId, Compatible: compatible));

    public static ContentQuery<ContentCollectionEvidence> Collection(ContentCollectionProbe probe) =>
        new(read: (_, content, op) =>
            from active in Optional(probe).ToFin(Fail: op.InvalidInput())
            from collectionLease in Optional(active.Collection).ToFin(Fail: op.InvalidInput())
            from kindLease in Optional(active.Kinds).ToFin(Fail: op.InvalidInput())
            from result in collectionLease.Use(collection => kindLease.Use(kinds =>
                from usage in op.Catch(() => ContentUsageFilter.Of(collection.GetFilterContentByUsage(), op))
                from count in op.Catch(() => Fin.Succ(collection.Count()))
                from members in toSeq(Enumerable.Range(0, count)).TraverseM(index => op.Catch(() =>
                    Optional(collection.ContentAt(index)).ToFin(Fail: op.InvalidResult())
                        .Map(static row => row.Id))).As()
                from kind in ContentKind.Of(content, op)
                from kindCount in op.Catch(() => Fin.Succ(kinds.Count()))
                from contains in op.Catch(() => Fin.Succ(kinds.Contains((RenderContentKind)kind.Key)))
                from single in kindCount == 1
                    ? ContentKind.Of(kinds.SingleKind(), op).Map(static value => Some(value))
                    : Fin.Succ(Option<ContentKind>.None)
                from evidence in op.Catch(() => Fin.Succ(new ContentCollectionEvidence(
                    Usage: usage,
                    Members: toSeq(members),
                    ForcedVaries: collection.GetForcedVaries(),
                    SearchPattern: Optional(collection.GetSearchPattern())
                        .Filter(static value => !string.IsNullOrWhiteSpace(value)),
                    NeedsPreview: collection.ContentNeedsPreviewThumbnail(content),
                    KindCount: kindCount,
                    ContainsContentKind: contains,
                    SingleKind: single)))
                select evidence))
            select result);

    private static Fin<ContentIcon> Own(bool succeeded, System.Drawing.Bitmap? bitmap, Op op) {
        if (succeeded) {
            return Optional(bitmap).ToFin(Fail: op.InvalidResult())
                .Map(static image => new ContentIcon(new Lease<System.Drawing.Bitmap>.Owned(Value: image)));
        }
        Optional(bitmap).Iter(static image => image.Dispose());
        return Fin.Fail<ContentIcon>(op.InvalidResult());
    }

    private static ContentQuery<TOut> As<TContent, TOut>(Func<TContent, Op, Fin<TOut>> project)
        where TContent : RenderContent where TOut : IDetachedDocumentResult =>
        new(read: (_, content, op) => Optional(content as TContent).ToFin(Fail: op.InvalidInput()).Bind(typed => project(typed, op)));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RegistryCommand {
    private RegistryCommand() { }
    public sealed record RegisterContent(Assembly Assembly, Guid PlugInId) : RegistryCommand;
    public sealed record RegisterSerializer(ContentSerializer Serializer, Guid PlugInId) : RegistryCommand;
    public sealed record Change(DocumentSession Session, ContentTransaction Transaction) : RegistryCommand;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RegistryResult : IDetachedDocumentResult {
    private RegistryResult() { }
    public sealed record Registered(Seq<Type> Types) : RegistryResult;
    public sealed record SerializerRegistered : RegistryResult;
    public sealed record Changed(ContentReceipt Receipt) : RegistryResult;
}

public sealed record ContentTypeCensus(
    Seq<ContentUuidSeed> BuiltInUuids,
    Seq<ContentTypeInfo> RegisteredFactories) : IDetachedDocumentResult;

public sealed class RegistryQuery<T> where T : IDetachedDocumentResult {
    private readonly Func<Op, Fin<T>> run;

    internal RegistryQuery(Func<Op, Fin<T>> run) => this.run = run;

    internal Fin<T> Run(Op op) => run(op);
}

public static class RegistryQuery {
    public static RegistryQuery<ContentTypeCensus> Factories { get; } =
        new(op =>
            from builtIn in ContentUuidCatalog.Census(op)
            from registered in ContentTypeInfo.Census(op)
            select new ContentTypeCensus(BuiltInUuids: builtIn, RegisteredFactories: registered));

    public static RegistryQuery<T> Content<T>(DocumentSession session, ContentRef target, ContentQuery<T> query)
        where T : IDetachedDocumentResult =>
        new(op => Registry.Query(session, target, query, op));

    public static RegistryQuery<ContentRoster> Roster(DocumentSession session, ContentKind kind) =>
        new(op => Registry.Roster(session, kind, op));

    public static RegistryQuery<EnvironmentBindings> CurrentEnvironments(DocumentSession session) =>
        new(op => Registry.CurrentEnvironments(session, op));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Registry {
    public static Fin<RegistryResult> Run(RegistryCommand command) {
        Op op = Op.Of();
        return from active in Optional(command).ToFin(Fail: op.InvalidInput())
               from result in active.Switch(
                   context: op,
                   registerContent: static (state, request) => Register(request.Assembly, request.PlugInId, state)
                       .Map(static types => (RegistryResult)new RegistryResult.Registered(types)),
                   registerSerializer: static (state, request) =>
                       from serializer in Optional(request.Serializer).ToFin(Fail: state.InvalidInput())
                       from _ in guard(request.PlugInId != Guid.Empty, state.InvalidInput())
                       from registered in serializer.Register(request.PlugInId)
                       select (RegistryResult)new RegistryResult.SerializerRegistered(),
                   change: static (state, request) => Commit(request.Session, request.Transaction, state)
                       .Map(static receipt => (RegistryResult)new RegistryResult.Changed(receipt)))
               select result;
    }

    public static Fin<T> Read<T>(RegistryQuery<T> query) where T : IDetachedDocumentResult {
        Op op = Op.Of();
        return from active in Optional(query).ToFin(Fail: op.InvalidInput())
               from result in active.Run(op)
               select result;
    }

    private static Fin<Seq<Type>> Register(Assembly assembly, Guid pluginId, Op op) {
        return from active in Optional(assembly).ToFin(Fail: op.InvalidInput())
               from _ in guard(pluginId != Guid.Empty, op.InvalidInput())
               from registered in op.Catch(() => Optional(RenderContent.RegisterContent(assembly: active, pluginId: pluginId))
            .ToFin(Fail: op.InvalidResult())
            .Map(static types => toSeq(types)))
               select registered;
    }

    private static Fin<ContentReceipt> Commit(DocumentSession session, ContentTransaction plan, Op op) {
        return from activeSession in Optional(session).ToFin(Fail: op.InvalidInput())
               from active in Optional(plan).ToFin(Fail: op.InvalidInput())
               from kind in Optional(active.Kind).ToFin(Fail: op.InvalidInput())
               from reason in Optional(active.Reason).ToFin(Fail: op.InvalidInput())
               from redraw in Optional(active.Redraw).ToFin(Fail: op.InvalidInput())
               from undo in Optional(active.Undo).ToFin(Fail: op.InvalidInput())
               from name in op.AcceptText(value: active.Name)
               from _ in guard(
                   !active.Operations.IsEmpty && active.Operations.ForAll(static operation => operation is not null),
                   op.InvalidInput())
               let admitted = active with { Kind = kind, Reason = reason, Redraw = redraw, Undo = undo }
               let recording = admitted.Undo.Enabled && admitted.Operations.Exists(static operation => operation.RecordsUndo)
               from receipt in activeSession.Demand(
                   use: document => Change(document: document, plan: admitted, name: name, recording: recording, op: op),
                   key: op,
                   needs: SessionNeed.Mutation(undo: recording, redraw: admitted.Redraw).ToArray())
               select receipt;
    }

    private static Fin<ContentReceipt> Change(RhinoDoc document, ContentTransaction plan, string name, bool recording, Op op) =>
        DocumentCommit.Sealed(
            document: document,
            name: name,
            recordsUndo: recording,
            redraw: plan.Redraw,
            run: () => TableScoped(
                kind: plan.Kind,
                document: document,
                reason: plan.Reason,
                body: () => plan.Operations.TraverseM(operation => operation.Apply(
                        document: document, scope: plan.Kind, reason: plan.Reason, op: op)).As()
                    .Map(static receipts => receipts.Fold(ContentReceipt.Empty, static (state, value) => state + value)),
                op: op),
            stamp: static (receipt, serial) => serial > 0u ? receipt + ContentReceipt.UndoRecords(serials: Seq(serial)) : receipt,
            op: op);

    private static Fin<T> TableScoped<T>(ContentKind kind, RhinoDoc document, ChangeReason reason, Func<Fin<T>> body, Op op) {
        Fin<Unit> Close() => op.Catch(() => {
            kind.Close(document: document);
            return Fin.Succ(value: unit);
        });

        Fin<T> outcome = op.Catch(() => {
            kind.Open(document: document, reason: reason.Native);
            return Fin.Succ(value: unit);
        }).Bind(_ => op.Catch(body));

        return outcome.Match(
            Succ: value => Close().Map(_ => value),
            Fail: primary => Close().Match(
                Succ: _ => Fin.Fail<T>(error: primary),
                Fail: restoration => Fin.Fail<T>(error: primary + restoration)));
    }

    internal static Fin<T> Query<T>(DocumentSession session, ContentRef target, ContentQuery<T> query, Op op) {
        return from activeSession in Optional(session).ToFin(Fail: op.InvalidInput())
               from activeTarget in Optional(target).ToFin(Fail: op.InvalidInput())
               from active in Optional(query).ToFin(Fail: op.InvalidInput())
               from result in activeSession.Demand(
                   use: document =>
                       from content in activeTarget.Resolve(document: document, key: op)
                       from answer in active.Run(document: document, content: content, op: op)
                       select answer,
                   key: op,
                   needs: [SessionNeed.Read])
               select result;
    }

    internal static Fin<ContentRoster> Roster(DocumentSession session, ContentKind kind, Op op) {
        return from activeSession in Optional(session).ToFin(Fail: op.InvalidInput())
               from activeKind in Optional(kind).ToFin(Fail: op.InvalidInput())
               from result in activeSession.Demand(
                   use: document => op.Catch(() => Fin.Succ(value: new ContentRoster(
                       Kind: activeKind,
                       Ids: activeKind.Roster(document).Map(static content => content.Id)))),
                   key: op,
                   needs: [SessionNeed.Read])
               select result;
    }

    internal static Fin<EnvironmentBindings> CurrentEnvironments(DocumentSession session, Op op) {
        return from activeSession in Optional(session).ToFin(Fail: op.InvalidInput())
               from result in activeSession.Demand(
                   use: document => op.Catch(() => {
                       ICurrentEnvironment current = document.CurrentEnvironment;
                       return Fin.Succ(value: new EnvironmentBindings(
                           Background: Optional(current.ForBackground).Map(static content => content.Id),
                           Reflection: Optional(current.ForReflectionAndRefraction).Map(static content => content.Id),
                           Lighting: Optional(current.ForLighting).Map(static content => content.Id)));
                   }),
                   key: op,
                   needs: [SessionNeed.Read])
               select result;
    }
}
```

## [05]-[RECEIPTS]

- Owner: `ContentSlot` `[SmartEnum<int>]` — the consequence vocabulary; `ContentBody` — the payload union; `ContentReceipt` — the additive fold over the fact stream with slot-keyed projections, the same fact-stream form the document table and block rails carry.
- Law: one fact stream, kind-discriminated — content ids, assigned object ids, export paths, and undo serials are `ContentBody` cases on one record; every projection is a `Choose` over the stream, and a new consequence class is one slot row or one body case; the mapping page's channel bind stamps its `Mapped` facts onto this same receipt.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ContentSlot {
    public static readonly ContentSlot Minted = new(key: 0);
    public static readonly ContentSlot Adopted = new(key: 1);
    public static readonly ContentSlot Detached = new(key: 2);
    public static readonly ContentSlot Renamed = new(key: 3);
    public static readonly ContentSlot Grafted = new(key: 4);
    public static readonly ContentSlot Pruned = new(key: 5);
    public static readonly ContentSlot SlotSet = new(key: 6);
    public static readonly ContentSlot FieldSet = new(key: 7);
    public static readonly ContentSlot Configured = new(key: 8);
    public static readonly ContentSlot Assigned = new(key: 9);
    public static readonly ContentSlot Swapped = new(key: 10);
    public static readonly ContentSlot Grouped = new(key: 11);
    public static readonly ContentSlot Ungrouped = new(key: 12);
    public static readonly ContentSlot Exported = new(key: 13);
    public static readonly ContentSlot Undo = new(key: 14);
    public static readonly ContentSlot Mapped = new(key: 15);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentBody {
    private ContentBody() { }
    public sealed record Content(Guid Id) : ContentBody;
    public sealed record Object(Guid Id) : ContentBody;
    public sealed record Path(string Value) : ContentBody;
    public sealed record Record(uint Serial) : ContentBody;
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct ContentFactRow(ContentSlot Slot, ContentBody Body);

public readonly record struct ContentReceipt : IDetachedDocumentResult {
    private readonly Seq<ContentFactRow> facts;

    private ContentReceipt(Seq<ContentFactRow> facts) => this.facts = facts;

    public static ContentReceipt Empty { get; } = new(facts: Seq<ContentFactRow>());

    public Seq<ContentFactRow> Facts => facts;

    public static ContentReceipt operator +(ContentReceipt left, ContentReceipt right) =>
        new(facts: left.facts + right.facts);

    public static ContentReceipt Content(ContentSlot slot, Guid id) =>
        new(facts: Seq(new ContentFactRow(Slot: slot, Body: new ContentBody.Content(Id: id))));

    public static ContentReceipt Objects(ContentSlot slot, Seq<Guid> ids) =>
        new(facts: ids.Distinct().Filter(static id => id != Guid.Empty)
            .Map(id => new ContentFactRow(Slot: slot, Body: new ContentBody.Object(Id: id))));

    public static ContentReceipt Path(ContentSlot slot, string path) =>
        new(facts: Seq(new ContentFactRow(Slot: slot, Body: new ContentBody.Path(Value: path))));

    public static ContentReceipt UndoRecords(Seq<uint> serials) =>
        new(facts: serials.Filter(static serial => serial > 0u)
            .Map(serial => new ContentFactRow(Slot: ContentSlot.Undo, Body: new ContentBody.Record(Serial: serial))));

    public Seq<Guid> Contents(ContentSlot slot) =>
        facts.Filter(fact => fact.Slot == slot)
            .Choose(static fact => fact.Body is ContentBody.Content body ? Some(body.Id) : Option<Guid>.None);

    public Seq<Guid> Ids(ContentSlot slot) =>
        facts.Filter(fact => fact.Slot == slot)
            .Choose(static fact => fact.Body is ContentBody.Object body ? Some(body.Id) : Option<Guid>.None);

    public int FactCount(ContentSlot slot) =>
        facts.Count(fact => fact.Slot == slot);
}
```

## [06]-[EVENTS]

- Owner: `ContentPulse` carries each catalogued static event as one bind row; `ContentSignal` closes detached payloads; `ContentStream` owns transactional attach, document gating, symmetric release, and a `RetentionPolicy`-bounded `ContentStreamFailure` ledger surfacing typed `RetentionOverflow` evidence.
- Law: every reference-like host member projects inside the callback — content becomes its guid, the document becomes `DocKey`, the preview bitmap clones into an owned lease; a live `RenderContent` never rides a fact.
- Law: the stream and the table family split by granularity — the Document events page's `RenderContent` payload reports table transitions and material assignment; this stream reports per-content lifecycle, change context, and field mutation the table family cannot; a consumer needing both composes two watches.
- Law: reason filtering occurs at the bind — `PulseFilter` drops changed and field facts whose reason the filter names; filtering never claims debounce or coalescing semantics the host event stream does not provide.
- Law: callback delivery transfers the original fact to the sink and prepares a detached ledger copy first. Success releases the spare copy; failure retains it with the fault and releases the delivered original before the host delegate returns.
- Law: the failure ledger is capacity-bounded by the injected `RetentionPolicy`; an overflow evicts the oldest retained failures, releases each evicted fact on the owner's existing `Release` rail, and folds its fault into typed `RetentionOverflow` evidence, so a full ledger sheds resources without a silent drop and its `Overflow` count-and-fault read survives the eviction.
- Law: one locked lifecycle cell admits delivery, counts in-flight callbacks, mutates failure retention, and publishes close; detachment, sinks, and release execute outside it. Close blocks new sink entry, drains every admitted callback before capturing the subscription and ledger, and publishes completion only after release; reentrant close delegates capture to the final callback instead of waiting on itself.
- Law: `ContentHooks.Mount` registers the `rasm.rhino.render.content` point on the `HookRegistry` row grammar — ask `ContentObservation` carrying scope, pulses, filter, retention, and sink; grant `ContentStream` — so each binder mints its own stream and the point stays observe-only per the registry census.
- Growth: a new host content event is one `ContentPulse` row with its bind column; a new evidence axis is one `ContentSignal` case.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class LifecycleReason {
    public static readonly LifecycleReason None = new("none", RenderContentChangeReason.None);
    public static readonly LifecycleReason Attach = new("attach", RenderContentChangeReason.Attach);
    public static readonly LifecycleReason Detach = new("detach", RenderContentChangeReason.Detach);
    public static readonly LifecycleReason ChangeAttach = new("change-attach", RenderContentChangeReason.ChangeAttach);
    public static readonly LifecycleReason ChangeDetach = new("change-detach", RenderContentChangeReason.ChangeDetach);
    public static readonly LifecycleReason AttachUndo = new("attach-undo", RenderContentChangeReason.AttachUndo);
    public static readonly LifecycleReason DetachUndo = new("detach-undo", RenderContentChangeReason.DetachUndo);
    public static readonly LifecycleReason Open = new("open", RenderContentChangeReason.Open);
    public static readonly LifecycleReason Delete = new("delete", RenderContentChangeReason.Delete);

    internal RenderContentChangeReason Native { get; }

    internal static Fin<LifecycleReason> Of(RenderContentChangeReason native, Op key) =>
        key.Row(Items, native, static item => item.Native);
}

[SmartEnum<string>]
public sealed partial class PreviewQuality {
    public static readonly PreviewQuality None = new("none", Rhino.Render.Utilities.PreviewQuality.None);
    public static readonly PreviewQuality Low = new("low", Rhino.Render.Utilities.PreviewQuality.Low);
    public static readonly PreviewQuality Medium = new("medium", Rhino.Render.Utilities.PreviewQuality.Medium);
    public static readonly PreviewQuality Progressive = new(
        "progressive", Rhino.Render.Utilities.PreviewQuality.IntermediateProgressive);
    public static readonly PreviewQuality Full = new("full", Rhino.Render.Utilities.PreviewQuality.Full);

    internal Rhino.Render.Utilities.PreviewQuality Native { get; }

    internal static Fin<PreviewQuality> Of(Rhino.Render.Utilities.PreviewQuality native, Op key) =>
        key.Row(Items, native, static item => item.Native);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContentSignal : IDisposable {
    private ContentSignal() { }
    public sealed record Lifecycle(Guid Content, LifecycleReason Reason) : ContentSignal;
    public sealed record Changed(Guid Content, ChangeReason Reason, Option<Guid> Old) : ContentSignal;
    public sealed record FieldChanged(Guid Content, string Field, ChangeReason Reason) : ContentSignal;
    public sealed record EnvironmentFlip(EnvironmentRole Usage) : ContentSignal;
    public sealed record PreviewReady(
        Lease<System.Drawing.Bitmap> Image,
        Option<(int Width, int Height)> Signature,
        PreviewQuality Quality) : ContentSignal;

    public void Dispose() => Switch(
        lifecycle: static _ => unit,
        changed: static _ => unit,
        fieldChanged: static _ => unit,
        environmentFlip: static _ => unit,
        previewReady: static signal => Optional(signal.Image).Map(static image => image.Dispose()).IfNone(unit));

    internal Fin<ContentSignal> Detached(Op key) => Switch(
        context: key,
        lifecycle: static (_, signal) => Fin.Succ<ContentSignal>(new Lifecycle(
            Content: signal.Content,
            Reason: signal.Reason)),
        changed: static (_, signal) => Fin.Succ<ContentSignal>(new Changed(
            Content: signal.Content,
            Reason: signal.Reason,
            Old: signal.Old)),
        fieldChanged: static (_, signal) => Fin.Succ<ContentSignal>(new FieldChanged(
            Content: signal.Content,
            Field: signal.Field,
            Reason: signal.Reason)),
        environmentFlip: static (_, signal) => Fin.Succ<ContentSignal>(new EnvironmentFlip(Usage: signal.Usage)),
        previewReady: static (op, signal) => op.Catch(() =>
            Optional(signal.Image).ToFin(Fail: op.InvalidResult()).Map(image =>
                (ContentSignal)new PreviewReady(
                    Image: new Lease<System.Drawing.Bitmap>.Owned(
                        Value: (System.Drawing.Bitmap)image.Resource.Clone()),
                    Signature: signal.Signature,
                    Quality: signal.Quality))));
}

public readonly record struct ContentFact(ContentPulse Pulse, Option<DocKey> Key, ContentSignal Signal)
    : IDisposable, IDetachedDocumentResult {
    public void Dispose() => Optional(Signal).Iter(static signal => signal.Dispose());

    internal Fin<ContentFact> Detached(Op key) =>
        Optional(Signal).ToFin(Fail: key.InvalidResult())
            .Bind(signal => signal.Detached(key))
            .Map(signal => this with { Signal = signal });
}

public sealed record PulseFilter(Seq<ChangeReason> DroppedReasons) {
    public static readonly PulseFilter None = new(DroppedReasons: Seq<ChangeReason>());
    public static readonly PulseFilter WithoutRealTimeUi = new(DroppedReasons: Seq(ChangeReason.RealTimeUi));

    internal bool Admits(Option<ChangeReason> reason) =>
        reason.Map(row => !DroppedReasons.Contains(row)).IfNone(true);
}

[SmartEnum<int>]
public sealed partial class ContentPulse {
    public static readonly ContentPulse Added = new(key: 0, bind: Plain(
        subscribe: static h => RenderContent.ContentAdded += h, unsubscribe: static h => RenderContent.ContentAdded -= h));
    public static readonly ContentPulse Renamed = new(key: 1, bind: Plain(
        subscribe: static h => RenderContent.ContentRenamed += h, unsubscribe: static h => RenderContent.ContentRenamed -= h));
    public static readonly ContentPulse Deleting = new(key: 2, bind: Plain(
        subscribe: static h => RenderContent.ContentDeleting += h, unsubscribe: static h => RenderContent.ContentDeleting -= h));
    public static readonly ContentPulse Deleted = new(key: 3, bind: Plain(
        subscribe: static h => RenderContent.ContentDeleted += h, unsubscribe: static h => RenderContent.ContentDeleted -= h));
    public static readonly ContentPulse Replacing = new(key: 4, bind: Plain(
        subscribe: static h => RenderContent.ContentReplacing += h, unsubscribe: static h => RenderContent.ContentReplacing -= h));
    public static readonly ContentPulse Replaced = new(key: 5, bind: Plain(
        subscribe: static h => RenderContent.ContentReplaced += h, unsubscribe: static h => RenderContent.ContentReplaced -= h));
    public static readonly ContentPulse UpdatePreview = new(key: 6, bind: Plain(
        subscribe: static h => RenderContent.ContentUpdatePreview += h, unsubscribe: static h => RenderContent.ContentUpdatePreview -= h));
    public static readonly ContentPulse EnvironmentFlip = new(key: 7, bind: (pulse, scope, filter, deliver) =>
        Subscription.Attach<EventHandler<RenderContentEventArgs>>(
            subscribe: static h => RenderContent.CurrentEnvironmentChanged += h,
            unsubscribe: static h => RenderContent.CurrentEnvironmentChanged -= h,
            handler: (_, args) => ignore(
                EnvironmentRole.Of(args.EnvironmentUsageEx, Op.Of(name: nameof(ContentPulse))).ToOption()
                    .Bind(role => Gate(pulse: pulse, scope: scope, document: args.Document,
                        signal: new ContentSignal.EnvironmentFlip(Usage: role)))
                    .Match(Some: deliver, None: static () => Fin.Succ(value: unit)))));
    public static readonly ContentPulse Changed = new(key: 8, bind: (pulse, scope, filter, deliver) =>
        Subscription.Attach<EventHandler<RenderContentChangedEventArgs>>(
            subscribe: static h => RenderContent.ContentChanged += h,
            unsubscribe: static h => RenderContent.ContentChanged -= h,
            handler: (_, args) => ignore(
                ChangeReason.Of(native: args.ChangeContext, key: Op.Of(name: nameof(ContentPulse))).ToOption()
                    .Filter(reason => filter.Admits(Some(reason)))
                    .Bind(reason => Gate(pulse: pulse, scope: scope, document: args.Document,
                        signal: new ContentSignal.Changed(
                            Content: args.Content.Id, Reason: reason,
                            Old: Optional(args.OldContent).Map(static old => old.Id))))
                    .Match(Some: deliver, None: static () => Fin.Succ(value: unit)))));
    public static readonly ContentPulse FieldChanged = new(key: 9, bind: (pulse, scope, filter, deliver) =>
        Subscription.Attach<EventHandler<RenderContentFieldChangedEventArgs>>(
            subscribe: static h => RenderContent.ContentFieldChanged += h,
            unsubscribe: static h => RenderContent.ContentFieldChanged -= h,
            handler: (_, args) => ignore(
                ChangeReason.Of(native: args.ChangeContext, key: Op.Of(name: nameof(ContentPulse))).ToOption()
                    .Filter(reason => filter.Admits(Some(reason)))
                    .Bind(reason => Gate(pulse: pulse, scope: scope, document: args.Document,
                        signal: new ContentSignal.FieldChanged(Content: args.Content.Id, Field: args.FieldName, Reason: reason)))
                    .Match(Some: deliver, None: static () => Fin.Succ(value: unit)))));
    public static readonly ContentPulse PreviewReady = new(key: 10, bind: (pulse, scope, filter, deliver) =>
        Subscription.Attach<EventHandler<PreviewRenderedEventArgs>>(
            subscribe: static h => RenderContent.PreviewRendered += h,
            unsubscribe: static h => RenderContent.PreviewRendered -= h,
            handler: (_, args) => ignore(scope is EventScope.AnyDocument
                ? (from image in Optional(args.Bitmap)
                   from quality in PreviewQuality.Of(args.Quality, Op.Of(name: nameof(ContentPulse))).ToOption()
                   select new ContentFact(
                        Pulse: pulse,
                        Key: Option<DocKey>.None,
                        Signal: new ContentSignal.PreviewReady(
                            Image: new Lease<System.Drawing.Bitmap>.Owned(Value: (System.Drawing.Bitmap)image.Clone()),
                            Signature: Optional(args.PreviewJobSignature)
                                .Map(static signature => (signature.ImageWidth(), signature.ImageHeight())),
                            Quality: quality)))
                    .Match(Some: deliver, None: static () => Fin.Succ(value: unit))
                : Fin.Succ(value: unit))));

    [UseDelegateFromConstructor]
    internal partial Fin<Subscription> Bind(
        ContentPulse pulse, EventScope scope, PulseFilter filter, Func<ContentFact, Fin<Unit>> deliver);

    private static Func<ContentPulse, EventScope, PulseFilter, Func<ContentFact, Fin<Unit>>, Fin<Subscription>> Plain(
        Action<EventHandler<RenderContentEventArgs>> subscribe,
        Action<EventHandler<RenderContentEventArgs>> unsubscribe) =>
        (pulse, scope, _, deliver) => Subscription.Attach(
            subscribe: subscribe,
            unsubscribe: unsubscribe,
            handler: (EventHandler<RenderContentEventArgs>)((_, args) => ignore(
                LifecycleReason.Of(args.Reason, Op.Of(name: nameof(ContentPulse))).ToOption()
                    .Bind(reason => Gate(pulse: pulse, scope: scope, document: args.Document,
                        signal: new ContentSignal.Lifecycle(Content: args.Content.Id, Reason: reason)))
                .Match(Some: deliver, None: static () => Fin.Succ(value: unit)))));

    private static Option<ContentFact> Gate(ContentPulse pulse, EventScope scope, RhinoDoc? document, ContentSignal signal) =>
        Optional(document)
            .Bind(static active => DocKey.Of(document: active, key: Op.Of(name: nameof(ContentPulse))).ToOption())
            .Match(
                Some: key => scope.Switch(
                    (Key: key, Pulse: pulse, Signal: signal),
                    document: static (state, watched) => watched.Key == state.Key
                        ? Some(new ContentFact(Pulse: state.Pulse, Key: Some(state.Key), Signal: state.Signal))
                        : Option<ContentFact>.None,
                    anyDocument: static (state, _) => Some(new ContentFact(Pulse: state.Pulse, Key: Some(state.Key), Signal: state.Signal))),
                None: () => scope is EventScope.AnyDocument
                    ? Some(new ContentFact(Pulse: pulse, Key: Option<DocKey>.None, Signal: signal))
                    : Option<ContentFact>.None);
}

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed record ContentStreamFailure(ContentFact Fact, Error Fault) : IDisposable, IDetachedDocumentResult {
    public void Dispose() => Fact.Dispose();
}

public sealed record ContentObservation(
    EventScope Scope,
    Seq<ContentPulse> Pulses,
    PulseFilter Filter,
    RetentionPolicy Retention,
    Func<ContentFact, Fin<Unit>> Sink);

public static class ContentHooks {
    public static Fin<IDisposable> Mount(PluginKey plugin, Op? key = null) =>
        HookRegistry.Mount(
            mount: new HookMount(
                Point: HookPoint.RenderContent,
                Plugin: plugin,
                Ask: typeof(ContentObservation),
                Grant: typeof(ContentStream),
                Bind: static ask => ask switch {
                    ContentObservation request => ContentStream.Of(
                            scope: request.Scope,
                            pulses: request.Pulses,
                            filter: request.Filter,
                            retention: request.Retention,
                            sink: request.Sink)
                        .Map(static stream => (object)stream),
                    _ => Fin.Fail<object>(error: Op.Of(name: nameof(ContentHooks)).InvalidInput()),
                }),
            key: key.OrDefault());
}

public sealed class ContentStream : IDisposable {
    private readonly ContentStreamState state;

    private ContentStream(ContentStreamState state) => this.state = state;

    public Seq<ContentStreamFailure> Failures => state.Failures;
    public RetentionOverflow Overflow => state.Overflow;

    public void Dispose() => state.Dispose();

    public static Fin<ContentStream> Of(
        EventScope scope, Seq<ContentPulse> pulses, PulseFilter filter, RetentionPolicy retention,
        Func<ContentFact, Fin<Unit>> sink) {
        Op op = Op.Of(name: nameof(ContentStream));
        return from activeScope in Optional(scope).ToFin(Fail: op.InvalidInput())
               from activeFilter in Optional(filter).ToFin(Fail: op.InvalidInput())
               from activeRetention in Optional(retention).ToFin(Fail: op.InvalidInput())
               from activeSink in Optional(sink).ToFin(Fail: op.InvalidInput())
               from _ in guard(!pulses.IsEmpty && pulses.ForAll(static pulse => pulse is not null), op.InvalidInput())
               let state = new ContentStreamState(retention: activeRetention)
               from attached in Subscription.AttachAll(pulses.Distinct().Map(pulse =>
                   (Func<Fin<Subscription>>)(() => pulse.Bind(
                       pulse: pulse,
                       scope: activeScope,
                       filter: activeFilter,
                       deliver: fact => state.Deliver(fact, activeSink, op)))))
               from _attached in state.Attach(subscription: attached, op: op)
               select new ContentStream(state: state);
    }

    private sealed class ContentStreamState : IDisposable {
        private readonly object gate = new();
        private readonly RetentionPolicy retention;
        private readonly System.Collections.Generic.Dictionary<int, int> callers = [];
        private Subscription? subscription;
        private FailureLedger<ContentStreamFailure> ledger = FailureLedger<ContentStreamFailure>.Empty;
        private bool accepting = true;
        private bool closing;
        private bool closed;
        private int inFlight;

        internal ContentStreamState(RetentionPolicy retention) => this.retention = retention;

        internal Seq<ContentStreamFailure> Failures {
            get {
                lock (gate) {
                    return ledger.Retained;
                }
            }
        }

        internal RetentionOverflow Overflow {
            get {
                lock (gate) {
                    return ledger.Overflow;
                }
            }
        }

        internal Fin<Unit> Attach(Subscription subscription, Op op) {
            lock (gate) {
                if (accepting && !closed) {
                    this.subscription = subscription;
                    return Fin.Succ(value: unit);
                }
            }
            return op.Catch(() => {
                subscription.Dispose();
                return Fin.Fail<Unit>(op.InvalidContext());
            });
        }

        internal Fin<Unit> Deliver(ContentFact fact, Func<ContentFact, Fin<Unit>> sink, Op op) {
            int caller = Environment.CurrentManagedThreadId;
            lock (gate) {
                if (!accepting) {
                    return Settled(
                        primary: op.InvalidContext(),
                        releases: Seq<Func<Fin<Unit>>>(() => Release(fact: fact, op: op)));
                }
                inFlight++;
                callers.TryGetValue(key: caller, value: out int depth);
                callers[caller] = depth + 1;
            }
            try {
                return Delivered(fact: fact, sink: sink, op: op);
            }
            finally {
                Option<(Subscription? Subscription, Seq<ContentStreamFailure> Failures)> closure = None;
                lock (gate) {
                    inFlight--;
                    int depth = callers[caller] - 1;
                    if (depth == 0) {
                        callers.Remove(key: caller);
                    }
                    else {
                        callers[caller] = depth;
                    }
                    if (!accepting && inFlight == 0 && !closing && !closed) {
                        closure = Some(BeginClose());
                    }
                    Monitor.PulseAll(gate);
                }
                closure.Iter(FinishClose);
            }
        }

        private Fin<Unit> Delivered(ContentFact fact, Func<ContentFact, Fin<Unit>> sink, Op op) =>
            fact.Detached(op).Match(
                Succ: detached => op.Catch(() => sink(fact)).Match(
                    Succ: value => Accepted(detached: detached, value: value, op: op),
                    Fail: fault => Retained(original: fact, detached: detached, fault: fault, op: op)),
                Fail: fault => Rejected(original: fact, fault: fault, op: op));

        private static Fin<Unit> Accepted(ContentFact detached, Unit value, Op op) =>
            Release(fact: detached, op: op).Map(_ => value);

        private Fin<Unit> Retained(ContentFact original, ContentFact detached, Error fault, Op op) =>
            op.Catch(() => {
                lock (gate) {
                    if (closing || closed) {
                        return Fin.Succ(value: (Open: false, Evicted: Seq<ContentStreamFailure>()));
                    }
                    (FailureLedger<ContentStreamFailure> next, Seq<ContentStreamFailure> evicted) = ledger.Admit(
                        policy: retention,
                        incoming: new ContentStreamFailure(Fact: detached, Fault: fault),
                        fault: static failure => failure.Fault);
                    ledger = next;
                    return Fin.Succ(value: (Open: true, Evicted: evicted));
                }
            }).Match(
                Succ: state => state.Open
                    ? Settled(
                        primary: fault,
                        releases: Seq<Func<Fin<Unit>>>(() => Release(fact: original, op: op))
                            + state.Evicted.Map(dropped =>
                                (Func<Fin<Unit>>)(() => Release(fact: dropped.Fact, op: op))))
                    : Settled(
                        primary: fault + op.InvalidContext(),
                        releases: Seq<Func<Fin<Unit>>>(
                            () => Release(fact: original, op: op),
                            () => Release(fact: detached, op: op))),
                Fail: custody => Settled(
                    primary: fault + custody,
                    releases: Seq<Func<Fin<Unit>>>(
                        () => Release(fact: original, op: op),
                        () => Release(fact: detached, op: op))));

        private static Fin<Unit> Rejected(ContentFact original, Error fault, Op op) =>
            Settled(
                primary: fault,
                releases: Seq<Func<Fin<Unit>>>(() => Release(fact: original, op: op)));

        private static Fin<Unit> Settled(Error primary, Seq<Func<Fin<Unit>>> releases) =>
            Fin.Fail<Unit>(error: releases.Fold(
                primary,
                static (held, release) => release().Match(
                    Succ: _ => held,
                    Fail: fault => held + fault)));

        private static Fin<Unit> Release(ContentFact fact, Op op) => op.Catch(() => {
            fact.Dispose();
            return Fin.Succ(value: unit);
        });

        public void Dispose() {
            Option<(Subscription? Subscription, Seq<ContentStreamFailure> Failures)> closure = None;
            int caller = Environment.CurrentManagedThreadId;
            lock (gate) {
                if (closed) {
                    return;
                }
                accepting = false;
                if (callers.ContainsKey(key: caller)) {
                    return;
                }
                if (inFlight == 0 && !closing) {
                    closure = Some(BeginClose());
                }
                else {
                    while (!closed) {
                        Monitor.Wait(gate);
                    }
                }
            }
            closure.Iter(FinishClose);
        }

        private (Subscription? Subscription, Seq<ContentStreamFailure> Failures) BeginClose() {
            closing = true;
            Subscription? captured = subscription;
            Seq<ContentStreamFailure> failures = ledger.Retained;
            subscription = null;
            ledger = FailureLedger<ContentStreamFailure>.Empty;
            return (Subscription: captured, Failures: failures);
        }

        private void FinishClose((Subscription? Subscription, Seq<ContentStreamFailure> Failures) closure) {
            try {
                closure.Subscription?.Dispose();
                closure.Failures.Iter(static failure => failure.Dispose());
            }
            finally {
                lock (gate) {
                    closing = false;
                    closed = true;
                    Monitor.PulseAll(gate);
                }
            }
        }
    }
}
```

## [07]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]       | [OWNER]                           | [FORM]                            | [ENTRY]            |
| :-----: | :-------------- | :-------------------------------- | :-------------------------------- | :----------------- |
|  [01]   | UUID seeds      | `ContentUuidCatalog`              | generated kind and role data      | `Census` / `Find`  |
|  [02]   | factory census  | `ContentTypeCensus`               | UUIDs plus registered factories   | `Registry.Read`    |
|  [03]   | custom format   | `ContentSerializer`               | transfer, reports, failure ledger | `Registry.Run`     |
|  [04]   | mutation        | `ContentOp` / `ContentTransaction` | admission or target mutation      | `Registry.Run`     |
|  [05]   | typed reads     | `RegistryQuery<T>`                | result-correlated programs        | `Registry.Read<T>` |
|  [06]   | collection read | `ContentCollectionEvidence`       | leased set and kind evidence      | `Collection`        |
|  [07]   | receipts        | `ContentReceipt`                  | additive fact rows                | `RegistryResult`   |
|  [08]   | content events  | `ContentPulse`                    | verified event rows               | `ContentStream.Of` |
|  [09]   | event evidence  | `ContentSignal`                   | payload and failure ledger        | `ContentStream.Of` |
|  [10]   | hook point      | `ContentHooks`                    | `rasm.rhino.render.content` mount | `ContentHooks.Mount` |
|  [11]   | failure ledger  | `RetentionPolicy`                 | bounded ledger, overflow evidence | `Of` / `Admit`     |
