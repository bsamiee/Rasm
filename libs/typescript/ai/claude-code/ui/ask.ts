import type { AskOptions } from 'claude-code';
import { Array, Data, Effect, Match, Option, PlatformError, Record, Result, Schema } from 'effect';
import { Engine } from '#/engine/interface';

// --- [TYPES] ---------------------------------------------------------------------------

interface Choice<L extends string> {
    readonly label: L;
    readonly description?: string;
    readonly preview?: string;
}
type Question<L extends string = string> = Required<Pick<AskOptions, 'header'>> &
    Pick<AskOptions, 'multiSelect'> & {
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

// --- [CONSTANTS] -----------------------------------------------------------------------

const parts = /(?<=^|, )(?:"(?:[^"\\]|\\.)*"(?=, |$)|(?:(?!, )[^])*)/gu;

// --- [MODELS] --------------------------------------------------------------------------

const Answer = Data.taggedEnum<Answers>();
const Answered = Schema.Struct({ answers: Schema.Record(Schema.String, Schema.String), annotations: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ notes: Schema.optionalKey(Schema.String), unanswered: Schema.optionalKey(Schema.String) }))) });
const Quoted = Schema.fromJsonString(Schema.String);

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = <L extends string>({ options, multiSelect }: Question<L>, text: string, notes: Option.Option<string>): Answer<L> => {
    const labels = Record.fromIterableWith(options, ({ label }): readonly [string, L] => [label, label]);
    const said = multiSelect === true ? text.matchAll(parts).map(([part]) => Option.getOrElse(Schema.decodeOption(Quoted)(part), () => part)) : [text];
    const [picked, typed] = Array.partition(said, (part) => Result.fromOption(Record.get(labels, part), () => part));
    return Array.match(picked, {
        onNonEmpty: (chosen) => Answer.Picked({ labels: chosen, other: Option.map(Option.liftPredicate(typed, Array.isReadonlyArrayNonEmpty), Array.join(', ')), notes }),
        onEmpty: () => Option.match(notes, { onSome: (noted) => Answer.Noted({ notes: noted }), onNone: () => Answer.Other({ text: Array.join(typed, ', ') }) }),
    });
};
const ask = Effect.fnUntraced(function* <const L extends string>(question: Question<L>) {
    const engine = yield* Engine;
    const called = yield* engine.callTool({ tool: 'AskUserQuestion', input: { questions: [{ ...question, multiSelect: question.multiSelect ?? false, options: question.options.map((choice) => ({ description: '', ...choice })) }] } });
    const said = Option.map(Schema.decodeUnknownOption(Answered)(called.result), ({ answers, annotations }) => ({ text: answers[question.question], ...annotations?.[question.question] }));
    const refusal = { module: 'agent', method: 'call' } as const;
    return yield* Match.value({ ...called, said: Option.getOrUndefined(said) }).pipe(
        Match.whenOr({ deny: Match.string }, { said: { unanswered: Match.string } }, () => Effect.succeed(Answer.Declined<L>())),
        Match.when({ isError: true }, ({ text }) => PlatformError.badArgument({ ...refusal, description: text })),
        Match.when({ said: { text: Match.string } }, ({ said: { text, notes } }) => Effect.succeed(read(question, text, Option.fromUndefinedOr(notes)))),
        Match.orElse(({ result }) => PlatformError.systemError({ ...refusal, _tag: 'InvalidData', pathOrDescriptor: question.question, description: 'an AskUserQuestion result holding no answer to the question', cause: result })),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Answer, ask, type Question };
