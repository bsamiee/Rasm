import type { Document, PreflightProcess } from 'adobe:indesign';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Effect, flow, Option, Result, Schema } from 'effect';
import { Model } from './model.ts';
import { Reference, resolve, Specifier } from './references.ts';

// --- [MODELS] --------------------------------------------------------------------------

class ProcessView extends Schema.TaggedClass<ProcessView, { readonly brand: unique symbol }>()('PreflightProcess', {
    ...Reference.fields,
    targetObject: Reference,
    appliedProfile: Schema.OptionFromOptionalKey(Reference.annotate({ description: 'Absent for a profile the document embeds' })),
    processResults: Schema.String,
    aggregatedResults: Schema.Result(
        Schema.Tuple([
            Schema.String.annotate({ description: 'documentName' }),
            Schema.String.annotate({ description: 'profileName' }),
            Schema.Array(
                Schema.Tuple([
                    Schema.Int.annotate({ description: 'parentNodeID, the row depth: 1 category, 2 rule, 3 issue, each row under the nearest shallower row before it' }),
                    Schema.String.annotate({ description: 'errorName: category or rule with its issue count, or the object kind or link an issue names' }),
                    Schema.String.annotate({ description: 'pageNumber: page name of an issue, PB on the pasteboard' }),
                    Schema.String.annotate({ description: 'errorInfo' }),
                    Schema.Array(Schema.Tuple([Schema.String, Schema.String])).annotate({ description: 'errorDetail: label and description pairs' }),
                ]),
            ).annotate({ description: 'results, empty exactly when the process found no issue' }),
        ]),
        Schema.Defect(),
    ).annotate({ description: "Rows once the process completes or has nothing to do, else InDesign's refusal while it runs" }),
}) {
    static readonly viewed = ({ owning }: Model['Service']) => flow(Option.liftPredicate(owning<PreflightProcess>('aggregatedResults')), Option.map(ProcessView.of));
    static readonly of = (process: PreflightProcess) =>
        Effect.try({
            try: () => ProcessView.make({ ...Reference.of(process), targetObject: Reference.of(process.targetObject), appliedProfile: Option.map(Option.fromUndefinedOr(process.appliedProfile), Reference.of), processResults: process.processResults, aggregatedResults: Result.try(() => process.aggregatedResults) }),
            catch: (cause) => new Cause.UnknownError(cause, process.toSpecifier()),
        });
    static readonly live = (document: Document) => Effect.flatMap(Effect.try({ try: () => document.activeProcess, catch: (cause) => new Cause.UnknownError(cause, `activeProcess of ${document.toSpecifier()}`) }), ProcessView.of);
}
class Preflight extends Schema.Class<Preflight, { readonly brand: unique symbol }>('Preflight')(
    { document: Schema.OptionFromOptionalKey(Specifier.annotate({ description: 'Specifier of an open document; every open document with live preflight on when absent' })) },
    { description: 'Live preflight process of each open document, which the Preflight panel shows and InDesign completes on idle after a change' },
) {
    static readonly Process = ProcessView;
    static readonly Result = Schema.Array(ProcessView);

    run() {
        const { document } = this;
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
            return yield* Effect.forEach(targets, ProcessView.live);
        });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Preflight };
