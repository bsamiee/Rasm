import { Array, Data, Option, Schema, Struct } from 'effect';
import { BidiClass, GeneralCategory, JoiningGroup, JoiningType, Script } from 'icu';

// --- [TYPES] ---------------------------------------------------------------------------

interface JoiningState {
    readonly offset: number;
    readonly joins: readonly Span[];
    readonly open: Option.Option<number>;
    readonly previous: number;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORDS = /\P{White_Space}+/gu;
const LETTERS = /\p{L}/gu;
const TRAILING_WORD = /\P{White_Space}*$/u;
const WORD_END = /\p{White_Space}|$/u;
const UNJOINED = [JoiningType.NonJoining, JoiningType.Transparent];
const STRONG = new Map([
    [BidiClass.LeftToRight, 'LTR'],
    [BidiClass.RightToLeft, 'RTL'],
    [BidiClass.ArabicLetter, 'RTL'],
] as const);

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
        Array.fromIterable(text.matchAll(WORDS)).map(({ 0: content, index }) => new Word({ start: index, end: index + content.length, joins: closedJoins([...content].reduce<JoiningState>(joiningStep, { offset: index, joins: [], open: Option.none(), previous: 0 })) }));
    static readonly expand = (text: string, first: number, last: number): readonly [number, number] => {
        const start = text.slice(0, first + 1).search(TRAILING_WORD);
        const end = text.slice(0, last).search(TRAILING_WORD) < last ? last + text.slice(last).search(WORD_END) : last;
        return [Math.min(start, end), end];
    };
    static readonly letters = (text: string) =>
        Array.fromIterable(text.matchAll(LETTERS)).map(({ 0: letter, index: offset }) => {
            const point = letter.codePointAt(0) ?? 0;
            return { offset, group: JoiningGroup.forChar(point), script: Script.forChar(point) };
        });
    static readonly direction = (text: string) => Array.findFirst(Array.fromIterable(text), (character) => Option.fromUndefinedOr(STRONG.get(BidiClass.forChar(character.codePointAt(0) ?? 0))));
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

export { Word };
