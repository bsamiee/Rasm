import { Array, Cause, Effect, FileSystem, Option, Path, Record, Schema } from 'effect';
import { TextFormat } from '#/eng/typescript/text-format';

// --- [CONSTANTS] -----------------------------------------------------------------------

const Postgres = {
    image: 'postgres:19rc1',
    environment: { POSTGRES_INITDB_ARGS: '--locale-provider=builtin --builtin-locale=PG_UNICODE_FAST' },
    command: ['postgres', '-c', 'shared_preload_libraries=pg_stat_statements'],
};
const VITEST_INTEGRATION_TESTS = '**/*.integration.test.{ts,tsx}';
const XCODE_REQUIREMENT = 'up-to-next-major-version';

// --- [MODELS] --------------------------------------------------------------------------

const Strings = Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));
const Daemon = Schema.Struct({ run: Schema.OptionFromOptionalKey(Schema.String), boot: Schema.Boolean.pipe(Schema.withDecodingDefaultKey(Effect.succeed(false))) }).pipe(Schema.encodeKeys({ boot: 'boot_start' }));
const XcodeRemote = Schema.Struct({ kind: Schema.Literal('remote'), repository: Schema.String, version: Schema.StructWithRest(Schema.Struct({ [XCODE_REQUIREMENT]: Schema.optionalKey(Schema.String) }), [Schema.Record(Schema.String, Schema.Json)]) });
const AstGrepLanguage = Schema.Struct({ libraryPath: Schema.String, extensions: Schema.Array(Schema.String), languageSymbol: Schema.optionalKey(Schema.String), expandoChar: Schema.optionalKey(Schema.String), outlineRules: Schema.OptionFromOptionalKey(Schema.String) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const decodeFile = Effect.fnUntraced(function* <A>(schema: Schema.Codec<A, string>, ...segments: readonly string[]) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    return yield* Effect.flatMap(fs.readFileString(path.join(...segments)), Schema.decodeEffect(schema));
});
const manifest = <A>(file: string, schema: Schema.Codec<A, string>) => ({ file, schema, read: (directory: string) => decodeFile(schema, directory, file) });
const libraryRoot = ({ sideEffects, bin, exports }: typeof Package.schema.Type) => Option.filter(Record.get(exports, '.'), () => Option.contains(sideEffects, false) && Option.isNone(bin));
const extendScriptEntry = ({ exports }: typeof Package.schema.Type) => Record.get(exports, './extendscript');
const suitePath = (path: Path.Path, base: string, directory: string) => (directory === base || directory.startsWith(`${base}${path.sep}`) ? directory : path.join(base, ...directory.split(path.sep).filter((segment, index) => index !== 1 || segment !== path.basename(base))));
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
            description: Schema.String,
            dependencies: Strings,
            devDependencies: Strings,
            exports: Strings,
            imports: Strings,
            sideEffects: Schema.OptionFromOptionalKey(Schema.Union([Schema.Boolean, Schema.Array(Schema.String)])),
            bin: Schema.OptionFromOptionalKey(Schema.Union([Schema.String, Schema.Record(Schema.String, Schema.String)])),
        }),
    ),
);
const Identity = manifest(Package.file, Schema.fromJsonString(Schema.Struct({ author: Schema.String, repository: Schema.String })));
const Installed = manifest(Package.file, Schema.fromJsonString(Schema.Struct({ name: Schema.String, dependencies: Strings, optionalDependencies: Strings, peerDependencies: Strings, napi: Schema.OptionFromOptionalKey(Schema.Unknown) })));
const ClaudeHooks = manifest('hooks/hooks.json', Schema.fromJsonString(Schema.Struct({ modules: Schema.NonEmptyArray(Schema.String) })));
const Mise = manifest('.config/mise.toml', TextFormat.Toml.pipe(Schema.decodeTo(Schema.Struct({ daemons: Schema.Record(Schema.String, Schema.Union([Schema.String, Daemon])) }))));
const Workspace = manifest('pnpm-workspace.yaml', TextFormat.Yaml.pipe(Schema.decodeTo(Schema.Struct({ packages: Schema.Array(Schema.String) }))));
const Xcode = manifest(
    'project.xcproj',
    TextFormat.Json5.pipe(
        Schema.decodeTo(
            Schema.Struct({
                files: Schema.Array(Schema.Struct({ path: Schema.OptionFromOptionalKey(Schema.String) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
                packages: Schema.Array(Schema.Union([XcodeRemote, Schema.Struct({ kind: Schema.Literal('local'), path: Schema.String })])).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
                targets: Schema.Array(
                    Schema.Struct({
                        name: Schema.String,
                        members: Schema.Array(Schema.Struct({ package: Schema.String, product: Schema.String, phase: Schema.Struct({ 'build-phase': Schema.Literal('frameworks') }) }).pipe(Schema.encodeKeys({ product: 'product-name', phase: 'build-phase' }))).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
                        settings: Schema.Struct({ PRODUCT_BUNDLE_IDENTIFIER: Schema.OptionFromOptionalKey(Schema.String), CODE_SIGN_ENTITLEMENTS: Schema.optionalKey(Schema.String), INFOPLIST_FILE: Schema.optionalKey(Schema.String) }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
                    }).pipe(Schema.encodeKeys({ members: 'package-product-members', settings: 'build-settings' })),
                ).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
            }),
        ),
    ),
);
const XcodeProject = {
    ...Xcode,
    bundleIdentifier: Effect.fnUntraced(function* (directory: string) {
        const [{ targets }, path] = yield* Effect.all([Xcode.read(directory), Path.Path]);
        const target = path.parse(directory).name;
        return yield* Effect.fromOption(
            Option.flatMap(
                Array.findFirst(targets, ({ name }) => name === target),
                ({ settings }) => settings.PRODUCT_BUNDLE_IDENTIFIER,
            ),
            () => new Cause.IllegalArgumentError(`${directory} target ${target} has no PRODUCT_BUNDLE_IDENTIFIER`),
        );
    }),
};
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
    outlineCatalogs: (path: Path.Path, languages: Readonly<Record<string, typeof AstGrepLanguage.Type>>) => Array.getSomes(Record.collect(languages, (language, { outlineRules }) => Option.map(outlineRules, (file) => ({ language, file, source: path.join(path.dirname(file), 'sources', path.basename(file)) })))),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { ClaudeHooks, decodeFile, extendScriptEntry, Identity, Installed, libraryRoot, Mise, members, Package, Postgres, Sgconfig, suitePath, VITEST_INTEGRATION_TESTS, Workspace, XCODE_REQUIREMENT, XcodeProject, XcodeRemote };
