import { extname } from 'node:path';
import type { ProjectConfiguration, TargetConfiguration } from '@nx/devkit';
import type { Identity, Members } from '@rasm/nx-devkit/plugin';
import { printed, ToolError } from '@rasm/nx-devkit/process';
import { Effect, type Option, Path, Schema } from 'effect';
import type { Run } from '#/executors/run/executor';

// --- [TYPES] ---------------------------------------------------------------------------

interface Part {
    readonly root: string;
    readonly name: string;
    readonly file: string;
    readonly technologies: readonly string[];
    readonly clean: string;
    readonly members: Members;
    readonly published?: TargetConfiguration;
    readonly release?: NonNullable<ProjectConfiguration['release']>;
}
type Settings = typeof Options.Type & { readonly placed: (tool: string, name: string) => Option.Option<string> };

// --- [CONSTANTS] -----------------------------------------------------------------------

const SHORT_VERSION = 'CFBundleShortVersionString';
const GIT_STATUS = { GIT_CONFIG_COUNT: '1', GIT_CONFIG_KEY_0: 'status.branch', GIT_CONFIG_VALUE_0: 'false' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ trustPackages: Schema.optionalKey(Schema.Boolean) }).annotate({ description: 'trustPackages: passes -skipMacroValidation and -skipPackagePluginValidation to xcodebuild' });
const VersionActionsOptions = Schema.Struct({ manifest: Schema.String });
const Kind = Schema.Literals(['.plist', '.xcproj']);
const Versioned = Schema.fromJsonString(Schema.Struct({ version: Schema.NullOr(Schema.String), project: Schema.String }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const analyzer = Effect.gen(function* () {
    const path = yield* Path.Path;
    return yield* path.fromFileUrl(yield* Effect.try({ try: () => new URL(import.meta.resolve('#analyzer')), catch: ToolError.of('#analyzer', 'does not resolve') }));
});

const tokened = (path: Path.Path, at: string) => (path.isAbsolute(at) || at.startsWith('..') ? [] : [path.join('{workspaceRoot}', at)]);

const published = (plugin: Identity, technology: string, folders: readonly string[]): TargetConfiguration => ({
    executor: `${plugin.name}:release-publish`,
    dependsOn: ['^nx-release-publish'],
    inputs: plugin.inputs,
    options: { folders },
    metadata: {
        technologies: [technology],
        description: "Attaches the zip or disk image the release build wrote to the project's GitHub release, then adds each notarized disk image of an app naming SUFeedURL to its Sparkle feed where SPARKLE_ED_PRIVATE_KEY is set",
        help: { command: 'gh release upload --help', example: { options: { dryRun: true } } },
    },
});

const EDITORS = {
    '.plist': {
        command: Effect.succeed('plutil'),
        args: (newVersion?: string) => (newVersion === undefined ? ['-extract', SHORT_VERSION, 'raw', '-o', '-', '-'] : ['-replace', SHORT_VERSION, '-string', newVersion, '-o', '-', '-']),
        read: (output: string, newVersion?: string) => Effect.succeed({ version: newVersion ?? output.trim(), project: output }),
    },
    '.xcproj': {
        command: analyzer,
        args: (newVersion?: string) => ['version', ...(newVersion === undefined ? [] : ['--marketing-version', newVersion])],
        read: (output: string) => Schema.decodeEffect(Versioned)(output),
    },
} as const satisfies Record<typeof Kind.Type, unknown>;

const versioned = Effect.fnUntraced(function* (root: string, at: string, text: string, newVersion?: string) {
    const editor = EDITORS[yield* Effect.mapError(Schema.decodeUnknownEffect(Kind)(extname(at)), ToolError.of(at, `is no ${Kind.literals.join(' or ')} manifest`))];
    return yield* editor.read(yield* printed(yield* editor.command, [...editor.args(newVersion)], { cwd: root, input: text }), newVersion);
});

const target = (plugin: Identity, runs: (typeof Run.Type)['runs'], metadata: NonNullable<TargetConfiguration['metadata']>, { options, ...extra }: Partial<TargetConfiguration> = {}): TargetConfiguration => ({
    executor: `${plugin.name}:run`,
    options: { runs, env: GIT_STATUS, ...options },
    metadata,
    ...extra,
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { analyzer, GIT_STATUS, Options, type Part, published, type Settings, SHORT_VERSION, target, tokened, VersionActionsOptions, versioned };
