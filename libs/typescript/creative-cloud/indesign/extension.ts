import { READING } from '@rasm/mcp/link';
import { listen } from '@rasm/mcp/server';
import { CHARACTERS, Font as Metrics, Options, read } from '@rasm/typography/font';
import { Array, Effect, FileSystem, Option, Schema } from 'effect';
import { Tool } from 'effect/ai';
import { Font, InDesign, PATH, Peer, Subset } from './contract.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const extension = Effect.fn('extension')(function* <F extends Schema.Struct.Fields>(product: string, route: { readonly fields: F; readonly select: (route: Schema.Struct.Type<F>) => Option.Option<(peer: typeof Peer.Type) => boolean> }) {
    const [link, files] = yield* Effect.all([listen(InDesign, { path: PATH, origin: 'file://', peer: `InDesign with the ${product} panel`, identity: Peer, route }), FileSystem.FileSystem]);
    const face = Tool.make('font', { parameters: Font.payloadSchema.pipe(Schema.fieldsAssign(Options.fields)), success: Font.successSchema.pipe(Schema.fieldsAssign({ metrics: Schema.OptionFromOptionalKey(Metrics) })) })
        .annotateMerge(READING)
        .annotateMerge(Font.annotations);
    return {
        peers: link.peers,
        publish: Effect.andThen(
            link.publish,
            link.tool(
                face,
                Effect.fnUntraced(function* ({ name, ...options }, call) {
                    const found = yield* call(Font, { name });
                    const own = yield* Effect.transposeOption(Option.map(found.installed, ({ location }) => files.exists(location)));
                    const text = [...Option.toArray(options.text), ...Object.values(CHARACTERS)].join('');
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
        ),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { extension };
