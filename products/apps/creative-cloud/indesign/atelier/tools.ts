import { Server } from '@rasm/mcp';
import { Declaration } from '@rasm/object-model';
import { Unmatched } from '@rasm/search';
import type { Source } from '@rasm/typography';
import { Face, Kerning, Span, Word } from '@rasm/typography/shaping';
import { Array, Cause, Context, Effect, Equivalence, Number, Option, Order, Predicate, Record, Schema, Stream, Struct } from 'effect';
import { Tool } from 'effect/ai';
import type { RpcClient, RpcClientError, RpcGroup } from 'effect/rpc';
import { type InDesign, Kerns, Typeset } from '#/contract';

// --- [TYPES] ---------------------------------------------------------------------------

type Client = RpcClient.RpcClient.Flat<RpcGroup.Rpcs<typeof InDesign>, RpcClientError.RpcClientError>;
interface Placed {
    readonly text: string;
    readonly font: string;
    readonly axes: readonly number[];
    readonly start: number;
    readonly end: number;
}
type Font<E, R> = (name: string, characters: readonly string[]) => Effect.Effect<{ readonly source: Option.Option<typeof Source.Type> }, E, R>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const SAME_FACE = Equivalence.Struct({ font: Equivalence.String, axes: Array.makeEquivalence(Equivalence.Number) });
const MODES = { kern: { restores: false, shapes: true }, replace: { restores: true, shapes: true }, restore: { restores: true, shapes: false } } as const;

// --- [MODELS] --------------------------------------------------------------------------

const KerningTool = Tool.make('kerning', {
    description: 'Computes pair kerning and baseline shift from glyph outlines across text, story or frame widened to whole words, and writes them in one undo step',
    parameters: Schema.Struct({
        target: Kerns.fields.target,
        rules: Kerning.pipe(Schema.withDecodingDefaultTypeKey(Effect.succeed(Kerning.make({})))),
        mode: Schema.OptionFromOptionalKey(Schema.Literals(Record.keys(MODES)).annotate({ description: 'kern keeps manual kerning. replace restores metrics kerning across the words, then kerns. restore writes metrics kerning alone. Absent, a whole story kerns and text inside one replaces' })),
    }),
    success: Kerns.Result,
});
const TypesetTool = Tool.make('typeset', {
    description: 'Reads settings of text, story, frame or text defaults, or writes given settings in one undo step, turning off stylistic sets rival to one turned on and refusing kashida in a face with no tatweel advance and no jalt',
    parameters: Schema.Struct(Typeset.fields),
    success: Typeset.Result,
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const faced = <E, R>(font: Font<E, R>, located: readonly Placed[], extra: readonly string[]) =>
    Effect.forEach(
        Array.dedupeWith(located, SAME_FACE),
        Effect.fnUntraced(function* (first: Placed) {
            const { font: name, axes } = first;
            const group = located.filter((span) => SAME_FACE(first, span));
            const { source } = yield* font(name, [...group.map(Struct.get('text')), ...extra]);
            const subset = yield* Effect.fromOption(source, () => Unmatched.make({ subject: 'font file', query: Option.some(name) }));
            const face = yield* Face.open(subset, Face.Instance.make({ coordinates: axes }));
            return { font: name, face, placed: group.map((span) => ({ span, face })) };
        }),
    );
const kerned = <E, R>(client: Client, font: Font<E, R>) =>
    Effect.fnUntraced(function* ({ target, rules, mode }: Tool.Parameters<typeof KerningTool>) {
        const selection = yield* client('runs', { target });
        const text = selection.runs.map(Struct.get('text')).join('');
        const [first, last] = Word.expand(text, selection.start, selection.start + selection.length);
        const { restores, shapes } = MODES[Option.getOrElse(mode, () => (selection.length === text.length ? 'kern' : 'replace'))];
        const restore = Option.liftPredicate({ index: first, length: last - first }, ({ length }) => restores && length > 0);
        const faces = !shapes || first === last ? [] : yield* faced(font, Span.styled(selection.runs, first, last), []);
        const ordered = Array.sort(
            faces.flatMap(Struct.get('placed')),
            Order.mapInput(Number.Order, ({ span }: (typeof faces)[number]['placed'][number]) => span.start),
        );
        const rows = yield* Stream.fromIterable(ordered).pipe(
            Stream.map(({ span, face }) => ({ font: span.font, kerns: rules.kerns(face, span.text).map(Struct.evolve({ index: Number.sum(span.start) })) })),
            Server.progress(({ font: name }, at) => ({ progress: at + 1, total: ordered.length, message: name })),
            Stream.runCollect,
        );
        return yield* client('kerns', { target: selection.specifier, restore, kerns: rows.flatMap(Struct.get('kerns')) });
    });
const typeset = <E, R>(client: Client, font: Font<E, R>) =>
    Effect.fnUntraced(function* ({ target, settings }: Tool.Parameters<typeof TypesetTool>) {
        const elongating = Option.exists(Option.flatMapNullishOr(settings, Struct.get('kashida')), (kashida) => !Predicate.isTagged(kashida, 'None'));
        const tatweel = String.fromCodePoint(Face.TATWEEL);
        const faces = elongating || Option.isSome(Option.flatMapNullishOr(settings, Struct.get('features'))) ? yield* Effect.flatMap(client('runs', { target }), (selection) => faced(font, Span.styled(selection.runs, selection.start, selection.start + selection.length), elongating ? [tatweel] : [])) : [];
        const broken = elongating ? faces.filter(({ face }) => !face.kashida) : [];
        yield* Array.isReadonlyArrayEmpty(broken) ? Effect.void : new Cause.IllegalArgumentError(`${broken.map(Struct.get('font')).join(', ')} draw no ${tatweel} with positive advance and hold no jalt, so kashida breaks their joins`);
        return yield* client('typeset', { target, settings: Option.map(settings, Struct.evolve({ features: (features = {}) => faces.reduce((exclusive, { face }) => face.layout.exclusive(exclusive), features) })) });
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

const tools = {
    instructions: [
        'execute_script runs body in linked InDesign synchronously in one undo step that a throw rolls back',
        'execute_script runs awaited work of a linked InDesign body after the answer',
        'execute_script results encode InDesign enumerators by name, InDesign collections as {type, specifier, length}, and InDesign objects as {type, specifier, name, valid}',
    ],
    group: {
        tools: {
            inspect: { description: 'Reads object fields with declarations and typed view, or collection members, / for application. File fields read as paths', annotations: Context.make(Tool.Readonly, true) },
            change: { description: 'Writes object fields in one undo step, failures name every refused field by path' },
            call: { description: 'Calls object or collection method in one undo step, object_model lists parameters' },
            find_change: { description: 'Finds or changes matches in one undo step and restores search preferences, values take change forms' },
            invoke: { description: 'Invokes menu action in one undo step, menu lists actions by id, name, and area' },
            adjust: { description: 'Adds kerning, tracking, baseline shift or mark offsets to target text, / for text defaults' },
        },
        resources: {
            object_model: { description: 'Class and enumeration names of the linked application, or one declaration with its members', completion: { name: Declaration.match({ Index: ({ classes, enumerations }) => [...classes, ...enumerations], Class: ({ name }) => [name], Enumeration: ({ name }) => [name] }) } },
            menu: { description: 'Menu actions of one area or every area', completion: { area: (rows: readonly { readonly area: string }[]) => Array.dedupe(Array.map(rows, Struct.get('area'))) } },
            preflight: {
                description: 'Preflight rows per document, or the finished live results when no profile is given. Each read waits for the preflight process, which restarts with every live check',
                completion: { document: Array.map((process: { readonly targetObject: { readonly specifier: string } }) => process.targetObject.specifier) },
                report: Array.some<{ readonly aggregatedResults: readonly unknown[] }>(({ aggregatedResults }) => Array.isReadonlyArrayNonEmpty(aggregatedResults)),
            },
        },
    },
    kerning: { tag: 'kerns', tool: KerningTool, run: kerned },
    typeset: { tag: 'typeset', tool: TypesetTool, run: typeset },
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { tools };
