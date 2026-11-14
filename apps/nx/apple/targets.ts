import { isAbsolute, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import type { ProjectConfiguration, TargetConfiguration } from '@nx/devkit';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Cause, Effect, Schema } from 'effect';
import type { Run } from '#/executors/run/executor';

// --- [TYPES] ---------------------------------------------------------------------------

interface Part {
    readonly root: string;
    readonly name: string;
    readonly file: string;
    readonly technologies: readonly string[];
    readonly targets: Readonly<Record<string, TargetConfiguration>>;
    readonly release?: NonNullable<ProjectConfiguration['release']>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLUGIN = '@rasm/nx-apple';
const SHARED = distribution.name;
const GIT_STATUS = { GIT_CONFIG_COUNT: '1', GIT_CONFIG_KEY_0: 'status.branch', GIT_CONFIG_VALUE_0: 'false' } as const;
const INPUTS: NonNullable<TargetConfiguration['inputs']> = [{ runtime: 'node -p process.platform+process.arch' }, { env: 'DEVELOPER_DIR' }, { env: 'TOOLCHAINS' }, { externalDependencies: [] }];
const ATTACH: TargetConfiguration = {
    executor: `${SHARED}:attach`,
    dependsOn: ['pack--ditto'],
    metadata: { description: "Uploads the files pack--ditto wrote to the GitHub release of the version the project's manifest states through gh release upload", help: { command: 'gh release upload --help', example: { options: { dryRun: true } } } },
};

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ trustPackages: Schema.optionalKey(Schema.Boolean), outputPath: Schema.optionalKey(Schema.String) }).annotate({
    description: 'trustPackages: passes -skipMacroValidation and -skipPackagePluginValidation to xcodebuild. outputPath: workspace folder holding archives and their packages under <outputPath>/xcode/<project>/<scheme> and product zips under <outputPath>/swiftpm/<product>, absent gives no pack--ditto target',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const refused = (subject: string, detail: string) => (cause?: unknown) => new Cause.UnknownError(cause, `${subject}: ${detail}`);

const analyzer = Effect.try({ try: () => fileURLToPath(import.meta.resolve('#analyzer')), catch: refused('#analyzer', 'does not resolve') });

const tokened = (at: string) => (isAbsolute(at) || at.startsWith('..') ? [] : [join('{workspaceRoot}', at)]);

const target = (runs: (typeof Run.Type)['runs'], metadata: NonNullable<TargetConfiguration['metadata']>, { options, ...extra }: Partial<TargetConfiguration> = {}): TargetConfiguration => ({
    executor: `${PLUGIN}:run`,
    options: { runs, env: GIT_STATUS, ...options },
    metadata,
    ...extra,
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { ATTACH, analyzer, GIT_STATUS, INPUTS, Options, type Part, PLUGIN, refused, SHARED, target, tokened };
