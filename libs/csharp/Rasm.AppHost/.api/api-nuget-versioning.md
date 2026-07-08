# [RASM_APPHOST_API_NUGET_VERSIONING]

`NuGet.Versioning` supplies the real SemVer-2.0 grammar and the `[min,max)` interval algebra the
supply-chain `SemverGate` needs to admit a plugin artifact: `System.Version` is not semver-aware
(no prerelease labels, no metadata, no range syntax), so the gate parses `artifact.ContractRange`
as a `VersionRange`, parses the host plugin-contract version as a `NuGetVersion`, and decides
admission with `VersionRange.Satisfies(host)` rather than a hand-split `lower-upper` string and a
`System.Version` comparison. The same `FindBestMatch` / `VersionComparer` surface resolves the
newest compatible version when several candidates exist.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NuGet.Versioning`
- package: `NuGet.Versioning`
- assembly: `NuGet.Versioning`
- namespace: `NuGet.Versioning`
- license: `Apache-2.0`
- asset: runtime library (multi-target `net8.0` + `net472`; the `net10.0` consumer binds `lib/net8.0` by TFM precedence)
- rail: supply-chain

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: version values
- rail: supply-chain

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]    | [RAIL]                                                              |
| :-----: | :---------------- | :--------------- | :----------------------------------------------------------------- |
|  [01]   | `SemanticVersion` | base value       | `Major`/`Minor`/`Patch`, `ReleaseLabels`, `Metadata`, `IsPrerelease` — `IComparable`, full operator set |
|  [02]   | `NuGetVersion`    | derived value    | adds `Revision`, `IsSemVer2`, `IsLegacyVersion`, `OriginalVersion`, `Version` — the gate's version type |

[PUBLIC_TYPE_SCOPE]: range and interval algebra
- rail: supply-chain

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]     | [RAIL]                                                              |
| :-----: | :----------------- | :---------------- | :----------------------------------------------------------------- |
|  [01]   | `VersionRange`     | interval value    | `[min,max)` range with floating support — `Satisfies`/`FindBestMatch`/`PrettyPrint` |
|  [02]   | `VersionRangeBase` | range base        | `MinVersion`/`MaxVersion`, `IsMinInclusive`/`IsMaxInclusive`, `Satisfies`, `IsSubSetOrEqualTo` |
|  [03]   | `FloatRange`       | floating spec     | `NuGetVersionFloatBehavior` + min version + release prefix — the `*` / `1.2.*` grammar |

[PUBLIC_TYPE_SCOPE]: comparison and formatting
- rail: supply-chain

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [RAIL]                                                              |
| :-----: | :---------------------- | :-------------- | :----------------------------------------------------------------- |
|  [01]   | `VersionComparer`       | comparer        | `IVersionComparer` — `Default`/`Version`/`VersionRelease`/`VersionReleaseMetadata` statics |
|  [02]   | `IVersionComparer`      | comparer iface  | `IEqualityComparer<SemanticVersion>` + `IComparer<SemanticVersion>` |
|  [03]   | `VersionComparison`     | enum            | `Default`, `Version`, `VersionRelease`, `VersionReleaseMetadata`   |
|  [04]   | `NuGetVersionFloatBehavior` | enum        | `None`/`Prerelease`/`Revision`/`Patch`/`Minor`/`Major`/`AbsoluteLatest` + `Prerelease*` variants |
|  [05]   | `VersionFormatter`      | format provider | `ICustomFormatter` — `N`/`V`/`F`/`R`/`x`/`y`/`z` format specifiers via `.Instance` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: version parsing (boundary — never construct from a `System.Version`)
- rail: supply-chain

| [INDEX] | [SURFACE]                                                                          | [ENTRY_FAMILY] | [RAIL]                                                       |
| :-----: | :-------------------------------------------------------------------------------- | :------------- | :---------------------------------------------------------- |
|  [01]   | `NuGetVersion.TryParse(string? value, out NuGetVersion? version)`                  | parse call     | non-throwing parse — the gate's host-version boundary       |
|  [02]   | `NuGetVersion.TryParseStrict(string value, out NuGetVersion? version)`             | parse call     | strict SemVer-2.0-only parse (rejects legacy 4-part)        |
|  [03]   | `NuGetVersion.Parse(string value)`                                                 | parse call     | throwing parse                                              |
|  [04]   | `SemanticVersion.TryParse(string value, out SemanticVersion? version)`             | parse call     | base-type non-throwing parse                                |

[ENTRYPOINT_SCOPE]: range parsing and admission decision (the `SemverGate` core)
- rail: supply-chain

| [INDEX] | [SURFACE]                                                                          | [ENTRY_FAMILY] | [RAIL]                                                              |
| :-----: | :-------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------------- |
|  [01]   | `VersionRange.TryParse(string value, out VersionRange? versionRange)`              | parse call     | parses `[1.0,2.0)` / `1.2.*` / `(,3.0]` — the contract-range boundary |
|  [02]   | `VersionRange.TryParse(string value, bool allowFloating, out VersionRange? versionRange)` | parse call     | floating-aware overload                                            |
|  [03]   | `VersionRangeBase.Satisfies(NuGetVersion version)`                                 | decide call    | returns `bool` — **the admission predicate** (replaces a hand-split min/max compare) |
|  [04]   | `VersionRangeBase.Satisfies(NuGetVersion version, VersionComparison versionComparison)` | decide call    | comparison-mode overload (e.g. ignore metadata)                   |
|  [05]   | `VersionRange.FindBestMatch(IEnumerable<NuGetVersion>? versions)`                  | resolve call   | returns `NuGetVersion?` — newest in-range candidate when several exist |
|  [06]   | `VersionRange.IsBetter(NuGetVersion? current, NuGetVersion? considering)`          | resolve call   | pairwise candidate preference                                     |
|  [07]   | `VersionRangeBase.IsSubSetOrEqualTo(VersionRangeBase? possibleSuperSet)`           | relate call    | range-containment test for policy narrowing                      |

[ENTRYPOINT_SCOPE]: comparison, combination, and formatting
- rail: supply-chain

| [INDEX] | [SURFACE]                                                                          | [ENTRY_FAMILY] | [RAIL]                                                              |
| :-----: | :-------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------------- |
|  [01]   | `VersionComparer.Compare(SemanticVersion? version1, SemanticVersion? version2, VersionComparison versionComparison)` | compare call   | static comparison under an explicit mode                          |
|  [02]   | `VersionComparer.Get(VersionComparison versionComparison)`                         | comparer call  | resolves the `IVersionComparer` for a mode                        |
|  [03]   | `NuGetVersion.CompareTo(SemanticVersion? other, VersionComparison versionComparison)` | compare call   | instance comparison threading the mode                            |
|  [04]   | `VersionRange.Combine(IEnumerable<VersionRange> ranges)`                           | combine call   | union of ranges (widest covering range)                           |
|  [05]   | `VersionRange.CommonSubSet(IEnumerable<VersionRange> ranges)`                      | combine call   | intersection of ranges                                            |
|  [06]   | `NuGetVersion.ToNormalizedString()` / `SemanticVersion.ToFullString()`            | format call    | canonical / full (with metadata) string forms                    |
|  [07]   | `VersionRange.PrettyPrint()` / `ToLegacyShortString()`                             | format call    | human-readable range rendering for a fault message               |

## [04]-[IMPLEMENTATION_LAW]

[VERSION_TOPOLOGY]:
- `SemanticVersion` models SemVer with `Major`/`Minor`/`Patch`, an ordered `ReleaseLabels : IEnumerable<string>` prerelease segment, and `Metadata`; it implements `IComparable`, `IComparable<SemanticVersion>`, `IEquatable<SemanticVersion>`, `IFormattable`, and the full `<`/`<=`/`>`/`>=`/`==`/`!=` operator set, with comparison precedence ignoring build metadata.
- `NuGetVersion : SemanticVersion` adds a fourth `Revision` (legacy `System.Version` interop via the `Version` property and `IsLegacyVersion` when `Revision > 0`), `IsSemVer2` (true when SemVer-2.0 prerelease/metadata is present), and `OriginalVersion` (the verbatim input). `TryParseStrict` rejects the legacy 4-part form; `TryParse` admits it.
- this is precisely the gap `System.Version` cannot fill: it has no prerelease ordering, no metadata, and no range syntax, so a semver-compatible admission decision MUST go through this type.

[RANGE_TOPOLOGY]:
- `VersionRange : VersionRangeBase` is the `[min,max)` interval value with `MinVersion`/`MaxVersion` and `IsMinInclusive`/`IsMaxInclusive` bounds; the `interval-notation` grammar (`[1.0,2.0)`, `(,3.0]`, `1.2.*`, `[1.0.0]`) parses through `TryParse`.
- `Satisfies(NuGetVersion)` is the canonical membership test; `FindBestMatch(IEnumerable<NuGetVersion>)` returns the newest in-range candidate and `IsBetter` drives its pairwise preference — the resolution path when a registry offers several versions.
- floating ranges (`1.2.*`, `*`) carry a `FloatRange` whose `NuGetVersionFloatBehavior` selects the floated component; `IsFloating` flags them and `ToNonSnapshotRange()` strips the float for a pinned comparison.
- the static singletons `VersionRange.All` / `AllStable` / `None` / `AllFloating` are the policy anchors; `Combine` (union) and `CommonSubSet` (intersection) compose a policy from several ranges.

[COMPARISON_TOPOLOGY]:
- `VersionComparer` exposes four modes as statics — `Default` and `Version` (numeric only), `VersionRelease` (numeric + prerelease), `VersionReleaseMetadata` (everything) — and `Get(VersionComparison)` resolves the `IVersionComparer` for a mode; pass the matching `VersionComparison` to `Satisfies`/`CompareTo` when metadata must be ignored.
- `VersionFormatter.Instance` is the `ICustomFormatter` backing the `N`/`V`/`F`/`R`/`x`/`y`/`z` format specifiers used by `ToString(format, provider)` for canonical fault-message rendering.

[LOCAL_ADMISSION]:
- the supply-chain `SemverGate` replaces its `System.Version.TryParse` + manual `range.Split('-')` `[min,max)` logic with: `VersionRange.TryParse(artifact.ContractRange, out var range)` and `NuGetVersion.TryParse(Policy.HostContractVersion, out var host)`, then `range.Satisfies(host)` — a parse failure on either side fails the gate closed (`SupplyChainFault.VersionIncompatible`), matching the existing fail-closed posture.
- the gate keeps the decision pure and total: `TryParse` (never `Parse`) on both boundary inputs, no exception path in the admission fold; the `Fin`-rail integration wraps the `bool` result, so a malformed contract range is a typed denial, not a throw.
- when the host advertises several compatible plugin-contract versions, `FindBestMatch` selects the newest in-range version for the load decision; `PrettyPrint()` / `ToNormalizedString()` render the range and version into the `VersionIncompatible` fault payload.
- only the version/range/comparer surface is admitted for the contract-range gate; package-graph resolution, framework-compatibility, and the wider NuGet client surface are out of scope.

[RAIL_LAW]:
- Package: `NuGet.Versioning`
- Owns: SemVer-2.0 parsing, the `[min,max)` range algebra, and version comparison for the plugin-contract `SemverGate`
- Accept: `VersionRange.TryParse` + `NuGetVersion.TryParse` at the artifact/host boundary, decided by `VersionRange.Satisfies` and resolved by `FindBestMatch`
- Reject: a `System.Version`-based semver check, a hand-split `lower-upper` range string, a throwing `Parse` in the admission fold, and a string-compare version ordering
