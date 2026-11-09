import { Array, Data, Number, Option, Schema, Struct } from 'effect';
import { Bidi, GeneralCategory, JoiningGroup, JoiningType, LineSegmenter, Script } from 'icu';

// --- [TYPES] ---------------------------------------------------------------------------

interface JoiningState {
    readonly offset: number;
    readonly joins: readonly Span[];
    readonly open: Option.Option<number>;
    readonly previous: number;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const LETTERS = /\p{L}/gu;
const PARAGRAPH_SEPARATOR = '\u{2029}';
const UNJOINED = [JoiningType.NonJoining, JoiningType.Transparent];
const LINES = LineSegmenter.createAuto();
const BIDI = new Bidi();

// --- [MODELS] --------------------------------------------------------------------------

class Span extends Schema.Class<Span, { readonly brand: unique symbol }>('Span')(Schema.Struct(Struct.Record(['start', 'end'], Schema.Int.check(Schema.isGreaterThanOrEqualTo(0)))).check(Schema.makeFilter(({ start, end }) => start <= end, { description: 'start <= end' })), {
    description: 'UTF-16 range, end exclusive',
}) {
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
class Word extends Data.Class<{
    readonly start: number;
    readonly end: number;
    readonly joins: readonly Span[];
}> {
    static readonly Span = Span;
    static readonly of = (text: string): readonly Word[] =>
        Array.window(
            Array.unfold(LINES.segment(text), (breaks) => Option.map(Option.liftPredicate(breaks.next(), Number.isGreaterThanOrEqualTo(0)), (at) => [at, breaks] as const)),
            2,
        ).flatMap(([start, next]) => {
            const content = text.slice(start, next).trimEnd();
            return content === '' ? [] : [new Word({ start, end: start + content.length, joins: closedJoins([...content].reduce<JoiningState>(joiningStep, { offset: start, joins: [], open: Option.none(), previous: 0 })) })];
        });
    static readonly expand = (text: string, first: number, last: number): readonly [number, number] =>
        Word.of(text)
            .filter(({ start, end }) => start < last && first < end)
            .reduce<readonly [number, number]>(([from, to], { start, end }) => [Math.min(from, start), Math.max(to, end)], [first, last]);
    static readonly letters = (text: string) =>
        Array.fromIterable(text.matchAll(LETTERS)).map(({ 0: letter, index: offset }) => {
            const point = letter.codePointAt(0) ?? 0;
            return { offset, group: JoiningGroup.forChar(point), script: Script.forChar(point) };
        });
    static readonly direction = (text: string) => {
        const levels = BIDI.forText(`${text}${PARAGRAPH_SEPARATOR}`, null);
        const rightToLeft: boolean = Bidi.levelIsRtl(levels.levelAt(levels.size - 1));
        return rightToLeft ? 'RTL' : 'LTR';
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const isDigit = (point: number) => GeneralCategory.forChar(point) === GeneralCategory.DecimalNumber;
const closedJoins = ({ offset, joins, open }: JoiningState): readonly Span[] => [...joins, ...Option.toArray(Option.map(open, (start) => Span.make({ start, end: offset })))];
const joiningStep = (state: JoiningState, character: string): JoiningState => {
    const point = character.codePointAt(0) ?? 0;
    const type = JoiningType.forChar(point);
    const transparent = type === JoiningType.Transparent;
    const joinsForward = [JoiningType.DualJoining, JoiningType.LeftJoining, JoiningType.JoinCausing];
    const joinsBackward = [JoiningType.DualJoining, JoiningType.RightJoining, JoiningType.JoinCausing];
    const joins = transparent || (isDigit(point) && isDigit(state.previous)) || (joinsForward.includes(JoiningType.forChar(state.previous)) && joinsBackward.includes(type));
    return {
        offset: state.offset + character.length,
        ...(Option.isSome(state.open) && joins ? { joins: state.joins, open: state.open } : { joins: closedJoins(state), open: UNJOINED.includes(type) && !isDigit(point) ? Option.none() : Option.some(state.offset) }),
        previous: transparent ? state.previous : point,
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Span, Word };
