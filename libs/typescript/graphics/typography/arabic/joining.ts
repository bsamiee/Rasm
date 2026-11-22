import { Array, Effect, Option, Schema, SchemaGetter, SchemaIssue } from 'effect';
import { GeneralCategory, JoiningGroup, JoiningType } from 'icu';
import { letters, Span } from '#/shaping/word.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface JoiningState {
    readonly offset: number;
    readonly joins: readonly Span[];
    readonly open: Option.Option<number>;
    readonly previous: number;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const UNJOINED = new Set([JoiningType.NonJoining, JoiningType.Transparent]);
const JOINS_FORWARD = new Set([JoiningType.DualJoining, JoiningType.LeftJoining, JoiningType.JoinCausing]);
const JOINS_BACKWARD = new Set([JoiningType.DualJoining, JoiningType.RightJoining, JoiningType.JoinCausing]);

// --- [MODELS] --------------------------------------------------------------------------

const JoiningGroupName = Schema.String.annotate({ description: 'ICU joining group name (Beh, FarsiYeh)' }).pipe(
    Schema.decodeTo(Schema.instanceOf(JoiningGroup), {
        decode: SchemaGetter.transformEffect((name: string, options) => Effect.fromOption(Option.fromNullOr(JoiningGroup.tryFromStr(name)), () => new SchemaIssue.InvalidValue({ message: `No joining group named ${name}` }, name, options))),
        encode: SchemaGetter.transform((group: JoiningGroup) => group.value),
    }),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const isDigit = (point: number) => GeneralCategory.forChar(point) === GeneralCategory.DecimalNumber;
const closedJoins = ({ offset, joins, open }: JoiningState): readonly Span[] => [...joins, ...Option.toArray(Option.map(open, (start) => Span.make({ start, end: offset })))];
const joiningStep = (state: JoiningState, character: string): JoiningState => {
    const point = character.codePointAt(0) ?? 0;
    const type = JoiningType.forChar(point);
    const transparent = type === JoiningType.Transparent;
    const joining = transparent || (isDigit(point) && isDigit(state.previous)) || (JOINS_FORWARD.has(JoiningType.forChar(state.previous)) && JOINS_BACKWARD.has(type));
    return {
        offset: state.offset + character.length,
        ...(Option.isSome(state.open) && joining ? { joins: state.joins, open: state.open } : { joins: closedJoins(state), open: UNJOINED.has(type) && !isDigit(point) ? Option.none() : Option.some(state.offset) }),
        previous: transparent ? state.previous : point,
    };
};
const joins = (text: string, word: Span) => closedJoins(Array.fromIterable(word.slice(text)).reduce<JoiningState>(joiningStep, { offset: word.start, joins: [], open: Option.none(), previous: 0 }));
const joiningGroups = (text: string) => letters(text).map(({ offset, point }) => ({ offset, group: JoiningGroup.forChar(point) }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { JoiningGroupName, joiningGroups, joins };
