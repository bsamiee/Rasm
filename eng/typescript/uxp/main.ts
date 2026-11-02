import { runMain } from '@effect/platform-node/NodeRuntime';
import { layer } from '@effect/platform-node/NodeServices';
import { Manifest } from '@rasm/uxp/manifest';
import { Array, Cause, Config, Effect, FileSystem, Path, Schema, SchemaGetter, String } from 'effect';
import { Argument, Command } from 'effect/cli';
import { output, stdout } from '#/eng/typescript/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ADOBE = 'Library/Application Support/Adobe';
const INSTALLED = 'Installation Successful';
const UNMATCHED = '-411';

// --- [MODELS] --------------------------------------------------------------------------

const Listing = Schema.String.pipe(
    Schema.decodeTo(Schema.Unknown, {
        decode: SchemaGetter.transform((text: string) =>
            Array.map(text.split(/\n\s*\n/u).filter(String.isNonEmpty), (block) => {
                const [header = '', , , ...rows] = block.trim().split('\n');
                return { product: header.replace(/^\d+ extensions? installed for /u, ''), extensions: Array.map(rows, (row) => row.trim().split(/ {2,}/u)) };
            }),
        ),
        encode: SchemaGetter.forbiddenEncoding,
    }),
    Schema.decodeTo(Schema.Array(Schema.Struct({ product: Schema.String, extensions: Schema.Array(Schema.Tuple([Schema.String, Schema.String, Schema.String])) }))),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const install = Effect.fn('install')(function* (built: string, agent: string) {
    const [fs, path, home] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.String('HOME')]);
    const [manifest, listing] = yield* Effect.all([fs.readFileString(path.join(built, 'manifest.json')).pipe(Effect.flatMap(Schema.decodeEffect(Manifest.json))), stdout(agent, ['--list', 'all']).pipe(Effect.flatMap(Schema.decodeEffect(Listing)))], { concurrency: 'unbounded' });
    const installed = listing.some(({ extensions }) => extensions.some(([, name]) => name === manifest.name));
    yield* installed ? stdout(agent, ['--remove', manifest.name]) : Effect.void;
    const { text } = yield* output(agent, ['--install', `${built}.ccx`]);
    yield* Effect.filterOrFail(Effect.succeed(text), String.includes(INSTALLED), (answer) => new Cause.UnknownError(undefined, answer.includes(UNMATCHED) ? `No installed ${manifest.host.app} release at ${manifest.host.minVersion} or later accepts ${manifest.name}` : `Adobe plugin installer refused ${manifest.name}. ${answer.trim()}`));
    yield* fs.remove(path.join(home, ADOBE, 'CEP', 'extensions', manifest.id), { recursive: true, force: true });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const path = yield* Path.Path;
    const bundle = path.join('/', ADOBE, 'Adobe Desktop Common', 'RemoteComponents', 'UPI', 'UnifiedPluginInstallerAgent', 'UnifiedPluginInstallerAgent.app', 'Contents');
    const { CFBundleShortVersionString, CFBundleExecutable } = yield* stdout('plutil', ['-convert', 'json', '-o', '-', path.join(bundle, 'Info.plist')]).pipe(Effect.flatMap(Schema.decodeEffect(Schema.fromJsonString(Schema.Struct({ CFBundleShortVersionString: Schema.String, CFBundleExecutable: Schema.String })))));
    return yield* Command.make('install', { built: Argument.Directory('built', { mustExist: true }) }, ({ built }) => install(built, path.join(bundle, 'MacOS', CFBundleExecutable))).pipe(Command.run({ version: CFBundleShortVersionString }));
}).pipe(Effect.provide(layer), runMain);
