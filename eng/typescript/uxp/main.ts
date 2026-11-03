import { NodeRuntime, NodeServices } from '@effect/platform-node';
import { Manifest } from '@rasm/uxp/manifest';
import { Array, Cause, Config, Effect, FileSystem, Path, Schema, SchemaGetter, String } from 'effect';
import { Argument, Command } from 'effect/cli';
import { fromString, Plist } from '#/eng/typescript/parsed';
import { run } from '#/eng/typescript/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ADOBE = 'Library/Application Support/Adobe';
const NO_HOST_RELEASE = /(?<![\w-])-411(?!\w)/u;
const SECTION = /\n\s*\n/u;
const RULER = /[=]+/gu;

// --- [MODELS] --------------------------------------------------------------------------

const Listing = fromString('extension listing', (text) =>
    text
        .split(SECTION)
        .filter(String.isNonEmpty)
        .map((block) => String.split(block.trim(), '\n')),
).pipe(
    Schema.decodeTo(Schema.Array(Schema.TupleWithRest(Schema.Tuple([Schema.String, Schema.String, Schema.String]), [Schema.String]))),
    Schema.decodeTo(Schema.Unknown, {
        decode: SchemaGetter.transform((blocks) =>
            blocks.flatMap(([, names, ruler, ...rows]) => {
                const columns = Array.map(Array.fromIterable(ruler.matchAll(RULER)), ({ index, 0: span }) => [index, index + span.length] as const);
                const cells = (row: string) => columns.map(([start, end]) => row.slice(start, end).trim());
                return rows.map((row) => Object.fromEntries(Array.zip(cells(names), cells(row))));
            }),
        ),
        encode: SchemaGetter.forbiddenEncoding,
    }),
    Schema.decodeTo(Schema.Array(Schema.Struct({ 'Extension Name': Schema.String }))),
);
const Bundle = Plist.pipe(Schema.decodeTo(Schema.Struct({ CFBundleShortVersionString: Schema.String, CFBundleExecutable: Schema.String })));

// --- [OPERATIONS] ----------------------------------------------------------------------

const install = Effect.fn('install')(function* (built: string, agent: string) {
    const [fs, path, home] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Config.String('HOME')]);
    const [manifest, listing] = yield* Effect.all([fs.readFileString(path.join(built, Manifest.file)).pipe(Effect.flatMap(Schema.decodeEffect(Manifest.json))), run(agent, ['--list', 'all']).pipe(Effect.flatMap(Schema.decodeEffect(Listing)))], { concurrency: 'unbounded' });
    yield* listing.some((extension) => extension['Extension Name'] === manifest.name) ? run(agent, ['--remove', manifest.name]) : Effect.void;
    const installed = 'Installation Successful';
    yield* run(agent, ['--install', `${built}.ccx`]).pipe(
        Effect.catchTag('ExitError', ({ output }) => Effect.succeed(output)),
        Effect.filterOrFail(String.includes(installed), (answer) => new Cause.UnknownError(answer, NO_HOST_RELEASE.test(answer) ? `No installed ${manifest.host.app} release at ${manifest.host.minVersion} or later accepts ${manifest.name}` : `${path.basename(agent)} refused ${manifest.name}: ${answer.trim()}`)),
    );
    yield* fs.remove(path.join(home, ADOBE, 'CEP', 'extensions', manifest.id), { recursive: true, force: true });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

Effect.gen(function* () {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const bundle = path.join('/', ADOBE, 'Adobe Desktop Common', 'RemoteComponents', 'UPI', 'UnifiedPluginInstallerAgent', 'UnifiedPluginInstallerAgent.app', 'Contents');
    const { CFBundleShortVersionString, CFBundleExecutable } = yield* fs.readFileString(path.join(bundle, 'Info.plist')).pipe(Effect.flatMap(Schema.decodeEffect(Bundle)));
    return yield* Command.make('install', { built: Argument.Directory('built', { mustExist: true }) }, ({ built }) => install(built, path.join(bundle, 'MacOS', CFBundleExecutable))).pipe(Command.run({ version: CFBundleShortVersionString }));
}).pipe(Effect.provide(NodeServices.layer), NodeRuntime.runMain);
