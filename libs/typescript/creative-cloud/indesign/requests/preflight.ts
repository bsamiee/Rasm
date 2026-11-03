import type { Document, PreflightProcess } from 'adobe:indesign';
import { HostError, Reference } from '@rasm/automation/requests';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Effect, flow, Option, Predicate, Record, Result, Schema, SchemaGetter, String } from 'effect';
import { Model } from '#/dom/model';
import { reference, Specifier } from '#/dom/specifier';

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
}).annotate({ description: 'Category or rule with issue count, or issue object with problem and fix' });
const Flat = Schema.Tuple([Schema.Int, Schema.String, Schema.String, Schema.String, Schema.Array(Schema.Tuple([Schema.String, Schema.String]))]);
class PreflightProcessView extends Schema.TaggedClass<PreflightProcessView, { readonly brand: unique symbol }>()('PreflightProcess', {
    ...Reference.fields,
    targetObject: Reference,
    appliedProfile: Schema.OptionFromOptionalKey(Reference.annotate({ description: 'Absent for document-embedded profiles' })),
    aggregatedResults: Schema.Result(Schema.Array(Row), Schema.Defect()).annotate({ description: 'Rows of finished process, empty without issues, refusal while checking' }),
}) {
    static readonly matches = (model: typeof Model.Service) => model.owning<PreflightProcess>('aggregatedResults');
    static readonly of = (process: PreflightProcess) =>
        Effect.try({
            try: () =>
                PreflightProcessView.make({
                    ...reference(process),
                    targetObject: reference(process.targetObject),
                    appliedProfile: Option.map(Option.fromUndefinedOr(process.appliedProfile), reference),
                    aggregatedResults: aggregated(process),
                }),
            catch: HostError.refused(process.toSpecifier()),
        });
}
class Preflight extends Schema.Class<Preflight, { readonly brand: unique symbol }>('Preflight')({
    document: Schema.OptionFromOptionalKey(Specifier.Path.annotate({ description: 'Open document, every document with live preflight when absent' })),
    profile: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Profile name in any case, panel profile when absent' })),
}) {
    static readonly Result = Schema.Array(Schema.Struct({ targetObject: Reference, appliedProfile: Reference, aggregatedResults: Schema.Array(Row) }));
    static readonly handle = Effect.fn('Preflight.handle')(function* ({ document, profile }: Preflight) {
        const model = yield* Model;
        const open = model.app.documents.everyItem().getElements();
        const targets = yield* Option.match(document, {
            onNone: () => Effect.succeed(open.filter((each) => !each.preflightOptions.preflightOff)),
            onSome: (specifier) =>
                Effect.map(
                    Effect.filterOrFail(Specifier.resolve(specifier), model.document, () => Unmatched.make({ subject: 'open document', query: Option.some(specifier.path), candidates: Array.map(open, (each) => each.toSpecifier()) })),
                    Array.of,
                ),
        });
        return yield* Effect.forEach(targets, (target) => checked(target, profile));
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const nested: (rows: readonly (typeof Flat.Type)[]) => readonly Row[] = Array.match({
    onEmpty: Array.empty,
    onNonEmpty: ([[depth, label, page, , details], ...rest]) => {
        const [under, after] = Array.span(rest, ([below]) => below > depth);
        return [{ label, ...Record.getSomes({ page: Option.liftPredicate(page, String.isNonEmpty) }), details: Record.fromEntries(details), rows: nested(under) }, ...nested(after)];
    },
});
const Aggregated = Schema.Tuple([Schema.String, Schema.String, Schema.Array(Flat)]).pipe(Schema.decodeTo(Schema.Array(Row), { decode: SchemaGetter.transform(([, , rows]) => nested(rows)), encode: SchemaGetter.forbiddenEncoding }));
const aggregated = (process: PreflightProcess) => Result.try(() => process.aggregatedResults).pipe(Result.flatMap(Schema.decodeUnknownResult(Aggregated)));
const checked = Effect.fn('checked')(function* (document: Document, named: Option.Option<string>) {
    const { app } = yield* Model;
    const { preflightOff, preflightWorkingProfile: working } = document.preflightOptions;
    const profiles = [...app.preflightProfiles.everyItem().getElements(), ...document.preflightProfiles.everyItem().getElements()];
    const byName = (name: string) => Unmatched.single(profiles, 'preflight profile', Option.some(name), (each) => [each.name]);
    const live = preflightOff ? Option.none() : Option.some(document.activeProcess);
    const applied = Option.flatMap(live, (process) => Option.fromUndefinedOr(process.appliedProfile));
    const chosen = yield* Option.match(named, { onSome: byName, onNone: () => Option.match(applied, { onSome: Effect.succeed, onNone: () => (Predicate.isString(working) ? byName(working) : Effect.succeed(working)) }) });
    const refused = HostError.refused(`preflight of ${document.toSpecifier()} under ${chosen.name}`);
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
        targetObject: reference(document),
        appliedProfile: reference(chosen),
        aggregatedResults: yield* Option.match(finished, { onSome: Effect.succeed, onNone: () => waited }),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Preflight, PreflightProcessView };
