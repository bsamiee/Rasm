# [COMPUTE_SOLVER_SATISFY]

Rasm.Compute rule satisfaction: the SMT owner beside the optimizer — Z3 VERIFIES-AND-EXPLAINS where CP-SAT OPTIMIZES, orthogonal concerns on two admitted engines, one page each. Every typed `ComplianceRule` set lowers to `Microsoft.Z3` assertions from the CAS — each rule an AngouriMath `Entity.Statement` walked term-by-term onto `Context.Mk*` terms (the nonlinear NRA/NIA arithmetic CP-SAT cannot reach), asserted through `Solver.AssertAndTrack` under one tracking literal PER RULE so an UNSATISFIABLE `UnsatCore` names the EXACT violated rules, never an opaque refusal. Verdict is the three-way `SatisfyVerdict` — SATISFIABLE carries the `Model` witness beside the `Consequences` implied set every passing design must satisfy, UNSATISFIABLE the unsat-core rule names, UNKNOWN a typed `(Solve, Numeric)` shortfall beside the solver's own decision, conflict, and restart counts — surfacing as an `AssessmentResult` a discipline route carries.

Ownership is ONE `Context` per CHECK — the AST factory and arena (`IDisposable`; every `Expr`/`Sort`/`Solver` it mints dies with it) minted inside `CheckExact` and disposed at the verdict boundary, so a `Runtime/scheduling#JOB_GRAPH` sweep worker never shares an arena and no context outlives the verdict that produced it. Osx-arm64 `libz3` provisions through the Forge nix lane (NuGet stable ships win-x64/osx-x64 natives only); a `Context` operation without the native FAULTS AT INIT, never a silent degrade. One `Discipline.Compliance` contract row mints ONLY when a verdict must persist as its own content-keyed `Node.Assessment` the `Analysis/assessment` Sweep dispatches — a verdict enriching an existing discipline's `AssessmentResult` rides that route, no `Compliance` row minted this campaign.

## [01]-[INDEX]

- [02]-[RULE_SATISFACTION]: typed `ComplianceRule` set lowered CAS→Z3, the tracked-assertion `SatisfyVerdict` three-way with unsat-core explanation, per-check `Context` arena.
- [03]-[RULE_POPULATION_DERIVATION]: typed node-class selector over the contract graph deriving one `RuleGrounding` per matching member from one rule template.
- [04]-[RULE_COVERAGE_PROOF]: population-versus-grounded coverage fact gating a rule whose quantification is short of its own class.

## [02]-[RULE_SATISFACTION]

- Owner: `TrackedName` `[ValueObject<string>]` is the ONE admitted symbol, rule, element, and bound spelling and its grammar; `TrackClass` `[SmartEnum<string>]` closes the tracking-literal namespace and owns `Format`/`Parse` over it; `SatisfyRight` `[SmartEnum<string>] : ICapability<SatisfyRight>` closes the posture axis and `SatisfyPolicy` `[ComplexValueObject]` carries the timeout beside one `CapabilitySet<SatisfyRight>`; `ComplianceRule` carries one named `Entity.Statement`, citation, element-grounding rows, and a hypothesis discriminant; `RuleLowering` walks the same positional nodes as `Symbolic/dimensional#DIMENSION_PROOF`, including Boolean equivalence through `Context.MkIff`; `SatisfyVerdict` `[Union]` carries the three outcomes; `RuleSatisfaction` asserts the declared box and the base rules, brackets one `Solver.Push` frame over the hypothesis pass, checks once inside it, and projects the witness/core.
- Cases: `Satisfiable` carries every declared free variable as `WitnessValue.Rational` or exact Z3 text beside the implied-literal set every model satisfies; `Unsatisfiable` carries tracked `name`/`name@element`/`hyp@name`/`bound@name` literals; `Unknown` carries `(SolvePhase, FailureKind, Reason)` without coercion, beside the `SearchCounters` decisions/conflicts/restarts read off the same holding. `TrackClass` closes at three rows and `SatisfyRight` at three, both ACCUMULATING axes: a fourth tracked class is one row carrying its own prefix and a fourth posture one row on the capability roster.
- Entry: `Check` accumulates every admission defect at once — names, unique tracking identities, finite ordered bounds, grounding coverage, free-variable coverage, and the timeout window — then consumes the `Pregate` interval decision: EVERY `ProvenViolated` rule settles `Unsatisfiable` together and an all-`ProvenSatisfied` roster settles `Satisfiable` at the box midpoint before any native allocation; only an `Indeterminate` remainder mints the bracketed `Context`. Every asserted CAS variable resolves through a declared bound or grounding binding; the lowering never silently mints an untracked symbol. A supplied `CoverageFact` roster gates the same call: an incomplete fact under `SatisfyRight.RequireCoverage` refuses before the check, and every fact rides the assessment beside the verdict facts.
- Result: the verdict surfaces on the carrying discipline's `AssessmentResult`: `rule:<name>` facts TRI-STATE — a flag true only where a SATISFIABLE model witnessed every rule, a flag false only for a rule the unsat core names, and `unassessed` text for every rule an UNSAT decided nothing about — beside one `citation:<name>` row per violated rule (the column the `Implied` law calls actionable is only actionable where the refusal carries it), rational witnesses as ratio facts, non-rational witnesses as exact text, `implied` rows for each universal literal, the rule half of the raw unsat core, one `unsat-assumption` row per `hyp@` literal and one `unsat-box` row per `bound@` literal, the coverage population/grounded ratios with one `coverage-missing` row per unbound member, or the typed unknown triple (`satisfy-unknown-phase`/`satisfy-unknown-kind`/`satisfy-unknown` — the `SolvePhase`/`FailureKind` evidence stays typed at the assessment boundary, never a bare reason string) beside `satisfy-decisions`/`satisfy-conflicts`/`satisfy-restarts`, each present only where the run's own counter table published it; no satisfy-local result exists.
- Packages: Microsoft.Z3 (the `Context` AST factory/arena and `AssertAndTrack`/`Check`/`Model`/`UnsatCore`/`Consequences`/`Statistics`/`ReasonUnknown`/`Push`/`Pop`/`Mk*` term surface — MIT; arm64 native Forge-provisioned, fault-at-init), AngouriMath (the `Entity.Statement` rule source, the one lowering algebra), Thinktecture.Runtime.Extensions (`[ValueObject<string>]`, `[SmartEnum<string>]`, `[ComplexValueObject]`, `[Union]`, generated `Switch`), LanguageExt.Core (`Fin`/`Option`/`Validation`/`Set`/`Map`/`IO.Bracket`), NodaTime, Rasm (project, the kernel `Op` key and the `Domain/validation` `ICapability`/`CapabilitySet` column), Rasm.Element (project, the `ElementGraph` population the groundings derive from), BCL inbox.
- Growth: a new rule is one `ComplianceRule` DATA row; a new element population under an existing rule is one `RuleGrounding` row (the template quantifies, never a per-element rule copy) or one `NodeClassSelector` the derivation folds; a new lowered node family is one `RuleLowering` arm (the walk fails typed on an unmapped node, never silently); a new verdict projection is one field on the verdict case; a further search counter is one `SearchCounters` column with its own Z3 key; a new binding source under an existing selector is one row on `NodeClassSelector.Bindings`; zero new surface.
- Boundary: Z3 VERIFIES-AND-EXPLAINS, CP-SAT OPTIMIZES — a rule-consistency question with an unsat-core explanation lands here, a design-space search on `Solver/optimizer`'s cp-sat/milp rows, and cross-wiring either engine onto the other is rejected; the lowering source is the CAS, so the `Symbolic/dimensional#DIMENSION_PROOF` gate proves a rule's unit-consistency BEFORE it asserts and a stringly rule DSL beside the CAS is rejected; the `Context` is per-check and dies with the verdict, so a cached global arena — which Z3 does not make thread-safe across workers — is unreachable by construction; the lowering fails an unmapped CAS node as a typed `<satisfy-unmapped-node:{kind}>` fault on the same `Fin` the verdict rides, so a foreign `Entity` shape is a named refusal rather than an exception caught as an opaque `z3:` string; `UNKNOWN` stays honest AND MEASURED — the NRA/NIA fragment is undecidable in general, so the policy timeout and `ReasonUnknown` surface as the typed shortfall beside the solver's own decision, conflict, and restart counts, never a coerced SAT/UNSAT nor a managed fallback SMT when the Forge-provisioned native is absent; every counter is `Option<long>` because the statistics indexer answers `null` on a key the running tactic never published and a zero there would name a search that decided nothing.
- Boundary: TRACKING is total — the declared box asserts under its own `bound@<name>` literal and a hypothesis rule under `hyp@<name>`, so an infeasible box and a refuted assumption land IN the core under their own namespace instead of vanishing into an untracked assertion that reports an empty core for a genuinely unsatisfiable problem, and the three literal classes partition by ROW rather than by string archaeology over a rule roster.
- Boundary: an UNSAT core proves the conflicting SUBSET alone, so a rule outside it was never decided — the facts publish `unassessed` for it and reserve the passing flag for the SATISFIABLE branch where the model witnesses every rule; a blanket true outside the core is the deleted form that reports a pass no solver established.
- Boundary: the universal `Consequences` extraction rides one posture row on the SATISFIABLE branch of the same holding — no second verdict authority, no re-`Check`, and an empty implied set where the policy declines or the extraction cannot settle.
- Boundary: the `Symbolic/lowering#ENCLOSURE` `EnclosureFold.Certify` interval pre-gate answers a rule whose enclosure proves over the declared bounds BEFORE the Z3 context is minted — the COMPLETE `ProvenViolated` roster short-circuits `Unsatisfiable` (a single-rule core hides every other rule the same box already refutes), an all-`ProvenSatisfied` roster short-circuits `Satisfiable`, and every other rule falls through to the exact engine. `Certify`'s typed DECLINE propagates as ABSENCE, never as a fabricated `(double.MinValue, double.MaxValue)` enclosure: the owner refuses to widen precisely so an unbounded node stays distinguishable from a box that straddles zero, and re-minting that interval here discards the proof at the owner's own call site.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct TrackedName {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = !string.IsNullOrWhiteSpace(value)
            && value.All(static character => char.IsLetterOrDigit(character) || character is '_' or '-' or '.')
            ? null
            : new ValidationError("TrackedName admits letters, digits, and `_-.` alone, and reserves `@`.");
}

[SmartEnum<string>]
public sealed partial class TrackClass {
    public static readonly TrackClass Rule = new(key: "rule", prefix: "");
    public static readonly TrackClass Hypothesis = new(key: "hyp", prefix: "hyp@");
    public static readonly TrackClass Bound = new(key: "bound", prefix: "bound@");

    public string Prefix { get; }

    public string Format(TrackedName name, Option<TrackedName> element) =>
        element.Match(Some: member => $"{Prefix}{name.Value}@{member.Value}", None: () => $"{Prefix}{name.Value}");

    public static TrackClass Of(string literal) =>
        toSeq(toSeq(Items).Filter(row => row.Prefix.Length > 0 && literal.StartsWith(row.Prefix, StringComparison.Ordinal))
            .OrderByDescending(static row => row.Prefix.Length))
            .Head.IfNone(Rule);

    public Option<TrackedName> Parse(string literal) {
        if (!ReferenceEquals(this, Rule)) { return None; }
        int separator = literal.IndexOf('@', StringComparison.Ordinal);
        return TrackedName.TryCreate(separator < 0 ? literal : literal[..separator], out TrackedName name) ? Some(name) : None;
    }
}

[SmartEnum<string>]
public sealed partial class SatisfyRight : ICapability<SatisfyRight> {
    public static readonly SatisfyRight WitnessCompletion = new(key: "witness-completion");
    public static readonly SatisfyRight Implications = new(key: "implications");
    public static readonly SatisfyRight RequireCoverage = new(key: "require-coverage");
}

[ComplexValueObject]
public sealed partial class SatisfyPolicy {
    public static SatisfyPolicy Canonical { get; } = Create(
        timeout: Duration.FromSeconds(30),
        rights: CapabilitySet<SatisfyRight>.Of(SatisfyRight.WitnessCompletion, SatisfyRight.RequireCoverage));

    public Duration Timeout { get; }
    public CapabilitySet<SatisfyRight> Rights { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref Duration timeout, ref CapabilitySet<SatisfyRight> rights) =>
        validationError = timeout > Duration.Zero && timeout.TotalMilliseconds <= uint.MaxValue
            ? null
            : new ValidationError("SatisfyPolicy requires a positive timeout expressible as uint milliseconds.");

    public uint Milliseconds => (uint)Timeout.TotalMilliseconds;
}

[Equatable]
public sealed partial record ComplianceRule(
    TrackedName Name, SymbolicExpr Constraint, string Citation,
    [property: OrderedEquality] Seq<RuleGrounding> Grounding, bool Hypothesis);

[Equatable]
public sealed partial record RuleGrounding(TrackedName Element, [property: OrderedEquality] Map<TrackedName, double> Bindings);

public readonly record struct SearchCounters(Option<long> Decisions, Option<long> Conflicts, Option<long> Restarts) {
    public static readonly SearchCounters Absent = new(None, None, None);

    public static SearchCounters Of(Microsoft.Z3.Statistics statistics) =>
        new(Counter(statistics, "decisions"), Counter(statistics, "conflicts"), Counter(statistics, "restarts"));

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

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WitnessValue {
    private WitnessValue() { }

    public sealed record Rational(double Value) : WitnessValue;
    public sealed record Exact(string Value) : WitnessValue;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SatisfyVerdict {
    private SatisfyVerdict() { }

    public sealed record Satisfiable(Map<TrackedName, WitnessValue> Witness, Seq<string> Implied) : SatisfyVerdict;

    public sealed record Unsatisfiable(Seq<string> ViolatedRules) : SatisfyVerdict;

    public sealed record Unknown(SolvePhase Phase, FailureKind Kind, string Reason, SearchCounters Counters) : SatisfyVerdict;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class RuleSatisfaction {
    public static Fin<SatisfyVerdict> Check(
        Seq<ComplianceRule> rules, Map<TrackedName, (double Lower, double Upper)> bounds, SatisfyPolicy policy,
        Seq<CoverageFact> coverage = default) =>
        from _ in Admit(rules, bounds, policy, coverage)
        from verdict in Pregate(rules, bounds).Match(
            Some: Fin.Succ,
            None: () => CheckExact(rules, bounds, policy))
        select verdict;

    static Option<SatisfyVerdict> Pregate(Seq<ComplianceRule> rules, Map<TrackedName, (double Lower, double Upper)> bounds) {
        Seq<TrackedName> order = toSeq(bounds.Keys);
        ImmutableArray<Interval> box = [.. order.Map(name => Interval.Of(bounds[name].Lower, bounds[name].Upper))];
        Seq<(ComplianceRule Rule, Option<EnclosureVerdict> Verdict)> certified = rules.Map(rule => (
            Rule: rule,
            Verdict: from _ in guard(rule.Grounding.IsEmpty, unit).ToFin().ToOption()
                     from g in Gform(rule.Constraint)
                     from verdict in EnclosureFold.Certify(g, order, box).ToOption()
                     select verdict));
        Seq<string> violated = certified
            .Filter(static pair => pair.Verdict.Case is EnclosureVerdict.ProvenViolated)
            .Map(static pair => pair.Rule.Name.Value);
        return !violated.IsEmpty
            ? Some((SatisfyVerdict)new SatisfyVerdict.Unsatisfiable(violated))
            : certified.ForAll(static pair => pair.Verdict.Case is EnclosureVerdict.ProvenSatisfied)
                ? Some((SatisfyVerdict)new SatisfyVerdict.Satisfiable(
                    order.Fold(Map<TrackedName, WitnessValue>(), (acc, name) =>
                        acc.Add(name, new WitnessValue.Rational((bounds[name].Lower + bounds[name].Upper) * 0.5))),
                    Seq<string>()))
                : Option<SatisfyVerdict>.None;
    }

    static Option<SymbolicExpr> Gform(SymbolicExpr constraint) => constraint.Entity switch {
        Entity.LessOrEqualf le => Some(SymbolicExpr.Of(le.Left - le.Right)),
        Entity.Lessf lt => Some(SymbolicExpr.Of(lt.Left - lt.Right)),
        Entity.GreaterOrEqualf ge => Some(SymbolicExpr.Of(ge.Right - ge.Left)),
        Entity.Greaterf gt => Some(SymbolicExpr.Of(gt.Right - gt.Left)),
        _ => Option<SymbolicExpr>.None,
    };

    static Fin<SatisfyVerdict> CheckExact(
        Seq<ComplianceRule> rules, Map<TrackedName, (double Lower, double Upper)> bounds, SatisfyPolicy policy) =>
        Try.lift(() => {
            using Microsoft.Z3.Context context = new();
            using Microsoft.Z3.Solver solver = context.MkSolver();
            solver.Set("timeout", policy.Milliseconds);
            Map<TrackedName, Microsoft.Z3.RealExpr> variables = bounds.Fold(
                Map<TrackedName, Microsoft.Z3.RealExpr>(),
                (acc, name, _) => acc.Add(name, context.MkRealConst(name.Value)));
            Boxed(solver, context, variables, bounds);
            return from _ in Assert(solver, context, variables, rules.Filter(static rule => !rule.Hypothesis), TrackClass.Rule)
                   from verdict in Framed(solver, context, variables, rules, policy)
                   select verdict;
        }).Run().Bind(static inner => inner);

    static void Boxed(
        Microsoft.Z3.Solver solver, Microsoft.Z3.Context context, Map<TrackedName, Microsoft.Z3.RealExpr> variables,
        Map<TrackedName, (double Lower, double Upper)> bounds) =>
        bounds.Iter((name, window) => solver.AssertAndTrack(
            context.MkAnd(
                context.MkGe(variables[name], context.MkReal(Exact(window.Lower))),
                context.MkLe(variables[name], context.MkReal(Exact(window.Upper)))),
            context.MkBoolConst(TrackClass.Bound.Format(name, None))));

    static Fin<SatisfyVerdict> Framed(
        Microsoft.Z3.Solver solver, Microsoft.Z3.Context context, Map<TrackedName, Microsoft.Z3.RealExpr> variables,
        Seq<ComplianceRule> rules, SatisfyPolicy policy) =>
        rules.Filter(static rule => rule.Hypothesis) is { IsEmpty: false } assumed
            ? IO.lift(fun(solver.Push))
                .Bracket(
                    Use: _ => IO.lift(() => Assert(solver, context, variables, assumed, TrackClass.Hypothesis)
                        .Map(_ => Settle(solver, variables, policy))),
                    Fin: _ => IO.lift(fun(solver.Pop)))
                .Run()
            : Fin.Succ(Settle(solver, variables, policy));

    static Fin<Unit> Assert(
        Microsoft.Z3.Solver solver, Microsoft.Z3.Context context, Map<TrackedName, Microsoft.Z3.RealExpr> variables,
        Seq<ComplianceRule> admitted, TrackClass tracked) =>
        admitted.TraverseM(rule =>
            rule.Grounding.IsEmpty
                ? Track(solver, context, RuleLowering.Lower(context, variables, rule.Constraint, rule.Name), tracked.Format(rule.Name, None))
                : rule.Grounding.TraverseM(ground => Track(
                    solver, context,
                    RuleLowering.Lower(context, Ground(context, variables, ground), rule.Constraint, rule.Name),
                    tracked.Format(rule.Name, Some(ground.Element)))).As().Map(static _ => unit))
            .As().Map(static _ => unit);

    static Map<TrackedName, Microsoft.Z3.RealExpr> Ground(
        Microsoft.Z3.Context context, Map<TrackedName, Microsoft.Z3.RealExpr> variables, RuleGrounding ground) =>
        ground.Bindings.Fold(variables, (acc, name, value) => acc.AddOrUpdate(name, (Microsoft.Z3.RealExpr)context.MkReal(Exact(value))));

    static string Exact(double value) => value.ToString(System.Globalization.CultureInfo.InvariantCulture);

    static Fin<Unit> Track(Microsoft.Z3.Solver solver, Microsoft.Z3.Context context, Fin<Microsoft.Z3.BoolExpr> lowered, string literal) =>
        lowered.Map(assertion => { solver.AssertAndTrack(assertion, context.MkBoolConst(literal)); return unit; });

    static SatisfyVerdict Settle(
        Microsoft.Z3.Solver solver, Map<TrackedName, Microsoft.Z3.RealExpr> variables, SatisfyPolicy policy) =>
        solver.Check() switch {
            Microsoft.Z3.Status.SATISFIABLE => new SatisfyVerdict.Satisfiable(
                variables.Map((_, variable) => RuleLowering.Witness(
                    solver.Model.Evaluate(variable, completion: policy.Rights.Admits(SatisfyRight.WitnessCompletion)))),
                Implied(solver, variables, policy)),
            Microsoft.Z3.Status.UNSATISFIABLE => new SatisfyVerdict.Unsatisfiable(
                toSeq(solver.UnsatCore).Map(static literal => literal.ToString())),
            _ => new SatisfyVerdict.Unknown(SolvePhase.Solve, FailureKind.Numeric, solver.ReasonUnknown, SearchCounters.Of(solver.Statistics)),
        };

    static Seq<string> Implied(
        Microsoft.Z3.Solver solver, Map<TrackedName, Microsoft.Z3.RealExpr> variables, SatisfyPolicy policy) =>
        !policy.Rights.Admits(SatisfyRight.Implications)
            ? Seq<string>()
            : solver.Consequences([], variables.Values.ToSeq().Map(static variable => (Microsoft.Z3.Expr)variable), out Microsoft.Z3.BoolExpr[] implied)
              is Microsoft.Z3.Status.SATISFIABLE
                ? toSeq(implied).Map(static literal => literal.ToString())
                : Seq<string>();

    static Fin<Unit> Admit(
        Seq<ComplianceRule> rules, Map<TrackedName, (double Lower, double Upper)> bounds, SatisfyPolicy policy,
        Seq<CoverageFact> coverage) =>
        AdmissionSlots.Accumulate(Seq(
            Refusal.Unless(!rules.IsEmpty, ComputeArea.Solver, new ComputeViolation.Capacity(CapacityRequirement.NonEmpty, new CapacityEvidence.Count(rules.Count, 1L))),
            Refusal.Unless(toSet(rules.Map(static rule => rule.Name)).Count == rules.Count, ComputeArea.Solver, new ComputeViolation.Contract(
                ComputeContract.Unique,
                new ContractEvidence.Count(toSet(rules.Map(static rule => rule.Name)).Count, rules.Count))),
            Refusal.Unless(!policy.Rights.Admits(SatisfyRight.RequireCoverage) || coverage.ForAll(static fact => fact.Complete), ComputeArea.Solver,
                new ComputeViolation.Contract(ComputeContract.Complete, new ContractEvidence.Count(coverage.Filter(static fact => fact.Complete).Count, coverage.Count))))
         + toSeq(bounds.AsIterable()).Map(static pair => Refusal.Unless(
             double.IsFinite(pair.Value.Lower) && double.IsFinite(pair.Value.Upper) && pair.Value.Lower <= pair.Value.Upper, ComputeArea.Solver,
             new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Key(pair.Key.Value))))
         + rules.Map(rule => AdmitRule(rule, bounds)))
            .ToFin();

    static Validation<Error, Unit> AdmitRule(ComplianceRule rule, Map<TrackedName, (double Lower, double Upper)> bounds) {
        Seq<string> symbols = toSeq(rule.Constraint.Entity.Vars).Map(static variable => variable.Name);
        Seq<TrackedName> free = symbols.Choose(SymbolName);
        return AdmissionSlots.Accumulate(Seq(
            Refusal.Unless(!string.IsNullOrWhiteSpace(rule.Citation), ComputeArea.Solver, new ComputeViolation.Required(ComputeSubject.Input)),
            Refusal.Unless(free.Count == symbols.Count, ComputeArea.Solver, new ComputeViolation.Shape(ShapeRequirement.Arity, new ShapeEvidence.Count(free.Count, symbols.Count))),
            Refusal.Unless(!rule.Grounding.IsEmpty || free.ForAll(bounds.ContainsKey), ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Complete, new ContractEvidence.Count(free.Filter(bounds.ContainsKey).Count, free.Count))),
            Refusal.Unless(toSet(rule.Grounding.Map(static ground => ground.Element)).Count == rule.Grounding.Count, ComputeArea.Solver, new ComputeViolation.Contract(
                ComputeContract.Unique,
                new ContractEvidence.Count(toSet(rule.Grounding.Map(static ground => ground.Element)).Count, rule.Grounding.Count))))
         + rule.Grounding.Map(ground => AdmissionSlots.Accumulate(Seq(
             Refusal.Unless(free.ForAll(name => bounds.ContainsKey(name) || ground.Bindings.ContainsKey(name)), ComputeArea.Solver,
                 new ComputeViolation.Contract(ComputeContract.Complete, new ContractEvidence.Count(
                     free.Filter(name => bounds.ContainsKey(name) || ground.Bindings.ContainsKey(name)).Count, free.Count))),
             Refusal.Unless(ground.Bindings.Values.ForAll(double.IsFinite), ComputeArea.Solver,
                 new ComputeViolation.NonFinite(ComputeSubject.Value, new ScalarEvidence.Sequence(ground.Bindings.Count))))))
            );
    }

    static Option<TrackedName> SymbolName(string text) =>
        TrackedName.TryCreate(text, out TrackedName admitted) ? Some(admitted) : None;

    public static Fin<Seq<AssessmentFact>> Facts(Seq<ComplianceRule> rules, SatisfyVerdict verdict, Seq<CoverageFact> coverage = default) =>
        verdict.Switch(
            state: (Rules: rules, Coverage: coverage),
            satisfiable: static (s, sat) =>
                sat.Witness.Map(static (name, value) => WitnessFact(name, value)).Values.ToSeq()
                    .TraverseM(static fact => fact).As()
                    .Map(witness => s.Rules.Map(static rule => AssessmentFact.Flag($"rule:{rule.Name.Value}", true))
                        + witness
                        + sat.Implied.Map(static literal => AssessmentFact.Text("implied", literal))
                        + s.Coverage.Bind(static fact => fact.Facts)),
            unsatisfiable: static (s, unsat) => Fin.Succ(
                s.Rules.Bind(rule => Assessed(rule, unsat.ViolatedRules))
                + Seq(AssessmentFact.Text("unsat-core", string.Join(",", unsat.ViolatedRules.Choose(Named).Map(static name => name.Value))))
                + unsat.ViolatedRules.Filter(static literal => ReferenceEquals(TrackClass.Of(literal), TrackClass.Hypothesis))
                    .Map(static literal => AssessmentFact.Text("unsat-assumption", literal))
                + unsat.ViolatedRules.Filter(static literal => ReferenceEquals(TrackClass.Of(literal), TrackClass.Bound))
                    .Map(static literal => AssessmentFact.Text("unsat-box", literal))
                + s.Coverage.Bind(static fact => fact.Facts)),
            unknown: static (s, unknown) => Fin.Succ(Seq(
                AssessmentFact.Text("satisfy-unknown-phase", unknown.Phase.Key),
                AssessmentFact.Text("satisfy-unknown-kind", unknown.Kind.Key),
                AssessmentFact.Text("satisfy-unknown", unknown.Reason))
                + unknown.Counters.Facts
                + s.Coverage.Bind(static fact => fact.Facts)));

    static Option<TrackedName> Named(string literal) => TrackClass.Of(literal).Parse(literal);

    static Seq<AssessmentFact> Assessed(ComplianceRule rule, Seq<string> core) =>
        core.Choose(Named).Exists(name => name == rule.Name)
            ? Seq(AssessmentFact.Flag($"rule:{rule.Name.Value}", false), AssessmentFact.Text($"citation:{rule.Name.Value}", rule.Citation))
            : Seq(AssessmentFact.Text($"rule:{rule.Name.Value}", "unassessed"));

    static Fin<AssessmentFact> WitnessFact(TrackedName name, WitnessValue value) =>
        value.Switch(
            rational: rational => AssessmentFact.Ratio($"witness:{name.Value}", rational.Value),
            exact: exact => Fin.Succ(AssessmentFact.Text($"witness:{name.Value}", exact.Value)));
}

public static class RuleLowering {
    public static Fin<Microsoft.Z3.BoolExpr> Lower(
        Microsoft.Z3.Context context, Map<TrackedName, Microsoft.Z3.RealExpr> variables, SymbolicExpr constraint, TrackedName rule) =>
        constraint.Entity switch {
            Entity.Equalsf(Entity.Statement left, Entity.Statement right) =>
                from l in Lower(context, variables, SymbolicExpr.Of(left), rule)
                from r in Lower(context, variables, SymbolicExpr.Of(right), rule)
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
            Entity.Notf(Entity inner) => Lower(context, variables, SymbolicExpr.Of(inner), rule).Map(context.MkNot),
            Entity node => Fin.Fail<Microsoft.Z3.BoolExpr>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.None()))),
        };

    static Fin<Microsoft.Z3.BoolExpr> Relate(
        Microsoft.Z3.Context context, Map<TrackedName, Microsoft.Z3.RealExpr> variables, Entity left, Entity right,
        Func<Microsoft.Z3.ArithExpr, Microsoft.Z3.ArithExpr, Microsoft.Z3.BoolExpr> relate) =>
        from l in Arith(context, variables, left)
        from r in Arith(context, variables, right)
        select relate(l, r);

    static Fin<Microsoft.Z3.BoolExpr> Connect(
        Microsoft.Z3.Context context, Map<TrackedName, Microsoft.Z3.RealExpr> variables, TrackedName rule, Entity left, Entity right,
        Func<Microsoft.Z3.BoolExpr, Microsoft.Z3.BoolExpr, Microsoft.Z3.BoolExpr> connect) =>
        from l in Lower(context, variables, SymbolicExpr.Of(left), rule)
        from r in Lower(context, variables, SymbolicExpr.Of(right), rule)
        select connect(l, r);

    static Fin<Microsoft.Z3.ArithExpr> Arith(Microsoft.Z3.Context context, Map<TrackedName, Microsoft.Z3.RealExpr> variables, Entity node) =>
        node switch {
            Entity.Number.Rational rational => Fin.Succ((Microsoft.Z3.ArithExpr)context.MkReal(rational.Stringize())),
            Entity.Number.Real real => Fin.Succ((Microsoft.Z3.ArithExpr)context.MkReal(real.Stringize())),
            Entity.Variable variable => (TrackedName.TryCreate(variable.Name, out TrackedName symbol) ? variables.Find(symbol) : None)
                .Map(static bound => (Microsoft.Z3.ArithExpr)bound)
                .ToFin(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Required(ComputeSubject.Input))),
            Entity.Sumf(Entity left, Entity right) => Combine(context, variables, left, right, static (c, l, r) => (Microsoft.Z3.ArithExpr)c.MkAdd(l, r)),
            Entity.Minusf(Entity left, Entity right) => Combine(context, variables, left, right, static (c, l, r) => (Microsoft.Z3.ArithExpr)c.MkSub(l, r)),
            Entity.Mulf(Entity left, Entity right) => Combine(context, variables, left, right, static (c, l, r) => (Microsoft.Z3.ArithExpr)c.MkMul(l, r)),
            Entity.Divf(Entity left, Entity right) => Combine(context, variables, left, right, static (c, l, r) => c.MkDiv(l, r)),
            Entity.Powf(Entity left, Entity right) => Combine(context, variables, left, right, static (c, l, r) => (Microsoft.Z3.ArithExpr)c.MkPower(l, r)),
            Entity.Absf(Entity argument) => Arith(context, variables, argument).Map(value => Rectified(context, value)),
            Entity.Signumf(Entity argument) => Arith(context, variables, argument).Map(value => Sign(context, value)),
            Entity node => Fin.Fail<Microsoft.Z3.ArithExpr>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Type(node.GetType())))),
        };

    static Fin<Microsoft.Z3.ArithExpr> Combine(
        Microsoft.Z3.Context context, Map<TrackedName, Microsoft.Z3.RealExpr> variables, Entity left, Entity right,
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

- Owner: `NodeClassSelector` the typed selector over `Element/graph/element#ELEMENT_GRAPH` node classes — the classification row and `ObjectKind` predicate naming the member class, and the binding roster mapping each rule symbol onto the Element-declared property row its value reads from; `RuleSatisfaction.Ground` the derivation folding one rule TEMPLATE and one selector into the grounded rule the exact engine asserts.
- Entry: `public static Fin<ComplianceRule> Ground(ElementGraph graph, ComplianceRule template, NodeClassSelector selector)` — the template carries an EMPTY grounding roster and a selector-covered free-variable set, and the fold returns it carrying one `RuleGrounding` per matching member whose `Element` is the node id text admitted as a `TrackedName`. A caller-supplied grounding roster stays the manual lane and never enters this fold.
- Auto: `Ground` walks `ElementGraph.ObjectNodes`, admits each member through `NodeClassSelector.Admits`, bakes it through the memoized `Bake` fold so the type→occurrence inheritance the contract owns applies once, and binds every declared symbol off the baked property bags; a member missing one symbol — or carrying a node id the tracking grammar refuses — yields NO grounding, so the population and grounded counts diverge and `[04]` names the gap.
- Boundary: the selector composes the DECLARING package's own row statics — `StructuralRows` for the cross-package structural vocabulary, the owning package's `PropertyCategory` roster otherwise — because a call-site `PropertyName.Create` forks the spelling the declaring package already froze and the fork surfaces only as a rule that silently grounds nothing. Numeric admission is the three scalar `PropertyValue` arms alone: a measured row reads its SI magnitude, a number and an integer read their own value, and every other value case is NOT a rule binding — a text or enumerated row coerced to a double is the deleted form. The assessment spine stays the CALLER'S: this fold derives a rule, never a verdict, and never mints an `AssessmentResult`. Selector validity is a CONSTRUCTION refusal, so no caller mints a blank-classification selector and no fold re-tests one.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[ComplexValueObject]
public sealed partial class NodeClassSelector {
    public Seq<Classification> Branch { get; }
    public ObjectKind Kind { get; }
    public Map<TrackedName, PropertyName> Bindings { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref Seq<Classification> branch,
        ref ObjectKind kind, ref Map<TrackedName, PropertyName> bindings) =>
        validationError = !branch.IsEmpty && !bindings.IsEmpty
            ? null
            : new ValidationError("NodeClassSelector requires a resolved classification branch and at least one binding row.");

    public Predicate<ElementLeaf> Selection =>
        new Predicate<ElementLeaf>.All(Seq<Predicate<ElementLeaf>>(
            new Predicate<ElementLeaf>.Leaf(new ElementLeaf.ByKind(Kind)),
            new Predicate<ElementLeaf>.Leaf(new ElementLeaf.ByClassification(Branch))));

    public bool Admits(Node.Object node) =>
        Selection.Holds(
            leaf => leaf switch {
                ElementLeaf.ByKind kind => MatchVerdict.Of(node.Kind == kind.Kind),
                ElementLeaf.ByClassification cls => MatchVerdict.Of(cls.Branch.Contains(node.Classification)),
                _ => MatchVerdict.Fault(new ComputeFault.Violation(
                    ComputeArea.Solver,
                    new ComputeViolation.Contract(
                        ComputeContract.Valid,
                        new ContractEvidence.Type(leaf.GetType())))),
            },
            static _ => MatchVerdict.Fault(new ComputeFault.Violation(
                ComputeArea.Solver,
                new ComputeViolation.Unsupported(ComputeCapability.SelectorClosure))).Holds;

    public Option<Map<TrackedName, double>> Bind(Element member) =>
        Bindings.Fold(Some(Map<TrackedName, double>()), (acc, symbol, row) =>
            acc.Bind(bound => member.Properties
                .Choose(bag => bag.Find(row))
                .Head
                .Bind(Numeric)
                .Map(value => bound.Add(symbol, value))));

    static Option<double> Numeric(PropertyValue value) => value switch {
        PropertyValue.Measure measure => Some(measure.Value.Si),
        PropertyValue.Number number when double.IsFinite(number.Value) => Some(number.Value),
        PropertyValue.Integer integer => Some((double)integer.Value),
        _ => Option<double>.None,
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static partial class RuleSatisfaction {

    public static Fin<ComplianceRule> Ground(ElementGraph graph, ComplianceRule template, NodeClassSelector selector) =>
        !template.Grounding.IsEmpty
            ? Fin.Fail<ComplianceRule>(new ComputeFault.Violation(ComputeArea.Solver, new ComputeViolation.Contract(ComputeContract.Valid, new ContractEvidence.Key(template.Name.Value))))
            : Population(graph, selector)
                .TraverseM(node => graph.Bake(node.Id, GroundKey).Map(member => (Id: node.Id, Member: member))).As()
                .Map(members => template with {
                    Grounding = members.Choose(pair =>
                        from element in SymbolName(pair.Id.ToValue())
                        from bindings in selector.Bind(pair.Member)
                        select new RuleGrounding(element, bindings)),
                });

    internal static Seq<Node.Object> Population(ElementGraph graph, NodeClassSelector selector) =>
        graph.ObjectNodes.Filter(selector.Admits);
}
```

## [04]-[RULE_COVERAGE_PROOF]

- Owner: `CoverageFact` the population-versus-grounded evidence carrier with the roster of members the selector admitted but could not bind; `RuleSatisfaction.Coverage` the proof fold over the same population the derivation walks.
- Entry: `public static CoverageFact Coverage(ElementGraph graph, NodeClassSelector selector, ComplianceRule rule)` — the rule supplies the grounding roster actually asserted, and the fact reports how much of the admitted class it covers; the read is total over an admitted graph, so no result wraps it.
- Boundary: `Check` consumes the fact — an incomplete fact refuses under `SatisfyRight.RequireCoverage` and rides the assessment as evidence otherwise — because a SATISFIABLE verdict over a partial population reads identically to one over the whole class, and the difference is exactly what a compliance consumer is asking about. The fact lands beside the verdict facts on the SAME `AssessmentResult`, never as a second result. The bound and unbound halves come out of ONE walk, so the three counts can never disagree about the population they measured.

```csharp
// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct CoverageFact(int Population, int Grounded, Seq<string> Missing) {
    public bool Complete => Grounded == Population && Missing.IsEmpty;

    public Seq<AssessmentFact> Facts =>
        Seq(AssessmentFact.Ratio("coverage-population", Population), AssessmentFact.Ratio("coverage-grounded", Grounded))
        + Missing.Map(static element => AssessmentFact.Text("coverage-missing", element));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static partial class RuleSatisfaction {
    public static CoverageFact Coverage(ElementGraph graph, NodeClassSelector selector, ComplianceRule rule) {
        Set<TrackedName> grounded = toSet(rule.Grounding.Map(static ground => ground.Element));
        (Seq<Node.Object> bound, Seq<Node.Object> missing) =
            Population(graph, selector).Partition(node => SymbolName(node.Id.ToValue()).Exists(grounded.Contains));
        return new CoverageFact(bound.Count + missing.Count, bound.Count, missing.Map(static node => node.Id.ToValue()));
    }
}
```
