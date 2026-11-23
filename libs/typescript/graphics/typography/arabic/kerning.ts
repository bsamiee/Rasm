import { Array, Effect, Option, Schema, Struct } from 'effect';
import { JoiningGroupName, joiningGroups, joins } from '#/arabic/joining';
import { Settings } from '#/settings';
import type { Face } from '#/shaping/face';
import { Profile } from '#/shaping/internal/profile';
import { ShapedText } from '#/shaping/shaped-text';
import { baseDirection, Span, words } from '#/shaping/word';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LONE_PUNCTUATION = /^\p{P}$/u;
const SEPARATOR = /^[\p{Zs}\u{200C}]?$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Boundary = Schema.Struct({
    slant: Schema.Finite.annotate({ description: 'Em from foot to top, positive toward preceding run' }).pipe(Schema.withDecodingDefaultKey(Effect.succeed(0)), Schema.withConstructorDefault(Effect.succeed(0))),
    top: Schema.OptionFromOptionalKey(Schema.Finite.annotate({ description: 'Height in em, face ascender when absent' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
    foot: Schema.OptionFromOptionalKey(Schema.Finite.annotate({ description: 'Height in em, face descender when absent' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
}).check(Schema.makeFilter(({ top, foot }) => !Option.exists(Option.all([top, foot]), ([upper, lower]) => upper <= lower), { description: 'top above foot' }));
const Rule = Schema.Struct({
    side: Schema.Literals(['following', 'preceding']).annotate({ description: 'Run the groups test, the first two letters of its word from the run when following, its last letter when preceding' }),
    groups: Schema.Array(JoiningGroupName),
    boundary: Boundary.annotate({ description: 'Line moved to touch preceding run from gap side' }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({})), Schema.withConstructorDefault(Effect.sync(() => Boundary.make({})))),
    correction: Schema.Finite.annotate({ description: 'Added to gap, in em' }).pipe(Schema.withDecodingDefaultKey(Effect.succeed(0)), Schema.withConstructorDefault(Effect.succeed(0))),
});
const Fallback = Rule.mapFields(Struct.omit(['side', 'groups']));
class Kerning extends Schema.Class<Kerning, { readonly brand: unique symbol }>('Kerning')(
    {
        rows: Schema.Array(Rule).pipe(Schema.withDecodingDefaultKey(Effect.succeed([])), Schema.withConstructorDefault(Effect.succeed([]))),
        fallback: Fallback.pipe(Schema.withDecodingDefaultKey(Effect.succeed({})), Schema.withConstructorDefault(Effect.sync(() => Fallback.make({})))),
        rise: Schema.Finite.check(Schema.isGreaterThanOrEqualTo(0))
            .annotate({ description: 'Em the following run may shift up or down to nest closer, 0 keeping one baseline' })
            .pipe(Schema.withDecodingDefaultKey(Effect.succeed(0)), Schema.withConstructorDefault(Effect.succeed(0))),
    },
    { description: 'Outline kerning between the cursive joins of Arabic-script words, gap rules by ICU joining group, first matching row wins, fallback otherwise' },
) {
    kerns(face: Face, text: string, settings = Settings.make({})) {
        const { unitsPerEm } = face;
        const { ascender, descender } = face.metrics.extents.horizontal;
        const [outlines, profiles] = [new Map<number, ReturnType<typeof Profile.outline>>(), new Map<string, Profile>()];
        const outline = (glyph: number) => outlines.getOrInsertComputed(glyph, () => Profile.outline(face.font.glyphToJson(glyph)));
        const direction = Option.getOrElse(settings.direction, () => baseDirection(text));
        const profile = (span: string) => profiles.getOrInsertComputed(span, () => Profile.of(ShapedText.of(face, ShapedText.Run.make({ text: span, direction: LONE_PUNCTUATION.test(span) ? Option.some(direction) : Option.none() }), settings), unitsPerEm, outline));
        const spans = words(text).flatMap((word) =>
            Array.window([word.start, ...joins(text, word).flatMap((join) => [join.start, join.end]), word.end], 2)
                .map(([from, to]) => ({ span: Span.make({ start: from, end: to }), rest: Span.make({ start: from, end: word.end }) }))
                .filter(({ span }, at) => at % 2 === 1 || LONE_PUNCTUATION.test(span.slice(text))),
        );
        return Array.window(spans, 2)
            .filter(([before, after]) => !LONE_PUNCTUATION.test(after.span.slice(text)) && SEPARATOR.test(text.slice(before.span.end, after.span.start)))
            .map(([{ span: before }, after]) => {
                const letters = { following: Array.take(joiningGroups(after.rest.slice(text)), 2), preceding: Array.takeRight(joiningGroups(before.slice(text)), 1) };
                const { boundary, correction } = Array.findFirst(this.rows, ({ side, groups }) => letters[side].some(({ group }) => groups.includes(group))).pipe(Option.getOrElse(() => this.fallback));
                const preceding = profile(before.slice(text));
                const bounded = preceding.bounded({ slant: boundary.slant * unitsPerEm, top: Option.getOrElse(boundary.top, () => ascender) * unitsPerEm, foot: Option.getOrElse(boundary.foot, () => descender) * unitsPerEm });
                return {
                    index: after.span.start,
                    length: after.span.length,
                    ...Option.match(Profile.gap(profile(after.span.slice(text)), bounded, Math.round(this.rise * unitsPerEm), preceding.advance), {
                        onNone: () => ({ kerning: 0, baselineShift: 0 }),
                        onSome: ({ gap, shift }) => ({ kerning: -Math.max(gap / unitsPerEm + correction, 0), baselineShift: shift / unitsPerEm }),
                    }),
                };
            })
            .filter(({ kerning, baselineShift }) => kerning !== 0 || baselineShift !== 0);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Kerning };
