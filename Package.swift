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
    .unsafeFlags(["-warn-soft-deprecated", "-Xfrontend", "-warn-long-expression-type-checking-scopes", "-Xfrontend", "200"]),
    .treatAllWarnings(as: .error),
    .treatWarning("ExplicitSendable", as: .error),
    .treatWarning("StrictLanguageFeatures", as: .error),
]
let linkerSettings: [LinkerSetting] = [.unsafeFlags(["-Xlinker", "-fatal_warnings"])]
let embeddedInfo: (String) -> [LinkerSetting] = { folder in
    [.unsafeFlags(["-Xlinker", "-sectcreate", "-Xlinker", "__TEXT", "-Xlinker", "__info_plist", "-Xlinker", "\(Context.packageDirectory)/\(folder)/Info.plist"])]
}

// --- [MODELS] --------------------------------------------------------------------------

let modules: [Target] = [

    // --- [CORE]

    .target(name: "Encoding", path: "libs/swift/core/Encoding"),
    .target(name: "Results", path: "libs/swift/core/Results"),
    .target(name: "JSONRPC", dependencies: ["Encoding", "Results"], path: "libs/swift/core/JSONRPC"),
    .target(
        name: "EffectRPC",
        dependencies: [
            "Encoding", "JSONRPC", "Results", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"), .product(name: "Tracing", package: "swift-distributed-tracing"),
        ],
        path: "libs/swift/core/EffectRPC",
    ),

    // --- [MACOS]

    .target(
        name: "Files",
        dependencies: [
            "Results", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"), .product(name: "CasePaths", package: "swift-case-paths"),
            .product(name: "CasePathsCore", package: "swift-case-paths"), .product(name: "Logging", package: "swift-log"),
        ],
        path: "libs/swift/macos/Files",
    ),
    .target(
        name: "Telemetry",
        dependencies: [
            "Encoding", "Files", .product(name: "Instrumentation", package: "swift-distributed-tracing"), .product(name: "Logging", package: "swift-log"),
            .product(name: "ServiceLifecycle", package: "swift-service-lifecycle"), .product(name: "Tracing", package: "swift-distributed-tracing"),
        ],
        path: "libs/swift/macos/Telemetry",
    ),
    .target(
        name: "Processes",
        dependencies: [
            "Results", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"), .product(name: "CasePaths", package: "swift-case-paths"),
            .product(name: "Subprocess", package: "swift-subprocess"),
        ],
        path: "libs/swift/macos/Processes",
    ),
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
    .target(
        name: "ApplicationServer",
        dependencies: [
            "AppleEvents", "Applications", "EffectRPC", "Processes", "Results", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"),
            .product(name: "CasePaths", package: "swift-case-paths"), .product(name: "CasePathsCore", package: "swift-case-paths"), .product(name: "OrderedCollections", package: "swift-collections"),
        ],
        path: "libs/swift/macos/ApplicationServer",
    ),
    .target(
        name: "CurrentProcess",
        dependencies: [
            "Applications", "Files", "Telemetry", .product(name: "CasePathsCore", package: "swift-case-paths"), .product(name: "Configuration", package: "swift-configuration"),
            .product(name: "Instrumentation", package: "swift-distributed-tracing"), .product(name: "Logging", package: "swift-log"), .product(name: "OTel", package: "swift-otel"),
            .product(name: "ServiceLifecycle", package: "swift-service-lifecycle"), .product(name: "SystemMetrics", package: "swift-system-metrics"),
        ],
        path: "libs/swift/macos/CurrentProcess",
    ),
]
let executables: [Target] = [
    .executableTarget(
        name: "watch-apps",
        dependencies: [
            "ApplicationServer", "Applications", "CurrentProcess", "EffectRPC", "Encoding", "Files", "Processes", "Results", "Telemetry",
            .product(name: "ArgumentParser", package: "swift-argument-parser"), .product(name: "Configuration", package: "swift-configuration"),
            .product(name: "Logging", package: "swift-log"), .product(name: "ServiceLifecycle", package: "swift-service-lifecycle"),
            .product(name: "Tracing", package: "swift-distributed-tracing"), .product(name: "UnixSignals", package: "swift-service-lifecycle"),
            .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"), .product(name: "CasePaths", package: "swift-case-paths"),
            .product(name: "OrderedCollections", package: "swift-collections"), .product(name: "Subprocess", package: "swift-subprocess"),
        ],
        path: "apps/macos/watch-apps",
        exclude: ["Info.plist"],
    )
]
let tests: [Target] = [
    .testTarget(
        name: "EncodingTests",
        dependencies: [
            "Encoding", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"), .product(name: "CustomDump", package: "swift-custom-dump"),
            .product(name: "PropertyBased", package: "swift-property-based"),
        ],
        path: "tests/swift/libs/core/Encoding",
    ),
    .testTarget(
        name: "ResultsTests",
        dependencies: ["Results", .product(name: "CustomDump", package: "swift-custom-dump"), .product(name: "PropertyBased", package: "swift-property-based")],
        path: "tests/swift/libs/core/Results",
    ),
    .testTarget(name: "JSONRPCTests", dependencies: ["Encoding", "JSONRPC", .product(name: "CustomDump", package: "swift-custom-dump")], path: "tests/swift/libs/core/JSONRPC"),
    .testTarget(
        name: "EffectRPCTests",
        dependencies: ["EffectRPC", "Encoding", "Results", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"), .product(name: "CustomDump", package: "swift-custom-dump")],
        path: "tests/swift/libs/core/EffectRPC",
    ),
]

// --- [COMPOSITION] ---------------------------------------------------------------------

let package = Package(
    name: "Rasm",
    platforms: [.macOS(.v27)],
    products: modules.map { module in .library(name: module.name, targets: [module.name]) },
    dependencies: [

        // --- [LANGUAGE]

        .package(url: "https://github.com/apple/swift-async-algorithms", from: "1.1.7"),
        .package(url: "https://github.com/pointfreeco/swift-case-paths", from: "1.10.0"),
        .package(url: "https://github.com/apple/swift-collections", from: "1.7.5"),

        // --- [SYSTEM]

        .package(url: "https://github.com/apple/swift-argument-parser", from: "1.8.2"),
        .package(url: "https://github.com/apple/swift-configuration", from: "1.5.0"),
        .package(url: "https://github.com/swift-server/swift-service-lifecycle", from: "2.13.0"),
        .package(url: "https://github.com/swiftlang/swift-subprocess", from: "1.0.2"),

        // --- [TELEMETRY]

        .package(url: "https://github.com/apple/swift-distributed-tracing", from: "1.7.4"),
        .package(url: "https://github.com/apple/swift-log", from: "1.17.0"),
        .package(url: "https://github.com/swift-otel/swift-otel", from: "1.6.0"),
        .package(url: "https://github.com/apple/swift-system-metrics", from: "1.3.0"),

        // --- [TESTING]

        .package(url: "https://github.com/pointfreeco/swift-custom-dump", from: "1.7.3"),
        .package(url: "https://github.com/pointfreeco/swift-issue-reporting", from: "2.1.2"),
        .package(url: "https://github.com/x-sheep/swift-property-based", from: "2.0.1"),
    ],
    targets: modules + executables + tests,
)

for target: Target in package.targets {
    target.swiftSettings = swiftSettings
    target.linkerSettings = linkerSettings + (target.type == .executable ? target.path.map(embeddedInfo) ?? [] : [])
    target.dependencies += target.type == .test ? [.product(name: "IssueReportingTestSupport", package: "swift-issue-reporting")] : []
}
