# [COMPUTE_SOLVER_SATISFY]

Rasm.Compute rule satisfaction: the SMT owner beside the optimizer — Z3 VERIFIES-AND-EXPLAINS where CP-SAT OPTIMIZES, orthogonal concerns on two admitted engines, one page each. A typed `ComplianceRule` set lowers to `Microsoft.Z3` assertions from the CAS — each rule an AngouriMath `Entity.Statement` walked term-by-term onto `Context.Mk*` terms (the nonlinear NRA/NIA arithmetic CP-SAT cannot reach), asserted through `Solver.AssertAndTrack` under one tracking literal PER RULE so an UNSATISFIABLE `UnsatCore` names the EXACT violated rules, never an opaque refusal. Verdict is the three-way `SatisfyVerdict` — SATISFIABLE carries the `Model` witness, UNSATISFIABLE the unsat-core rule names, UNKNOWN a typed `(Solve, Numeric)` shortfall — surfacing as an `AssessmentResult` a discipline route carries.

Ownership is ONE `Context` per `Runtime/scheduling#JOB_GRAPH` sweep worker — the AST factory and arena (`IDisposable`; every `Expr`/`Sort`/`Solver` it mints dies with it), disposed at the `AssessmentResult` boundary, never a shared global nor a context outliving its verdict. Osx-arm64 `libz3` provisions through the Forge nix lane (NuGet stable ships win-x64/osx-x64 natives only); a `Context` operation without the native FAULTS AT INIT, never a silent degrade. A `Discipline.Compliance` seam row mints ONLY when a verdict must persist as its own content-keyed `Node.Assessment` the `Analysis/assessment` Sweep dispatches — a verdict enriching an existing discipline's `AssessmentResult` rides that route, no `Compliance` row minted this campaign.

## [01]-[INDEX]

- [02]-[RULE_SATISFACTION]: typed `ComplianceRule` set lowered CAS→Z3, the tracked-assertion `SatisfyVerdict` three-way with unsat-core explanation, per-worker `Context` arena.

## [02]-[RULE_SATISFACTION]

- Owner: `ComplianceRule` the typed rule carrier (name, `SymbolicExpr` constraint as `Entity.Statement`, enforced citation) — a rule set is DATA the caller authors, never code; `RuleLowering` the `Entity`→Z3 walk over the SAME positional node-records the `Symbolic/dimensional#DIMENSION_PROOF` fold descends, each arm minting through `Context.Mk*`, one walk never a per-rule hand lowering; `SatisfyVerdict` `[Union]` the three-way outcome; `RuleSatisfaction` the assert-track/`Check`/project fold.
- Cases: `Satisfiable` carries the `Model` witness (each free variable's evaluated value, the concrete point PROVING the rules consistent); `Unsatisfiable` carries the `UnsatCore` literals mapped back to rule names, the EXACT violated subset a compliance report needs; `Unknown` is the honest incompleteness arm (nonlinear past the decidable fragment, a timeout) as the typed `(Solve, Numeric)` shortfall, never coerced onto a definite verdict.
- Entry: `Check` mints ONE bracketed `Context` — an absent `libz3` throws at construction and converts to the typed `ComputeFault` HERE (fault-at-init); a rule whose expression fails the `RuleLowering` walk (unmapped node, non-statement) faults typed naming the rule; each rule asserts under its `MkBoolConst` tracking literal plus the bound assertions, and the bracket disposes every minted AST at the verdict boundary.
- Receipt: the verdict surfaces as an `AssessmentResult` fact stream on the carrying discipline's route — `rule:<name>` `Flag` facts (violated false), the SAT witness as dimensionless `Measure` facts, the governing ratio `1.0` on UNSAT / `0.0` on SAT — riding the one `ComputeReceipt.Assessment` case; no satisfy-local receipt.
- Packages: Microsoft.Z3 (the `Context` AST factory/arena and `AssertAndTrack`/`Check`/`Model`/`UnsatCore`/`Mk*` term surface — MIT; arm64 native Forge-provisioned, fault-at-init), AngouriMath (the `Entity.Statement` rule source, the one lowering algebra), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new rule is one `ComplianceRule` DATA row; a new lowered node family is one `RuleLowering` arm (the walk breaks typed on an unmapped node, never silently); a new verdict projection is one field on the verdict case; an OPTIMIZING rule query (maximize slack, Z3 soft assertions with weights) is the recorded `Optimize` growth row on this SAME owner and engine — never a second exact rail beside `Solver/optimizer`'s CP-SAT/MILP; zero new surface.
- Boundary: Z3 VERIFIES-AND-EXPLAINS, CP-SAT OPTIMIZES — a rule-consistency question with an unsat-core explanation lands here, a design-space search on `Solver/optimizer`'s cp-sat/milp rows, and cross-wiring either engine onto the other is rejected; the lowering source is the CAS, so the `Symbolic/dimensional#DIMENSION_PROOF` gate proves a rule's unit-consistency BEFORE it asserts and a stringly rule DSL beside the CAS is rejected; a cached global `Context` is rejected because Z3 contexts are not thread-safe across workers; `UNKNOWN` stays honest — the NRA/NIA fragment is undecidable in general, so the policy timeout and `ReasonUnknown` surface as the typed shortfall, never a coerced SAT/UNSAT nor a managed fallback SMT when the Forge-provisioned native is absent.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// A rule is DATA: the unsat-core name, the CAS statement it asserts, the citation it enforces — caller-authored.
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
    // ONE bracketed Context per check — the AST arena; every minted Expr/Sort/Solver dies at the verdict boundary,
    // and an absent libz3 throws at construction, converting HERE (fault-at-init, never a silent managed degrade).
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

    // Verdict as the uniform fact stream the carrying discipline writes back: per-rule Flag facts, SAT witness
    // as dimensionless Measures, governing ratio 1.0 on UNSAT / 0.0 on SAT / NaN on UNKNOWN.
    public static Fin<Seq<AssessmentFact>> Facts(Seq<ComplianceRule> rules, SatisfyVerdict verdict) =>
        verdict switch {
            SatisfyVerdict.Satisfiable sat =>
                sat.Witness.Map((name, value) => (name, value)).Values.ToSeq()
                    .TraverseM(static pair => AssessmentFact.Ratio($"witness:{pair.name}", pair.value)).As()
                    .Map(witness => rules.Map(static rule => AssessmentFact.Flag($"rule:{rule.Name}", true)) + witness),
            SatisfyVerdict.Unsatisfiable unsat => FinSucc(
                rules.Map(rule => AssessmentFact.Flag($"rule:{rule.Name}", !unsat.ViolatedRules.Contains(rule.Name)))
                    + Seq(AssessmentFact.Text("unsat-core", string.Join(",", unsat.ViolatedRules)))),
            SatisfyVerdict.Unknown unknown => FinSucc(Seq(AssessmentFact.Text("satisfy-unknown", unknown.Reason))),
            _ => FinSucc(Seq<AssessmentFact>()),
        };
}

// Entity -> Z3 walk: the SAME positional node-records the dimensional fold descends, each arm minting through
// the Context factories — the CAS is the one lowering source, one walk for every rule.
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

    // Arithmetic sub-walk over the Entity node records — Sumf/Minusf/Mulf/Divf/Powf and the leaves; the NRA
    // MkPower reach is exactly what CP-SAT's linear model cannot express.
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

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
