# Rasm.Element LanguageExt/Thinktecture Audit

- `libs/dotnet/Rasm.Element/.planning/Graph/element.md:53`
  - from: `[ValueObject<string>]` on `NodeId`, while consumers read generated `NodeId.Value`
  - to: keep the default-private key; replace every `.Value` read with the generated `.ToValue()`
  - why: Thinktecture already emits key projection; exposing another public key member would enlarge the owner surface.

- `libs/dotnet/Rasm.Element/.planning/Graph/element.md:109`
  - from: default-private `[ValueObject<string>]` plus hand-written `PredefinedType.Token => Value`
  - to: delete `Token`; replace `.Token`/`.Value` reads with the generated `.ToValue()`
  - why: generated key projection preserves the token while deleting the invalid forwarding member and adding no public key property.

- `libs/dotnet/Rasm.Element/.planning/Properties/property.md:44`
  - from: `[ValueObject<string>]` on `PropertyName`, while canonical and wire consumers read `.Value`
  - to: keep the declaration; replace `.Value` reads with generated `.ToValue()`
  - why: Thinktecture's existing projection reaches the private key without adding a public member.

- `libs/dotnet/Rasm.Element/.planning/Composition/material.md:57`
  - from: default-private `MaterialId` key while `WireCodec.ToWire`, node canonicalization, and inheritance read `.Value`
  - to: replace `.Value` reads with generated `.ToValue()`; keep the key private
  - why: the generated projection is the existing owner surface and avoids minting a new public key property.

- `libs/dotnet/Rasm.Element/.planning/Composition/material.md:512`
  - from: default-private `Currency` key while `MaterialPropertySet.CanonicalBytes` reads `c.Currency.Value`
  - to: keep the key private; replace `c.Currency.Value` with `c.Currency.ToValue()`
  - why: Thinktecture already generates key egress, so no new public member is required.

- `libs/dotnet/Rasm.Element/.planning/Properties/quantity.md:124`
  - from: default-private `QuantityType` key while registry, wire, formatting, and fault paths read `.Value`
  - to: keep the default-private key; replace `.Value` reads with generated `.ToValue()`
  - why: the existing generated projection fixes every inconsistent read without increasing module surface.

- `libs/dotnet/Rasm.Element/.planning/Relations/relation.md:173`
  - from: default-private `WireName`/`RoleName` keys while canonical and table paths read `.Value`
  - to: keep both keys private; replace `.Value` reads with generated `.ToValue()`
  - why: Thinktecture supplies key egress directly and no public key members need be generated.

- `libs/dotnet/Rasm.Element/.planning/Query/predicate.md:47`
  - from: default-private `WalkDepth` key while `PredicateKey.Write` reads `walk.Depth.Value`
  - to: keep `[ValueObject<int>]`; write `walk.Depth.ToValue()`
  - why: Thinktecture's generated key projection is already present and avoids adding another owner member.

- `libs/dotnet/Rasm.Element/.planning/Projection/address.md:39,83,115`
  - from: local uppercase-only `Hex.Admit` plus string-returning `ContentAddress.ToValue()`/`BlobKey.ToValue()` beside generated key projections
  - to: delete all three; parse through `ContentHash.Admit`, render with `ContentHash.Hex(value.ToValue())`, and retain generated raw-key `ToValue()`
  - why: kernel `ContentHash` owns the exact lowercase round trip; Thinktecture already emits the otherwise-colliding raw-key projection.

- `libs/dotnet/Rasm.Element/.planning/Assessment/observation.md:392`
  - from: `value.Match(Some: Fin.Succ, None: () => new ElementFault.ValueRejected(...))`
  - to: `value.ToFin(new ElementFault.ValueRejected(key, $"<observation-representative-absent:{column}>"))`
  - why: `Option.ToFin(Error)` is the existing lossless presence-to-result transition and removes the manual arm pair.

- `libs/dotnet/Rasm.Element/.planning/Graph/element.md:223`
  - from: nested `Option.Match` arms manually minting `Fin.Succ` and two absence faults in `ResolveRepresentation`
  - to: `At(slot).ToFin(absentFault).Bind(hash => slot.Decode(this, hash).ToFin(unresolvableFault))`
  - why: the two checks are dependent; LanguageExt `ToFin` plus `Bind` preserves both distinct faults without nested carrier elimination.

- `libs/dotnet/Rasm.Element/.planning/Graph/table.md:534`
  - from: `graph.Find<Node.Object>(id).Match(Some: Fin.Succ, None: () => Fin.Fail<Node.Object>(...))`
  - to: `graph.Find<Node.Object>(id).ToFin(new ElementFault.NodeAbsent(key, $"<tabulate-root-absent:{id.ToValue()}>"))`
  - why: `Option.ToFin` is exactly the existing optional-lookup admission and removes a redundant manual result reconstruction.

- `libs/dotnet/Rasm.Element/.planning/Properties/quantity.md:341`
  - from: `SiUnit(...).Match(None: () => Fin.Fail(...), Some: unit => key.Catch(...).Bind(Admit))`
  - to: `SiUnit(...).ToFin(missingUnitFault).Bind(unit => key.Catch(() => Fin.Succ(quantity.ToUnit(unit))).Bind(si => Admit(si, key)))`
  - why: the unit lookup and conversion are dependent result steps; `ToFin`/`Bind` retain the same typed and exceptional failures.

- `libs/dotnet/Rasm.Element/.planning/Geospatial/coverage.md:217`
  - from: `Grid.Coarsen(key).Match(Succ: next => Gate(...), Fail: refusal => Gate(false, refusal))`
  - to: `Grid.Coarsen(key).ToValidation().Bind(next => Gate(next == pair.Item2.Grid, key, "<coverage-level-off-coarsen-chain>", ...))`
  - why: `Fin.ToValidation` preserves the original refusal and `Validation.Bind` expresses the dependent grid comparison without rebuilding failure.

- `libs/dotnet/Rasm.Element/.planning/Query/predicate.md:110`
  - from: `ConcurrentDictionary<string, Regex> CompiledPatterns` plus the forwarding `Compiled(string)` member
  - to: one `Func<string, Regex> Compiled = memo(static pattern => new Regex(...))`; call it from `Of` and `Decide`
  - why: LanguageExt `memo(Func<A,B>)` owns the synchronized pure-function cache; one delegate replaces the table plus lookup wrapper.

- `libs/dotnet/Rasm.Element/.planning/Graph/wirepayload.md:170`
  - from: equality/key ladders plus `UnreachableException` in `ToWire(ObjectKind|ReleaseVersion|RepresentationSlot)`
  - to: each method calls the receiver's generated exhaustive `Switch`, one arm per existing wire mapping
  - why: Thinktecture already owns closed-row dispatch; a new row then compile-breaks instead of reaching a runtime default.

- `libs/dotnet/Rasm.Element/.planning/Graph/wire.md:132`
  - from: `ToWire(UncertaintyKind)` equality ladder ending in `UnreachableException`
  - to: `value.Switch(exact: ..., absolute: ..., relative: ..., interval: ..., normal: ...)`
  - why: generated SmartEnum dispatch is exhaustive and shorter while preserving the exact protobuf mapping.

- `libs/dotnet/Rasm.Element/.planning/Graph/wiresubstance.md:238`
  - from: equality ladders plus `UnreachableException` in `ToWire(FireRating|MeasurementBasis)`
  - to: generated exhaustive `value.Switch(...)` mappings
  - why: Thinktecture owns both closed rosters; manual comparisons duplicate them and make growth a runtime failure.

- `libs/dotnet/Rasm.Element/.planning/Graph/wirevalue.md:210`
  - from: manual equality/key ladders in `ToWire(InheritanceMode|EvidenceGrade|Interpolation|AttestationRole)`
  - to: generated exhaustive `value.Switch(...)` mappings
  - why: the generated dispatch removes terminal `UnreachableException` arms and stays compile-exhaustive with the existing owners.

- `libs/dotnet/Rasm.Element/.planning/Graph/wireevidence.md:225`
  - from: six hand-written SmartEnum equality ladders ending in `UnreachableException`
  - to: generated exhaustive `Switch` in `ToWire(Discipline|SamplingKind|ObservationGrade|AssessmentOutcome|SolvePhase|FailureKind)`
  - why: these are Thinktecture-owned closed rosters; generated dispatch preserves every mapping and forces future cases at compile time.

- `libs/dotnet/Rasm.Element/.planning/Composition/acoustic.md:67`
  - from: public `AcousticBand.Count => Items.Count`
  - to: delete `Count`; use `AcousticBand.Items.Count` at every Element/Materials call site
  - why: Thinktecture already exposes the roster; the forwarding property adds module surface and no domain behavior.

- `libs/dotnet/Rasm.Element/.planning/Composition/material.md:508`
  - from: `LifecycleStage.Index`, `ImpactCategory.Index`, and both `Count` wrappers over generated `Key`/`Items.Count`
  - to: delete all four aliases; use `.Key` and `LifecycleStage.Items.Count`/`ImpactCategory.Items.Count`
  - why: the aliases duplicate Thinktecture's generated owner surface and are not independent domain facts.

- `libs/dotnet/Rasm.Element/.planning/Composition/acoustic.md:70`
  - from: two-row payloadless `[SmartEnum<int>] SlideSense` used only as `Sense.Key` in `RatingContour`
  - to: delete `SlideSense`; store the existing `1`/`-1` directly as the closed `RatingContour` row's `Sense` column
  - why: the contour roster already closes the values; the extra generated type and two rows add symbols without behavior or reuse.

- `libs/dotnet/Rasm.Element/.planning/Projection/observe.md:81`
  - from: two-row payloadless `WaiverMark`, its `Of(Option<ConstraintWaiver>)`, and `WaiverMark.Of(f.Waiver).Key`
  - to: delete `WaiverMark`; emit `f.Waiver.IsSome ? "waived" : "unwaived"` at the sole telemetry boundary
  - why: the type merely re-encodes Option presence for one string tag; removing it preserves the emitted values and deletes three module symbols.

- `libs/dotnet/Rasm.Element/.planning/Graph/wirevalue.md:198`
  - from: `(ToInheritance(...), ToEvidenceGrade(...)).Apply(...).As().ToFin()`
  - to: `(ToInheritance(...), ToEvidenceGrade(...)).Apply(...).As()`
  - why: both operands are `Fin`; `As()` already returns `Fin<(InheritanceMode,EvidenceGrade)>`, and `Fin` has no `ToFin()` API.

- `libs/dotnet/Rasm.Element/.planning/Graph/wiresubstance.md:341`
  - from: two `Fin<double[]>` spectra joined by `.Apply(...).As().ToFin()`
  - to: join them with `.Apply(...).As().Bind(...)`; delete `.ToFin()`
  - why: `Apply` is already specialized to `Fin`; re-anchoring lands the concrete carrier and no `Fin.ToFin` exists.

- `libs/dotnet/Rasm.Element/.planning/Composition/material.md:873`
  - from: optional curve `Match` whose present arm joins two `Fin<double>` values with `.As().ToFin()`
  - to: `curve.TraverseM(sample => (sample.AtAdmitted(...), sample.AtAdmitted(...)).Apply(...).As().Bind(...)).As().Map(_ => unit)`
  - why: `Option.TraverseM` owns the absent/present effect inversion, and the inner `As()` is already `Fin`; delete the nonexistent conversion.

- `libs/dotnet/Rasm.Element/.planning/Composition/material.md:781,795,809,819,837,840,915,945`
  - from: repeated `Option.Match(Some: validate.Map(Some), None: Success(None))` validation shells
  - to: each optional column uses `option.Traverse(validate).As()`; `Rayleigh` traverses its tuple `Apply` the same way
  - why: `Option.Traverse` preserves `None`, lifts `Some`, and keeps every `Validation<Error,_>` failure for the surrounding applicative fan-in.

- `libs/dotnet/Rasm.Element/.planning/Geospatial/coverage.md:151,211`
  - from: `AdmittedRange` and `AdmittedBlocks` manually rebuild `Validation.Success` for absent options
  - to: `range.Traverse(validate).As().Map(_ => unit)` and `level.Block.Traverse(validate).As().Map(_ => unit)`
  - why: LanguageExt's `Option.Traverse` is total over absence and preserves the existing accumulating validation on presence.

- `libs/dotnet/Rasm.Element/.planning/Assessment/observation.md:262`
  - from: `cadence.Match(Some: span => In(...).Map(_ => unit), None: () => Success(unit))`
  - to: `cadence.Traverse(span => In(span.TotalSeconds, Band.Positive, "observation-cadence-seconds", key)).As().Map(_ => unit)`
  - why: `Option.Traverse` already expresses optional accumulating admission and removes the hand-built success arm.

- `libs/dotnet/Rasm.Element/.planning/Properties/quantity.md:564`
  - from: `DimensionOf(type).Match(Some: Success, None: () => Fail(fault))`
  - to: `DimensionOf(type).ToValidation<Error>(new ElementFault.ValueRejected(...))`
  - why: `Option.ToValidation` is the existing presence-to-accumulating transition and preserves the identical typed refusal.

- `libs/dotnet/Rasm.Element/.planning/Graph/element.md:600`
  - from: `Find<Node.Object>(objectId).Match(Some: root => BakeObject(...), None: () => Fin.Fail(...))`
  - to: `Find<Node.Object>(objectId).ToFin(new ElementFault.NodeAbsent(...)).Bind(root => BakeObject(...))`
  - why: `Option.ToFin` plus `Bind` is the existing dependent lookup rail and removes the manual carrier reconstruction.

- `libs/dotnet/Rasm.Element/.planning/Composition/material.md:61`
  - from: `MaterialId.Of(string) => Create(value)` and its call sites
  - to: delete `Of`; call generated `MaterialId.Create(value)` directly
  - why: the wrapper adds no admission or domain behavior beyond Thinktecture's existing factory.

- `libs/dotnet/Rasm.Element/.planning/Classification/classification.md:102`
  - from: `Discipline.Parse(token, key) => key.AcceptValidated<Discipline>(token)` and its Persistence caller
  - to: delete `Parse`; call `key.AcceptValidated<Discipline>(token)` at the existing boundary
  - why: the existing kernel bridge preserves the same generated SmartEnum validation and `KernelFault.InvalidValue`; the forwarding symbol adds no behavior.

- `libs/dotnet/Rasm.Element/.planning/Composition/material.md:418,529,554`
  - from: `FireRating.Parse` plus unused `MeasurementBasis.Parse`/`ImpactCategory.Parse`, each forwarding to kernel roster admission
  - to: delete all three; replace live `FireRating.Parse` calls with `key.Row<string, FireRating>(token)`
  - why: `Op.Row` is the pre-existing generated-roster admission owner, including the comparer-aware `ImpactCategory.Name` lookup.

- `libs/dotnet/Rasm.Element/.planning/Composition/material.md:475`
  - from: `FireResistance.Rei/R/Ei/I` factory siblings forwarding to `Of(FireCoverage, minutes, key)`
  - to: delete them; update callers to `FireResistance.Of(FireCoverage.*, minutes, key)`
  - why: `FireCoverage` already carries the criterion discriminants; the named aliases add no behavior and widen the module surface.

- `libs/dotnet/Rasm.Element/.planning/Projection/address.md:52,108`
  - from: `ContentAddress.Of(UInt128) => Create(contentHash)` and `BlobKey.Of(UInt128) => Create(digest)`
  - to: delete both forwarding overloads; call each generated `Create(UInt128)` directly
  - why: Thinktecture already owns raw-key admission; the span-taking `Of` hashes bytes, while these overloads only rename `Create`.

- `libs/dotnet/Rasm.Element/.planning/Assessment/assessment.md:52`
  - from: `AnalysisRoute.Of(token, key) => key.AcceptValidated<AnalysisRoute>(token)` and its Element/Compute callers
  - to: delete `Of`; call `key.AcceptValidated<AnalysisRoute>(token)` at every existing boundary
  - why: kernel admission is the existing owner, and the package ruling explicitly deletes wrappers on `[ValidationError]` value objects.

- `libs/dotnet/Rasm.Element/.planning/Assessment/observation.md:54`
  - from: `SensorId.Of(token, key) => key.AcceptValidated<SensorId>(token)` and its wire/corpus calls
  - to: delete `Of`; call `key.AcceptValidated<SensorId>(token)` at those boundaries
  - why: kernel admission already preserves the generated validation and operation key; the `[ValidationError]` wrapper is ruled redundant.

- `libs/dotnet/Rasm.Element/.planning/Properties/quantity.md:132`
  - from: unused `QuantityType.Of(name, key) => key.OrDefault().AcceptValidated<QuantityType>(name)`
  - to: delete `Of`; use `key.OrDefault().AcceptValidated<QuantityType>(name)` if an untrusted-name boundary lands
  - why: the kernel bridge already owns the exact generated admission, and the package ruling deletes generic-token wrappers.

- `libs/dotnet/Rasm.Element/.planning/Composition/material.md:519`
  - from: `Currency.Parse(code, key) => key.AcceptValidated<Currency>(code)` and its Element/Materials callers
  - to: delete `Parse`; call `key.AcceptValidated<Currency>(code)` at every existing boundary
  - why: the kernel bridge invokes the same generated normalization and `ValidationError` lowering; the alias adds no behavior.

- `libs/dotnet/Rasm.Element/.planning/Relations/relation.md:115`
  - from: `CardinalPoint.Of(reference, key) => key.Row<int, CardinalPoint>(reference)` used once by `MaterialUsage.Of`
  - to: delete `Of`; traverse with `reference => key.Row<int, CardinalPoint>(reference)`
  - why: kernel `Op.Row` already owns generated-roster lookup and its typed refusal; the alias only enlarges the roster surface.

- `libs/dotnet/Rasm.Element/.planning/Graph/delta.md:303`
  - from: `nodes.Find(realizing).Match(Some: n => ..., None: () => Fin.Fail(new ElementFault.NodeAbsent(...)))`
  - to: `nodes.Find(realizing).ToFin(new ElementFault.NodeAbsent(...)).Bind(n => n is Node.Object ? Fin.Succ(unit) : new ElementFault.RelationshipInvalid(...))`
  - why: LanguageExt `Option.ToFin` plus `Bind` preserves the distinct absent and wrong-kind failures without rebuilding the carrier arms.

- `libs/dotnet/Rasm.Element/.planning/Properties/property.md:226`
  - from: nested `low.Match(... high.Match(...))` returning `false` from both `None` arms
  - to: `low.Bind(lo => high.Map(hi => lo.Si > hi.Si)).IfNone(false)`
  - why: existing `Option.Bind`, `Map`, and `IfNone` express the same both-present predicate without duplicated absence branches.

- `libs/dotnet/Rasm.Element/.planning/Projection/audit.md:117`
  - from: two-row `SweepOrigin`, the `AuditCategory.Origin` column, and nine constructor arguments whose value is never read
  - to: delete the type, column, and arguments; retain each row's existing `Sweep` delegate
  - why: the delegate already owns all sweep behavior, including `BakeRejected`'s empty population arm; the parallel tag has no consumer.

- `libs/dotnet/Rasm.Element/.planning/Graph/corpus.md:49`
  - from: two-row `LaneUse` and `CorpusLane.Use`, assigned on every lane but never read
  - to: delete `LaneUse`, the `Use` column, and the second constructor argument from each `CorpusLane` row
  - why: deterministic seeding reads only the generated lane `Key`; the extra roster and column carry no behavior.

- `libs/dotnet/Rasm.Element/.planning/Graph/corpus.md:482`
  - from: `CorpusProfile.Of(...).Match(Succ: profile => profile, Fail: _ => throw new InvalidOperationException(message))`
  - to: `CorpusProfile.Of(...).IfFail(_ => throw new InvalidOperationException(message))`
  - why: LanguageExt `Fin.IfFail(Func<Error,A>)` preserves the success value and the exact declaration-defect exception without manual elimination.
