import { stripped } from '@rasm/automation';
import { listen } from '@rasm/mcp/link';
import { Progress, READING } from '@rasm/mcp/server';
import { Face, Instance, Run, Shaped } from '@rasm/typography/face';
import { Duration, Effect, Formatter, Option, type Record, Schedule, Schema, SchemaAST, SchemaGetter, SchemaIssue, Stream } from 'effect';
import { type McpSchema, McpServer, Tool } from 'effect/ai';
import { FontView } from '../fonts.ts';
import { Script } from '../scripts.ts';
import { Invoke } from '../session.ts';
import { Enter, Font, InDesign, Invocation, PATH, Peer, Scripted, WIRE } from './contract.ts';

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
    const server = yield* McpServer.McpServer;
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
                Effect.andThen(Effect.tap(sent, (value) => Effect.forEach(Option.toArray(Schema.decodeUnknownOption(Schema.Struct({ preflight: Schema.NonEmptyArray(Schema.Unknown) }))(value)), () => server.notifications['notifications/resources/updated']({ uri: 'indesign://preflight' }), { discard: true }))),
            ),
    });
    const script = Tool.make(Scripted._tag, {
        description: [...Option.toArray(Option.fromUndefinedOr(SchemaAST.resolveDescription(Script.ast))), 'TypeScript is stripped before the source runs.'].join(' '),
        parameters: Schema.Struct({
            ...Script.fields,
            code: Schema.String.annotate({ description: 'JavaScript or TypeScript function body' }).pipe(
                Schema.decodeTo(Schema.String, { decode: SchemaGetter.transformEffect((code: string) => Effect.mapError(stripped(code), (cause) => new SchemaIssue.InvalidValue({ message: Formatter.format(cause) }, code))), encode: SchemaGetter.passthrough() }),
            ),
        }),
        success: Script.Result,
    });
    const face = Tool.make(Font._tag, {
        parameters: Font.payloadSchema.pipe(Schema.fieldsAssign({ ...Instance.fields, ...Run.fields })),
        success: Schema.Struct({ ...FontView.fields, face: Schema.OptionFromOptionalKey(Face), shaped: Schema.OptionFromOptionalKey(Shaped) }),
    }).annotateMerge(READING);
    return {
        peers: link.peers,
        invoke: (chosen: Route, action: typeof Invoke.Type.action) => Effect.flatMap(link.call(chosen), (use) => use(Invocation, Invoke.make({ action }))),
        publish: Effect.all(
            [
                link.publish,
                link.tool(script, (payload, call) => call(Scripted, Script.make(payload))),
                link.tool(
                    face,
                    Effect.fnUntraced(function* ({ name, text, features, coordinates, size }, call) {
                        const found = yield* call(Font, { name, text });
                        const opened = yield* Effect.transposeOption(
                            Option.map(found.installed, ({ source, postscriptName, coordinates: own }) => Face.open(source.data, source.subset || Option.isSome(own) ? Option.none() : Option.some(postscriptName), { coordinates: Option.firstSomeOf([coordinates, source.subset ? Option.none() : own]), size })),
                        );
                        return { ...found, installed: Option.map(found.installed, ({ source, ...installed }) => installed), face: opened, shaped: yield* Effect.transposeOption(Option.map(opened, (typeface) => typeface.shape({ text, features }))) };
                    }),
                ),
            ],
            { discard: true },
        ),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { extension };
