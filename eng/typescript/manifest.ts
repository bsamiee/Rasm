import { Effect, FileSystem, Path, Schema } from 'effect';
import { Toml } from '#/eng/typescript/parsed';

// --- [MODELS] --------------------------------------------------------------------------

const Dependencies = Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));
const Daemon = Schema.Struct({ run: Schema.OptionFromOptionalKey(Schema.String), port: Schema.OptionFromOptionalKey(Schema.Int), boot: Schema.Boolean.pipe(Schema.withDecodingDefaultKey(Effect.succeed(false))) }).pipe(Schema.encodeKeys({ boot: 'boot_start' }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const decodeFile = Effect.fnUntraced(function* <A>(schema: Schema.Codec<A, string>, ...segments: readonly string[]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    return yield* Effect.flatMap(fs.readFileString(path.join(...segments)), Schema.decodeEffect(schema));
});
const manifest = <A>(file: string, schema: Schema.Codec<A, string>) => ({ file, read: (directory: string) => decodeFile(schema, directory, file) });

// --- [MANIFESTS] -----------------------------------------------------------------------

const Package = manifest('package.json', Schema.fromJsonString(Schema.Struct({ name: Schema.String, description: Schema.String, dependencies: Dependencies, devDependencies: Dependencies })));
const Mise = manifest('mise.toml', Toml.pipe(Schema.decodeTo(Schema.Struct({ daemons: Schema.Record(Schema.String, Schema.Union([Schema.String, Daemon])) }))));

// --- [EXPORTS] -------------------------------------------------------------------------

export { decodeFile, Mise, Package };
