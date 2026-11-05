import { parse, patch } from '@decimalturn/toml-patch';
import type { Tree } from '@nx/devkit';
import { readNxJsonFromDisk } from '@nx/devkit/internal';
import { Array, Effect, Optic, Result, Schema, Struct } from 'effect';
import { Identity } from '#/eng/typescript/manifest';
import { decodeTree, syncGenerator } from '#/tools/nx/sync/tree';
import { openWorkspace, SWIFT_MANIFEST } from '#/tools/nx/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

interface MiseEnvironment {
    readonly env: { readonly MACOSX_DEPLOYMENT_TARGET: string };
}
interface PixiWorkspace {
    readonly tool: { readonly pixi: { readonly workspace: { readonly platforms: readonly (string | { readonly platform: string; readonly macos: string })[] } } };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const TOOLS_MINOR = /\..*$/u;
const PIXI_PLATFORM = 'osx-arm64';

// --- [MODELS] --------------------------------------------------------------------------

const Argument = <A extends Schema.Top>(value: A) => Schema.Struct({ value }).pipe(Schema.encodeKeys({ value: '_0' }));
const SwiftSetting = Schema.Struct({
    tool: Schema.String,
    kind: Schema.Struct({
        enableUpcomingFeature: Schema.optionalKey(Argument(Schema.String)),
        strictMemorySafety: Schema.optionalKey(Schema.Struct({})),
        treatAllWarnings: Schema.optionalKey(Argument(Schema.String)),
        treatWarning: Schema.optionalKey(Argument(Schema.String)),
        unsafeFlags: Schema.optionalKey(Argument(Schema.Array(Schema.String))),
    }),
});
const SwiftManifest = Schema.Struct({
    tools: Schema.Struct({ version: Schema.String }).pipe(Schema.encodeKeys({ version: '_version' })),
    platforms: Schema.Array(Schema.Struct({ name: Schema.String, version: Schema.String }).pipe(Schema.encodeKeys({ name: 'platformName' }))),
    targets: Schema.NonEmptyArray(Schema.Struct({ settings: Schema.Array(SwiftSetting) })),
}).pipe(Schema.encodeKeys({ tools: 'toolsVersion' }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const enabled = (held: boolean) => (held ? 'YES' : 'NO');
const written = (anchor: (key: string) => RegExp) => (values: Readonly<Record<string, string>>) => (text: string) => Object.entries(values).reduce((held, [key, value]) => held.replace(anchor(key), () => value), text);
const xcconfigRows = written((key) => new RegExp(`(?<=^${key} = ).*$`, 'mu'));
const yamlRows = written((key) => new RegExp(`(?<=^ *${key}: ).*$`, 'mu'));
const propertyRows = written((tag) => new RegExp(`(?<=<${tag}>)[^<]*(?=</${tag}>)`, 'u'));
const tomlRows =
    <A>(update: (held: A) => A) =>
    (text: string) =>
        patch(text, update(parse(text)));
const rows = Effect.fnUntraced(function* (tree: Tree) {
    const workspace = yield* openWorkspace({ workspaceRoot: tree.root, nxJsonConfiguration: readNxJsonFromDisk(tree.root) });
    const [{ author, repository }, { tools, platforms, targets }] = yield* Effect.all([decodeTree(tree, Identity.schema, Identity.file), workspace.cached(SwiftManifest, [SWIFT_MANIFEST], workspace.swiftPackage('dump-package'))]);
    const { version } = yield* Effect.fromOption(Array.findFirst(platforms, ({ name }) => name === 'macos'));
    const [{ settings }] = targets;
    const kinds = settings.map(Struct.get('kind'));
    const flags = (tool: string) => settings.filter((setting) => setting.tool === tool).flatMap(({ kind }) => kind.unsafeFlags?.value ?? []);
    const features = kinds.flatMap(({ enableUpcomingFeature }) => (enableUpcomingFeature === undefined ? [] : ['-enable-upcoming-feature', enableUpcomingFeature.value]));
    return yield* Effect.forEach(
        Object.entries({
            'Xcode.xcconfig': xcconfigRows({
                COPYRIGHT_HOLDER: author,
                MACOSX_DEPLOYMENT_TARGET: version,
                OTHER_LDFLAGS: flags('linker').join(' '),
                OTHER_SWIFT_FLAGS: [...features, ...flags('swift')].join(' '),
                SPARKLE_FEED_URL: `${repository.replace('//', '/$()/')}/releases/download/$(PRODUCT_NAME)-updates/appcast.xml`,
                SWIFT_STRICT_MEMORY_SAFETY: enabled(kinds.some(({ strictMemorySafety }) => strictMemorySafety !== undefined)),
                SWIFT_TREAT_WARNINGS_AS_ERRORS: enabled(kinds.some(({ treatAllWarnings }) => treatAllWarnings?.value === 'error')),
                SWIFT_VERSION: tools.version.replace(TOOLS_MINOR, '.0'),
                SWIFT_WARNINGS_AS_ERRORS_GROUPS: Array.filterMap(kinds, ({ treatWarning }) => (treatWarning === undefined ? Result.failVoid : Result.succeed(treatWarning.value))).join(' '),
            }),
            '.swiftlint.yml': yamlRows({ macOS_deployment_target: `'${version}'` }),
            'Directory.Build.props': propertyRows({ Authors: author, PackageProjectUrl: repository }),
            'mise.toml': tomlRows(
                Optic.id<MiseEnvironment>()
                    .key('env')
                    .key('MACOSX_DEPLOYMENT_TARGET')
                    .modify(() => version),
            ),
            'pyproject.toml': tomlRows(
                Optic.id<PixiWorkspace>()
                    .key('tool')
                    .key('pixi')
                    .key('workspace')
                    .key('platforms')
                    .modify(Array.map((row) => ((typeof row === 'string' ? row : row.platform) === PIXI_PLATFORM ? { platform: PIXI_PLATFORM, macos: version } : row))),
            ),
        }),
        Effect.fnUntraced(function* ([file, update]) {
            return { file, next: update(yield* Effect.fromNullishOr(tree.read(file, 'utf-8'))) };
        }),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const buildSettings = syncGenerator(`Build settings differ from package.json or ${SWIFT_MANIFEST}`, rows);

// --- [EXPORTS] -------------------------------------------------------------------------

export { buildSettings };
