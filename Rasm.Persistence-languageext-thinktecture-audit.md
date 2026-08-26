# Rasm.Persistence LanguageExt / Thinktecture Audit

`libs/dotnet/Rasm.Persistence/.planning/Store/redrive.md:85`
`from` `TransportPosture(Status, None)`
`to` `Status.Match(Some: status => TransportPosture(status, None), None: static () => Retriability.Transient)`
`why` `Status` is `Option<int>`, while the local owner takes `int`; LanguageExt `Option.Match` preserves the connection-without-status transient posture.

`libs/dotnet/Rasm.Persistence/.planning/Store/redrive.md:165`
`from` `toSeq(stated).Head().Bind(...)`
`to` `toSeq(stated).Head.Bind(...)`
`why` LanguageExt `Seq.Head` is the existing `Option<T>` property; no `Head()` member exists.

`libs/dotnet/Rasm.Persistence/.planning/Query/cypher.md:200`
`from` `steps.Filter(...).Map(s => Decode(...)).TraverseM(identity).As()`
`to` `steps.Filter(...).TraverseM(s => Decode(...)).As()`
`why` LanguageExt `TraverseM(f)` already maps and short-circuit-sequences the `Fin`; the intermediate `Seq<Fin<SetKey>>` is redundant.

`libs/dotnet/Rasm.Persistence/.planning/Query/federation.md:233`
`from` `toSeq(set.Inputs).Map(input => Visit(input, state)).TraverseM(identity).As()`
`to` `toSeq(set.Inputs).TraverseM(input => Visit(input, state)).As()`
`why` LanguageExt `TraverseM(f)` is the existing fused first-failure traversal and preserves input order and failure semantics.

`libs/dotnet/Rasm.Persistence/.planning/Query/federation.md:319`
`from` `toSeq(literal.Values.Expressions).Map(row => row.Fields is ... ? Fin.Succ(...) : Fin.Fail(...)).TraverseM(identity).As()`
`to` `toSeq(literal.Values.Expressions).TraverseM(row => row.Fields is ... ? Fin.Succ(...) : Fin.Fail(...)).As()`
`why` LanguageExt `TraverseM(f)` directly sequences the same per-row `Fin` and deletes the intermediate carrier roster without changing its existing caught-factory failure.

`libs/dotnet/Rasm.Persistence/.planning/Version/commits.md:803`
`from` `Permutations(ops).Map(order => Crdt.Seed(...).Bind(...)).TraverseM(identity).As()`
`to` `Permutations(ops).TraverseM(order => Crdt.Seed(...).Bind(...)).As()`
`why` LanguageExt `TraverseM(f)` owns the same ordered short-circuit fold; `Map(...).TraverseM(identity)` is a redundant FP shell.

`libs/dotnet/Rasm.Persistence/.planning/Query/datasets.md:327`
`from` `rows.Map(fact).Traverse(Cells).As().ToFin()`
`to` `rows.Traverse(row => Cells(fact(row))).As().ToFin()`
`why` LanguageExt `Traverse(f)` already maps while applicatively accumulating every `Cells` failure; the prior projection adds no behavior.

`libs/dotnet/Rasm.Persistence/.planning/Query/federation.md:490`
`from` `Atom<HashMap<UInt128, FederatedResult>>`, `hold.Swap(m => m.AddOrUpdate(key, value))`, and `hold.Value.Find(key)`
`to` `AtomHashMap<UInt128, FederatedResult>`, `hold.AddOrUpdate(key, value)`, and `hold.Find(key)`
`why` LanguageExt `AtomHashMap` is the existing keyed shared cell; it preserves CAS semantics while avoiding a whole-map swap and removes both wrapper accesses.

`libs/dotnet/Rasm.Persistence/.planning/Store/provisioning.md:1359`
`from` `held.Match(Some: value => vault.Unwrap(...).Map(opened => Some(opened)), None: () => Fin.Succ(None))`
`to` `held.TraverseM(value => vault.Unwrap(space, key.Span, value)).As()`
`why` LanguageExt `Option.TraverseM` is absence-total: `None` lifts unchanged and `Some` runs the same `Fin` once, deleting both manual arms.

`libs/dotnet/Rasm.Persistence/.planning/Store/provisioning.md:1364`
`from` `members.Fold(Fin.Succ(Seq<ReadOnlyMemory<byte>>()), (held, member) => held.Bind(opened => vault.Unwrap(...).Map(value => opened.Add(value))))`
`to` `members.TraverseM(member => vault.Unwrap(space, key.Span, member)).As()`
`why` LanguageExt `TraverseM` already performs the ordered first-failure `Fin` sequence and returns the same `Seq` without a hand-built accumulator.

`libs/dotnet/Rasm.Persistence/.planning/Store/provisioning.md:1368`
`from` `rows.Fold(Fin.Succ(Seq<(ReadOnlyMemory<byte>, ReadOnlyMemory<byte>)>()), (held, row) => held.Bind(opened => vault.Unwrap(...).Map(value => opened.Add((row.Key, value)))))`
`to` `rows.TraverseM(row => vault.Unwrap(space, row.Key.Span, row.Value).Map(value => (Key: row.Key, Value: value))).As()`
`why` LanguageExt `TraverseM` preserves row order and first failure while replacing the manual `Fin`/`Seq` accumulation.

`libs/dotnet/Rasm.Persistence/.planning/Store/provisioning.md:493`
`from` `demand.Floors.ToSeq().Choose(floor => ... floor.Key ... floor.Value ...)`
`to` `demand.Floors.AsIterable().ToSeq().Choose(floor => ... floor.Key ... floor.Value ...)`
`why` LanguageExt `HashMap.ToSeq()` returns values only; `AsIterable()` is the existing named pair walk required by both projections.

`libs/dotnet/Rasm.Persistence/.planning/Store/provisioning.md:667,1144`
`from` both `dek.Match(Some: key => HandleBridge.Status(...), None: () => Fin.Succ(unit))` branches
`to` `dek.TraverseM(key => HandleBridge.Status(...)).As().Map(static _ => unit)` at 667 and `policy.Dek.TraverseM(key => HandleBridge.Status(...)).As()` at 1144
`why` LanguageExt `Option.TraverseM` already lifts absence to successful `Fin<Option<Unit>>` and runs the same status check once when present; the first projection preserves its declared `Fin<Unit>`.

`libs/dotnet/Rasm.Persistence/.planning/Ingest/tabular.md:331`
`from` `Op.Of().Catch(() => Fin.Succ(codec())).Match(Succ: value => (Validation<Error, TValue>)value, Fail: e => (Validation<Error, TValue>)TabularFault.Lift(e))`
`to` `Op.Of().Catch(() => Fin.Succ(codec())).MapFail(TabularFault.Lift).ToValidation()`
`why` LanguageExt `Fin.MapFail` plus `Fin.ToValidation` performs the same typed failure projection and carrier ingress without manual elimination.

`libs/dotnet/Rasm.Persistence/.planning/Ingest/schedule.md:435`
`from` `Op.Of().Catch(() => Fin.Succ(codec())).Match(Succ: value => (Validation<Error, TValue>)value, Fail: e => (Validation<Error, TValue>)ScheduleFault.Lift(e))`
`to` `Op.Of().Catch(() => Fin.Succ(codec())).MapFail(ScheduleFault.Lift).ToValidation()`
`why` LanguageExt already owns both operations; the existing `ScheduleFault.Lift` error identity is preserved exactly.

`libs/dotnet/Rasm.Persistence/.planning/Ingest/geospatial.md:317`
`from` `Op.Of().Catch(() => Fin.Succ(codec())).Match(Succ: value => (Validation<Error, TValue>)value, Fail: e => GeoIngestFault.Lift(format, e))`
`to` `Op.Of().Catch(() => Fin.Succ(codec())).MapFail(e => GeoIngestFault.Lift(format, e)).ToValidation()`
`why` LanguageExt `MapFail` preserves the format-aware fault lift before the existing direct `Fin.ToValidation` conversion.

`libs/dotnet/Rasm.Persistence/.planning/Ingest/pointcloud.md:250`
`from` the `Fin.Match` that casts success to `Validation` and conditionally unwraps `ScanRefusal` on failure
`to` `Op.Of().Catch(() => Fin.Succ(codec())).MapFail(e => e.Exception.Case is ScanRefusal refusal ? refusal.Fault : e).ToValidation()`
`why` LanguageExt `MapFail` and `ToValidation` preserve the exact refusal fault and unchanged foreign error while deleting the manual carrier fold.

`libs/dotnet/Rasm.Persistence/.planning/Ingest/tabular.md:364`
`from` nested `Fin.Match` and `Optional(bound).Match` branches constructing `Validation<Error, T>`
`to` `Op.Of().Catch(...).MapFail(TabularFault.Lift).ToValidation().Bind(bound => Optional(bound).ToValidation((Error)new TabularFault.CellCast(None, row.At, Some(typeof(T).Name))))`
`why` LanguageExt `Fin.ToValidation` and `Option.ToValidation` are the existing two admissions; exception lifting and null-deserialization refusal remain unchanged.

`libs/dotnet/Rasm.Persistence/.planning/Ingest/tabular.md:538`
`from` the `Fin.Match` casting copied rows to `Validation` and wrapping failure in `TabularFault.BulkRefused`
`to` `IO.liftAsync(async () => (await Op.Of().Catch(...)).MapFail(error => new TabularFault.BulkRefused(error)).ToValidation())`
`why` LanguageExt `MapFail` followed by `ToValidation` preserves the exact bulk fault and success value without manual branch construction.

`libs/dotnet/Rasm.Persistence/.planning/Element/codec.md:116`
`from` `Find(...).Match(Some: Fin.Succ, None: () => Fin.Fail<SnapshotCodec>(new CodecFault.NoMutualCodec(...)))`
`to` `Find(...).ToFin(new CodecFault.NoMutualCodec(surface.Key))`
`why` LanguageExt `Option.ToFin` is the exact presence-to-result admission and preserves the same selected row or refusal.

`libs/dotnet/Rasm.Persistence/.planning/Element/identity.md:1111`
`from` `Op.Of().Catch(...).Match(Succ: _ => Fin.Succ(new SchemaVerdict.Serving()), Fail: error => Fin.Fail(new IdentityFault.ApplyFailed(error)))`
`to` `Op.Of().Catch(...).Map(static _ => (SchemaVerdict)new SchemaVerdict.Serving()).MapFail(static error => new IdentityFault.ApplyFailed(error))`
`why` LanguageExt `Map`/`MapFail` preserve both branches without manually reconstructing the same `Fin` carrier.

`libs/dotnet/Rasm.Persistence/.planning/Element/identity.md:1117`
`from` `IO.liftAsync(...).Bind(result => result.Match(Succ: IO.pure, Fail: error => IO.fail<SchemaVerdict>(new IdentityFault.ApplyFailed(error))))`
`to` `IO.liftAsync(...).Bind(result => IO.lift(result.MapFail(static error => new IdentityFault.ApplyFailed(error))))`
`why` LanguageExt `Fin.MapFail` plus `IO.lift(Fin<A>)` performs the existing failure projection and carrier lift directly.

`libs/dotnet/Rasm.Persistence/.planning/Ingest/geospatial.md:442`
`from` `held.Match(Some: collection => (Validation<Error, FeatureCollection>)collection, None: () => new GeoIngestFault.PayloadRejected(...))`
`to` `held.ToValidation((Error)new GeoIngestFault.PayloadRejected("geojson", "<null-document>"))`
`why` LanguageExt `Option.ToValidation` preserves the present collection and the exact absent-document fault without a manual carrier fold.

`libs/dotnet/Rasm.Persistence/.planning/Ingest/schedule.md:483`
`from` `Optional(key).Match(Some: value => (Validation<Error, int>)value, None: () => new ScheduleFault.RowUnkeyed(...))`
`to` `Optional(key).ToValidation((Error)new ScheduleFault.RowUnkeyed(row, detail.IfNone("<unnamed>")))`
`why` LanguageExt `Option.ToValidation` is the same nullable-key admission and deletes both hand-built arms.

`libs/dotnet/Rasm.Persistence/.planning/Query/backend.md:588`
`from` `TimeSpine.Validate(token, null, out TimeSpine? spine)` followed by `Fail<Error, TimeSpine>(ValidationError)` / `Success`
`to` `Op.Of().Row<string, TimeSpine>(token).ToValidation()`
`why` Thinktecture `ValidationError` is not a LanguageExt `Error`; the existing Domain `Op.Row` bridge performs generated smart-enum lookup on the correct carrier.

`libs/dotnet/Rasm.Persistence/.planning/Query/backend.md:626`
`from` `Identifier.Validate(raw, null, out Identifier admitted)` followed by `Fail<Error, Identifier>(ValidationError)` / `Success`
`to` `Op.Of().AcceptValidated<Identifier>(raw).ToValidation()`
`why` Domain `AcceptValidated` is the existing Thinktecture-to-LanguageExt bridge and maps ephemeral factory evidence to the kernel fault plane.

`libs/dotnet/Rasm.Persistence/.planning/Query/backend.md:653`
`from` `ColumnType.Validate(token, null, out ColumnType? type)` plus manual `Validation` construction
`to` `Op.Of().Row<string, ColumnType>(token).MapFail(_ => new BackendFault.Unprovisioned($"<column-type:{token}>")).ToValidation()`
`why` Domain `Op.Row` uses Thinktecture's generated roster and `MapFail` preserves the existing backend-specific refusal without manual arms.

`libs/dotnet/Rasm.Persistence/.planning/Query/columnar.md:523`
`from` `stamp.Bind(ParseStamp).Match(Some: Fin<UInt128>.Succ, None: () => Fin<UInt128>.Fail(new ColumnarFault.UnstampedArtifact(artifact)))`
`to` `stamp.Bind(ParseStamp).ToFin(new ColumnarFault.UnstampedArtifact(artifact))`
`why` LanguageExt `Option.ToFin` preserves the successful stamp parse and identical absent-or-malformed refusal.

`libs/dotnet/Rasm.Persistence/.planning/Query/federation.md:379`
`from` `SetPath.Validate(fields[segment.Field], null, out SetPath path) is null ? Some(path) : None`
`to` `SetPath.TryCreate(fields[segment.Field], out SetPath path) ? Some(path) : None`
`why` Thinktecture already generates `TryCreate`; the caller discards validation detail, so `Validate` manufactures evidence it never uses.

`libs/dotnet/Rasm.Persistence/.planning/Query/lakehouse.md:188`
`from` `Optional(Path.GetDirectoryName(published)).Match(Some: directory => Publish(...), None: () => Fin.Fail<long>(fault))`
`to` `Optional(Path.GetDirectoryName(published)).ToFin(new ColumnarFault.PolicyRefused("generation-directory", published)).Bind(directory => Publish(batches, published, directory, declaration.Fields(metadata), order, custody))`
`why` LanguageExt `ToFin` plus `Bind` is the existing dependent admission and preserves the exact missing-directory failure.

`libs/dotnet/Rasm.Persistence/.planning/Query/retrieval.md:406`
`from` `corpus.Head.Match(Some: first => Fitted(...), None: () => Fin.Fail<ProductCodebook>(new RetrievalFault.EmptyCorpus()))`
`to` `corpus.Head.ToFin(new RetrievalFault.EmptyCorpus()).Bind(first => Fitted(corpus, first.Length, subspaces, codesPerSubspace, passes))`
`why` LanguageExt `Option.ToFin` performs the same non-empty admission before the dependent fit without reconstructing `Fin` arms.

`libs/dotnet/Rasm.Persistence/.planning/Query/retrieval.md:716`
`from` `positions.Head.Match(Some: first => Fin.Succ(new DocumentHit(...)), None: () => Fin.Fail<DocumentHit>(fault))`
`to` `positions.Head.ToFin(new RetrievalFault.Mismatched("snippet-positions", "at least one match position", "none")).Map(first => new DocumentHit(kind.Key, subject, member, title, first.Start, first.Length, snippet, score))`
`why` LanguageExt `ToFin` and `Map` preserve the same head requirement and success projection with no manual carrier construction.

`libs/dotnet/Rasm.Persistence/.planning/Query/serving.md:128`
`from` `Last.Match(Some: name => Identifier.Validate(...) ? Fin.Fail(ValidationError) : Fin.Succ(Quote(admitted)), None: () => Fin.Fail(missing))`
`to` `toSeq(readRelation.NamedTable?.Names ?? []).Last.ToFin(new BackendFault.Unlowerable(state.Backend.Key, "<unnamed-relation>")).Bind(name => Op.Of().AcceptValidated<Identifier>(name).Map(state.Backend.Quote))`
`why` Domain `AcceptValidated` correctly crosses Thinktecture factory evidence into LanguageExt; `ValidationError` cannot be returned as `Error`.

`libs/dotnet/Rasm.Persistence/.planning/Query/serving.md:180`
`from` `Names.TraverseM(name => Identifier.Validate(...) ? Fin.Fail<string>(ValidationError) : Fin.Succ(Quote(admitted)))`
`to` `Names.TraverseM(name => Op.Of().AcceptValidated<Identifier>(name).Map(state.Backend.Quote))`
`why` The existing Domain generated-admission bridge preserves ordered first-failure traversal and removes invalid cross-carrier error use.

`libs/dotnet/Rasm.Persistence/.planning/Query/serving.md:400`
`from` `toSeq(plan.Relations).Last.Match(Some: root => new BackendPlan().Visit(root, scope), None: () => Fin.Fail<string>(fault))`
`to` `toSeq(plan.Relations).Last.ToFin(new BackendFault.Unlowerable(scope.Backend.Key, "<empty-plan>")).Bind(root => new BackendPlan().Visit(root, scope))`
`why` LanguageExt `Option.ToFin` and `Bind` are the same empty-plan gate and dependent lowering without manual `Fin` arms.

`libs/dotnet/Rasm.Persistence/.planning/Query/serving.md:631`
`from` `schema.Columns.Find(...).Match(Some: column => column.Type.Wire.ToValidation<Error>().Map(...), None: () => Fail<Error, Option<...>>(fault))`
`to` `schema.Columns.Find(column => column.Name == schema.Time).ToValidation((Error)new BackendFault.Unprovisioned($"<schema-spine:{schema.Dataset}>")).Bind(column => column.Type.Wire.ToValidation().Map(wire => Some((schema.Time, wire))))`
`why` LanguageExt `Option.ToValidation(error)` preserves the missing-column fault, while `Fin.ToValidation()` admits the dependent wire with its fixed `Error` type and takes no generic argument.

`libs/dotnet/Rasm.Persistence/.planning/Query/datasets.md:113,130-133,210-213,318,373,375-380`
`from` every `Fin<T>.ToValidation<Error>()` call at these sites
`to` `Fin<T>.ToValidation()`
`why` LanguageExt `Fin` already fixes its failure carrier to `Error` and publishes only the parameterless `ToValidation()`; the explicit type argument names a nonexistent overload.

`libs/dotnet/Rasm.Persistence/.planning/Query/backend.md:348,582`
`from` both `Fin<T>.ToValidation<Error>()` calls
`to` `Fin<T>.ToValidation()`
`why` LanguageExt's `Fin.ToValidation()` needs no error type parameter because `Fin` is already `Error`-typed; the current generic spelling cannot bind.

`libs/dotnet/Rasm.Persistence/.planning/Query/serving.md:535,674,675`
`from` the remaining `Fin<T>.ToValidation<Error>()` calls
`to` `Fin<T>.ToValidation()`
`why` LanguageExt exposes only parameterless `Fin.ToValidation()` and preserves the same successes and fixed `Error` failures without an invented generic overload.

`libs/dotnet/Rasm.Persistence/.planning/Version/recovery.md:259`
`from` `.ToValidation<Error>()` on the `Fin<string>` restore result
`to` `.ToValidation()`
`why` LanguageExt `Fin.ToValidation()` already returns `Validation<Error,T>`; no error-type generic parameter exists or changes the carrier.

`libs/dotnet/Rasm.Persistence/.planning/Store/provisioning.md:183`
`from` `ExtensionAdmission.Admissible(...) => this switch { Preload ..., BaseType ..., AccessMethod ..., Standalone ..., _ => false }`
`to` `this.Switch(preload: p => preloaded.Contains(p.Library), baseType: b => created.Admits(b.Extension), accessMethod: static _ => true, standalone: static _ => true)`
`why` Thinktecture's generated total `Switch` removes the impossible default and restores compile-time exhaustiveness when a case is added.

`libs/dotnet/Rasm.Persistence/.planning/Store/provisioning.md:1080`
`from` `.Match(Succ: Fin<T>.Succ, Fail: fault => Disposed<T>(store, fault))` plus the single-caller `Disposed<T>` helper
`to` `.BindFail(fault => { store.Dispose(); return Fin<T>.Fail(fault); })`
`why` LanguageExt `Fin.BindFail` runs the same failure-only disposal, preserves the original error, and deletes one module-level symbol.

`libs/dotnet/Rasm.Persistence/.planning/Version/ledger.md:990`
`from` `Lifted(fin)`, `Dialed(ioFin)`, and the two forwarding helpers through line 1013
`to` `IO.lift(fin)` and `ioFin.Bind(IO.lift)`; delete `Lifted<T>` and `Dialed<T>`
`why` LanguageExt `IO.lift(Fin<A>)` is the existing success/failure lowering, and `IO.Bind` already composes it with `IO<Fin<A>>`; both module symbols are behavior-free.

`libs/dotnet/Rasm.Persistence/.planning/Store/observability.md:104,157,355`
`from` the three `IO.liftAsync(... Fin<T> ...).Bind(IO.liftFin)` chains
`to` `IO.liftAsync(... Fin<T> ...).Bind(IO.lift)`
`why` LanguageExt publishes `IO.lift(Fin<A>)` and no `IO.liftFin`; the existing bind still lowers the async-carried `Fin` onto the same `IO` error channel.

`libs/dotnet/Rasm.Persistence/.planning/Store/placement.md:141,160`
`from` both `IO.lift(() => Op.Of().Catch(... Fin<T> ...)).Bind(IO.liftFin)` chains
`to` the existing `IO.lift(() => Op.Of().Catch(... Fin<T> ...))` with the trailing bind deleted
`why` LanguageExt's result-typed `IO.lift(Func<Fin<A>>)` already returns `IO<A>`; `liftFin` does not exist and a second lowering is both invalid and redundant.

`libs/dotnet/Rasm.Persistence/.planning/Store/provisioning.md:457,519`
`from` both `IO.liftAsync(... Fin<T> ...).Bind(IO.liftFin)` chains
`to` `.Bind(IO.lift)` at the same two sites
`why` The async lift carries `Fin<T>` as its value, and LanguageExt's existing `IO.lift(Fin<A>)` is the exact lowering function; `IO.liftFin` is absent.

`libs/dotnet/Rasm.Persistence/.planning/Store/blobstore.md:436,441,447,613`
`from` the four `IO.liftFin` binds
`to` `.Bind(IO.lift)` after the async lifts at 436/447/613, and delete the bind after result-typed `IO.lift(() => Fin<Unit>)` at 441
`why` LanguageExt has `IO.lift(Fin<A>)` but no `liftFin`; its `Func<Fin<A>>` overload already lowers the synchronous finalizer without a second bind.

`libs/dotnet/Rasm.Persistence/.planning/Query/lakehouse.md:68,94,262`
`from` the three `IO.liftFin` binds
`to` `.Bind(IO.lift)` after the async lift at 68, and delete the binds after `IO.lift(() => Fin<T>)` at 94/262
`why` LanguageExt's settled-result lift lowers the async carrier, while its result-typed thunk overload already answers `IO<T>`; no `liftFin` API exists.

`libs/dotnet/Rasm.Persistence/.planning/Query/cypher.md:189`
`from` `IO.liftAsync(... Fin<Duration> ...).Bind(IO.liftFin)`
`to` `IO.liftAsync(... Fin<Duration> ...).Bind(IO.lift)`
`why` LanguageExt `IO.lift(Fin<A>)` is the exact settled-result lowering and `IO.liftFin` is not a published member.

`libs/dotnet/Rasm.Persistence/.planning/Query/lane.md:130,135,157,160,179`
`from` the five `IO.liftFin` binds
`to` `.Bind(IO.lift)` after async lifts at 130/157/179, and delete the binds after result-typed `IO.lift(() => Fin<T>)` at 135/160
`why` LanguageExt separates async value lifting from settled-`Fin` lowering, while `IO.lift(Func<Fin<A>>)` already combines both; `liftFin` exists in neither surface.

`libs/dotnet/Rasm.Persistence/.planning/Version/ingress.md:209`
`from` `IO.liftAsync(... Fin<IngressTally> ...).Bind(IO.liftFin)`
`to` `IO.liftAsync(... Fin<IngressTally> ...).Bind(IO.lift)`
`why` LanguageExt's existing `IO.lift(Fin<A>)` preserves the same success/failure lowering from the async-carried result; `liftFin` is nonexistent.

`libs/dotnet/Rasm.Persistence/.planning/Query/retrieval.md:592`
`from` `IO.liftAsync(... Fin<T> ...).Bind(IO.liftFin)`
`to` `IO.liftAsync(... Fin<T> ...).Bind(IO.lift)`
`why` LanguageExt publishes the settled `Fin` overload under `IO.lift`; changing only the method name preserves the cached value and error channel.

`libs/dotnet/Rasm.Persistence/.planning/Query/columnar.md:252,273,290,301,331,370`
`from` the six `IO.liftFin` binds
`to` `.Bind(IO.lift)` after async lifts at 252/290/331, and delete the binds after result-typed `IO.lift(() => Fin<T>)` at 273/301/370
`why` LanguageExt has no `liftFin`; `IO.lift(Fin<A>)` lowers carried async results, while `IO.lift(Func<Fin<A>>)` already returns `IO<A>` for synchronous bodies.

`libs/dotnet/Rasm.Persistence/.planning/Query/cache.md:492,606`
`from` both `IO.liftAsync(... Fin<T> ...).Bind(IO.liftFin)` chains
`to` `.Bind(IO.lift)` at both sites
`why` LanguageExt's settled-result `IO.lift` preserves the same lowering before cache composition; `IO.liftFin` is not in the package surface.

`libs/dotnet/Rasm.Persistence/.planning/Element/graph.md:295,343,355,370`
`from` the four `IO.liftAsync(... Fin<Option<T>> ...).Bind(IO.liftFin)` chains
`to` `.Bind(IO.lift)` at all four sites
`why` LanguageExt `IO.lift(Fin<A>)` lowers each async-carried optional result exactly; the named `liftFin` member does not exist.

`libs/dotnet/Rasm.Persistence/.planning/Version/retention.md:436`
`from` `IO.liftFin(Seq(...).TraverseM(...).As().Map(static _ => unit))`
`to` `IO.lift(Seq(...).TraverseM(...).As().Map(static _ => unit))`
`why` The argument is already a settled `Fin<Unit>`, which LanguageExt lifts whole through `IO.lift(Fin<A>)`; no `liftFin` alias exists.

`libs/dotnet/Rasm.Persistence/.planning/Version/timetravel.md:189,209,243`
`from` the three direct `IO.liftFin(fin)` calls
`to` `IO.lift(fin)` at all three sites
`why` LanguageExt's settled-result overload is the exact `Fin`-to-`IO` bridge and preserves every success and failure without an invented API name.

`libs/dotnet/Rasm.Persistence/.planning/Version/recovery.md:140,152`
`from` the two `IO.liftFin` binds
`to` `.Bind(IO.lift)` after the async lift at 140, and delete the bind after result-typed `IO.lift(() => Fin<T>)` at 152
`why` LanguageExt lowers the async-carried result with `IO.lift(Fin<A>)`, while the synchronous result thunk is already lowered by its selected overload.

`libs/dotnet/Rasm.Persistence/.planning/Element/codec.md:436,439,446,449`
`from` all four `IO.lift(() => Fin<T>).Bind(IO.liftFin)` chains
`to` keep each existing `IO.lift(() => Fin<T>)` and delete the trailing bind
`why` LanguageExt overload selection chooses `IO.lift(Func<Fin<A>>)` and directly answers `IO<A>`; the nonexistent `liftFin` adds no legal or semantic step.

`libs/dotnet/Rasm.Persistence/.planning/Version/egress.md:70,192,205,220,229`
`from` the five `IO.liftFin` uses
`to` `.Bind(IO.lift)` at 70/192/205 and direct `IO.lift(fin)` at 220/229
`why` LanguageExt's one settled-result overload serves both the async/carried binds and direct metric `Fin<Unit>` values; `IO.liftFin` is absent from the package.

`libs/dotnet/Rasm.Persistence/.planning/Version/provenance.md:440`
`from` `IO.liftFin(WitnessedHead.Canonical(...))`
`to` `IO.lift(WitnessedHead.Canonical(...))`
`why` `Canonical` already returns `Fin<WitnessedHead>` and LanguageExt `IO.lift(Fin<A>)` preserves its exact settled success or failure.

`libs/dotnet/Rasm.Persistence/.planning/Store/provisioning.md:1136,1169`
`from` `IO.lift(() => <Fin<BackupState>>)` and `IO.lift(() => <Fin<long>>)` in methods declared `IO<Fin<...>>`
`to` `IO.lift<Fin<BackupState>>(() => ...)` and `IO.lift<Fin<long>>(() => ...)`
`why` LanguageExt otherwise selects `IO.lift(Func<Fin<A>>)` and returns `IO<A>`; the explicit type argument is the documented way to carry, rather than lower, the `Fin` value.

`libs/dotnet/Rasm.Persistence/.planning/Query/federation.md:399,416`
`from` both `IO.lift(() => <Fin<LoweringTarget/Plan>>).Bind(result => result.Match(...))` chains
`to` `IO.lift<Fin<LoweringTarget>>(() => ...).Bind(...)` and `IO.lift<Fin<Plan>>(() => ...).Bind(...)`
`why` LanguageExt's bare result-thunk overload lowers to `IO<A>`, leaving no `Fin.Match`; explicit `IO.lift<Fin<A>>` preserves the currently intended carried branch fold and `IO<Fin<FederatedResult>>` surface.

`libs/dotnet/Rasm.Persistence/.planning/Query/columnar.md:334,344,362`
`from` the three bare `IO.lift(() => Fin<T>)` bodies declared as `IO<Fin<T>>`
`to` `IO.lift<Fin<ArrowPartitions>>`, `IO.lift<Fin<IArrowArrayStream>>`, and `IO.lift<Fin<AdbcConnection>>` respectively
`why` LanguageExt's result-typed thunk overload normally lowers to `IO<T>`; explicit generic lifting is required to preserve these deliberately carried result values.

`libs/dotnet/Rasm.Persistence/.planning/Version/ingress.md:176`
`from` `IO.lift(() => key.Catch(...))` in `Close`, whose declared result is `IO<Fin<Unit>>`
`to` `IO.lift<Fin<Unit>>(() => key.Catch(...))`
`why` LanguageExt's bare `Func<Fin<A>>` overload answers `IO<A>`; the explicit type argument is the existing package mechanism for carrying the `Fin` unchanged.

`libs/dotnet/Rasm.Persistence/.planning/Version/provenance.md:283`
`from` `nodes.GroupBy(...).Map(...)` and `edges.GroupBy(...).Map(...)`
`to` `toSeq(nodes.GroupBy(...)).Map(...)` and `toSeq(edges.GroupBy(...)).Map(...)`
`why` LINQ `GroupBy` exits `Seq`; LanguageExt exposes no `Map` on `IEnumerable`, so each grouped run must re-enter through `Prelude.toSeq` before carrier mapping.

`libs/dotnet/Rasm.Persistence/.planning/Version/provenance.md:302`
`from` the three `Option.Map(...).IfNoneUnsafe(() => null)` JSON-LD members at lines 302, 309, and 310
`to` `option.Match<string?>(Some: projection, None: static () => null)`
`why` LanguageExt v5 has no `IfNoneUnsafe`; `Option.Match` is the existing total nullable boundary fold and preserves the emitted nulls.

`libs/dotnet/Rasm.Persistence/.planning/Version/provenance.md:85`
`from` `Items.Find(cls => actor.Roles.Contains(cls.Key)).IfNone(Person)`
`to` `toSeq(Items).Find(cls => actor.Roles.Contains(cls.Key)).IfNone(Person)`
`why` Thinktecture `Items` is `IReadOnlyList<AgentClass>` and LanguageExt publishes no `Find` on `IEnumerable`; `toSeq` is the required carrier admission.

`libs/dotnet/Rasm.Persistence/.planning/Version/ledger.md:393`
`from` `toSeq(ColumnFamily.Items.Filter(static f => f.Durable))`
`to` `toSeq(ColumnFamily.Items).Filter(static f => f.Durable)`
`why` Thinktecture `Items` has no `Filter`; the generated roster must enter LanguageExt `Seq` before the carrier operation is selected.

`libs/dotnet/Rasm.Persistence/.planning/Ingest/pointcloud.md:169`
`from` `toSeq(regions).Map(row => new ScanRegion(... row.Key ... row.Value ...))`
`to` `regions.AsIterable().ToSeq().Map(row => new ScanRegion(... row.Key ... row.Value ...))`
`why` `HashMap<K,V>` has two enumerable constructions, so `toSeq(regions)` cannot infer a pair shape; LanguageExt `AsIterable` is the named key-value walk.

`libs/dotnet/Rasm.Persistence/.planning/Ingest/pointcloud.md:211`
`from` both `batch.*.Map(_ => new T[...]).IfNoneUnsafe(() => null)` allocations
`to` `batch.*.Match<T[]?>(Some: _ => new T[...], None: static () => null)`
`why` LanguageExt v5 removed `IfNoneUnsafe`; total `Option.Match` preserves the nullable arrays and deletes nonexistent API use.

`libs/dotnet/Rasm.Persistence/.planning/Store/coordination.md:426,521`
`from` `toSeq(amounts).Filter(...)` and `toSeq(amounts).OrderBy(...).ToSeq()`
`to` `amounts.AsIterable().ToSeq().Filter(...)` and `toSeq(amounts.AsIterable().OrderBy(...))`
`why` LanguageExt `HashMap.AsIterable` selects named key-value pairs, while `Prelude.toSeq` must re-admit the LINQ ordered run; both original spellings fail inference or binding.

`libs/dotnet/Rasm.Persistence/.planning/Version/merge.md:382`
`from` `w.Sorted(r.ByIdentifier.ToSeq(), pair => pair.Key.Key, ..., (pair, x) => ... pair.Value ...)`
`to` `w.Sorted(r.ByIdentifier.AsIterable().ToSeq(), pair => pair.Key.Key, ..., (pair, x) => ... pair.Value ...)`
`why` LanguageExt `HashMap.ToSeq()` returns values only; `AsIterable()` is the existing pair carrier required by the key and value projections.

`libs/dotnet/Rasm.Persistence/.planning/Query/backend.md:285`
`from` `cells.Distinct().ToSeq()` and `roster.Zip(Range(0, roster.Count))`
`to` `cells.Distinct()` and `roster.Map((cell, index) => (cell, index))`
`why` LanguageExt `Seq.Distinct` already returns `Seq`, and indexed `Seq.Map` owns the ordinal pairing without feeding non-`Seq` `Range` to `Seq.Zip`.

`libs/dotnet/Rasm.Persistence/.planning/Version/timetravel.md:91`
`from` the four `Deltas.Filter(...).Map(...).Distinct().ToSeq()` projections through line 94
`to` the same four projections ending at `.Distinct()`
`why` LanguageExt `SeqExtensions.Distinct` already returns concrete `Seq<T>`; every trailing `ToSeq` is a behavior-free carrier round trip.

`libs/dotnet/Rasm.Persistence/.planning/Store/placement.md:271`
`from` `(ClientSealed sealed_, { IsSome: true }) => sealed_.Keyring.Unwrap(dek.ValueUnsafe(), ...)`
`to` `(ClientSealed sealed_, { IsSome: true, Case: WrappedKey key }) => sealed_.Keyring.Unwrap(key, ...)`
`why` LanguageExt exposes the proven `Option.Case` in the existing pattern; capturing it removes the unsafe unwrap without changing the exhaustive switch.

`libs/dotnet/Rasm.Persistence/.planning/Query/lane.md:402`
`from` `WalkDepth.Validate(bound, null, out WalkDepth admitted) is null ? Fin.Succ(admitted) : Fin.Fail(new SelectionFault.Depth(bound))`
`to` `Op.Of().AcceptValidated<WalkDepth>(bound).MapFail(_ => new SelectionFault.Depth(bound))`
`why` Domain `AcceptValidated` is the existing Thinktecture-to-`Fin` bridge; `MapFail` retains the selection-owned refusal without manual factory evidence handling.

`libs/dotnet/Rasm.Persistence/.planning/Query/serving.md:256`
`from` `QuantileRule.Validate(token, null, out QuantileRule? asked)` plus manual valid/invalid `Fin` arms
`to` `Op.Of().Row<string, QuantileRule>(token).MapFail(_ => new BackendFault.Unlowerable(...)).Bind(asked => asked == QuantileRule.Interpolated ? Fin.Succ(unit) : Fin.Fail<Unit>(...))`
`why` Domain `Op.Row` already lifts Thinktecture smart-enum lookup; the dependent convention check and both existing backend faults remain unchanged.

`libs/dotnet/Rasm.Persistence/.planning/Query/retrieval.md:709`
`from` `DocumentPredicate.TryGet(admitted.Predicate, out DocumentPredicate? row) ? Fin.Succ(row) : Fin.Fail<DocumentPredicate>(...)`
`to` `Op.Of().Row<string, DocumentPredicate>(admitted.Predicate).MapFail(_ => new RetrievalFault.Mismatched(...))`
`why` Domain `Op.Row` is the existing generated-roster-to-`Fin` bridge and `MapFail` preserves the retrieval-specific mismatch evidence.

`libs/dotnet/Rasm.Persistence/.planning/Ingest/schedule.md:51,164,194`
`from` the eight one-caller `ScheduleDirection` through `ScheduleUnit.Of` lookups, including `ConstraintKind.ByWire` and `ScheduleUnit.ByWire`
`to` inline `toSeq(Owner.Items).Find(row => row.Wire == wire)` at their call sites, with `.IfNone(existingDefault)` for `RecurrenceKind`, `DayKind`, `DependencyKind`, and `ScheduleUnit`
`why` Thinktecture `Items` is the existing roster and LanguageExt `Find` preserves each optional/default policy; delete all eight `Of` symbols and both duplicate dictionaries.

`libs/dotnet/Rasm.Persistence/.planning/Store/blobstore.md:199,222,252`
`from` the three `present.Match(Some: IO.pure/projection, None: () => IO.fail<...>(new RemoteStoreFault.NotFound(...)))` folds and the twin at `Store/placement.md:304`
`to` `IO.lift(present.ToFin(new RemoteStoreFault.NotFound(...)).Map(existingProjection))`, omitting `Map` where success is unchanged
`why` LanguageExt `Option.ToFin` plus `IO.lift(Fin<A>)` is the existing presence-to-effect path and preserves every success projection and exact missing-blob error.

`libs/dotnet/Rasm.Persistence/.planning/Query/lane.md:452`
`from` `held.Fold(Fin.Succ(Seq<SetKey>()), (acc, operand) => acc.Bind(... Evaluate ... carried + one.Keys))`
`to` `held.TraverseM(operand => Evaluate(operand, scope, resolve).Map(static one => one.Keys)).As().Map(rows => KeySelection.Of(rows.Bind(identity), scope))`
`why` LanguageExt `TraverseM` already performs the ordered first-failure evaluation; the final `Bind(identity)` preserves the same key concatenation without a hand-built `Fin` accumulator.

`libs/dotnet/Rasm.Persistence/.planning/Query/retrieval.md:466`
`from` `coded.Fold(Fin.Succ(unit), (held, row) => held.Bind(_ => <row checks>))`
`to` `coded.TraverseM(row => <same row checks>).As().Map(static _ => unit)`
`why` LanguageExt `TraverseM` owns the same ordered first-failure check and removes the manual `Fin<Unit>` sequencing shell.

`libs/dotnet/Rasm.Persistence/.planning/Version/commits.md:66`
`from` `other.Slots.Fold(... s.Key ... s.Value ...)` and `other.Slots.ForAll(s => ... s.Key ... s.Value)`
`to` `other.Slots.AsIterable().Fold(...)` and `other.Slots.AsIterable().ForAll(...)`
`why` LanguageExt folds a two-parameter `HashMap` as values alone; `AsIterable` is the existing named key-value carrier required by both vector operations.

`libs/dotnet/Rasm.Persistence/.planning/Version/commits.md:361`
`from` `r.Origins.Fold(Fin.Succ(l), (acc, row) => acc.Bind(held => Counter(held, row.Key, row.Value)))`
`to` `r.Origins.AsIterable().FoldM(l, static (held, row) => Counter(held, row.Key, row.Value)).As()`
`why` LanguageExt `AsIterable` exposes the keyed row and `FoldM` owns the dependent `Fin` state fold; the original generic `HashMap.Fold` sees values only.

`libs/dotnet/Rasm.Persistence/.planning/Ingest/tabular.md:262`
`from` `Run(TabularOp op) => Executed(op)` plus the private `Executed(TabularOp op) => op.Switch(...)`
`to` `Run(TabularOp op) => op.Switch(...)` with the existing generated dispatch arms; delete `Executed`
`why` Thinktecture already supplies the exhaustive `TabularOp.Switch`; the forwarding method adds no policy and costs one module-level symbol and one fenced line.

`libs/dotnet/Rasm.Persistence/.planning/Store/observability.md:443`
`from` `Batch(rows, frame) => Batched(rows, frame)` plus private `Batched(rows, frame) => ArrowLanding.Build(...)`
`to` `Batch(rows, frame) => ArrowLanding.Build(...)` with the existing arguments; delete `Batched`
`why` `ArrowLanding.Build` is the existing target-folder owner and the private forwarding layer adds no persistence behavior; inlining removes one module symbol and one fenced line.

`libs/dotnet/Rasm.Persistence/.planning/Store/schema.md:43,60`
`from` both `new ValidationError(string.Join(" | ", new object?[] { value }))` calls
`to` `ValidationError.Create(value)`
`why` Thinktecture's existing `ValidationError.Create(string)` mints the same message directly; the singleton array and join cannot change output.

`libs/dotnet/Rasm.Persistence/.planning/Query/retrieval.md:310,318,326`
`from` each `new ValidationError(string.Join(" | ", new object?[] { message }))`
`to` `ValidationError.Create(message)`
`why` Thinktecture's existing message factory preserves the sole formatted message exactly; the one-element array and join are dead machinery.

`libs/dotnet/Rasm.Persistence/.planning/Query/columnar.md:55,66,75,83`
`from` the four `new ValidationError(string.Join(" | ", new object?[] { ... }))` calls
`to` `ValidationError.Create(message)` for singleton messages and `ValidationError.Create($"execution-threads | {value.ToString(CultureInfo.InvariantCulture)}")` at line 75
`why` Thinktecture's message factory accepts the finished evidence; direct interpolation preserves the sole two-part rendering while deleting every object array and join.

`libs/dotnet/Rasm.Persistence/.planning/Query/lane.md:103,280,291`
`from` each `new ValidationError(string.Join(" | ", new object?[] { message }))`
`to` `ValidationError.Create(message)`
`why` Thinktecture's existing factory receives the identical sole message; allocating and joining a one-element object array is redundant.

`libs/dotnet/Rasm.Persistence/.planning/Query/cypher.md:64,73,81,89,98`
`from` each `new ValidationError(string.Join(" | ", new object?[] { message }))`
`to` `ValidationError.Create(message)`
`why` Thinktecture already owns single-message evidence through `ValidationError.Create`; the join has no second value to delimit.

`libs/dotnet/Rasm.Persistence/.planning/Ingest/geospatial.md:127`
`from` `new ValidationError(string.Join(" | ", new object?[] { $"<geo-spec:{string.Join(',', broken)}>" }))`
`to` `ValidationError.Create($"<geo-spec:{string.Join(',', broken)}>")`
`why` Thinktecture's message factory takes the completed evidence directly; the inner join remains the real composition while the outer singleton join is inert.

`libs/dotnet/Rasm.Persistence/.planning/Ingest/pointcloud.md:75,77`
`from` both `new ValidationError(string.Join(" | ", new object?[] { message }))` calls
`to` `ValidationError.Create(message)`
`why` Thinktecture's message factory preserves both literal errors exactly and removes the pointless singleton arrays and joins.

`libs/dotnet/Rasm.Persistence/.planning/Ingest/schedule.md:324,326`
`from` both `new ValidationError(string.Join(" | ", new object?[] { message }))` calls
`to` `ValidationError.Create(message)`
`why` Thinktecture already mints validation evidence from one message through `ValidationError.Create`; no delimiter can appear for a singleton input.

`libs/dotnet/Rasm.Persistence/.planning/Ingest/tabular.md:97,117,140,142,144,146`
`from` each `new ValidationError(string.Join(" | ", new object?[] { message }))`
`to` `ValidationError.Create(message)`
`why` Thinktecture's message factory accepts every literal directly; the six one-element object arrays and joins preserve no information or ordering.

`libs/dotnet/Rasm.Persistence/.planning/Store/provisioning.md:1288`
`from` `refused.IsSome ? refused.ValueUnsafe() : transaction.Commit()`
`to` `refused.IfNone(transaction.Commit)`
`why` LanguageExt `Option.IfNone(Func<T>)` preserves the lazy commit fallback and returns the present LMDB refusal directly, without an unsafe unwrap.

`libs/dotnet/Rasm.Persistence/.planning/Element/identity.md:87`
`from` `IdentityShapeRow.Of(StoreProfile profile) => Get(profile.Key)`
`to` delete `Of`; use the generated `IdentityShapeRow.Get(profile.Key)` at any consumer
`why` Thinktecture already owns keyed lookup, and this unused forwarding alias adds one module symbol with no admission or policy.

`libs/dotnet/Rasm.Persistence/.planning/Element/identity.md:1083`
`from` `IdentityDdl.Extensions(required) => required.Map(ext => ext.CreateSql)`
`to` delete `Extensions`; project `required.Map(static ext => ext.CreateSql)` where the sequence is consumed
`why` `ServerExtension.CreateSql` is the existing owner and the unused one-line projection adds no DDL behavior.

`libs/dotnet/Rasm.Persistence/.planning/Query/cache.md:330-334`
`from` the `SolverMemo` class whose `Get` calls `resolve` and whose `Put` calls `record`
`to` delete `SolverMemo`; invoke the already-injected `resolve(kind, identity)` and `record(row)` delegates directly
`why` both methods are behavior-free forwarding shells over the existing persistence ports; deleting them removes one class and two module symbols.
