import { stripTypeScriptTypes } from 'node:module';
import { listen } from '@rasm/mcp/link';
import { Progress, READING } from '@rasm/mcp/server';
import { CHARACTERS, Font as Metrics, Options, read } from '@rasm/typography/font';
import { Array, Duration, Effect, Equal, FileSystem, Formatter, Option, Schedule, Schema, SchemaIssue, Stream } from 'effect';
import { Tool } from 'effect/ai';
import { Enter, Font, InDesign, PATH, Peer, Subset, Transpile } from './contract.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const extension = Effect.fn('extension')(function* <F extends Readonly<Record<string, Schema.Codec<unknown, unknown>>>>(product: string, route: { readonly fields: F; readonly select: (route: Schema.Struct.Type<F>) => Option.Option<(peer: typeof Peer.Type) => boolean> }) {
    const peer = `InDesign with the ${product} panel`;
    const quiet = Duration.seconds(2);
    const told = {
        Held: `An open menu, dialog, or drag in ${peer} holds the call before it starts`,
        Unanswered: `${peer} sent no answer for ${Duration.toSeconds(quiet)} s while a dialog the call opened, or long work, holds the call. The call finishes once released`,
        Entered: `${peer} runs the call`,
    };
    const [link, files] = yield* Effect.all([
        listen(InDesign, {
            path: PATH,
            origin: 'file://',
            peer,
            identity: Peer,
            route,
            watch: (sent, call) =>
                Stream.fromSchedule(Schedule.min([Schedule.exponential(Duration.millis(2)), Schedule.spaced(Duration.seconds(1))])).pipe(
                    Stream.mapEffect(() => Effect.timeoutOrElse(call(Enter, {}), { duration: quiet, orElse: () => Effect.succeed('Unanswered' as const) }), { concurrency: 'unbounded', unordered: true }),
                    Stream.mapAccum(
                        () => Option.none<keyof typeof told>(),
                        (last, phase) => {
                            const stage = ({ Held: Option.some('Held'), Unanswered: Option.some('Unanswered'), Entered: Option.as(last, 'Entered'), Idle: last } satisfies Record<typeof phase, Option.Option<keyof typeof told>>)[phase];
                            return [stage, Equal.equals(stage, last) ? [] : Option.toArray(stage)] as const;
                        },
                    ),
                    Stream.runForEach((stage) => Effect.flatMap(Progress, (report) => report(told[stage]))),
                    Effect.ignore,
                    Effect.forkChild,
                    Effect.andThen(sent),
                ),
        }),
        FileSystem.FileSystem,
    ]);
    const face = Tool.make('font', { parameters: Font.payloadSchema.pipe(Schema.fieldsAssign(Options.fields)), success: Font.successSchema.pipe(Schema.fieldsAssign({ metrics: Schema.OptionFromOptionalKey(Metrics) })) })
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
                    const found = yield* call(Font, { name });
                    const own = yield* Effect.transposeOption(Option.map(found.installed, ({ location }) => files.exists(location)));
                    const text = [options.text, ...Object.values(CHARACTERS)].join('');
                    const file = yield* Effect.transposeOption(Option.map(Option.all({ installed: found.installed, own }), ({ installed, own: readable }) => (readable ? Effect.succeed(installed.location) : call(Subset, { postscriptName: installed.postscriptName, text }))));
                    const held = Option.filter(
                        Option.flatMap(found.installed, (installed) => Option.liftPredicate(installed.coordinates, Array.isReadonlyArrayNonEmpty)),
                        () => Option.contains(own, true),
                    );
                    const coordinates = Option.orElse(options.coordinates, () => held);
                    const metrics = yield* Effect.transposeOption(Option.map(Option.all({ installed: found.installed, file }), ({ installed, file: path }) => read(path, installed.postscriptName, { ...options, coordinates })));
                    return { ...found, metrics };
                }),
            ),
        ).pipe(Effect.provideService(Transpile, (code) => Effect.try({ try: () => stripTypeScriptTypes(`${open}${code}${close}`, { mode: 'strip' }).slice(open.length, -close.length), catch: (cause) => new SchemaIssue.InvalidValue({ message: Formatter.format(cause) }, code) }))),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { extension };
