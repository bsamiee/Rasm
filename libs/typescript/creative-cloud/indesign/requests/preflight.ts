import type { Document, PreflightProcess } from 'adobe:indesign';
import { HostError } from '@rasm/automation/requests';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Effect, flow, Option, Predicate, Record, Result, Schema, SchemaGetter, String } from 'effect';
import { Preflight } from '#/contract';
import { Model } from '#/dom/model';

// --- [MODELS] --------------------------------------------------------------------------

const Flat = Schema.Tuple([Schema.Int, Schema.String, Schema.String, Schema.String, Schema.Array(Schema.Tuple([Schema.String, Schema.String]))]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const nested: (rows: readonly (typeof Flat.Type)[]) => readonly (typeof Preflight.Issue.Type)[] = Array.match({
    onEmpty: Array.empty,
    onNonEmpty: ([[depth, label, page, , details], ...rest]) => {
        const [under, after] = Array.span(rest, ([below]) => below > depth);
        return [{ label, ...Record.getSomes({ page: Option.liftPredicate(page, String.isNonEmpty) }), details: Record.fromEntries(details), rows: nested(under) }, ...nested(after)];
    },
});
const Aggregated = Schema.Tuple([Schema.String, Schema.String, Schema.Array(Flat)]).pipe(Schema.decodeTo(Schema.Array(Preflight.Issue), { decode: SchemaGetter.transform(([, , rows]) => nested(rows)), encode: SchemaGetter.forbiddenEncoding }));
const aggregated = (process: PreflightProcess) => Result.try(() => process.aggregatedResults).pipe(Result.flatMap(Schema.decodeUnknownResult(Aggregated)));
const preflightView = Model.view(
    (model) => model.preflightProcess,
    (process: PreflightProcess) =>
        Model.read(Preflight.View, process.toSpecifier(), () => ({
            _tag: 'PreflightProcess',
            ...Model.referenceUnsafe(process),
            targetObject: process.targetObject,
            appliedProfile: Option.fromNullishOr(process.appliedProfile),
            aggregatedResults: aggregated(process),
        })),
);
const checked = Effect.fn('checked')(function* (document: Document, named: Option.Option<string>) {
    const { app } = yield* Model;
    const { preflightOff, preflightWorkingProfile: working } = document.preflightOptions;
    const profiles = [...app.preflightProfiles.everyItem().getElements(), ...document.preflightProfiles.everyItem().getElements()];
    const profile = (name: string) => Unmatched.single(profiles, 'preflight profile', Option.some(name), (each) => [each.name]);
    const live = preflightOff ? Option.none() : Option.some(document.activeProcess);
    const applied = Option.flatMap(live, (process) => Option.fromNullishOr(process.appliedProfile));
    const chosen = yield* Option.match(named, { onSome: profile, onNone: () => Option.match(applied, { onSome: Effect.succeed, onNone: () => (Predicate.isString(working) ? profile(working) : Effect.succeed(working)) }) });
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
    return yield* Effect.all({
        targetObject: Model.reference(document),
        appliedProfile: Model.reference(chosen),
        aggregatedResults: Option.match(finished, { onSome: Effect.succeed, onNone: () => waited }),
    });
});
const preflight = Effect.fn('preflight')(function* ({ document, profile }: Preflight) {
    const model = yield* Model;
    const open = model.app.documents.everyItem().getElements();
    const targets = yield* Option.match(document, {
        onNone: () => Effect.succeed(open.filter((each) => !each.preflightOptions.preflightOff)),
        onSome: (specifier) =>
            Effect.map(
                Effect.filterOrElse(Model.resolve(specifier), model.document, () => Effect.flatMap(specifier.path, (written) => Unmatched.make({ subject: 'open document', query: Option.some(written), candidates: Array.map(open, (each) => each.toSpecifier()) }))),
                Array.of,
            ),
    });
    return yield* Effect.forEach(targets, (target) => checked(target, profile));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { preflight, preflightView };
