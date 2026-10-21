// swift-tools-version: 6.4
import PackageDescription

// --- [CONSTANTS] -----------------------------------------------------------------------

let settings: [SwiftSetting] = [
    .enableUpcomingFeature("ExistentialAny"),
    .enableUpcomingFeature("ImmutableWeakCaptures"),
    .enableUpcomingFeature("InferIsolatedConformances"),
    .enableUpcomingFeature("InternalImportsByDefault"),
    .enableUpcomingFeature("MemberImportVisibility"),
    .enableUpcomingFeature("NonisolatedNonsendingByDefault"),
    .strictMemorySafety(),
    .unsafeFlags(["-warn-soft-deprecated"]),
    .treatAllWarnings(as: .error),
    .treatWarning("StrictLanguageFeatures", as: .error),
]

// --- [MODELS] --------------------------------------------------------------------------

let modules: [Target] = [
    .target(name: "Results", path: "libs/swift/core/Results"),
    .target(name: "Encoding", path: "libs/swift/core/Encoding"),
    .target(name: "CurrentProcess", path: "libs/swift/macos/CurrentProcess"),
    .target(name: "Files", dependencies: ["Results"], path: "libs/swift/macos/Files"),
    .target(name: "Lockfiles", dependencies: ["Files", "Results", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms")], path: "libs/swift/macos/Lockfiles"),
    .target(name: "Fonts", dependencies: ["Results"], path: "libs/swift/macos/Fonts"),
    .target(name: "Processes", dependencies: ["Results", .product(name: "Subprocess", package: "swift-subprocess")], path: "libs/swift/macos/Processes"),
    .target(name: "Keychain", dependencies: ["Encoding", "Processes", "Results", .product(name: "Subprocess", package: "swift-subprocess")], path: "libs/swift/macos/Keychain"),
    .target(
        name: "Applications",
        dependencies: ["Encoding", "Files", "Results", .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"), .product(name: "Subprocess", package: "swift-subprocess")],
        path: "libs/swift/macos/Applications",
    ),
    .target(name: "AppleEvents", dependencies: ["Applications", "Encoding", "Results"], path: "libs/swift/macos/AppleEvents"),
    .target(name: "JSONRPC", dependencies: ["Encoding", "Results"], path: "libs/swift/rpc/JSONRPC"),
    .target(name: "EffectRPC", dependencies: ["CurrentProcess", "Encoding", "Results"], path: "libs/swift/rpc/EffectRPC"),
]

// --- [COMPOSITION] ---------------------------------------------------------------------

let package: Package = Package(
    name: "Rasm",
    platforms: [.macOS(.v27)],
    products: modules.map { module in .library(name: module.name, targets: [module.name]) },
    dependencies: [
        .package(url: "https://github.com/apple/swift-argument-parser", from: "1.8.2"),
        .package(url: "https://github.com/apple/swift-async-algorithms", from: "1.1.7"),
        .package(url: "https://github.com/apple/swift-collections", from: "1.7.3"),
        .package(url: "https://github.com/swiftlang/swift-subprocess", from: "1.0.2"),
    ],
    targets: modules + [
        .executableTarget(
            name: "watch-apps",
            dependencies: [
                "Applications", "Encoding", "Files", "Results", .product(name: "ArgumentParser", package: "swift-argument-parser"),
                .product(name: "AsyncAlgorithms", package: "swift-async-algorithms"), .product(name: "OrderedCollections", package: "swift-collections"),
                .product(name: "Subprocess", package: "swift-subprocess"),
            ],
            path: "tools/watch-apps",
        )
    ],
)

for target: Target in package.targets {
    target.swiftSettings = settings
    target.linkerSettings = [.unsafeFlags(["-Xlinker", "-fatal_warnings"])]
}
