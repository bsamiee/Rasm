# [RASM_APPHOST_API_NUGET_VERSIONING]

`NuGet.Versioning` owns the SemVer-2.0 grammar, the `[min,max)` interval algebra, and mode-scoped version comparison — the version leg of the `SupplyChainGate.Admit` supply-chain decision that `System.Version` cannot express. `NuGetVersion` parses the host plugin-contract version, `VersionRange.Satisfies` decides admission, and `FindBestMatch` resolves the newest compatible candidate among several. Only the version, range, and comparer surface is admitted; package-graph resolution and framework compatibility stay out of scope.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `NuGet.Versioning`
- package: `NuGet.Versioning` (Apache-2.0)
- assembly: `NuGet.Versioning`
- namespace: `NuGet.Versioning`
- asset: runtime library; multi-target `net8.0`/`net472`, the `net10.0` consumer binds `lib/net8.0` by TFM precedence
- rail: supply-chain

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: version values, range algebra, and comparison

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]  | [CAPABILITY]                   |
| :-----: | :-------------------------- | :------------- | :----------------------------- |
|  [01]   | `SemanticVersion`           | class          | SemVer-2.0 base value type     |
|  [02]   | `NuGetVersion`              | class          | the gate's version type        |
|  [03]   | `VersionRange`              | class          | `[min,max)` interval value     |
|  [04]   | `VersionRangeBase`          | abstract class | range bounds and membership    |
|  [05]   | `FloatRange`                | class          | float grammar spec             |
|  [06]   | `VersionComparer`           | sealed class   | mode-specific comparer statics |
|  [07]   | `IVersionComparer`          | interface      | equality and ordering contract |
|  [08]   | `VersionComparison`         | enum           | comparison modes               |
|  [09]   | `NuGetVersionFloatBehavior` | enum           | float behavior modes           |
|  [10]   | `VersionFormatter`          | class          | custom format specifiers       |

[VersionComparison]: `Default` `Version` `VersionRelease` `VersionReleaseMetadata`
[NuGetVersionFloatBehavior]: `None` `Prerelease` `Revision` `Patch` `Minor` `Major` `AbsoluteLatest` and the `Prerelease*` variants

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: version and range parsing, the admission decision, comparison, and formatting

| [INDEX] | [SURFACE]                                                                      | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :----------------------------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `NuGetVersion.TryParse(string, out NuGetVersion) -> bool`                      | static   | non-throwing host-version parse |
|  [02]   | `NuGetVersion.TryParseStrict(string, out NuGetVersion) -> bool`                | static   | strict SemVer-2.0 parse         |
|  [03]   | `NuGetVersion.Parse(string) -> NuGetVersion`                                   | static   | throwing parse                  |
|  [04]   | `SemanticVersion.TryParse(string, out SemanticVersion) -> bool`                | static   | base non-throwing parse         |
|  [05]   | `VersionRange.TryParse(string, out VersionRange) -> bool`                      | static   | contract-range parse            |
|  [06]   | `VersionRange.TryParse(string, bool, out VersionRange) -> bool`                | static   | floating-aware parse            |
|  [07]   | `VersionRangeBase.Satisfies(NuGetVersion) -> bool`                             | instance | admission predicate             |
|  [08]   | `VersionRangeBase.Satisfies(NuGetVersion, VersionComparison) -> bool`          | instance | mode-scoped predicate           |
|  [09]   | `VersionRange.FindBestMatch(IEnumerable<NuGetVersion>) -> NuGetVersion`        | instance | newest in-range candidate       |
|  [10]   | `VersionRange.IsBetter(NuGetVersion, NuGetVersion) -> bool`                    | instance | pairwise candidate preference   |
|  [11]   | `VersionRangeBase.IsSubSetOrEqualTo(VersionRangeBase) -> bool`                 | instance | policy containment              |
|  [12]   | `VersionComparer.Compare(SemanticVersion, SemanticVersion, VersionComparison)` | instance | explicit-mode comparison        |
|  [13]   | `VersionComparer.Get(VersionComparison) -> IVersionComparer`                   | static   | comparer selection              |
|  [14]   | `NuGetVersion.CompareTo(SemanticVersion, VersionComparison)`                   | instance | explicit-mode comparison        |
|  [15]   | `VersionRange.Combine(IEnumerable<VersionRange>) -> VersionRange`              | static   | widest-covering union           |
|  [16]   | `VersionRange.CommonSubSet(IEnumerable<VersionRange>) -> VersionRange`         | static   | range intersection              |
|  [17]   | `NuGetVersion.ToNormalizedString()`                                            | instance | canonical string                |
|  [18]   | `SemanticVersion.ToFullString()`                                               | instance | full string with metadata       |
|  [19]   | `VersionRange.PrettyPrint()`                                                   | instance | readable fault range            |
|  [20]   | `VersionRange.ToLegacyShortString()`                                           | instance | legacy short range              |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `SemanticVersion` models SemVer-2.0 with `Major`/`Minor`/`Patch`, an ordered `ReleaseLabels` prerelease segment, and `Metadata`, comparing by precedence that ignores build metadata across the full `<`/`<=`/`>`/`>=`/`==`/`!=` operator set. `NuGetVersion : SemanticVersion` adds a fourth `Revision` with `System.Version` interop through `Version` and `IsLegacyVersion` (`Revision > 0`), flags SemVer-2.0 payloads via `IsSemVer2`, and preserves the verbatim input as `OriginalVersion`; `TryParseStrict` rejects the legacy 4-part form that `TryParse` admits. `System.Version` carries no prerelease ordering, metadata, or range syntax, so the admission decision routes through `NuGetVersion`.
- `VersionRange : VersionRangeBase` is the `[min,max)` interval value with `MinVersion`/`MaxVersion` and `IsMinInclusive`/`IsMaxInclusive` bounds; the interval-notation grammar (`[1.0,2.0)`, `(,3.0]`, `1.2.*`, `[1.0.0]`) parses through `TryParse`, `Satisfies` is the membership test, `FindBestMatch` returns the newest in-range candidate, and `IsBetter` drives its pairwise preference. A floating range carries a `FloatRange` whose `NuGetVersionFloatBehavior` selects the floated component; `IsFloating` flags it and `ToNonSnapshotRange` strips the float for a pinned comparison. Static singletons `All`/`AllStable`/`None`/`AllFloating` anchor policy, `Combine` unions and `CommonSubSet` intersects several ranges into one.
- `VersionComparer` exposes four mode statics — `Default`/`Version` numeric-only, `VersionRelease` adding prerelease, `VersionReleaseMetadata` everything — and `Get(VersionComparison)` resolves the `IVersionComparer` a mode-scoped `Satisfies`/`CompareTo` consumes. `VersionFormatter.Instance` backs the `N`/`V`/`F`/`R`/`x`/`y`/`z` specifiers `ToString(format, provider)` renders for fault messages.

[STACKING]:
- `Sandbox/admission`(`.planning/Sandbox/admission.md`): the `SupplyChainGate.Admit` version leg parses the subject's declared range through `VersionRange.TryParse` and decides membership with `VersionRange.Satisfies(NuGetVersion)`, accumulating applicatively beside the signature leg so a forged and out-of-contract subject reports both faults; a parse failure on either boundary fails closed as `SupplyChainFault.VersionIncompatible`.
- `api-sigstore`(`.api/api-sigstore.md`): owns the signature and provenance half of that same `Admit` row, this catalog owns the version half `System.Version` cannot express, and the two legs compose one admit verdict.
- within-lib: `FindBestMatch` resolves the newest in-range candidate across several registry versions, and `PrettyPrint`/`ToNormalizedString` render the range and version into the `VersionIncompatible` fault payload.

[LOCAL_ADMISSION]:
- Admission enters through `VersionRange.TryParse` and `NuGetVersion.TryParse` (never `Parse`) at the artifact/host boundary, decided by `Satisfies`; the total fold lowers the `bool` onto the `Validation`/`Fin` rail, so a malformed contract range is a typed denial rather than a throw.
- Only the version/range/comparer surface is admitted; package-graph resolution, framework-compatibility, and the wider NuGet client surface stay out of scope.

[RAIL_LAW]:
- Package: `NuGet.Versioning`
- Owns: SemVer-2.0 parsing, the `[min,max)` range algebra, and version comparison for the `SupplyChainGate` version leg
- Accept: `VersionRange.TryParse` and `NuGetVersion.TryParse` at the artifact/host boundary, decided by `VersionRange.Satisfies` and resolved by `FindBestMatch`
- Reject: a `System.Version`-based semver check, a hand-split `lower-upper` range string, a throwing `Parse` in the admission fold, and a string-compare version ordering
