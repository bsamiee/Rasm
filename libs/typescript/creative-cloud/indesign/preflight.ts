import type { Document } from 'adobe:indesign';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Effect, HashMap, Match, Option, Predicate, Schema, Struct } from 'effect';
import { Model } from './model.ts';
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
    static readonly introduced = Match.type<unknown>().pipe(
        Match.when(Schema.is(Schema.Struct({ preflight: Preflight.Summary })), ({ preflight }) => preflight),
        Match.when(Schema.is(Schema.Struct({ preflight: Schema.Option(Preflight.Summary) })), ({ preflight }) => Option.getOrElse(preflight, Array.empty)),
        Match.orElse(() => []),
    );
    static readonly of = Effect.fn('Preflight.of')(function* (document: Document, named: Option.Option<string>) {
        const { app } = yield* Model;
        const selected = Option.getOrElse(named, () => document.preflightOptions.preflightWorkingProfile);
        const profiles = app.preflightProfiles.everyItem();
        const profile = Predicate.isString(selected) ? (yield* Unmatched.single('preflight profile', Option.some(selected))(Array.zip(profiles.getElements(), profiles.name), ([, name]) => [name]))[0] : selected;
        const refused = (cause: unknown) => new Cause.UnknownError(cause, `preflight of ${document.toSpecifier()}`);
        const process = yield* Effect.acquireRelease(Effect.try({ try: () => app.preflightProcesses.add(document, profile), catch: refused }), (added) => Effect.sync(() => added.remove()));
        yield* Effect.try({ try: () => process.waitForProcess(), catch: refused });
        const [, profileName, rows] = process.aggregatedResults;
        const rules = Array.drop(
            Array.scan(rows, '', (rule, [, name, , , details]) => (Array.isReadonlyArrayEmpty(details) ? name : rule)),
            1,
        );
        const issues = Array.zipWith(rows, rules, ([, text, page, , details], rule) => (Array.isReadonlyArrayEmpty(details) ? [] : [{ rule, page, text, details: Object.fromEntries(details) }])).flat();
        return yield* read(Preflight.Result, document.toSpecifier(), () => ({ document: Reference.of(document), profile: profileName, profiles: profiles.name, issues }));
    }, Effect.scoped);
    static readonly baseline = (documents: readonly Document[]) =>
        Effect.map(
            Effect.forEach(
                Array.dedupeWith(documents, (a, b) => a.id === b.id),
                (document) => Effect.map(Preflight.of(document, Option.none()), ({ issues }) => [document.toSpecifier(), issues] as const),
            ),
            HashMap.fromIterable,
        );
    static readonly summarized = (baseline: HashMap.HashMap<string, readonly Issue[]>, touched: readonly Document[]) =>
        Effect.forEach(
            touched,
            Effect.fnUntraced(function* (document) {
                const { issues } = yield* Preflight.of(document, Option.none());
                const counted = (counts: HashMap.HashMap<Omit<Issue, 'rule'>, number>, issue: Issue, step: number) => HashMap.modifyAt(counts, Struct.omit(issue, ['rule']), (count) => Option.some(Option.getOrElse(count, () => 0) + step));
                const held = Option.getOrElse(HashMap.get(baseline, document.toSpecifier()), Array.empty).reduce((counts, issue) => counted(counts, issue, 1), HashMap.empty<Omit<Issue, 'rule'>, number>());
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
