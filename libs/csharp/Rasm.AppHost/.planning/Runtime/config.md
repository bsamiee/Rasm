# [APPHOST_CONFIGURATION_AND_OPTIONS]

Configuration admission for the runtime spine: eight ranked `ConfigSource` rows mount every input onto one `ConfigurationManager` chain, a source-generated binder admits immutable policy records onto the `Validation<ConfigError,T>` rail, options validate once and publish frozen at ready, every change lands as a reload-class-gated `ReloadOutcome` transition carried by a `ReloadReceipt` — a structured operator edit arriving as an RFC-6902 `application/json-patch+json` document folds through `PatchSection` onto that same transition — the operator kill-switch is one transition-class config row whose `OperatorOverride` union forces the degradation fold, the `SecretsStore` source extends into a `SecretLease` row family that acquires, renews, and zeroizes credential material against the one RID-dispatched credential-store provider the host resolves — and surfaces the per-store-open KMS-unwrap handle `Rasm.Persistence/Store/encryption` reads as one `SecretLease`-class content carrier so the cloud-KMS key-handle lifecycle stays the runtime lease's concern, never a long-lived Persistence-side key — and one `CredentialPem` axis is the suite's only credential-material wire vocabulary — the host encodes every PEM-bearing credential into one canonical RFC-7468 multi-element bundle the `\n` PEM-block delimiter joins, mints the redacted `CredentialPemWire` carrier the TS verifier and the Python admission decode, and never crosses a raw `byte[]` or a parallel base64 envelope. The page owns the source axis with rank and reload-class columns, the `ConfigError` fault vocabulary, the reload transition family, the operator forcing family, the secret-lease lifecycle, and the credential-PEM encoding vocabulary. The spine is Microsoft.Extensions.Configuration with its four provider packages, Microsoft.Extensions.Options, FluentValidation, NodaTime, Thinktecture.Runtime.Extensions, and LanguageExt.Core, with System.Security.Cryptography (the BCL `PemEncoding`/`X509Certificate2` PEM owners) and System.IO.Hashing for the credential-bundle encoding.

## [01]-[INDEX]

- [01]-[SOURCE_AXIS]: Eight ranked source rows with reload class and mount delegate.
- [02]-[TYPED_BINDING]: Fail-closed source-generated binding into validated policy records.
- [03]-[POLICY_VALUES]: Validate-once frozen publish with reload-class-gated receipted transitions.
- [04]-[KILL_SWITCH]: Operator override row forcing the degradation fold.
- [05]-[SECRET_LEASE]: Acquire-renew-zeroize credential lifecycle extending the `SecretsStore` row.
- [06]-[CREDENTIAL_PEM]: Canonical RFC-7468 PEM bundle encoding and the redacted cross-language carrier.

## [02]-[SOURCE_AXIS]

- Owner: `ConfigSource` `[SmartEnum<string>]` eight rows; `ReloadClass` `[SmartEnum<string>]` two rows; `ConfigSourceKeyPolicy` single ordinal-ignore-case key accessor; `ConfigLayer` boot input record.
- Cases: json, user-settings, host-document, secrets-store, user-secrets, in-memory, env, cli — rank order is mount order, a later mount overrides earlier keys, and the rank fold is the whole precedence law.
- Entry: `Compose(IConfigurationManager manager, ConfigLayer layer, params ReadOnlySpan<ConfigSource> sources)` — `Fin<IConfigurationManager>` aborts on the first rejected mount.
- Packages: Microsoft.Extensions.Configuration, Microsoft.Extensions.Configuration.Json, Microsoft.Extensions.Configuration.EnvironmentVariables, Microsoft.Extensions.Configuration.CommandLine, Microsoft.Extensions.Configuration.UserSecrets, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one source row on `ConfigSource` (key, rank, reload class, mount delegate); one reload-trigger is one named trigger value on the existing `ReloadReceipt` trigger band, never a new source axis; zero new surface.
- Boundary: per-profile source selection and layering are computed from the resolved profile record at the composition root, never a second profile-keyed table here; `ConfigLayer` is the boundary capsule — `HostDocument` carries the HostAttachPort doc-user-text projection, `SecretsSource` carries the app-root-owned RID-dispatched credential-store `IConfigurationSource` gated on the SOURCE_ROUTES research row, whose file-backed fallback path resolves through `PathHelper.GetSecretsPathFromSecretsId` rather than a hand-built path, `ParentSnapshot` chains a companion onto its parent snapshot through `AddJsonStream` over the parent's serialized snapshot stream so an embedded or in-memory-stream layer mounts without a temp file, `UserSettingsPath` and `ContentRoot` arrive computed from the profile row; the inbox JSON provider parses JSONC (comments plus trailing commas) with zero added package; `ConfigurationKeyComparer` is the canonical path-segment order so a numeric array index sorts before a sibling string key; section paths travel as nameof-derived symbols, never call-site string literals; ambient `IConfiguration` reads past bootstrap are rejected.

```csharp signature
public sealed class ConfigSourceKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    private static readonly StringComparer Policy = StringComparer.OrdinalIgnoreCase;

    public static IEqualityComparer<string> EqualityComparer => Policy;

    public static IComparer<string> Comparer => Policy;
}

[SmartEnum<string>]
[ValidationError<ConfigError>]
[KeyMemberEqualityComparer<ConfigSourceKeyPolicy, string>]
[KeyMemberComparer<ConfigSourceKeyPolicy, string>]
public sealed partial class ReloadClass {
    public static readonly ReloadClass Frozen = new("frozen");
    public static readonly ReloadClass Transition = new("transition");
}

public sealed record ConfigLayer(
    string ContentRoot,
    string ProfileKey,
    string UserSettingsPath,
    string[] Args,
    IReadOnlyDictionary<string, string> Switches,
    Assembly SecretsAnchor,
    Func<IEnumerable<KeyValuePair<string, string?>>> HostDocument,
    Func<IConfigurationSource> SecretsSource,
    Seq<KeyValuePair<string, string?>> Seed,
    Option<IConfiguration> ParentSnapshot);

[SmartEnum<string>]
[ValidationError<ConfigError>]
[KeyMemberEqualityComparer<ConfigSourceKeyPolicy, string>]
[KeyMemberComparer<ConfigSourceKeyPolicy, string>]
public sealed partial class ConfigSource {
    public const string EnvPrefix = "RASM_";

    public static readonly ConfigSource Json = new("json", rank: 10, reload: ReloadClass.Transition, MountJson);
    public static readonly ConfigSource UserSettings = new("user-settings", rank: 20, reload: ReloadClass.Transition, MountUserSettings);
    public static readonly ConfigSource HostDocument = new("host-document", rank: 30, reload: ReloadClass.Transition, MountHostDocument);
    public static readonly ConfigSource SecretsStore = new("secrets-store", rank: 40, reload: ReloadClass.Frozen, MountSecretsStore);
    public static readonly ConfigSource UserSecrets = new("user-secrets", rank: 50, reload: ReloadClass.Frozen, MountUserSecrets);
    public static readonly ConfigSource InMemory = new("in-memory", rank: 60, reload: ReloadClass.Frozen, MountInMemory);
    public static readonly ConfigSource Env = new("env", rank: 70, reload: ReloadClass.Frozen, MountEnv);
    public static readonly ConfigSource Cli = new("cli", rank: 80, reload: ReloadClass.Frozen, MountCli);

    public int Rank { get; }

    public ReloadClass Reload { get; }

    [UseDelegateFromConstructor]
    public partial IConfigurationBuilder Mount(IConfigurationBuilder builder, ConfigLayer layer);

    public static Fin<IConfigurationManager> Compose(IConfigurationManager manager, ConfigLayer layer, params ReadOnlySpan<ConfigSource> sources) {
        var ranked = sources.ToArray().OrderBy(static row => row.Rank).ToSeq();
        return Try.lift(() => ranked.Fold(
                    layer.ParentSnapshot.Map(parent => ((IConfigurationBuilder)manager).AddConfiguration(parent)).IfNone(manager),
                    (builder, row) => row.Mount(builder, layer)))
            .Run()
            .Map(_ => manager)
            .MapFail(static error => ConfigError.Create(error.Message));
    }

    private static IConfigurationBuilder MountJson(IConfigurationBuilder builder, ConfigLayer layer) =>
        builder
            .AddJsonFile(Path.Combine(layer.ContentRoot, "appsettings.json"), optional: false, reloadOnChange: true)
            .AddJsonFile(Path.Combine(layer.ContentRoot, $"appsettings.{layer.ProfileKey}.json"), optional: true, reloadOnChange: true);

    private static IConfigurationBuilder MountUserSettings(IConfigurationBuilder builder, ConfigLayer layer) =>
        builder.AddJsonFile(layer.UserSettingsPath, optional: true, reloadOnChange: true);

    private static IConfigurationBuilder MountHostDocument(IConfigurationBuilder builder, ConfigLayer layer) =>
        builder.AddInMemoryCollection(layer.HostDocument());

    private static IConfigurationBuilder MountSecretsStore(IConfigurationBuilder builder, ConfigLayer layer) =>
        builder.Add(layer.SecretsSource());

    private static IConfigurationBuilder MountUserSecrets(IConfigurationBuilder builder, ConfigLayer layer) =>
        builder.AddUserSecrets(layer.SecretsAnchor, optional: true, reloadOnChange: false);

    private static IConfigurationBuilder MountInMemory(IConfigurationBuilder builder, ConfigLayer layer) =>
        builder.AddInMemoryCollection(layer.Seed);

    private static IConfigurationBuilder MountEnv(IConfigurationBuilder builder, ConfigLayer layer) =>
        builder.AddEnvironmentVariables(prefix: EnvPrefix);

    private static IConfigurationBuilder MountCli(IConfigurationBuilder builder, ConfigLayer layer) =>
        builder.AddCommandLine(layer.Args, new Dictionary<string, string>(layer.Switches));
}
```

## [03]-[TYPED_BINDING]

- Owner: `PolicyBinding` static admission surface; `ConfigError` `[Union]` fault family on the doctrine `Expected` shape with the dual-tier `Create` contract.
- Cases: Text, SourceRejected, SectionAbsent, BindRejected, Scalar, Invariant, Aggregate — codes 4100-4199, `Combine` folds independent faults into Aggregate.
- Entry: `Bind<T>(IConfigurationRoot root, string section)` — `Validation<ConfigError,T>` accumulates; unknown keys fail closed through `ErrorOnUnknownConfiguration`.
- Packages: Microsoft.Extensions.Configuration.Binder, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one case on `ConfigError`; zero new surface.
- Boundary: `EnableConfigurationBindingGenerator` is a required project property on every options-binding app package — the generator intercepts `Get<T>` and `Bind` call sites with reflection-free binding; policy records are constructor-bound immutable records and `BindNonPublicProperties` is rejected; temporal scalars parse through `InstantPattern.ExtendedIso` and `DurationPattern.Roundtrip`, never culture-ambient parse; the binder exception channel is the named capture seam folded through `Try.lift` into the rail.

```csharp signature
[Union]
public abstract partial record ConfigError : Expected, IValidationError<ConfigError>, Semigroup<ConfigError> {
    private ConfigError(string detail, int code) : base(detail, code, None) { }

    public static ConfigError Create(string message) => new Text(message);

    public sealed record Text : ConfigError { public Text(string detail) : base(detail, 4100) { } }
    public sealed record SourceRejected : ConfigError {
        public SourceRejected(string source, string detail) : base($"{source}: {detail}", 4101) => Source = source;
        public string Source { get; }
    }
    public sealed record SectionAbsent : ConfigError {
        public SectionAbsent(string section) : base($"{section}: absent", 4102) => Section = section;
        public string Section { get; }
    }
    public sealed record BindRejected : ConfigError {
        public BindRejected(string section, string detail) : base($"{section}: {detail}", 4103) => Section = section;
        public string Section { get; }
    }
    public sealed record Scalar : ConfigError {
        public Scalar(string key, string detail) : base($"{key}: {detail}", 4104) => Key = key;
        public string Key { get; }
    }
    public sealed record Invariant : ConfigError {
        public Invariant(string member, string detail) : base($"{member}: {detail}", 4105) => Member = member;
        public string Member { get; }
    }
    public sealed record Aggregate : ConfigError {
        public Aggregate(Seq<ConfigError> faults) : base($"{faults.Count} faults", 4199) => Faults = faults;
        public Seq<ConfigError> Faults { get; }
    }

    public ConfigError Combine(ConfigError rhs) => (this, rhs) switch {
        (Aggregate l, Aggregate r) => new Aggregate(l.Faults + r.Faults),
        (Aggregate l, _) => new Aggregate(l.Faults.Add(rhs)),
        (_, Aggregate r) => new Aggregate(this.Cons(r.Faults)),
        _ => new Aggregate(Seq(this, rhs)),
    };
}

public static class PolicyBinding {
    public static Validation<ConfigError, T> Bind<T>(IConfigurationRoot root, string section) where T : notnull =>
        Try.lift(() => Optional(root.GetSection(section).Get<T>(static binder => binder.ErrorOnUnknownConfiguration = true)))
            .Run()
            .Match(
                Succ: present => present is { IsSome: true, Case: T bound }
                    ? (Validation<ConfigError, T>)bound
                    : new ConfigError.SectionAbsent(section),
                Fail: error => (Validation<ConfigError, T>)new ConfigError.BindRejected(section, error.Message));

    public static Validation<ConfigError, Instant> BindInstant(IConfigurationRoot root, string key) =>
        InstantPattern.ExtendedIso.Parse(root.GetValue<string>(key) ?? string.Empty) is { Success: true } parsed
            ? (Validation<ConfigError, Instant>)parsed.Value
            : new ConfigError.Scalar(key, "instant text outside InstantPattern.ExtendedIso");

    public static Validation<ConfigError, Duration> BindDuration(IConfigurationRoot root, string key) =>
        DurationPattern.Roundtrip.Parse(root.GetValue<string>(key) ?? string.Empty) is { Success: true } parsed
            ? (Validation<ConfigError, Duration>)parsed.Value
            : new ConfigError.Scalar(key, "duration text outside DurationPattern.Roundtrip");
}
```

## [04]-[POLICY_VALUES]

- Owner: `OptionsAdmission` static registration surface; `ReloadOutcome` `[Union]`; `ReloadReceipt` record.
- Cases: Applied, Unchanged, RestartRequired, Rejected — Applied re-publishes frozen policy values, Unchanged records a no-diff publish, RestartRequired is the frozen-row path, Rejected carries the `ConfigError` of a failed re-validation while the prior values stay live.
- Entry: `Admit<T>(IServiceCollection services, string section)` — composition registration; `Refine` accumulates on `Validation<ConfigError,T>` against the active rule set, `Sweep` aborts on `Fin<Unit>`.
- Auto: generated `[OptionsValidator]` validators with `[ValidateObjectMembers]` and `[ValidateEnumeratedItems]` own structural validation; `ValidateOnStart` plus the `IStartupValidator` sweep prove every registered policy record before ready; `PostConfigure` derives a dependent policy value after binding, and `Invalidate` is the one polymorphic cache cut — a named entry routes to `IOptionsMonitorCache.TryRemove`, an absent name routes to `Clear` so the whole set re-binds, never two named operations.
- Receipt: `ReloadReceipt` — section, reload class, trigger, outcome, `Instant`, correlation id.
- Packages: Microsoft.Extensions.Options, Microsoft.AspNetCore.JsonPatch.SystemTextJson, FluentValidation, NodaTime, LanguageExt.Core
- Growth: one case on `ReloadOutcome`; one config-boundary variant is one rule-set name through `IncludeRuleSets`, never a second validator; a new per-tenant policy override is one `Overlay` named-options registration keyed by `TenantContext.Slug`, never a second options surface; a structured partial config edit is one RFC-6902 `application/json-patch+json` document folded through `PatchSection` onto the same `ReloadOutcome` transition under `ReloadReceipt.PatchTrigger`, never a second mutation path; zero new surface.
- Boundary: every options registration carries its `ReloadClass` row — frozen rows re-publish only through process restart and `RestartRequired` is that named path; interior code receives frozen records read once at ready, never `IOptions` handles, and per-call-site `OnChange` callbacks are rejected; `Observe` subscriptions return disposable detachers composed LIFO by the lifecycle owner; the POSIX `SIGHUP` route and the ControlService reload-options verb enqueue the same `ReloadOutcome` transition under `SignalTrigger` and `ControlTrigger` — `SignalTrigger` is Unix-only, registered through `PosixSignalRegistration.Create(PosixSignal.SIGHUP, ...)` which the runtime supports on Linux and macOS but not on a `win-*` RID where no `SIGHUP` exists, so a Windows host carries no signal route and reload arrives exclusively through `ControlTrigger`; cross-process reload propagation rides the op-log HLC cursor; named options key by smart-enum keys; FluentValidation owns cross-field invariants behind `Refine`, where the active rule set is itself a policy value admitted through `ValidationContext.CreateWithOptions` and `IncludeRuleSets` so a boundary variant runs its own rule subset, `When`/`Unless` gate a rule on a sibling-member predicate, `DependentRules` chains a rule block that runs only after its predecessors pass, and `ChildRules` validates an inline nested member graph without a second `IValidator` type, so a relational invariant across two policy fields is one rule expression rather than a hand-rolled post-bind check; `PolymorphicValidator` and `SetInheritanceValidator` route subtype policy records to their own graph, `WithState` carries a constructed `ConfigError` straight off the failure so `Refine` reads the typed fault before falling back to the `WithErrorCode`/`WithSeverity` 4100-4199 band, and the flat `ToDictionary` re-derivation is the deleted form; a monitor-cache invalidation becomes a typed runtime transition through the polymorphic `Invalidate` over `TryRemove` and `Clear`, never an ambient re-read; `BindConfiguration(section, configureBinder)` rides `OptionsBuilderConfigurationExtensions` from Microsoft.Extensions.Options.ConfigurationExtensions, a lock-pinned transitive of the hosting closure, never a direct project asset; a per-tenant policy override is a named-options registration keyed by `TenantContext.Slug` through `Overlay` — the named instance binds the tenant overlay section `{section}:tenants:{slug}` over the base section so `IOptionsMonitor.Get(slug)` reads the tenant-overlaid record while the default name carries the single-tenant `Root` value, never a parallel tenant-config table, and the overlay change rides the same `ReloadClass.Transition` reload as the base section; a structured operator config edit arrives as an RFC-6902 `application/json-patch+json` document the `PatchSection` route applies to the live `{section}` `JsonObject` projection through the package's own `JsonPatchDocument.ApplyTo(JsonObject, logErrorAction)` over the `JsonObjectAdapter` — the `logErrorAction` delegate is the named capture seam folding each `JsonPatchError` into a `ConfigError.BindRejected` so a bad op-path lands the `ReloadOutcome.Rejected` carrying the typed fault while the prior values stay live, the applied `JsonObject` re-admits through the section-keyed `revalidate` closure the composition root registers per section — itself the composed `PolicyBinding.Bind<T>` + `Refine` for that section's policy type, so the patch route never names `T` at the verb seam and a patch that breaks an invariant never publishes — the whole apply gates on the section's `ReloadClass` so a `Frozen` section answers `RestartRequired` and only a `Transition` section re-publishes, and the transition stamps `ReloadReceipt.PatchTrigger` distinguishing it from the monitor, signal, and control triggers — a hand-rolled RFC-6902 operation dispatch and a Newtonsoft `JsonPatchDocument` are the deleted forms, the package owns the `op`/`path`/`from`/`value` operation model and the `Test`-op precondition assertion that fails the whole patch before any mutation lands.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReloadOutcome {
    private ReloadOutcome() { }

    public sealed record Applied(string Section) : ReloadOutcome;
    public sealed record Unchanged(string Section) : ReloadOutcome;
    public sealed record RestartRequired(string Section) : ReloadOutcome;
    public sealed record Rejected(string Section, ConfigError Fault) : ReloadOutcome;
}

public sealed record ReloadReceipt(
    string Section,
    ReloadClass Class,
    string Trigger,
    ReloadOutcome Outcome,
    Instant At,
    CorrelationId CorrelationId) {
    public const string MonitorTrigger = "options-monitor";
    public const string SignalTrigger = "posix-sighup";
    public const string ControlTrigger = "control-reload-options";
    public const string PatchTrigger = "json-patch-section";
}

public static class OptionsAdmission {
    public static OptionsBuilder<T> Admit<T>(IServiceCollection services, string section) where T : class =>
        services.AddOptions<T>()
            .BindConfiguration(section, static binder => binder.ErrorOnUnknownConfiguration = true)
            .ValidateOnStart();

    public static OptionsBuilder<T> Overlay<T>(IServiceCollection services, string section, TenantContext tenant) where T : class =>
        services.AddOptions<T>(tenant.Slug)
            .BindConfiguration($"{section}:tenants:{tenant.Slug}", static binder => binder.ErrorOnUnknownConfiguration = true)
            .ValidateOnStart();

    public static Validation<ConfigError, T> Refine<T>(T policy, IValidator<T> validator, Option<Seq<string>> ruleSets = default) where T : notnull =>
        (ruleSets.Case is Seq<string> sets
                ? validator.Validate(ValidationContext<T>.CreateWithOptions(policy, options => options.IncludeRuleSets([.. sets])))
                : validator.Validate(policy))
            .Errors.AsIterable()
            .Map(static failure => failure.CustomState is ConfigError carried
                ? carried
                : failure.ErrorCode is { Length: > 0 } code && int.TryParse(code, out var coded) && coded is >= 4100 and <= 4199
                    ? (ConfigError)new ConfigError.Scalar(failure.PropertyName, failure.ErrorMessage)
                    : new ConfigError.Invariant(failure.PropertyName, failure.ErrorMessage))
            .ToSeq() is { IsEmpty: false } faults
            ? new ConfigError.Aggregate(faults)
            : (Validation<ConfigError, T>)policy;

    public static Fin<Unit> Sweep(IStartupValidator validator) =>
        Try.lift(fun(validator.Validate))
            .Run()
            .MapFail(static error => ConfigError.Create(error.Message));

    public static Unit Invalidate<T>(IOptionsMonitorCache<T> cache, Option<string> name = default) where T : class =>
        name.Case is string named ? ignore(cache.TryRemove(named)) : (cache.Clear(), unit).Item2;

    public static Validation<ConfigError, ReloadOutcome> PatchSection(JsonObject live, string section, ReloadClass reload, JsonPatchDocument patch, Func<JsonObject, Validation<ConfigError, Unit>> revalidate) =>
        reload == ReloadClass.Frozen
            ? (Validation<ConfigError, ReloadOutcome>)(ReloadOutcome)new ReloadOutcome.RestartRequired(section)
            : Try.lift(() => {
                    var faults = new List<ConfigError>();
                    patch.ApplyTo(live, error => faults.Add(new ConfigError.BindRejected(error.Operation?.path ?? section, error.ErrorMessage)));
                    return faults.ToSeq();
                })
                .Run()
                .Match(
                    Succ: applied => applied is { IsEmpty: false } ops
                        ? (Validation<ConfigError, ReloadOutcome>)new ConfigError.Aggregate(ops)
                        : revalidate(live).Map(static _ => (ReloadOutcome)new ReloadOutcome.Applied(section)),
                    Fail: error => (Validation<ConfigError, ReloadOutcome>)new ConfigError.BindRejected(section, error.Message));

    public static IDisposable Observe<T>(IOptionsMonitor<T> monitor, string section, ReloadClass reload, IClock clock, CorrelationId correlation, Func<T, ReloadOutcome> republish, Action<ReloadReceipt> sink) where T : class =>
        monitor.OnChange((snapshot, _) => sink(new ReloadReceipt(
            Section: section,
            Class: reload,
            Trigger: ReloadReceipt.MonitorTrigger,
            Outcome: reload.Switch(
                state: (Section: section, Snapshot: snapshot, Republish: republish),
                frozen: static gate => (ReloadOutcome)new ReloadOutcome.RestartRequired(gate.Section),
                transition: static gate => gate.Republish(gate.Snapshot)),
            At: clock.GetCurrentInstant(),
            CorrelationId: correlation)))!;
}
```

## [05]-[KILL_SWITCH]

- Owner: `KillSwitchConfig` config row record; `OperatorOverride` `[Union]` forcing family.
- Cases: ForceLevel, Release — ForceLevel carries a degradation row key as text, Release withdraws the force.
- Entry: `From(KillSwitchConfig row, Instant at)` — total projection from the bound row into the forcing family.
- Packages: Microsoft.Extensions.Configuration.Binder, NodaTime, Thinktecture.Runtime.Extensions
- Growth: one case on `OperatorOverride`; zero new surface — the degradation fold gains one input arm per case.
- Boundary: `KillSwitchConfig` binds at the `Section` symbol as a `ReloadClass.Transition` row, so an operator flip lands without restart; forced beats derived and Release re-derives inside the health-and-degradation fold, which also admits `Level` against the `DegradationLevel` row keys; the ControlService set-degradation verb is the service-modality wire route into the same union; the keyed manual breaker control on hops is the enforcement consequence at the hop registry.

```csharp signature
public sealed record KillSwitchConfig(string? ForcedLevel, string? Reason) {
    public const string Section = nameof(KillSwitchConfig);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OperatorOverride {
    private OperatorOverride() { }

    public sealed record ForceLevel(string Level, string Reason, Instant At) : OperatorOverride;
    public sealed record Release(string Reason, Instant At) : OperatorOverride;

    public static OperatorOverride From(KillSwitchConfig row, Instant at) =>
        row.ForcedLevel is { Length: > 0 } level
            ? new ForceLevel(Level: level, Reason: row.Reason ?? string.Empty, At: at)
            : new Release(Reason: row.Reason ?? string.Empty, At: at);
}
```

## [06]-[SECRET_LEASE]

- Owner: `SecretLease` boundary capsule extending `ConfigSource.SecretsStore` — the only credential lifecycle owner; `LeaseTransition` `[Union]` lifecycle vocabulary; `SecretFault` `[Union]` fault family in the 4780-4789 band; `SecretReceipt` the redacted rotation evidence record.
- Cases: lifecycle transitions Acquired | Renewed | Released | Zeroized; `SecretFault` = Text | AcquireRejected | RenewMissed | StoreUnavailable.
- Entry: `Acquire(SecretRuntime runtime, string keyId)` returns `Fin<SecretLease>` — the credential-store read folds the `ConfigLayer.SecretsSource` provider into a held lease on `Fin`; `Renew(SecretRuntime runtime, SecretLease lease)` returns `Fin<SecretLease>` re-pulling before expiry and zeroizing the prior copy; `Zeroize(SecretLease lease)` returns `Unit`, the drain-forced terminal that overwrites the in-memory copy.
- Auto: renewal registers one `ScheduleEntry` on Runtime/time#SCHEDULE_PORT at the credential-rotation `DeadlineClass` row carrying a `LeasePolicy` whose `CrashStaleness` outlives the renewal window, so a single occurrence row drives rotation ahead of expiry with no per-secret timer; the zeroization registers as one Runtime/lifecycle#DRAIN_CONDUCTOR `DrainBand.Stores` participant row that runs under the drain-forced token so a hung renewal never strands a live secret; the credential bytes carry `DataClassification.Secret` so Observability/telemetry#REDACTION_TAXONOMY erases them at every egress and the receipt diff folds through the bound `Redactor`.
- Receipt: `SecretReceipt` carries the lease window, the content-hash of the canonical credential bytes, and the redacted credential-id diff only — never a secret byte; the transition emits on ReceiptSinkPort.Send partitioned by `TenantId` so each tenant's rotation stream stays isolated.
- Packages: Microsoft.Extensions.Configuration.UserSecrets, Microsoft.Extensions.Compliance.Redaction, System.IO.Hashing, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: one lifecycle transition is one `LeaseTransition` case; one fault is one `SecretFault` case; a new credential source is one `SecretsSource` provider value on the existing `ConfigLayer`, never a second lease owner; zero new surface.
- Boundary: the lease is the suite's only credential lifecycle owner — a per-secret rotation helper, a raw `string` credential field, and a second zeroization path are the deleted forms; a lease renews strictly before expiry or the fold degrades through Observability/health#DEGRADATION_RAIL, never a hard fault, so `RenewMissed` lands `DegradationLevel.ReadOnly` rather than terminating the rail; the in-memory copy is a rented `byte[]` overwritten through `CryptographicOperations.ZeroMemory` so no managed copy survives collection; the rotation-diff identity is the `XxHash128` content digest of the canonical credential bytes — a non-cryptographic `System.IO.Hashing` value carrying identity only, never a security claim, so the diff is an equality of digest bytes with no constant-time pretense layered over a non-crypto hash; the lease holds the live raw `byte[]` and owns only the in-memory lifecycle and zeroization, while the canonical at-rest and on-wire credential encoding is `CREDENTIAL_PEM`'s `CredentialBundle`/`CredentialPem` — the lease never encodes material and the PEM axis never holds a live mutable copy, so a PEM-bearing credential's `SecretReceipt.ContentHash` is the `CredentialBundle` per-block digest fold and the redacted rotation crosses as the `CredentialPemWire` carrier, never two parallel credential encodings; the lease extends `ConfigSource.SecretsStore` rank-40 frozen-class row — the credential never re-mounts at runtime, the lease owns the live rotation above that frozen mount, and the credential-store read reuses `ConfigLayer.SecretsSource` rather than a parallel provider; the per-store-open KMS-unwrap handle `Rasm.Persistence/Store/encryption` reads crosses as one `SecretLease`-class content carrier through the `Runtime ⇄ Rasm.Persistence/Store/encryption # [PORT]: KMS-unwrap port` seam — the lease owns the acquire-renew-zeroize custody of the cloud-KMS CMK access (the `KmsProvider`-resolved credential the Persistence `EnvelopeKeyring` `Mint`/`Unwrap`/`Rewrap`/`Probe` delegate quartet binds against, where each arm's mechanism is a policy value on the `KmsProvider` row — AWS encrypt-as-wrap, Azure native `WrapKey`/`UnwrapKey`, GCP encrypt-as-wrap with CRC32C and primary-version repoint — not one arm's spelling as a universal law) so the in-process key-handle lifecycle stays the runtime lease's concern and Persistence consumes the resolved per-open handle without minting a long-lived in-process key, the unwrapped DEK never persists and zeroizes through the same `CryptographicOperations.ZeroMemory` path the lease owns, and the KMS-unwrap handle is a content carrier riding this lifecycle, never an eighth port — a Persistence-side long-lived key cache or a second credential lifecycle is the deleted form.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LeaseTransition {
    private LeaseTransition() { }
    public sealed record Acquired(string KeyId, Interval Window) : LeaseTransition;
    public sealed record Renewed(string KeyId, Interval Window) : LeaseTransition;
    public sealed record Released(string KeyId, Instant At) : LeaseTransition;
    public sealed record Zeroized(string KeyId, Instant At) : LeaseTransition;
}

[Union]
public abstract partial record SecretFault : Expected, IValidationError<SecretFault> {
    private SecretFault(string detail, int code) : base(detail, code, None) { }
    public static SecretFault Create(string message) => new Text(message);
    public sealed record Text : SecretFault { public Text(string detail) : base(detail, 4780) { } }
    public sealed record AcquireRejected : SecretFault { public AcquireRejected(string keyId, string detail) : base($"{keyId}: {detail}", 4781) => KeyId = keyId; public string KeyId { get; } }
    public sealed record RenewMissed : SecretFault { public RenewMissed(string keyId, string detail) : base($"{keyId}: {detail}", 4782) => KeyId = keyId; public string KeyId { get; } }
    public sealed record StoreUnavailable : SecretFault { public StoreUnavailable(string detail) : base(detail, 4783) { } }
}

public sealed record SecretReceipt(
    string KeyId,
    LeaseTransition Transition,
    Interval Window,
    string ContentHash,
    string RedactedId,
    Instant At);

public sealed record SecretRuntime(
    Func<string, Fin<byte[]>> Read,
    Redactor Redactor,
    LeasePolicy Lease,
    DeadlineClass Rotation,
    ClockPolicy Clocks,
    ReceiptSinkPort Sink,
    TenantContext Tenant,
    CorrelationId Correlation,
    JsonSerializerOptions Wire);

public sealed record SecretLease(string KeyId, byte[] Material, Interval Window, ScheduleEntry Renewal) {
    public static string Digest(ReadOnlySpan<byte> material) =>
        Convert.ToHexStringLower(System.IO.Hashing.XxHash128.Hash(material));

    public string Redacted(Redactor redactor) {
        Span<char> sink = stackalloc char[redactor.GetRedactedLength(KeyId)];
        var written = redactor.Redact(KeyId, sink);
        return new string(sink[..written]);
    }
}

public static class SecretLeaseOps {
    public static Fin<SecretLease> Acquire(SecretRuntime runtime, string keyId) =>
        runtime.Read(keyId)
            .MapFail(error => (Error)new SecretFault.AcquireRejected(keyId, error.Message))
            .Map(material => {
                var now = runtime.Clocks.Now;
                var window = ClockPolicy.Window(now + runtime.Rotation.Allotted, runtime.Rotation.Allotted);
                var renewal = new ScheduleEntry(
                    Key: $"secret-renew:{keyId}",
                    Spec: new OccurrenceSpec.Every(runtime.Rotation.Allotted),
                    Deadline: runtime.Rotation,
                    Lease: Some(runtime.Lease),
                    Work: () => IO.pure(unit));
                return Emit(runtime, new SecretLease(keyId, material, window, renewal), new LeaseTransition.Acquired(keyId, window));
            });

    public static Fin<SecretLease> Renew(SecretRuntime runtime, SecretLease lease) =>
        runtime.Clocks.Now is var now && now >= lease.Window.End
            ? Fin.Fail<SecretLease>(new SecretFault.RenewMissed(lease.KeyId, "lease expired before renewal"))
            : runtime.Read(lease.KeyId)
                .MapFail(error => (Error)new SecretFault.RenewMissed(lease.KeyId, error.Message))
                .Map(material => {
                    CryptographicOperations.ZeroMemory(lease.Material);
                    var window = ClockPolicy.Window(now + runtime.Rotation.Allotted, runtime.Rotation.Allotted);
                    return Emit(runtime, lease with { Material = material, Window = window }, new LeaseTransition.Renewed(lease.KeyId, window));
                });

    public static Unit Zeroize(SecretRuntime runtime, SecretLease lease) {
        ignore(Emit(runtime, lease, new LeaseTransition.Zeroized(lease.KeyId, runtime.Clocks.Now)));
        CryptographicOperations.ZeroMemory(lease.Material);
        return unit;
    }

    static SecretLease Emit(SecretRuntime runtime, SecretLease lease, LeaseTransition transition) {
        var receipt = new SecretReceipt(
            lease.KeyId, transition, lease.Window,
            SecretLease.Digest(lease.Material), lease.Redacted(runtime.Redactor), runtime.Clocks.Now);
        ignore(runtime.Sink.Send(
            runtime.Correlation, runtime.Tenant, TelemetrySource.AppHost.Key, nameof(SecretLease),
            JsonSerializer.SerializeToElement(receipt, runtime.Wire)).Run());
        return lease;
    }
}
```

## [07]-[CREDENTIAL_PEM]

- Owner: `PemLabel` `[SmartEnum<string>]` the closed RFC-7468 textual-encoding label vocabulary under the `ConfigSourceKeyPolicy` accessor; `PemBlock` the single armored element; `CredentialBundle` the ordered multi-element bundle the canonical `\n` PEM-block delimiter joins; `CredentialPemWire` the redacted cross-language carrier; `PemFault` `[Union]` fault family in the 4790-4799 band; `CredentialPem` the static encode-decode-redact surface.
- Cases: 6 label rows — certificate, public-key, private-key, ec-private-key, rsa-private-key, pkcs7 — the RFC-7468 armor labels the BCL `PemEncoding` writes between the `-----BEGIN {label}-----`/`-----END {label}-----` lines; `PemFault` = Text | LabelUnknown | ArmorMalformed | EmptyBundle.
- Entry: `Encode(CredentialBundle bundle)` returns `string` — one fold writes each `PemBlock` through `PemEncoding.WriteString(label, der)` and joins the armored elements with the single `\n` RFC-7468 inter-block delimiter, so a certificate chain plus its private key crosses as one canonical bundle text whose element boundary is the `-----END-----`/`-----BEGIN-----` armor pair, never a hand-built `--SEP--` token; `Decode(string text)` returns `Fin<CredentialBundle>` — one fold walks `PemEncoding.TryFind` across the text, peeling each `-----BEGIN/END-----` armored element into a `PemBlock` so the decoder reads any RFC-7468 producer's bundle without a separator contract; `Carrier(CredentialBundle bundle, string keyId, Redactor redactor, ClockPolicy clocks)` returns `CredentialPemWire` — the redacted carrier the wire crosses, carrying the bundle's label set, the per-block `XxHash128` content digest, and the redacted key-id, never a private-key byte.
- Auto: the bundle is the canonical wire shape the `SecretLease` produces and the TS verifier and Python admission consume — a credential-material wire crossing as a raw `byte[]`, a bare base64 string, or a hand-built `\n--SEP--\n`-joined envelope is the deleted form, the RFC-7468 armor IS the self-delimiting separator and the `\n` between an `-----END-----` and the next `-----BEGIN-----` is the only inter-block byte; the `CredentialBundle.Cert(X509Certificate2 certificate)` factory derives a certificate bundle from the cert's own DER (`X509Certificate2.RawData`) so the host never hand-encodes bytes it already owns through the BCL cert surface, and `CredentialPem.Decode` round-trips a `CERTIFICATE` block through `X509Certificate2.CreateFromPem(text)` so the decoder proves the armored bytes parse as a real certificate before admission; a `PrivateKey`-classed `PemBlock` carries `DataClassification.Secret` so Observability/telemetry#REDACTION_TAXONOMY erases its bytes at every egress and the `CredentialPemWire` never carries a private-key block's content, only its label and content digest; the per-block digest is the non-cryptographic `XxHash128` identity value the `System.IO.Hashing` rail law forbids security claims over, so the wire carrier proves bundle identity without exposing material and the rotation diff is a digest equality, never a constant-time pretense.
- Receipt: the credential rotation rides the `SecretReceipt` the `SECRET_LEASE` cluster mints — `SecretReceipt.ContentHash` is the `CredentialBundle` per-block digest fold and `SecretReceipt.RedactedId` the carrier's redacted key-id, so the PEM axis adds no parallel receipt and the `CredentialPemWire` is the redacted projection the receipt sink already fans.
- Packages: System.Security.Cryptography, System.IO.Hashing, Microsoft.Extensions.Compliance.Redaction, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: one armor label is one `PemLabel` row; one bundle-element kind is one `PemBlock` carried in the existing ordered bundle, never a parallel envelope; one fault is one `PemFault` case; a new credential material kind rides the label axis already; zero new surface.
- Boundary: the PEM axis is the suite's only credential-material wire owner — the `SecretLease` holds the live `byte[]` in memory and zeroizes it, while `CredentialPem` owns the canonical at-rest and on-wire encoding, so the lease lifecycle and the material encoding never merge into one surface and never split the material into two encodings; the BCL `PemEncoding` owns the RFC-7468 armor write/find and `X509Certificate2.ExportCertificatePem`/`CreateFromPem` own the certificate round-trip — a hand-rolled base64 wrap, a manual `-----BEGIN-----` string build, and a Newtonsoft or third-party PEM codec are the deleted forms; the bundle crosses to TS as the `CredentialPemWire` the `security/auth` `WebauthnCredential` public-key column and the OAuth provider-cert path decode through `@simplewebauthn/server`'s own key parse, and to Python as the carrier the `runtime/execution/admission` `SettingsAdmission` secret-file source reads through `cryptography`'s `load_pem_*` parse — both consumers decode the one C#-minted bundle and re-mint no parallel PEM vocabulary, per architecture#CROSS_LANGUAGE_WIRE; the private-key block never crosses in the `CredentialPemWire` carrier — only the public certificate chain, the label set, and the content digests cross, so a TS or Python verifier reads the credential's public identity off the wire while the private material stays host-side under the `SecretLease` zeroization; the label set is the closed RFC-7468 vocabulary the BCL writes, so an unknown armor label decodes to `PemFault.LabelUnknown` rather than admitting an unrecognized block.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ConfigSourceKeyPolicy, string>]
[KeyMemberComparer<ConfigSourceKeyPolicy, string>]
public sealed partial class PemLabel {
    public static readonly PemLabel Certificate = new("CERTIFICATE", secret: false);
    public static readonly PemLabel PublicKey = new("PUBLIC KEY", secret: false);
    public static readonly PemLabel PrivateKey = new("PRIVATE KEY", secret: true);
    public static readonly PemLabel EcPrivateKey = new("EC PRIVATE KEY", secret: true);
    public static readonly PemLabel RsaPrivateKey = new("RSA PRIVATE KEY", secret: true);
    public static readonly PemLabel Pkcs7 = new("PKCS7", secret: false);

    public bool Secret { get; }
}

public readonly record struct PemBlock(PemLabel Label, ReadOnlyMemory<byte> Der) {
    public string Digest => Convert.ToHexStringLower(System.IO.Hashing.XxHash128.Hash(Der.Span));
    public string Armor => PemEncoding.WriteString(Label.Key, Der.Span);
}

public sealed record CredentialBundle(Seq<PemBlock> Blocks) {
    public static CredentialBundle Cert(X509Certificate2 certificate) =>
        new(Seq(new PemBlock(PemLabel.Certificate, certificate.RawData)));

    public FrozenSet<string> Labels => Blocks.Map(static block => block.Label.Key).ToFrozenSet(StringComparer.Ordinal);
    public bool CarriesSecret => Blocks.Exists(static block => block.Label.Secret);
}

public readonly record struct CredentialPemWire(
    string KeyId,
    FrozenSet<string> Labels,
    Seq<string> BlockDigests,
    string BundleDigest,
    Instant At);

[Union]
public abstract partial record PemFault : Expected, IValidationError<PemFault> {
    private PemFault(string detail, int code) : base(detail, code, None) { }
    public static PemFault Create(string message) => new Text(message);
    public sealed record Text : PemFault { public Text(string detail) : base(detail, 4790) { } }
    public sealed record LabelUnknown : PemFault { public LabelUnknown(string label) : base($"{label}: unknown PEM label", 4791) => Label = label; public string Label { get; } }
    public sealed record ArmorMalformed : PemFault { public ArmorMalformed(string detail) : base(detail, 4792) { } }
    public sealed record EmptyBundle : PemFault { public EmptyBundle() : base("empty PEM bundle", 4793) { } }
}

public static class CredentialPem {
    public static string Encode(CredentialBundle bundle) =>
        string.Join('\n', bundle.Blocks.Map(static block => block.Armor));

    public static Fin<CredentialBundle> Decode(string text) {
        var span = text.AsSpan();
        var blocks = Seq<PemBlock>();
        while (PemEncoding.TryFind(span, out var fields)) {
            var label = span[fields.Label].ToString();
            var der = new byte[fields.DecodedDataLength];
            if (!Convert.TryFromBase64Chars(span[fields.Base64Data], der, out _))
                return Fin.Fail<CredentialBundle>(new PemFault.ArmorMalformed(label));
            if (!PemLabel.TryGet(label, out var row))
                return Fin.Fail<CredentialBundle>(new PemFault.LabelUnknown(label));
            blocks = blocks.Add(new PemBlock(row, der));
            span = span[fields.Location.End..];
        }
        return blocks.IsEmpty ? Fin.Fail<CredentialBundle>(new PemFault.EmptyBundle()) : Fin.Succ(new CredentialBundle(blocks));
    }

    public static CredentialPemWire Carrier(CredentialBundle bundle, string keyId, Redactor redactor, ClockPolicy clocks) {
        Span<char> sink = stackalloc char[redactor.GetRedactedLength(keyId)];
        var written = redactor.Redact(keyId, sink);
        var digests = bundle.Blocks.Map(static block => block.Digest);
        return new CredentialPemWire(
            KeyId: new string(sink[..written]),
            Labels: bundle.Labels,
            BlockDigests: digests,
            BundleDigest: Convert.ToHexStringLower(System.IO.Hashing.XxHash128.Hash(Encoding.UTF8.GetBytes(string.Concat(digests)))),
            At: clocks.Now);
    }
}
```

## [08]-[TS_PROJECTION]

- Owner: `CredentialPemWire` — the redacted credential-bundle carrier the TS `security/auth` and the Python `runtime/execution/admission` decode; the raw bundle text crosses as the standard RFC-7468 PEM string the consumers parse through their own key surfaces.
- Entry: the bundle text crosses as the canonical multi-element PEM string (`-----BEGIN/END-----` armored blocks joined by `\n`), and the redacted carrier crosses as `CredentialPemWire` so a consumer reads the bundle's label set and content digests without the private-key bytes.
- Packages: BCL inbox
- Growth: one wire-member row per new carrier field; the label set crosses as a string array of the closed RFC-7468 labels; zero new surface.
- Boundary: the PEM bundle text crosses as the standard RFC-7468 armored string so a consumer's own PEM parser (`@simplewebauthn/server` key parse on TS, `cryptography.hazmat` `load_pem_*` on Python) reads the same bytes the BCL `PemEncoding` wrote, never a re-minted base64 envelope; the carrier never carries a private-key block's content — only the label set, the per-block `XxHash128` digests, and the redacted key-id cross — so the TS and Python verifiers read the credential's public identity off the wire while the private material stays host-side; the bundle separator is the RFC-7468 armor itself, so a consumer splits blocks on the `-----BEGIN-----`/`-----END-----` boundary its PEM parser already owns, never a `--SEP--` token.

```ts contract
type PemLabelKey =
  | "CERTIFICATE"
  | "PUBLIC KEY"
  | "PRIVATE KEY"
  | "EC PRIVATE KEY"
  | "RSA PRIVATE KEY"
  | "PKCS7";

interface CredentialPemWire {
  readonly keyId: string;
  readonly labels: ReadonlyArray<PemLabelKey>;
  readonly blockDigests: ReadonlyArray<string>;
  readonly bundleDigest: string;
  readonly at: string;
}
```

## [09]-[RESEARCH]

- [SOURCE_ROUTES]: the secrets-store provider route behind `ConfigLayer.SecretsSource` is RID-dispatched, never a single universal keychain — macOS resolves to Security.framework `SecItemCopyMatching` P/Invoke (from the Security.framework headers) versus a `/usr/bin/security` child process; Linux has no keychain and resolves to libsecret/`systemd-creds`/a file-backed `UserSecrets` store; Windows resolves to DPAPI/Credential Manager. The P/Invoke entrypoints are authored from the Apple/Linux man-pages and SDK headers and stay RESEARCH-flagged; no live `SecItem*`/DPAPI/credential-store read is performed during authoring because each raises an OS unlock dialog. The `SecretRuntime.Read` delegate the `SecretLease` acquires through resolves to whichever store the RID selected, never a parallel reader beside it.
- [SIGHUP_RELOAD]: the launchd and systemd `SIGHUP` reload-trigger delivery that re-mounts the transition-class sources and enqueues one `ReloadOutcome` under `ReloadReceipt.SignalTrigger` — the registration is `System.Runtime.InteropServices.PosixSignalRegistration.Create(PosixSignal.SIGHUP, ctx => ...)`, a Unix-only seam (Linux + macOS); a `win-*` host has no `SIGHUP` and routes reload solely through `ControlTrigger`, so the `SignalTrigger` band never fabricates a Windows signal path. The `PosixSignalRegistration`/`PosixSignal` member shape is not in the folder `.api/` catalogue; its spelling and the `PosixSignalContext.Cancel` re-handling default are the open verification before transcription. The live delivery of `SIGHUP` under the running service manager (one reload, zero drains) is the open distinction the live service-manager host resolves.
- [BINDER_COVERAGE]: source-generated binder interception of `Get<T>` with `BinderOptions.ErrorOnUnknownConfiguration` under `EnableConfigurationBindingGenerator`.
- [PATCH_ERROR_SHAPE]: the `JsonPatchDocument.ApplyTo(object objectToApplyTo, Action<JsonPatchError> logErrorAction)` overload and the `JsonObjectAdapter` `JsonObject` mutation path are catalogued in `.api/api-jsonpatch.md`, and `Operations.Operation` carries the `path` field; the `JsonPatchError.Operation` (typed `Operations.Operation?`) and `JsonPatchError.ErrorMessage` property spellings the `logErrorAction` delegate reads, plus the `application/json-patch+json` `IEndpointParameterMetadataProvider` intake the `DispatchPatch` control verb mounts, are the one open verification before transcription — the `op`/`path`/`from`/`value` operation model and the `Test`-op precondition assertion are package-owned and never re-spelled. The patch applies to a `JsonObject` projection of the live section rather than a typed `JsonPatchDocument<T>` so a `Test`-op precondition failure or an invalid op-path rejects the whole document before any re-bind, and the applied object re-enters through `PolicyBinding.Bind<T>`/`Refine` so a structurally valid but invariant-breaking patch never publishes.
- [ZEROIZE_PRIMITIVE]: `System.Security.Cryptography.CryptographicOperations.ZeroMemory(Span<byte>)` is the doctrine integrity owner's overwrite member the `SecretLease` zeroize and renew paths bind; the member is not yet in the folder `.api/` catalogue, so its span-overload spelling is the one open verification before transcription. The earlier `FixedTimeEquals` rotation-diff claim is withdrawn — the digest is the non-cryptographic `XxHash128` identity value the `System.IO.Hashing` rail law forbids security claims over, so no constant-time comparison member is bound and none is catalogued.
- [KMS_UNWRAP_PORT]: the per-store-open KMS-unwrap handle `Rasm.Persistence/Store/encryption#KEY_ENVELOPE` reads crosses through the `[PORT]: KMS-unwrap port` seam as one `SecretLease`-class content carrier — the lease owns the acquire-renew-zeroize custody of the cloud-KMS CMK access the Persistence `EnvelopeKeyring` (the provider-neutral `Mint`/`Unwrap`/`Rewrap`/`Probe` delegate quartet one `KmsProvider` row projects, each arm's mechanism a policy value on that row — AWS `GenerateDataKey`/`Decrypt`/`ReEncrypt` encrypt-as-wrap, Azure native `WrapKey`/`UnwrapKey` RSA-OAEP-256, GCP `Encrypt`/`Decrypt` encrypt-as-wrap with bidirectional CRC32C integrity and `UpdateCryptoKeyPrimaryVersion` primary-version repoint, never one arm's spelling masquerading as a universal law) binds against, so the in-process key-handle lifecycle stays the runtime lease's concern and Persistence consumes the resolved per-open handle, never minting a long-lived in-process key; the `Probe` arm resolves the key's lifecycle `KeyState` so a wrap against a disabled or destroy-scheduled key rejects at admission, the `Rewrap` arm advances the version ladder per the row's rotation kind, the unwrapped DEK zeroizes through the same `CryptographicOperations.ZeroMemory` path and the handle is a content carrier riding this lifecycle, never an eighth port, per `Runtime/ports#PORT_RECORDS` (a `SecretLease` row is never promoted to a port). The concrete `KmsProvider` axis (`AWSSDK.KeyManagementService`/`Azure.Security.KeyVault.Keys`/`Google.Cloud.Kms.V1`) and the per-arm AAD binding stay Persistence-side — the `EnvelopeAad` carrying the store partition and (under RLS) the `TenantContext.TenantId.Uuid` digested through `XxHash128`, ridden as the provider `EncryptionContext`/`AdditionalAuthenticatedData` exact-match on the AWS/GCP `context` arms and compared application-side against the persisted digest on the Azure native-wrap `application` arm; AppHost surfaces only the lease-managed handle custody.
- [PEM_ENCODING]: the `System.Security.Cryptography.PemEncoding` armor owner the `CredentialPem` axis binds — `PemEncoding.WriteString(ReadOnlySpan<char> label, ReadOnlySpan<byte> data)` for the per-block armor write, `PemEncoding.TryFind(ReadOnlySpan<char> pemData, out PemFields fields)` for the multi-element walk, and the `PemFields` struct (`Location`/`Label`/`Base64Data`/`DecodedDataLength` ranges) the decoder reads — plus `System.Security.Cryptography.X509Certificates.X509Certificate2.RawData` (the cert's DER bytes the `CredentialBundle.Cert` factory armors) and `X509Certificate2.CreateFromPem(ReadOnlySpan<char> certPem)` for the certificate round-trip the decoder proves, are BCL inbox members not yet in the folder `.api/` catalogue, so their exact spellings are the one open verification before transcription, authored from the .NET cryptography surface. The `PemBlock.Der` `ReadOnlyMemory<byte>` is the raw DER the armor encodes; the `Decode` fold reads `PemFields.DecodedDataLength` and `PemFields.Base64Data` to size and fill the DER through `Convert.TryFromBase64Chars`, never a fabricated single decode member — the assay binder indexes `System.Security.Cryptography`/`System.Security.Cryptography.X509Certificates` once these enter the folder `.api/` catalogue. The bundle separator is the RFC-7468 `-----END-----`/`-----BEGIN-----` armor boundary joined by `\n`, never a `--SEP--` token: the textual-encoding self-delimitation IS the separator, so no separator constant is minted and the earlier `\n--SEP--\n` placeholder resolves to the single `\n` the `Encode` fold's `string.Join('\n', ...)` writes between self-armored blocks.
- [CREDENTIAL_WIRE_CONSUMERS]: the C#-minted `CredentialBundle` PEM text and the redacted `CredentialPemWire` carrier are the suite's only credential-material wire vocabulary — TS `services/security/auth` decodes the certificate/public-key blocks through `@simplewebauthn/server`'s own key parse and Python `runtime/execution/admission` `SettingsAdmission` reads the bundle through `cryptography.hazmat.primitives.serialization` `load_pem_*`, both decoding the one host-minted RFC-7468 bundle and re-minting no parallel PEM vocabulary per architecture#CROSS_LANGUAGE_WIRE. The `\n`-joined armor is the wire contract; the consumer-side `load_pem_*`/key-parse spellings are the peer branches' own admissions, not this page's fences.
