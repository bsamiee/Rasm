# [PERSISTENCE_ELEMENT_AUTHORITY]

Rasm.Persistence gates every durable object interaction through one object-ACL authorization algebra — a TOTAL, pure set-algebra with zero `KmsProvider` and zero store operation, split cleanly from the crypto custody tier (`Element/identity#KMS_CUSTODY` owns signing and DEK envelopes; this page owns WHO MAY). `Grant` is the one `[SmartEnum<string>]` object-authorization vocabulary — object, audit, and branch rights in a single wire-keyed set — whose combinable membership is the `GrantSet` frozen-set value with superuser-aware `Admits` containment; `AclScope` is the gated-object-kind altitude ladder (`tenant ← branch ← document ← element-set`) whose `Parent` invariant rejects a mis-stacked inheritance chain; `AclEntry` is the per-subject allow/deny grant with provenance and a `[From, Until)` window; `ObjectAcl` the owner-plus-inherited grant chain; `Authority.Admit` the one deny-over-allow fold producing the closed `AuthDecision` authz verdict (`Granted`/`Denied`/`ScopeMismatch`/`Expired` — the authz half of the fissioned decision union; the crypto half lives on identity as `CustodyVerdict`). Every actor slot is the Persistence-owned `Element/graph#STORE_RAIL` `StoreActor` (subject + role claims) — never an AppHost `Principal`. The `Version/commits#COMMIT_DAG` `BranchRef` grant is this same `GrantSet` narrowed under `AclScope.Branch`, never a parallel branch enum; the `Element/identity#ELEMENT_IDENTITY` `Tenant` RLS column is the coarse partition and this object ACL the fine within-tenant grant, two altitudes never duplicated. `ObjectAcl` is the frozen vocabulary its consumers import — `ElementIdentity.Acl` persists it as the jsonb column and the AppHost identity-store PORT decodes it — so the split never forks the type. Faults: NONE — the algebra is total over its closed verdicts; a persistence failure around an ACL row rails the composed `Element/identity#SCHEMA_VERDICT` `IdentityFault` band (8340), never a new band.

## [01]-[INDEX]

- [02]-[GRANT_ALGEBRA]: the `Grant` vocabulary, the `GrantSet` frozen-set value, superuser containment, and the set-equality hashing boundary.
- [03]-[OBJECT_ACL]: the `AclScope` altitude ladder, the windowed `AclEntry`, the owner-plus-inherited `ObjectAcl` chain, and the ladder invariant.
- [04]-[AUTHORITY]: the deny-over-allow `Effective` fold, the lapsed-grant probe, the one `Admit` entry over the closed `AuthDecision` verdict, and the `Inequalities`-fed grant-shift audit diff.

## [02]-[GRANT_ALGEBRA]

- Owner: `Grant` the one `[SmartEnum<string>]` object-authorization vocabulary every ACL entry draws from — wire-keyed rows, because the membership CROSSES the wire: the `Element/identity#ELEMENT_IDENTITY` jsonb `Acl` column persists it under `ElementJson` and the AppHost identity-store PORT round-trips it, so each grant travels as its bare key through the generated Thinktecture converters with zero hand codec (a keyless row would strand the jsonb round-trip); `GrantSet` the `[Equatable]` frozen-set value carrying the `[SetEquality]` `Grant` set, the value-derived `Owner` superset, and the set-algebra `Admits`/`Union`/`Without` operators.
- Cases: `Read`, `Write`, `Delete`, `Share`, `Revoke` (object), `Audit` (the blame/audit-log lane), `Merge`, `Rebase`, `ForcePush` (branch), `Admin` (the explicit superuser bit) — `GrantSet.Owner` is `Grant.Items.ToFrozenSet()`, the full vocabulary frozen once off the generated `Items` surface so a new `Grant` row joins the owner superset with zero edits.
- Entry: `GrantSet.Of(params ReadOnlySpan<Grant>)` mints a membership value; `Admits(GrantSet demand)` is superuser-aware containment (an `Admin`-bearing set admits any demand); `Union`/`Without` are the allow/deny fold primitives the `#AUTHORITY` fold composes.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + generated `Items`/`Switch` + the wire converters `Element/codec#CODEC_AXIS` registers once), Generator.Equals (`[Equatable]` + `[SetEquality]` — `ISet<T>.SetEquals` fast path), System.Collections.Frozen (`FrozenSet` `Union`/`Except`/`IsSubsetOf`/`Contains`), LanguageExt.Core, BCL inbox.
- Growth: one `Grant` row per new permission — `Owner` absorbs it value-derived, `Admits` and the fold are membership-generic, so a new right is ONE static line; zero new surface — a `[Flags]` enum bitfield (the `shapes.md` `ReplaceFlags` law), a second branch-only grant enum, or a per-right boolean column is the deleted form.
- Boundary: `Grant` is distinct in name and concept from the AppHost `Agent/capability#CAPABILITY_REGISTRY` effect-gating `Capability` (`LocalCompute`/`StoreRead`/`StoreWrite`/`RemoteCompute`) — the two never share a name across the strata; `[SetEquality]` equality is exact set membership BUT its hash contribution is ALWAYS ZERO (`SetEqualityComparer<T>.GetHashCode` returns 0), so a `GrantSet` must NEVER key a dictionary, `HashMap`, or `HashSet` — every `GrantSet` collides into bucket 0 and a set-keyed lookup degrades to a linear scan; `ObjectAcl` therefore keys its entries by the string SUBJECT and carries the `GrantSet` as the VALUE, the safe fold shape; equality-compare and `Admits`-fold freely — only hashing is off-limits.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Collections.Frozen;
using Generator.Equals;
using LanguageExt;
using NodaTime;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Element;

// --- [TYPES] ---------------------------------------------------------------------------
// The object-authorization vocabulary: wire-keyed rows — the jsonb `ElementIdentity.Acl` column and the
// AppHost identity-store PORT round-trip each grant as its bare key through the generated Thinktecture
// converters (zero hand codec). Distinct from the AppHost effect-gating `Capability`. Combinable authority
// is the GrantSet frozen-set value below, NOT a [Flags] enum (the ReplaceFlags law).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Grant {
    public static readonly Grant Read = new("read");
    public static readonly Grant Write = new("write");
    public static readonly Grant Delete = new("delete");
    public static readonly Grant Share = new("share");
    public static readonly Grant Revoke = new("revoke");
    public static readonly Grant Audit = new("audit");
    public static readonly Grant Merge = new("merge");
    public static readonly Grant Rebase = new("rebase");
    public static readonly Grant ForcePush = new("force-push");
    public static readonly Grant Admin = new("admin");
}

// Combinable authority as set algebra (ReplaceFlags): the frozen Grant set, value-equal by SET membership through
// Generator.Equals `[SetEquality]` (NOT a Thinktecture key owner — the two equality models never stack; a
// multi-member structural value is `[Equatable]`), so two sets with the same grants compare equal regardless of
// order. HASH-0 BOUNDARY: set-equality members contribute 0 to `GetHashCode`, so a GrantSet is NEVER a
// dictionary/HashMap/HashSet KEY — key by subject string, carry the set as the value. `Owner` is the
// value-derived full vocabulary; `Admits` is superuser-aware containment (an Admin-bearing set admits any
// demand); union/difference are the allow/deny fold primitives.
[Equatable]
public sealed partial record GrantSet([property: SetEquality] FrozenSet<Grant> Grants) {
    public static readonly GrantSet None = new(FrozenSet<Grant>.Empty);
    public static readonly GrantSet Owner = new(Grant.Items.ToFrozenSet());

    public static GrantSet Of(params ReadOnlySpan<Grant> grants) => new(grants.ToArray().ToFrozenSet());
    public GrantSet Union(GrantSet other) => new(Grants.Union(other.Grants).ToFrozenSet());
    public GrantSet Without(GrantSet other) => new(Grants.Except(other.Grants).ToFrozenSet());
    public bool Admits(GrantSet demand) => Grants.Contains(Grant.Admin) || demand.Grants.IsSubsetOf(Grants);
}
```

## [03]-[OBJECT_ACL]

- Owner: `AclScope` the `[SmartEnum<string>]` gated-object-kind vocabulary carrying its altitude `Parent`; `AclEntry` the per-subject allow/deny grant record with provenance (`GrantedBy` the Persistence-owned `StoreActor`) and a `[From, Until)` validity window; `ObjectAcl` the owner-plus-inherited grant chain keyed by string subject with the `LadderValid` chain invariant.
- Cases: `AclScope` is `tenant ← branch ← document ← element-set` — each row carries `Option<AclScope> Parent` so the inheritance chain's legality is data on the vocabulary, never a validation table; `AclEntry.Live(now)` is the window admission (a future `From` stays denied, a passed `Until` lapses), `AclEntry.Lapsed(now)` the expiry probe the `Expired` verdict reads.
- Entry: `ObjectAcl.LadderValid` recursively asserts `child.Kind.Parent == Some(parent.Kind)` down the `Inherited` chain so a mis-stacked ACL (a document inheriting from an element-set) rejects at `Admit` as `ScopeMismatch`, never silently grants.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core (`HashMap`/`Option` structural value equality by construction — no Generator.Equals or Thinktecture owner stacked on `ObjectAcl`, the deleted over-ownership), NodaTime, BCL inbox.
- Growth: one `AclScope` row per new gated kind (its `Parent` slots it into the ladder); a new entry axis (a group, a service account) is rows in the existing subject-keyed maps, never a third map shape; zero new surface — a parallel role-ACL type, a per-scope ACL class, or an unvalidated inheritance chain is the deleted form.
- Boundary: `ObjectAcl` is THE frozen vocabulary its consumers import — `Element/identity#ELEMENT_IDENTITY` persists it (`ElementIdentity.Acl`, the jsonb `ComplexProperty`), the `Rasm.AppHost/Runtime` identity-store PORT decodes it, and `Version/commits#Movable` gates branch movement on its `GrantSet` under `AclScope.Branch` — one type, three consumers, never a fork; `Principals`/`Roles` key by string SUBJECT (the `StoreActor.Subject` and role-claim strings) because the `GrantSet` hash-0 boundary forbids set-keyed maps and because the subject is the wire-stable identity the AppHost port round-trips; the `Owner` slot is the full `StoreActor` value for provenance while owner RECOGNITION compares `Subject` only (role claims are session facts, not identity); the `[From, Until)` window time-boxes both ends so a scheduled grant and a lapsed one are data states, never mutation events.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AclScope {
    public static readonly AclScope Tenant = new("tenant", None);
    public static readonly AclScope Branch = new("branch", Some(Tenant));
    public static readonly AclScope Document = new("document", Some(Branch));
    public static readonly AclScope ElementSet = new("element-set", Some(Document));
    public Option<AclScope> Parent { get; }
    private AclScope(string key, Option<AclScope> parent) : this(key) => Parent = parent;
}

// --- [MODELS] --------------------------------------------------------------------------
// `GrantedBy` is the Persistence-owned `StoreActor` (`Element/graph#STORE_RAIL`) — the AppHost `Principal`
// never crosses down; the port maps at the boundary.
public sealed record AclEntry(GrantSet Allow, GrantSet Deny, StoreActor GrantedBy, Instant At, Option<Instant> From, Option<Instant> Until) {
    public bool Live(Instant now) => From.Match(Some: f => now >= f, None: static () => true) && Until.Match(Some: u => now < u, None: static () => true);
    public bool Lapsed(Instant now) => Until.Match(Some: u => now >= u, None: static () => false);
}

// A plain record: LanguageExt HashMap/Option carry structural value equality by construction, so no
// Generator.Equals or Thinktecture owner is needed (stacking either over the already-value-equal LanguageExt
// collections is the deleted over-ownership) — the recursive Inherited chain compares whole by default equality.
// Subject-string keys are the hash-safe fold shape (the GrantSet hash-0 boundary) and the wire-stable identity
// the AppHost identity-store PORT round-trips.
public sealed record ObjectAcl(
    UInt128 Scope, AclScope Kind, StoreActor Owner,
    HashMap<string, AclEntry> Principals, HashMap<string, AclEntry> Roles, Option<ObjectAcl> Inherited) {
    public bool LadderValid => Inherited.Match(Some: p => Kind.Parent == Some(p.Kind) && p.LadderValid, None: static () => true);
}
```

## [04]-[AUTHORITY]

- Owner: `AuthDecision` the closed `[Union]` authz verdict — the authorization half of the fissioned decision union (the crypto half is `Element/identity#KMS_CUSTODY` `CustodyVerdict`; the two never re-fuse); `Authority` the static surface owning the deny-over-allow `Effective` fold, the `LapsedFor` expiry probe, the one `Admit` entry, and the `Shift` grant-diff projection feeding the `Grant.Audit` lane.
- Cases: `Granted(GrantSet Effective)` carries the computed effective set so a caller never re-folds; `Denied(StoreActor Actor, GrantSet Demand, UInt128 Scope)` names the refused demand; `ScopeMismatch(UInt128 Demanded, UInt128 Actual)` covers both a wrong-object demand and an invalid inheritance ladder; `Expired(StoreActor Actor, Instant LapsedAt)` distinguishes a grant that WOULD have admitted but lapsed, `LapsedAt` the entry's real `Until` (the latest across direct/role/inherited candidates) — the operator-actionable renewal signal a bare deny would bury.
- Entry: `public static AuthDecision Admit(ObjectAcl acl, StoreActor actor, GrantSet demand, UInt128 scope, Instant now)` is the one polymorphic admission — roles ride the actor's own `Roles` claims, so the entry takes no parallel roles parameter; `Effective` folds owner, direct, role, and inherited grants with deny-over-allow (an explicit deny set-difference overrides every inherited allow); `LapsedFor` resolves the latest lapse instant among entries that would have admitted the demand; `Shift(GrantSet before, GrantSet after)` projects the member-level `Added`/`Removed` grant deltas as typed audit rows.
- Packages: Thinktecture.Runtime.Extensions, Generator.Equals (`EqualityComparer.Default.Inequalities` + `MemberPathSegmentKind.Added`/`Removed` — the structured set-membership diff), LanguageExt.Core, NodaTime, BCL inbox.
- Growth: one `AuthDecision` case per new verdict; a new admission dimension (a quota, an IP fence) is a clause inside the one `Admit` fold, never a second entry; the audit trail is `Shift` rows appended through the `Version/provenance#ATTESTED_LEDGER` consumer under the `Grant.Audit` right, never a parallel audit log; zero new surface — a boolean `CanAccess`, a per-scope `Admit` overload family, or a re-fused authz+crypto union is the deleted form.
- Boundary: the fold is deny-over-allow — `allow.Without(deny)` runs LAST so an explicit `AclEntry.Deny` at any altitude defeats every inherited and role allow; the `Owner` subject carries `GrantSet.Owner` so the creator never locks itself out; `Admit` is TOTAL — every input resolves to exactly one verdict, no exception path and no fault band (Band NONE; a persistence failure around the ACL row is the composed `IdentityFault`, raised by the store tier, not by this algebra); the branch gate is THIS `Admit` under `AclScope.Branch` with the branch rights (`Merge`/`Rebase`/`ForcePush`) in the demanded set — `Version/commits#Movable` composes it, a second branch-permission surface is the deleted form; `Shift` is an in-memory audit projection over `[SetEquality]` `Inequalities` rows — it complements, never replaces, the content-keyed attested ledger (tamper evidence stays `Version/provenance`).

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
// The authz half of the fissioned decision union: Granted/Denied/ScopeMismatch/Expired. The crypto half
// (Attested/Authentic/Unsigned/Unauthored/Forged/DigestWidth/Wrapped/Unwrapped/KeyUnusable) is
// `Element/identity#KMS_CUSTODY` `CustodyVerdict` — the two never re-fuse into one union.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AuthDecision {
    private AuthDecision() { }
    public sealed record Granted(GrantSet Effective) : AuthDecision;
    public sealed record Denied(StoreActor Actor, GrantSet Demand, UInt128 Scope) : AuthDecision;
    public sealed record ScopeMismatch(UInt128 Demanded, UInt128 Actual) : AuthDecision;
    public sealed record Expired(StoreActor Actor, Instant LapsedAt) : AuthDecision;
}

// --- [MODELS] --------------------------------------------------------------------------
// One grant-membership delta: the `Grant.Audit`-lane row `Shift` projects off the `[SetEquality]`
// member diff — `Granted: true` is an Added segment, `false` a Removed one.
public readonly record struct AclShift(Grant Grant, bool Granted);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Authority {
    // Deny-over-allow: owner, inherited, role, and direct allows union first; direct + role denies subtract
    // LAST, so an explicit deny at any altitude defeats every inherited allow. Owner recognition compares
    // Subject only — role claims are session facts, never identity.
    public static GrantSet Effective(ObjectAcl acl, StoreActor actor, Instant now) {
        var owned = string.Equals(acl.Owner.Subject, actor.Subject, StringComparison.Ordinal) ? GrantSet.Owner : GrantSet.None;
        var direct = acl.Principals.Find(actor.Subject).Filter(e => e.Live(now));
        var inherited = acl.Inherited.Map(p => Effective(p, actor, now)).IfNone(GrantSet.None);
        var roleAllow = actor.Roles.Fold(GrantSet.None, (acc, role) => acc.Union(acl.Roles.Find(role).Filter(e => e.Live(now)).Map(static e => e.Allow).IfNone(GrantSet.None)));
        var allow = owned.Union(inherited).Union(roleAllow).Union(direct.Map(static e => e.Allow).IfNone(GrantSet.None));
        var deny = direct.Map(static e => e.Deny).IfNone(GrantSet.None).Union(actor.Roles.Fold(GrantSet.None, (acc, role) => acc.Union(acl.Roles.Find(role).Filter(e => e.Live(now)).Map(static e => e.Deny).IfNone(GrantSet.None))));
        return allow.Without(deny);
    }

    // The lapsed-would-have-admitted probe resolves the LATEST lapse instant across direct, role, and
    // inherited entries, so `Expired.LapsedAt` names the real `Until`, never the observation time.
    static Option<Instant> LapsedFor(ObjectAcl acl, StoreActor actor, GrantSet demand, Instant now) {
        Seq<AclEntry> local = acl.Principals.Find(actor.Subject).ToSeq() + actor.Roles.Bind(role => acl.Roles.Find(role).ToSeq());
        Seq<Instant> lapses = local.Filter(e => e.Lapsed(now) && e.Allow.Admits(demand)).Bind(static e => e.Until.ToSeq())
            + acl.Inherited.Bind(p => LapsedFor(p, actor, demand, now)).ToSeq();
        return lapses.Fold(Option<Instant>.None, static (acc, at) => Some(acc.Match(Some: held => Instant.Max(held, at), None: () => at)));
    }

    // TOTAL admission: every input resolves to exactly one verdict. An invalid ladder rejects as
    // ScopeMismatch (a mis-stacked chain never silently grants); a lapsed would-have-admitted entry
    // surfaces as Expired carrying its real lapse instant — the renewal signal a bare deny would bury.
    public static AuthDecision Admit(ObjectAcl acl, StoreActor actor, GrantSet demand, UInt128 scope, Instant now) =>
        acl.Scope != scope || !acl.LadderValid ? new AuthDecision.ScopeMismatch(scope, acl.Scope)
        : Effective(acl, actor, now) is var grant && grant.Admits(demand) ? new AuthDecision.Granted(grant)
        : LapsedFor(acl, actor, demand, now).Match<AuthDecision>(
            Some: at => new AuthDecision.Expired(actor, at),
            None: () => new AuthDecision.Denied(actor, demand, scope));

    // The Audit-lane grant diff: the generated `[SetEquality]` member diff reports set-membership deltas as
    // Added/Removed path segments; each becomes one typed AclShift row the audit trail appends through the
    // attested ledger. In-memory projection only — tamper evidence stays `Version/provenance#ATTESTED_LEDGER`.
    public static Seq<AclShift> Shift(GrantSet before, GrantSet after) =>
        toSeq(GrantSet.EqualityComparer.Default.Inequalities(before, after))
            .Bind(static delta => delta.Path.Segments[^1].Kind switch {
                MemberPathSegmentKind.Added => Seq1(new AclShift((Grant)(delta.Right ?? delta.Left)!, Granted: true)),
                MemberPathSegmentKind.Removed => Seq1(new AclShift((Grant)(delta.Left ?? delta.Right)!, Granted: false)),
                _ => Seq<AclShift>.Empty,
            });
}
```

| [INDEX] | [POLICY]              | [VALUE]                                  | [BINDING]                                                  |
| :-----: | :-------------------- | :---------------------------------------- | :--------------------------------------------------------- |
|  [01]   | authority model       | `GrantSet` frozen-set algebra            | `[Flags]` enum is the deleted form; `Admin` value-derived superuser |
|  [02]   | hash boundary         | `[SetEquality]` hashes to 0              | a `GrantSet` never keys a map; subject strings key the fold |
|  [03]   | precedence            | deny-over-allow, `Without` last          | explicit deny defeats every inherited/role allow           |
|  [04]   | inheritance           | `AclScope.Parent` ladder invariant       | mis-stacked chain → `ScopeMismatch`, never a silent grant  |
|  [05]   | branch gate           | same `GrantSet` under `AclScope.Branch`  | `Version/commits#Movable` composes `Admit`; no second enum |
|  [06]   | audit diff            | `Shift` over `Inequalities` Added/Removed | typed `AclShift` rows into the attested ledger            |
|  [07]   | fault band            | NONE — total algebra                     | store-tier failures rail `IdentityFault` 8340              |
