import { Array, Data, Effect, Option } from 'effect';
import { Unicode } from '#/text/unicode';

// --- [TYPES] ---------------------------------------------------------------------------

interface Span {
    readonly start: number;
    readonly end: number;
}
interface Joining {
    readonly offset: number;
    readonly runs: readonly Span[];
    readonly open: Option.Option<number>;
    readonly forward: boolean;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const TRAILING_WORD = /\P{White_Space}*$/u;
const WORD_END = /\p{White_Space}|$/u;

// --- [MODELS] --------------------------------------------------------------------------

class Word extends Data.Class<{
    readonly start: number;
    readonly end: number;
    readonly runs: readonly Span[];
    readonly spacesBefore: number;
    readonly breakBefore: boolean;
}> {
    static readonly of = Effect.fn('Word.of')(function* (text: string) {
        const { JoiningType, Script, ScriptWithExtensions } = yield* Unicode;
        const scripts = new ScriptWithExtensions();
        const arabic = Script.Arabic.toIntegerValue();
        const joinsForward = [JoiningType.DualJoining, JoiningType.LeftJoining, JoiningType.JoinCausing];
        const joinsBackward = [JoiningType.DualJoining, JoiningType.RightJoining, JoiningType.JoinCausing];
        const step = (state: Joining, character: string): Joining => {
            const point = character.codePointAt(0) ?? 0;
            const type = JoiningType.forChar(point);
            const transparent = type === JoiningType.Transparent;
            const member = scripts.hasScript(point, arabic);
            const joins = transparent || ((member || type === JoiningType.JoinCausing) && state.forward && joinsBackward.includes(type));
            return {
                offset: state.offset + character.length,
                ...(Option.isSome(state.open) && joins ? { runs: state.runs, open: state.open } : { runs: closed(state), open: member ? Option.some(state.offset) : Option.none() }),
                forward: transparent ? state.forward : joinsForward.includes(type),
            };
        };
        const [, words] = Array.mapAccum(Array.fromIterable(text.matchAll(/\P{White_Space}+/gu)), 0, (previous, { 0: content, index }) => {
            const end = index + content.length;
            const gap = text.slice(previous, index);
            const spacesBefore = gap.replace(/\P{Zs}/gu, '').length;
            return [end, new Word({ start: index, end, runs: closed([...content].reduce<Joining>(step, { offset: index, runs: [], open: Option.none(), forward: false })), spacesBefore, breakBefore: gap.length > spacesBefore })] as const;
        });
        return words;
    });
    static readonly expand = (text: string, first: number, last: number): readonly [number, number] => {
        const start = text.slice(0, first + 1).search(TRAILING_WORD);
        const end = text.slice(0, last).search(TRAILING_WORD) < last ? last + text.slice(last).search(WORD_END) : last;
        return [Math.min(start, end), end];
    };

    get arabic() {
        return Array.isReadonlyArrayNonEmpty(this.runs);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const closed = ({ offset, runs, open }: Joining): readonly Span[] => [...runs, ...Option.toArray(Option.map(open, (start) => ({ start, end: offset })))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { Word };
