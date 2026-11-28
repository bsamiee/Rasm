import type { Args, AskOptions, EngineInterface, Frozen, Next, ToolCallResult } from 'claude-code';
import { Array, Cause, Context, Data, Effect, Layer, Match, MutableRef, Option, PlatformError, Record, Result, Schema, Semaphore, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface Choice<L extends string> {
    readonly label: L;
    readonly description?: string;
    readonly preview?: string;
}
type Question<L extends string = string> = Pick<AskOptions, 'header' | 'multiSelect'> & {
    readonly question: string;
    readonly options: readonly [Choice<L>, Choice<L>] | readonly [Choice<L>, Choice<L>, Choice<L>] | readonly [Choice<L>, Choice<L>, Choice<L>, Choice<L>];
};
type Answer<L = string> = Data.TaggedEnum<{
    Picked: { readonly labels: Array.NonEmptyReadonlyArray<L>; readonly other: Option.Option<string>; readonly notes: Option.Option<string> };
    Other: { readonly text: string };
    Noted: { readonly notes: string };
    Declined: object;
}>;
interface Answers extends Data.TaggedEnum.WithGenerics<1> {
    readonly taggedEnum: Answer<this['A']>;
}
interface Asking {
    readonly question: Question;
    readonly heard: Option.Option<ToolCallResult>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const parts = /(?:^|, )(?:"(?<quoted>(?:[^"\\]|\\.)*)"(?=, |$)|(?<bare>(?:(?!, )[^])*))/gu;

// --- [MODELS] --------------------------------------------------------------------------

const Answer = Data.taggedEnum<Answers>();
const Rest = Schema.Record(Schema.String, Schema.Unknown);
const Questions = Schema.Struct({ questions: Schema.Tuple([Schema.StructWithRest(Schema.Struct({ question: Schema.String, options: Schema.Array(Schema.StructWithRest(Schema.Struct({ label: Schema.String }), [Rest])) }), [Rest])]) });
const Annotations = Schema.Struct({ result: Schema.Struct({ annotations: Schema.Record(Schema.String, Schema.Struct({ notes: Schema.optionalKey(Schema.String) })) }) });
const Quoted = Schema.fromJsonString(Schema.String);

// --- [SERVICES] ------------------------------------------------------------------------

class Ask extends Context.Service<Ask, <const L extends string>(question: Question<L>) => Effect.Effect<Answer<L>, PlatformError.PlatformError>>()('@rasm/claude-code/ui/ask') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = <L extends string>({ options, multiSelect }: Question<L>, text: string, notes: Option.Option<string>): Answer<L> => {
    const labels = Record.fromIterableWith(options, ({ label }): readonly [string, L] => [label, label]);
    const said =
        multiSelect === true
            ? text
                  .matchAll(parts)
                  .map(({ groups }) => Option.match(Option.fromUndefinedOr(groups?.['quoted']), { onNone: () => groups?.['bare'] ?? '', onSome: (quoted) => Option.getOrElse(Schema.decodeOption(Quoted)(`"${quoted}"`), () => quoted) }))
                  .toArray()
            : [text];
    const [picked, typed] = Array.partition(said, (part) => Result.fromOption(Record.get(labels, part), () => part));
    const other = Option.map(Option.filter(Option.some(typed), Array.isReadonlyArrayNonEmpty), Array.join(', '));
    return Array.isReadonlyArrayNonEmpty(picked) ? Answer.Picked({ labels: picked, other, notes }) : Option.match(notes, { onSome: (noted) => Answer.Noted({ notes: noted }), onNone: () => Answer.Other({ text: Option.getOrElse(other, () => text) }) });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

function pending($: EngineInterface, asking: MutableRef.MutableRef<Option.Option<Asking>>, e: Frozen<Args<'tool.call'>>, next: Pick<Next<'tool.call'>, 'origin'>) {
    return Option.filter(Option.all({ held: MutableRef.get(asking), drawn: Schema.decodeUnknownOption(Questions)(e) }), ({ held, drawn }) => next.origin.plugin === $.plugin.name && drawn.questions[0].question === held.question.question);
}

function dialog($: EngineInterface, asking: MutableRef.MutableRef<Option.Option<Asking>>, e: Frozen<Args<'tool.call'>>, next: Next<'tool.call'>) {
    return Option.match(pending($, asking, e, next), {
        onNone: () => next(e),
        onSome: ({ held, drawn: { questions } }) => {
            const choices = Record.fromIterableWith(held.question.options, (choice) => [choice.label, choice]);
            const [{ options, ...asked }] = questions;
            return next({ ...e, questions: [{ ...asked, options: options.map((option) => ({ ...option, ...Option.getOrUndefined(Record.get(choices, option.label)) })) }] }).then((result) => {
                MutableRef.update(
                    asking,
                    Option.map((now) => (now === held ? { ...held, heard: Option.some(result) } : now)),
                );
                return result;
            });
        },
    });
}

function ask($: EngineInterface, asking: MutableRef.MutableRef<Option.Option<Asking>>) {
    const permits = Semaphore.makeUnsafe(1);
    return Layer.succeed(Ask)(<const L extends string>(question: Question<L>) =>
        Effect.gen(function* () {
            yield* Effect.sync(() => MutableRef.set(asking, Option.some<Asking>({ question, heard: Option.none() })));
            const answered = yield* Effect.result(Effect.tryPromise({ try: () => $.ui.ask(question.question, { options: question.options.map(Struct.get('label')), ...Struct.pick(question, ['header', 'multiSelect']) }), catch: (cause) => new Cause.UnknownError(cause) }));
            const heard = Option.flatMap(MutableRef.get(asking), Struct.get('heard'));
            const notes = heard.pipe(
                Option.flatMap(Schema.decodeUnknownOption(Annotations)),
                Option.flatMap(({ result }) => Record.get(result.annotations, question.question)),
                Option.flatMapNullishOr(Struct.get('notes')),
            );
            const refusal = { module: 'ui', method: 'ask', pathOrDescriptor: question.question } as const;
            return yield* Match.value({ heard: Option.getOrUndefined(heard), answered }).pipe(
                Match.when({ heard: { deny: Match.string } }, ({ heard: { deny } }) => Effect.fail(PlatformError.systemError({ ...refusal, _tag: 'PermissionDenied', description: deny }))),
                Match.when({ heard: { isError: true } }, () => Effect.succeed(Answer.Declined<L>())),
                Match.when({ answered: Result.isSuccess }, ({ answered: { success } }) => Effect.succeed(read(question, success, notes))),
                Match.orElse(({ heard: raised, answered: refused }) => Effect.fail(PlatformError.systemError({ ...refusal, _tag: raised === undefined ? 'NotFound' : 'Unknown', cause: Result.merge(refused) }))),
            );
        }).pipe(Effect.ensuring(Effect.sync(() => MutableRef.set(asking, Option.none()))), Semaphore.withPermit(permits)),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Answer, Ask, ask, type Asking, dialog, pending, type Question };
