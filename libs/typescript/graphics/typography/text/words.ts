import { Array, Data, Effect, Option, Order } from 'effect';
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

const separators = [' ', '\r', '\n', ' '] as const;
const spans = new RegExp(`[^${separators.join('')}]+`, 'gu');

// --- [MODELS] --------------------------------------------------------------------------

class Word extends Data.Class<{
    readonly start: number;
    readonly end: number;
    readonly arabic: boolean;
    readonly runs: readonly Span[];
    readonly spacesBefore: number;
    readonly breakBefore: boolean;
}> {
    static readonly of = Effect.fn('Word.of')(function* (text: string) {
        const { JoiningType, Script, ScriptWithExtensions } = yield* Unicode;
        const scripts = new ScriptWithExtensions();
        const arabic = Script.Arabic.toIntegerValue();
        const forward = [JoiningType.DualJoining, JoiningType.LeftJoining, JoiningType.JoinCausing];
        const backward = [JoiningType.DualJoining, JoiningType.RightJoining, JoiningType.JoinCausing];
        const step = (state: Joining, character: string): Joining => {
            const point = character.codePointAt(0) ?? 0;
            const type = JoiningType.forChar(point);
            const transparent = type === JoiningType.Transparent;
            const member = scripts.hasScript(point, arabic);
            const joins = transparent || ((member || type === JoiningType.JoinCausing) && state.forward && backward.includes(type));
            return {
                offset: state.offset + character.length,
                ...(Option.isSome(state.open) && joins ? { runs: state.runs, open: state.open } : { runs: closed(state), open: member ? Option.some(state.offset) : Option.none() }),
                forward: transparent ? state.forward : forward.includes(type),
            };
        };
        const matches = Array.fromIterable(text.matchAll(spans));
        return Array.zipWith(matches, [0, ...matches.map(({ 0: content, index }) => index + content.length)], ({ 0: content, index }, previous) => {
            const runs = closed([...content].reduce<Joining>(step, { offset: index, runs: [], open: Option.none(), forward: false }));
            const gap = text.slice(previous, index);
            const spacesBefore = Array.countBy(gap, (unit) => unit === ' ');
            return new Word({ start: index, end: index + content.length, arabic: Array.isReadonlyArrayNonEmpty(runs), runs, spacesBefore, breakBefore: gap.length > spacesBefore });
        });
    });
    static readonly expand = (text: string, first: number, last: number): readonly [number, number] => {
        const before = Array.max(
            Array.map(separators, (separator) => text.lastIndexOf(separator, first)),
            Order.Number,
        );
        const after = Array.min(
            Array.map(separators, (separator) => {
                const found = text.indexOf(separator, last);
                return found < 0 ? text.length : found;
            }),
            Order.Number,
        );
        return [before > 0 ? before + 1 : 0, last === 0 || Array.contains(separators, text.charAt(last - 1)) ? last : after];
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const closed = ({ offset, runs, open }: Joining): readonly Span[] => [...runs, ...Option.toArray(Option.map(open, (start) => ({ start, end: offset })))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { Word };
