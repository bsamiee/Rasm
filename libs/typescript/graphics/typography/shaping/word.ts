import { Array, Equal, Number, Option, Result, Schema, Struct } from 'effect';
import { Bidi, LineSegmenter, Script } from 'icu';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LETTERS = /\p{L}/gu;
const LINES = LineSegmenter.createAuto();
const BIDI = new Bidi();

// --- [MODELS] --------------------------------------------------------------------------

class Span extends Schema.Class<Span, { readonly brand: unique symbol }>('Span')(Schema.Struct(Struct.Record(['start', 'end'], Schema.Int.check(Schema.isGreaterThanOrEqualTo(0)))).check(Schema.makeFilter(({ start, end }) => start <= end, { description: 'start <= end' })), {
    description: 'UTF-16 range, end exclusive',
}) {
    static readonly styled = <R extends { readonly text: string; readonly font: string; readonly axes: readonly number[] }>(runs: readonly R[], first: number, last: number): readonly (R & { readonly start: number; readonly end: number })[] => {
        const [, placed] = Array.mapAccum(runs, 0, (at, run): readonly [number, R & { readonly start: number; readonly end: number }] => [at + run.text.length, { ...run, start: at, end: at + run.text.length }]);
        return Array.match(placed, { onEmpty: Array.empty, onNonEmpty: Array.groupWith((left, right) => left.font === right.font && Equal.equals(left.axes, right.axes)) })
            .map((group) => ({ ...Array.headNonEmpty(group), end: Array.lastNonEmpty(group).end, text: group.map(Struct.get('text')).join('') }))
            .filter((span) => (span.start < last && span.end > first) || (span.start === first && span.end === last))
            .map((span) => ({ ...span, start: Math.max(span.start, first), end: Math.min(span.end, last), text: span.text.slice(Math.max(span.start, first) - span.start, Math.min(span.end, last) - span.start) }));
    };

    get length() {
        return this.end - this.start;
    }
    slice(text: string) {
        return text.slice(this.start, this.end);
    }
    contains(offset: number) {
        return this.start <= offset && offset < this.end;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const words = (text: string): readonly Span[] => {
    const breaks = Array.unfold(LINES.segment(text), (segments) => Option.map(Option.liftPredicate(segments.next(), Number.isGreaterThanOrEqualTo(0)), (at) => [at, segments] as const));
    return Array.filterMap(Array.window(breaks, 2), ([start, next]) => {
        const content = text.slice(start, next).trimEnd();
        return content === '' ? Result.failVoid : Result.succeed(Span.make({ start, end: start + content.length }));
    });
};
const expandToWords = (text: string, first: number, last: number): readonly [number, number] =>
    words(text)
        .filter(({ start, end }) => start < last && first < end)
        .reduce<readonly [number, number]>(([from, to], { start, end }) => [Math.min(from, start), Math.max(to, end)], [first, last]);
const letters = (text: string) =>
    text
        .matchAll(LETTERS)
        .map(({ 0: letter, index: offset }) => {
            const point = letter.codePointAt(0) ?? 0;
            return { offset, point, script: Script.forChar(point) };
        })
        .toArray();
const baseDirection = (text: string) => {
    const levels = BIDI.forText(`${text}\u{2029}`, null);
    const rightToLeft: boolean = Bidi.levelIsRtl(levels.levelAt(levels.size - 1));
    return rightToLeft ? 'RTL' : 'LTR';
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { baseDirection, expandToWords, letters, Span, words };
