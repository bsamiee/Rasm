import type { Document } from 'adobe:indesign';
import { Unmatched } from '@rasm/search';
import { Array, Context, Effect, HashMap, Layer, Option, Predicate, Ref, Schema, Struct } from 'effect';
import { Host } from './host.ts';
import { Classes } from './objects/classes.ts';
import { Reference, read } from './objects/specifiers.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Issue = Schema.Struct({
    rule: Schema.String.annotate({ description: 'Preflight rule the issue belongs to' }),
    page: Schema.String,
    text: Schema.String.annotate({ description: 'What the rule found, as the Preflight panel lists it' }),
    details: Schema.Record(Schema.String, Schema.String).annotate({ description: 'Native label and description pairs: Problem, Fix, and rule-specific values' }),
}).annotate({ identifier: 'Issue' });
class Preflight extends Schema.TaggedClass<Preflight, { readonly brand: unique symbol }>()('Preflight', { document: Reference, profile: Schema.String, issues: Schema.Array(Issue) }, { description: "Preflight issues of a document under a profile, the document's working profile when absent" }) {
    static readonly Summary = Schema.Array(Reference.pipe(Schema.fieldsAssign({ errors: Schema.Int, issues: Schema.Array(Issue).annotate({ description: 'Issues this mutation introduced' }) }))).annotate({ description: 'Preflight of every document the mutation touched, under its working profile' });
    static readonly of = Effect.fn('of')(function* (document: Document, named: Option.Option<string>) {
        const [{ indesign }, { reference }] = yield* Effect.all([Host, Classes]);
        const { app } = indesign;
        const selected = Option.getOrElse(named, () => document.preflightOptions.preflightWorkingProfile);
        const profile = yield* Predicate.isString(selected)
            ? Effect.fromOption(
                  Array.findFirst(app.preflightProfiles.everyItem().getElements(), (each) => each.name === selected),
                  () => Unmatched.make({ subject: 'preflight profile', query: selected, candidates: app.preflightProfiles.everyItem().name }),
              )
            : Effect.succeed(selected);
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
        const issues = Array.zipWith(rows, rules, ([, , page, text, details], rule) => (Array.isReadonlyArrayEmpty(details) ? [] : [{ rule, page, text, details: Object.fromEntries(details) }])).flat();
        return yield* read(Preflight, () => ({ _tag: 'Preflight', document: reference(document), profile: profileName, issues }));
    }, Effect.scoped);
}

// --- [SERVICES] ------------------------------------------------------------------------

class Reports extends Context.Service<Reports>()('@rasm/indesign/preflight/Reports', {
    make: Effect.gen(function* () {
        const held = yield* Ref.make(HashMap.empty<number, readonly (typeof Issue.Type)[]>());
        const report = Effect.fnUntraced(function* (document: Document) {
            const { reference } = yield* Classes;
            const { issues } = yield* Preflight.of(document, Option.none());
            const seen = yield* Ref.getAndUpdate(held, HashMap.set(document.id, issues));
            const known = new Set(
                Array.map(
                    Option.getOrElse(HashMap.get(seen, document.id), (): readonly (typeof Issue.Type)[] => []),
                    Struct.get('text'),
                ),
            );
            const introduced = issues.filter(({ text }) => !known.has(text));
            return yield* read(Preflight.Summary.value, () => ({ ...reference(document), errors: issues.length, issues: introduced }));
        });
        return { summarized: (documents: readonly Document[]) => Effect.forEach(documents, report) };
    }),
}) {
    static readonly layer = Layer.effect(this, this.make);
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Preflight, Reports };
