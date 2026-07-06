# [APPHOST_CONFIGURATION_AND_OPTIONS]

Configuration admission for the runtime spine: eight ranked `ConfigSource` rows mount every input onto one `ConfigurationManager` chain, a source-generated binder admits immutable policy records onto the `Validation<ConfigError,T>` rail, options validate once and publish frozen at ready, every change lands as a reload-class-gated `ReloadOutcome` transition carried by a `ReloadReceipt` — a structured operator edit arriving as an RFC-6902 `application/json-patch+json` document folds through `PatchSection` onto that same transition — and the operator kill-switch is one transition-class config row whose `OperatorOverride` union forces the degradation fold. The page owns the source axis with rank and reload-class columns, the `ConfigError` fault vocabulary, the reload transition family, and the operator forcing family; the credential-material lifecycle the `SecretsStore` row feeds is `Runtime/secrets`' concern, extending the frozen rank-40 mount from above, never a second source axis. The spine is Microsoft.Extensions.Configuration with its four provider packages, Microsoft.Extensions.Options, FluentValidation, NodaTime, Thinktecture.Runtime.Extensions, and LanguageExt.Core.

## [01]-[INDEX]

- [01]-[SOURCE_AXIS]: Eight ranked source rows with reload class and mount delegate.
- [02]-[TYPED_BINDING]: Fail-closed source-generated binding into validated policy records.
- [03]-[POLICY_VALUES]: Validate-once frozen publish with reload-class-gated receipted transitions.
- [04]-[KILL_SWITCH]: Operator override row forcing the degradation fold.

## [02]-[SOURCE_AXIS]

- Owner: `ConfigSource` `[SmartEnum<string>]` eight rows; `ReloadClass` `[SmartEnum<string>]` two rows; `ComparerAccessors.StringOrdinalIgnoreCase` accessor; `ConfigLayer` boot input record.
- Cases: json, user-settings, host-document, secrets-store, user-secrets, in-memory, env, cli — rank order is mount order, a later mount overrides earlier keys, and the rank fold is the whole precedence law.
- Entry: `Compose(IConfigurationManager manager, ConfigLayer layer, params ReadOnlySpan<ConfigSource> sources)` — `Fin<IConfigurationManager>` aborts on the first rejected mount.
- Packages: Microsoft.Extensions.Configuration, Microsoft.Extensions.Configuration.Json, Microsoft.Extensions.Configuration.EnvironmentVariables, Microsoft.Extensions.Configuration.CommandLine, Microsoft.Extensions.Configuration.UserSecrets, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one source row on `ConfigSource` (key, rank, reload class, mount delegate); one reload-trigger is one named trigger value on the existing `ReloadReceipt` trigger band, never a new source axis; zero new surface.
- Boundary: per-profile source selection and layering are computed from the resolved profile record at the composition root, never a second profile-keyed table here; `ConfigLayer` is the boundary capsule — `HostDocument` carries the HostAttachPort doc-user-text projection, `SecretsSource` carries the app-root-owned RID-dispatched credential-store `IConfigurationSource` gated on the SOURCE_ROUTES research row, whose file-backed fallback path resolves through `PathHelper.GetSecretsPathFromSecretsId` rather than a hand-built path, `ParentSnapshot` chains a companion onto its parent snapshot through `AddJsonStream` over the parent's serialized snapshot stream so an embedded or in-memory-stream layer mounts without a temp file, `UserSettingsPath` and `ContentRoot` arrive computed from the profile row; the inbox JSON provider parses JSONC (comments plus trailing commas) with zero added package; `ConfigurationKeyComparer` is the canonical path-segment order so a numeric array index sorts before a sibling string key; section paths travel as nameof-derived symbols, never call-site string literals; ambient `IConfiguration` reads past bootstrap are rejected.

```csharp signature

[SmartEnum<string>]
[ValidationError<ConfigError>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
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
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
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
- Cases: Text, SourceRejected, SectionAbsent, BindRejected, Scalar, Invariant, Aggregate — codes derive through `FaultBand.Config` (the registry's one legal multi-decade stride), `Combine` folds independent faults into Aggregate.
- Entry: `Bind<T>(IConfigurationRoot root, string section)` — `Validation<ConfigError,T>` accumulates; unknown keys fail closed through `ErrorOnUnknownConfiguration`.
- Packages: Microsoft.Extensions.Configuration.Binder, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one case on `ConfigError`; zero new surface.
- Boundary: `EnableConfigurationBindingGenerator` is a required project property on every options-binding app package — the generator intercepts `Get<T>` and `Bind` call sites with reflection-free binding; policy records are constructor-bound immutable records and `BindNonPublicProperties` is rejected; temporal scalars parse through `InstantPattern.ExtendedIso` and `DurationPattern.Roundtrip`, never culture-ambient parse; the binder exception channel is the named capture seam folded through `Try.lift` into the rail.

```csharp signature
[Union]
public abstract partial record ConfigError : Expected, IValidationError<ConfigError>, Semigroup<ConfigError> {
    private ConfigError(string detail, int code) : base(detail, code, None) { }

    public static ConfigError Create(string message) => new Text(message);

    public sealed record Text : ConfigError { public Text(string detail) : base(detail, FaultBand.Config.Code(0)) { } }
    public sealed record SourceRejected : ConfigError {
        public SourceRejected(string source, string detail) : base($"{source}: {detail}", FaultBand.Config.Code(1)) => Source = source;
        public string Source { get; }
    }
    public sealed record SectionAbsent : ConfigError {
        public SectionAbsent(string section) : base($"{section}: absent", FaultBand.Config.Code(2)) => Section = section;
        public string Section { get; }
    }
    public sealed record BindRejected : ConfigError {
        public BindRejected(string section, string detail) : base($"{section}: {detail}", FaultBand.Config.Code(3)) => Section = section;
        public string Section { get; }
    }
    public sealed record Scalar : ConfigError {
        public Scalar(string key, string detail) : base($"{key}: {detail}", FaultBand.Config.Code(4)) => Key = key;
        public string Key { get; }
    }
    public sealed record Invariant : ConfigError {
        public Invariant(string member, string detail) : base($"{member}: {detail}", FaultBand.Config.Code(5)) => Member = member;
        public string Member { get; }
    }
    public sealed record Aggregate : ConfigError {
        public Aggregate(Seq<ConfigError> faults) : base($"{faults.Count} faults", FaultBand.Config.Code(99)) => Faults = faults;
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
- Boundary: every options registration carries its `ReloadClass` row — frozen rows re-publish only through process restart and `RestartRequired` is that named path; interior code receives frozen records read once at ready, never `IOptions` handles, and per-call-site `OnChange` callbacks are rejected; `Observe` subscriptions return disposable detachers composed LIFO by the lifecycle owner; the POSIX `SIGHUP` route and the ControlService reload-options verb enqueue the same `ReloadOutcome` transition under `SignalTrigger` and `ControlTrigger` — `SignalTrigger` is Unix-only, registered through `PosixSignalRegistration.Create(PosixSignal.SIGHUP, ...)` which the runtime supports on Linux and macOS but not on a `win-*` RID where no `SIGHUP` exists, so a Windows host carries no signal route and reload arrives exclusively through `ControlTrigger`; cross-process reload propagation rides the op-log HLC cursor; named options key by smart-enum keys; FluentValidation owns cross-field invariants behind `Refine`, where the active rule set is itself a policy value admitted through `ValidationContext.CreateWithOptions` and `IncludeRuleSets` so a boundary variant runs its own rule subset, `When`/`Unless` gate a rule on a sibling-member predicate, `DependentRules` chains a rule block that runs only after its predecessors pass, and `ChildRules` validates an inline nested member graph without a second `IValidator` type, so a relational invariant across two policy fields is one rule expression rather than a hand-rolled post-bind check; `PolymorphicValidator` and `SetInheritanceValidator` route subtype policy records to their own graph, `WithState` carries a constructed `ConfigError` straight off the failure so `Refine` reads the typed fault before falling back to a `WithErrorCode`/`WithSeverity` code the `FaultBand.Config` registry row owns, and the flat `ToDictionary` re-derivation is the deleted form; a monitor-cache invalidation becomes a typed runtime transition through the polymorphic `Invalidate` over `TryRemove` and `Clear`, never an ambient re-read; `BindConfiguration(section, configureBinder)` rides `OptionsBuilderConfigurationExtensions` from Microsoft.Extensions.Options.ConfigurationExtensions, a lock-pinned transitive of the hosting closure, never a direct project asset; a per-tenant policy override is a named-options registration keyed by `TenantContext.Slug` through `Overlay` — the named instance binds the tenant overlay section `{section}:tenants:{slug}` over the base section so `IOptionsMonitor.Get(slug)` reads the tenant-overlaid record while the default name carries the single-tenant `Root` value, never a parallel tenant-config table, and the overlay change rides the same `ReloadClass.Transition` reload as the base section; a structured operator config edit arrives as an RFC-6902 `application/json-patch+json` document the `PatchSection` route applies to the live `{section}` `JsonObject` projection through the package's own `JsonPatchDocument.ApplyTo(JsonObject, logErrorAction)` over the `JsonObjectAdapter` — the `logErrorAction` delegate is the named capture seam folding each `JsonPatchError` into a `ConfigError.BindRejected` so a bad op-path lands the `ReloadOutcome.Rejected` carrying the typed fault while the prior values stay live, the applied `JsonObject` re-admits through the section-keyed `revalidate` closure the composition root registers per section — itself the composed `PolicyBinding.Bind<T>` + `Refine` for that section's policy type, so the patch route never names `T` at the verb seam and a patch that breaks an invariant never publishes — the whole apply gates on the section's `ReloadClass` so a `Frozen` section answers `RestartRequired` and only a `Transition` section re-publishes, and the transition stamps `ReloadReceipt.PatchTrigger` distinguishing it from the monitor, signal, and control triggers — a hand-rolled RFC-6902 operation dispatch and a Newtonsoft `JsonPatchDocument` are the deleted forms, the package owns the `op`/`path`/`from`/`value` operation model and the `Test`-op precondition assertion that fails the whole patch before any mutation lands.

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
                : failure.ErrorCode is { Length: > 0 } code && int.TryParse(code, out var coded) && FaultBand.OwnerOf(coded).Exists(static band => band == FaultBand.Config)
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
- Cases: ForceLevel, ForceFlagsOff, Release — ForceLevel carries a degradation row key as text, ForceFlagsOff carries the forced-off flag-key set the `Runtime/features#KILL_SWITCH_FOLD` `ForcesOff` predicate reads, Release withdraws the force.
- Entry: `From(KillSwitchConfig row, Instant at)` — total projection from the bound row into the forcing family.
- Packages: Microsoft.Extensions.Configuration.Binder, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: one case on `OperatorOverride`; zero new surface — the degradation fold and the features kill-switch fold each read their owning case and ignore the sibling's.
- Boundary: `KillSwitchConfig` binds at the `Section` symbol as a `ReloadClass.Transition` row, so an operator flip lands without restart; forced beats derived and Release re-derives inside the health-and-degradation fold, which also admits `Level` against the `DegradationLevel` row keys; the ControlService set-degradation verb is the service-modality wire route into the same union; the keyed manual breaker control on hops is the enforcement consequence at the hop registry.

```csharp signature
public sealed record KillSwitchConfig(string? ForcedLevel, string? ForcedFlagsOff, string? Reason) {
    public const string Section = nameof(KillSwitchConfig);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OperatorOverride {
    private OperatorOverride() { }

    public sealed record ForceLevel(string Level, string Reason, Instant At) : OperatorOverride;
    public sealed record ForceFlagsOff(FrozenSet<string> Flags, string Reason, Instant At) : OperatorOverride;
    public sealed record Release(string Reason, Instant At) : OperatorOverride;

    // The features KILL_SWITCH_FOLD predicate: only the flag-forcing case answers true, so the
    // degradation fold and the flag fold read one union without inspecting each other's cases.
    public bool ForcesOff(string flag) => this is ForceFlagsOff f && f.Flags.Contains(flag);

    public static OperatorOverride From(KillSwitchConfig row, Instant at) =>
        row.ForcedLevel is { Length: > 0 } level
            ? new ForceLevel(Level: level, Reason: row.Reason ?? string.Empty, At: at)
        : row.ForcedFlagsOff is { Length: > 0 } flags
            ? new ForceFlagsOff(
                flags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToFrozenSet(StringComparer.Ordinal),
                row.Reason ?? string.Empty, At: at)
            : new Release(Reason: row.Reason ?? string.Empty, At: at);
}
```

## [06]-[RESEARCH]

- [SOURCE_ROUTES]: the secrets-store provider route behind `ConfigLayer.SecretsSource` is RID-dispatched, never a single universal keychain — macOS resolves to Security.framework `SecItemCopyMatching` P/Invoke (from the Security.framework headers) versus a `/usr/bin/security` child process; Linux has no keychain and resolves to libsecret/`systemd-creds`/a file-backed `UserSecrets` store; Windows resolves to DPAPI/Credential Manager. The P/Invoke entrypoints are authored from the Apple/Linux man-pages and SDK headers and stay RESEARCH-flagged; no live `SecItem*`/DPAPI/credential-store read is performed during authoring because each raises an OS unlock dialog. The `Runtime/secrets#SECRET_LEASE` `SecretRuntime.Read` delegate acquires through whichever store the RID selected, never a parallel reader beside it.
- [SIGHUP_RELOAD]: the launchd and systemd `SIGHUP` reload-trigger delivery that re-mounts the transition-class sources and enqueues one `ReloadOutcome` under `ReloadReceipt.SignalTrigger` — the registration is `System.Runtime.InteropServices.PosixSignalRegistration.Create(PosixSignal.SIGHUP, ctx => ...)`, a Unix-only seam (Linux + macOS); a `win-*` host has no `SIGHUP` and routes reload solely through `ControlTrigger`, so the `SignalTrigger` band never fabricates a Windows signal path. The `PosixSignalRegistration.Create(PosixSignal, Action<PosixSignalContext>)`/`PosixSignal`/`PosixSignalContext.Cancel` member shape is catalogued at the substrate tier. The live delivery of `SIGHUP` under the running service manager (one reload, zero drains) is the open distinction the live service-manager host resolves.
- [BINDER_COVERAGE]: source-generated binder interception of `Get<T>` with `BinderOptions.ErrorOnUnknownConfiguration` under `EnableConfigurationBindingGenerator`.
- [PATCH_ERROR_SHAPE]: the `JsonPatchDocument.ApplyTo(object objectToApplyTo, Action<JsonPatchError> logErrorAction)` overload and the `JsonObjectAdapter` `JsonObject` mutation path are catalogued in `.api/api-jsonpatch.md`, and `Operations.Operation` carries the `path` field, and the `JsonPatchError.Operation` (typed `Operations.Operation?`)/`JsonPatchError.ErrorMessage` property spellings the `logErrorAction` delegate reads are catalogued beside it; the `application/json-patch+json` `IEndpointParameterMetadataProvider` intake the `DispatchPatch` control verb mounts is package-owned; the `op`/`path`/`from`/`value` operation model and the `Test`-op precondition assertion are package-owned and never re-spelled. The patch applies to a `JsonObject` projection of the live section rather than a typed `JsonPatchDocument<T>` so a `Test`-op precondition failure or an invalid op-path rejects the whole document before any re-bind, and the applied object re-enters through `PolicyBinding.Bind<T>`/`Refine` so a structurally valid but invariant-breaking patch never publishes.
