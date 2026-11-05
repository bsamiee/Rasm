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

// --- [MODELS] --------------------------------------------------------------------------

let modules: [Target] = [
    // --- [CORE]
    .target(name: "Encoding", path: "libs/swift/core/Encoding"),
    .target(name: "Results", path: "libs/swift/core/Results"),
    .target(name: "JSONRPC", dependencies: ["Encoding", "Results"], path: "libs/swift/core/JSONRPC"),
    // --- [MACOS]
    .target(name: "CurrentProcess", dependencies: ["Results"], path: "libs/swift/macos/CurrentProcess"),
    .target(
        name: "Files",
        dependencies: [
            "CurrentProcess", "Results", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"), .product(name: "CasePaths", package: "swift-case-paths"),
            .product(name: "CasePathsCore", package: "swift-case-paths"),
        ],
        path: "libs/swift/macos/Files",
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
            "AppleEvents", "Applications", "JSONRPC", "Processes", "Results", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"),
            .product(name: "CasePaths", package: "swift-case-paths"), .product(name: "CasePathsCore", package: "swift-case-paths"), .product(name: "OrderedCollections", package: "swift-collections"),
        ],
        path: "libs/swift/macos/ApplicationServer",
    ),
]
let executables: [Target] = [
    .executableTarget(
        name: "watch-apps",
        dependencies: [
            "ApplicationServer", "Applications", "CurrentProcess", "Encoding", "JSONRPC", "Processes", "Results", .product(name: "ArgumentParser", package: "swift-argument-parser"),
            .product(name: "CasePaths", package: "swift-case-paths"), .product(name: "OrderedCollections", package: "swift-collections"), .product(name: "Subprocess", package: "swift-subprocess"),
        ],
        path: "tools/watch-apps",
    )
]
let support: [Target] = [
    .target(
        name: "TestSupport",
        dependencies: ["Processes", .product(name: "Subprocess", package: "swift-subprocess")],
        path: "tests/swift/support",
    ),
    .executableTarget(
        name: "AppKitFixture",
        dependencies: ["CurrentProcess", "Processes", .product(name: "Subprocess", package: "swift-subprocess")],
        path: "tests/swift/fixtures/appkit",
    ),
]
let suites: [Target] = [
    // --- [CORE]
    .testTarget(
        name: "EncodingTests",
        dependencies: ["Encoding", .product(name: "CustomDump", package: "swift-custom-dump"), .product(name: "PropertyBased", package: "swift-property-based")],
        path: "tests/swift/libs/core/Encoding",
    ),
    .testTarget(
        name: "ResultsTests",
        dependencies: [
            "Encoding", "Results", .product(name: "CasePaths", package: "swift-case-paths"), .product(name: "CustomDump", package: "swift-custom-dump"),
            .product(name: "PropertyBased", package: "swift-property-based"),
        ],
        path: "tests/swift/libs/core/Results",
    ),
    // --- [MACOS]
    .testTarget(
        name: "FilesTests",
        dependencies: [
            "Files", "TestSupport", .product(name: "CasePaths", package: "swift-case-paths"), .product(name: "CustomDump", package: "swift-custom-dump"),
            .product(name: "PropertyBased", package: "swift-property-based"),
        ],
        path: "tests/swift/libs/macos/Files",
    ),
    .testTarget(
        name: "ProcessesTests",
        dependencies: [
            "Encoding", "Processes", "TestSupport", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"), .product(name: "CasePaths", package: "swift-case-paths"),
            .product(name: "CustomDump", package: "swift-custom-dump"), .product(name: "PropertyBased", package: "swift-property-based"), .product(name: "Subprocess", package: "swift-subprocess"),
        ],
        path: "tests/swift/libs/macos/Processes",
    ),
    .testTarget(
        name: "ApplicationsTests",
        dependencies: [
            "Applications", "Encoding", "Processes", "Results", "TestSupport", .product(name: "CasePaths", package: "swift-case-paths"), .product(name: "CustomDump", package: "swift-custom-dump"),
            .product(name: "PropertyBased", package: "swift-property-based"), .product(name: "Subprocess", package: "swift-subprocess"),
        ],
        path: "tests/swift/libs/macos/Applications",
        exclude: ["Integration"],
    ),
    .testTarget(
        name: "ApplicationsIntegrationTests",
        dependencies: [
            "AppKitFixture", "Applications", "Encoding", "Processes", "TestSupport", .product(name: "CasePaths", package: "swift-case-paths"),
            .product(name: "CasePathsCore", package: "swift-case-paths"), .product(name: "CustomDump", package: "swift-custom-dump"),
            .product(name: "OrderedCollections", package: "swift-collections"), .product(name: "Subprocess", package: "swift-subprocess"),
        ],
        path: "tests/swift/libs/macos/Applications/Integration",
    ),
    .testTarget(
        name: "ApplicationServerTests",
        dependencies: ["AppleEvents", "ApplicationServer", "Applications", "Encoding", .product(name: "CustomDump", package: "swift-custom-dump")],
        path: "tests/swift/libs/macos/ApplicationServer",
        exclude: ["Integration"],
    ),
    .testTarget(
        name: "ApplicationServerIntegrationTests",
        dependencies: [
            "AppKitFixture", "ApplicationServer", "Applications", "TestSupport", .product(name: "CasePaths", package: "swift-case-paths"),
            .product(name: "CasePathsCore", package: "swift-case-paths"), .product(name: "CustomDump", package: "swift-custom-dump"),
        ],
        path: "tests/swift/libs/macos/ApplicationServer/Integration",
    ),
    // --- [TOOLS]
    .testTarget(
        name: "watch-appsTests",
        dependencies: ["Encoding", "TestSupport", "watch-apps", .product(name: "CustomDump", package: "swift-custom-dump"), .product(name: "Subprocess", package: "swift-subprocess")],
        path: "tests/swift/tools/watch-apps",
        exclude: ["Integration"],
    ),
    .testTarget(
        name: "watch-appsIntegrationTests",
        dependencies: [
            "AppKitFixture", "Encoding", "TestSupport", "watch-apps", .product(name: "CustomDump", package: "swift-custom-dump"), .product(name: "Subprocess", package: "swift-subprocess"),
        ],
        path: "tests/swift/tools/watch-apps/Integration",
    ),
]

// --- [COMPOSITION] ---------------------------------------------------------------------

let package: Package = Package(
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
        .package(url: "https://github.com/swiftlang/swift-subprocess", from: "1.0.2"),
        // --- [TESTING]
        .package(url: "https://github.com/pointfreeco/swift-custom-dump", from: "1.7.3"),
        .package(url: "https://github.com/pointfreeco/swift-issue-reporting", from: "2.1.2"),
        .package(url: "https://github.com/x-sheep/swift-property-based", from: "2.0.1"),
    ],
    targets: modules + executables + support + suites,
)

for target: Target in package.targets {
    target.swiftSettings = swiftSettings
    target.linkerSettings = linkerSettings
    target.dependencies += target.type == .test ? [.product(name: "IssueReportingTestSupport", package: "swift-issue-reporting")] : []
}
