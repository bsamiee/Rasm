import { parse, patch } from '@decimalturn/toml-patch';
import type { Tree } from '@nx/devkit';
import { readNxJsonFromDisk } from '@nx/devkit/internal';
import { Mise, MiseLock, Package, Xcconfig } from '@rasm/eng';
import { Array, Cause, Effect, Function, Optic, Option, Schema } from 'effect';
import XMLBuilder from 'fast-xml-builder';
import { applyEdits } from 'jsonc-parser';
import { parseDocument, YAMLMap } from 'yaml';
import { decodeTree, syncGenerator, YAML_LAYOUT } from '#/sync/tree.ts';
import { openWorkspace, PYPROJECT, SWIFT_MANIFEST } from '#/workspace.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface MiseEnvironment {
    readonly env: { readonly MACOSX_DEPLOYMENT_TARGET: string };
}
interface PixiWorkspace {
    readonly tool: { readonly pixi: { readonly workspace: { readonly platforms: readonly (string | { readonly platform: string; readonly macos: string })[] } } };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const TOOLS_MINOR = /\..*$/u;
const XML = new XMLBuilder();

// --- [MODELS] --------------------------------------------------------------------------

const Argument = <A extends Schema.Top>(value: A) => Schema.Struct({ value }).pipe(Schema.encodeKeys({ value: '_0' }));
const Severity = Schema.Literals(['warning', 'error']);
const SwiftFlags = Schema.Struct({
    enableUpcomingFeature: Schema.optionalKey(Argument(Schema.String)),
    strictMemorySafety: Schema.optionalKey(Schema.Struct({})),
    treatAllWarnings: Schema.optionalKey(Argument(Severity)),
    treatWarning: Schema.optionalKey(Schema.Struct({ group: Schema.String, severity: Severity }).pipe(Schema.encodeKeys({ group: '_0', severity: '_1' }))),
    unsafeFlags: Schema.optionalKey(Argument(Schema.Array(Schema.String))),
});
const SwiftSetting = Schema.Struct({ tool: Schema.String, condition: Schema.optionalKey(Schema.Unknown), kind: Schema.StructWithRest(SwiftFlags, [Schema.Record(Schema.String, Schema.Unknown)]) });
const SwiftManifest = Schema.Struct({
    tools: Schema.Struct({ version: Schema.String }).pipe(Schema.encodeKeys({ version: '_version' })),
    platforms: Schema.Array(Schema.Struct({ name: Schema.String, version: Schema.String }).pipe(Schema.encodeKeys({ name: 'platformName' }))),
    targets: Schema.NonEmptyArray(Schema.Struct({ name: Schema.String, settings: Schema.Array(SwiftSetting) })),
}).pipe(Schema.encodeKeys({ tools: 'toolsVersion' }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const flags = (settings: readonly (typeof SwiftSetting.Type)[], compiler: string) =>
    settings
        .filter(({ tool }) => tool === compiler)
        .flatMap(({ kind }) => [
            ...(kind.enableUpcomingFeature === undefined ? [] : ['-enable-upcoming-feature', kind.enableUpcomingFeature.value]),
            ...(kind.strictMemorySafety === undefined ? [] : ['-strict-memory-safety']),
            ...(kind.treatAllWarnings === undefined ? [] : [kind.treatAllWarnings.value === 'error' ? '-warnings-as-errors' : '-no-warnings-as-errors']),
            ...(kind.treatWarning === undefined ? [] : [`-W${kind.treatWarning.severity}`, kind.treatWarning.group]),
            ...(kind.unsafeFlags?.value ?? []),
        ])
        .join(' ');
const written =
    (anchor: (key: string) => RegExp, content: (key: string, value: string) => string = (_, value) => value) =>
    (values: Readonly<Record<string, string>>) =>
    (text: string, file: string) =>
        Effect.validate(Object.entries(values), ([key, value]) =>
            Effect.succeed([...text.matchAll(anchor(RegExp.escape(key)))]).pipe(
                Effect.filterOrFail(Array.isArrayNonEmpty, () => new Cause.IllegalArgumentError(`${file}: found no ${key} setting`)),
                Effect.map(Array.map((row) => ({ offset: row.index, length: row[0].length, content: content(key, value) }))),
            ),
        ).pipe(Effect.map((edits) => applyEdits(text, edits.flat())));
const xcconfigRows = written((key) => new RegExp(`(?<=^${key} = ).*$`, 'gmu'));
const yamlRow = (parent: string, key: string, value: string) => (text: string, file: string) => {
    const document = parseDocument(text);
    return Option.fromNullishOr(document.value instanceof YAMLMap ? document.value.get(parent) : undefined).pipe(
        Option.filter((node) => node instanceof YAMLMap),
        Effect.fromOption,
        Effect.mapBoth({ onFailure: () => new Cause.IllegalArgumentError(`${file}: found no ${parent} mapping`), onSuccess: (mapping) => mapping.set(key, value) && document.toString(YAML_LAYOUT) }),
    );
};
const propertyRows = written(
    (tag) => new RegExp(`<${tag}>[^<]*</${tag}>`, 'gu'),
    (tag, value) => XML.build({ [tag]: value }),
);
const packageRows = written(
    (name) => new RegExp(`(?<=releases/download/v)[^/]+/${name}@[^#]+(?=#)`, 'gu'),
    (name, value) => `${value}/${name}@${value}`,
);
const tomlRows =
    <A>(update: (held: A) => A) =>
    (text: string) =>
        Effect.sync(() => patch(text, update(parse(text))));
const readSettings = Effect.fnUntraced(function* (tree: Tree) {
    const workspace = yield* openWorkspace({ workspaceRoot: tree.root, nxJsonConfiguration: readNxJsonFromDisk(tree.root) });
    const [manifest, { tools, platforms, targets }, locked] = yield* Effect.all([decodeTree(tree, Package.schema, Package.file), workspace.swiftManifest(SwiftManifest, 'dump-package'), decodeTree(tree, MiseLock.schema, MiseLock.file)]);
    const [author, license, repository, { version }] = yield* Effect.all([Effect.fromNullishOr(manifest.author), Effect.fromNullishOr(manifest.license), Effect.fromNullishOr(manifest.repository), Effect.fromNullishOr(platforms.find(({ name }) => name === 'macos'))]);
    const [first] = targets;
    yield* Effect.validate(targets, ({ name, settings }) => {
        const unsupported = settings.filter(({ tool, kind, condition }) => (tool === 'swift' || tool === 'linker') && (condition !== undefined || !Object.keys(kind).every(Object.hasOwn.bind(null, SwiftFlags.fields))));
        return unsupported.length === 0 ? Effect.void : Effect.fail(new Cause.IllegalArgumentError(`${name}: conditional or unsupported Xcode flags (${unsupported.flatMap(({ kind }) => Object.keys(kind)).join(', ')})`));
    });
    const [swift, linker] = [flags(first.settings, 'swift'), flags(first.settings, 'linker')];
    const divergent = targets.filter(({ settings }) => flags(settings, 'swift') !== swift || flags(settings, 'linker') !== linker);
    if (divergent.length > 0) {
        return yield* new Cause.IllegalArgumentError(`${divergent.map(({ name }) => name).join(', ')}: Xcode flags differ from ${first.name}`);
    }
    return { author, license, repository, version, swift, linker, tools: tools.version, hk: Array.headNonEmpty(locked.tools.hk).version };
});
const writers = {
    [Xcconfig.file]: ({ author, repository, version, linker, swift, tools }) =>
        xcconfigRows({
            COPYRIGHT_HOLDER: author,
            MACOSX_DEPLOYMENT_TARGET: version,
            OTHER_LDFLAGS: linker,
            OTHER_SWIFT_FLAGS: swift,
            SPARKLE_FEED_URL: `${repository.replace('//', '/$()/')}/releases/download/$(PRODUCT_NAME)-updates/appcast.xml`,
            SWIFT_VERSION: tools.replace(TOOLS_MINOR, '.0'),
        }),
    '.swiftlint.yml': ({ version }) => yamlRow('deployment_target', 'macOS_deployment_target', version),
    'Directory.Build.props': ({ author, license, repository }) => propertyRows({ Authors: author, PackageLicenseExpression: license, PackageProjectUrl: repository }),
    [Mise.file]: ({ version }) =>
        tomlRows(
            Optic.id<MiseEnvironment>()
                .key('env')
                .key('MACOSX_DEPLOYMENT_TARGET')
                .modify(() => version),
        ),
    '.config/hk.pkl': ({ hk }) => packageRows({ hk }),
    [PYPROJECT]: ({ version }) =>
        tomlRows(
            Optic.id<PixiWorkspace>()
                .key('tool')
                .key('pixi')
                .key('workspace')
                .key('platforms')
                .modify(Array.map((row) => Function.pipe(typeof row === 'string' ? row : row.platform, (platform) => (platform.startsWith('osx-') ? { platform, macos: version } : row)))),
        ),
} as const satisfies Record<string, (held: Effect.Success<ReturnType<typeof readSettings>>) => (text: string, file: string) => Effect.Effect<string, unknown>>;
const rows = Effect.fnUntraced(function* (tree: Tree) {
    const held = yield* readSettings(tree);
    return yield* Effect.validate(
        Object.entries(writers),
        Effect.fnUntraced(function* ([file, write]) {
            return { file, next: yield* write(held)(yield* Effect.fromNullishOr(tree.read(file, 'utf-8')), file) };
        }),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const buildSettings = Object.assign(syncGenerator(`Build settings differ from package.json, ${SWIFT_MANIFEST}, or ${MiseLock.file}`, rows), { files: [Package.file, SWIFT_MANIFEST, MiseLock.file, ...Object.keys(writers)] });

// --- [EXPORTS] -------------------------------------------------------------------------

export { buildSettings };
