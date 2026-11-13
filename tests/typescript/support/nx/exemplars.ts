// --- [CONSTANTS] -----------------------------------------------------------------------

const README = {
    tasks: 'README.md TASKS',
    aggregates: 'README.md PRINCIPLES "Verbs aggregate <verb>--<tool> targets and read their inputs"',
    tagged: 'README.md PRINCIPLES "Plugins tag a project they give release configuration release"',
    modules: 'README.md STRUCTURE "Swift modules are root Package.swift targets, each in its own folder"',
    configures: 'README.md PRINCIPLES "@rasm/nx-cmake configures the build folder each configure preset states"',
    packages: 'README.md DELIVERY "C++ packages release as a CPack archive of their install tree"',
    attached: 'README.md TASKS "attach uploads the files a project packaging targets wrote to the GitHub release of its version", "Attach and publish targets depend on the targets that package their files"',
    cleaned: 'README.md TASKS "clean removes the outputs a project targets declare and runs its tool clean"',
    read: 'README.md PRINCIPLES "Task inputs hold the files a task tool reads: plugin and executor source, .config/mise.lock, tool --version output, and credentials are no input"',
} as const;
const NX = {
    detection: 'nrwl/nx e2e/dotnet/src/dotnet.test.ts:31-74',
    roots: 'nrwl/nx packages/nx/src/project-graph/plugins/public-api.ts:49-52',
    edges: 'nrwl/nx e2e/dotnet/src/dotnet.test.ts:201-216, e2e/dotnet/src/dotnet-dependency-graph.test.ts:108-134',
    owned: 'nrwl/nx packages/gradle/src/plugin/dependencies.spec.ts:59-78, 106-127',
    duplicate: 'nrwl/nx packages/nx/src/project-graph/error-types.ts MultipleProjectsWithSameNameError',
    inputs: 'nrwl/nx e2e/dotnet/src/dotnet-advanced-msbuild.test.ts:426-462',
    read: 'nrwl/nx astro-docs/src/content/docs/reference/inputs.mdoc:10',
    narrow: 'nrwl/nx e2e/gradle/src/gradle-task-dependencies.test.ts:186-195',
    cached: 'nrwl/nx astro-docs/src/content/docs/features/cache-task-results.mdoc:60-71, e2e/dotnet/src/dotnet-cache.test.ts:62-76',
    effects: 'nrwl/nx astro-docs/src/content/docs/features/cache-task-results.mdoc:73-77',
    stable: 'nrwl/nx astro-docs/src/content/docs/kb/performant-project-graph-plugins.mdoc:189-191',
    reused: 'nrwl/nx packages/dotnet/src/analyzer/analyzer-client.spec.ts:238-252, astro-docs/src/content/docs/kb/performant-project-graph-plugins.mdoc:50',
    reread: 'nrwl/nx packages/dotnet/src/analyzer/analyzer-client.spec.ts:254-270',
    partial: 'nrwl/nx packages/nx/src/project-graph/plugins/utils.spec.ts:132-185',
    affected: 'nrwl/nx astro-docs/src/content/docs/features/CI Features/affected.mdoc',
    restored: 'nrwl/nx e2e/dotnet/src/dotnet-cache.test.ts:62-76',
    cleaned: 'nrwl/nx e2e/dotnet/src/dotnet-cache.test.ts:423-435',
    many: 'nrwl/nx e2e/dotnet/src/dotnet-cache.test.ts:330-347',
    mixed: 'nrwl/nx e2e/dotnet/src/dotnet-cache.test.ts:174-207',
    tested: 'nrwl/nx e2e/dotnet/src/dotnet.test.ts:124-128, e2e/dotnet/src/dotnet-cache.test.ts:229-239',
    packed: 'nrwl/nx e2e/dotnet/src/dotnet.test.ts:169-176',
    dependent: 'nrwl/nx e2e/gradle/src/gradle-task-dependencies.test.ts:160-184',
    unrelated: 'nrwl/nx e2e/gradle/src/gradle-task-dependencies.test.ts:186-195',
    unread: 'nrwl/nx e2e/gradle/src/gradle-cache.test.ts:73-76',
    batch: 'nrwl/nx e2e/gradle/src/gradle.test.ts:587-605',
    results: 'nrwl/nx packages/gradle/src/executors/gradle/gradle-batch.impl.spec.ts:351-420',
    disk: 'nrwl/nx packages/nx/src/command-line/release/version/resolve-current-version.ts:100, version/test-utils.ts:152-164',
    written: 'nrwl/nx packages/nx/src/command-line/release/version/test-utils.ts:181-202',
    dependents: 'nrwl/nx packages/nx/src/command-line/release/version/release-group-processor.spec.ts:763-773',
    lock: 'nrwl/nx e2e/release/src/lock-file-updates.test.ts, astro-docs/src/content/docs/kb/publish-rust-crates.mdoc:147',
    layout: 'nrwl/nx packages/js/src/release/version-actions.spec.ts:111-145, 187-208',
    registry: 'nrwl/nx packages/nx/src/command-line/release/version/resolve-current-version.ts:153-185, version/version-actions.ts:401-423',
    publish: 'nrwl/nx packages/nx/src/utils/package-json.ts:301-322, packages/nx/src/utils/package-json.spec.ts:205-250',
    private: 'nrwl/nx e2e/release/src/private-js-packages.test.ts "should skip private packages"',
    published: 'nrwl/nx astro-docs/src/content/docs/kb/publish-rust-crates.mdoc:185-204, e2e/release/src/release-publishable-libraries.test.ts:124-147',
} as const;
const VERBS = ['codegen', 'build', 'pack', 'attach', 'check', 'fix', 'test', 'serve', 'preview', 'install', 'deploy', 'upgrade', 'clean'];

// --- [EXPORTS] -------------------------------------------------------------------------

export { NX, README, VERBS };
