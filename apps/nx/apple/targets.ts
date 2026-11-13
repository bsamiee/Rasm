import { isAbsolute, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import type { TargetConfiguration } from '@nx/devkit';
import { ToolError } from '@rasm/nx-devkit/process';
import { type Members, PLATFORM, type Stated } from '@rasm/nx-devkit/project';
import { Effect, type Option, Schema } from 'effect';
import type { Run } from '#/executors/run/executor';

// --- [TYPES] ---------------------------------------------------------------------------

interface Part extends Pick<Stated, 'release' | 'packaged' | 'publish'> {
    readonly root: string;
    readonly name: string;
    readonly file: string;
    readonly technologies: readonly string[];
    readonly clean: string;
    readonly members: Members;
}
type Settings = typeof Options.Type & { readonly placed: (tool: string, name: string) => Option.Option<string> };

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLUGIN = '@rasm/nx-apple';
const SHORT_VERSION = 'CFBundleShortVersionString';
const PACKAGES = ['.dmg', '.zip'] as const;
const GIT_STATUS = { GIT_CONFIG_COUNT: '1', GIT_CONFIG_KEY_0: 'status.branch', GIT_CONFIG_VALUE_0: 'false' } as const;
const INPUTS = [PLATFORM, { env: 'DEVELOPER_DIR' }, { env: 'TOOLCHAINS' }] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ trustPackages: Schema.optionalKey(Schema.Boolean) }).annotate({ description: 'trustPackages: passes -skipMacroValidation and -skipPackagePluginValidation to xcodebuild' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const analyzer = Effect.try({ try: () => fileURLToPath(import.meta.resolve('#analyzer')), catch: ToolError.of('#analyzer', 'does not resolve') });

const tokened = (at: string) => (isAbsolute(at) || at.startsWith('..') ? [] : [join('{workspaceRoot}', at)]);

const target = (runs: (typeof Run.Type)['runs'], metadata: NonNullable<TargetConfiguration['metadata']>, { options, ...extra }: Partial<TargetConfiguration> = {}): TargetConfiguration => ({
    executor: `${PLUGIN}:run`,
    options: { runs, env: GIT_STATUS, ...options },
    metadata,
    ...extra,
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { analyzer, GIT_STATUS, INPUTS, Options, PACKAGES, type Part, PLUGIN, type Settings, SHORT_VERSION, target, tokened };
