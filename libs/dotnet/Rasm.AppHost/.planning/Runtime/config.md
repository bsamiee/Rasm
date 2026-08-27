# [APPHOST_CONFIGURATION_AND_OPTIONS]

Configuration admission for the runtime spine: eight ranked `ConfigSource` rows mount every input onto one `ConfigurationManager` chain, a source-generated binder admits immutable policy records onto the standard `Validation<Error,T>`, options validate once and publish frozen at ready, every change lands as a reload-class-gated `ReloadOutcome`, and structured edits fold through `PatchSection` onto that outcome. `ConfigError` supplies typed leaves; LanguageExt `ManyErrors` carries plural refusals without a package-local aggregate case.

## [01]-[INDEX]

- [02]-[SOURCE_AXIS]: Eight ranked source rows with reload class, re-read capability, and mount delegate.
- [04]-[POLICY_VALUES]: Validate-once frozen publish with reload-class-gated outcomes.
- [05]-[KILL_SWITCH]: Operator override row forcing the degradation fold.

## [02]-[SOURCE_AXIS]

- Owner: `ConfigSource` `[SmartEnum<string>]` eight rows carrying the rank, the reload class, the provider's own re-read capability, and the mount delegate; `ReloadClass` `[SmartEnum<string>]` two rows; `ComparerAccessors.StringOrdinalIgnoreCase` accessor; `ConfigLayer` boot input record; `HostDocumentSource`/`HostDocumentProvider` the re-readable host-document provider pair.
- Cases: json, user-settings, host-document, secrets-store, user-secrets, in-memory, env, cli — rank order is mount order, a later mount overrides earlier keys, and the rank fold is the whole precedence law.
- Consumed: `ConfigKeys` is the rank-10 `appsettings` key roster this axis PUBLISHES for a reader elsewhere in the package to bind — one row per key a design page reads at a `Frozen` or `Transition` class, so a key a consumer resolves is a key a source declares and neither end carries a spelling the other never named.
- Entry: `Compose(IConfigurationManager manager, ConfigLayer layer, params ReadOnlySpan<ConfigSource> sources)` — `Fin<IConfigurationManager>` proves every row's class against its provider's re-read capability before mounting any of them and names EVERY mismatch in one refusal; `Transitional(params ReadOnlySpan<ConfigSource> sources)` projects the rank-ordered transition-class subset a reload re-reads.
- Packages: Microsoft.Extensions.Configuration, Microsoft.Extensions.Configuration.Json, Microsoft.Extensions.Configuration.EnvironmentVariables, Microsoft.Extensions.Configuration.CommandLine, Microsoft.Extensions.Configuration.UserSecrets, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one source row on `ConfigSource` (key, rank, reload class, re-read capability, mount delegate); zero new surface.
- Boundary: per-profile source selection and layering are computed from the resolved profile record at the composition root, never a second profile-keyed table here; the reload class is a CLAIM about the provider a row mounts, so `Compose` proves the pair before mounting — a `Transition` row over a snapshot provider promises a re-read the root's own `Reload()` structurally cannot deliver, and a `Frozen` row over a watching provider re-admits at runtime the material a running process pinned at boot, so the two-column agreement is a composition refusal rather than a comment; `HostDocument` is the row that proof caught — an `AddInMemoryCollection` snapshot cannot answer its declared `Transition` class, so the row mounts `HostDocumentSource`, whose provider re-invokes the layer's projection on `Load()` and raises its own reload token when the host signals a document change, which is also the one consumer the host-attach document-changed row owed; `ConfigLayer` is the boundary capsule — `HostDocument` carries the HostAttachPort doc-user-text projection and `HostDocumentWatch` the subscription the provider arms on it, `SecretsSource` carries the app-root-owned credential-store `IConfigurationSource`, RID-dispatched because no universal keychain exists (macOS the in-process Security.framework `SecItemCopyMatching` adapter, never a `/usr/bin/security` child process, so the read stays inside the host for parity with the launchd adapter; Linux, having no keychain, libsecret or `systemd-creds` or the file-backed `UserSecrets` store; Windows DPAPI Credential Manager), with the `Runtime/secrets#SECRET_LEASE` `SecretRuntime.Read` delegate acquiring through whichever store the RID selected so the suite carries one credential reader and never a second beside it, and the file-backed path resolving through `PathHelper.GetSecretsPathFromSecretsId` rather than a hand-built path, `ParentSnapshot` chains a companion onto its parent snapshot through `AddJsonStream` over the parent's serialized snapshot stream so an embedded or in-memory-stream layer mounts without a temp file, `UserSettingsPath` and `ContentRoot` arrive computed from the profile row; the inbox JSON provider parses JSONC (comments plus trailing commas) with zero added package; `ConfigurationKeyComparer` is the canonical path-segment order so a numeric array index sorts before a sibling string key; section paths travel as nameof-derived symbols, never call-site string literals; ambient `IConfiguration` reads past bootstrap are rejected; a key a package member resolves lands as a `ConfigKeys` row in the same change that lands the read, because a governance row binding a key no source publishes and a reader resolving a key no roster names fail the same way — silently, with a default standing in for a value nobody set — so the roster is the audit surface both ends prove against and a row is DELETED only when its reader is.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[ValidationError]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ReloadClass {
    public static readonly ReloadClass Frozen = new("frozen");
    public static readonly ReloadClass Transition = new("transition");
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ConfigKeyRow(string Section, string Name, ReloadClass Reload) {
    public string Path => $"{Section}:{Name}";
}

public static class ConfigKeys {
    public static readonly ConfigKeyRow FeedStable = new(nameof(FeedBinding), "stable", ReloadClass.Frozen);
    public static readonly ConfigKeyRow FeedBeta = new(nameof(FeedBinding), "beta", ReloadClass.Frozen);
    public static readonly ConfigKeyRow FeedCanary = new(nameof(FeedBinding), "canary", ReloadClass.Frozen);

    public static readonly Seq<ConfigKeyRow> Published = [FeedStable, FeedBeta, FeedCanary];
}

public sealed record ConfigLayer(
    string ContentRoot,
    string ProfileKey,
    string UserSettingsPath,
    Seq<string> Args,
    FrozenDictionary<string, string> Switches,
    Assembly SecretsAnchor,
    Func<IEnumerable<KeyValuePair<string, string?>>> HostDocument,
    Func<Action, IDisposable> HostDocumentWatch,
    Func<IConfigurationSource> SecretsSource,
    Seq<KeyValuePair<string, string?>> Seed,
    Option<IConfiguration> ParentSnapshot);

public sealed class HostDocumentProvider : ConfigurationProvider, IDisposable {
    readonly ConfigLayer layer;
    readonly IDisposable watch;

    public HostDocumentProvider(ConfigLayer layer) {
        this.layer = layer;
        watch = layer.HostDocumentWatch(() => { Load(); OnReload(); });
    }

    public override void Load() =>
        Data = layer.HostDocument().ToDictionary(
            static row => row.Key, static row => row.Value, StringComparer.OrdinalIgnoreCase);

    public void Dispose() => watch.Dispose();
}

public sealed record HostDocumentSource(ConfigLayer Layer) : IConfigurationSource {
    public IConfigurationProvider Build(IConfigurationBuilder builder) => new HostDocumentProvider(Layer);
}

[SmartEnum<string>]
[ValidationError]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ConfigSource {
    public const string EnvPrefix = "RASM_";

    // --- [ROWS]
    public static readonly ConfigSource Json = new("json", rank: 10, reload: ReloadClass.Transition, rereads: true, MountJson);
    public static readonly ConfigSource UserSettings = new("user-settings", rank: 20, reload: ReloadClass.Transition, rereads: true, MountUserSettings);
    public static readonly ConfigSource HostDocument = new("host-document", rank: 30, reload: ReloadClass.Transition, rereads: true, MountHostDocument);
    public static readonly ConfigSource SecretsStore = new("secrets-store", rank: 40, reload: ReloadClass.Frozen, rereads: false, MountSecretsStore);
    public static readonly ConfigSource UserSecrets = new("user-secrets", rank: 50, reload: ReloadClass.Frozen, rereads: false, MountUserSecrets);
    public static readonly ConfigSource InMemory = new("in-memory", rank: 60, reload: ReloadClass.Frozen, rereads: false, MountInMemory);
    public static readonly ConfigSource Env = new("env", rank: 70, reload: ReloadClass.Frozen, rereads: false, MountEnv);
    public static readonly ConfigSource Cli = new("cli", rank: 80, reload: ReloadClass.Frozen, rereads: false, MountCli);

    public int Rank { get; }

    public ReloadClass Reload { get; }

    public bool Rereads { get; }

    [UseDelegateFromConstructor]
    public partial IConfigurationBuilder Mount(IConfigurationBuilder builder, ConfigLayer layer);

    public static Seq<ConfigSource> Transitional(params ReadOnlySpan<ConfigSource> sources) =>
        Ranked(sources).Filter(static row => row.Reload == ReloadClass.Transition);

    public static Fin<IConfigurationManager> Compose(IConfigurationManager manager, ConfigLayer layer, params ReadOnlySpan<ConfigSource> sources) =>
        Ranked(sources) switch {
            var ranked => ranked.Filter(static row => (row.Reload == ReloadClass.Transition) != row.Rereads) is { IsEmpty: false } mismatched
                ? Fin.Fail<IConfigurationManager>(Error.Many(mismatched.Map(static row =>
                    (Error)new ConfigError.SourceRejected(row.Key, $"reload={row.Reload.Key} rereads={row.Rereads}"))))
                : Try.lift(() => Fin.Succ(ranked.Fold(
                        layer.ParentSnapshot.Map(parent => ((IConfigurationBuilder)manager).AddConfiguration(parent)).IfNone(manager),
                        (builder, row) => row.Mount(builder, layer)))).Run().Bind(static inner => inner)
                    .Map(_ => manager),
        };

    static Seq<ConfigSource> Ranked(ReadOnlySpan<ConfigSource> sources) => toSeq(sources.ToArray().OrderBy(static row => row.Rank));

    private static IConfigurationBuilder MountJson(IConfigurationBuilder builder, ConfigLayer layer) =>
        builder
            .AddJsonFile(Path.Combine(layer.ContentRoot, "appsettings.json"), optional: false, reloadOnChange: true)
            .AddJsonFile(Path.Combine(layer.ContentRoot, $"appsettings.{layer.ProfileKey}.json"), optional: true, reloadOnChange: true);

    private static IConfigurationBuilder MountUserSettings(IConfigurationBuilder builder, ConfigLayer layer) =>
        builder.AddJsonFile(layer.UserSettingsPath, optional: true, reloadOnChange: true);

    private static IConfigurationBuilder MountHostDocument(IConfigurationBuilder builder, ConfigLayer layer) =>
        builder.Add(new HostDocumentSource(layer));

    private static IConfigurationBuilder MountSecretsStore(IConfigurationBuilder builder, ConfigLayer layer) =>
        builder.Add(layer.SecretsSource());

    private static IConfigurationBuilder MountUserSecrets(IConfigurationBuilder builder, ConfigLayer layer) =>
        builder.AddUserSecrets(layer.SecretsAnchor, optional: true, reloadOnChange: false);

    private static IConfigurationBuilder MountInMemory(IConfigurationBuilder builder, ConfigLayer layer) =>
        builder.AddInMemoryCollection(layer.Seed);

    private static IConfigurationBuilder MountEnv(IConfigurationBuilder builder, ConfigLayer layer) =>
        builder.AddEnvironmentVariables(prefix: EnvPrefix);

    private static IConfigurationBuilder MountCli(IConfigurationBuilder builder, ConfigLayer layer) =>
        builder.AddCommandLine([.. layer.Args], layer.Switches.ToDictionary(StringComparer.Ordinal));
}
```

## [03]-[TYPED_BINDING]

- Owner: `PolicyBinding` owns admission; `ConfigError` is the direct `[FaultCase]` family on `FaultBand.Config`.
- Cases: `SourceRejected | SectionAbsent | BindRejected | Scalar | Invariant`; plural refusal uses LanguageExt `ManyErrors` on `Validation<Error,T>`.
- Entry: `Bind<T>(IConfigurationRoot root, string section)` — `Validation<Error,T>` accumulates; unknown keys fail closed through `ErrorOnUnknownConfiguration`.
- Packages: Microsoft.Extensions.Configuration.Binder, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one case on `ConfigError`; zero new surface.
- Boundary: `EnableConfigurationBindingGenerator` is a required project property on every options-binding app package — the generator intercepts `Get<T>` and `Bind` call sites with reflection-free binding; policy records are constructor-bound immutable records and `BindNonPublicProperties` is rejected; temporal scalars parse through `InstantPattern.ExtendedIso` and `DurationPattern.Roundtrip`, never culture-ambient parse; the binder exception channel folds through `Try.lift` without message reminting.

```csharp
// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ConfigError : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Config;
    private ConfigError(string detail) => Detail = detail;
    public string Detail { get; }
    public sealed override string Message => Detail;

    [FaultCase(0)]
    public sealed partial record SourceRejected : ConfigError {
        public SourceRejected(string source, string detail) : base($"{source}: {detail}") => Source = source;
        public string Source { get; }
    }
    [FaultCase(1)]
    public sealed partial record SectionAbsent : ConfigError {
        public SectionAbsent(string section) : base($"{section}: absent") => Section = section;
        public string Section { get; }
    }
    [FaultCase(2)]
    public sealed partial record BindRejected(string Section, Error Cause)
        : ConfigError($"{Section}: {Cause.Message}"), ICausedFault;
    [FaultCase(3)]
    public sealed partial record Scalar : ConfigError {
        public Scalar(string key, string detail) : base($"{key}: {detail}") => Key = key;
        public string Key { get; }
    }
    [FaultCase(4)]
    public sealed partial record Invariant : ConfigError {
        public Invariant(string member, string detail) : base($"{member}: {detail}") => Member = member;
        public string Member { get; }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class PolicyBinding {
    public static Validation<Error, T> Bind<T>(IConfigurationRoot root, string section) where T : notnull =>
        Try.lift(() => Fin.Succ(Optional(
                root.GetSection(section).Get<T>(static binder => binder.ErrorOnUnknownConfiguration = true)))).Run().Bind(static inner => inner)
            .MapFail(error => (Error)new ConfigError.BindRejected(section, error))
            .Bind(configured => configured.ToFin((Error)new ConfigError.SectionAbsent(section)))
            .ToValidation();

    public static Validation<Error, Instant> BindInstant(IConfigurationRoot root, string key) =>
        Admit(InstantPattern.ExtendedIso, root, nameof(InstantPattern.ExtendedIso));

    public static Validation<Error, Duration> BindDuration(IConfigurationRoot root, string key) =>
        Admit(DurationPattern.Roundtrip, root, nameof(DurationPattern.Roundtrip));

    static Validation<Error, T> Admit<T>(IPattern<T> pattern, IConfigurationRoot root, string key, string named) =>
        pattern.Parse(root.GetValue<string>() ?? string.Empty) is { Success: true } parsed
            ? Success<Error, T>(parsed.Value)
            : Fail<Error, T>(new ConfigError.Scalar($"text outside {named}"));
}
```

## [04]-[POLICY_VALUES]

- Owner: `OptionsAdmission` static registration surface; `ReloadGate` the per-section generation cell; `ReloadFold` the one coalescing reload entry every trigger reaches; `ReloadOutcome` `[Union]` carrying its own case-key projection.
- Cases: Applied, Unchanged, RestartRequired, Rejected — Applied re-publishes frozen policy values, Unchanged records a no-diff publish, RestartRequired is the frozen-row path, Rejected carries the `ConfigError` of a failed re-validation while the prior values stay live.
- Entry: `Admit<T>(IServiceCollection services, string section)` — composition registration; `ReloadFold.Fire(ReloadGate gate, IConfigurationRoot root, string section, ReloadClass reload, ...)` is the ONE reload entry the monitor fan, the signal route, the control verb, and the patch route all reach, returning `Option<ReloadOutcome>` so a redundant fire leaves nothing behind; `Refine` accumulates on `Validation<Error,T>` against the active rule set, `Sweep` aborts on `Fin<Unit>`.
- Auto: generated `[OptionsValidator]` validators with `[ValidateObjectMembers]` and `[ValidateEnumeratedItems]` own structural validation; `ValidateOnStart` plus the `IStartupValidator` sweep prove every registered policy record before ready; `PostConfigure` derives a dependent policy value after binding, and `Invalidate` is the one polymorphic cache cut — a named entry routes to `IOptionsMonitorCache.TryRemove`, an absent name routes to `Clear` so the whole set re-binds, never two named operations; a section's generation is the kernel `CanonicalWriter.Sorted` fold over its own admitted pairs — the writer publishes the `ConfigurationKeyComparer` order, so no caller sorts beside it, and `Optional` presence-frames an unset value so an absent key never aliases an empty one; the provider fan collapses at `ReloadGate` before any outcome publishes, because one measured `IConfigurationRoot.Reload()` over the landed source shape fires the root token three times and the per-name `OnChange` callbacks six — `FileConfigurationProvider.Load` ends with an unconditional `OnReload()` so every mounted file provider raises beside the explicit change, and `OnChange` fires once per registered options name per root fire — so a one-outcome-per-signal claim without a gate is a claim the platform contradicts by construction.
- Packages: Rasm (kernel `ContentHash.Of` with `CanonicalWriter.Sorted`/`Optional`), Microsoft.Extensions.Options, Microsoft.AspNetCore.JsonPatch.SystemTextJson, FluentValidation, NodaTime, LanguageExt.Core
- Growth: one case on `ReloadOutcome`; one config-boundary variant is one rule-set name through `IncludeRuleSets`, never a second validator; a new per-tenant policy override is one `Overlay` named-options registration keyed by `TenantContext.Slug`, never a second options surface; a structured partial config edit is one RFC-6902 `application/json-patch+json` document folded through `PatchSection` onto the same `ReloadOutcome`, never a second mutation path; zero new surface.
- Boundary: every options registration carries its `ReloadClass` row — frozen rows re-publish only through process restart and `RestartRequired` is that named path; interior code receives frozen records read once at ready, never `IOptions` handles, and per-call-site `OnChange` callbacks are rejected; `Observe` subscriptions return disposable detachers composed LIFO by the lifecycle owner; the POSIX `SIGHUP` route enqueues its `ReloadOutcome` through `ReloadFold.Fire`; `SIGHUP` is registered on every RID rather than Unix alone, because `PosixSignal.SIGHUP` carries no `[UnsupportedOSPlatform]` and the runtime maps it to the Windows console-close event, so a Windows host takes an explicit platform gate at the registration site; the SIGHUP handler dispatches on the ThreadPool rather than the dedicated signal thread SIGINT, SIGQUIT, and SIGTERM get, and a service manager applies its start timeout to a reload window, so a saturated pool is a reload that never completes with stale configuration still live — the fold therefore hands the manager's reload bracket its own outcome rather than parking inside the handler; cross-process reload propagation rides the op-log HLC cursor; named options key by smart-enum keys; FluentValidation owns cross-field invariants behind `Refine`, where the active rule set is itself a policy value admitted through `ValidationContext.CreateWithOptions` and `IncludeRuleSets` so a boundary variant runs its own rule subset, `When`/`Unless` gate a rule on a sibling-member predicate, `DependentRules` chains a rule block that runs only after its predecessors pass, and `ChildRules` validates an inline nested member graph without a second `IValidator` type, so a relational invariant across two policy fields is one rule expression rather than a hand-rolled post-bind check; `PolymorphicValidator` and `SetInheritanceValidator` route subtype policy records to their own graph, `WithState` carries a constructed `ConfigError` straight off the failure so `Refine` reads the typed fault before falling back to a `WithErrorCode`/`WithSeverity` code the `FaultBand.Config` registry row owns, and the flat `ToDictionary` re-derivation is the deleted form; a monitor-cache invalidation becomes a typed runtime transition through the polymorphic `Invalidate` over `TryRemove` and `Clear`, never an ambient re-read; `BindConfiguration(section, configureBinder)` rides `OptionsBuilderConfigurationExtensions` from Microsoft.Extensions.Options.ConfigurationExtensions, a lock-pinned transitive of the hosting closure, never a direct project asset; a per-tenant policy override is a named-options registration keyed by `TenantContext.Slug` through `Overlay` — the named instance binds the tenant overlay section `{section}:tenants:{slug}` over the base section so `IOptionsMonitor.Get(slug)` reads the tenant-overlaid record while the default name carries the single-tenant `Root` value, never a parallel tenant-config table, and the overlay change rides the same `ReloadClass.Transition` reload as the base section; a structured operator config edit arrives as an RFC-6902 `application/json-patch+json` document the `PatchSection` route applies to a CLONE of the live `{section}` `JsonObject` projection through the package's own `JsonPatchDocument.ApplyTo(JsonObject, logErrorAction)` over the `JsonObjectAdapter` — the `logErrorAction` delegate is the named capture boundary folding each cause-less `JsonPatchError` into `ConfigError.Invariant`, while a foreign bind failure enters `ConfigError.BindRejected` with its exact `Error` cause; only an admitted candidate leaves the member for the caller to swap in, the candidate re-admitting through the section-keyed `revalidate` closure the composition root registers per section — itself the composed `PolicyBinding.Bind<T>` + `Refine` for that section's policy type, so the patch route never names `T` at the verb boundary and a patch that breaks an invariant never publishes — the whole apply gates on the section's `ReloadClass` so a `Frozen` section answers `RestartRequired` and only a `Transition` section re-publishes — a hand-rolled RFC-6902 operation dispatch and a Newtonsoft `JsonPatchDocument` are the deleted forms, the package owns the `op`/`path`/`from`/`value` operation model and the `Test`-op precondition assertion that fails the whole patch before any mutation lands.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReloadOutcome {
    private ReloadOutcome() { }

    public sealed record Applied(string Section) : ReloadOutcome;
    public sealed record Unchanged(string Section) : ReloadOutcome;
    public sealed record RestartRequired(string Section) : ReloadOutcome;
    public sealed record Rejected(string Section, ConfigError Fault) : ReloadOutcome;

    public string Key => Map(
        applied: nameof(Applied),
        unchanged: nameof(Unchanged),
        restartRequired: nameof(RestartRequired),
        rejected: nameof(Rejected));
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ReloadGate(Atom<HashMap<string, UInt128>> Published) {
    public static ReloadGate Live() => new(Atom(HashMap<string, UInt128>()));

    public static UInt128 Generation(IConfigurationRoot root, string section) =>
        ContentHash.Of(
            toSeq(root.GetSection(section).AsEnumerable(makePathsRelative: true)),
            static (rows, writer) => writer.Sorted(
                rows, static row => row.Key, ConfigurationKeyComparer.Instance,
                static (row, inner) => inner.String(row.Key)
                    .Optional(Optional(row.Value), static (text, framed) => framed.String(text))));

    public Option<UInt128> Admit(string section, UInt128 generation) {
        var admitted = Option<UInt128>.None;
        ignore(Published.SwapMaybe(held => {
            admitted = None;
            return held.Find(section) == Some(generation)
                ? Option<HashMap<string, UInt128>>.None
                : Some((admitted = Some(generation), held.AddOrUpdate(section, generation)).Item2);
        }));
        return admitted;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ReloadFold {
    public static Option<ReloadOutcome> Fire(
        ReloadGate gate, IConfigurationRoot root, string section, ReloadClass reload,
        Func<ReloadOutcome> republish) =>
        gate.Admit(section, ReloadGate.Generation(root, section))
            .Map(_ => reload.Switch(
                state: (Section: section, Republish: republish),
                frozen: static gated => (ReloadOutcome)new ReloadOutcome.RestartRequired(gated.Section),
                transition: static gated => gated.Republish()));
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

    public static Validation<Error, T> Refine<T>(T policy, IValidator<T> validator, Option<Seq<string>> ruleSets = default) where T : notnull =>
        (ruleSets is { IsSome: true, Case: Seq<string> sets }
                ? validator.Validate(ValidationContext<T>.CreateWithOptions(policy, options => options.IncludeRuleSets([.. sets])))
                : validator.Validate(policy))
            .Errors.AsIterable()
            .Map(static failure => failure.CustomState is ConfigError carried
                ? (Error)carried
                : failure.ErrorCode is { Length: > 0 } code && int.TryParse(code, out var coded) && FaultBand.OwnerOf(kind: BandKind.Fault, code: coded).Exists(static band => band == FaultBand.Config)

                    ? (Error)new ConfigError.Scalar(failure.PropertyName, failure.ErrorMessage)
                    : new ConfigError.Invariant(failure.PropertyName, failure.ErrorMessage))
            .ToSeq() is { IsEmpty: false } faults
            ? Error.Many(faults)
            : (Validation<Error, T>)policy;

    public static Fin<Unit> Sweep(IStartupValidator validator) => Try.lift(validator.Validate).Run().Bind(static inner => inner);

    public static Unit Invalidate<T>(IOptionsMonitorCache<T> cache, Option<string> name = default) where T : class =>
        name is { IsSome: true, Case: string named } ? ignore(cache.TryRemove(named)) : (cache.Clear(), unit).Item2;

    public static Validation<Error, (JsonObject Admitted, ReloadOutcome Outcome)> PatchSection(JsonObject live, string section, ReloadClass reload, JsonPatchDocument patch, Func<JsonObject, Validation<Error, Unit>> revalidate) =>
        reload == ReloadClass.Frozen
            ? (live, (ReloadOutcome)new ReloadOutcome.RestartRequired(section))
            : Try.lift(() => {
                    var faults = new List<ConfigError>();
                    var candidate = live.DeepClone().AsObject();
                    patch.ApplyTo(candidate, error => faults.Add(new ConfigError.Invariant(
                        error.Operation?.path ?? section, error.ErrorMessage)));
                    return Fin.Succ((Candidate: candidate, Faults: toSeq(faults)));
                }).Run().Bind(static inner => inner)
                .Match(
                    Succ: applied => applied.Faults is { IsEmpty: false } ops
                        ? (Validation<Error, (JsonObject, ReloadOutcome)>)Error.Many(ops.Map(static fault => (Error)fault))
                        : revalidate(applied.Candidate).Map(_ => (applied.Candidate, (ReloadOutcome)new ReloadOutcome.Applied(section))),
                    Fail: error => Fail<Error, (JsonObject, ReloadOutcome)>(error));

    public static Option<IDisposable> Observe<T>(
        IOptionsMonitor<T> monitor, ReloadGate gate, IConfigurationRoot root, string section, ReloadClass reload,
        Func<T, ReloadOutcome> republish, Action<ReloadOutcome> publish) where T : class =>
        Optional(monitor.OnChange((snapshot, _) =>
            ReloadFold.Fire(gate, root, section, reload, () => republish(snapshot))
                .Iter(publish)));
}
```

## [05]-[KILL_SWITCH]

- Owner: `KillSwitchConfig` config row record; `OperatorOverride` `[Union]` forcing family.
- Cases: ForceLevel, ForceFlagsOff, Release — ForceLevel carries a degradation row key as text, ForceFlagsOff carries the forced-off flag-key set the `Runtime/features#KILL_SWITCH_FOLD` `ForcesOff` predicate reads, Release withdraws the force; each carries `Option<string> Reason`, so an operator who gave none is distinguishable from one who typed an empty line.
- Entry: `From(KillSwitchConfig row, Instant at)` — total projection from the bound row into the forcing family.
- Packages: Microsoft.Extensions.Configuration.Binder, NodaTime, Thinktecture.Runtime.Extensions, BCL inbox
- Growth: one case on `OperatorOverride`; zero new surface — the degradation fold and the features kill-switch fold each read their owning case and ignore the sibling's.
- Boundary: `KillSwitchConfig` is the binder's own nullable target and `From` its ONE reader, so every nullable admits at that boundary and the forcing family carries `Option` alone — an empty-string reason standing in for an absent one is the deleted form; the row binds at the `Section` symbol as a `ReloadClass.Transition` row, so an operator flip lands without restart; forced beats derived and Release re-derives inside the health-and-degradation fold, which also admits `Level` against the `DegradationLevel` row keys; the ControlService set-degradation verb is the service-modality wire route into the same union; the keyed manual breaker control on hops is the enforcement consequence at the hop registry.

```csharp
// --- [BOUNDARIES] ----------------------------------------------------------------------
public sealed record KillSwitchConfig(string? ForcedLevel, string? ForcedFlagsOff, string? Reason) {
    public const string Section = nameof(KillSwitchConfig);
}

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OperatorOverride {
    private OperatorOverride() { }

    public sealed record ForceLevel(string Level, Option<string> Reason, Instant At) : OperatorOverride;
    public sealed record ForceFlagsOff(FrozenSet<string> Flags, Option<string> Reason, Instant At) : OperatorOverride;
    public sealed record Release(Option<string> Reason, Instant At) : OperatorOverride;

    public bool ForcesOff(string flag) => this is ForceFlagsOff f && f.Flags.Contains(flag);

    public static OperatorOverride From(KillSwitchConfig row, Instant at) =>
        Optional(row.Reason).Filter(static text => !string.IsNullOrWhiteSpace(text)) switch {
            var reason => row.ForcedLevel is { Length: > 0 } level
                ? new ForceLevel(Level: level, Reason: reason, At: at)
            : row.ForcedFlagsOff is { Length: > 0 } flags
                ? new ForceFlagsOff(
                    flags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToFrozenSet(StringComparer.Ordinal),
                    reason, At: at)
                : new Release(Reason: reason, At: at),
        };
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
