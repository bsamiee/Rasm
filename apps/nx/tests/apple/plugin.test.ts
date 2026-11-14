import { layer } from '@effect/platform-node/NodeServices';
import { assert, it } from '@effect/vitest';
import plugin from '@rasm/nx-apple/package.json' with { type: 'json' };
import { conforms } from '@rasm/testing/conformance';
import { Consumer, Packed } from '@rasm/testing/nx';
import { Array, Duration, Effect, Layer, Path, Record, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPORT = 'swift package describe --type json; xcodebuild -list -json -project <project>; xcodebuild -showBuildSettings -json';
const [PACKAGE, PROJECT, OUTPUT] = ['Package.swift', 'apps/Viewer/Viewer.xcodeproj/project.xcproj', 'dist'] as const;
const [CORE, TOOL, VIEWER] = ['libs/Core', 'apps/Tool', 'apps/Viewer'] as const;
const SOURCES = { core: 'libs/Core/Core.swift', tool: 'apps/Tool/main.swift', tests: 'libs/tests/CoreTests/CoreTests.swift' } as const;
const REFERENCE = '<BuildableReference BuildableIdentifier="primary" BlueprintIdentifier="A10000000000000000000002" BuildableName="Viewer.app" BlueprintName="Viewer" ReferencedContainer="container:Viewer.xcodeproj"/>';
const files = {
    '.config/mise.lock': '# tool versions\n',
    'Package.swift':
        '// swift-tools-version: 6.4\nimport PackageDescription\n\nlet package = Package(\n    name: "Fixture",\n    platforms: [.macOS(.v27)],\n    targets: [\n        .target(name: "Core", path: "libs/Core"),\n        .target(name: "Middle", dependencies: ["Core"], path: "libs/Middle"),\n        .executableTarget(name: "Tool", dependencies: ["Middle"], path: "apps/Tool"),\n        .testTarget(name: "CoreTests", dependencies: ["Core"], path: "libs/tests/CoreTests"),\n    ]\n)\n',
    'libs/Core/Core.swift': 'public func one() -> Int { 1 }\n',
    'libs/Middle/Middle.swift': 'import Core\n\npublic func two() -> Int { one() + 1 }\n',
    'apps/Tool/main.swift': 'import Middle\n\nprint(two())\n',
    'libs/tests/CoreTests/CoreTests.swift': 'import Core\nimport Testing\n\n@Test func oneIsOne() { #expect(one() == 1) }\n',
    'apps/Viewer/Viewer.xcodeproj/project.xcproj': `{
  "default-configuration": "Release",
  "configurations": [
    { "name": "Debug" },
    { "name": "Release" },
  ],
  "localizations": {
    "development": "en",
    "supported": [
      "Base",
    ],
  },
  "files": [
    {
      "kind": "folder",
      "path": "Sources",
      "target-membership": [
        "Viewer",
      ],
    }, {
      "kind": "group",
      "name": "Products",
      "children": [
        { "path": "<PRODUCTS>/Viewer.app", "id": "A10000000000000000000001", "type": "wrapper.application", "index": false },
      ],
    },
  ],
  "targets": [
    {
      "name": "Viewer",
      "id": "A10000000000000000000002",
      "product": "Products/Viewer.app",
      "product-type": "application",
      "build-phases": [
        "compile-sources",
        "frameworks",
        "resources",
      ],
      "build-settings": {
        "PRODUCT_BUNDLE_IDENTIFIER": "example.fixture.viewer",
      },
    },
  ],
  "build-settings": {
    "CODE_SIGN_IDENTITY": "-",
    "CODE_SIGN_STYLE": "Manual",
    "CURRENT_PROJECT_VERSION": "1",
    "GENERATE_INFOPLIST_FILE": "YES",
    "MACOSX_DEPLOYMENT_TARGET": "27.0",
    "MARKETING_VERSION": "0.1.0",
    "PRODUCT_NAME": "$(TARGET_NAME)",
    "SDKROOT": "macosx",
    "SWIFT_VERSION": "6.0",
  },
  "last-upgrade": "27.2",
  "last-swift-update": "27.2",
}
`,
    'apps/Viewer/Viewer.xcodeproj/xcshareddata/xcschemes/Viewer.xcscheme': `<?xml version="1.0" encoding="UTF-8"?>\n<Scheme LastUpgradeVersion="2720" version="1.7">\n    <BuildAction parallelizeBuildables="YES" buildImplicitDependencies="YES">\n        <BuildActionEntries>\n            <BuildActionEntry buildForTesting="NO" buildForRunning="YES" buildForProfiling="YES" buildForArchiving="YES" buildForAnalyzing="YES">\n                ${REFERENCE}\n            </BuildActionEntry>\n        </BuildActionEntries>\n    </BuildAction>\n    <LaunchAction buildConfiguration="Debug">\n        <BuildableProductRunnable runnableDebuggingMode="0">\n            ${REFERENCE}\n        </BuildableProductRunnable>\n    </LaunchAction>\n</Scheme>\n`,
    'apps/Viewer/Sources/ViewerApp.swift': 'import SwiftUI\n\n@main\nstruct ViewerApp: App {\n    var body: some Scene {\n        WindowGroup { Text("viewer") }\n    }\n}\n',
};

// --- [MODELS] --------------------------------------------------------------------------

const Target = Schema.Struct({
    name: Schema.String,
    type: Schema.String,
    path: Schema.String,
    sources: Schema.Array(Schema.String),
    target_dependencies: Schema.optionalKey(Schema.NullOr(Schema.Array(Schema.String))),
    product_memberships: Schema.optionalKey(Schema.NullOr(Schema.Array(Schema.String))),
});
const Described = Schema.fromJsonString(Schema.Struct({ targets: Schema.Array(Target) })).annotate({
    description: 'JSON swift package describe --type json prints: each target of the package with its type, folder, sources, the targets it depends on, and the products holding it',
});
const Listed = Schema.fromJsonString(Schema.Struct({ project: Schema.Struct({ name: Schema.String, schemes: Schema.NonEmptyArray(Schema.String) }) })).annotate({ description: 'JSON xcodebuild -list -json prints: the project name and its schemes' });
const Settings = Schema.fromJsonString(Schema.NonEmptyArray(Schema.Struct({ buildSettings: Schema.Struct({ MARKETING_VERSION: Schema.String, SKIP_INSTALL: Schema.String }) }))).annotate({
    description: 'JSON xcodebuild -showBuildSettings -json prints: per target the version Xcode resolves for one configuration, and whether an archive holds its product',
});
const Names = Schema.fromJsonString(Schema.Array(Schema.String));

// --- [OPERATIONS] ----------------------------------------------------------------------

const described = (path: Path.Path, targets: readonly (typeof Target.Type)[], { name, type, path: root, sources, target_dependencies }: typeof Target.Type) => {
    const tested = targets.some((other) => other.type === 'test' && (other.target_dependencies ?? []).includes(name));
    return {
        name,
        version: 'none',
        root,
        manifest: PACKAGE,
        released: false,
        verbs: ['build', ...(tested ? ['test'] : []), ...(type === 'executable' ? ['install'] : [])],
        sources: [PACKAGE, ...sources.map((file) => path.join(root, file))],
        dependencies: targets.filter((other) => (target_dependencies ?? []).includes(other.name)).map((other) => other.path),
        executables: type === 'executable' ? [name] : [],
    };
};

const modules = Effect.gen(function* () {
    const [{ called }, path] = yield* Effect.all([Consumer, Path.Path]);
    const { checked } = yield* called('swift', ['package', 'describe', '--type', 'json']);
    const printed = yield* checked;
    const { targets } = yield* Schema.decodeEffect(Described)(printed.slice(printed.indexOf('{')));
    return targets.filter(({ product_memberships }) => Array.isReadonlyArrayNonEmpty(product_memberships ?? [])).map((target) => described(path, targets, target));
}).pipe(Effect.orDie);

const application = Effect.fnUntraced(function* (manifest: string) {
    const [{ called }, path] = yield* Effect.all([Consumer, Path.Path]);
    const [project, root] = [path.dirname(manifest), path.dirname(path.dirname(manifest))];
    const listed = yield* Effect.flatMap(called('xcodebuild', ['-list', '-json', '-project', project]), ({ stdout }) => Schema.decodeEffect(Listed)(stdout));
    const shown = yield* called('xcodebuild', ['-showBuildSettings', '-json', '-project', project, '-scheme', Array.headNonEmpty(listed.project.schemes), '-configuration', 'Release']);
    const { MARKETING_VERSION, SKIP_INSTALL } = Array.headNonEmpty(yield* Schema.decodeEffect(Settings)(shown.stdout)).buildSettings;
    const tracked = yield* called('git', ['ls-files', '--cached', '--others', '--exclude-standard', '--', root]);
    return {
        name: listed.project.name,
        version: MARKETING_VERSION,
        root,
        manifest,
        released: SKIP_INSTALL === 'NO',
        verbs: ['build', ...(SKIP_INSTALL === 'NO' ? ['install', 'pack'] : [])],
        sources: tracked.stdout.split('\n').filter((file) => file !== ''),
        dependencies: Array.empty<string>(),
        executables: Array.empty<string>(),
    };
}, Effect.orDie);

const members = Effect.gen(function* () {
    const { called } = yield* Consumer;
    const listed = yield* called('git', ['ls-files', '--cached', '--others', '--exclude-standard', '--', '*.xcproj']);
    const applications = yield* Effect.forEach(
        listed.stdout.split('\n').filter((manifest) => manifest !== ''),
        application,
    );
    return [...(yield* modules), ...applications];
});

const member = (root: string) => Effect.flatMap(members, (stated) => Effect.orDie(Effect.fromOption(Array.findFirst(stated, (candidate) => candidate.root === root))));

const built = Effect.fnUntraced(function* ({ executables }: Effect.Success<typeof members>[number]) {
    const [{ root, called }, path] = yield* Effect.all([Consumer, Path.Path]);
    const { stdout } = yield* called('swift', ['build', '--show-bin-path']);
    return executables.map((name) => path.join(path.relative(root, stdout.trim()), name));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const bound = Duration.toMillis('10 minutes');
const options = { timeout: bound, excludeTestServices: true } as const;

const workspace = (stated: Readonly<Record<string, string>> = {}) =>
    Consumer.layer({
        resolved: import.meta.url,
        settings: { plugins: [{ plugin: plugin.name, options: { outputPath: OUTPUT } }], release: { projects: ['tag:release'] } },
        ignored: ['.build', OUTPUT, '.swiftpm', 'xcuserdata'],
        files: { ...files, ...stated },
    }).pipe(Layer.provideMerge(Packed.layer(plugin.name)), Layer.provideMerge(layer));

conforms({
    workspace,
    members,
    built,
    report: REPORT,
    tools: ['swift', 'xcodebuild'],
    verbs: ['build', 'test', 'install', 'pack'],
    shared: [],
    unlocked: [],
    scratch: ['.build'],
    added: Consumer.use(({ wrote }) =>
        wrote({
            [PACKAGE]: files[PACKAGE].replace('        .target(name: "Core", path: "libs/Core"),', '        .target(name: "Core", dependencies: ["Extra"], path: "libs/Core"),\n        .target(name: "Extra", path: "libs/Extra"),'),
            'libs/Extra/Extra.swift': 'public func zero() -> Int { 0 }\n',
        }),
    ),
    published: true,
    refused: { 'broken/Package.swift': '// swift-tools-version: 6.4\nimport PackageDescription\n\nlet package = Package(name: \n' },
    manifests: (next) => ({ [PROJECT]: files[PROJECT].replace('"MARKETING_VERSION": "0.1.0"', `"MARKETING_VERSION": "${next}"`).replace('"CURRENT_PROJECT_VERSION": "1"', '"CURRENT_PROJECT_VERSION": "2"') }),
    edits: {
        verbs: ['build'],
        dependent: TOOL,
        upstream: (mark) => ({ [SOURCES.core]: `${files[SOURCES.core]}public func probe_${mark}() -> Int { 7 }\n` }),
        outside: (mark) => ({ 'apps/Viewer/notes.md': `${mark}\n` }),
        tested: CORE,
        failing: { [SOURCES.tests]: files[SOURCES.tests].replace('one() == 1', 'one() == 2') },
    },
});

it.layer(
    workspace(),
    options,
)((methods) => {
    methods.effect(
        'marks every module with a build affected by a change to Package.swift',
        Effect.fnUntraced(function* () {
            const [{ nx }, stated] = yield* Effect.all([Consumer, modules]);
            const { stdout } = yield* nx('show', 'projects', '--affected', `--files=${PACKAGE}`, '--withTarget=build', '--json');
            assert.includeMembers(
                [...(yield* Effect.orDie(Schema.decodeEffect(Names)(stdout)))],
                stated.map(({ name }) => name),
                `${REPORT}; README.md TASKS "--files=<path> limits it to one change's reach"; nrwl/nx astro-docs/src/content/docs/features/CI Features/affected.mdoc`,
            );
        }),
        bound,
    );

    methods.effect(
        'packs the app into a .zip and exits 0 with no App Store Connect key set, and publishes over that zip without an upload',
        Effect.fnUntraced(function* () {
            const [{ called, written, committed }, path, { name }] = yield* Effect.all([Consumer, Path.Path, member(VIEWER)]);
            const unset = ['ASC_API_KEY_P8', 'ASC_API_KEY_ID', 'ASC_API_ISSUER_ID'].flatMap((key) => ['-u', key]);
            const [packed, created] = yield* written(called('env', [...unset, 'nx', 'pack', name, '--skip-nx-cache', '--output-style=static']));
            assert.strictEqual(packed.code, 0, `README.md OWNERS "Mac app release form: ASC_API_* rows set notarize a .dmg for Sparkle, absent release a .zip": ${packed.stdout}${packed.stderr}`);
            assert.sameMembers(
                Array.intersection(
                    created.map((file) => path.extname(file)),
                    ['.zip', '.dmg'],
                ),
                ['.zip'],
                `README.md OWNERS "Mac app release form: ASC_API_* rows set notarize a .dmg for Sparkle, absent release a .zip": ${created.join(', ')}`,
            );
            yield* committed('pack');
            const published = yield* called('env', [...unset, 'nx', 'release', 'publish', '--dry-run']);
            const printed = `${published.stdout}${published.stderr}`;
            assert.strictEqual(published.code, 0, `README.md DELIVERY "a .dmg gets a Sparkle feed in R2": ${printed}`);
            assert.notInclude(printed, 'generate_appcast', 'README.md DELIVERY "a .dmg gets a Sparkle feed in R2": a zip gets no feed');
        }),
        bound,
    );
});

it.layer(
    workspace({
        [PACKAGE]: files[PACKAGE]
            .replace('    targets: [', '    dependencies: [.package(url: "https://github.com/apple/swift-argument-parser", from: "1.5.0")],\n    targets: [')
            .replace('dependencies: ["Middle"], path: "apps/Tool"', 'dependencies: ["Middle", .product(name: "ArgumentParser", package: "swift-argument-parser")], path: "apps/Tool"'),
    }),
    options,
)((methods) => {
    methods.effect(
        'reads the graph of a package with a remote dependency and no resolved file without resolving or fetching it',
        Effect.fnUntraced(function* () {
            const { nx, written } = yield* Consumer;
            const [shown, created] = yield* written(nx('show', 'projects'));
            assert.strictEqual(shown.code, 0, shown.stderr);
            assert.deepStrictEqual(
                created.filter((file) => file === 'Package.resolved' || file.startsWith('.build/checkouts/') || file.startsWith('.build/repositories/')),
                [],
                'nrwl/nx astro-docs/src/content/docs/kb/performant-project-graph-plugins.mdoc; swift package describe resolves no dependency',
            );
        }),
        bound,
    );
});

it.layer(
    workspace({
        'apps/Viewer/Package.swift': '// swift-tools-version: 6.4\nimport PackageDescription\n\nlet package = Package(name: "Viewer", platforms: [.macOS(.v27)], targets: [.executableTarget(name: "Probe", path: "Probe")])\n',
        'apps/Viewer/Probe/main.swift': 'print("probe")\n',
    }),
    options,
)((methods) => {
    methods.effect(
        'gives one project the targets of a Swift package and an Xcode project holding one folder under one name',
        Effect.fnUntraced(function* () {
            const { nodes, failures } = yield* Effect.flatMap(Consumer, ({ read }) => read);
            const held = Object.values(nodes).filter(({ data }) => data.root === VIEWER);
            assert.deepStrictEqual(failures, [], 'nrwl/nx packages/nx/src/project-graph/utils/project-configuration/target-merging.ts mergeTargetConfigurations');
            assert.lengthOf(held, 1, 'nrwl/nx packages/nx/src/project-graph/plugins/public-api.ts CreateNodesResult "projects" keyed by root');
            assert.lengthOf(
                Record.keys(held[0]?.data.targets ?? {}).filter((target) => target.startsWith('build--')),
                2,
                'README.md PRINCIPLES "Verbs aggregate <verb>--<tool> targets": swift build and xcodebuild each build the folder',
            );
        }),
        bound,
    );
});
