# [PERSISTENCE_STORE_REDRIVE]

Rasm.Persistence bands every cross-process store boundary into one closed fault family and publishes the re-drive currency the composition root's executor crosses on. `RemoteStoreFault` closes the remote-object boundary over the kernel `Fault` floor, with one `Lift` fold mapping documented SDK refusals into direct cause-bearing provider leaves while preserving every unclassified `Error`, and one `Granted` fold admitting the credential-free grant plane's bare HTTP status. Retriability is the kernel `Retriability` discriminant a band OVERRIDES, never a package-local `bool` interface: returned transport derives its posture from typed status, provider transport from its typed status or connection exception, and every provider leaf retains the exact SDK cause without hiding identity behind a nested classifier. `RetryShape` is the SECOND axis — the re-offer ROUTE, naming where a recovery re-enters rather than whether one is offered — so frozen refusals route `Rescoped` to a thaw and `Aborted` routes `Restarted` to a resumed session while both stay terminal to a bare re-offer. This tier CLASSIFIES and executes nothing: the discriminant rides the fault, the policy rides the runtime, and `StoreRedrivePort` is the port whatever executor the root bound reaches through.

`Retriability`, `RedrivePolicy`, `Verdict`, and `Redrive.Run`/`Redrive.Settle` compose from the kernel `Rasm/Domain/results#REDRIVE`; `[FaultCase]`/`Fault` and `FaultBand` from the kernel fault-catalog floor and the `Element/graph#FAULT_TABLES` registry; `ContentAddress` from `Element/codec#CONTENT_ADDRESS`. `ObjectVerb` names the object plane's closed operation identity every leg slot and every lifted fault carries, and `RetryShape` composes from `Store/provisioning#ENGINE_OPERATIONS`, the ONE re-drive route vocabulary every Persistence fault family reads. `Store/blobstore#TRANSFER` `ObjectIo.Bound` is the single crossing that composes `Lift`, and `ObjectClient` carries the root-bound port beside its tenant.

## [01]-[INDEX]

- [02]-[FAULT_BAND]: `ObjectVerb` the closed operation identity, `RemoteStoreFault` the direct boundary union with its `Retriability` posture and `RetryShape` route, and the two admission folds — `Lift` over documented SDK refusals, `Granted` over the grant plane's bare HTTP status carrying its `Retry-After` window.
- [03]-[REDRIVE_BOUNDARY]: `StoreHop` the band-neutral hop identity closing every cross-process plane's verb vocabulary, `StoreVerdict` the four-arm attempt verdict folding the kernel `Redrive.Settle`, `StoreRedrivePort` the composition-root-bound executor port, and `LocalRedrive` the in-process row an unbound root degrades to.

## [02]-[FAULT_BAND]

- Owner: `ObjectVerb` the `[SmartEnum<string>]` operation identity every leg slot names and every lifted fault carries, its `ColdRefuses` column reading one provider code under two meanings; `[FaultCase]` the fault roster realizing the kernel `[FaultCase]` floor over the `RemoteStore` row; `RemoteStoreFault` the closed `[Union]` boundary band at 540x over `Fault`, owning its own `Message` projection while `Code` seal off the base, the `Retriability` posture it overrides, the `RetryShape` route it publishes, and the two admission folds `Lift` and `Granted`.
- Cases: `NotFound`, `Conflict` (the write-once `412` the placement treats as a benign no-op), cause-bearing `Aborted` (a torn ceremony whose staged parts survive), `Transport` (the ONE re-drivable case, carrying verb, key, status, provider code, and the server-stated `Retry-After` window where the transport publishes one), `IntegrityBreach`, `Locked`, `Denied`, `Oversize`, `GrantExpired`, `InvalidRange`, and `Frozen`.
- Law: retriability is the kernel `Retriability` a case OVERRIDES, so a case that never overrides is Terminal by construction and every case but `Transport` states nothing; a package-local `bool` discriminant is the deleted form — one axis short of the throttled case, and it collapses the re-offer ROUTE into the same bit.
- Law: `RetryShape` answers WHERE a recovery re-enters and `Retriability` answers WHETHER a bare re-offer is admitted, so the two never substitute: `Frozen` refuses a re-offer (re-fetching thaws nothing) yet routes `Rescoped` to `Rehydrate`, and `Aborted` refuses one yet routes `Restarted` to the durable session its staged parts survive under.
- Law: a server-stated delay outranks the curve. `Transport` carries `Retry-After` as an `Option<Duration>` and yields `Throttled` wherever the transport stated one, so the kernel `Redrive.Run` exits to `Settle` and re-draws nothing against a window the server already named.
- Entry: `Lift` folds documented SDK exception families structurally at the one boundary crossing — the `412` precondition to `Conflict`, `404` to `NotFound`, `401`/`403` to `Denied`, `413` to `Oversize`, a no-response connection failure to the transient `Transport` at status 0, every other provider status to a typed `Transport`, and every unclassified error unchanged; `Granted` is the grant plane's equivalent over a bare `HttpResponseMessage`.
- Boundary: `RemoteStoreFault` derives directly from kernel `Fault`; `[FaultCase]` generates its numeric identity and typed leaves publish retriability. SDK exceptions retain their exact `Error` on direct provider leaves; leaves lift bare onto `Fin<T>`/`IO<T>` with no parallel classification field or conversion hop.
- Packages: LanguageExt.Core (`Option`, `Fin`, `IO`), NodaTime (`Duration` the throttle window), Thinktecture.Runtime.Extensions (`[SmartEnum]`, `[Union]`), AWSSDK.S3 (`AmazonS3Exception` — `StatusCode` + `ErrorCode`), Azure.Storage.Blobs (`RequestFailedException` — `Status` + `ErrorCode`), Google.Cloud.Storage.V1 (`GoogleApiException` — `HttpStatusCode` + `Error.Code`), Minio (`Minio.Exceptions` the fourth family), BCL inbox (`HttpResponseMessage`, `HttpHeaders.TryGetValues`).
- Growth: a new boundary failure is one `RemoteStoreFault` case with its three projection rows; a new dialed operation is one `ObjectVerb` row every leg slot and every lifted fault then names; a new provider contributes its exception family to `Lift` alone; a per-leg exception catch, a second status fold, a band-local retriability interface, a per-case `bool`, or a re-drive executed at this tier is the deleted form.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Persistence.Element;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ObjectVerb {
    public static readonly ObjectVerb Write = new("write", coldRefuses: true);
    public static readonly ObjectVerb Read = new("read", coldRefuses: true);
    public static readonly ObjectVerb Erase = new("erase", coldRefuses: true);
    public static readonly ObjectVerb List = new("list", coldRefuses: true);
    public static readonly ObjectVerb Grant = new("grant", coldRefuses: true);
    public static readonly ObjectVerb Transition = new("transition", coldRefuses: true);
    public static readonly ObjectVerb Restore = new("restore", coldRefuses: false);
    public static readonly ObjectVerb Lifecycle = new("lifecycle", coldRefuses: true);
    public bool ColdRefuses { get; }
    private ObjectVerb(string key, bool coldRefuses) : this(key) => ColdRefuses = coldRefuses;
}

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RemoteStoreFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.RemoteStore;
    private RemoteStoreFault() { }

    [FaultCase(0)]
    public sealed partial record NotFound(ContentAddress Key) : RemoteStoreFault();
    [FaultCase(1)]
    public sealed partial record Conflict(ContentAddress Key, string Condition) : RemoteStoreFault();
    [FaultCase(2)]
    public sealed partial record Aborted(ContentAddress Key, int Parts, Error Cause) : RemoteStoreFault(), ICausedFault {
        public override RetryShape Route => RetryShape.Restarted;
    }
    [FaultCase(3)]
    public sealed partial record Transport(string Provider, ObjectVerb Verb, ContentAddress Key, int Status, string Code, Option<Duration> RetryAfter = default) : RemoteStoreFault() {
        public override Retriability Retriability => TransportPosture(Status, RetryAfter);
        public override RetryShape Route => Retriability is Retriability.TerminalCase ? RetryShape.Terminal : RetryShape.Waited;
    }
    [FaultCase(4)]
    public sealed partial record IntegrityBreach(ContentAddress Key, string Provider) : RemoteStoreFault();
    [FaultCase(5)]
    public sealed partial record Locked(ContentAddress Key, string Mode, Instant Until) : RemoteStoreFault();
    [FaultCase(6)]
    public sealed partial record Denied(ContentAddress Key, string Provider, string Code) : RemoteStoreFault();
    [FaultCase(7)]
    public sealed partial record Oversize(ContentAddress Key, string Provider, string Code) : RemoteStoreFault();
    [FaultCase(8)]
    public sealed partial record GrantExpired(ContentAddress Key) : RemoteStoreFault();
    [FaultCase(9)]
    public sealed partial record InvalidRange(ContentAddress Key, long Start, long End, long Length) : RemoteStoreFault();
    [FaultCase(10)]
    public sealed partial record Frozen(ContentAddress Key, string Provider, ObjectVerb Verb) : RemoteStoreFault() {
        public override RetryShape Route => RetryShape.Rescoped;
    }
    [FaultCase(11)] public sealed partial record ProviderNotFound(ContentAddress Key, Error Cause) : RemoteStoreFault(), ICausedFault;
    [FaultCase(12)] public sealed partial record ProviderConflict(ContentAddress Key, string Condition, Error Cause) : RemoteStoreFault(), ICausedFault;
    [FaultCase(13)]
    public sealed partial record ProviderTransport(string Provider, ObjectVerb Verb, ContentAddress Key, Option<int> Status, string Code, Error Cause) : RemoteStoreFault(), ICausedFault {
        public override Retriability Retriability => Status.Match(Some: status => TransportPosture(status, None), None: static () => Retriability.Transient);
        public override RetryShape Route => Retriability is Retriability.TerminalCase ? RetryShape.Terminal : RetryShape.Waited;
    }
    [FaultCase(14)] public sealed partial record ProviderDenied(ContentAddress Key, string Provider, string Code, Error Cause) : RemoteStoreFault(), ICausedFault;
    [FaultCase(15)] public sealed partial record ProviderOversize(ContentAddress Key, string Provider, string Code, Error Cause) : RemoteStoreFault(), ICausedFault;
    [FaultCase(16)]
    public sealed partial record ProviderFrozen(ContentAddress Key, string Provider, ObjectVerb Verb, Error Cause) : RemoteStoreFault(), ICausedFault {
        public override RetryShape Route => RetryShape.Rescoped;
    }

    public virtual RetryShape Route => RetryShape.Terminal;

    static Retriability TransportPosture(int status, Option<Duration> retryAfter) => status is 0 or 408 or 429 or >= 500
        ? retryAfter.Match(Some: Retriability.Throttled, None: static () => Retriability.Transient)
        : Retriability.Terminal;

    public override string Message => Switch(
        notFound:        static c => $"blob {c.Key.ToValue():x32} absent",
        conflict:        static c => $"blob {c.Key.ToValue():x32} {c.Condition}",
        aborted:         static c => $"blob {c.Key.ToValue():x32} aborted@{c.Parts}: {c.Cause.Message}",
        transport:       static c => $"{c.Provider} {c.Verb.Key} {c.Key.ToValue():x32} {c.Status}:{c.Code}",
        integrityBreach: static c => $"blob {c.Key.ToValue():x32} {c.Provider} checksum mismatch",
        locked:          static c => $"blob {c.Key.ToValue():x32} WORM {c.Mode}",
        denied:          static c => $"blob {c.Key.ToValue():x32} {c.Provider} denied: {c.Code}",
        oversize:        static c => $"blob {c.Key.ToValue():x32} {c.Provider} oversize: {c.Code}",
        grantExpired:    static c => $"blob {c.Key.ToValue():x32} grant expired",
        invalidRange:    static c => $"blob {c.Key.ToValue():x32} range {c.Start}-{c.End}/{c.Length}",
        frozen:            static c => $"blob {c.Key.ToValue():x32} {c.Provider} {c.Verb.Key} frozen",
        providerNotFound:  static c => $"blob {c.Key.ToValue():x32} absent: {c.Cause.Message}",
        providerConflict:  static c => $"blob {c.Key.ToValue():x32} {c.Condition}: {c.Cause.Message}",
        providerTransport: static c => $"{c.Provider} {c.Verb.Key} {c.Key.ToValue():x32} {c.Status.Map(static status => status.ToString(CultureInfo.InvariantCulture)).IfNone("<no-status>")}:{c.Code}: {c.Cause.Message}",
        providerDenied:    static c => $"blob {c.Key.ToValue():x32} {c.Provider} denied: {c.Code}: {c.Cause.Message}",
        providerOversize:  static c => $"blob {c.Key.ToValue():x32} {c.Provider} oversize: {c.Code}: {c.Cause.Message}",
        providerFrozen:    static c => $"blob {c.Key.ToValue():x32} {c.Provider} {c.Verb.Key} frozen: {c.Cause.Message}");

    // --- [ADMISSION]
    public static Error Lift(string provider, ObjectVerb verb, ContentAddress key, Error error) => error switch {
        RemoteStoreFault fault => fault,
        { Exception.Case: AmazonS3Exception { ErrorCode: "InvalidObjectState" } } => new ProviderFrozen(key, provider, verb, error),
        { Exception.Case: AmazonS3Exception s3 } => s3.StatusCode switch {
            HttpStatusCode.PreconditionFailed => new ProviderConflict(key, "if-none-match", error),
            HttpStatusCode.NotFound => new ProviderNotFound(key, error),
            HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized => new ProviderDenied(key, provider, s3.ErrorCode, error),
            HttpStatusCode.RequestEntityTooLarge => new ProviderOversize(key, provider, s3.ErrorCode, error),
            _ => new ProviderTransport(provider, verb, key, Some((int)s3.StatusCode), s3.ErrorCode, error),
        },
        { Exception.Case: RequestFailedException { ErrorCode: "BlobArchived" } } => new ProviderFrozen(key, provider, verb, error),
        { Exception.Case: RequestFailedException az } => az.Status switch {
            412 => new ProviderConflict(key, "if-none-match", error),
            404 => new ProviderNotFound(key, error),
            401 or 403 => new ProviderDenied(key, provider, az.ErrorCode ?? "azure", error),
            413 => new ProviderOversize(key, provider, az.ErrorCode ?? "azure", error),
            _ => new ProviderTransport(provider, verb, key, Some(az.Status), az.ErrorCode ?? "azure", error),
        },
        { Exception.Case: GoogleApiException gcs } => (int)gcs.HttpStatusCode switch {
            412 => new ProviderConflict(key, "if-generation-match", error),
            404 => new ProviderNotFound(key, error),
            401 or 403 => new ProviderDenied(key, provider, gcs.Error?.Code.ToString(CultureInfo.InvariantCulture) ?? "gcs", error),
            413 => new ProviderOversize(key, provider, gcs.Error?.Code.ToString(CultureInfo.InvariantCulture) ?? "gcs", error),
            int status => new ProviderTransport(provider, verb, key, Some(status), gcs.Error?.Code.ToString(CultureInfo.InvariantCulture) ?? "gcs", error),
        },
        { Exception.Case: PreconditionFailedException } => new ProviderConflict(key, "if-none-match", error),
        { Exception.Case: ObjectNotFoundException or BucketNotFoundException } => new ProviderNotFound(key, error),
        { Exception.Case: AccessDeniedException } => new ProviderDenied(key, provider, nameof(AccessDeniedException), error),
        { Exception.Case: ForbiddenException } => new ProviderDenied(key, provider, nameof(ForbiddenException), error),
        { Exception.Case: EntityTooLargeException } => new ProviderOversize(key, provider, nameof(EntityTooLargeException), error),
        { Exception.Case: InvalidContentLengthException } => new ProviderOversize(key, provider, nameof(InvalidContentLengthException), error),
        { Exception.Case: ConnectionException } => new ProviderTransport(provider, verb, key, None, "connection", error),
        _ => error,
    };

    public static RemoteStoreFault Granted(ObjectVerb verb, ContentAddress key, HttpResponseMessage response) => response.StatusCode switch {
        HttpStatusCode.Forbidden => new Denied(key, "presigned", "forbidden"),
        HttpStatusCode.NotFound => new NotFound(key),
        HttpStatusCode.PreconditionFailed => new Conflict(key, "if-none-match"),
        HttpStatusCode refused => new Transport("presigned", verb, key, (int)refused, refused.ToString(), Delay(response)),
    };

    static Option<Duration> Delay(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Retry-After", out IEnumerable<string>? stated)
            ? toSeq(stated).Head.Bind(static value => int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int seconds) && seconds >= 0
                ? Some(Duration.FromSeconds(seconds))
                : Option<Duration>.None)
            : None;
}
```

## [03]-[REDRIVE_BOUNDARY]

- Owner: `StoreHop` the band-neutral hop identity a re-drive names, `ColumnVerb` the wide-column plane's closed operation roster beside the object plane's `ObjectVerb`, `StoreVerdict` the neutral attempt verdict every cross-process band publishes, `StoreRedrivePort` the executor port the composition root binds, and `LocalRedrive` the in-process row realizing that port over the kernel executor.
- Cases: `StoreHop` is `Object(ObjectVerb)` and `WideColumn(ColumnVerb)` — one arm per cross-process PLANE, each wrapping that plane's own closed verb roster, so a pipeline row keys on an operation identity that names what it re-drives. `StoreVerdict` is `Delivered`, `Faulted(Reason, Attempt, After)` the scheduled re-offer carrying the delay the kernel settled, `Exhausted(Reason, Attempt)` the bound run dry, and `Refused(Reason)` the deterministic refusal — one arm per kernel `Verdict` case beside the delivered one, so the fold loses nothing the kernel decided.
- Law: `StoreVerdict.Of` is the ONE fold where classification meets policy, and it is band-NEUTRAL: retriability rides the kernel `Fault.Retriability` virtual the band overrides, so the fold reads `Redrive.Settle` and never a per-band member; a band-local verdict fold, or a `bool` handed over in place of the fault, is the deleted form — an executor's refusal arm must carry the typed error its caller matches on.
- Law: this package references `{Rasm, Rasm.Element}` alone and can name no pipeline type, so the port crosses on this package's own currency and the root resolves `(hop, instance)` to its own pipeline row. `Carry` is a generic METHOD rather than a delegate column, because an attempt's value crosses per pass and C# forbids a generic field.
- Law: the object plane crosses a PROCESS BOUNDARY, so `docs/stacks/csharp/domain/resilience.md` `[04]-[LAYER_SPLIT]` seats the executor at the composition root's hop pipeline; the pipeline is admissible precisely because no dialed op carries a multi-statement transaction — a content-addressed PUT and a conditional seal are each ONE request, so the executor brackets a single unit and replays from the boundary that unit begins at; a band whose re-drive owner is its callee's own transaction (`Store/coordination#COORDINATION_OP`) or its caller's in-process retry (`Store/provisioning#ENGINE_OPERATIONS`) therefore seats no arm here: offering a pipeline to a boundary that forbids one is law with no producer.
- Entry: `Carry` is the IN-PROCESS arm — the whole attempt re-offers inside one call over the kernel `Redrive.Run`, which admits TRANSIENT alone so a throttled fault exits to the durable arm carrying the server's own window. `Settle` is the DURABLE arm — one verdict per pass against the caller's own persisted ordinal (a resumed multipart session, a swept outbox row), holding no loop state, so a workflow that crossed a process boundary reads the same predicate its ordinal already counts.
- Auto: an UNBOUND root binds `LocalRedrive.Unbound`, whose `RedrivePolicy.None` bounds the curve at zero — `Redrive.Run` then runs exactly one pass and `Settle` abandons a re-offerable fault rather than deferring it, so the degrade is one pass with its typed refusal intact and never a silent success no caller can tell from a re-driven one; a null port reaching a leg is the deleted form.
- Packages: LanguageExt.Core (`IO`, `Fin`, `Error`, `Duration`), NodaTime (`Duration` the deferral window), Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum]`), the kernel `Rasm/Domain/results#REDRIVE` surface (`Retriability`, `RedrivePolicy`, `Verdict`, `Redrive`).
- Growth: a third cross-process plane is one `StoreHop` arm carrying its own closed verb roster, with zero edits at the port or the verdict fold — that is the whole cost of expressing a coordination or provisioning re-drive the day either band's executor moves to the hop pipeline; a new re-drive posture is one kernel `Retriability` case with the `Settle` arm it selects; a new backoff shape is a `Schedule` composition at the policy mint; an `ObjectVerb`-typed port parameter, a delegate-column port, a second verdict vocabulary, or a retry loop at this tier is the deleted form.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ColumnVerb {
    public static readonly ColumnVerb Claim = new("claim");
    public static readonly ColumnVerb Read = new("read");
    public static readonly ColumnVerb Write = new("write");
    public static readonly ColumnVerb Sweep = new("sweep");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StoreHop {
    private StoreHop() { }
    public sealed record Object(ObjectVerb Verb) : StoreHop;
    public sealed record WideColumn(ColumnVerb Verb) : StoreHop;

    public string Token => Switch(
        @object:    static o => $"object.{o.Verb.Key}",
        wideColumn: static w => $"wide-column.{w.Verb.Key}");
}

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record StoreVerdict {
    private StoreVerdict() { }
    public sealed record Delivered : StoreVerdict;
    public sealed record Faulted(Error Reason, int Attempt, Duration After) : StoreVerdict;
    public sealed record Exhausted(Error Reason, int Attempt) : StoreVerdict;
    public sealed record Refused(Error Reason) : StoreVerdict;

    public static StoreVerdict Of<T>(RedrivePolicy policy, int attempt, Fin<T> outcome) => outcome.Match(
        Succ: static _ => (StoreVerdict)new Delivered(),
        Fail: error => Redrive.Settle(policy: policy, fault: error, attempt: attempt).Switch(
            deferred:  deferred  => (StoreVerdict)new Faulted(error, deferred.Attempt, deferred.After),
            abandoned: abandoned => new Exhausted(error, abandoned.Attempt),
            terminal:  static _  => new Refused(error)));
}

// --- [SERVICES] ------------------------------------------------------------------------
public interface StoreRedrivePort {
    IO<T> Carry<T>(StoreHop hop, string instance, IO<T> attempt);
    StoreVerdict Settle<T>(StoreHop hop, string instance, int attempt, Fin<T> outcome);
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public sealed record LocalRedrive(RedrivePolicy Policy) : StoreRedrivePort {
    public static readonly StoreRedrivePort Unbound = new LocalRedrive(RedrivePolicy.None);
    public IO<T> Carry<T>(StoreHop hop, string instance, IO<T> attempt) => Redrive.Run(policy: Policy, work: attempt);
    public StoreVerdict Settle<T>(StoreHop hop, string instance, int attempt, Fin<T> outcome) =>
        StoreVerdict.Of(policy: Policy, attempt: attempt, outcome: outcome);
}
```

| [INDEX] | [POLICY]        | [VALUE]                                          | [BINDING]                                                          |
| :-----: | :-------------- | :----------------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | retriability    | kernel `Retriability` overridden per case        | returned/provider transport share one posture; others terminal     |
|  [02]   | re-offer route  | `RetryShape` beside the posture, never inside it | `Frozen` rescopes to a thaw; `Aborted` restarts a durable session  |
|  [03]   | throttle window | `Transport.RetryAfter` from the transport itself | a server-stated delay outranks the curve; a date form reads `None` |
|  [04]   | band admission  | one `Lift` per SDK edge, one `Granted` per grant | the engine interior sees only typed faults; no per-leg catch       |
|  [05]   | band code       | `[FaultCase]` ordinals on `Fault`                | `Code` seal off the base; no bare 540x literal                     |
|  [06]   | verb identity   | `ObjectVerb` on every crossing                   | one code reads per verb; a re-drive names what it re-offers        |
|  [07]   | hop identity    | `StoreHop` per cross-process plane               | a wide-column op never rides an object verb                        |
|  [08]   | verdict fold    | `StoreVerdict.Of` over `Redrive.Settle`          | band-neutral; a third band joins with no fold edit                 |
|  [09]   | executor seat   | root-bound `StoreRedrivePort`                    | this tier classifies and executes nothing                          |
|  [10]   | unbound degrade | `LocalRedrive.Unbound` at `RedrivePolicy.None`   | one pass by construction, typed refusal intact                     |

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
