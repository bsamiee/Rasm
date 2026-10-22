import type { Document, PreflightProcess, PreflightProfile } from 'adobe:indesign';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Effect, flow, Option, Predicate, Result, Schema, Struct } from 'effect';
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
                    Schema.String.annotate({ description: 'pageNumber: page name of an issue' }),
                    Schema.String.annotate({ description: 'errorInfo' }),
                    Schema.Array(Schema.Tuple([Schema.String, Schema.String])).annotate({ description: 'errorDetail: label and description pairs' }),
                ]),
            ).annotate({ description: 'results' }),
        ]),
        Schema.Defect(),
    ).annotate({ description: "Results as InDesign states them, or InDesign's refusal while live preflight composes changes" }),
}) {
    static readonly viewed = ({ owning }: Model['Service']) => flow(Option.liftPredicate(owning<PreflightProcess>('aggregatedResults')), Option.map(ProcessView.of));
    static readonly of = (process: PreflightProcess) =>
        Effect.map(Effect.result(Effect.try({ try: () => process.aggregatedResults, catch: (cause) => new Cause.UnknownError(cause, `aggregatedResults of ${process.toSpecifier()}`) })), (aggregatedResults) =>
            ProcessView.make({ ...Reference.of(process), targetObject: Reference.of(process.targetObject), appliedProfile: Option.map(Option.fromUndefinedOr(process.appliedProfile), Reference.of), processResults: process.processResults, aggregatedResults }),
        );
    static readonly live = (document: Document) => Effect.flatMap(Effect.try({ try: () => document.activeProcess, catch: (cause) => new Cause.UnknownError(cause, `activeProcess of ${document.toSpecifier()}`) }), ProcessView.of);
    static readonly under = Effect.fn('ProcessView.under')(function* (document: Document, named: Option.Option<string>) {
        const { app } = yield* Model;
        const { preflightOff, preflightWorkingProfile: working } = document.preflightOptions;
        const profiles = [...app.preflightProfiles.everyItem().getElements(), ...document.preflightProfiles.everyItem().getElements()];
        const byName = (name: string) => Unmatched.single('preflight profile', Option.some(name))(profiles, flow(Struct.get('name'), Array.of));
        const held = Option.orElse(
            Option.flatMap(
                Option.liftPredicate(document, () => !preflightOff),
                (open) => Option.fromUndefinedOr(open.activeProcess.appliedProfile),
            ),
            () => Option.liftPredicate(working, (each): each is PreflightProfile => !Predicate.isString(each)),
        );
        const chosen = yield* Option.match(named, { onSome: byName, onNone: () => Option.match(held, { onSome: Effect.succeed, onNone: () => byName(Predicate.isString(working) ? working : working.name) }) });
        const refused = (cause: unknown) => new Cause.UnknownError(cause, `preflight of ${document.toSpecifier()} under ${chosen.name}`);
        const process = yield* Effect.acquireRelease(Effect.try({ try: () => app.preflightProcesses.add(document, chosen), catch: refused }), (added) => Effect.sync(() => added.remove()));
        yield* Effect.try({ try: () => process.waitForProcess(), catch: refused });
        return yield* ProcessView.of(process);
    }, Effect.scoped);

    get issues() {
        return Result.match(this.aggregatedResults, {
            onFailure: () => [],
            onSuccess: ([, , results]) => results.filter(([depth], index) => Option.match(Array.get(results, index + 1), { onNone: () => true, onSome: ([next]) => next <= depth })),
        });
    }
}
class Preflight extends Schema.Class<Preflight, { readonly brand: unique symbol }>('Preflight')(
    {
        document: Schema.OptionFromOptionalKey(Specifier.annotate({ description: 'Specifier of an open document; every open document with live preflight on when absent' })),
        waitForProcess: Schema.OptionFromOptionalKey(Schema.Literal(true).annotate({ description: "Runs a process under the document's profile and waits for InDesign to finish it, current when the read returns" })),
        profile: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Profile name of the application or the document, in any case, to run a process under' })),
    },
    {
        description: "Preflight processes of open documents: each document's live process, which InDesign completes on idle after a change, or a run that ignores the document's and the application's preflight options",
    },
) {
    static readonly Process = ProcessView;
    static readonly Result = Schema.Array(ProcessView);

    run() {
        const { document, waitForProcess, profile } = this;
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
            return yield* Effect.forEach(
                targets,
                Option.match(
                    Option.orElse(Option.map(profile, Option.some), () => Option.as(waitForProcess, Option.none<string>())),
                    {
                        onNone: () => ProcessView.live,
                        onSome: (named) => (target: Document) => ProcessView.under(target, named),
                    },
                ),
            );
        });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Preflight };
