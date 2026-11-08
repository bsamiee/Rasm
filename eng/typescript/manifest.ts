import { Array, Effect, FileSystem, Option, Path, Record, Schema } from 'effect';
import { TextFormat } from '#/text-format.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const EXTENDSCRIPT_TYPES = 'types-for-adobe';

// --- [MODELS] --------------------------------------------------------------------------

const Strings = Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));
const Daemon = Schema.Struct({ run: Schema.OptionFromOptionalKey(Schema.String), boot: Schema.Boolean.pipe(Schema.withDecodingDefaultKey(Effect.succeed(false))) }).pipe(Schema.encodeKeys({ boot: 'boot_start' }));
const AstGrepLanguage = Schema.Struct({ libraryPath: Schema.String, extensions: Schema.Array(Schema.String), languageSymbol: Schema.optionalKey(Schema.String), expandoChar: Schema.optionalKey(Schema.String), outlineRules: Schema.OptionFromOptionalKey(Schema.String) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const decodeFile = Effect.fnUntraced(function* <A>(schema: Schema.Codec<A, string>, ...segments: readonly string[]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    return yield* Effect.flatMap(fs.readFileString(path.join(...segments)), Schema.decodeEffect(schema));
});
const manifest = <A>(file: string, schema: Schema.Codec<A, string>) => ({ file, schema, read: (directory: string) => decodeFile(schema, directory, file) });
const librarySurface = ({ sideEffects, bin, exports, devDependencies }: typeof Package.schema.Type) =>
    Option.map(
        Option.filter(Record.pop(exports, '.'), () => Option.contains(sideEffects, false) && Option.isNone(bin)),
        ([root, others]) => ({ root, entries: Object.values(others), extendscript: Record.has(devDependencies, EXTENDSCRIPT_TYPES) ? Object.values(others) : [] }),
    );
const members = Effect.fnUntraced(function* (root: string, file: string) {
    const [fs, path, { packages }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Workspace.read(root)]);
    return Array.flatten(yield* Effect.forEach(packages, (glob) => fs.glob(path.join(glob, file), { root, exclude: ['**/node_modules/**'] }), { concurrency: 'unbounded' }));
});

// --- [MANIFESTS] -----------------------------------------------------------------------

const Package = manifest(
    'package.json',
    Schema.fromJsonString(
        Schema.Struct({
            name: Schema.String,
            author: Schema.optionalKey(Schema.String),
            license: Schema.optionalKey(Schema.String),
            repository: Schema.optionalKey(Schema.String),
            dependencies: Strings,
            devDependencies: Strings,
            exports: Strings,
            imports: Strings,
            sideEffects: Schema.OptionFromOptionalKey(Schema.Union([Schema.Boolean, Schema.Array(Schema.String)])),
            bin: Schema.OptionFromOptionalKey(Schema.Union([Schema.String, Schema.Record(Schema.String, Schema.String)])),
        }),
    ),
);
const Installed = manifest(Package.file, Schema.fromJsonString(Schema.Struct({ name: Schema.String, dependencies: Strings, optionalDependencies: Strings, peerDependencies: Strings, napi: Schema.OptionFromOptionalKey(Schema.Unknown) })));
const ClaudeHooks = manifest('hooks/hooks.json', Schema.fromJsonString(Schema.Struct({ modules: Schema.NonEmptyArray(Schema.String) })));
const Mise = {
    ...manifest('.config/mise.toml', TextFormat.Toml.pipe(Schema.decodeTo(Schema.Struct({ daemons: Schema.Record(Schema.String, Schema.Union([Schema.String, Daemon])) })))),
    declared: Schema.Array(Schema.Struct({ name: Schema.String, source: Schema.NullOr(Schema.String) })),
};
const MiseLock = manifest('.config/mise.lock', TextFormat.Toml.pipe(Schema.decodeTo(Schema.Struct({ tools: Schema.Record(Schema.Literals(['hk']), Schema.NonEmptyArray(Schema.Struct({ version: Schema.String }))) }))));
const Workspace = manifest('pnpm-workspace.yaml', TextFormat.Yaml.pipe(Schema.decodeTo(Schema.Struct({ packages: Schema.Array(Schema.String) }))));
const Sgconfig = {
    ...manifest(
        'sgconfig.yml',
        TextFormat.Yaml.pipe(
            Schema.decodeTo(
                Schema.Struct({
                    ruleDirs: Schema.Array(Schema.String),
                    utilDirs: Schema.Array(Schema.String),
                    customLanguages: Schema.Record(Schema.String, AstGrepLanguage),
                    languageGlobs: Schema.Record(Schema.String, Schema.Array(Schema.String)),
                }),
            ),
        ),
    ),
    outline: 'tools/ast-grep/outline',
    outlineCatalogs: (path: Path.Path, languages: Readonly<Record<string, typeof AstGrepLanguage.Type>>) => Array.getSomes(Record.collect(languages, (language, { outlineRules }) => Option.map(outlineRules, (file) => ({ language, file, source: path.join(Sgconfig.outline, path.basename(file)) })))),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { ClaudeHooks, decodeFile, Installed, librarySurface, Mise, MiseLock, members, Package, Sgconfig, Workspace };
