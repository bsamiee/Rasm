import { Array, Data, Effect, Option, Schema, SchemaGetter, SchemaIssue, Struct } from 'effect';
import { JoiningGroup } from 'icu';
import { Profile } from '#/composition/profile';
import type { Face } from '#/face';
import { Shaped } from '#/shaped';
import { Word } from '#/text/words';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LONE_PUNCTUATION = /^\p{P}$/u;
const SEPARATOR = /^[ \u{200C}]?$/u;
const ARABIC = Shaped.Run.make({ text: '', script: Option.some('arab'), direction: Option.some('RTL') });

// --- [MODELS] --------------------------------------------------------------------------

const Group = Schema.String.annotate({ description: 'ICU JoiningGroup value name' }).pipe(
    Schema.decodeTo(Schema.instanceOf(JoiningGroup), {
        decode: SchemaGetter.transformEffect((name: string, options) => Effect.fromOption(Option.fromNullOr(JoiningGroup.tryFromStr(name)), () => new SchemaIssue.InvalidValue({ message: `No joining group named ${name}` }, name, options))),
        encode: SchemaGetter.transform((group: JoiningGroup) => group.value),
    }),
);
const Wall = Schema.Struct({
    x: Schema.Finite.annotate({ description: 'Lean in em from foot to top, top toward preceding run when positive' }).pipe(Schema.withConstructorDefault(Effect.succeed(0))),
    top: Schema.OptionFromOptionalKey(Schema.Finite.annotate({ description: 'Height in em, face ascender when absent' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
    foot: Schema.OptionFromOptionalKey(Schema.Finite.annotate({ description: 'Height in em, face descender when absent' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
});
const Rule = Schema.Struct({
    side: Schema.Literals(['before', 'after']).annotate({ description: 'Letters read: before the first two of following run, after the last of preceding run' }),
    groups: Schema.Array(Group),
    wall: Wall.annotate({ description: 'Line slid to contact with preceding run from the gap side' }).pipe(Schema.withConstructorDefault(Effect.sync(() => Wall.make({})))),
    correction: Schema.Finite.annotate({ description: 'Added to gap, in em' }).pipe(Schema.withConstructorDefault(Effect.succeed(0))),
});
const Otherwise = Rule.mapFields(Struct.omit(['side', 'groups']));
class Kerning extends Data.Class<{
    readonly index: number;
    readonly em: number;
}> {
    static readonly Rule = Rule;
    static readonly Rules = Schema.Struct({
        rows: Schema.Array(Rule).pipe(Schema.withConstructorDefault(Effect.succeed([]))),
        otherwise: Otherwise.pipe(Schema.withConstructorDefault(Effect.sync(() => Otherwise.make({})))),
    });
    static readonly rule = (rules: typeof Kerning.Rules.Type, before: string, after: string) => {
        const facing = { before: Array.take(Word.letters(after), 2), after: Array.takeRight(Word.letters(before), 1) };
        return Option.getOrElse(
            Array.findFirst(rules.rows, ({ side, groups }) => facing[side].some(({ group }) => groups.includes(group))),
            () => rules.otherwise,
        );
    };
    static readonly pair =
        (face: Face, rules: typeof Kerning.Rules.Type, run: typeof Shaped.Run.Type = ARABIC) =>
        (before: string, after: string) => {
            const { wall, correction } = Kerning.rule(rules, before, after);
            const { ascender, descender } = face.metrics.extents.hhea;
            const profile = (text: string) => Profile.of(face, Shaped.of(face, { ...run, text }));
            const [preceding, following] = [profile(before), profile(after)];
            const walled = Profile.wall(preceding, { x: wall.x * face.unitsPerEm, top: Option.getOrElse(wall.top, () => ascender) * face.unitsPerEm, foot: Option.getOrElse(wall.foot, () => descender) * face.unitsPerEm });
            return Option.match(Profile.gap(following, walled), { onNone: () => 0, onSome: (gap) => Math.max(Math.min(gap, preceding.advance) / face.unitsPerEm + correction, 0) });
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
