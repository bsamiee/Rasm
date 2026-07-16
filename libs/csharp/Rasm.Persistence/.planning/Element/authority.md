# [PERSISTENCE_ELEMENT_AUTHORITY]

Rasm.Persistence gates every durable object interaction through one object-ACL authorization algebra that decides WHO MAY. `Grant` spans object lifecycle, review, governance, and branch control in one wire-keyed vocabulary; `GrantSet` is its frozen-set algebra; `AclScope` carries the inheritance altitude; `AclEntry` carries provenance and a valid `[From, Until)` window; `ObjectAcl` owns the inherited chain. `Authority.Admit` validates ACL integrity, accumulates allow and deny independently, gives a live deny precedence over lapse diagnosis, and returns one closed `AuthDecision`. Crypto custody remains `Element/identity#KMS_CUSTODY`.

Every actor slot is the Persistence-owned `Element/graph#STORE_RAIL` `StoreActor` (subject + role claims), never an AppHost `Principal`; a `Version/commits#COMMIT_DAG` `BranchRef` grant is this same `GrantSet` narrowed under `AclScope.Branch`, never a parallel branch enum; a `Element/identity#ELEMENT_IDENTITY` `Tenant` RLS column is the coarse partition and this object ACL the fine within-tenant grant, two altitudes never duplicated. `ObjectAcl` is the frozen vocabulary its consumers import — `ElementIdentity.Acl` persists it as the jsonb column, the AppHost identity-store port decodes it — so the split never forks the type. A persistence failure around an ACL row rails the composed `Element/identity#SCHEMA_VERDICT` `IdentityFault` band (8340), never a new one.

## [01]-[INDEX]

- [02]-[GRANT_ALGEBRA]: the `Grant` vocabulary, the `GrantSet` frozen-set value, superuser containment, and the set-equality hashing boundary.
- [03]-[OBJECT_ACL]: the `AclScope` altitude ladder, the windowed `AclEntry`, the owner-plus-inherited `ObjectAcl` chain, and the ladder invariant.
- [04]-[AUTHORITY]: the deny-over-allow `Effective` fold, the lapsed-grant probe, the one `Admit` entry over the closed `AuthDecision` verdict, and the `Inequalities`-fed grant-shift audit diff.

## [02]-[GRANT_ALGEBRA]

- Owner: `Grant` the one `[SmartEnum<string>]` object-authorization vocabulary every ACL entry draws from, wire-keyed because membership crosses the wire — the jsonb `Acl` column persists it under `ElementJson` and the AppHost identity-store port round-trips each grant as its bare key through the generated Thinktecture converters, so a keyless row strands that round-trip; `GrantSet` the `[Equatable]` frozen-set value carrying the `[SetEquality]` `Grant` set, the value-derived `Owner` superset, and the `Admits`/`Union`/`Without` operators.
- Cases: object lifecycle covers `Read`/`Create`/`Write`/`Delete`/`Restore`/`Move`/`Copy`/`Export`/`Share`/`Revoke`; review and issue control covers `Annotate`/`RequestReview`/`Lock`/`Approve`/`Publish`/`Resolve`; governance covers `Audit`/`Classify`/`Retain`/`LegalHold`/`ManageAcl`; branch control covers `Merge`/`Rebase`/`CherryPick`/`Tag`/`ForcePush`; `Admin` is the explicit superuser row. `GrantSet.Owner` derives from generated `Items`.
- Entry: `GrantSet.Of(params ReadOnlySpan<Grant>)` mints a membership; `Admits` is superuser-aware containment (an `Admin`-bearing set admits any demand); `Union`/`Without` are the allow/deny fold primitives the `#AUTHORITY` fold composes.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + the wire converters `Element/codec#CODEC_AXIS` registers), Generator.Equals (`[Equatable]`/`[SetEquality]`), System.Collections.Frozen, LanguageExt.Core, BCL inbox.
- Growth: one `Grant` row per new permission — `Owner` absorbs it value-derived and the fold stays membership-generic, so a new right is ONE static line; a `[Flags]` bitfield (the `shapes.md` `ReplaceFlags` law), a second branch-only enum, or a per-right boolean column is the deleted form.
- Boundary: `Grant` is distinct in name and concept from the AppHost `Agent/capability#CAPABILITY_REGISTRY` effect-gating `Capability`, the two never sharing a name across strata. HASH-0 TRAP: `[SetEquality]` compares exact set membership but its hash contribution is ALWAYS zero (`SetEqualityComparer<T>.GetHashCode` returns 0), so a `GrantSet` must NEVER key a dictionary, `HashMap`, or `HashSet` — every set collides into bucket 0 and the lookup degrades to a linear scan; `ObjectAcl` therefore keys entries by the string subject and carries the `GrantSet` as the value, equality-comparing and `Admits`-folding freely.

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
// Wire-keyed rows: the jsonb `ElementIdentity.Acl` column round-trips each grant as its bare key through the
// generated Thinktecture converters. Combinable authority is the GrantSet value below, never a [Flags] enum.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Grant {
    public static readonly Grant Read = new("read");
    public static readonly Grant Create = new("create");
    public static readonly Grant Write = new("write");
    public static readonly Grant Delete = new("delete");
    public static readonly Grant Restore = new("restore");
    public static readonly Grant Move = new("move");
    public static readonly Grant Copy = new("copy");
    public static readonly Grant Export = new("export");
    public static readonly Grant Annotate = new("annotate");
    public static readonly Grant RequestReview = new("request-review");
    public static readonly Grant Lock = new("lock");
    public static readonly Grant Approve = new("approve");
    public static readonly Grant Publish = new("publish");
    public static readonly Grant Resolve = new("resolve");
    public static readonly Grant Share = new("share");
    public static readonly Grant Revoke = new("revoke");
    public static readonly Grant Audit = new("audit");
    public static readonly Grant Classify = new("classify");
    public static readonly Grant Retain = new("retain");
    public static readonly Grant LegalHold = new("legal-hold");
    public static readonly Grant ManageAcl = new("manage-acl");
    public static readonly Grant Merge = new("merge");
    public static readonly Grant Rebase = new("rebase");
    public static readonly Grant CherryPick = new("cherry-pick");
    public static readonly Grant Tag = new("tag");
    public static readonly Grant ForcePush = new("force-push");
    public static readonly Grant Admin = new("admin");
}

// Set-algebra value, value-equal by SET membership through Generator.Equals `[SetEquality]` — `[Equatable]`, NOT
// a Thinktecture key owner (the two equality models never stack). HASH-0: `[SetEquality]` contributes 0 to
// `GetHashCode`, so a GrantSet NEVER keys a dictionary/HashMap/HashSet — key by subject string, set as value.
[Equatable]
public sealed partial record GrantSet([property: SetEquality] FrozenSet<Grant> Grants) {
    public static readonly GrantSet None = new(FrozenSet<Grant>.Empty);
    public static readonly GrantSet Owner = new(Grant.Items.ToFrozenSet());

    public static GrantSet Of(params ReadOnlySpan<Grant> grants) => new(grants.ToArray().ToFrozenSet());
    public GrantSet Union(GrantSet other) => new(Grants.Union(other.Grants).ToFrozenSet());
    public GrantSet Without(GrantSet other) => new(Grants.Except(other.Grants).ToFrozenSet());
    public bool Admits(GrantSet demand) => Grants.Contains(Grant.Admin) || demand.Grants.IsSubsetOf(Grants);
    public bool Blocks(GrantSet demand) => Grants.Contains(Grant.Admin) || Grants.Overlaps(demand.Grants);
}
```

## [03]-[OBJECT_ACL]

- Owner: `AclScope` carries its altitude parent; `AclEntry` carries allow, deny, provenance, and a time window; `ObjectAcl` owns the subject maps plus recursive `InvalidLadder` and `InvalidWindow` integrity projections.
- Cases: each `AclScope` row carries `Option<AclScope> Parent` so the inheritance chain's legality is data on the vocabulary, never a validation table; `AclEntry.Live(now)` is window admission (a future `From` denies, a passed `Until` lapses) and `AclEntry.Lapsed(now)` the expiry probe the `Expired` verdict reads.
- Entry: `ObjectAcl.InvalidLadder` identifies the first mis-stacked child; `InvalidWindow` identifies the first non-increasing `[From, Until)` interval. `Admit` returns their typed verdicts before evaluating grants.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core (`HashMap`/`Option` structural value equality — no Generator.Equals or Thinktecture owner stacked on `ObjectAcl`), NodaTime, BCL inbox.
- Growth: one `AclScope` row per new gated kind (its `Parent` slots it into the ladder); a new subject axis (a group, a service account) is rows in the existing subject-keyed maps, never a third map; a parallel role-ACL type, a per-scope ACL class, or an unvalidated chain is the deleted form.
- Boundary: `Principals`/`Roles` key by string subject (the `StoreActor.Subject` and role-claim strings) because the `GrantSet` hash-0 trap forbids set-keyed maps and the subject is the wire-stable identity the AppHost port round-trips; the `Owner` slot carries the full `StoreActor` for provenance while owner RECOGNITION compares `Subject` only, role claims being session facts not identity; the `[From, Until)` window time-boxes both ends so a scheduled grant and a lapsed one are data states, never mutation events.

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
// `GrantedBy` is the Persistence-owned `StoreActor` — the AppHost `Principal` never crosses down.
public sealed record AclEntry(GrantSet Allow, GrantSet Deny, StoreActor GrantedBy, Instant At, Option<Instant> From, Option<Instant> Until) {
    public bool WindowValid => From.Match(Some: from => Until.Match(Some: until => from < until, None: static () => true), None: static () => true);
    public bool Live(Instant now) => WindowValid && From.Match(Some: f => now >= f, None: static () => true) && Until.Match(Some: u => now < u, None: static () => true);
    public bool Lapsed(Instant now) => WindowValid && Until.Match(Some: u => now >= u, None: static () => false);
}

// Plain record: LanguageExt HashMap/Option give structural value equality, so no Generator.Equals or Thinktecture
// owner stacks here and the recursive Inherited chain compares whole. Subject-string keys are the hash-safe shape.
public sealed record ObjectAcl(
    UInt128 Scope, AclScope Kind, StoreActor Owner,
    HashMap<string, AclEntry> Principals, HashMap<string, AclEntry> Roles, Option<ObjectAcl> Inherited) {
    public Option<ObjectAcl> InvalidLadder => Inherited.Match(
        Some: parent => Kind.Parent == Some(parent.Kind) ? parent.InvalidLadder : Some(this),
        None: static () => None);
    public Option<AclEntry> InvalidWindow {
        get {
            Option<AclEntry> local = (toSeq(Principals).Map(static pair => pair.Value) + toSeq(Roles).Map(static pair => pair.Value)).Find(static entry => !entry.WindowValid);
            return local.Match(Some: static entry => Some(entry), None: () => Inherited.Bind(static parent => parent.InvalidWindow));
        }
    }
}
```

## [04]-[AUTHORITY]

- Owner: `AuthDecision` the closed `[Union]` authz verdict (the crypto half is `Element/identity#KMS_CUSTODY` `CustodyVerdict`, the two never re-fuse); `Authority` the static surface owning the deny-over-allow `Effective` fold, the `LapsedFor` expiry probe, the one `Admit` entry, and the `Shift` grant-diff projection feeding the `Grant.Audit` lane.
- Cases: `Granted(GrantSet Effective)` carries the computed set; `Denied(...)` names the refused demand; `ScopeMismatch(...)` covers a wrong object; `InvalidInheritance(...)` and `InvalidWindow(...)` expose malformed ACL data; `Expired(...)` carries the latest real lapse only when no live deny blocks the demand.
- Entry: `Admit(ObjectAcl acl, StoreActor actor, GrantSet demand, UInt128 scope, Instant now)` is the one polymorphic admission — roles ride the actor's own `Roles` claims, so it takes no parallel roles parameter; `Effective` folds owner, direct, role, and inherited grants deny-over-allow (an explicit deny set-difference overrides every inherited allow); `LapsedFor` resolves the latest lapse instant among entries that once admitted the demand; `Shift(before, after)` projects the member-level `Added`/`Removed` deltas as typed audit rows.
- Packages: Thinktecture.Runtime.Extensions, Generator.Equals (`Inequalities` + `MemberPathSegmentKind.Added`/`Removed` — the structured set-membership diff), LanguageExt.Core, NodaTime, BCL inbox.
- Growth: one `AuthDecision` case per new verdict; a new admission dimension (a quota, an IP fence) is a clause inside the one `Admit` fold, never a second entry; the audit trail is `Shift` rows appended through the `Version/provenance#ATTESTED_LEDGER` consumer under `Grant.Audit`, never a parallel log; a boolean `CanAccess`, a per-scope `Admit` overload family, or a re-fused authz+crypto union is the deleted form.
- Boundary: the fold is deny-over-allow at every altitude — allow and deny sets accumulate SEPARATELY down the `Inherited` chain and `allow.Without(deny)` runs ONCE at the root, so an explicit `AclEntry.Deny` at any live altitude defeats every descendant direct and role allow (a per-level subtraction that lets a child allow re-grant a parent deny is the deleted form); the `Owner` subject short-circuits to `GrantSet.Owner` before the fold so the creator never locks itself out; `Admit` is TOTAL — every input resolves to one verdict with no exception path and no fault band (a persistence failure around the ACL row is the composed `IdentityFault`, raised by the store tier); the branch gate is THIS `Admit` under `AclScope.Branch` with the branch rights in the demanded set, `Version/commits#Movable` composing it rather than a second branch-permission surface; `Shift` is an in-memory audit projection over `[SetEquality]` `Inequalities` rows — it complements, never replaces, the content-keyed attested ledger.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
// Authz half of the fissioned decision union; the crypto half is the identity-tier `CustodyVerdict` — the two never re-fuse.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AuthDecision {
    private AuthDecision() { }
    public sealed record Granted(GrantSet Effective) : AuthDecision;
    public sealed record Denied(StoreActor Actor, GrantSet Demand, UInt128 Scope) : AuthDecision;
    public sealed record ScopeMismatch(UInt128 Demanded, UInt128 Actual) : AuthDecision;
    public sealed record InvalidInheritance(UInt128 Scope, AclScope Kind, Option<AclScope> ActualParent) : AuthDecision;
    public sealed record InvalidWindow(UInt128 Scope, AclEntry Entry) : AuthDecision;
    public sealed record Expired(StoreActor Actor, Instant LapsedAt) : AuthDecision;
}

// --- [MODELS] --------------------------------------------------------------------------
// One grant-membership delta off the `[SetEquality]` member diff — `Granted: true` is an Added segment, `false` Removed.
public readonly record struct AclShift(Grant Grant, bool Granted);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Authority {
    // Deny-over-allow at EVERY altitude: allow and deny ACCUMULATE separately down the inheritance chain and the
    // subtraction runs ONCE at the root — a per-level `Without` re-orders precedence and lets a child direct allow
    // re-grant a parent's explicit deny (the lost-inherited-deny defect this split fold forecloses). Owner
    // recognition compares Subject only and short-circuits BEFORE the fold, so an explicit deny can never strand
    // the object's own creator.
    public static GrantSet Effective(ObjectAcl acl, StoreActor actor, Instant now) =>
        string.Equals(acl.Owner.Subject, actor.Subject, StringComparison.Ordinal)
            ? GrantSet.Owner
            : EffectiveNonOwner(acl, actor, now);

    static GrantSet EffectiveNonOwner(ObjectAcl acl, StoreActor actor, Instant now) {
        (GrantSet Allow, GrantSet Deny) grants = Folded(acl, actor, now);
        return grants.Allow.Without(grants.Deny);
    }

    static (GrantSet Allow, GrantSet Deny) Folded(ObjectAcl acl, StoreActor actor, Instant now) {
        Option<AclEntry> direct = acl.Principals.Find(actor.Subject).Filter(entry => entry.Live(now));
        (GrantSet Allow, GrantSet Deny) role = actor.Roles.Fold((Allow: GrantSet.None, Deny: GrantSet.None), (acc, role) =>
            acl.Roles.Find(role).Filter(entry => entry.Live(now)).Match(
                Some: entry => (acc.Allow.Union(entry.Allow), acc.Deny.Union(entry.Deny)),
                None: () => acc));
        (GrantSet Allow, GrantSet Deny) inherited = acl.Inherited.Map(parent => Folded(parent, actor, now)).IfNone((GrantSet.None, GrantSet.None));
        return (inherited.Item1.Union(role.Allow).Union(direct.Map(static e => e.Allow).IfNone(GrantSet.None)),
                inherited.Item2.Union(role.Deny).Union(direct.Map(static e => e.Deny).IfNone(GrantSet.None)));
    }

    // Resolves the LATEST lapse instant across direct, role, and inherited entries, so `Expired.LapsedAt` is the real `Until`.
    static Option<Instant> LapsedFor(ObjectAcl acl, StoreActor actor, GrantSet demand, Instant now) {
        Seq<AclEntry> local = acl.Principals.Find(actor.Subject).ToSeq() + actor.Roles.Bind(role => acl.Roles.Find(role).ToSeq());
        Seq<Instant> lapses = local.Filter(e => e.Lapsed(now) && e.Allow.Admits(demand)).Bind(static e => e.Until.ToSeq())
            + acl.Inherited.Bind(p => LapsedFor(p, actor, demand, now)).ToSeq();
        return lapses.Fold(Option<Instant>.None, static (acc, at) => Some(acc.Match(Some: held => Instant.Max(held, at), None: () => at)));
    }

    // Integrity precedes authorization; a live explicit deny precedes expiry diagnosis.
    public static AuthDecision Admit(ObjectAcl acl, StoreActor actor, GrantSet demand, UInt128 scope, Instant now) {
        if (acl.Scope != scope) return new AuthDecision.ScopeMismatch(scope, acl.Scope);
        if (acl.InvalidLadder is { IsSome: true } ladder) {
            ObjectAcl broken = ladder.IfNoneUnsafe(default(ObjectAcl)!);
            return new AuthDecision.InvalidInheritance(broken.Scope, broken.Kind, broken.Inherited.Map(static parent => parent.Kind));
        }
        if (acl.InvalidWindow is { IsSome: true } invalid) return new AuthDecision.InvalidWindow(acl.Scope, invalid.IfNoneUnsafe(default(AclEntry)!));
        GrantSet effective = Effective(acl, actor, now);
        if (effective.Admits(demand)) return new AuthDecision.Granted(effective);
        (GrantSet Allow, GrantSet Deny) current = Folded(acl, actor, now);
        return current.Deny.Blocks(demand)
            ? new AuthDecision.Denied(actor, demand, scope)
            : LapsedFor(acl, actor, demand, now).Match<AuthDecision>(
                Some: at => new AuthDecision.Expired(actor, at),
                None: () => new AuthDecision.Denied(actor, demand, scope));
    }

    // Audit-lane grant diff: the generated `[SetEquality]` member diff reports Added/Removed segments, each a typed
    // AclShift row. In-memory projection only — tamper evidence stays `Version/provenance`.
    public static Seq<AclShift> Shift(GrantSet before, GrantSet after) =>
        toSeq(GrantSet.EqualityComparer.Default.Inequalities(before, after))
            .Bind(static delta => delta.Path.Segments[^1].Kind switch {
                MemberPathSegmentKind.Added => Seq1(new AclShift((Grant)(delta.Right ?? delta.Left)!, Granted: true)),
                MemberPathSegmentKind.Removed => Seq1(new AclShift((Grant)(delta.Left ?? delta.Right)!, Granted: false)),
                _ => Seq<AclShift>.Empty,
            });
}
```

| [INDEX] | [POLICY]        | [VALUE]                                   | [BINDING]                                                           |
| :-----: | :-------------- | :---------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | authority model | `GrantSet` frozen-set algebra             | `[Flags]` enum is the deleted form; `Admin` value-derived superuser |
|  [02]   | hash boundary   | `[SetEquality]` hashes to 0               | a `GrantSet` never keys a map; subject strings key the fold         |
|  [03]   | precedence      | denies accumulate; `Without` once at root | inherited deny defeats every descendant direct/role allow           |
|  [04]   | inheritance     | `AclScope.Parent` ladder invariant        | mis-stacked chain → `InvalidInheritance`, never a silent grant      |
|  [05]   | branch gate     | same `GrantSet` under `AclScope.Branch`   | `Version/commits#Movable` composes `Admit`; no second enum          |
|  [06]   | audit diff      | `Shift` over `Inequalities` Added/Removed | typed `AclShift` rows into the attested ledger                      |
|  [07]   | fault band      | NONE — total algebra                      | store-tier failures rail `IdentityFault` 8340                       |

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
