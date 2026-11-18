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

    // --- [RPC]

    .target(
        name: "JSONRPC",
        dependencies: ["Encoding", "Results", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"), .product(name: "Tracing", package: "swift-distributed-tracing")],
        path: "libs/swift/rpc/JSONRPC",
    ),

    // --- [RUNTIME]

    .target(
        name: "Files",
        dependencies: [
            "Results", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"), .product(name: "CasePaths", package: "swift-case-paths"),
            .product(name: "CasePathsCore", package: "swift-case-paths"), .product(name: "Logging", package: "swift-log"),
        ],
        path: "libs/swift/runtime/Files",
    ),
    .target(
        name: "Telemetry",
        dependencies: [
            "Encoding", "Files", .product(name: "Instrumentation", package: "swift-distributed-tracing"), .product(name: "Logging", package: "swift-log"),
            .product(name: "ServiceLifecycle", package: "swift-service-lifecycle"), .product(name: "Tracing", package: "swift-distributed-tracing"),
        ],
        path: "libs/swift/runtime/Telemetry",
    ),
    .target(
        name: "Processes",
        dependencies: [
            "Encoding", "Files", "Results", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"), .product(name: "CasePaths", package: "swift-case-paths"),
            .product(name: "CasePathsCore", package: "swift-case-paths"), .product(name: "DequeModule", package: "swift-collections"),
            .product(name: "Subprocess", package: "swift-subprocess"),
        ],
        path: "libs/swift/runtime/Processes",
    ),
    .target(
        name: "CurrentProcess",
        dependencies: [
            "Applications", "Files", "Processes", "Telemetry", .product(name: "CasePathsCore", package: "swift-case-paths"), .product(name: "Configuration", package: "swift-configuration"),
            .product(name: "Instrumentation", package: "swift-distributed-tracing"), .product(name: "Logging", package: "swift-log"), .product(name: "OTel", package: "swift-otel"),
            .product(name: "ServiceLifecycle", package: "swift-service-lifecycle"), .product(name: "SystemMetrics", package: "swift-system-metrics"),
        ],
        path: "libs/swift/runtime/CurrentProcess",
    ),

    // --- [GRAPHICS]

    .target(name: "Fonts", dependencies: ["Results"], path: "libs/swift/graphics/Fonts"),

    // --- [DISTRIBUTION]

    .target(name: "Updater", dependencies: [.product(name: "Sparkle", package: "Sparkle")], path: "libs/swift/distribution/Updater"),

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
    ),
    .target(name: "ScreenCapture", dependencies: ["Applications", "Processes", "Results"], path: "libs/swift/macos/ScreenCapture"),
    .target(
        name: "ApplicationServer",
        dependencies: [
            "AppleEvents", "Applications", "CurrentProcess", "Files", "JSONRPC", "Processes", "Results", "ScreenCapture", "Telemetry",
            .product(name: "Logging", package: "swift-log"),
        ],
        path: "libs/swift/macos/ApplicationServer",
    ),
]
let applications: [Target] = [
    .target(
        name: "Atelier",
        dependencies: [
            "ApplicationServer", "Applications", "CurrentProcess", "Encoding", "JSONRPC", "Results", "Telemetry", "Updater",
            .product(name: "Logging", package: "swift-log"), .product(name: "ServiceLifecycle", package: "swift-service-lifecycle"),
            .product(name: "Tracing", package: "swift-distributed-tracing"), .product(name: "UnixSignals", package: "swift-service-lifecycle"),
        ],
        path: "products/apps/creative-cloud/multi-app/atelier/macos",
        exclude: ["Atelier.entitlements", "Atelier.xcodeproj", "Info.plist"],
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
        exclude: ["AGENTS.md", "CLAUDE.md", "Info.plist", "Relay.entitlements", "Relay.xcodeproj", "Resources/AppIcon.icon"],
        resources: [.process("Resources/Assets.xcassets")],
        swiftSettings: [.defaultIsolation(MainActor.self)],
    ),
    .target(
        name: "watch-app",
        dependencies: [
            "Applications", "CurrentProcess", "Encoding", "Files", "Processes", "ScreenCapture", "Telemetry",
            .product(name: "ArgumentParser", package: "swift-argument-parser"), .product(name: "Configuration", package: "swift-configuration"),
            .product(name: "Logging", package: "swift-log"), .product(name: "ServiceLifecycle", package: "swift-service-lifecycle"),
            .product(name: "Tracing", package: "swift-distributed-tracing"), .product(name: "UnixSignals", package: "swift-service-lifecycle"),
            .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"), .product(name: "CasePaths", package: "swift-case-paths"),
            .product(name: "OrderedCollections", package: "swift-collections"), .product(name: "Subprocess", package: "swift-subprocess"),
        ],
        path: "products/macos/watch-app",
        exclude: ["Resources", "watch-app.entitlements", "watch-app.xcodeproj"],
    ),
]
let tests: [Target] = [
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
        dependencies: ["Results", .product(name: "CustomDump", package: "swift-custom-dump"), .product(name: "PropertyBased", package: "swift-property-based")],
        path: "tests/libs/swift/data/Results",
    ),
    .testTarget(
        name: "JSONRPCTests",
        dependencies: ["Encoding", "JSONRPC", "Results", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"), .product(name: "CustomDump", package: "swift-custom-dump")],
        path: "tests/libs/swift/rpc/JSONRPC",
    ),
    .testTarget(
        name: "AppleEventsTests",
        dependencies: ["AppleEvents", "Encoding", "Results", .product(name: "CasePathsCore", package: "swift-case-paths"), .product(name: "CustomDump", package: "swift-custom-dump")],
        path: "tests/libs/swift/macos/AppleEvents",
    ),
    .testTarget(
        name: "ApplicationsTests",
        dependencies: ["AppleEvents", "Applications", "Encoding", .product(name: "CustomDump", package: "swift-custom-dump")],
        path: "tests/libs/swift/macos/Applications",
    ),
    .testTarget(
        name: "ApplicationServerTests",
        dependencies: ["ApplicationServer", "Applications", "JSONRPC", .product(name: "CustomDump", package: "swift-custom-dump")],
        path: "tests/libs/swift/macos/ApplicationServer",
    ),
    .testTarget(name: "ProcessesTests", dependencies: ["Processes", .product(name: "CustomDump", package: "swift-custom-dump")], path: "tests/libs/swift/runtime/Processes"),
]

// --- [COMPOSITION] ---------------------------------------------------------------------

let package = Package(
    name: "Rasm",
    platforms: [.macOS(.v27)],
    products: (modules + applications).map { module in .library(name: module.name, targets: [module.name]) },
    dependencies: [

        // --- [LANGUAGE]

        .package(url: "https://github.com/apple/swift-async-algorithms", from: "1.1.7"),
        .package(url: "https://github.com/pointfreeco/swift-case-paths", from: "1.10.0"),
        .package(url: "https://github.com/apple/swift-collections", from: "1.7.6"),

        // --- [SYSTEM]

        .package(url: "https://github.com/apple/swift-argument-parser", from: "1.8.2"),
        .package(url: "https://github.com/apple/swift-configuration", from: "1.5.0"),
        .package(url: "https://github.com/swift-server/swift-service-lifecycle", from: "2.13.0"),
        .package(url: "https://github.com/swiftlang/swift-subprocess", from: "1.0.2"),

        // --- [TELEMETRY]

        .package(url: "https://github.com/apple/swift-distributed-tracing", from: "1.7.4"),
        .package(url: "https://github.com/apple/swift-log", from: "1.17.2"),
        .package(url: "https://github.com/swift-otel/swift-otel", from: "1.6.0", traits: ["OTLPHTTP"]),
        .package(url: "https://github.com/apple/swift-system-metrics", from: "1.3.1"),

        // --- [DISTRIBUTION]

        .package(url: "https://github.com/sparkle-project/Sparkle", from: "2.11.0-beta.1"),

        // --- [TESTING]

        .package(url: "https://github.com/pointfreeco/swift-custom-dump", from: "1.7.3"),
        .package(url: "https://github.com/pointfreeco/swift-issue-reporting", from: "2.1.2"),
        .package(url: "https://github.com/x-sheep/swift-property-based", from: "2.0.1"),
    ],
    targets: modules + applications + tests,
)

for target: Target in package.targets {
    target.swiftSettings = swiftSettings + (target.resources == nil ? [softDeprecations] : []) + (target.swiftSettings ?? [])
    target.linkerSettings = linkerSettings
    target.dependencies += target.type == .test ? [.product(name: "IssueReportingTestSupport", package: "swift-issue-reporting")] : []
}
