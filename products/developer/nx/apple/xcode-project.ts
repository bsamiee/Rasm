import { basename, dirname, extname, join, relative } from 'node:path';
import type { TargetConfiguration } from '@nx/devkit';
import { EXECUTORS, inside, removable, type Verbs } from '@rasm/nx';
import { Array, Option, Predicate, Record, Schema, Struct } from 'effect';
import type { Run } from '#/executors/run/executor';
import { pruneExclusions, RESOLVED } from '#/swift-package';
import { APPCAST, ATTACH, INPUTS, NOTARY, type Options, type Part, PLUGIN, sequenced, tokened } from '#/targets';

// --- [MODELS] --------------------------------------------------------------------------

const Container = Schema.Struct({
    path: Schema.String,
    schemes: Schema.Array(
        Schema.Struct({
            name: Schema.String,
            containers: Schema.Array(Schema.String),
            plans: Schema.Array(Schema.String),
            tests: Schema.Boolean,
            launch: Schema.optionalKey(Schema.String),
            archive: Schema.optionalKey(Schema.Struct({ configuration: Schema.String, bundles: Schema.NonEmptyArray(Schema.String), feed: Schema.Boolean.annotate({ description: 'Whether an archived application states SUFeedURL in its Info.plist, the feed Sparkle checks for updates' }) })),
        }),
    ),
    configurations: Schema.Array(Schema.String),
    inputs: Schema.Array(Schema.String),
    projects: Schema.Array(Schema.String),
    products: Schema.Record(Schema.String, Schema.Array(Schema.String)),
    buildFiles: Schema.Array(Schema.String),
});
class Analyzed extends Schema.Opaque<Analyzed, unknown>()(Schema.fromJsonString(Schema.Struct({ containers: Schema.Array(Container), failures: Schema.Record(Schema.String, Schema.String) }))) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const xcodeParts = (options: Options, file: string, container: Analyzed['containers'][number], name: string, inputs: readonly string[], built: string, packaged: (at: string) => Option.Option<string>, data: string): Part => {
    const workspace = extname(built) === '.xcworkspace';
    const lock = join(built, workspace ? 'xcshareddata/swiftpm/Package.resolved' : 'project.xcworkspace/xcshareddata/swiftpm/Package.resolved');
    const flags = [workspace ? '-workspace' : '-project', built, '-skipMacroValidation', '-skipPackagePluginValidation'];
    const cloned = join(data, 'xcode-packages', container.path);
    const invoke = (action: string, scheme: string, extra: readonly string[]) => ['xcodebuild', action, ...flags, '-onlyUsePackageVersionsFromResolvedFile', '-scheme', scheme, ...extra] as const;
    const schemes = container.schemes.filter(({ containers }) => containers.length > 0);
    const listing = extname(container.path) === '.xcworkspace';
    const root = listing ? container.path : dirname(container.path);
    const locals = Array.dedupe(Array.getSomes(container.projects.map(packaged)));
    const seed = Array.findFirst(container.projects, (at) => Option.as(packaged(at), join(at, RESOLVED)));
    const archives = schemes.flatMap(({ name: scheme, archive }) => Array.fromNullishOr(archive).map((held) => ({ scheme, ...held })));
    const archived = Array.cartesianWith(Array.fromNullishOr(options.outputPath), archives, (at, { scheme, configuration, feed }) => {
        const folder = join(at, scheme);
        const archive = join(folder, `${scheme}.xcarchive`);
        return { feed, folder, archive, base: inside(root)(folder) ? join('{projectRoot}', relative(root, folder)) : join('{workspaceRoot}', folder), run: invoke('archive', scheme, ['-archivePath', archive, '-configuration', configuration]) };
    });
    const referenced = container.buildFiles.flatMap((at) => tokened(at).flatMap((input) => [input, join(input, '**/*'), ...[at, join(at, '**/*')].map((output) => ({ dependentTasksOutputFiles: output }))]));
    const read = pruneExclusions(Array.dedupe(['{projectRoot}/**/*', ...inputs, ...referenced, ...[lock, ...Option.toArray(seed)].flatMap(tokened), ...(workspace ? tokened(join(built, 'contents.xcworkspacedata')) : []), { runtime: 'xcodebuild -version' }, ...INPUTS]));
    const released = extname(dirname(file)) === '.xcodeproj' && Array.isReadonlyArrayNonEmpty(archived);
    const fed = released ? archived.filter(Struct.get('feed')) : [];
    const runs = schemes.filter(({ tests }) => tests).flatMap((scheme) => (scheme.plans.length === 0 ? [[]] : scheme.plans.map((plan) => ['-testPlan', plan])).map((planned) => invoke('test', scheme.name, planned)));
    const builds: Run['runs'] = schemes.map((scheme) => invoke('build', scheme.name, []));
    const installs = archives.flatMap(({ scheme, configuration }) => [
        invoke('archive', scheme, ['-configuration', configuration]),
        [
            'sh',
            '-c',
            `set -eo pipefail
changed=$("$@" | jq -r '.[].buildSettings | select(.PRODUCT_TYPE == "com.apple.product-type.application" and .SKIP_INSTALL != "YES") | .HOME, .PRODUCT_BUNDLE_IDENTIFIER' | while read -r home && read -r identifier; do
    ledger="$home/Library/Group Containers/group.com.apple.replayd/ScreenCaptureApprovals.plist"
    key=$(printf %s "$identifier" | sed 's/\\./\\\\./g')
    mkdir -p "\${ledger%/*}"
    [ -f "$ledger" ] || plutil -create xml1 "$ledger"
    plutil -extract "$key" xml1 -o /dev/null "$ledger" 2>/dev/null || plutil -insert "$key" -dictionary "$ledger"
    for field in kScreenCaptureApprovalLastAlerted kScreenCapturePrivacyHintDate; do
        [ "$(plutil -extract "$key.$field" raw -o - "$ledger" 2>/dev/null)" = 4000-01-01T00:00:00Z ] || { plutil -replace "$key.$field" -date 4000-01-01T00:00:00Z "$ledger"; [ "$home" != "$HOME" ] || echo "$identifier"; }
    done
done)
[ -z "$changed" ] || ! pgrep -qx replayd || killall -HUP replayd`,
            'sh',
            ...invoke('-showBuildSettings', scheme, ['-configuration', configuration, '-json']),
        ] as const,
    ]);
    const launches = Array.dedupe(schemes.flatMap(({ launch }) => Array.fromNullishOr(launch)));
    const modes = (planned: Run['runs']): Partial<TargetConfiguration> => ({
        configurations: Record.fromIterableWith(container.configurations, (configuration) => [configuration.toLowerCase(), { runs: planned.map((run) => [...run, '-configuration', configuration]) }]),
        ...(Predicate.isTupleOf(launches, 1) && { defaultConfiguration: launches[0].toLowerCase() }),
    });
    const purged = Array.findFirst(schemes, ({ containers }) => containers.some((at) => Option.isNone(packaged(at))));
    const work: Verbs = {
        ...(schemes.length > 0 && {
            build: {
                xcodebuild: sequenced(
                    builds,
                    {
                        technologies: ['xcode'],
                        description: 'Builds each shared scheme with xcodebuild build for the destination xcodebuild picks on this Mac, as app builds do, so a package scheme reuses the modules app builds compiled, pack--ditto archiving the schemes with installed products',
                        help: { command: 'xcodebuild -help', example: { args: ['-quiet'] } },
                    },
                    { cache: true, parallelism: false, ...(!listing && { dependsOn: ['^build--*'] }), inputs: read, ...modes(builds) },
                ),
            },
            check: { xcodebuild: { dependsOn: ['build--xcodebuild'], inputs: read, metadata: { technologies: ['xcode'], description: 'Type-checks each shared scheme with xcodebuild build', help: { command: 'xcodebuild -help', example: { args: ['-quiet'] } } } } },
            upgrade: {
                xcodebuild: sequenced(
                    [
                        ['rm', '-rf', join(cloned, 'workspace-state.json'), join(cloned, 'checkouts'), ...(Option.isNone(seed) ? [lock] : [])],
                        ...Option.toArray(Option.map(seed, (from) => ['ln', '-sf', relative(dirname(lock), from), lock] as const)),
                        ...Option.toArray(Option.map(Array.head(schemes), ({ name: scheme }) => ['xcodebuild', '-resolvePackageDependencies', ...flags, '-clonedSourcePackagesDirPath', cloned, '-scheme', scheme] as const)),
                    ],
                    {
                        technologies: ['xcode'],
                        description: `Removes the checkouts and state of ${name}'s upgrade package folder, keeping its git mirrors, links ${name}'s Package.resolved to the one of the local package it names, which upgrade--swift-package of that package moves, or removes it where ${name} names none, then resolves its package references with xcodebuild -resolvePackageDependencies, which keeps each pin their requirements allow and so resolves the versions the local package resolved, else the newest releases their requirements allow`,
                        help: { command: 'xcodebuild -help', example: { args: ['-quiet'] } },
                    },
                    { inputs: read, ...(locals.length > 0 && { dependsOn: [{ projects: locals, target: 'upgrade--swift-package', params: 'forward' }] }) },
                ),
            },
        }),
        ...(Array.isReadonlyArrayNonEmpty(archived) && {
            pack: {
                ditto: {
                    executor: `${PLUGIN}:package`,
                    cache: true,
                    parallelism: false,
                    dependsOn: ['^build--*'],
                    inputs: [...read, { env: NOTARY.keyId }],
                    options: { exports: archived.map(({ archive, folder, run }) => ({ archive, folder, run })) },
                    outputs: archived.flatMap(({ base, archive }) => ['*.dmg', '*.zip', basename(archive)].map((entry) => join(base, entry))),
                    metadata: {
                        technologies: ['xcode'],
                        description: `Archives each app with installed products with xcodebuild archive, then zips it with ditto, or where ${NOTARY.key}, ${NOTARY.keyId}, and ${NOTARY.issuer} are set exports it under the Developer ID Application identity into a disk image that is signed, notarized, and stapled`,
                        help: { command: 'man ditto', example: { args: ['--norsrc'] } },
                    },
                },
            },
        }),
        ...(runs.length > 0 && {
            test: {
                xcodebuild: sequenced(
                    runs,
                    {
                        technologies: ['xcode'],
                        description: 'Tests each test plan of a shared scheme with xcodebuild',
                        help: { command: 'xcodebuild -help', example: { args: ['-parallel-testing-enabled', 'YES'] } },
                    },
                    { cache: true, parallelism: false, dependsOn: ['build--xcodebuild'], inputs: read, ...modes(runs) },
                ),
            },
        }),
        ...(installs.length > 0 && {
            install: {
                xcodebuild: sequenced(
                    installs,
                    {
                        technologies: ['xcode'],
                        description:
                            "Builds each shared scheme archiving an application that installs, under -configuration set to the scheme's archive configuration, with xcodebuild archive into a temporary archive, then swaps each application of the archive's Products over the one at its INSTALL_PATH, the LOCAL_APPS_DIR xcodebuild evaluates on this Mac, only once every scheme built, signed, and archived, after each install writing every installed application's PRODUCT_BUNDLE_IDENTIFIER row into replayd's ScreenCaptureApprovals.plist under the HOME xcodebuild -showBuildSettings states, both alert dates at 4000-01-01, and signaling a running replayd to read it where that HOME is the HOME replayd runs under, so the application's first screen capture raises no alert",
                        help: { command: 'xcodebuild -help', example: { args: ['-jobs', '4'] } },
                    },
                    { parallelism: false, dependsOn: ['^build--*'], inputs: read, options: { applications: Array.dedupe(archives.flatMap(({ bundles }) => bundles)) } },
                ),
            },
        }),
        ...(released && { attach: { gh: ATTACH } }),
    };
    const removed = removable(work);
    const tools: Verbs = {
        ...work,
        ...(schemes.length > 0 && {
            clean: {
                xcodebuild: {
                    executor: EXECUTORS.clean,
                    parallelism: false,
                    inputs: read,
                    options: { commands: [...schemes.map(({ name: scheme }) => invoke('clean', scheme, [])), ...archived.map(({ archive }) => ['rm', '-rf', archive])], cwd: '.', targets: removed },
                    configurations: { cache: { commands: Option.toArray(Option.map(purged, ({ name: scheme }) => ['sh', '-c', 'set -eo pipefail; root=$("$@" | jq -er ".[0].buildSettings.BUILD_ROOT"); rm -rf "$(dirname "$(dirname "$root")")"', 'sh', ...invoke('-showBuildSettings', scheme, ['-json'])])) } },
                    metadata: {
                        technologies: ['xcode'],
                        description: `Removes the build products of each shared scheme with xcodebuild clean, then each archive, and under configuration cache removes the per-user DerivedData folder of ${name}, two folders above the BUILD_ROOT xcodebuild -showBuildSettings -json prints`,
                        help: { command: 'xcodebuild -help', example: { args: ['-quiet'] } },
                    },
                },
            },
        }),
    };
    return {
        root,
        name,
        file,
        technologies: ['xcode'],
        tools,
        ...(Array.isReadonlyArrayNonEmpty(fed) && {
            published: {
                executor: `${PLUGIN}:release-publish`,
                dependsOn: ['pack--ditto', '^nx-release-publish'],
                inputs: [{ dependentTasksOutputFiles: '**/*' }, { externalDependencies: [] }],
                options: { exports: fed.map(({ archive }) => ({ archive })) },
                metadata: {
                    technologies: ['xcode'],
                    description: `Signs the Sparkle feed of each disk image pack--ditto wrote for an application stating SUFeedURL with generate_appcast under ${APPCAST.key} and uploads it to the R2 bucket ${APPCAST.bucket} names with cf r2 objects put, skipping where either is unset or no disk image exists`,
                    help: { command: 'generate_appcast --help', example: { options: { dryRun: true } } },
                },
            },
        }),
        ...(released && { release: { version: { versionActions: `${PLUGIN}/release/version-actions`, versionActionsOptions: { manifest: relative(root, file) } } } }),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Analyzed, xcodeParts };
