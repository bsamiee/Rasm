import { layer } from '@effect/platform-node/NodeServices';
import plugin from '@rasm/nx-apple/package.json' with { type: 'json' };
import { Consumer, Packed } from '@rasm/testing/nx/consumer';
import { Array, Effect, Layer, Path, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const APPLE = {
    described: 'swift package describe --type json',
    listed: 'xcodebuild -list -json -project <project>',
    bin: 'swift build --show-bin-path',
    settings: 'xcodebuild -showBuildSettings -json',
    cleaned: 'swift package clean "Delete build artifacts", xcodebuild clean',
} as const;
const SETTINGS = 'buildSettings = {MACOSX_DEPLOYMENT_TARGET = 27.0; SDKROOT = macosx; SWIFT_VERSION = 6.0; };';
const PRODUCT = 'buildSettings = {CODE_SIGN_STYLE = Automatic; CURRENT_PROJECT_VERSION = 1; MARKETING_VERSION = 0.1.0; PRODUCT_NAME = "$(TARGET_NAME)"; };';
const files = {
    '.config/mise.lock': '# tool versions\n',
    'Package.swift':
        '// swift-tools-version: 6.4\nimport PackageDescription\n\nlet package = Package(\n    name: "Fixture",\n    platforms: [.macOS(.v27)],\n    targets: [\n        .target(name: "Core", path: "libs/Core"),\n        .target(name: "Middle", dependencies: ["Core"], path: "libs/Middle"),\n        .executableTarget(name: "Tool", dependencies: ["Middle"], path: "apps/Tool"),\n        .testTarget(name: "CoreTests", dependencies: ["Core"], path: "libs/tests/CoreTests"),\n    ]\n)\n',
    'libs/Core/Core.swift': 'public func one() -> Int { 1 }\n',
    'libs/Middle/Middle.swift': 'import Core\n\npublic func two() -> Int { one() + 1 }\n',
    'apps/Tool/main.swift': 'import Middle\n\nprint(two())\n',
    'libs/tests/CoreTests/CoreTests.swift': 'import Core\nimport Testing\n\n@Test func oneIsOne() { #expect(one() == 1) }\n',
    'apps/Viewer/Viewer.xcodeproj/project.pbxproj': `// !$*UTF8*$!
{
	archiveVersion = 1;
	classes = {
	};
	objectVersion = 77;
	objects = {
		A10000000000000000000001 /* Viewer */ = {isa = PBXFileReference; explicitFileType = "compiled.mach-o.executable"; includeInIndex = 0; path = Viewer; sourceTree = BUILT_PRODUCTS_DIR; };
		A10000000000000000000002 /* Sources */ = {isa = PBXFileSystemSynchronizedRootGroup; path = Sources; sourceTree = "<group>"; };
		A10000000000000000000003 = {isa = PBXGroup; children = (A10000000000000000000002 /* Sources */, A10000000000000000000004 /* Products */, ); sourceTree = "<group>"; };
		A10000000000000000000004 /* Products */ = {isa = PBXGroup; children = (A10000000000000000000001 /* Viewer */, ); name = Products; sourceTree = "<group>"; };
		A10000000000000000000005 /* Sources */ = {isa = PBXSourcesBuildPhase; buildActionMask = 2147483647; files = (); runOnlyForDeploymentPostprocessing = 0; };
		A10000000000000000000006 /* Viewer */ = {isa = PBXNativeTarget; buildConfigurationList = A1000000000000000000000A; buildPhases = (A10000000000000000000005 /* Sources */, ); buildRules = (); dependencies = (); fileSystemSynchronizedGroups = (A10000000000000000000002 /* Sources */, ); name = Viewer; productName = Viewer; productReference = A10000000000000000000001 /* Viewer */; productType = "com.apple.product-type.tool"; };
		A10000000000000000000007 /* Project object */ = {isa = PBXProject; attributes = {BuildIndependentTargetsInParallel = 1; LastUpgradeCheck = 2700; }; buildConfigurationList = A10000000000000000000009; developmentRegion = en; hasScannedForEncodings = 0; knownRegions = (en, Base, ); mainGroup = A10000000000000000000003; minimizedProjectReferenceProxies = 1; preferredProjectObjectVersion = 77; productRefGroup = A10000000000000000000004 /* Products */; projectDirPath = ""; projectRoot = ""; targets = (A10000000000000000000006 /* Viewer */, ); };
		A1000000000000000000000B /* Debug */ = {isa = XCBuildConfiguration; ${SETTINGS} name = Debug; };
		A1000000000000000000000C /* Release */ = {isa = XCBuildConfiguration; ${SETTINGS} name = Release; };
		A1000000000000000000000D /* Debug */ = {isa = XCBuildConfiguration; ${PRODUCT} name = Debug; };
		A1000000000000000000000E /* Release */ = {isa = XCBuildConfiguration; ${PRODUCT} name = Release; };
		A10000000000000000000009 /* Build configuration list for PBXProject "Viewer" */ = {isa = XCConfigurationList; buildConfigurations = (A1000000000000000000000B /* Debug */, A1000000000000000000000C /* Release */, ); defaultConfigurationIsVisible = 0; defaultConfigurationName = Release; };
		A1000000000000000000000A /* Build configuration list for PBXNativeTarget "Viewer" */ = {isa = XCConfigurationList; buildConfigurations = (A1000000000000000000000D /* Debug */, A1000000000000000000000E /* Release */, ); defaultConfigurationIsVisible = 0; defaultConfigurationName = Release; };
	};
	rootObject = A10000000000000000000007 /* Project object */;
}
`,
    'apps/Viewer/Sources/main.swift': 'print("viewer")\n',
};

// --- [MODELS] --------------------------------------------------------------------------

const Target = Schema.Struct({ name: Schema.String, type: Schema.String, path: Schema.String, sources: Schema.Array(Schema.String), target_dependencies: Schema.optionalKey(Schema.NullOr(Schema.Array(Schema.String))) });
const Described = Schema.fromJsonString(Schema.Struct({ targets: Schema.Array(Target) })).annotate({
    description: 'JSON swift package describe --type json prints: each target of the package with its type, folder, sources, and the targets it depends on',
});
const Listed = Schema.fromJsonString(Schema.Struct({ project: Schema.Struct({ name: Schema.String, targets: Schema.Array(Schema.String) }) })).annotate({ description: 'JSON xcodebuild -list -json prints: the project name and its targets' });
const Settings = Schema.fromJsonString(Schema.Array(Schema.Struct({ buildSettings: Schema.Struct({ MARKETING_VERSION: Schema.String, FULL_PRODUCT_NAME: Schema.String }) }))).annotate({
    description: 'JSON xcodebuild -showBuildSettings -json prints: per target the version and product file name Xcode resolves for one configuration',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const described = (path: Path.Path, manifest: string, targets: readonly (typeof Target.Type)[], { name, type, path: at, sources, target_dependencies }: typeof Target.Type) => {
    const folder = path.dirname(manifest);
    const needed = Array.getSomes((target_dependencies ?? []).map((dependency) => Array.findFirst(targets, (target) => target.name === dependency)));
    return {
        name,
        tests: type === 'test',
        executable: type === 'executable',
        root: path.join(folder, at),
        manifest,
        sources: sources.map((file) => path.join(folder, at, file)),
        dependencies: needed.map((target) => path.join(folder, target.path)),
    };
};

const packaged = Effect.fnUntraced(function* (manifest: string) {
    const [{ called }, path] = yield* Effect.all([Consumer, Path.Path]);
    const { code, stdout } = yield* called('swift', ['package', 'describe', '--type', 'json', '--package-path', path.dirname(manifest)]);
    const { targets } = Number(code) === 0 ? yield* Schema.decodeEffect(Described)(stdout.slice(stdout.indexOf('{'))) : { targets: [] };
    return targets.map((target) => described(path, manifest, targets, target));
}, Effect.orDie);

const modules = Effect.gen(function* () {
    const { called } = yield* Consumer;
    const listed = yield* called('git', ['ls-files', '--cached', '--others', '--exclude-standard', '--', 'Package.swift', '*/Package.swift']);
    const packages = yield* Effect.forEach(
        listed.stdout.split('\n').filter((manifest) => manifest !== ''),
        packaged,
    );
    return packages.flat();
});

const application = Effect.fnUntraced(function* (manifest: string) {
    const [{ called }, path] = yield* Effect.all([Consumer, Path.Path]);
    const root = path.dirname(path.dirname(manifest));
    const { stdout } = yield* called('xcodebuild', ['-list', '-json', '-project', path.dirname(manifest)]);
    const { project } = yield* Schema.decodeEffect(Listed)(stdout);
    return { name: project.name, tests: false, executable: false, root, manifest, sources: [path.join(root, 'Sources/main.swift')], dependencies: Array.empty<string>() };
}, Effect.orDie);

const applications = Effect.gen(function* () {
    const { called } = yield* Consumer;
    const listed = yield* called('git', ['ls-files', '--cached', '--others', '--exclude-standard', '--', '*.pbxproj']);
    return yield* Effect.forEach(
        listed.stdout.split('\n').filter((manifest) => manifest !== ''),
        application,
    );
});

const members = Effect.map(Effect.all([modules, applications]), ([swift, xcode]) => [...swift, ...xcode]);

const misplaced = (stated: Effect.Success<typeof members>, folders: readonly string[]) => [
    ...Array.difference(
        stated.filter(({ tests }) => !tests).map(({ root }) => root),
        folders,
    ).map((root) => `${root} holds no project`),
    ...Array.difference(
        folders,
        stated.map(({ root }) => root),
    ).map((root) => `${root} holds no target or Xcode project`),
];

const settings = Effect.fnUntraced(function* (project: string, target: string, configuration: string) {
    const { called } = yield* Consumer;
    const shown = yield* called('xcodebuild', ['-showBuildSettings', '-json', '-project', project, '-target', target, '-configuration', configuration]);
    const resolved = yield* Effect.flatMap(Schema.decodeEffect(Settings)(shown.stdout), (targets) => Effect.fromOption(Array.head(targets)));
    const { MARKETING_VERSION, FULL_PRODUCT_NAME } = resolved.buildSettings;
    return { version: MARKETING_VERSION, product: FULL_PRODUCT_NAME };
}, Effect.orDie);

// --- [COMPOSITION] ---------------------------------------------------------------------

const workspace = (stated: Readonly<Record<string, string>> = {}) =>
    Consumer.layer({
        resolved: import.meta.url,
        settings: { plugins: [plugin.name], release: { projects: ['tag:release'] } },
        ignored: ['.build', 'build', 'DerivedData', '.swiftpm', 'xcuserdata'],
        files: { ...files, ...stated },
    }).pipe(Layer.provideMerge(Packed.layer(plugin.name)), Layer.provideMerge(layer));

// --- [EXPORTS] -------------------------------------------------------------------------

export { APPLE, files, members, misplaced, modules, settings, workspace };
