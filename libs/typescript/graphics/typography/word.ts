import { Array, Data, Option } from 'effect';
import { GeneralCategory, JoiningGroup, JoiningType } from 'icu';

// --- [TYPES] ---------------------------------------------------------------------------

interface Span {
    readonly start: number;
    readonly end: number;
}
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

// --- [MODELS] --------------------------------------------------------------------------

class Word extends Data.Class<{
    readonly start: number;
    readonly end: number;
    readonly joins: readonly Span[];
}> {
    static readonly of = (text: string): readonly Word[] =>
        Array.fromIterable(text.matchAll(WORDS)).map(({ 0: content, index }) => new Word({ start: index, end: index + content.length, joins: closedJoins([...content].reduce<JoiningState>(joiningStep, { offset: index, joins: [], open: Option.none(), previous: 0 })) }));
    static readonly expand = (text: string, first: number, last: number): readonly [number, number] => {
        const start = text.slice(0, first + 1).search(TRAILING_WORD);
        const end = text.slice(0, last).search(TRAILING_WORD) < last ? last + text.slice(last).search(WORD_END) : last;
        return [Math.min(start, end), end];
    };
    static readonly letters = (text: string) => Array.fromIterable(text.matchAll(LETTERS)).map(({ 0: letter, index: offset }) => ({ offset, group: JoiningGroup.forChar(letter.codePointAt(0) ?? 0) }));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const isDigit = (point: number) => GeneralCategory.forChar(point) === GeneralCategory.DecimalNumber;
const closedJoins = ({ offset, joins, open }: JoiningState): readonly Span[] => [...joins, ...Option.toArray(Option.map(open, (start) => ({ start, end: offset })))];
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
