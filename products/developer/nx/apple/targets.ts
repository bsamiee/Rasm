import { isAbsolute, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { type ProjectConfiguration, readJsonFile, type TargetConfiguration } from '@nx/devkit';
import type { PackageJson } from '@nx/devkit/internal';
import type { Verbs } from '@rasm/nx-distribution/inference';
import distribution from '@rasm/nx-distribution/package.json' with { type: 'json' };
import { Array, Cause, Effect, Schema, type Scope } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import type { Run } from '#/executors/run/executor.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Part {
    readonly root: string;
    readonly name: string;
    readonly file: string;
    readonly technologies: Array.NonEmptyReadonlyArray<string>;
    readonly tools: Verbs;
    readonly published?: TargetConfiguration;
    readonly release?: NonNullable<ProjectConfiguration['release']>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#/package.json'))).name;
const NOTARY = { key: 'ASC_API_KEY_P8', keyId: 'ASC_API_KEY_ID', issuer: 'ASC_API_ISSUER_ID' } as const;
const SIGNED = { development: 'Apple Development', distribution: 'Developer ID Application' } as const;
const APPCAST = { key: 'SPARKLE_ED_PRIVATE_KEY', bucket: 'SPARKLE_FEED_BUCKET' } as const;
const SWIFTPM = ['--force-resolved-versions', '--no-color-diagnostics'] as const;
const PLATFORM = { runtime: 'swift --version' } as const;
const INPUTS: NonNullable<TargetConfiguration['inputs']> = [PLATFORM, { externalDependencies: [] }];
const ATTACH: TargetConfiguration = {
    executor: `${distribution.name}:attach`,
    dependsOn: ['pack--ditto'],
    inputs: [{ dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
    metadata: { description: 'Uploads the files pack--ditto wrote to the GitHub release of the tag nx release made for the project at the checked-out commit with gh release upload', help: { command: 'gh release upload --help', example: { options: { dryRun: true } } } },
};

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({
    trustPackages: Schema.optionalKey(Schema.Boolean),
    outputPath: Schema.optionalKey(Schema.String),
    coveragePath: Schema.optionalKey(Schema.String),
    derivedDataPath: Schema.optionalKey(Schema.String),
}).annotate({
    description:
        'trustPackages: passes -skipMacroValidation and -skipPackagePluginValidation to xcodebuild. outputPath: workspace folder holding each scheme archive under <outputPath>/<scheme> and each product zip under <outputPath>/<product>, {projectName} naming the project, absent zips products in the SwiftPM release bin folder and gives no Xcode pack target, since xcodebuild archives outside the workspace. coveragePath: workspace folder the coverage configuration of a test target writes lcov.info to, exported with llvm-cov from the profile swift test or xcodebuild writes, {projectName} naming the project, absent leaves both tools their default profile and writes no lcov.info. derivedDataPath: workspace folder the coverage configuration of an xcodebuild test target passes as -derivedDataPath, so its profile and binaries sit at workspace paths, {projectName} naming the project, absent uses <outputPath>/DerivedData, and with neither leaves Xcode its per-user DerivedData and writes no lcov.info',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const analyzer = Effect.try({ try: () => fileURLToPath(import.meta.resolve('@rasm/nx-apple-darwin-arm64/nx-apple-analyzer')), catch: (cause) => new Cause.UnknownError(cause, `@rasm/nx-apple-darwin-arm64 of ${PLUGIN} resolves to no installed core`) });

const tokened = (at: string) => (isAbsolute(at) || at.startsWith('..') ? [] : [join('{workspaceRoot}', at)]);

const ran = (argv: Array.NonEmptyReadonlyArray<string>, options: Pick<ChildProcess.CommandOptions, 'cwd' | 'env'>) =>
    Effect.flatMap(ChildProcessSpawner.ChildProcessSpawner, (spawner) => spawner.exitCode(ChildProcess.make(Array.headNonEmpty(argv), Array.tailNonEmpty(argv), { ...options, extendEnv: true, detached: false, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' })));

const sequenced = (runs: (typeof Run.Type)['runs'], metadata: NonNullable<TargetConfiguration['metadata']>, { options, ...extra }: Partial<TargetConfiguration<Partial<typeof Run.Type>>> = {}): TargetConfiguration => ({
    executor: `${PLUGIN}:run`,
    options: { runs, ...options },
    metadata,
    ...extra,
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const run = <A, E>(effect: Effect.Effect<A, E, NodeServices | Scope.Scope>) => effect.pipe(Effect.scoped, Effect.provide(layer), Effect.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

export { APPCAST, ATTACH, analyzer, INPUTS, NOTARY, Options, type Part, PLUGIN, ran, run, SIGNED, SWIFTPM, sequenced, tokened };
