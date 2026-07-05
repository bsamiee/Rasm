# [COMPUTE_SOLVER_SATISFY]

Rasm.Compute rule satisfaction: the SMT owner beside the optimizer — CP-SAT OPTIMIZES, Z3 VERIFIES-AND-EXPLAINS, orthogonal concerns on two admitted engines, one page each. A typed rule set lowers to `Microsoft.Z3` assertions NATURALLY from the CAS: each rule is a `Symbolic/expression#SYMBOLIC_EXPR` constraint (an AngouriMath `Entity.Statement` — an (in)equality-sorted `Entity`) walked term-by-term onto `Context.Mk*` terms (`MkRealConst`/`MkAdd`/`MkMul`/`MkPower`/`MkGe`/`MkAnd` — the nonlinear real/integer arithmetic NRA/NIA theories CP-SAT cannot reach), asserted through `Solver.AssertAndTrack` with one tracking literal PER RULE so an UNSATISFIABLE verdict's `UnsatCore` names the EXACT violated rules, never an opaque refusal. The verdict is a typed three-way `SatisfyVerdict` — SATISFIABLE carrying the `Model` witness values, UNSATISFIABLE the unsat-core rule names, UNKNOWN a typed `(Solve, Numeric)` shortfall naming the solver's reason — and surfaces as an `AssessmentResult` a discipline route carries. Ownership law: ONE `Context` per `Runtime/scheduling#JOB_GRAPH` sweep worker, the AST factory and owning arena (`IDisposable`; every `Expr`/`Sort`/`Solver` it mints dies with it), disposed at the `AssessmentResult` boundary — never a shared global context and never a context outliving its verdict. The osx-arm64 `libz3` provisions through the Forge nix lane (the NuGet stable ships win-x64/osx-x64 natives only); a `Context` operation without the provisioned native FAULTS AT INIT, never a silent degrade. A seam `Discipline.Compliance` row exists ONLY IF a rule verdict must persist as its own content-keyed `Node.Assessment` the `Analysis/assessment` Sweep dispatches — a verdict that merely enriches an existing discipline's `AssessmentResult` rides that discipline's route, the seam growth law the mechanism either way (no `Compliance` row is minted this campaign).

## [01]-[INDEX]

- [02]-[RULE_SATISFACTION]: the `ComplianceRule` typed rule carrier, the `SymbolicExpr` → Z3 term lowering, the tracked assertion set, the `SatisfyVerdict` three-way outcome with the unsat-core explanation, and the per-worker `Context` arena law.

## [02]-[RULE_SATISFACTION]

- Owner: `ComplianceRule` the typed rule carrier — a rule name, the `SymbolicExpr` constraint (an `Entity.Statement`), and the citation it enforces — so a rule set is DATA the callers author, never code; `RuleLowering` the `Entity`-to-Z3 term walk — the SAME node-record patterns the `Symbolic/dimensional#DIMENSION_PROOF` fold descends (`Sumf`/`Mulf`/`Divf`/`Powf`/`Minusf` positional records, `Entity.Variable.Name`, the numeric leaves), each arm minting through the `Context.Mk*` factories — one walk, never a per-rule hand lowering; `SatisfyVerdict` `[Union]` the three-way outcome (`Satisfiable(Map<string, double> Witness)` · `Unsatisfiable(Seq<string> ViolatedRules)` · `Unknown(string Reason)`); `RuleSatisfaction` the fold — assert-tracked rules, one `Check`, the typed projection.
- Cases: `SatisfyVerdict.Satisfiable` carries the `Model` witness (each free variable's evaluated value — the concrete design point PROVING the rules consistent); `Unsatisfiable` carries the `UnsatCore` tracking literals mapped back to rule names — the EXACT violated subset, the explanation a compliance report needs; `Unknown` is the honest incompleteness arm (a nonlinear system past the decidable fragment, a timeout) surfaced as the typed `(Solve, Numeric)` shortfall, never coerced onto either definite verdict.
- Entry: `public static Fin<SatisfyVerdict> Check(Seq<ComplianceRule> rules, Map<string, (double Lower, double Upper)> bounds, SatisfyPolicy policy, ClockPolicy clocks)` — mints ONE `Context` (the fault-at-init boundary: an absent provisioned `libz3` throws at construction and converts to the typed `ComputeFault` there), lowers each rule through `RuleLowering.Lower(context, rule.Constraint)` (a rule whose expression fails the walk — an unmapped node, a non-statement — is a typed admission fault naming the rule), asserts each through `Solver.AssertAndTrack(term, context.MkBoolConst(rule.Name))` plus the variable-bound assertions, runs `Solver.Check()` under the policy timeout, and projects `Status.SATISFIABLE`/`UNSATISFIABLE`/`UNKNOWN` onto the verdict — the `Model.Evaluate(var, completion: true)` witness reads on SAT, the `Solver.UnsatCore` literal names on UNSAT; the whole fold brackets the `Context` so every minted AST dies at the verdict boundary.
- Receipt: the verdict surfaces as an `AssessmentResult` fact stream on the carrying discipline's route — `rule:<name>` `Flag` facts (violated rules false), the witness values as dimensionless `Measure` facts on SAT, the governing ratio `1.0` on UNSAT (exceeded) and `0.0` on SAT — riding the one `ComputeReceipt.Assessment` case; no satisfy-local receipt.
- Packages: Microsoft.Z3 (the `Context` AST factory/arena, `Solver.AssertAndTrack`/`Check`/`Model`/`UnsatCore`, `Status`, the `MkRealConst`/`MkAdd`/`MkMul`/`MkPower`/`MkGe`/`MkGt`/`MkLe`/`MkLt`/`MkEq`/`MkAnd`/`MkNot` term factories — MIT; the arm64 native Forge-provisioned, fault-at-init), AngouriMath (the `Entity.Statement` rule source — the CAS is the lowering source, never a second expression algebra), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new rule is one `ComplianceRule` DATA row the caller authors; a new lowered node family is one `RuleLowering` arm (the walk breaks typed on an unmapped node, never silently); a new verdict projection is one field on the verdict case; an OPTIMIZING rule query (maximize slack under the rules) is the `Optimize` growth row on this same owner — recorded, not built, and never a second CP-SAT (exact optimization stays `Solver/optimizer`'s); zero new surface.
- Boundary: Z3 VERIFIES-AND-EXPLAINS and CP-SAT OPTIMIZES — a rule-consistency question with an unsat-core explanation lands here, a design-space search lands on `Solver/optimizer`'s cp-sat/milp rows, and cross-wiring either onto the other engine is the rejected form; the lowering source is the CAS — a rule authored as a `SymbolicExpr` statement lowers through the ONE `Entity` walk, so the dimensional gate (`Symbolic/dimensional#DIMENSION_PROOF`) can prove a rule's unit-consistency BEFORE it asserts and a stringly rule DSL beside the CAS is the deleted form; ONE `Context` per sweep worker, minted at `Check` and disposed at the verdict boundary — every `Expr`/`Sort`/`Solver` is arena-owned, a cached global context is the rejected form (Z3 contexts are not thread-safe across workers), and the tracking literals are per-rule `MkBoolConst` names so `UnsatCore` maps 1:1 back to `ComplianceRule.Name`; `UNKNOWN` is honest — the NRA/NIA fragment is undecidable in general, so the policy timeout and the solver's `ReasonUnknown` surface as the typed shortfall, never a coerced SAT/UNSAT; the native is Forge-provisioned (nixpkgs `z3` carries the arm64 derivation) and a `Context` ctor without it faults at init through the one lifted boundary — never a managed fallback SMT.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// A compliance rule is DATA: the name the unsat-core reports, the CAS statement it asserts, the citation
// it enforces. A rule set is authored by the caller (a code-compliance table, a design-guideline pack).
public sealed record ComplianceRule(string Name, SymbolicExpr Constraint, string Citation);

public sealed record SatisfyPolicy(Duration Timeout, bool WitnessCompletion = true) {
    public static readonly SatisfyPolicy Canonical = new(Duration.FromSeconds(30));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SatisfyVerdict {
    private SatisfyVerdict() { }

    public sealed record Satisfiable(Map<string, double> Witness) : SatisfyVerdict;
    public sealed record Unsatisfiable(Seq<string> ViolatedRules) : SatisfyVerdict;
    public sealed record Unknown(string Reason) : SatisfyVerdict;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class RuleSatisfaction {
    // ONE Context per check — the AST factory and owning arena, bracketed so every minted Expr/Sort/Solver
    // dies at the verdict boundary; an absent provisioned libz3 throws at construction and converts HERE
    // (fault-at-init, never a silent managed degrade).
    public static Fin<SatisfyVerdict> Check(Seq<ComplianceRule> rules, Map<string, (double Lower, double Upper)> bounds, SatisfyPolicy policy, ClockPolicy clocks) =>
        Try.lift(() => {
            using var context = new Microsoft.Z3.Context();
            using Microsoft.Z3.Solver solver = context.MkSolver();
            solver.Set("timeout", (uint)policy.Timeout.TotalMilliseconds);
            var variables = Map<string, Microsoft.Z3.RealExpr>();
            foreach (var (name, (lower, upper)) in bounds) {
                Microsoft.Z3.RealExpr variable = context.MkRealConst(name);
                variables = variables.Add(name, variable);
                solver.Assert(context.MkAnd(
                    context.MkGe(variable, context.MkReal(lower.ToString(System.Globalization.CultureInfo.InvariantCulture))),
                    context.MkLe(variable, context.MkReal(upper.ToString(System.Globalization.CultureInfo.InvariantCulture)))));
            }
            foreach (ComplianceRule rule in rules) {
                Microsoft.Z3.BoolExpr term = RuleLowering.Lower(context, variables, rule);
                solver.AssertAndTrack(term, context.MkBoolConst(rule.Name));
            }
            return solver.Check() switch {
                Microsoft.Z3.Status.SATISFIABLE => (SatisfyVerdict)new SatisfyVerdict.Satisfiable(
                    variables.Map((name, variable) => (name, Value: solver.Model.Evaluate(variable, completion: policy.WitnessCompletion)))
                        .Values.ToSeq()
                        .Fold(Map<string, double>(), (acc, pair) => acc.Add(pair.name, RuleLowering.Numeric(pair.Value)))),
                Microsoft.Z3.Status.UNSATISFIABLE => new SatisfyVerdict.Unsatisfiable(
                    toSeq(solver.UnsatCore).Map(static literal => literal.ToString())),
                _ => new SatisfyVerdict.Unknown(solver.ReasonUnknown),
            };
        }).Run().MapFail(static error => (Error)new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Numeric, $"<z3:{error.GetType().Name}:{error.Message}>"));

    // The verdict as the uniform fact stream a carrying discipline's route writes back: per-rule Flag facts,
    // the SAT witness as dimensionless Measures, the governing ratio 1.0 on UNSAT / 0.0 on SAT / NaN on UNKNOWN.
    public static Seq<AssessmentFact> Facts(Seq<ComplianceRule> rules, SatisfyVerdict verdict) =>
        verdict switch {
            SatisfyVerdict.Satisfiable sat =>
                rules.Map(static rule => AssessmentFact.Flag($"rule:{rule.Name}", true))
                    + sat.Witness.Map((name, value) => (name, value)).Values.ToSeq().Map(static pair => AssessmentFact.Ratio($"witness:{pair.name}", pair.value)),
            SatisfyVerdict.Unsatisfiable unsat =>
                rules.Map(rule => AssessmentFact.Flag($"rule:{rule.Name}", !unsat.ViolatedRules.Contains(rule.Name)))
                    + Seq(AssessmentFact.Text("unsat-core", string.Join(",", unsat.ViolatedRules))),
            SatisfyVerdict.Unknown unknown => Seq(AssessmentFact.Text("satisfy-unknown", unknown.Reason)),
            _ => Seq<AssessmentFact>(),
        };
}

// The Entity -> Z3 term walk: the SAME positional node-record patterns the dimensional fold descends,
// each arm minting through the Context factories — the CAS is the lowering source, one walk for every rule.
public static class RuleLowering {
    public static Microsoft.Z3.BoolExpr Lower(Microsoft.Z3.Context context, Map<string, Microsoft.Z3.RealExpr> variables, ComplianceRule rule) =>
        rule.Constraint.Entity switch {
            Entity.Equalsf(var left, var right) => context.MkEq(Arith(context, variables, left), Arith(context, variables, right)),
            Entity.Greaterf(var left, var right) => context.MkGt(Arith(context, variables, left), Arith(context, variables, right)),
            Entity.GreaterOrEqualf(var left, var right) => context.MkGe(Arith(context, variables, left), Arith(context, variables, right)),
            Entity.Lessf(var left, var right) => context.MkLt(Arith(context, variables, left), Arith(context, variables, right)),
            Entity.LessOrEqualf(var left, var right) => context.MkLe(Arith(context, variables, left), Arith(context, variables, right)),
            Entity.Andf(var a, var b) => context.MkAnd(Lower(context, variables, rule with { Constraint = SymbolicExpr.Of(a) }), Lower(context, variables, rule with { Constraint = SymbolicExpr.Of(b) })),
            Entity.Notf(var inner) => context.MkNot(Lower(context, variables, rule with { Constraint = SymbolicExpr.Of(inner) })),
            var node => throw new InvalidOperationException($"<rule-non-statement:{rule.Name}:{node.GetType().Name}>"),
        };

    // The arithmetic sub-walk over the Entity node records — Sumf/Minusf/Mulf/Divf/Powf and the leaves; the
    // NRA MkPower reach is exactly what CP-SAT's linear model cannot express.
    static Microsoft.Z3.ArithExpr Arith(Microsoft.Z3.Context context, Map<string, Microsoft.Z3.RealExpr> variables, Entity node) =>
        node switch {
            Entity.Number.Rational rational => context.MkReal(rational.Stringize()),
            Entity.Number.Real real => context.MkReal(real.Stringize()),
            Entity.Variable variable => variables.Find(variable.Name).IfNone(() => context.MkRealConst(variable.Name)),
            Entity.Sumf(var a, var b) => (Microsoft.Z3.ArithExpr)context.MkAdd(Arith(context, variables, a), Arith(context, variables, b)),
            Entity.Minusf(var a, var b) => (Microsoft.Z3.ArithExpr)context.MkSub(Arith(context, variables, a), Arith(context, variables, b)),
            Entity.Mulf(var a, var b) => (Microsoft.Z3.ArithExpr)context.MkMul(Arith(context, variables, a), Arith(context, variables, b)),
            Entity.Divf(var a, var b) => context.MkDiv(Arith(context, variables, a), Arith(context, variables, b)),
            Entity.Powf(var a, var b) => (Microsoft.Z3.ArithExpr)context.MkPower(Arith(context, variables, a), Arith(context, variables, b)),
            _ => throw new InvalidOperationException($"<rule-unmapped-node:{node.GetType().Name}>"),
        };

    public static double Numeric(Microsoft.Z3.Expr value) =>
        value is Microsoft.Z3.RatNum rational
            ? (double)rational.Numerator.BigInteger / (double)rational.Denominator.BigInteger
            : double.NaN;
}
```

## [03]-[RESEARCH]

- [OPTIMIZE_GROWTH]: the Z3 `Optimize` surface (maximize rule slack, soft assertions with weights) is the recorded growth row on this owner — an optimizing compliance query stays HERE when it lands (the engine and arena law unchanged), never a second exact-optimization rail beside `Solver/optimizer`'s CP-SAT/MILP rows.
- [COMPLIANCE_ROW_GATE]: a seam `Discipline.Compliance` row mints ONLY when a rule verdict must persist as its own content-keyed `Node.Assessment` the `Analysis/assessment` Sweep dispatches; today a verdict enriches the carrying discipline's `AssessmentResult` through `RuleSatisfaction.Facts`, so no row exists and the seam growth law is the mechanism when one is needed.
