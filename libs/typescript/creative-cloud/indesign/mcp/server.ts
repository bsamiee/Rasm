import { listen } from '@rasm/mcp/link';
import { Progress, READING } from '@rasm/mcp/server';
import { CHARACTERS, Face, Instance, Shaped, Text } from '@rasm/typography/face';
import { transformSync } from 'amaro';
import { Duration, Effect, Formatter, Option, type Record, Schedule, Schema, SchemaIssue, Stream, Struct } from 'effect';
import { type McpSchema, Tool } from 'effect/ai';
import { Transpile } from '../models.ts';
import { Enter, Font, InDesign, PATH, Peer, WIRE } from './protocol.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const extension = Effect.fn('extension')(function* <Route, Encoded>(
    product: string,
    route: {
        readonly schema: Schema.Codec<Route, Encoded> & { readonly fields: Readonly<Record<string, Schema.Codec<unknown, unknown>>> };
        readonly select: (route: Route, peers: readonly (typeof Peer.Type)[]) => Option.Option<(peer: typeof Peer.Type) => boolean>;
        readonly completion: Readonly<Record<string, (peers: readonly (typeof Peer.Type)[], input: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) => readonly string[]>>;
    },
    trailer: (route: Route, peer: typeof Peer.Type) => Effect.Effect<McpSchema.ContentBlock, Error>,
) {
    const peer = `InDesign with the ${product} panel`;
    const quiet = Duration.seconds(2);
    const link = yield* listen(InDesign, {
        path: PATH,
        origin: 'file://',
        peer,
        identity: Peer,
        serialization: WIRE,
        route,
        trailer,
        watch: (sent, call) =>
            Stream.fromSchedule(Schedule.min([Schedule.exponential(Duration.millis(2)), Schedule.spaced(Duration.seconds(1))])).pipe(
                Stream.mapEffect(() => Effect.timeoutOption(call(Enter, {}), quiet), { concurrency: 'unbounded', unordered: true }),
                Stream.filter((phase) => !Option.contains(phase, 'Idle')),
                Stream.changes,
                Stream.runForEach((phase) =>
                    Effect.flatMap(Progress, (report) =>
                        report(
                            `${peer}: ${Option.match(phase, {
                                onNone: () => `awaiting an answer for ${Duration.toSeconds(quiet)} s; a dialog opened by the call or long work can hold it until released`,
                                onSome: (stage) => (stage === 'Held' ? 'waiting for an open menu, dialog, or drag before the call starts' : 'running the call'),
                            })}`,
                        ),
                    ),
                ),
                Effect.ignore,
                Effect.forkChild,
                Effect.andThen(sent),
            ),
    });
    const face = Tool.make('font', {
        parameters: Font.payloadSchema.pipe(Schema.fieldsAssign({ ...Instance.fields, text: Text })),
        success: Font.successSchema.pipe(Schema.fieldsAssign({ installed: Schema.OptionFromOptionalKey(Font.successSchema.fields.installed.from.schema.mapFields(Struct.omit(['data'])).pipe(Schema.fieldsAssign({ face: Face, shaped: Shaped }))) })),
    })
        .annotateMerge(READING)
        .annotateMerge(Font.annotations);
    const [open, close] = ['async function body() {\n', '\n}'];
    return {
        peers: link.peers,
        publish: Effect.andThen(
            link.publish,
            link.tool(
                face,
                Effect.fnUntraced(function* ({ name, ...options }, call) {
                    const found = yield* call(Font, { name, text: [options.text, ...Object.values(CHARACTERS)].join('') });
                    const installed = yield* Effect.transposeOption(
                        Option.map(
                            found.installed,
                            Effect.fnUntraced(function* ({ data, ...font }) {
                                const coordinates = Option.firstSomeOf([options.coordinates, font.subset ? Option.none() : font.coordinates]);
                                const typeface = yield* Face.open(data, font.postscriptName, { coordinates, size: options.size });
                                return { ...font, face: typeface, shaped: yield* typeface.shape(options.text) };
                            }),
                        ),
                    );
                    return { ...found, installed };
                }),
            ),
        ).pipe(Effect.provideService(Transpile, (code) => Effect.try({ try: () => transformSync(`${open}${code}${close}`, { mode: 'strip-only' }).code.slice(open.length, -close.length), catch: (cause) => new SchemaIssue.InvalidValue({ message: Formatter.format(cause) }, code) }))),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { extension };
