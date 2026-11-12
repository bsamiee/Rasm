import { extname } from 'node:path';
import type { TargetConfiguration } from '@nx/devkit';
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
    readonly release?: typeof VersionActionsOptions.Type;
    readonly packaged?: readonly string[];
    readonly publish?: TargetConfiguration;
}
type Settings = typeof Options.Type & { readonly placed: (tool: string, name: string) => Option.Option<string> };

// --- [CONSTANTS] -----------------------------------------------------------------------

const SHORT_VERSION = 'CFBundleShortVersionString';
const PACKAGES = ['.dmg', '.zip'] as const;
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

export { analyzer, GIT_STATUS, Options, PACKAGES, type Part, type Settings, SHORT_VERSION, target, tokened, VersionActionsOptions, versioned };
