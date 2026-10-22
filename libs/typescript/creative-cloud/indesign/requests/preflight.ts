import type { Document, PreflightProcess } from 'adobe:indesign';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Effect, flow, Option, Predicate, Record, Result, Schema, String } from 'effect';
import { resolve } from '#/dom/location';
import { Model } from '#/dom/model';
import { Reference, Specifier } from '#/dom/reference';

// --- [TYPES] ---------------------------------------------------------------------------

interface Row {
    readonly label: string;
    readonly page?: string;
    readonly details: Record.ReadonlyRecord<string, string>;
    readonly rows: readonly Row[];
}

// --- [MODELS] --------------------------------------------------------------------------

const Row: Schema.Codec<Row> = Schema.Struct({
    label: Schema.String,
    page: Schema.optionalKey(Schema.String.annotate({ description: 'Page name, PB on the pasteboard' })),
    details: Schema.Record(Schema.String, Schema.String),
    rows: Schema.Array(Schema.suspend((): Schema.Codec<Row> => Row)),
}).annotate({ description: 'Preflight panel row: a category or rule with its issue count, or the object an issue names with its problem and fix, above the rows under it' });
const Aggregated = Schema.Tuple([Schema.String, Schema.String, Schema.Array(Schema.Tuple([Schema.Int, Schema.String, Schema.String, Schema.String, Schema.Array(Schema.Tuple([Schema.String, Schema.String]))]))]);
class PreflightProcessView extends Schema.TaggedClass<PreflightProcessView, { readonly brand: unique symbol }>()('PreflightProcess', {
    ...Reference.fields,
    targetObject: Reference,
    appliedProfile: Schema.OptionFromOptionalKey(Reference.annotate({ description: 'Absent for a profile the document embeds' })),
    aggregatedResults: Schema.Result(Schema.Array(Row), Schema.Defect()).annotate({ description: "Rows the Preflight panel shows once the process finishes, empty when it found no issue, else InDesign's refusal while it checks" }),
}) {
    static readonly viewed = ({ owning }: Model['Service']) => flow(Option.liftPredicate(owning<PreflightProcess>('aggregatedResults')), Option.map(PreflightProcessView.of));
    static readonly of = (process: PreflightProcess) =>
        Effect.try({
            try: () =>
                PreflightProcessView.make({
                    ...Reference.of(process),
                    targetObject: Reference.of(process.targetObject),
                    appliedProfile: Option.map(Option.fromUndefinedOr(process.appliedProfile), Reference.of),
                    aggregatedResults: aggregated(process),
                }),
            catch: (cause) => new Cause.UnknownError(cause, process.toSpecifier()),
        });
}
class Preflight extends Schema.Class<Preflight, { readonly brand: unique symbol }>('Preflight')(
    {
        document: Schema.OptionFromOptionalKey(Specifier.annotate({ description: 'Specifier of an open document; every open document with preflight on when absent' })),
        profile: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Profile name of the application or the document, in any case; the profile the Preflight panel applies when absent' })),
    },
    {
        description: "Preflight rows of each open document as the Preflight panel shows them: InDesign's live check once it finished, else a process run under the profile and waited for, which InDesign finishes in the background and which restarts the live check of every open document",
    },
) {
    static readonly Result = Schema.Array(Schema.Struct({ targetObject: Reference, appliedProfile: Reference, aggregatedResults: Schema.Array(Row) }));

    run() {
        const { document, profile } = this;
        return Effect.gen(function* () {
            const model = yield* Model;
            const open = model.app.documents.everyItem().getElements();
            const targets = yield* Option.match(document, {
                onNone: () => Effect.succeed(open.filter((each) => !each.preflightOptions.preflightOff)),
                onSome: (specifier) =>
                    Effect.map(
                        Effect.filterOrFail(resolve(specifier), model.document, () => Unmatched.make({ subject: 'open document', query: Option.some(specifier), candidates: Array.map(open, (each) => each.toSpecifier()) })),
                        Array.of,
                    ),
            });
            return yield* Effect.forEach(targets, (target) => checked(target, profile));
        });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const nested: (rows: (typeof Aggregated.Type)[2]) => readonly Row[] = Array.match({
    onEmpty: Array.empty,
    onNonEmpty: ([[depth, label, page, , details], ...rest]) => {
        const [under, after] = Array.span(rest, ([below]) => below > depth);
        return [{ label, ...Record.getSomes({ page: Option.liftPredicate(page, String.isNonEmpty) }), details: Record.fromEntries(details), rows: nested(under) }, ...nested(after)];
    },
});
const aggregated = (process: PreflightProcess) =>
    Result.try(() => process.aggregatedResults).pipe(
        Result.flatMap(Schema.decodeUnknownResult(Aggregated)),
        Result.map(([, , rows]) => nested(rows)),
    );
const checked = Effect.fn('checked')(function* (document: Document, named: Option.Option<string>) {
    const { app } = yield* Model;
    const { preflightOff, preflightWorkingProfile: working } = document.preflightOptions;
    const profiles = [...app.preflightProfiles.everyItem().getElements(), ...document.preflightProfiles.everyItem().getElements()];
    const byName = (name: string) => Unmatched.single('preflight profile', Option.some(name))(profiles, (each) => [each.name]);
    const live = Option.map(
        Option.liftPredicate(document, () => !preflightOff),
        (open) => open.activeProcess,
    );
    const applied = Option.flatMap(live, (process) => Option.fromUndefinedOr(process.appliedProfile));
    const chosen = yield* Option.match(named, { onSome: byName, onNone: () => Option.match(applied, { onSome: Effect.succeed, onNone: () => (Predicate.isString(working) ? byName(working) : Effect.succeed(working)) }) });
    const refused = (cause: unknown) => new Cause.UnknownError(cause, `preflight of ${document.toSpecifier()} under ${chosen.name}`);
    const finished = Option.flatMap(
        Option.filter(live, () => Option.isNone(named)),
        flow(aggregated, Result.getSuccess),
    );
    const waited = Effect.acquireUseRelease(
        Effect.try({ try: () => app.preflightProcesses.add(document, chosen), catch: refused }),
        (process) =>
            Effect.andThen(
                Effect.try({ try: () => process.waitForProcess(), catch: refused }),
                Effect.suspend(() => Effect.fromResult(Result.mapError(aggregated(process), refused))),
            ),
        (process) => Effect.sync(() => process.remove()),
    );
    return {
        targetObject: Reference.of(document),
        appliedProfile: Reference.of(chosen),
        aggregatedResults: yield* Option.match(finished, { onSome: Effect.succeed, onNone: () => waited }),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Preflight, PreflightProcessView };
