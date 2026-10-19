import type { Document } from 'adobe:indesign';
import { single } from '@rasm/search';
import { Array, Effect, HashMap, Option, Predicate, Schema, Struct } from 'effect';
import { Dom } from './dom.ts';
import { active, Reference, read } from './references.ts';

// --- [MODELS] --------------------------------------------------------------------------

class Issue extends Schema.Class<Issue, { readonly brand: unique symbol }>('Issue')({
    rule: Schema.String.annotate({ description: 'Preflight rule the issue belongs to, with the count of its issues in this report' }),
    page: Schema.String,
    text: Schema.String.annotate({ description: 'Kind of object the rule found, as the Preflight panel lists it' }),
    details: Schema.Record(Schema.String, Schema.String).annotate({ description: 'Native label and description pairs: Problem, Fix, and rule-specific values' }),
}) {}
class Preflight extends Schema.Class<Preflight, { readonly brand: unique symbol }>('Preflight')(
    { profile: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Preflight profile name, in any case' })) },
    { description: "Preflight results of the active document under a profile, the document's working profile when absent" },
) {
    static readonly Result = Schema.Struct({ document: Reference, profile: Schema.String, profiles: Schema.Array(Schema.String).annotate({ description: 'Preflight profiles the application holds' }), issues: Schema.Array(Issue) });
    static readonly Summary = Schema.Array(Reference.mapFields(Struct.assign({ errors: Schema.Int, issues: Schema.Array(Issue).annotate({ description: 'Issues this mutation introduced' }) }))).annotate({ description: 'Preflight of every document the mutation touched, under its working profile' });
    static readonly of = Effect.fn('of')(function* (document: Document, named: Option.Option<string>) {
        const { app } = yield* Dom;
        const selected = Option.getOrElse(named, () => document.preflightOptions.preflightWorkingProfile);
        const profiles = app.preflightProfiles.everyItem();
        const profile = Predicate.isString(selected) ? (yield* single('preflight profile', selected)(Array.zip(profiles.getElements(), profiles.name), { names: ([, name]) => [name], label: ([, name]) => name }))[0] : selected;
        const process = yield* Effect.acquireRelease(
            Effect.sync(() => app.preflightProcesses.add(document, profile)),
            (added) => Effect.sync(() => added.remove()),
        );
        yield* Effect.sync(() => process.waitForProcess());
        const [, profileName, rows] = process.aggregatedResults;
        const rules = Array.drop(
            Array.scan(rows, '', (rule, [, name, , , details]) => (Array.isReadonlyArrayEmpty(details) ? name : rule)),
            1,
        );
        const issues = Array.zipWith(rows, rules, ([, text, page, , details], rule) => (Array.isReadonlyArrayEmpty(details) ? [] : [{ rule, page, text, details: Object.fromEntries(details) }])).flat();
        return yield* read(Preflight.Result, () => ({ document: Reference.of(document), profile: profileName, profiles: profiles.name, issues }));
    }, Effect.scoped);
    static readonly baseline = Effect.fn('baseline')(function* () {
        const { app } = yield* Dom;
        return HashMap.fromIterable(yield* Effect.forEach(app.documents.everyItem().getElements(), (document) => Effect.map(Preflight.of(document, Option.none()), ({ issues }) => [document.id, issues] as const)));
    });
    static readonly summarized = (baseline: HashMap.HashMap<number, readonly Issue[]>, touched: readonly Document[]) =>
        Effect.forEach(
            touched,
            Effect.fnUntraced(function* (document) {
                const { issues } = yield* Preflight.of(document, Option.none());
                const counted = (counts: HashMap.HashMap<Omit<Issue, 'rule'>, number>, issue: Issue, step: number) => HashMap.modifyAt(counts, Struct.omit(issue, ['rule']), (count) => Option.some(Option.getOrElse(count, () => 0) + step));
                const held = Option.getOrElse(HashMap.get(baseline, document.id), Array.empty).reduce((counts, issue) => counted(counts, issue, 1), HashMap.empty<Omit<Issue, 'rule'>, number>());
                const [, introduced] = Array.mapAccum(issues, held, (counts, issue) => (Option.exists(HashMap.get(counts, Struct.omit(issue, ['rule'])), (count) => count > 0) ? [counted(counts, issue, -1), Option.none()] : [counts, Option.some(issue)]));
                return { ...Reference.of(document), errors: issues.length, issues: Array.getSomes(introduced) };
            }),
        );

    run() {
        const { profile } = this;
        return Effect.flatMap(active, (document) => Preflight.of(document, profile));
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Preflight };
