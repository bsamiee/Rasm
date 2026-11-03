import { Array, Data, Effect, Option, Schema, SchemaGetter, SchemaIssue, Struct } from 'effect';
import { JoiningGroup } from 'icu';
import { Profile } from '#/composition/profile';
import type { Face } from '#/face';
import { ShapedText } from '#/shaped-text';
import { Word } from '#/text/words';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LONE_PUNCTUATION = /^\p{P}$/u;
const SEPARATOR = /^[ \u{200C}]?$/u;
const ARABIC = ShapedText.Run.make({ text: '', script: Option.some('arab'), direction: Option.some('RTL') });

// --- [MODELS] --------------------------------------------------------------------------

const GroupName = Schema.String.annotate({ description: 'ICU joining group name' }).pipe(
    Schema.decodeTo(Schema.instanceOf(JoiningGroup), {
        decode: SchemaGetter.transformEffect((name: string, options) => Effect.fromOption(Option.fromNullOr(JoiningGroup.tryFromStr(name)), () => new SchemaIssue.InvalidValue({ message: `No joining group named ${name}` }, name, options))),
        encode: SchemaGetter.transform((group: JoiningGroup) => group.value),
    }),
);
const Boundary = Schema.Struct({
    slant: Schema.Finite.annotate({ description: 'Em from foot to top, positive toward preceding run' }).pipe(Schema.withConstructorDefault(Effect.succeed(0))),
    top: Schema.OptionFromOptionalKey(Schema.Finite.annotate({ description: 'Height in em, face ascender when absent' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
    foot: Schema.OptionFromOptionalKey(Schema.Finite.annotate({ description: 'Height in em, face descender when absent' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
});
const Rule = Schema.Struct({
    side: Schema.Literals(['before', 'after']).annotate({ description: 'Before reads first two letters of following run, after reads last letter of preceding run' }),
    groups: Schema.Array(GroupName),
    boundary: Boundary.annotate({ description: 'Line moved to touch preceding run from gap side' }).pipe(Schema.withConstructorDefault(Effect.sync(() => Boundary.make({})))),
    correction: Schema.Finite.annotate({ description: 'Added to gap, in em' }).pipe(Schema.withConstructorDefault(Effect.succeed(0))),
});
const Fallback = Rule.mapFields(Struct.omit(['side', 'groups']));
class Kerning extends Data.Class<{
    readonly index: number;
    readonly em: number;
}> {
    static readonly Rule = Rule;
    static readonly Rules = Schema.Struct({
        rows: Schema.Array(Rule).pipe(Schema.withConstructorDefault(Effect.succeed([]))),
        fallback: Fallback.pipe(Schema.withConstructorDefault(Effect.sync(() => Fallback.make({})))),
    });
    static readonly ruleFor = (rules: typeof Kerning.Rules.Type, before: string, after: string) => {
        const letters = { before: Array.take(Word.letters(after), 2), after: Array.takeRight(Word.letters(before), 1) };
        return Option.getOrElse(
            Array.findFirst(rules.rows, ({ side, groups }) => letters[side].some(({ group }) => groups.includes(group))),
            () => rules.fallback,
        );
    };
    static readonly pair =
        (face: Face, rules: typeof Kerning.Rules.Type, run: typeof ShapedText.Run.Type = ARABIC) =>
        (before: string, after: string) => {
            const { boundary, correction } = Kerning.ruleFor(rules, before, after);
            const { ascender, descender } = face.metrics.extents.hhea;
            const profile = (text: string) => Profile.of(face, ShapedText.of(face, { ...run, text }));
            const [preceding, following] = [profile(before), profile(after)];
            const bounded = Profile.withBoundary(preceding, { slant: boundary.slant * face.unitsPerEm, top: Option.getOrElse(boundary.top, () => ascender) * face.unitsPerEm, foot: Option.getOrElse(boundary.foot, () => descender) * face.unitsPerEm });
            return Option.match(Profile.gap(following, bounded), { onNone: () => 0, onSome: (gap) => Math.max(Math.min(gap, preceding.advance) / face.unitsPerEm + correction, 0) });
        };
    static readonly of = <R>(text: string, pair: (before: string, after: string) => Effect.Effect<number, never, R>) => {
        const content = ({ start, end }: Word['runs'][number]) => text.slice(start, end);
        const spans = Word.of(text).flatMap(({ start, end, runs }) =>
            Array.window([start, ...runs.flatMap((run) => [run.start, run.end]), end], 2)
                .map(([from, to]) => ({ start: from, end: to }))
                .filter((span, at) => at % 2 === 1 || LONE_PUNCTUATION.test(content(span))),
        );
        const adjacent = Array.window(spans, 2).filter(([before, after]) => !LONE_PUNCTUATION.test(content(after)) && SEPARATOR.test(text.slice(before.end, after.start)));
        return Effect.map(
            Effect.forEach(adjacent, ([before, after]) => Effect.map(pair(content(before), content(after)), (gap) => new Kerning({ index: after.start, em: -gap }))),
            Array.filter(({ em }) => em !== 0),
        );
    };
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Kerning };
