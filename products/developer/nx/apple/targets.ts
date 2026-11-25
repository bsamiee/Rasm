import { isAbsolute, join } from 'node:path';
import { arch, platform } from 'node:process';
import { fileURLToPath } from 'node:url';
import type { ProjectConfiguration, TargetConfiguration } from '@nx/devkit';
import { EXECUTORS, inside, PLATFORM, type Verbs } from '@rasm/nx';
import { type Array, Cause, Effect, FileSystem, Schema } from 'effect';
import type { Run } from '#/executors/run/executor';
import plugin from '#/package.json' with { type: 'json' };

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

const PLUGIN = plugin.name;
const CORE = `${PLUGIN}-${platform}-${arch}`;
const BUILDER = 'mise deps install nx-apple';
const NOTARY = { key: 'ASC_API_KEY_P8', keyId: 'ASC_API_KEY_ID', issuer: 'ASC_API_ISSUER_ID' } as const;
const SIGNED = { development: 'Apple Development', distribution: 'Developer ID Application' } as const;
const APPCAST = { key: 'SPARKLE_ED_PRIVATE_KEY', bucket: 'SPARKLE_FEED_BUCKET' } as const;
const SWIFTPM = ['--force-resolved-versions', '--disable-index-store'] as const;
const INPUTS: NonNullable<TargetConfiguration['inputs']> = [{ runtime: 'swift -print-target-info | plutil -extract compilerVersion raw - && xcrun --show-sdk-build-version' }, PLATFORM, { externalDependencies: [] }];
const ATTACH: TargetConfiguration = {
    executor: EXECUTORS.attach,
    dependsOn: ['pack--ditto'],
    inputs: [{ dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
    metadata: {
        description: 'Uploads the files pack--ditto wrote to the GitHub release of the tag nx release made for the project at the checked-out commit with gh release upload, creating that release with gh release create where none exists',
        help: { command: 'gh release upload --help', example: { options: { dryRun: true } } },
    },
};

// --- [MODELS] --------------------------------------------------------------------------

class Options extends Schema.Opaque<Options, unknown>()(
    Schema.Struct({
        outputPath: Schema.optionalKey(Schema.String),
        coveragePath: Schema.optionalKey(Schema.String),
        derivedDataPath: Schema.optionalKey(Schema.String),
    }).annotate({
        description:
            'outputPath: workspace folder holding each scheme archive under <outputPath>/<scheme> and each product zip under <outputPath>/<product>, {projectName} naming the project, absent zips products in the SwiftPM release bin folder and gives no Xcode pack target, since xcodebuild archives outside the workspace. coveragePath: workspace folder the coverage configuration of a test target writes lcov.info to, exported with llvm-cov from the profile swift test or xcodebuild writes, {projectName} naming the project, absent leaves both tools their default profile and writes no lcov.info. derivedDataPath: workspace folder the coverage configuration of an xcodebuild test target passes as -derivedDataPath, so its profile and binaries sit at workspace paths, {projectName} naming the project, absent uses <outputPath>/DerivedData, and with neither leaves Xcode its per-user DerivedData and writes no lcov.info',
    }),
) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const analyzer = Effect.try({ try: () => fileURLToPath(import.meta.resolve(`${CORE}/nx-apple-analyzer`)), catch: (cause) => new Cause.UnknownError(cause, `${PLUGIN} resolves no ${CORE} core on this host`) }).pipe(
    Effect.tap((binary) => FileSystem.FileSystem.use((fs) => fs.access(binary))),
    Effect.catchReason('PlatformError', 'NotFound', (cause) => new Cause.UnknownError(cause, `the ${CORE} core is absent, ${BUILDER} builds it`)),
);

const tokened = (at: string) => (!isAbsolute(at) && inside('.')(at) ? [join('{workspaceRoot}', at)] : []);

const sequenced = (runs: Run['runs'], metadata: NonNullable<TargetConfiguration['metadata']>, { options, ...extra }: Partial<TargetConfiguration<Partial<Run>>> = {}): TargetConfiguration => ({
    executor: `${PLUGIN}:run`,
    options: { runs, ...options },
    metadata,
    ...extra,
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { APPCAST, ATTACH, analyzer, BUILDER, CORE, INPUTS, NOTARY, Options, type Part, PLUGIN, SIGNED, SWIFTPM, sequenced, tokened };
