# [06] Testing Harness

The rebuilt test surface replaces the 3,973-LOC `RuleBehaviorTests.cs` god class + the 79-LOC hand-built TPA harness. Evidence base: the demolition coverage shape (doc 01 §8), the testing state-of-the-art (doc 03 §7), the per-rule positive+negative mandate (CLAUDE.md [4], doc 02 §4), and the harness red-team (doc 08 §H — one VALID gap on generated surfaces, the rest REFUTED-as-genuine). One test file per rule; meta-tests enforce the catalog invariants.

## [1]-[HARNESS]

`Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` + `DefaultVerifier` via `Harness.cs`: `using Verify = CSharpAnalyzerVerifier<Driver, DefaultVerifier>` (doc 03 §7). This retires:
- the hand-built compilation from `TRUSTED_PLATFORM_ASSEMBLIES` (`AnalyzerTestHarness.cs:68-78`) and hand directory-walk for repo root (`:53-61`);
- `concurrentAnalysis:false` (`:43`) — the harness now runs with concurrent analysis ENABLED, matching production (the current gap means the concurrent path is never tested); keep the analyzer-exception rethrow + deterministic ordering (`:42,62-67`).

**Markup spans `{|CSP0002:span|}` MANDATORY in every positive** — span assertions for free, killing the "first-match severity-only" shallow assertions (`RuleBehaviorTests.cs:902-922`: no span, no message-args, no fire-count; duplicate-emission regressions invisible).

## [2]-[REAL_PACKAGE_REFERENCES]

`ReferenceAssemblies` pinned to repo TFM + `.AddPackages(LanguageExt.Core, Thinktecture.Runtime.Extensions)` — REAL packages, ending the fake in-source `namespace LanguageExt { class Fin<T> }` shims that proved name-only matching (`RuleBehaviorTests.cs:288+`, 10+ sites; the analyzer never checked assembly identity). 08 H2 confirms these upgrades are all evidence-verified genuine.

## [3]-[GENERATED_SURFACE_STRATEGY] (08 H1 — the one VALID harness gap, resolved)

Real packages fix name-only matching for LanguageExt, but rules asserting over Thinktecture GENERATED surfaces (CSP0734 generated state-threaded `Switch` usage, CSP0733 alias-of-generated-factory) need the generator to RUN in-test or hand-stubbed generated partials — `CSharpAnalyzerVerifier` does NOT execute source generators. Per-rule-family strategy (settled):
- **Generator-run path:** for the rule families that assert over Thinktecture output, configure the test's `SolutionTransforms` to add the `Thinktecture.Runtime.Extensions` SOURCE GENERATOR to the project so the generated `Switch`/factory partials exist at analysis time. This is the canonical path for 0733/0734/0737/0740/0745/0802.
- **Hand-stubbed-partial path:** for rules where only the SHAPE of the generated member matters (not the generator's exact emission), the test source declares the expected `partial` member directly — faster, but only where the generated contract is stable. Documented per rule in its test file header.
Without this explicit choice those positives will not compile.

## [4]-[ONE_FILE_PER_RULE]

`Rules/<ID>.<Name>Tests.cs`: ≥1 positive with span, ≥1 negative valid-COMPACT-code (`VerifyAnalyzerAsync` clean), one case per documented exemption clause (0005's Union-pair, 0728's Op-level MapFail, 0729's interface block). This closes the ~53-rules-without-negatives hole (doc 01 §8: the entire CSP0004-0608 band plus most of 0702-0720 had NO negative test) and satisfies the positive+negative mandate. Per-category trivial-row bans (doc 04 §3, 08 A3) may share a per-category test file with one positive+negative per row, keyed on RuleRow ID (08 A3 keys pairing on RuleRow statics + descriptor ID, NOT file naming).

## [5]-[META_CATALOG_INVARIANTS] (`Meta/CatalogInvariants.cs`)

- (a) reflection — every static `RuleRow` in the assembly is in `Catalog.All`;
- (b) every row has a matching test by RuleRow-ID convention with BOTH positive and negative facts (08 A3: keyed on RuleRow statics + descriptor ID, not file name);
- (c) every Law-tier descriptor message contains `"fix:"`;
- (d) source grep — no `ToDisplayString` in `Rules/` PREDICATE code (verdict c bans it for type IDENTITY; message-ARG projection passes `ISymbol`/`.Name` and display formatting lives only in the Kernel `Report` path), no `CultureInfo.CurrentCulture` anywhere, no `DateTime`/`Random`/env reads in analyzer source;
- (e) release-tracking via REAL RS2000-RS2008 (AnalyzerReleases md wired as AdditionalFiles) — DELETE the regex re-implementation (`ReleaseDisciplineTests.cs:54-108`); a meta-test additionally asserts every NEW rule's Unshipped row exists (08 A1) and that the 4 reserved IDs (0016/0716/0721/0722) appear in neither `All` nor any emission;
- (f) the Rule×Scope applicability matrix (doc 04 §4, settled in doc 05 §10) is complete — every active rule has a declared `ScopeGate`;
- (g) Pressure-band sync — the meta-test parses `Directory.Build.props` and asserts the `<WarningsNotAsErrors>` CSP band EXACTLY equals `Catalog.All` Pressure-tier IDs (both directions). Without this, adding a Pressure rule while forgetting the band entry silently promotes it to error under repo-wide TWAE — the exact drift 08 F1 closed; this invariant keeps it closed against future rule adds.
- (h) harness-pin mirror — the `AddPackages` versions in `Harness.cs` (`LanguageExt.Core`, `Thinktecture.Runtime.Extensions`) EXACTLY equal the `Directory.Packages.props` pins, parsed from the props file; a central bump without the harness bump fails here instead of silently testing rules against a stale package surface (doc 04 §10 mirror law).

## [6]-[CONFIG_COVERAGE]

`TestState.AnalyzerConfigFiles` injects `build_property.CspScope` variants per rule to PROVE scope gating (e.g. CSP0601 fires under `CspScope=HotPath`, silent under `Domain`; a `csp.scope` per-tree override flips a Boundary project's kernel back into domain enforcement — 08 D2). AdditionalFiles fixtures prove data-driven rules go SILENT without config AND that CSP0901 fires when the section is missing in a non-tooling scope (08 F4). A `csp.CSP0741.case_threshold` fixture proves the parameter surface (doc 04 §5).

## [7]-[SNAPSHOT_POSTURE]

NONE for the analyzer (markup is state-of-the-art, doc 03 §7). Verify snapshots ONLY for the extracted UnionOps generator's emitted source (the generator is the one component where golden-source comparison is the right tool, doc 03 §7).

## [8]-[CARRIED_HARNESS_CONCEPTS]

Carry forward the deterministic details (01 §9.9): hashed assembly names per source, ordered diagnostics, analyzer-exception rethrow — `DefaultVerifier` already provides ordered diagnostics and rethrow; preserve the per-source assembly-name hashing where the harness wrapper needs it for parallel test isolation. Carry the catalog-meta-invariant CONCEPT (every rule emitted, positively tested, release-md synced — `RuleBehaviorTests.cs:924-945`, `ReleaseDisciplineTests.cs:37-53`) but re-base release tracking on native RS2000 and ADD the per-rule negative-test invariant (§5b).
