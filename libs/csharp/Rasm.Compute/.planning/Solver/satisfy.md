# [COMPUTE_SOLVER_SATISFY]

Rasm.Compute rule satisfaction: the SMT owner beside the optimizer — Z3 VERIFIES-AND-EXPLAINS where CP-SAT OPTIMIZES, orthogonal concerns on two admitted engines, one page each. A typed `ComplianceRule` set lowers to `Microsoft.Z3` assertions from the CAS — each rule an AngouriMath `Entity.Statement` walked term-by-term onto `Context.Mk*` terms (the nonlinear NRA/NIA arithmetic CP-SAT cannot reach), asserted through `Solver.AssertAndTrack` under one tracking literal PER RULE so an UNSATISFIABLE `UnsatCore` names the EXACT violated rules, never an opaque refusal. Verdict is the three-way `SatisfyVerdict` — SATISFIABLE carries the `Model` witness, UNSATISFIABLE the unsat-core rule names, UNKNOWN a typed `(Solve, Numeric)` shortfall — surfacing as an `AssessmentResult` a discipline route carries.

Ownership is ONE `Context` per `Runtime/scheduling#JOB_GRAPH` sweep worker — the AST factory and arena (`IDisposable`; every `Expr`/`Sort`/`Solver` it mints dies with it), disposed at the `AssessmentResult` boundary, never a shared global nor a context outliving its verdict. Osx-arm64 `libz3` provisions through the Forge nix lane (NuGet stable ships win-x64/osx-x64 natives only); a `Context` operation without the native FAULTS AT INIT, never a silent degrade. A `Discipline.Compliance` seam row mints ONLY when a verdict must persist as its own content-keyed `Node.Assessment` the `Analysis/assessment` Sweep dispatches — a verdict enriching an existing discipline's `AssessmentResult` rides that route, no `Compliance` row minted this campaign.

## [01]-[INDEX]

- [02]-[RULE_SATISFACTION]: typed `ComplianceRule` set lowered CAS→Z3, the tracked-assertion `SatisfyVerdict` three-way with unsat-core explanation, per-worker `Context` arena.

## [02]-[RULE_SATISFACTION]

- Owner: `ComplianceRule` carries one named `Entity.Statement`, citation, element-grounding rows, and a hypothesis discriminant; `RuleLowering` walks the same positional nodes as `Symbolic/dimensional#DIMENSION_PROOF`, including Boolean equivalence through `Context.MkIff`; `SatisfyVerdict` `[Union]` carries the three outcomes; `RuleSatisfaction` asserts base rules, opens one `Solver.Push` frame for hypotheses, checks once, projects the witness/core, then `Pop`s the frame.
- Cases: `Satisfiable` carries every declared free variable as `WitnessValue.Rational` or exact Z3 text; `Unsatisfiable` carries tracked `name`/`name@element` literals; `Unknown` carries `(SolvePhase, FailureKind, Reason)` without coercion.
- Entry: `Check` validates names, unique tracking identities, finite ordered bounds, grounding coverage, free-variable coverage, and timeout conversion, then consumes the `Pregate` interval decision — a `ProvenViolated` rule settles `Unsatisfiable` and an all-`ProvenSatisfied` roster settles `Satisfiable` at the box midpoint before any native allocation; only `Indeterminate` mints the bracketed `Context`. Every asserted CAS variable resolves through a declared bound or grounding binding; the lowering never silently mints an untracked symbol.
- Receipt: the verdict surfaces on the carrying discipline's `AssessmentResult`: `rule:<name>` flags, rational witnesses as ratio facts, non-rational witnesses as exact text, the raw unsat core, or the typed unknown triple (`satisfy-unknown-phase`/`satisfy-unknown-kind`/`satisfy-unknown` — the `SolvePhase`/`FailureKind` evidence stays typed at the assessment boundary, never a bare reason string); no satisfy-local receipt exists.
- Packages: Microsoft.Z3 (the `Context` AST factory/arena and `AssertAndTrack`/`Check`/`Model`/`UnsatCore`/`Mk*` term surface — MIT; arm64 native Forge-provisioned, fault-at-init), AngouriMath (the `Entity.Statement` rule source, the one lowering algebra), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new rule is one `ComplianceRule` DATA row; a new element population under an existing rule is one `RuleGrounding` row (the template quantifies, never a per-element rule copy); a new lowered node family is one `RuleLowering` arm (the walk breaks typed on an unmapped node, never silently); a new verdict projection is one field on the verdict case; an OPTIMIZING rule query (maximize slack, Z3 soft assertions with weights) is the recorded `Optimize` growth row on this SAME owner and engine — never a second exact rail beside `Solver/optimizer`'s CP-SAT/MILP; zero new surface.
- Boundary: Z3 VERIFIES-AND-EXPLAINS, CP-SAT OPTIMIZES — a rule-consistency question with an unsat-core explanation lands here, a design-space search on `Solver/optimizer`'s cp-sat/milp rows, and cross-wiring either engine onto the other is rejected; the lowering source is the CAS, so the `Symbolic/dimensional#DIMENSION_PROOF` gate proves a rule's unit-consistency BEFORE it asserts and a stringly rule DSL beside the CAS is rejected; a cached global `Context` is rejected because Z3 contexts are not thread-safe across workers; `UNKNOWN` stays honest — the NRA/NIA fragment is undecidable in general, so the policy timeout and `ReasonUnknown` surface as the typed shortfall, never a coerced SAT/UNSAT nor a managed fallback SMT when the Forge-provisioned native is absent; the `Symbolic/lowering#ENCLOSURE_AND_COLUMNS` `EnclosureFold.Certify` interval pre-gate answers a rule whose enclosure proves over the declared bounds BEFORE the Z3 context is minted — `ProvenSatisfied`/`ProvenViolated` short-circuit the check, `Indeterminate` falls through to the exact engine, and the gate is a filter over the same admitted `SymbolicExpr`, never a second verdict authority.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
public sealed record ComplianceRule(string Name, SymbolicExpr Constraint, string Citation, Seq<RuleGrounding> Grounding, bool Hypothesis);

public sealed record RuleGrounding(string Element, Map<string, double> Bindings);

public sealed record SatisfyPolicy(Duration Timeout, bool WitnessCompletion = true) {
    public static readonly SatisfyPolicy Canonical = new(Duration.FromSeconds(30));

    public bool Invalid => Timeout <= Duration.Zero || Timeout.TotalMilliseconds > uint.MaxValue;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WitnessValue {
    private WitnessValue() { }

    public sealed record Rational(double Value) : WitnessValue;
    public sealed record Exact(string Value) : WitnessValue;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SatisfyVerdict {
    private SatisfyVerdict() { }

    public sealed record Satisfiable(Map<string, WitnessValue> Witness) : SatisfyVerdict;
    public sealed record Unsatisfiable(Seq<string> ViolatedRules) : SatisfyVerdict;
    public sealed record Unknown(SolvePhase Phase, FailureKind Kind, string Reason) : SatisfyVerdict;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class RuleSatisfaction {
    public static Fin<SatisfyVerdict> Check(Seq<ComplianceRule> rules, Map<string, (double Lower, double Upper)> bounds, SatisfyPolicy policy) =>
        from _ in Admit(rules, bounds, policy)
        from verdict in Pregate(rules, bounds).Match(
            Some: Fin.Succ,
            None: () => CheckExact(rules, bounds, policy))
        select verdict;

    // Interval pre-gate over the SAME admitted rule set: each ungrounded comparison rule adapts to g(x) ≤ 0 and
    // certifies through EnclosureFold.Certify over the declared box BEFORE any native allocation — one
    // ProvenViolated rule settles Unsatisfiable, an all-ProvenSatisfied roster settles Satisfiable at the box
    // midpoint (every box point satisfies, so the midpoint is a genuine witness), and any Indeterminate,
    // grounded, or non-comparison rule sends the whole set to the exact engine. Filter, never verdict authority.
    static Option<SatisfyVerdict> Pregate(Seq<ComplianceRule> rules, Map<string, (double Lower, double Upper)> bounds) {
        Seq<string> order = toSeq(bounds.Keys);
        ImmutableArray<Interval> box = [.. order.Map(name => Interval.Of(bounds[name].Lower, bounds[name].Upper))];
        Seq<(ComplianceRule Rule, IntervalVerdict Verdict)> certified = rules.Map(rule =>
            rule.Grounding.IsEmpty
                ? Gform(rule.Constraint).Match(
                    Some: g => (rule, EnclosureFold.Certify(g, order, box).IfFail(_ => new IntervalVerdict.Indeterminate(Interval.Of(double.MinValue, double.MaxValue)))),
                    None: () => (rule, (IntervalVerdict)new IntervalVerdict.Indeterminate(Interval.Of(double.MinValue, double.MaxValue))))
                : (rule, new IntervalVerdict.Indeterminate(Interval.Of(double.MinValue, double.MaxValue))));
        return certified.Find(static pair => pair.Verdict is IntervalVerdict.ProvenViolated).Match(
            Some: violated => Some((SatisfyVerdict)new SatisfyVerdict.Unsatisfiable(Seq(violated.Rule.Name))),
            None: () => certified.ForAll(static pair => pair.Verdict is IntervalVerdict.ProvenSatisfied)
                ? Some((SatisfyVerdict)new SatisfyVerdict.Satisfiable(
                    order.Fold(Map<string, WitnessValue>(), (acc, name) => acc.Add(name, new WitnessValue.Rational((bounds[name].Lower + bounds[name].Upper) * 0.5)))))
                : Option<SatisfyVerdict>.None);
    }

    // A comparison statement adapts to the g(x) ≤ 0 enclosure form; any other statement shape is exact-rail-only.
    static Option<SymbolicExpr> Gform(SymbolicExpr constraint) => constraint.Entity switch {
        Entity.LessOrEqualf le => Some(SymbolicExpr.Of(le.Left - le.Right)),
        Entity.Lessf lt => Some(SymbolicExpr.Of(lt.Left - lt.Right)),
        Entity.GreaterOrEqualf ge => Some(SymbolicExpr.Of(ge.Right - ge.Left)),
        Entity.Greaterf gt => Some(SymbolicExpr.Of(gt.Right - gt.Left)),
        _ => Option<SymbolicExpr>.None,
    };

    static Fin<SatisfyVerdict> CheckExact(Seq<ComplianceRule> rules, Map<string, (double Lower, double Upper)> bounds, SatisfyPolicy policy) =>
        from verdict in Try.lift(() => {
            using Microsoft.Z3.Context context = new();
            using Microsoft.Z3.Solver solver = context.MkSolver();
            solver.Set("timeout", (uint)policy.Timeout.TotalMilliseconds);
            Map<string, Microsoft.Z3.RealExpr> variables = Map<string, Microsoft.Z3.RealExpr>();
            foreach ((string name, (double lower, double upper)) in bounds) {
                Microsoft.Z3.RealExpr variable = context.MkRealConst(name);
                variables = variables.Add(name, variable);
                solver.Assert(context.MkAnd(
                    context.MkGe(variable, context.MkReal(lower.ToString(System.Globalization.CultureInfo.InvariantCulture))),
                    context.MkLe(variable, context.MkReal(upper.ToString(System.Globalization.CultureInfo.InvariantCulture)))));
            }
            bool framed = false;
            foreach (bool hypothesis in new[] { false, true }) {
                if (hypothesis) {
                    if (!rules.Exists(static rule => rule.Hypothesis)) { continue; }
                    solver.Push();
                    framed = true;
                }
                foreach (ComplianceRule rule in rules.Filter(rule => rule.Hypothesis == hypothesis)) {
                    if (rule.Grounding.IsEmpty) {
                        solver.AssertAndTrack(RuleLowering.Lower(context, variables, rule), context.MkBoolConst(rule.Name));
                        continue;
                    }
                    foreach (RuleGrounding ground in rule.Grounding) {
                        Map<string, Microsoft.Z3.RealExpr> bound = variables;
                        foreach ((string name, double value) in ground.Bindings) {
                            bound = bound.AddOrUpdate(name, (Microsoft.Z3.RealExpr)context.MkReal(value.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                        }
                        solver.AssertAndTrack(RuleLowering.Lower(context, bound, rule), context.MkBoolConst($"{rule.Name}@{ground.Element}"));
                    }
                }
            }
            SatisfyVerdict verdict = solver.Check() switch {
                Microsoft.Z3.Status.SATISFIABLE => (SatisfyVerdict)new SatisfyVerdict.Satisfiable(
                    variables.Map((name, variable) => (name, Value: solver.Model.Evaluate(variable, completion: policy.WitnessCompletion)))
                        .Values.ToSeq()
                        .Fold(Map<string, WitnessValue>(), (acc, pair) => acc.Add(pair.name, RuleLowering.Witness(pair.Value)))),
                Microsoft.Z3.Status.UNSATISFIABLE => new SatisfyVerdict.Unsatisfiable(
                    toSeq(solver.UnsatCore).Map(static literal => literal.ToString())),
                _ => new SatisfyVerdict.Unknown(SolvePhase.Solve, FailureKind.Numeric, solver.ReasonUnknown),
            };
            if (framed) { solver.Pop(); }
            return verdict;
        }).Run().MapFail(static error => (Error)new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Numeric, $"<z3:{error.GetType().Name}:{error.Message}>"))
        select verdict;

    static Fin<Unit> Admit(Seq<ComplianceRule> rules, Map<string, (double Lower, double Upper)> bounds, SatisfyPolicy policy) {
        if (policy.Invalid || rules.IsEmpty || rules.Map(static rule => rule.Name).ToHashSet(StringComparer.Ordinal).Count != rules.Count)
            return Fin.Fail<Unit>(ComputeFault.Create("<satisfy-invalid-policy-or-rule-set>"));
        foreach ((string name, (double lower, double upper)) in bounds) {
            if (!Name(name) || !double.IsFinite(lower) || !double.IsFinite(upper) || lower > upper)
                return Fin.Fail<Unit>(ComputeFault.Create($"<satisfy-invalid-bound:{name}>"));
        }
        foreach (ComplianceRule rule in rules) {
            if (!Name(rule.Name) || string.IsNullOrWhiteSpace(rule.Citation))
                return Fin.Fail<Unit>(ComputeFault.Create($"<satisfy-invalid-rule:{rule.Name}>"));
            HashSet<string> free = rule.Constraint.Entity.Vars.Select(static variable => variable.Name).ToHashSet(StringComparer.Ordinal);
            if (rule.Grounding.IsEmpty && free.Any(name => !bounds.ContainsKey(name)))
                return Fin.Fail<Unit>(ComputeFault.Create($"<satisfy-unbound-variable:{rule.Name}>"));
            if (rule.Grounding.Map(static ground => ground.Element).ToHashSet(StringComparer.Ordinal).Count != rule.Grounding.Count)
                return Fin.Fail<Unit>(ComputeFault.Create($"<satisfy-duplicate-grounding:{rule.Name}>"));
            foreach (RuleGrounding ground in rule.Grounding) {
                if (!Name(ground.Element) || free.Any(name => !bounds.ContainsKey(name) && !ground.Bindings.ContainsKey(name)))
                    return Fin.Fail<Unit>(ComputeFault.Create($"<satisfy-invalid-grounding:{rule.Name}:{ground.Element}>"));
                foreach ((string name, double value) in ground.Bindings) {
                    if (!Name(name) || !double.IsFinite(value))
                        return Fin.Fail<Unit>(ComputeFault.Create($"<satisfy-invalid-binding:{rule.Name}:{ground.Element}:{name}>"));
                }
            }
        }
        return Fin.Succ(unit);
    }

    static bool Name(string value) =>
        !string.IsNullOrWhiteSpace(value) && value[0] != '@' && value.All(static character => char.IsLetterOrDigit(character) || character is '_' or '-' or '.');

    public static Fin<Seq<AssessmentFact>> Facts(Seq<ComplianceRule> rules, SatisfyVerdict verdict) =>
        verdict.Switch(
            satisfiable: sat =>
                sat.Witness.Map((name, value) => (name, value)).Values.ToSeq()
                    .TraverseM(static pair => WitnessFact(pair.name, pair.value)).As()
                    .Map(witness => rules.Map(static rule => AssessmentFact.Flag($"rule:{rule.Name}", true)) + witness),
            unsatisfiable: unsat => FinSucc(
                rules.Map(rule => AssessmentFact.Flag($"rule:{rule.Name}", !unsat.ViolatedRules.Exists(literal => literal == rule.Name || literal.StartsWith($"{rule.Name}@", StringComparison.Ordinal))))
                    + Seq(AssessmentFact.Text("unsat-core", string.Join(",", unsat.ViolatedRules)))),
            unknown: static unknown => FinSucc(Seq(
                AssessmentFact.Text("satisfy-unknown-phase", unknown.Phase.Key),
                AssessmentFact.Text("satisfy-unknown-kind", unknown.Kind.Key),
                AssessmentFact.Text("satisfy-unknown", unknown.Reason))));

    static Fin<AssessmentFact> WitnessFact(string name, WitnessValue value) =>
        value.Switch(
            rational: rational => AssessmentFact.Ratio($"witness:{name}", rational.Value),
            exact: exact => FinSucc(AssessmentFact.Text($"witness:{name}", exact.Value)));
}

public static class RuleLowering {
    public static Microsoft.Z3.BoolExpr Lower(Microsoft.Z3.Context context, Map<string, Microsoft.Z3.RealExpr> variables, ComplianceRule rule) =>
        rule.Constraint.Entity switch {
            Entity.Equalsf(Entity.Statement left, Entity.Statement right) => context.MkIff(
                Lower(context, variables, rule with { Constraint = SymbolicExpr.Of(left) }),
                Lower(context, variables, rule with { Constraint = SymbolicExpr.Of(right) })),
            Entity.Equalsf(Entity left, Entity right) => context.MkEq(Arith(context, variables, left), Arith(context, variables, right)),
            Entity.Greaterf(Entity left, Entity right) => context.MkGt(Arith(context, variables, left), Arith(context, variables, right)),
            Entity.GreaterOrEqualf(Entity left, Entity right) => context.MkGe(Arith(context, variables, left), Arith(context, variables, right)),
            Entity.Lessf(Entity left, Entity right) => context.MkLt(Arith(context, variables, left), Arith(context, variables, right)),
            Entity.LessOrEqualf(Entity left, Entity right) => context.MkLe(Arith(context, variables, left), Arith(context, variables, right)),
            Entity.Andf(Entity left, Entity right) => context.MkAnd(Lower(context, variables, rule with { Constraint = SymbolicExpr.Of(left) }), Lower(context, variables, rule with { Constraint = SymbolicExpr.Of(right) })),
            Entity.Orf(Entity left, Entity right) => context.MkOr(Lower(context, variables, rule with { Constraint = SymbolicExpr.Of(left) }), Lower(context, variables, rule with { Constraint = SymbolicExpr.Of(right) })),
            Entity.Xorf(Entity left, Entity right) => context.MkXor(Lower(context, variables, rule with { Constraint = SymbolicExpr.Of(left) }), Lower(context, variables, rule with { Constraint = SymbolicExpr.Of(right) })),
            Entity.Impliesf(Entity assumption, Entity conclusion) => context.MkImplies(Lower(context, variables, rule with { Constraint = SymbolicExpr.Of(assumption) }), Lower(context, variables, rule with { Constraint = SymbolicExpr.Of(conclusion) })),
            Entity.Notf(Entity inner) => context.MkNot(Lower(context, variables, rule with { Constraint = SymbolicExpr.Of(inner) })),
            Entity node => throw new InvalidOperationException($"<rule-non-statement:{rule.Name}:{node.GetType().Name}>"),
        };

    static Microsoft.Z3.ArithExpr Arith(Microsoft.Z3.Context context, Map<string, Microsoft.Z3.RealExpr> variables, Entity node) =>
        node switch {
            Entity.Number.Rational rational => context.MkReal(rational.Stringize()),
            Entity.Number.Real real => context.MkReal(real.Stringize()),
            Entity.Variable variable => variables.Find(variable.Name).IfNone(() => throw new InvalidOperationException($"<rule-unbound-variable:{variable.Name}>")),
            Entity.Sumf(Entity left, Entity right) => (Microsoft.Z3.ArithExpr)context.MkAdd(Arith(context, variables, left), Arith(context, variables, right)),
            Entity.Minusf(Entity left, Entity right) => (Microsoft.Z3.ArithExpr)context.MkSub(Arith(context, variables, left), Arith(context, variables, right)),
            Entity.Mulf(Entity left, Entity right) => (Microsoft.Z3.ArithExpr)context.MkMul(Arith(context, variables, left), Arith(context, variables, right)),
            Entity.Divf(Entity left, Entity right) => context.MkDiv(Arith(context, variables, left), Arith(context, variables, right)),
            Entity.Powf(Entity left, Entity right) => (Microsoft.Z3.ArithExpr)context.MkPower(Arith(context, variables, left), Arith(context, variables, right)),
            Entity.Absf(Entity argument) => Rectified(context, Arith(context, variables, argument)),
            Entity.Signumf(Entity argument) => Sign(context, Arith(context, variables, argument)),
            _ => throw new InvalidOperationException($"<rule-unmapped-node:{node.GetType().Name}>"),
        };

    static Microsoft.Z3.ArithExpr Rectified(Microsoft.Z3.Context context, Microsoft.Z3.ArithExpr value) =>
        (Microsoft.Z3.ArithExpr)context.MkITE(context.MkGe(value, context.MkReal(0)), value, context.MkUnaryMinus(value));

    static Microsoft.Z3.ArithExpr Sign(Microsoft.Z3.Context context, Microsoft.Z3.ArithExpr value) =>
        (Microsoft.Z3.ArithExpr)context.MkITE(context.MkGt(value, context.MkReal(0)), context.MkReal(1),
            context.MkITE(context.MkLt(value, context.MkReal(0)), context.MkReal(-1), context.MkReal(0)));

    public static WitnessValue Witness(Microsoft.Z3.Expr value) {
        if (value is not Microsoft.Z3.RatNum rational) { return new WitnessValue.Exact(value.ToString()); }
        double projected = (double)rational.Numerator.BigInteger / (double)rational.Denominator.BigInteger;
        return double.IsFinite(projected) ? new WitnessValue.Rational(projected) : new WitnessValue.Exact(value.ToString());
    }
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
