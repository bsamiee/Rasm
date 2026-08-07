# [COMPUTE_SOLVER_SATISFY]

Rasm.Compute rule satisfaction: the SMT owner beside the optimizer — Z3 VERIFIES-AND-EXPLAINS where CP-SAT OPTIMIZES, orthogonal concerns on two admitted engines, one page each. Every typed `ComplianceRule` set lowers to `Microsoft.Z3` assertions from the CAS — each rule an AngouriMath `Entity.Statement` walked term-by-term onto `Context.Mk*` terms (the nonlinear NRA/NIA arithmetic CP-SAT cannot reach), asserted through `Solver.AssertAndTrack` under one tracking literal PER RULE so an UNSATISFIABLE `UnsatCore` names the EXACT violated rules, never an opaque refusal. Verdict is the three-way `SatisfyVerdict` — SATISFIABLE carries the `Model` witness beside the `Consequences` implied set every passing design must satisfy, UNSATISFIABLE the unsat-core rule names, UNKNOWN a typed `(Solve, Numeric)` shortfall beside the solver's own decision, conflict, and restart counts — surfacing as an `AssessmentResult` a discipline route carries.

Ownership is ONE `Context` per CHECK — the AST factory and arena (`IDisposable`; every `Expr`/`Sort`/`Solver` it mints dies with it) minted inside `CheckExact` and disposed at the verdict boundary, so a `Runtime/scheduling#JOB_GRAPH` sweep worker never shares an arena and no context outlives the verdict that produced it. Osx-arm64 `libz3` provisions through the Forge nix lane (NuGet stable ships win-x64/osx-x64 natives only); a `Context` operation without the native FAULTS AT INIT, never a silent degrade. One `Discipline.Compliance` seam row mints ONLY when a verdict must persist as its own content-keyed `Node.Assessment` the `Analysis/assessment` Sweep dispatches — a verdict enriching an existing discipline's `AssessmentResult` rides that route, no `Compliance` row minted this campaign.

## [01]-[INDEX]

- [02]-[RULE_SATISFACTION]: typed `ComplianceRule` set lowered CAS→Z3, the tracked-assertion `SatisfyVerdict` three-way with unsat-core explanation, per-check `Context` arena.
- [03]-[RULE_POPULATION_DERIVATION]: typed node-class selector over the seam graph deriving one `RuleGrounding` per matching member from one rule template.
- [04]-[RULE_COVERAGE_PROOF]: population-versus-grounded coverage fact gating a rule whose quantification is short of its own class.

## [02]-[RULE_SATISFACTION]

- Owner: `ComplianceRule` carries one named `Entity.Statement`, citation, element-grounding rows, and a hypothesis discriminant; `RuleLowering` walks the same positional nodes as `Symbolic/dimensional#DIMENSION_PROOF`, including Boolean equivalence through `Context.MkIff`; `SatisfyVerdict` `[Union]` carries the three outcomes; `RuleSatisfaction` asserts the declared box and the base rules, opens one `Solver.Push` frame for hypotheses, checks once, projects the witness/core, then `Pop`s the frame.
- Cases: `Satisfiable` carries every declared free variable as `WitnessValue.Rational` or exact Z3 text beside the implied-literal set every model satisfies; `Unsatisfiable` carries tracked `name`/`name@element`/`hyp@name`/`bound@name` literals; `Unknown` carries `(SolvePhase, FailureKind, Reason)` without coercion, beside the `SearchCounters` decisions/conflicts/restarts read off the same holding.
- Entry: `Check` validates names, unique tracking identities, finite ordered bounds, grounding coverage, free-variable coverage, and timeout conversion, then consumes the `Pregate` interval decision — EVERY `ProvenViolated` rule settles `Unsatisfiable` together and an all-`ProvenSatisfied` roster settles `Satisfiable` at the box midpoint before any native allocation; only an `Indeterminate` remainder mints the bracketed `Context`. Every asserted CAS variable resolves through a declared bound or grounding binding; the lowering never silently mints an untracked symbol. A supplied `CoverageFact` roster gates the same call: an incomplete fact under `SatisfyPolicy.RequireCoverage` refuses before the check, and every fact rides the assessment beside the verdict facts.
- Receipt: the verdict surfaces on the carrying discipline's `AssessmentResult`: `rule:<name>` facts TRI-STATE — a flag true only where a SATISFIABLE model witnessed every rule, a flag false only for a rule the unsat core names, and `unassessed` text for every rule an UNSAT decided nothing about — beside rational witnesses as ratio facts, non-rational witnesses as exact text, `implied` rows for each universal literal, the rule half of the raw unsat core, one `unsat-assumption` row per `hyp@` literal and one `unsat-box` row per `bound@` literal, the coverage population/grounded ratios with one `coverage-missing` row per unbound member, or the typed unknown triple (`satisfy-unknown-phase`/`satisfy-unknown-kind`/`satisfy-unknown` — the `SolvePhase`/`FailureKind` evidence stays typed at the assessment boundary, never a bare reason string) beside `satisfy-decisions`/`satisfy-conflicts`/`satisfy-restarts`, each present only where the run's own counter table published it; no satisfy-local receipt exists.
- Packages: Microsoft.Z3 (the `Context` AST factory/arena and `AssertAndTrack`/`Check`/`Model`/`UnsatCore`/`Consequences`/`Statistics`/`ReasonUnknown`/`Mk*` term surface — MIT; arm64 native Forge-provisioned, fault-at-init), AngouriMath (the `Entity.Statement` rule source, the one lowering algebra), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm (project, the kernel `Op` key), Rasm.Element (project, the `ElementGraph` population the groundings derive from), BCL inbox.
- Growth: a new rule is one `ComplianceRule` DATA row; a new element population under an existing rule is one `RuleGrounding` row (the template quantifies, never a per-element rule copy) or one `NodeClassSelector` the derivation folds; a new lowered node family is one `RuleLowering` arm (the walk rails typed on an unmapped node, never silently); a new verdict projection is one field on the verdict case; a further search counter is one `SearchCounters` column with its own Z3 key; a new binding source under an existing selector is one row on `NodeClassSelector.Bindings`; zero new surface.
- Boundary: Z3 VERIFIES-AND-EXPLAINS, CP-SAT OPTIMIZES — a rule-consistency question with an unsat-core explanation lands here, a design-space search on `Solver/optimizer`'s cp-sat/milp rows, and cross-wiring either engine onto the other is rejected; the lowering source is the CAS, so the `Symbolic/dimensional#DIMENSION_PROOF` gate proves a rule's unit-consistency BEFORE it asserts and a stringly rule DSL beside the CAS is rejected; the `Context` is per-check and dies with the verdict, so a cached global arena — which Z3 does not make thread-safe across workers — is unreachable by construction; the lowering rails an unmapped CAS node as a typed `<satisfy-unmapped-node:{kind}>` fault on the same `Fin` the verdict rides, so a foreign `Entity` shape is a named refusal rather than an exception caught as an opaque `z3:` string; `UNKNOWN` stays honest AND MEASURED — the NRA/NIA fragment is undecidable in general, so the policy timeout and `ReasonUnknown` surface as the typed shortfall beside the solver's own decision, conflict, and restart counts, never a coerced SAT/UNSAT nor a managed fallback SMT when the Forge-provisioned native is absent; every counter is `Option<long>` because the statistics indexer answers `null` on a key the running tactic never published and a zero there would name a search that decided nothing.
- Boundary: TRACKING is total — the declared box asserts under its own `bound@<name>` literal and a hypothesis rule under `hyp@<name>`, so an infeasible box and a refuted assumption land IN the core under their own namespace instead of vanishing into an untracked assertion that reports an empty core for a genuinely unsatisfiable problem, and the three literal classes partition by prefix rather than by string archaeology over a rule roster.
- Boundary: an UNSAT core proves the conflicting SUBSET alone, so a rule outside it was never decided — the facts publish `unassessed` for it and reserve the passing flag for the SATISFIABLE branch where the model witnesses every rule; a blanket true outside the core is the deleted form that reports a pass no solver established.
- Boundary: the universal `Consequences` extraction rides one policy column on the SATISFIABLE branch of the same holding — no second verdict authority, no re-`Check`, and an empty implied set where the policy declines or the extraction cannot settle.
- Boundary: the `Symbolic/lowering#ENCLOSURE_AND_COLUMNS` `EnclosureFold.Certify` interval pre-gate answers a rule whose enclosure proves over the declared bounds BEFORE the Z3 context is minted — the COMPLETE `ProvenViolated` roster short-circuits `Unsatisfiable` (a single-rule core hides every other rule the same box already refutes), an all-`ProvenSatisfied` roster short-circuits `Satisfiable`, `Indeterminate` falls through to the exact engine, and the gate is a filter over the same admitted `SymbolicExpr`, never a second verdict authority.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
public sealed record ComplianceRule(string Name, SymbolicExpr Constraint, string Citation, Seq<RuleGrounding> Grounding, bool Hypothesis);

public sealed record RuleGrounding(string Element, Map<string, double> Bindings);

public sealed record SatisfyPolicy(Duration Timeout, bool WitnessCompletion = true, bool Implications = false, bool RequireCoverage = true) {
    public static readonly SatisfyPolicy Canonical = new(Duration.FromSeconds(30));

    public bool Invalid => Timeout <= Duration.Zero || Timeout.TotalMilliseconds > uint.MaxValue;
}

// Search counters read off the solver's own `Statistics` table after the check that produced the verdict. Every
// column is `Option<long>` because the indexer answers `null` on a key the running tactic never published, and a
// zero there would read as a solver that decided nothing — the discrimination the whole carrier exists to make.
public readonly record struct SearchCounters(Option<long> Decisions, Option<long> Conflicts, Option<long> Restarts) {
    public static readonly SearchCounters Absent = new(None, None, None);

    // Z3 publishes its counters under its own names; the roster is closed here so a renamed upstream key surfaces
    // as absence rather than as a fabricated count.
    public static SearchCounters Of(Microsoft.Z3.Statistics statistics) =>
        new(Counter(statistics, "decisions"), Counter(statistics, "conflicts"), Counter(statistics, "restarts"));

    // `Entry` discriminates its own storage, so a double-valued counter narrows and an unset flag pair answers
    // absence rather than reading whichever field happens to hold zero.
    static Option<long> Counter(Microsoft.Z3.Statistics statistics, string key) =>
        statistics[key] switch {
            { IsUInt: true } entry => Some((long)entry.UIntValue),
            { IsDouble: true } entry when double.IsFinite(entry.DoubleValue) => Some((long)entry.DoubleValue),
            _ => None,
        };

    public Seq<AssessmentFact> Facts =>
        Decisions.Map(static value => AssessmentFact.Ratio("satisfy-decisions", value)).ToSeq()
        + Conflicts.Map(static value => AssessmentFact.Ratio("satisfy-conflicts", value)).ToSeq()
        + Restarts.Map(static value => AssessmentFact.Ratio("satisfy-restarts", value)).ToSeq();
}

// Tracking-literal namespaces. Every assertion the solver tracks carries its class in the literal itself, so an
// unsat core partitions by prefix — a rule name that happens to read like a bound name can never be mistaken for
// one, and a new tracked class is one row here plus one arm on `Tracked.Of`.
public static class Tracked {
    public const string Hypothesis = "hyp@";
    public const string Bound = "bound@";

    // The rule a literal names, absent for the box and assumption namespaces: a rule literal is `<name>` or
    // `<name>@<element>`, so the name is the head up to the first element separator.
    public static Option<string> Rule(string literal) {
        if (literal.StartsWith(Hypothesis, StringComparison.Ordinal) || literal.StartsWith(Bound, StringComparison.Ordinal)) { return None; }
        int separator = literal.IndexOf('@', StringComparison.Ordinal);
        return Some(separator < 0 ? literal : literal[..separator]);
    }
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

    // `Witness` is ONE design that passes; `Implied` is what EVERY passing design must satisfy — the answer a
    // code-compliance consumer actually asks, and what makes the `Citation` column on `ComplianceRule` actionable
    // rather than decorative. The set is empty when the policy declines the extraction, never absent as a case.
    public sealed record Satisfiable(Map<string, WitnessValue> Witness, Seq<string> Implied) : SatisfyVerdict;

    // The COMPLETE conflicting set, never its first member: an interval pre-gate that refutes four rules over the
    // same box and reports one leaves three violations for a second campaign to rediscover.
    public sealed record Unsatisfiable(Seq<string> ViolatedRules) : SatisfyVerdict;

    // `Reason` cannot separate a timeout at three decisions — a lowering pathology a caller fixes by re-encoding —
    // from one at three million, a genuinely hard NRA fragment where raising `SatisfyPolicy.Timeout` is the only
    // move; the counters read off the same solver holding are what decide it.
    public sealed record Unknown(SolvePhase Phase, FailureKind Kind, string Reason, SearchCounters Counters) : SatisfyVerdict;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class RuleSatisfaction {
    public static Fin<SatisfyVerdict> Check(
        Seq<ComplianceRule> rules, Map<string, (double Lower, double Upper)> bounds, SatisfyPolicy policy, Seq<CoverageFact> coverage = default) =>
        from _ in Admit(rules, bounds, policy, coverage)
        from verdict in Pregate(rules, bounds).Match(
            Some: Fin.Succ,
            None: () => CheckExact(rules, bounds, policy))
        select verdict;

    // Interval pre-gate over the SAME admitted rule set: each ungrounded comparison rule adapts to g(x) ≤ 0 and
    // certifies through EnclosureFold.Certify over the declared box BEFORE any native allocation — the whole
    // ProvenViolated roster settles Unsatisfiable, an all-ProvenSatisfied roster settles Satisfiable at the box
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
        Seq<string> violated = certified
            .Filter(static pair => pair.Verdict is IntervalVerdict.ProvenViolated)
            .Map(static pair => pair.Rule.Name);
        return !violated.IsEmpty
            ? Some((SatisfyVerdict)new SatisfyVerdict.Unsatisfiable(violated))
            // Implications stay EMPTY on the pre-gate branch: an enclosure proves the box satisfies, never what
            // every model must carry, so the extraction rides the exact engine alone rather than inventing a
            // universal claim from an interval bound.
            : certified.ForAll(static pair => pair.Verdict is IntervalVerdict.ProvenSatisfied)
                ? Some((SatisfyVerdict)new SatisfyVerdict.Satisfiable(
                    order.Fold(Map<string, WitnessValue>(), (acc, name) => acc.Add(name, new WitnessValue.Rational((bounds[name].Lower + bounds[name].Upper) * 0.5))),
                    Seq<string>()))
                : Option<SatisfyVerdict>.None;
    }

    // Comparison statements adapt to the g(x) ≤ 0 enclosure form; any other statement shape is exact-rail-only.
    static Option<SymbolicExpr> Gform(SymbolicExpr constraint) => constraint.Entity switch {
        Entity.LessOrEqualf le => Some(SymbolicExpr.Of(le.Left - le.Right)),
        Entity.Lessf lt => Some(SymbolicExpr.Of(lt.Left - lt.Right)),
        Entity.GreaterOrEqualf ge => Some(SymbolicExpr.Of(ge.Right - ge.Left)),
        Entity.Greaterf gt => Some(SymbolicExpr.Of(gt.Right - gt.Left)),
        _ => Option<SymbolicExpr>.None,
    };

    // The throw funnel brackets the NATIVE boundary alone; the lowering's own typed refusal rails through the
    // inner `Fin`, so an unmapped CAS node keeps its name instead of arriving as an exception type string.
    static Fin<SatisfyVerdict> CheckExact(Seq<ComplianceRule> rules, Map<string, (double Lower, double Upper)> bounds, SatisfyPolicy policy) =>
        Try.lift(() => {
            using Microsoft.Z3.Context context = new();
            using Microsoft.Z3.Solver solver = context.MkSolver();
            solver.Set("timeout", (uint)policy.Timeout.TotalMilliseconds);
            Map<string, Microsoft.Z3.RealExpr> variables = Map<string, Microsoft.Z3.RealExpr>();
            foreach ((string name, (double lower, double upper)) in bounds) {
                Microsoft.Z3.RealExpr variable = context.MkRealConst(name);
                variables = variables.Add(name, variable);
                // The declared box is TRACKED like every rule: a box no rule set can satisfy lands in the core
                // under its own literal instead of reporting an empty core for an unsatisfiable problem.
                solver.AssertAndTrack(
                    context.MkAnd(
                        context.MkGe(variable, context.MkReal(lower.ToString(System.Globalization.CultureInfo.InvariantCulture))),
                        context.MkLe(variable, context.MkReal(upper.ToString(System.Globalization.CultureInfo.InvariantCulture)))),
                    context.MkBoolConst($"{Tracked.Bound}{name}"));
            }
            Option<Error> lowering = None;
            bool framed = false;
            foreach (bool hypothesis in new[] { false, true }) {
                if (hypothesis) {
                    if (!rules.Exists(static rule => rule.Hypothesis)) { continue; }
                    solver.Push();
                    framed = true;
                }
                string prefix = hypothesis ? Tracked.Hypothesis : "";
                foreach (ComplianceRule rule in rules.Filter(rule => rule.Hypothesis == hypothesis)) {
                    if (rule.Grounding.IsEmpty) {
                        lowering = Track(solver, context, RuleLowering.Lower(context, variables, rule), $"{prefix}{rule.Name}");
                        if (lowering.IsSome) { break; }
                        continue;
                    }
                    foreach (RuleGrounding ground in rule.Grounding) {
                        Map<string, Microsoft.Z3.RealExpr> bound = variables;
                        foreach ((string name, double value) in ground.Bindings) {
                            bound = bound.AddOrUpdate(name, (Microsoft.Z3.RealExpr)context.MkReal(value.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                        }
                        lowering = Track(solver, context, RuleLowering.Lower(context, bound, rule), $"{prefix}{rule.Name}@{ground.Element}");
                        if (lowering.IsSome) { break; }
                    }
                    if (lowering.IsSome) { break; }
                }
                if (lowering.IsSome) { break; }
            }
            if (lowering.Case is Error refused) { return Fin.Fail<SatisfyVerdict>(refused); }
            SatisfyVerdict verdict = solver.Check() switch {
                Microsoft.Z3.Status.SATISFIABLE => (SatisfyVerdict)new SatisfyVerdict.Satisfiable(
                    variables.Map((name, variable) => (name, Value: solver.Model.Evaluate(variable, completion: policy.WitnessCompletion)))
                        .Values.ToSeq()
                        .Fold(Map<string, WitnessValue>(), (acc, pair) => acc.Add(pair.name, RuleLowering.Witness(pair.Value))),
                    Implied(solver, variables, policy)),
                Microsoft.Z3.Status.UNSATISFIABLE => new SatisfyVerdict.Unsatisfiable(
                    toSeq(solver.UnsatCore).Map(static literal => literal.ToString())),
                // Counters read from the SAME holding that produced the verdict; a second `Check` would re-run the
                // search and report a different one.
                _ => new SatisfyVerdict.Unknown(SolvePhase.Solve, FailureKind.Numeric, solver.ReasonUnknown, SearchCounters.Of(solver.Statistics)),
            };
            if (framed) { solver.Pop(); }
            return Fin.Succ(verdict);
        }).Run()
        .MapFail(static error => (Error)new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Numeric, $"<z3:{error.GetType().Name}:{error.Message}>"))
        .Bind(static inner => inner);

    static Option<Error> Track(Microsoft.Z3.Solver solver, Microsoft.Z3.Context context, Fin<Microsoft.Z3.BoolExpr> lowered, string literal) =>
        lowered.Match(
            Succ: assertion => { solver.AssertAndTrack(assertion, context.MkBoolConst(literal)); return Option<Error>.None; },
            Fail: Some);

    // Universal implications over the declared variable set, on the SAME holding the witness came from: the fixed
    // point every model satisfies, so a rule roster answers "every passing design carries beam depth >= 450" and
    // not merely "here is one that does". An empty roster is the honest read for a policy that declines the cost,
    // for a status the extraction cannot settle, and for a rule set implying nothing beyond its own bounds.
    static Seq<string> Implied(Microsoft.Z3.Solver solver, Map<string, Microsoft.Z3.RealExpr> variables, SatisfyPolicy policy) =>
        !policy.Implications
            ? Seq<string>()
            : solver.Consequences([], variables.Values.ToSeq().Map(static variable => (Microsoft.Z3.Expr)variable), out Microsoft.Z3.BoolExpr[] implied)
              is Microsoft.Z3.Status.SATISFIABLE
                ? toSeq(implied).Map(static literal => literal.ToString())
                : Seq<string>();

    static Fin<Unit> Admit(Seq<ComplianceRule> rules, Map<string, (double Lower, double Upper)> bounds, SatisfyPolicy policy, Seq<CoverageFact> coverage) {
        if (policy.Invalid || rules.IsEmpty || rules.Map(static rule => rule.Name).ToHashSet(StringComparer.Ordinal).Count != rules.Count)
            return Fin.Fail<Unit>(ComputeFault.Create("<satisfy-invalid-policy-or-rule-set>"));
        // A rule quantifying over fewer members than its own class holds answers a NARROWER question than the one
        // asked, and the verdict reads identically either way — so the gap refuses here rather than surfacing as a
        // clean SATISFIABLE over a partial population.
        if (policy.RequireCoverage && coverage.Exists(static fact => !fact.Complete))
            return Fin.Fail<Unit>(ComputeFault.Create("<satisfy-coverage-gap>"));
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

    // A tracking literal reserves `@` for its own namespacing, so a rule, element, or bound name never carries one.
    static bool Name(string value) =>
        !string.IsNullOrWhiteSpace(value) && value[0] != '@' && value.All(static character => char.IsLetterOrDigit(character) || character is '_' or '-' or '.');

    public static Fin<Seq<AssessmentFact>> Facts(Seq<ComplianceRule> rules, SatisfyVerdict verdict, Seq<CoverageFact> coverage = default) =>
        verdict.Switch(
            state: (Rules: rules, Coverage: coverage.IsEmpty ? Seq<CoverageFact>() : coverage),
            satisfiable: static (s, sat) =>
                sat.Witness.Map(static (name, value) => (name, value)).Values.ToSeq()
                    .TraverseM(static pair => WitnessFact(pair.name, pair.value)).As()
                    .Map(witness => s.Rules.Map(static rule => AssessmentFact.Flag($"rule:{rule.Name}", true))
                        + witness
                        + sat.Implied.Map(static literal => AssessmentFact.Text("implied", literal))
                        + s.Coverage.Bind(static fact => fact.Facts)),
            unsatisfiable: static (s, unsat) => Fin.Succ(
                s.Rules.Map(rule => Assessed(rule, unsat.ViolatedRules))
                + Seq(AssessmentFact.Text("unsat-core", string.Join(",", unsat.ViolatedRules.Choose(Tracked.Rule))))
                + unsat.ViolatedRules.Filter(static literal => literal.StartsWith(Tracked.Hypothesis, StringComparison.Ordinal))
                    .Map(static literal => AssessmentFact.Text("unsat-assumption", literal))
                + unsat.ViolatedRules.Filter(static literal => literal.StartsWith(Tracked.Bound, StringComparison.Ordinal))
                    .Map(static literal => AssessmentFact.Text("unsat-box", literal))
                + s.Coverage.Bind(static fact => fact.Facts)),
            unknown: static (s, unknown) => Fin.Succ(Seq(
                AssessmentFact.Text("satisfy-unknown-phase", unknown.Phase.Key),
                AssessmentFact.Text("satisfy-unknown-kind", unknown.Kind.Key),
                AssessmentFact.Text("satisfy-unknown", unknown.Reason))
                + unknown.Counters.Facts
                + s.Coverage.Bind(static fact => fact.Facts)));

    // TRI-STATE: the core names the conflicting subset, so a rule outside it was never decided and reports
    // `unassessed`. A false flag is a proven violation; a true flag exists only where a model witnessed the rule.
    static AssessmentFact Assessed(ComplianceRule rule, Seq<string> core) =>
        core.Choose(Tracked.Rule).Exists(name => name == rule.Name)
            ? AssessmentFact.Flag($"rule:{rule.Name}", false)
            : AssessmentFact.Text($"rule:{rule.Name}", "unassessed");

    static Fin<AssessmentFact> WitnessFact(string name, WitnessValue value) =>
        value.Switch(
            rational: rational => AssessmentFact.Ratio($"witness:{name}", rational.Value),
            exact: exact => Fin.Succ(AssessmentFact.Text($"witness:{name}", exact.Value)));
}

public static class RuleLowering {
    public static Fin<Microsoft.Z3.BoolExpr> Lower(Microsoft.Z3.Context context, Map<string, Microsoft.Z3.RealExpr> variables, ComplianceRule rule) =>
        rule.Constraint.Entity switch {
            Entity.Equalsf(Entity.Statement left, Entity.Statement right) =>
                from l in Lower(context, variables, rule with { Constraint = SymbolicExpr.Of(left) })
                from r in Lower(context, variables, rule with { Constraint = SymbolicExpr.Of(right) })
                select context.MkIff(l, r),
            Entity.Equalsf(Entity left, Entity right) => Relate(context, variables, left, right, context.MkEq),
            Entity.Greaterf(Entity left, Entity right) => Relate(context, variables, left, right, context.MkGt),
            Entity.GreaterOrEqualf(Entity left, Entity right) => Relate(context, variables, left, right, context.MkGe),
            Entity.Lessf(Entity left, Entity right) => Relate(context, variables, left, right, context.MkLt),
            Entity.LessOrEqualf(Entity left, Entity right) => Relate(context, variables, left, right, context.MkLe),
            Entity.Andf(Entity left, Entity right) => Connect(context, variables, rule, left, right, context.MkAnd),
            Entity.Orf(Entity left, Entity right) => Connect(context, variables, rule, left, right, context.MkOr),
            Entity.Xorf(Entity left, Entity right) => Connect(context, variables, rule, left, right, context.MkXor),
            Entity.Impliesf(Entity assumption, Entity conclusion) => Connect(context, variables, rule, assumption, conclusion, context.MkImplies),
            Entity.Notf(Entity inner) => Lower(context, variables, rule with { Constraint = SymbolicExpr.Of(inner) }).Map(context.MkNot),
            Entity node => Fin.Fail<Microsoft.Z3.BoolExpr>(ComputeFault.Create($"<satisfy-non-statement:{rule.Name}:{node.GetType().Name}>")),
        };

    // Relate and Connect are the arity-2 folds every binary arm shares: the arm supplies its own `Mk*` combinator
    // and the walk is written once, so a new relational or connective family is one row on the dispatch above.
    static Fin<Microsoft.Z3.BoolExpr> Relate(
        Microsoft.Z3.Context context, Map<string, Microsoft.Z3.RealExpr> variables, Entity left, Entity right,
        Func<Microsoft.Z3.ArithExpr, Microsoft.Z3.ArithExpr, Microsoft.Z3.BoolExpr> relate) =>
        from l in Arith(context, variables, left)
        from r in Arith(context, variables, right)
        select relate(l, r);

    static Fin<Microsoft.Z3.BoolExpr> Connect(
        Microsoft.Z3.Context context, Map<string, Microsoft.Z3.RealExpr> variables, ComplianceRule rule, Entity left, Entity right,
        Func<Microsoft.Z3.BoolExpr, Microsoft.Z3.BoolExpr, Microsoft.Z3.BoolExpr> connect) =>
        from l in Lower(context, variables, rule with { Constraint = SymbolicExpr.Of(left) })
        from r in Lower(context, variables, rule with { Constraint = SymbolicExpr.Of(right) })
        select connect(l, r);

    static Fin<Microsoft.Z3.ArithExpr> Arith(Microsoft.Z3.Context context, Map<string, Microsoft.Z3.RealExpr> variables, Entity node) =>
        node switch {
            Entity.Number.Rational rational => Fin.Succ((Microsoft.Z3.ArithExpr)context.MkReal(rational.Stringize())),
            Entity.Number.Real real => Fin.Succ((Microsoft.Z3.ArithExpr)context.MkReal(real.Stringize())),
            Entity.Variable variable => variables.Find(variable.Name)
                .Map(static bound => (Microsoft.Z3.ArithExpr)bound)
                .ToFin(ComputeFault.Create($"<satisfy-unbound-variable:{variable.Name}>")),
            Entity.Sumf(Entity left, Entity right) => Combine(context, variables, left, right, static (c, l, r) => (Microsoft.Z3.ArithExpr)c.MkAdd(l, r)),
            Entity.Minusf(Entity left, Entity right) => Combine(context, variables, left, right, static (c, l, r) => (Microsoft.Z3.ArithExpr)c.MkSub(l, r)),
            Entity.Mulf(Entity left, Entity right) => Combine(context, variables, left, right, static (c, l, r) => (Microsoft.Z3.ArithExpr)c.MkMul(l, r)),
            Entity.Divf(Entity left, Entity right) => Combine(context, variables, left, right, static (c, l, r) => c.MkDiv(l, r)),
            Entity.Powf(Entity left, Entity right) => Combine(context, variables, left, right, static (c, l, r) => (Microsoft.Z3.ArithExpr)c.MkPower(l, r)),
            Entity.Absf(Entity argument) => Arith(context, variables, argument).Map(value => Rectified(context, value)),
            Entity.Signumf(Entity argument) => Arith(context, variables, argument).Map(value => Sign(context, value)),
            // A CAS node this walk does not map is a NAMED refusal on the verdict rail, so the caller learns which
            // family to encode away rather than reading an exception type through the native throw funnel.
            Entity node => Fin.Fail<Microsoft.Z3.ArithExpr>(ComputeFault.Create($"<satisfy-unmapped-node:{node.GetType().Name}>")),
        };

    static Fin<Microsoft.Z3.ArithExpr> Combine(
        Microsoft.Z3.Context context, Map<string, Microsoft.Z3.RealExpr> variables, Entity left, Entity right,
        Func<Microsoft.Z3.Context, Microsoft.Z3.ArithExpr, Microsoft.Z3.ArithExpr, Microsoft.Z3.ArithExpr> combine) =>
        from l in Arith(context, variables, left)
        from r in Arith(context, variables, right)
        select combine(context, l, r);

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

## [03]-[RULE_POPULATION_DERIVATION]

- Owner: `NodeClassSelector` the typed selector over `Element/graph/element#ELEMENT_GRAPH` node classes — the classification row and `ObjectKind` predicate naming the member class, and the binding roster mapping each rule symbol onto the Element-declared property row its value reads from; `RuleSatisfaction.Ground` the derivation folding one rule TEMPLATE and one selector into the grounded rule the exact rail asserts.
- Entry: `public static Fin<ComplianceRule> Ground(ElementGraph graph, ComplianceRule template, NodeClassSelector selector)` — the template carries an EMPTY grounding roster and a selector-covered free-variable set, and the fold returns it carrying one `RuleGrounding` per matching member whose `Element` is the node id text. A caller-supplied grounding roster stays the manual lane and never enters this fold.
- Auto: `Ground` walks `ElementGraph.ObjectNodes`, admits each member through `NodeClassSelector.Admits`, bakes it through the memoized `Bake` fold so the type→occurrence inheritance the seam owns applies once, and binds every declared symbol off the baked property bags; a member missing one symbol yields NO grounding, so the population and grounded counts diverge and `[04]` names the gap.
- Boundary: the selector composes the DECLARING package's own row statics — `StructuralRows` for the cross-package structural vocabulary, the owning package's `PropertyCategory` roster otherwise — because a call-site `PropertyName.Create` forks the spelling the declaring package already froze and the fork surfaces only as a rule that silently grounds nothing. Numeric admission is the three scalar `PropertyValue` arms alone: a measured row reads its SI magnitude, a number and an integer read their own value, and every other value case is NOT a rule binding — a text or enumerated row coerced to a double is the deleted form. The assessment spine stays the CALLER'S: this fold derives a rule, never a verdict, and never mints an `AssessmentResult`.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------

// One typed selector over the seam's node classes a rule population is drawn from: the classification row and the
// object-kind predicate that name the class, and the binding roster that names WHICH declared property row feeds
// each rule symbol. Both halves are DATA, so a new population under an existing rule is one selector value and a
// new binding one roster row — never a per-element rule copy the template exists to foreclose.
public sealed record NodeClassSelector(string ClassificationSystem, string ClassificationCode, ObjectKind Kind, Map<string, PropertyName> Bindings) {
    public bool Invalid =>
        string.IsNullOrWhiteSpace(ClassificationSystem) || string.IsNullOrWhiteSpace(ClassificationCode) || Bindings.IsEmpty;

    public bool Admits(Node.Object node) =>
        node.Kind == Kind
        && string.Equals(node.Classification.System, ClassificationSystem, StringComparison.Ordinal)
        && string.Equals(node.Classification.Code, ClassificationCode, StringComparison.Ordinal);

    // ALL-OR-NOTHING per member: a partially bound member grounds nothing, because a rule asserted over a subset of
    // its own symbols is a different rule. The absence is the coverage evidence `[04]` publishes.
    public Option<Map<string, double>> Bind(Element member) =>
        Bindings.Fold(Some(Map<string, double>()), (acc, symbol, row) =>
            acc.Bind(bound => member.Properties
                .Choose(bag => bag.Find(row))
                .Head
                .Bind(Numeric)
                .Map(value => bound.Add(symbol, value))));

    // The three SCALAR arms are the whole numeric vocabulary of a rule binding — a measured row through its SI
    // magnitude, a number and an integer through their own value. Every other value case answers absent rather than
    // coercing: a text, enumerated, bounded, or table row read as a double asserts a number the model never carried.
    static Option<double> Numeric(PropertyValue value) => value switch {
        PropertyValue.Measure measure => Some(measure.Value.Si),
        PropertyValue.Number number when double.IsFinite(number.Value) => Some(number.Value),
        PropertyValue.Integer integer => Some((double)integer.Value),
        _ => Option<double>.None,
    };
}

// --- [OPERATIONS] --------------------------------------------------------------------------

public static partial class RuleSatisfaction {
    private static readonly Op GroundKey = Op.Of(name: nameof(Ground));

    public static Fin<ComplianceRule> Ground(ElementGraph graph, ComplianceRule template, NodeClassSelector selector) =>
        selector.Invalid || !template.Grounding.IsEmpty
            ? Fin.Fail<ComplianceRule>(ComputeFault.Create($"<satisfy-grounding-template:{template.Name}>"))
            : Population(graph, selector)
                .TraverseM(node => graph.Bake(node.Id, GroundKey).Map(member => (Id: node.Id, Member: member))).As()
                .Map(members => template with {
                    Grounding = members.Choose(pair =>
                        selector.Bind(pair.Member).Map(bindings => new RuleGrounding(pair.Id.Value, bindings))),
                });

    // The class POPULATION: every object node the selector admits, read once and shared by the derivation and the
    // coverage proof so the two can never disagree about what the class holds.
    internal static Seq<Node.Object> Population(ElementGraph graph, NodeClassSelector selector) =>
        graph.ObjectNodes.Filter(selector.Admits);
}
```

## [04]-[RULE_COVERAGE_PROOF]

- Owner: `CoverageFact` the population-versus-grounded evidence carrier with the roster of members the selector admitted but could not bind; `RuleSatisfaction.Coverage` the proof fold over the same population the derivation walks.
- Entry: `public static CoverageFact Coverage(ElementGraph graph, NodeClassSelector selector, ComplianceRule rule)` — the rule supplies the grounding roster actually asserted, and the fact reports how much of the admitted class it covers; the read is total over an admitted graph, so no rail wraps it.
- Boundary: `Check` consumes the fact — an incomplete fact refuses under `SatisfyPolicy.RequireCoverage` and rides the assessment as evidence otherwise — because a SATISFIABLE verdict over a partial population reads identically to one over the whole class, and the difference is exactly what a compliance consumer is asking about. The fact lands beside the verdict facts on the SAME `AssessmentResult`, never as a second receipt.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------

// What the rule ACTUALLY quantified over against what its own class holds. `Missing` names each admitted member the
// selector could not bind, so a coverage gap is a repairable roster rather than a count a reader cannot act on.
public readonly record struct CoverageFact(int Population, int Grounded, Seq<string> Missing) {
    public bool Complete => Grounded == Population && Missing.IsEmpty;

    public Seq<AssessmentFact> Facts =>
        Seq(AssessmentFact.Ratio("coverage-population", Population), AssessmentFact.Ratio("coverage-grounded", Grounded))
        + Missing.Map(static element => AssessmentFact.Text("coverage-missing", element));
}

// --- [OPERATIONS] --------------------------------------------------------------------------

public static partial class RuleSatisfaction {
    public static CoverageFact Coverage(ElementGraph graph, NodeClassSelector selector, ComplianceRule rule) {
        Seq<Node.Object> population = Population(graph, selector);
        HashSet<string> grounded = [.. rule.Grounding.Map(static ground => ground.Element)];
        return new CoverageFact(
            population.Count,
            population.Count(node => grounded.Contains(node.Id.Value)),
            population.Filter(node => !grounded.Contains(node.Id.Value)).Map(static node => node.Id.Value));
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
