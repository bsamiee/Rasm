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
    .target(name: "CurrentProcess", path: "libs/swift/macos/CurrentProcess"),
    .target(
        name: "Files",
        dependencies: ["CurrentProcess", "Results", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"), .product(name: "CasePaths", package: "swift-case-paths")],
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
        name: "Applications",
        dependencies: [
            "Encoding", "Files", "Processes", "Results", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"), .product(name: "CasePaths", package: "swift-case-paths"),
            .product(name: "OrderedCollections", package: "swift-collections"), .product(name: "Subprocess", package: "swift-subprocess"),
        ],
        path: "libs/swift/macos/Applications",
    ),
    .target(
        name: "AppleEvents",
        dependencies: ["Applications", "Encoding", "Results", .product(name: "CasePaths", package: "swift-case-paths"), .product(name: "OrderedCollections", package: "swift-collections")],
        path: "libs/swift/macos/AppleEvents",
    ),
]
let executables: [Target] = [
    .executableTarget(
        name: "watch-apps",
        dependencies: [
            "Applications", "Encoding", "Processes", "Results", .product(name: "ArgumentParser", package: "swift-argument-parser"),
            .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"), .product(name: "CasePaths", package: "swift-case-paths"),
            .product(name: "OrderedCollections", package: "swift-collections"),
        ],
        path: "tools/watch-apps",
    )
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
    ],
    targets: modules + executables,
)

for target: Target in package.targets {
    target.swiftSettings = swiftSettings
    target.linkerSettings = linkerSettings
}
