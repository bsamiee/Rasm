// swift-tools-version: 6.4
import PackageDescription

// --- [CONSTANTS] -----------------------------------------------------------------------

let swiftSettings: [SwiftSetting] = [
    .enableUpcomingFeature("ExistentialAny"),
    .enableUpcomingFeature("ImmutableWeakCaptures"),
    .enableUpcomingFeature("InferIsolatedConformances"),
    .enableUpcomingFeature("InternalImportsByDefault"),
    .enableUpcomingFeature("MemberImportVisibility"),
    .enableUpcomingFeature("NonisolatedNonsendingByDefault"),
    .strictMemorySafety(),
    .unsafeFlags(["-Xfrontend", "-warn-long-expression-type-checking-scopes", "-Xfrontend", "200"]),
    .treatAllWarnings(as: .error),
    .treatWarning("ExplicitSendable", as: .error),
    .treatWarning("StrictLanguageFeatures", as: .error),
]
let softDeprecations: SwiftSetting = .unsafeFlags(["-warn-soft-deprecated"])
let linkerSettings: [LinkerSetting] = [.unsafeFlags(["-Xlinker", "-fatal_warnings"])]

// --- [MODELS] --------------------------------------------------------------------------

let modules: [Target] = [

    // --- [DATA]

    .target(name: "Encoding", path: "libs/swift/data/Encoding"),
    .target(name: "Results", path: "libs/swift/data/Results"),

    // --- [NETWORK]

    .target(
        name: "JSONRPC",
        dependencies: [
            "Encoding", "Results", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"), .product(name: "CasePaths", package: "swift-case-paths"),
            .product(name: "CasePathsCore", package: "swift-case-paths"), .product(name: "OTelSemanticConventions", package: "swift-otel-semantic-conventions"),
            .product(name: "Tracing", package: "swift-distributed-tracing"),
        ],
        path: "libs/swift/network/JSONRPC",
    ),

    // --- [SYSTEM]

    .target(
        name: "Files",
        dependencies: [
            "Results", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"), .product(name: "CasePaths", package: "swift-case-paths"),
            .product(name: "CasePathsCore", package: "swift-case-paths"), .product(name: "Logging", package: "swift-log"),
        ],
        path: "libs/swift/system/Files",
    ),
    .target(
        name: "Processes",
        dependencies: [
            "Encoding", "Files", "Results", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"), .product(name: "CasePaths", package: "swift-case-paths"),
            .product(name: "CasePathsCore", package: "swift-case-paths"), .product(name: "DequeModule", package: "swift-collections"),
            .product(name: "Subprocess", package: "swift-subprocess"),
        ],
        path: "libs/swift/system/Processes",
    ),

    // --- [OBSERVABILITY]

    .target(
        name: "Telemetry",
        dependencies: [
            "Encoding", "Files", .product(name: "Instrumentation", package: "swift-distributed-tracing"), .product(name: "Logging", package: "swift-log"),
            .product(name: "OTel", package: "swift-otel"), .product(name: "OTelSemanticConventions", package: "swift-otel-semantic-conventions"),
            .product(name: "ServiceLifecycle", package: "swift-service-lifecycle"),
        ],
        path: "libs/swift/observability/Telemetry",
    ),

    // --- [DISTRIBUTION]

    .target(name: "Updater", dependencies: ["Processes", .product(name: "Logging", package: "swift-log"), .product(name: "Sparkle", package: "Sparkle")], path: "libs/swift/distribution/Updater"),

    // --- [MACOS]

    .target(
        name: "AppleEvents",
        dependencies: [
            "Encoding", "Results", .product(name: "CasePaths", package: "swift-case-paths"), .product(name: "CasePathsCore", package: "swift-case-paths"),
            .product(name: "OrderedCollections", package: "swift-collections"),
        ],
        path: "libs/swift/macos/AppleEvents",
    ),
    .target(
        name: "Applications",
        dependencies: [
            "AppleEvents", "Encoding", "Files", "Processes", "Results", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"), .product(name: "CasePaths", package: "swift-case-paths"),
            .product(name: "CasePathsCore", package: "swift-case-paths"), .product(name: "OrderedCollections", package: "swift-collections"),
            .product(name: "Subprocess", package: "swift-subprocess"),
        ],
        path: "libs/swift/macos/Applications",
        swiftSettings: [.enableExperimentalFeature("Extern")],
        linkerSettings: [.unsafeFlags(["-Xlinker", "-F/System/Library/PrivateFrameworks"]), .linkedFramework("SkyLight")],
    ),
    .target(
        name: "CurrentProcess",
        dependencies: [
            "Applications", "Encoding", "Files", "Processes", "Results", "Telemetry", .product(name: "CasePathsCore", package: "swift-case-paths"),
            .product(name: "Configuration", package: "swift-configuration"),
            .product(name: "Instrumentation", package: "swift-distributed-tracing"), .product(name: "Logging", package: "swift-log"), .product(name: "OTel", package: "swift-otel"),
            .product(name: "OTelSemanticConventions", package: "swift-otel-semantic-conventions"), .product(name: "ServiceLifecycle", package: "swift-service-lifecycle"),
            .product(name: "SystemMetrics", package: "swift-system-metrics"),
        ],
        path: "libs/swift/macos/CurrentProcess",
    ),
    .target(
        name: "ApplicationServer",
        dependencies: [
            "AppleEvents", "Applications", "CurrentProcess", "Encoding", "Files", "JSONRPC", "Processes", "Results", "Telemetry", "Updater",
            .product(name: "CasePathsCore", package: "swift-case-paths"), .product(name: "Logging", package: "swift-log"), .product(name: "ServiceLifecycle", package: "swift-service-lifecycle"),
            .product(name: "Tracing", package: "swift-distributed-tracing"), .product(name: "UnixSignals", package: "swift-service-lifecycle"),
        ],
        path: "libs/swift/macos/ApplicationServer",
    ),
]
let applications: [Target] = [
    .target(
        name: "Atelier",
        dependencies: ["ApplicationServer", "Applications"],
        path: "products/apps/creative-cloud/atelier",
        exclude: ["AppIcon.icon", "Atelier.entitlements", "Atelier.xcodeproj", "Info.plist", "server"],
    ),
    .target(
        name: "Relay",
        dependencies: [
            "CurrentProcess", "Encoding", "Files", "JSONRPC", "Processes", "Results", "Telemetry", "Updater", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"),
            .product(name: "CasePaths", package: "swift-case-paths"), .product(name: "CasePathsCore", package: "swift-case-paths"), .product(name: "Logging", package: "swift-log"),
            .product(name: "ServiceLifecycle", package: "swift-service-lifecycle"), .product(name: "Sparkle", package: "Sparkle"),
            .product(name: "Subprocess", package: "swift-subprocess"), .product(name: "Tracing", package: "swift-distributed-tracing"),
        ],
        path: "products/macos/relay",
        exclude: ["AGENTS.md", "AppIcon.icon", "CLAUDE.md", "Info.plist", "Relay.entitlements", "Relay.xcodeproj"],
        resources: [.process("Assets.xcassets")],
        swiftSettings: [.defaultIsolation(MainActor.self)],
    ),
    .target(
        name: "Rasad",
        dependencies: ["ApplicationServer"],
        path: "products/macos/rasad",
        exclude: ["AppIcon.icon", "Info.plist", "Rasad.entitlements", "Rasad.xcodeproj", "README.md", "server"],
    ),
]
let support: [Target] = [
    .executableTarget(
        name: "Fixture",
        dependencies: [
            "Encoding", .product(name: "CasePaths", package: "swift-case-paths"), .product(name: "CasePathsCore", package: "swift-case-paths"),
            .product(name: "Subprocess", package: "swift-subprocess"),
        ],
        path: "tests/support/swift/Fixture",
    )
]
let tests: [Target] = [
    .testTarget(
        name: "FixtureTests",
        dependencies: ["Fixture", .product(name: "CasePathsCore", package: "swift-case-paths"), .product(name: "CustomDump", package: "swift-custom-dump")],
        path: "tests/support/swift/FixtureTests",
    ),
    .testTarget(
        name: "EncodingTests",
        dependencies: [
            "Encoding", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"), .product(name: "CustomDump", package: "swift-custom-dump"),
            .product(name: "PropertyBased", package: "swift-property-based"),
        ],
        path: "tests/libs/swift/data/Encoding",
    ),
    .testTarget(
        name: "ResultsTests",
        dependencies: [
            "Results", .product(name: "Clocks", package: "swift-clocks"), .product(name: "CustomDump", package: "swift-custom-dump"),
            .product(name: "PropertyBased", package: "swift-property-based"),
        ],
        path: "tests/libs/swift/data/Results",
    ),
    .testTarget(
        name: "JSONRPCTests",
        dependencies: [
            "Encoding", "JSONRPC", "Results", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"), .product(name: "CasePathsCore", package: "swift-case-paths"),
            .product(name: "CustomDump", package: "swift-custom-dump"),
        ],
        path: "tests/libs/swift/network/JSONRPC",
    ),
    .testTarget(name: "FilesTests", dependencies: ["Files", .product(name: "CustomDump", package: "swift-custom-dump")], path: "tests/libs/swift/system/Files"),
    .testTarget(name: "FilesPerformanceTests", dependencies: ["Files"], path: "tests/libs/swift/system/FilesPerformance"),
    .testTarget(name: "ProcessesTests", dependencies: ["Processes", .product(name: "CustomDump", package: "swift-custom-dump")], path: "tests/libs/swift/system/Processes"),
    .testTarget(
        name: "TelemetryTests",
        dependencies: [
            "Encoding", "Files", "Telemetry", .product(name: "CustomDump", package: "swift-custom-dump"), .product(name: "InMemoryLogging", package: "swift-log"),
            .product(name: "InMemoryTracing", package: "swift-distributed-tracing"), .product(name: "Instrumentation", package: "swift-distributed-tracing"),
            .product(name: "Logging", package: "swift-log"), .product(name: "OTel", package: "swift-otel"),
        ],
        path: "tests/libs/swift/observability/Telemetry",
    ),
    .testTarget(
        name: "UpdaterTests",
        dependencies: ["Updater", .product(name: "InMemoryLogging", package: "swift-log"), .product(name: "Logging", package: "swift-log")],
        path: "tests/libs/swift/distribution/Updater",
    ),
    .testTarget(
        name: "AppleEventsTests",
        dependencies: [
            "AppleEvents", "Encoding", "Fixture", "Results", .product(name: "CasePathsCore", package: "swift-case-paths"),
            .product(name: "CustomDump", package: "swift-custom-dump"), .product(name: "PropertyBased", package: "swift-property-based"),
        ],
        path: "tests/libs/swift/macos/AppleEvents",
    ),
    .testTarget(
        name: "ApplicationsTests",
        dependencies: [
            "AppleEvents", "Applications", "Encoding", "Fixture", "Processes", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"),
            .product(name: "CasePathsCore", package: "swift-case-paths"), .product(name: "CustomDump", package: "swift-custom-dump"),
            .product(name: "PropertyBased", package: "swift-property-based"), .product(name: "Subprocess", package: "swift-subprocess"),
        ],
        path: "tests/libs/swift/macos/Applications",
    ),
    .testTarget(
        name: "CurrentProcessTests",
        dependencies: [
            "CurrentProcess", .product(name: "Configuration", package: "swift-configuration"), .product(name: "CustomDump", package: "swift-custom-dump"),
            .product(name: "InMemoryLogging", package: "swift-log"), .product(name: "Logging", package: "swift-log"),
        ],
        path: "tests/libs/swift/macos/CurrentProcess",
    ),
    .testTarget(
        name: "ApplicationServerTests",
        dependencies: [
            "AppleEvents", "ApplicationServer", "Applications", "Encoding", "JSONRPC", "Results", .product(name: "CasePaths", package: "swift-case-paths"),
            .product(name: "CustomDump", package: "swift-custom-dump"),
        ],
        path: "tests/libs/swift/macos/ApplicationServer",
    ),
]

// --- [COMPOSITION] ---------------------------------------------------------------------

let package = Package(
    name: "Rasm",
    platforms: [.macOS(.v27)],
    products: applications.map { module in .library(name: module.name, targets: [module.name]) },
    dependencies: [

        // --- [LANGUAGE]

        .package(url: "https://github.com/apple/swift-async-algorithms", from: "1.1.7"),
        .package(url: "https://github.com/pointfreeco/swift-case-paths", from: "1.10.0"),
        .package(url: "https://github.com/apple/swift-collections", from: "1.7.6"),

        // --- [SYSTEM]

        .package(url: "https://github.com/apple/swift-configuration", from: "1.5.0"),
        .package(url: "https://github.com/swift-server/swift-service-lifecycle", from: "2.13.0"),
        .package(url: "https://github.com/swiftlang/swift-subprocess", from: "1.0.2"),

        // --- [OBSERVABILITY]

        .package(url: "https://github.com/apple/swift-distributed-tracing", from: "1.7.4"),
        .package(url: "https://github.com/apple/swift-log", from: "1.17.2"),
        .package(url: "https://github.com/swift-otel/swift-otel", from: "1.6.1", traits: ["OTLPHTTP"]),
        .package(url: "https://github.com/swift-otel/swift-otel-semantic-conventions", .upToNextMinor(from: "1.44.0"), traits: [.defaults, "Experimental"]),
        .package(url: "https://github.com/apple/swift-system-metrics", from: "1.3.1"),

        // --- [DISTRIBUTION]

        .package(url: "https://github.com/sparkle-project/Sparkle", from: "2.11.0-beta.1"),

        // --- [TESTING]

        .package(url: "https://github.com/pointfreeco/swift-clocks", from: "1.1.1"),
        .package(url: "https://github.com/pointfreeco/swift-custom-dump", from: "1.7.3"),
        .package(url: "https://github.com/pointfreeco/swift-issue-reporting", from: "2.1.2"),
        .package(url: "https://github.com/x-sheep/swift-property-based", from: "2.1.0"),
    ],
    targets: modules + applications + support + tests,
)

for target: Target in package.targets {
    target.swiftSettings = swiftSettings + (target.resources == nil ? [softDeprecations] : []) + (target.swiftSettings ?? [])
    target.linkerSettings = linkerSettings + (target.linkerSettings ?? [])
    target.dependencies += target.type == .test ? [.product(name: "IssueReportingTestSupport", package: "swift-issue-reporting")] : []
}
