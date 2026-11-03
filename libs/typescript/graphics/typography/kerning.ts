import { Array, Effect, Option, Schema, SchemaGetter, SchemaIssue, Struct } from 'effect';
import { BidiClass, JoiningGroup } from 'icu';
import type { Face } from '#/face';
import { Profile } from '#/profile';
import { Settings } from '#/settings';
import { ShapedText } from '#/shaped-text';
import { Word } from '#/word';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LONE_PUNCTUATION = /^\p{P}$/u;
const SEPARATOR = /^[\p{Zs}\u{200C}]?$/u;
const STRONG = new Map<BidiClass, Option.Option.Value<Settings['direction']>>([
    [BidiClass.LeftToRight, 'LTR'],
    [BidiClass.RightToLeft, 'RTL'],
    [BidiClass.ArabicLetter, 'RTL'],
]);

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
}).check(Schema.makeFilter(({ top, foot }) => !Option.exists(Option.all([top, foot]), ([upper, lower]) => upper <= lower), { description: 'top above foot' }));
const Rule = Schema.Struct({
    side: Schema.Literals(['following', 'preceding']).annotate({ description: 'Run the groups test, its first two letters when following, its last letter when preceding' }),
    groups: Schema.Array(GroupName),
    boundary: Boundary.annotate({ description: 'Line moved to touch preceding run from gap side' }).pipe(Schema.withConstructorDefault(Effect.sync(() => Boundary.make({})))),
    correction: Schema.Finite.annotate({ description: 'Added to gap, in em' }).pipe(Schema.withConstructorDefault(Effect.succeed(0))),
});
const Fallback = Rule.mapFields(Struct.omit(['side', 'groups']));
class Kerning extends Schema.Class<Kerning, { readonly brand: unique symbol }>('Kerning')(
    {
        rows: Schema.Array(Rule).pipe(Schema.withConstructorDefault(Effect.succeed([]))),
        fallback: Fallback.pipe(Schema.withConstructorDefault(Effect.sync(() => Fallback.make({})))),
    },
    { description: 'Gap rules by ICU joining group, first matching row wins, fallback otherwise' },
) {
    kerns(face: Face, text: string, settings = Settings.make({})) {
        const { unitsPerEm } = face;
        const { ascender, descender } = face.metrics.extents.horizontal;
        const outline = memoized((glyph: number) => Profile.outline(face.font.glyphToJson(glyph)));
        const direction = Option.orElse(settings.direction, () => Array.findFirst(Array.fromIterable(text), (character) => Option.fromUndefinedOr(STRONG.get(BidiClass.forChar(character.codePointAt(0) ?? 0)))));
        const profile = memoized((span: string) => Profile.of(ShapedText.of(face, ShapedText.Run.make({ text: span, direction: LONE_PUNCTUATION.test(span) ? direction : Option.none() }), settings), unitsPerEm, outline));
        const gap = (before: string, after: string) => {
            const { boundary, correction } = ruleFor(this, before, after);
            const preceding = profile(before);
            const bounded = preceding.bounded({ slant: boundary.slant * unitsPerEm, top: Option.getOrElse(boundary.top, () => ascender) * unitsPerEm, foot: Option.getOrElse(boundary.foot, () => descender) * unitsPerEm });
            return Option.match(profile(after).gap(bounded), { onNone: () => 0, onSome: (clearance) => Math.max(Math.min(clearance, preceding.advance) / unitsPerEm + correction, 0) });
        };
        const content = ({ start, end }: Word['joins'][number]) => text.slice(start, end);
        const spans = Word.of(text).flatMap(({ start, end, joins }) =>
            Array.window([start, ...joins.flatMap((join) => [join.start, join.end]), end], 2)
                .map(([from, to]) => ({ start: from, end: to }))
                .filter((span, at) => at % 2 === 1 || LONE_PUNCTUATION.test(content(span))),
        );
        return Array.window(spans, 2)
            .filter(([before, after]) => !LONE_PUNCTUATION.test(content(after)) && SEPARATOR.test(text.slice(before.end, after.start)))
            .map(([before, after]) => ({ index: after.start, kerning: -gap(content(before), content(after)) }))
            .filter(({ kerning }) => kerning !== 0);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const ruleFor = ({ rows, fallback }: Kerning, before: string, after: string) => {
    const letters = { following: Array.take(Word.letters(after), 2), preceding: Array.takeRight(Word.letters(before), 1) };
    return Option.getOrElse(
        Array.findFirst(rows, ({ side, groups }) => letters[side].some(({ group }) => groups.includes(group))),
        () => fallback,
    );
};
const memoized = <K, V>(compute: (key: K) => V) => {
    const computed = new Map<K, V>();
    return (key: K): V =>
        Option.getOrElse(Option.fromUndefinedOr(computed.get(key)), () => {
            const value = compute(key);
            computed.set(key, value);
            return value;
        });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Kerning };
