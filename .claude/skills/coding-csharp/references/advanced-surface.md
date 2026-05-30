# Advanced Surface

Verified against production `libs/csharp/` and pinned LanguageExt / Thinktecture / MathNet packages. **Not** basic `Map`/`Bind`/`Create`. Canonical detail: `docs/external-libs/` (`languageext/operators.md`, `prelude.md`, `combinators.md`, `thinktecture/union-attributes.md`, `mathnet/rasm.md`).

---

## Operator Disambiguation

| Symbol | Carrier | Semantics | Production |
| ------ | ------- | --------- | ------------ |
| `\|` | `Option<T>` | LanguageExt alternative | moderate |
| `\|` | `[Flags]` enum | Bitwise OR | common |
| `\|` | Rasm struct (`RepaintRequest`, `Subscription`, `FileOverride<T>`, …) | Hand absorption / priority lattice | heavy |
| `\|` | `Schedule` | Chain retry transformers | **unused in libs** |
| `&` | `Validation<E,A>` | Applicative product (LE); Rasm uses `.Apply().As()` | **unused** |
| `+` | `Error` | Monoid append when folding faults (`Ui.cs`) | GH boundary |
| `+` | Rasm struct (`Requirement`, `VectorField`, receipts, `OverlayDecision`, …) | Hand semigroup append | heavy |
| `-`, `*` | `VectorField` / `ScalarField` / `Direction` | Field/direction algebra | Vectors |

Not used in Rasm production: Kleisli `>>`/`<<`; Mapster `.Adapt` (one GH2 boundary call); LanguageExt `Decision` type (**does not exist** in LE v5).

---

## Rasm Decision Monoids (not LanguageExt)

| Type | Namespace | Identity / combine |
| ---- | --------- | ------------------ |
| `OverlayDecision` | `Rasm.Rhino.UI` | `Ignore`, `operator +` |
| `Decision` | `Rasm.Grasshopper.Components` | `Pass`, `Handled`, `operator +` |

---

## Thinktecture + Rasm Codegen

| Attribute | Effect |
| --------- | ------ |
| `[Union(SwitchMapStateParameterName = "…")]` | State-threaded `.Switch(ctx, …)` |
| `[GenerateUnionOps]` (Rasm.Domain) | Per-case `Op SelfOp = Op.Of(nameof(Case))` |
| `[SkipUnionOps]` (Rasm.Domain) | Opt out of CSP0802 SelfOp requirement |
| `[UseDelegateFromConstructor]` | SmartEnum or union-case delegate behavior |
| `[ComplexValueObject]` | Multi-field VO — partial class/struct only |
| `[ValidationError<TFault>]` | Factory `ValidateFactoryArguments(ref TFault?, …)` |
| `[ValueObject<T>(KeyMemberName, KeyMemberAccessModifier, …)]` | Branded scalar |
| `[KeyMemberEqualityComparer<T>]` / `[KeyMemberComparer<T>]` | Key comparison policy |

Thinktecture `[Union]` does **not** generate `operator +`/`|` in TT 10.2.0. Hand operators are on separate types or out-of-scope namespaces.

Generic sums: `PromptTransition<T>`, `MotionSpec<T>`, `GhUiRequest<T>` — plain `abstract record` + manual `switch`.

---

## LanguageExt Advanced Combinators (production-weighted)

| Combinator | Weight |
| ---------- | ------ |
| `.TraverseM(f).As()` | heavy — `.As()` mandatory in v5 |
| `.Traverse(identity).As()` | moderate |
| `.Choose(f)` | heavy |
| `.MapFail` / `.BindFail` / `.IfFail` | boundary-heavy |
| `.BiBind(Succ:, Fail:)` | rare |
| `(a,b,c).Apply(f).As()` | validation |
| `guard` | heavy |
| `Optional(x).ToFin(Fail:…)` | heavy |
| `Try.lift<Fin<T>>().Run()` | native boundaries |
| `Eff.runtime<RT>()` / `Env.Asks` / `Eff.Lift` | common |
| `Atom.Swap` / `SwapMaybe` | common |

Unused in production: `@catch`, `.Retry`, `guardnot`, `BiMap`, Schedule algebra, trait arrows (`ComposeK` — skill `transforms.md` only).

Validation shapes: `Validation<Error,T>`; `Validation<Seq<UiFault>,T>`; `[ValidationError<TFault>]` on VOs. **`Validation<string,T>` does not compile in v5.**

Prelude (global): `Some`/`None`, `Optional`, `toSeq`, `toHashMap`, `unit`, `identity`. Time units: `LanguageExt.UnitsOfMeasure.ms` (not global).

---

## Rasm Op Boundary

| Member | Role |
| ------ | ---- |
| `Op.Of(name).OrDefault()` | Branded operation key |
| `op.Need(value)` | Null admission |
| `op.Catch(() => Fin<T>)` | Exception capsule |
| `Op.SideWhen` | Conditional host effect |
| `op.AcceptValidated<TVO>(value)` | VO → `Fin` |
| `op.ValidateParallel` / `AcceptAll` | GH UI batch validation |
| `[BoundaryAdapter]` | CSP analyzer contract |

---

## MathNet / CSparse (Vectors)

| Surface | Notes |
| ------- | ----- |
| `SmallestEigenpairsDetailed`, `SolveDetailed` | Public sparse API |
| `BiCgStab` + preconditioner + stop criteria | MathNet iterative |
| `CholeskySparse` | CSparse SPD |
| `SolveReceipt` + `SolvePath`/`SolveStop` SmartEnums | Records + enums |
| Cloud Sinkhorn / quantile | `DenseMatrixD`, `SortedArrayStatistics.Quantile` |

MathNet.Symbolics: pinned, zero production references.

---

## Load With

| Task | Also read |
| ---- | --------- |
| Union dispatch + SelfOp policy | `objects.md` §5.1, `union-attributes.md` |
| Op boundary + validation shapes | `effects.md`, `languageext/rasm.md` §4 |
| SpringConfig / UiFault pattern | `Motion.cs`, `objects.md` §2.1 |
| Trait recursion (schematic) | `transforms.md` |
