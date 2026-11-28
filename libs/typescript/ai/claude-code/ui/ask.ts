import type { AskOptions } from 'claude-code';
import { Array, Data, Effect, Match, Option, PlatformError, Record, Result, Schema, Struct } from 'effect';
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

const parts = /(?:^|, )(?:"(?<quoted>(?:[^"\\]|\\.)*)"(?=, |$)|(?<bare>(?:(?!, )[^])*))/gu;

// --- [MODELS] --------------------------------------------------------------------------

const Answer = Data.taggedEnum<Answers>();
const Answered = Schema.Struct({ result: Schema.Struct({ answers: Schema.Record(Schema.String, Schema.String), annotations: Schema.Record(Schema.String, Schema.Struct({ notes: Schema.optionalKey(Schema.String) })) }) });
const Quoted = Schema.fromJsonString(Schema.String);

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
const ask = Effect.fnUntraced(function* <const L extends string>(question: Question<L>) {
    const engine = yield* Engine;
    const refusal = { module: 'agent', method: 'call', pathOrDescriptor: question.question } as const;
    const called = yield* engine.callTool({ tool: 'AskUserQuestion', input: { questions: [{ question: question.question, header: question.header, multiSelect: question.multiSelect ?? false, options: question.options.map((choice) => ({ description: '', ...choice })) }] } });
    const answered = Option.flatMap(Schema.decodeUnknownOption(Answered)(called), ({ result: { answers, annotations } }) => Option.map(Record.get(answers, question.question), (text) => read(question, text, Option.flatMapNullishOr(Record.get(annotations, question.question), Struct.get('notes')))));
    return yield* Match.value(called).pipe(
        Match.when({ deny: Match.string }, ({ deny }) => Effect.fail(PlatformError.systemError({ ...refusal, _tag: 'PermissionDenied', description: deny }))),
        Match.when({ isError: true }, () => Effect.succeed(Answer.Declined<L>())),
        Match.orElse(() => Effect.fromOption(answered, () => PlatformError.systemError({ ...refusal, _tag: 'InvalidData', description: 'an AskUserQuestion result holding no answer to the question' }))),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Answer, ask, type Question };
