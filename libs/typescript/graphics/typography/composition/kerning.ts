import { Array, Data, Effect, flow, Option, Order, Schema, Struct } from 'effect';
import { Profile } from '#/composition/profile';
import type { Face } from '#/face';
import { Shaped } from '#/shaped';
import { Unicode } from '#/text/unicode';
import { Word } from '#/text/words';

// --- [TYPES] ---------------------------------------------------------------------------

type Span = Word['runs'][number];

// --- [MODELS] --------------------------------------------------------------------------

const Wall = Schema.Struct({
    x: Schema.Finite.annotate({ description: 'Lean in em from foot to top, top toward preceding run when positive' }).pipe(Schema.withConstructorDefault(Effect.succeed(0))),
    top: Schema.OptionFromOptionalKey(Schema.Finite.annotate({ description: 'Height in em, face ascender when absent' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
    foot: Schema.OptionFromOptionalKey(Schema.Finite.annotate({ description: 'Height in em, face descender when absent' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
});
const Rule = Schema.Struct({
    side: Schema.Literals(['before', 'after']).annotate({ description: 'Letter side facing gap: before reads first two base letters of following run, after reads last of preceding run' }),
    groups: Schema.Array(Schema.String).annotate({ description: 'ICU JoiningGroup value names' }),
    wall: Wall.annotate({ description: 'Line slid to contact with preceding run from the gap side' }).pipe(Schema.withConstructorDefault(Effect.sync(() => Wall.make({})))),
    correction: Schema.Finite.annotate({ description: 'Added to gap, in em' }).pipe(Schema.withConstructorDefault(Effect.succeed(0))),
});
const Otherwise = Rule.mapFields(Struct.omit(['side', 'groups']));
class Kerning extends Data.Class<{
    readonly index: number;
    readonly before: string;
    readonly after: string;
    readonly em: number;
}> {
    static readonly Rule = Rule;
    static readonly Rules = Schema.Struct({
        rows: Schema.Array(Rule).pipe(Schema.withConstructorDefault(Effect.succeed([]))),
        otherwise: Otherwise.pipe(Schema.withConstructorDefault(Effect.sync(() => Otherwise.make({})))),
    });
    static readonly rule = Effect.fn('Kerning.rule')(function* (rules: typeof Kerning.Rules.Type, before: string, after: string) {
        const { GeneralCategory, GeneralCategoryGroup, JoiningGroup } = yield* Unicode;
        const letter = GeneralCategoryGroup.letter();
        const letters = (text: string) =>
            points(text)
                .filter((point) => letter.contains(GeneralCategory.forChar(point)))
                .map((point) => JoiningGroup.forChar(point).value);
        const facing = { before: Array.take(letters(after), 2), after: Array.takeRight(letters(before), 1) };
        return Option.getOrElse(
            Array.findFirst(rules.rows, ({ side, groups }) => facing[side].some((name) => groups.includes(name))),
            () => rules.otherwise,
        );
    });
    static readonly pair = (face: Face, rules: typeof Kerning.Rules.Type) =>
        Effect.fn('Kerning.pair')(function* (before: string, after: string) {
            const { wall, correction } = yield* Kerning.rule(rules, before, after);
            const { ascender, descender } = face.metrics.extents.hhea;
            const [preceding, following] = yield* Effect.all([profile(face, before), profile(face, after)]);
            const walled = Profile.wall(preceding, {
                x: wall.x * preceding.unitsPerEm,
                top: Option.getOrElse(wall.top, () => ascender) * preceding.unitsPerEm,
                foot: Option.getOrElse(wall.foot, () => descender) * preceding.unitsPerEm,
            });
            return Option.map(Profile.gap(following, walled), (gap) => Math.max(Math.min(gap, preceding.advance) / preceding.unitsPerEm + correction, 0));
        });
    static readonly of = Effect.fn('Kerning.of')(function* <R>(text: string, pair: (before: string, after: string) => Effect.Effect<Option.Option<number>, never, R>) {
        const { GeneralCategory, GeneralCategoryGroup } = yield* Unicode;
        const punctuation = GeneralCategoryGroup.punctuation();
        const words = yield* Word.of(text);
        const content = ({ start, end }: Span) => text.slice(start, end);
        const lone = (span: Span) => Array.match(points(content(span)), { onEmpty: () => false, onNonEmpty: (all) => all.length === 1 && punctuation.contains(GeneralCategory.forChar(Array.headNonEmpty(all))) });
        const spans = words.flatMap(({ start, end, runs }) => Array.sortWith([...runs, ...Array.zipWith([start, ...runs.map(Struct.get('end'))], [...runs.map(Struct.get('start')), end], (from, to) => ({ start: from, end: to })).filter(lone)], Struct.get('start'), Order.Number));
        const adjacent = Array.window(spans, 2).filter(([before, after]) => !lone(after) && Array.contains(['', ' ', '\u200c'], text.slice(before.end, after.start)));
        return yield* Effect.map(
            Effect.forEach(adjacent, ([before, after]) =>
                Effect.map(
                    pair(content(before), content(after)),
                    flow(
                        Option.filter((gap) => gap !== 0),
                        Option.map((gap) => new Kerning({ index: after.start, before: content(before), after: content(after), em: -gap })),
                    ),
                ),
            ),
            Array.getSomes,
        );
    });
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const points = (text: string) => Array.fromIterable(text).map((character) => character.codePointAt(0) ?? 0);
const profile = (face: Face, text: string) =>
    Schema.decodeEffect(Shaped.Run)({ text, script: 'arab', direction: 'RTL' }).pipe(
        Effect.orDie,
        Effect.map((run) => Profile.of(face, Shaped.of(face, run))),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { Kerning };
