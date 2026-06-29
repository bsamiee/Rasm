# [COMPUTE_SYMBOLIC_EXPRESSION]

Rasm.Compute symbolic lane head: the closed computer-algebra owner over the admitted `MathNet.Symbolics` F# `Expression` algebra, internalized once as the `SymbolicExpr` `[ComplexValueObject]` that carries the boundary-projected F# `Expression` as a stored member while its identity is the canonical-normal-form `XxHash128` content key ALONE — the `Expression` member is excluded from equality through a `[MemberEqualityComparer]` opt-in on `ContentKey`, never the structural F# DU (which ships `[<StructuralEquality;NoComparison>]`, so a structural compare is undefined and a hash over the normalized `Infix.Format` projection is the only stable identity, and two algebraically-equal builds with distinct raw trees collapse onto one key). The page owns the `SymbolicExpr` value wrapping the boundary-projected F# `Expression`, the one polymorphic `Build` entry discriminating an infix-string parse from a structured construction over the `BuildSpec` `[Union]` (never a `Parse`/`FromExpr`/`FromInfix` factory trio), the `SymbolicOp` `[Union]` operation family folding the verified twelve-module `MathNet.Symbolics` surface — `Calculus.Differentiate`/`Taylor`/`TangentLine`, `Rational`/`Algebraic`/`Trigonometric`/`Polynomial` simplification with `PartialFraction`/`Gcd`/`Coefficients`, `Structure.Substitute`, `Evaluate.Evaluate` over a `symbol→FloatingPoint` map, `Approximate.Approximate`/`ApproximateSubstitute` partial substitution over a `symbol→Approximation` map, `Compile.compileExpression{,1,2,3}` delegate lowering, and `Infix`/`LaTeX` projection (every algebra-module member binds the `[CompilationSourceName]`-carried PascalCase C# name; only the `Compile` and `Operators` modules keep their camelCase F# spelling) — through one generated total `Switch`, never a per-operation method ladder, and the `SymbolicFault` rail extending the existing `ComputeFault` 2200 band at the verified-next-free codes 2213/2214/2215 (parse-failure / undefined-symbol / non-differentiable), one fault family per package, never a parallel `SymbolicError`. Every F# call crosses one boundary: the `Expression` DU, the `FSharpOption<Func<…>>` compile carrier, the `FSharpOption<Expression>` parse carrier, and the `FloatingPoint` evaluation DU are foreign shapes projected at the seam into `SymbolicExpr`, a `Fin<T>` rail, or a raw `double`, and no foreign carrier survives into the interior.

The canonical content key this page mints is the single identity the lane composes by: `Symbolic/dimensional#DIMENSION_PROOF` reads the parsed `SymbolicExpr` DU payloads (`Sum`/`Product`/`Power`/`Function`) for its SI base-dimension fold, and `Symbolic/lowering#LOWERING` keys its read-through `CompiledExpr` cache and its `SymbolicJacobian` partial-derivative lowering (the `SymbolicTape` the `Solver/optimizer#OPTIMIZER_LANE` `DescendAdjoint` folds) off this same `XxHash128` over the simplified form — the differentiate operation and the canonical key both live here, the delegate carrier and the cache plumbing live on `Symbolic/lowering#LOWERING`, and the dimension monomial lives on `Symbolic/dimensional#DIMENSION_PROOF`, three distinct concerns over one shared value. The page is host-local and carries no TS_PROJECTION cluster: the `SymbolicExpr` is an interior value that never sits between wire and rail; its only cross-surface contribution is the content key the branch `csharp:IDEAS#SYMBOLIC_PARAMETRIC_ALGEBRA` seam crosses by reference to the Persistence cost-catalog/QTO-formula consumers, aligned at the key, never coupled into a sibling interior. `FParsec` is consumed transitively through `Infix.Parse` and is never called directly — a registry row only. The single statement exemption is the boundary marshal: the foreign `FloatingPoint`/`FSharpOption` reads in `FloatBox`/`ExpressionCapsule` and the interop `IDictionary` builders the F# `Evaluate`/`ApproximateSubstitute` seams require, all of which read foreign carriers in language-owned control flow to project them into the rail; every other operation is a generated `Switch` fold or a `Bind` chain. An in-proc symbolic-regression fit is the rejected form — offline formula discovery belongs to the Python branch, and this page owns only the analytic algebra over an already-known expression.

## [01]-[INDEX]

- [01]-[SYMBOLIC_EXPR]: `SymbolicExpr` `[ComplexValueObject]`; the F# `Expression` boundary capsule; canonical key.
- [02]-[BUILD_ADMISSION]: `BuildSpec` `[Union]` parse/structured entry; the `SymbolicFault` 2213–2215 rail.
- [03]-[OPERATION_FOLD]: `SymbolicOp` `[Union]` over the twelve-module surface; one total `Apply` switch.

## [02]-[SYMBOLIC_EXPR]

- Owner: `SymbolicExpr` `[ComplexValueObject]` over the F# `Expression`, identity-bearing through its canonical `XxHash128` content key (not the F# `NoComparison` DU); `ExpressionCapsule` the one boundary owner projecting the `FSharpOption<Expression>` parse carrier and the `FSharpOption<Func<…>>` compile carrier into the rail; `ComparerAccessors.StringOrdinal` the ordinal equality/comparer accessor so the free-symbol set folds and sorts deterministically; `FloatBox` the projector collapsing the `FloatingPoint` evaluation DU to a `Fin<double>`.
- Cases: the wrapped `Expression` DU carries fourteen cases (`Number of BigRational` · `Approximation` · `Identifier of Symbol` · `Argument` · `Constant` · `Sum of Expression list` · `Product of Expression list` · `Power of Expression*Expression` · `Function of Function*Expression` · `FunctionN` · `ComplexInfinity` · `PositiveInfinity` · `NegativeInfinity` · `Undefined`), of which the ten data-carrying cases are nested types and the four nullary constants (`ComplexInfinity`/`PositiveInfinity`/`NegativeInfinity`/`Undefined`) are static singletons read through their `Is*` predicates; the `FloatingPoint` projection carries ten arms collapsed to one `Fin<double>` at the `Real`/`PosInf`/`NegInf` numeric leaves and a `non-real` fault on the `Complex`/vector/matrix/`Undef`/`ComplexInf` arms.
- Entry: `SymbolicExpr.Of(Expression expression)` is the interior factory — it routes the generated `[ComplexValueObject]` `Create(expression, default)` whose `NormalizeValidate` derives the content key once from the expression (the passed key slot is always overwritten, so a forged key can never desync identity); `private static string CanonicalProjection(Expression e)` is the canonical-normal-form projector — it normalizes the expression through `Rational.Reduce`, which rebuilds the tree in automatically-simplified, operand-canonical (ASAE) form, and renders it with `Infix.Format` (the package ships no strict/fully-parenthesized infix mode, so the `Rational.Reduce` rewrite is what fixes an order-canonical form before the hash, never a format flag), and `public string Canonical` re-exposes it; `public UInt128 ContentKey` is the `XxHash128.HashToUInt128` over the canonical-form UTF-16 bytes (`MemoryMarshal.AsBytes` reinterprets the `ReadOnlySpan<char>` of the normalized string — the host-local LE char bytes, deterministic across .NET targets, never a UTF-8 transcode), the one identity `dimensional`/`lowering` compose by; `public Seq<string> FreeSymbols` folds `SymbolicExpression.CollectVariables()` into the ordinal-sorted free-identifier set; `public Fin<double> Evaluate(Map<string, double> bindings)` projects through `FloatBox`.
- Auto: the canonical key derives once at construction in `NormalizeValidate` from `CanonicalProjection` (the `Rational.Reduce`-normalized `Infix.Format` output, never the raw display `Infix.Format` of an unnormalized tree) so two structurally distinct but algebraically equal builds that reduce to the same normal form share one key and one cache slot — the normalization fixes the form because `Infix.Format` alone renders the tree it is given without reordering or reducing; `FreeSymbols` reads `CollectVariables()` rather than walking the DU because the wrapper already owns the free-identifier traversal; equality and hashing are the `[ComplexValueObject]`-generated members over `ContentKey` ALONE (the `Expression` member is opt-out of equality through the `[MemberEqualityComparer]` on `ContentKey`), so a `SymbolicExpr` is a dictionary key, a content-address seed, and a receipt fact without a hand-written `GetHashCode`.
- Receipt: `SymbolicExpr` carries no receipt of its own — it is an interior value; the differentiate/compile/evaluate outcomes ride the `lowering` `CompiledExpr` cache receipt and the `optimizer` `Optimization` receipt slot, and a parse/dimension/differentiability fault rides the `ComputeFault` rail at the admitting edge.
- Packages: MathNet.Symbolics, MathNet.Numerics.FSharp, System.IO.Hashing, FSharp.Core (transitive), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new canonical-form policy is one change at `CanonicalProjection`, never a parallel key; a new evaluation-result projection is one `FloatBox` arm; the wrapped DU grows only when `MathNet.Symbolics` adds a case, which the capsule's exhaustive read surfaces as a compile break, never a silent default.
- Boundary: identity is the `ContentKey`, never the F# DU — the `[ComplexValueObject]` carries the `Expression` as a stored member but EXCLUDES it from equality through the `[MemberEqualityComparer]` opt-in on `ContentKey`, so two algebraically-equal builds with distinct raw trees share one identity and a `SortedSet<SymbolicExpr>` keyed on the raw `[<StructuralEquality;NoComparison>]` DU (whose `IComparable`/`CompareTo` is undefined) is the named defect collapsed onto the hash key; the canonical projection is `Infix.Format` over the `Rational.Reduce`-normalized tree (there is no `Infix.formatStrict`/fully-parenthesized mode in the package — `VisualExpressionStyle` carries only `CompactPowersOfFunctions`, so the order-canonical form is produced by the `Rational.Reduce` ASAE rewrite, never a format flag), and rendering the raw unnormalized tree is the rejected key source because two algebraically equal trees can render distinct strings without the reduce pass; the `FloatingPoint` DU never crosses into the interior — `FloatBox` collapses it at the seam, mapping `Real r → Fin.Succ(r)`, `PosInf → +∞`, `NegInf → −∞`, and every `Complex`/`RealVector`/`ComplexVector`/`RealMatrix`/`ComplexMatrix`/`ComplexInf`/`Undef` arm to a `ComputeFault.SymbolUndefined` because the units/optimizer consumers admit only a real scalar, and because the nullary `PosInf`/`NegInf` arms are static singletons (not nested case types) the seam reads them through the `IsPosInf`/`IsNegInf` predicates rather than a type pattern; the `FSharpOption<Expression>` from `Infix.TryParse` and the `FSharpOption<Func<…>>` from `Compile.compileExpression1..3` are projected through `OptionModule.IsSome`/`.Value` exactly once at the capsule and a bare `.Value` on a `None` is the eliminated null-deref; `FParsec` is never referenced — `Infix.Parse`/`TryParse` owns the combinator surface and a direct `FParsec.CharParsers` call is the deleted re-implementation.

```csharp contract
// --- [TYPES] -----------------------------------------------------------------------------

// --- [MODELS] ----------------------------------------------------------------------------
[ComplexValueObject]
public readonly partial struct SymbolicExpr {
    public Expression Expression { get; }

    // ContentKey is the sole identity: the [MemberEqualityComparer] opt-in flips equality to
    // this member alone, so the carried Expression is stored-but-equality-excluded. The key is
    // derived from the expression in NormalizeValidate, never trusted from the Create slot.
    [MemberEqualityComparer<ComparerAccessors.Default<UInt128>, UInt128>]
    public UInt128 ContentKey { get; }

    static partial void NormalizeValidate(ref Expression expression, ref UInt128 contentKey) =>
        contentKey = XxHash128.HashToUInt128(MemoryMarshal.AsBytes(CanonicalProjection(expression).AsSpan()));

    public static SymbolicExpr Of(Expression expression) => Create(expression, default);

    private static string CanonicalProjection(Expression e) =>
        Infix.Format(Rational.Reduce(e));

    public string Canonical => CanonicalProjection(this.Expression);

    public string Display => Infix.Format(this.Expression);

    public string LaTeX => MathNet.Symbolics.LaTeX.Format(this.Expression);

    public Seq<string> FreeSymbols =>
        toSeq(new SymbolicExpression(this.Expression).CollectVariables())
            .Map(static v => v.VariableName)
            .Distinct()
            .OrderBy(static name => name, ComparerAccessors.StringOrdinal.Comparer)
            .ToSeq();

    public Fin<double> Evaluate(Map<string, double> bindings) {
        var symbols = bindings.Fold(
            new Dictionary<string, FloatingPoint>(StringComparer.Ordinal),
            static (map, pair) => { map[pair.Key] = FloatingPoint.NewReal(pair.Value); return map; });
        return FloatBox.Project(MathNet.Symbolics.Evaluate.Evaluate(symbols, this.Expression));
    }
}

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class FloatBox {
    public static Fin<double> Project(FloatingPoint value) =>
        value switch {
            FloatingPoint.Real real => Fin.Succ(real.Item),
            { IsPosInf: true } => Fin.Succ(double.PositiveInfinity),
            { IsNegInf: true } => Fin.Succ(double.NegativeInfinity),
            _ => Fin.Fail<double>(new ComputeFault.SymbolUndefined($"<non-real:{value.GetType().Name}>")),
        };
}

public static class ExpressionCapsule {
    public static Fin<SymbolicExpr> ProjectParse(FSharpOption<Expression> parsed, string source) =>
        OptionModule.IsSome(parsed) && parsed.Value is { } expression && !expression.IsUndefined
            ? Fin.Succ(SymbolicExpr.Of(expression))
            : Fin.Fail<SymbolicExpr>(new ComputeFault.ParseRejected(source));

    public static Fin<TDelegate> ProjectCompile<TDelegate>(FSharpOption<TDelegate> compiled, string canonical)
        where TDelegate : Delegate =>
        OptionModule.IsSome(compiled) && compiled.Value is { } @delegate
            ? Fin.Succ(@delegate)
            : Fin.Fail<TDelegate>(new ComputeFault.NonDifferentiable($"<compile-declined:{canonical}>"));
}
```

## [03]-[BUILD_ADMISSION]

- Owner: `BuildSpec` `[Union]` the single admission carrier discriminating an infix-string parse (`Infix`) from a structured atom/operator construction (`Structured`) so one `Build` entry serves both inputs by shape; `Build` the one polymorphic entrypoint folding the spec through the generated total `Switch`; `SymbolicFault` the abstract base record nested on `ComputeFault` collecting the symbolic-lane failure class, with the three new arms (`ParseRejected` 2213, `SymbolUndefined` 2214, `NonDifferentiable` 2215) extending the verified-next-free 2200 band beside `Text` 2200 … `CacheCorrupt` 2212 — the base every symbolic-lane arm derives from, so `Symbolic/dimensional#DIMENSION_PROOF`'s `DimensionMismatch` (2216) lands as one more `SymbolicFault` arm rather than a second fault family.
- Cases: `BuildSpec` cases `Infix(string Source)` · `Structured(Expression Tree)` (2); `SymbolicFault` arms `ParseRejected` (2213, infix-string `Infix.TryParse` returned `None` or `Undefined`) · `SymbolUndefined` (2214, an evaluation/substitution reduced to a non-real `FloatingPoint` or referenced an unbound free symbol) · `NonDifferentiable` (2215, a `Calculus.Differentiate`/`Compile` step declined — a non-differentiable node or a compile-unsupported `FunctionN`), with 2216 the next-free code `Symbolic/dimensional#DIMENSION_PROOF` claims for `DimensionMismatch`.
- Entry: `public static Fin<SymbolicExpr> Build(BuildSpec spec)` is the one input-shape-dispatched entrypoint over the generated `Switch` — the `Infix` case routes `Infix.TryParse` through `ExpressionCapsule.ProjectParse` so a malformed string is a `ParseRejected` fault, never a thrown `FormatException` and never an `Undefined` leaf masquerading as a value, and the `Structured` case wraps a pre-built `Expression` directly through `SymbolicExpr.Of`; `public static Fin<SymbolicExpr> Build(string infix) => Build(new BuildSpec.Infix(infix))` and `public static SymbolicExpr Build(Expression tree) => SymbolicExpr.Of(tree)` are the two shape-typed conveniences over the one fold, never a parallel parser.
- Auto: `Build` reads `Infix.TryParse` (the non-throwing `FSharpOption<Expression>` form), never `Infix.ParseOrThrow` or `Infix.Parse` (which throws or returns an exception-carrying `Result`), so the parse failure lands on the `Fin` rail at the boundary; the `Structured` case carries an already-validated F# value (minted from `Expression.Symbol`/`Expression.Int32`/the operator surface), so it skips the parse and only computes the canonical key; the three `SymbolicFault` arms are partial-record extensions on the existing `ComputeFault` `[Union]` declared at `Runtime/admission#DISPATCH_SPINE`, so they share the doctrine `Expected` shape, the dual-tier `Create` contract, and the `IValidationError<ComputeFault>` rail without a second fault family.
- Receipt: a build never lands a receipt — it admits a value or a `ComputeFault`; the downstream compile/optimize outcome carries the receipt, and the admitting fault's code (2213/2214/2215) projects through `FaultDetail` at the wire edge exactly as every other `ComputeFault` does.
- Packages: MathNet.Symbolics, FSharp.Core (transitive `FSharpOption`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new input shape is one `BuildSpec` case plus one `Build` arm (e.g. a LaTeX-source admission would be one `BuildSpec.Latex` case routing a LaTeX parser), never a `BuildFromLatex` sibling; a new symbolic failure mode is one `SymbolicFault` arm on the next-free 2200-band code, never a parallel error type — `Symbolic/dimensional#DIMENSION_PROOF` takes 2216 for `DimensionMismatch`, and 2221+ stays free for the next symbolic failure class.
- Boundary: `Build` is the single entry over the generated total `Switch` — a `Parse`/`FromExpr`/`FromInfix` factory trio modeling one concept is the collapsed defect, and the input shape (`string` vs `Expression`) selects the arm, never the call site; the union `Switch` is exhaustive at compile time so a new `BuildSpec` case breaks loudly rather than falling through a runtime-silent `_`; `Infix.TryParse` is the admitted parse surface and `ParseOrThrow`/`Parse` are rejected because they raise or carry an exception `Result` into domain flow; an `Undefined` parse result is a `ParseRejected` fault and never a valid `SymbolicExpr` because `Undefined` is the F# parser's silent-failure sentinel; the `SymbolicFault` arms extend `ComputeFault` and a standalone `SymbolicError`/`ParseError` union is the rejected parallel rail; `FParsec` stays transitive — the `Build` parser never touches the combinator library directly.

```csharp contract
// --- [TYPES] -----------------------------------------------------------------------------
[Union]
public abstract partial record BuildSpec {
    private BuildSpec() { }

    public sealed record Infix(string Source) : BuildSpec;
    public sealed record Structured(Expression Tree) : BuildSpec;
}

// --- [ERRORS] ----------------------------------------------------------------------------
public abstract partial record ComputeFault {
    public abstract partial record SymbolicFault : ComputeFault {
        protected SymbolicFault(string detail, int code) : base(detail, code) { }
    }

    public sealed record ParseRejected : SymbolicFault { public ParseRejected(string detail) : base(detail, 2213) { } }
    public sealed record SymbolUndefined : SymbolicFault { public SymbolUndefined(string detail) : base(detail, 2214) { } }
    public sealed record NonDifferentiable : SymbolicFault { public NonDifferentiable(string detail) : base(detail, 2215) { } }
}

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class SymbolicBuild {
    public static Fin<SymbolicExpr> Build(BuildSpec spec) =>
        spec.Switch(
            infix: static i => ExpressionCapsule.ProjectParse(Infix.TryParse(i.Source), i.Source),
            structured: static s => Fin.Succ(SymbolicExpr.Of(s.Tree)));

    public static Fin<SymbolicExpr> Build(string infix) => Build(new BuildSpec.Infix(infix));

    public static SymbolicExpr Build(Expression tree) => SymbolicExpr.Of(tree);
}
```

## [04]-[OPERATION_FOLD]

- Owner: `SymbolicOp` `[Union]` the closed operation family over the verified twelve-module `MathNet.Symbolics` surface, each case carrying its operand data so one total `Apply` switch dispatches every algebraic transform; `SymbolicForm` `[SmartEnum<string>]` the simplification-axis rows naming the normalization route (`rational` · `algebraic` · `trigonometric` · `polynomial-collected`) folded into one `Simplify` arm; `Apply` the one static fold over `SymbolicOp` returning `Fin<SymbolicExpr>` so a new transform is a case, never a method; `Coefficients` the sibling typed projection returning `Fin<Seq<double>>`, kept off the transform union because a coefficient harvest yields a numeric vector rather than a `SymbolicExpr`.
- Cases: `SymbolicOp` cases `Differentiate(string Symbol)` · `Taylor(string Symbol, int Order, double At)` · `TangentLine(string Symbol, double At)` · `Simplify(SymbolicForm Form, string Symbol)` · `Substitute(Map<string, Expression> Bindings)` · `Approximate(Map<string, double> Bindings)` · `PartialFraction(string Symbol, Seq<SymbolicExpr> DenominatorFactors)` · `Gcd(SymbolicExpr Other, string Symbol)` (8 transform cases, each a `SymbolicExpr → Fin<SymbolicExpr>` arm), with the `Coefficients` harvest the sibling typed projection (a `SymbolicExpr → Fin<Seq<double>>` shape, not a transform) and the `Evaluate`/`Compile`/`Infix`/`LaTeX` terminals owned at their home clusters; `SymbolicForm` rows `rational` (`Rational.Simplify(symbol, e)`) · `algebraic` (`Algebraic.Expand(e)`) · `trigonometric` (`Trigonometric.Simplify(e)`) · `polynomial-collected` (`Polynomial.CollectTerms(symbol, e)`) (4) — the `rational`/`polynomial-collected` rows are symbol-relative (the `Simplify.Symbol` field carries the collection variable), the `algebraic`/`trigonometric` rows are whole-expression and ignore it, so the `SymbolicForm.Normalize(symbol, e)` delegate is one shape over both.
- Entry: `public static Fin<SymbolicExpr> Apply(SymbolicExpr source, SymbolicOp op)` is the one fold over the eight transform cases — `Differentiate` routes `Calculus.Differentiate(Expression.Symbol(name), tree)` and re-wraps, `Taylor` routes `Calculus.Taylor(order, Expression.Symbol(name), atExpr, tree)` (the expansion point is an `Expression.Real`, the verified `Taylor(int k, Expression symbol, Expression value, Expression expression)` arity whose `symbol` parameter is an `Expression`), `Simplify` reads the `SymbolicForm` row's symbol-relative module delegate, `Substitute` routes `Structure.Substitute`, `Gcd` routes the verified three-argument `Polynomial.Gcd(symbol, u, v)`, `PartialFraction` routes the verified three-argument `Polynomial.PartialFraction(symbol, numerator, denominatorFactors)` over the caller-supplied denominator factor list and `Recombine` folds its `(quotient, remainders)` split back through `Operators.add`, and `Approximate` routes `Approximate.ApproximateSubstitute` over an `IDictionary<string, Approximation>` — every arm re-wraps the resulting `Expression` through `SymbolicExpr.Of` so the canonical key recomputes; the coefficient harvest is NOT a transform case but the sibling `public static Fin<Seq<double>> Coefficients(SymbolicExpr source, string symbol)` reading `Polynomial.Coefficients` into the ordered coefficient vector the `Solver/optimizer#OPTIMIZER_LANE` seed consumes, because a coefficient extraction returns a `Seq<double>` and a degenerate echo arm inside `Apply` is the collapsed defect.
- Auto: `Apply` is a generated total `Switch` over the `SymbolicOp` `[Union]` in the allocation-free stateful form — the `source` threads as the `state` argument and each `static` case delegate takes `(case, source)`, so a closure is never allocated and an unhandled case is a compile break, never a runtime fall-through; `Differentiate` validates the symbol is in `FreeSymbols` before the `Calculus.Differentiate` call so a differentiate-by-absent-symbol is a `NonDifferentiable` fault rather than a silent zero; `Simplify` reads the `SymbolicForm` row's symbol-relative module delegate (`rational → Rational.Simplify(symbol, ·)`, `polynomial-collected → Polynomial.CollectTerms(symbol, ·)`, `algebraic → Algebraic.Expand`, `trigonometric → Trigonometric.Simplify`) so the normalization route is a row value, never a `form switch` cascade, and the symbol-relative routes read the `Simplify.Symbol` collection variable rather than an undefined placeholder; `Substitute`/`Approximate` fold the binding `Map` into the F# substitution/approximation structure once at the seam — the `Approximate` arm mints an `IDictionary<string, Approximation>` through `Approximate.Real`, the carrier `ApproximateSubstitute` requires, distinct from the `IDictionary<string, FloatingPoint>` the `Evaluate` seam takes; the typed `Coefficients` projection reads `Polynomial.Coefficients(Expression.Symbol(name), tree)` (returning an `Expression[]`) and projects each coefficient `Expression` through `RealNumberValue`, faulting any non-constant coefficient as `SymbolUndefined` so a symbolic-coefficient polynomial surfaces at the projection rather than corrupting the numeric vector.
- Receipt: the operation fold lands no receipt of its own; the differentiate output feeds `lowering` (which caches the compiled Jacobian delegate and stamps the `CompiledExpr` cache receipt) and the optimizer adjoint (which stamps the `Optimization` slot), the coefficient vector feeds the `Solver/optimizer#OPTIMIZER_LANE` numeric seed, and a transform fault rides the `ComputeFault` rail.
- Packages: MathNet.Symbolics (Calculus/Rational/Algebraic/Trigonometric/Polynomial/Structure/Approximate modules), MathNet.Numerics.FSharp (the `Expression→FloatingPoint` eval seam through `Evaluate.Evaluate`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new analytic transform is one `SymbolicOp` case plus one `Apply` arm binding its module function (a `Limit` or `Integrate` arm would bind the corresponding `MathNet.Symbolics` module); a new simplification route is one `SymbolicForm` row with its inline module delegate; a new projection (e.g. `MathML`) is one terminal beside `Infix`/`LaTeX`; zero new surface — a `Differentiator`/`Simplifier`/`Substitutor` sibling family is the collapsed defect folded onto `Apply`.
- Boundary: `Apply` is the one transform surface and a per-operation `Differentiate`/`Simplify`/`Substitute` static-method ladder is the rejected form collapsed into the union fold; `Coefficients` is the one collapsed exception that does NOT ride `Apply` because its result is a numeric vector, never a `SymbolicExpr` — a degenerate `SymbolicOp.Coefficients` case echoing the source through `Apply` is the rejected shoehorn; the twelve-module `MathNet.Symbolics` surface is mined whole — `Calculus` (differentiate/taylor/tangentLine), `Rational`/`Algebraic`/`Trigonometric` (simplify/expand/contract), `Polynomial` (Coefficients/CollectTerms/Gcd/PartialFraction — `CollectTerms`/`PartialFraction`/`Gcd` are symbol-relative, and `PartialFraction(symbol, numerator, denominatorFactors)` takes the pre-factored denominator list and returns a `(quotient, remainders)` split the arm recombines, never a single-argument whole-expression call), `Structure` (Substitute), `Evaluate` (the numeric eval seam), `Approximate` (partial substitution + float approximation), `Compile` (delegate lowering, transcribed at `lowering`), and `Infix`/`LaTeX` (projection) — a local finite-difference gradient or a string `eval` re-implementation is the deleted lower-level form because the package owns the analytic derivative and the numeric evaluation; `Simplify` never chains all four normalization modules unconditionally — the `SymbolicForm` row selects one route because `Rational.Simplify` and `Trigonometric.Simplify` are not commutative and a blind pipeline can loop a trig-rational form; `Coefficients` faults a non-constant coefficient rather than coercing it because the numeric consumers admit only a real vector; the `Calculus`/`Polynomial`/`Rational`/`Structure` module functions all take their `symbol` argument as an `Expression` (an `Identifier`-wrapped variable, not the F# `Symbol` DU), so the seam constructs it through `Expression.Symbol(name)` — the `string→Expression` identifier factory — and the F# `Symbol` type is confined to the `Compile.compileExpression*` `Symbol list` argument the `lowering` page owns, never crossing the algebra fold; `Approximate.ApproximateSubstitute` is the admitted partial-substitution-plus-approximation surface (its map carries `Approximation` minted through `Approximate.Real`, never the `FloatingPoint` carrier of the `Evaluate.Evaluate` seam — conflating the two value DUs is the eliminated carrier defect), and a manual substitute-then-evaluate two-step is the collapsed redundant path.

```csharp contract
// --- [TYPES] -----------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SymbolicForm {
    public static readonly SymbolicForm Rational = new("rational", static (sym, e) => MathNet.Symbolics.Rational.Simplify(Expression.Symbol(sym), e));
    public static readonly SymbolicForm Algebraic = new("algebraic", static (_, e) => MathNet.Symbolics.Algebraic.Expand(e));
    public static readonly SymbolicForm Trigonometric = new("trigonometric", static (_, e) => MathNet.Symbolics.Trigonometric.Simplify(e));
    public static readonly SymbolicForm PolynomialCollected = new("polynomial-collected", static (sym, e) => Polynomial.CollectTerms(Expression.Symbol(sym), e));

    private readonly Func<string, Expression, Expression> normalize;

    public Expression Normalize(string symbol, Expression expression) => normalize(symbol, expression);
}

[Union]
public abstract partial record SymbolicOp {
    private SymbolicOp() { }

    public sealed record Differentiate(string Symbol) : SymbolicOp;
    public sealed record Taylor(string Symbol, int Order, double At) : SymbolicOp;
    public sealed record TangentLine(string Symbol, double At) : SymbolicOp;
    public sealed record Simplify(SymbolicForm Form, string Symbol) : SymbolicOp;
    public sealed record Substitute(Map<string, Expression> Bindings) : SymbolicOp;
    public sealed record Approximate(Map<string, double> Bindings) : SymbolicOp;
    public sealed record PartialFraction(string Symbol, Seq<SymbolicExpr> DenominatorFactors) : SymbolicOp;
    public sealed record Gcd(SymbolicExpr Other, string Symbol) : SymbolicOp;
}

// --- [OPERATIONS] ------------------------------------------------------------------------
public static class SymbolicOps {
    public static Fin<SymbolicExpr> Apply(SymbolicExpr source, SymbolicOp op) =>
        op.Switch(
            source,
            differentiate: static (c, src) => Guard(src, c.Symbol).Map(sym => SymbolicExpr.Of(Calculus.Differentiate(sym, src.Expression))),
            taylor: static (c, src) => Fin.Succ(SymbolicExpr.Of(Calculus.Taylor(c.Order, Expression.Symbol(c.Symbol), Expression.Real(c.At), src.Expression))),
            tangentLine: static (c, src) => Fin.Succ(SymbolicExpr.Of(Calculus.TangentLine(Expression.Symbol(c.Symbol), Expression.Real(c.At), src.Expression))),
            simplify: static (c, src) => Fin.Succ(SymbolicExpr.Of(c.Form.Normalize(c.Symbol, src.Expression))),
            substitute: static (c, src) => Fin.Succ(SymbolicExpr.Of(Fold(src.Expression, c.Bindings))),
            approximate: static (c, src) => Fin.Succ(SymbolicExpr.Of(Approximate.ApproximateSubstitute(ToApproxMap(c.Bindings), src.Expression))),
            partialFraction: static (c, src) => Fin.Succ(SymbolicExpr.Of(Recombine(Polynomial.PartialFraction(Expression.Symbol(c.Symbol), src.Expression, ListModule.OfSeq(c.DenominatorFactors.Map(static f => f.Expression)))))),
            gcd: static (c, src) => Fin.Succ(SymbolicExpr.Of(Polynomial.Gcd(Expression.Symbol(c.Symbol), src.Expression, c.Other.Expression))));

    public static Fin<Seq<double>> Coefficients(SymbolicExpr source, string symbol) =>
        toSeq(Polynomial.Coefficients(Expression.Symbol(symbol), source.Expression))
            .Traverse(term =>
                term is { IsNumber: true } or { IsApproximation: true }
                    ? Fin.Succ(new SymbolicExpression(term).RealNumberValue)
                    : Fin.Fail<double>(new ComputeFault.SymbolUndefined($"<non-constant-coefficient:{Infix.Format(term)}>")));

    static Fin<Expression> Guard(SymbolicExpr source, string symbol) =>
        source.FreeSymbols.Contains(symbol)
            ? Fin.Succ(Expression.Symbol(symbol))
            : Fin.Fail<Expression>(new ComputeFault.NonDifferentiable($"<absent-symbol:{symbol}>"));

    static Expression Recombine(Tuple<Expression, FSharpList<Expression>> split) =>
        toSeq(split.Item2).Fold(split.Item1, static (acc, term) => Operators.add(acc, term));

    static Expression Fold(Expression tree, Map<string, Expression> bindings) =>
        bindings.Fold(tree, static (acc, pair) => Structure.Substitute(Expression.Symbol(pair.Key), pair.Value, acc));

    static IDictionary<string, Approximation> ToApproxMap(Map<string, double> bindings) =>
        bindings.Fold(
            new Dictionary<string, Approximation>(StringComparer.Ordinal),
            static (map, pair) => { map[pair.Key] = Approximate.Real(pair.Value); return map; });
}
```
