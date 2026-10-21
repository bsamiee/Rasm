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
    .target(name: "Results", path: "libs/swift/macos/Results"),
    .target(name: "JSON", path: "libs/swift/macos/JSON"),
    .target(name: "Files", path: "libs/swift/macos/Files"),
    .target(name: "Fonts", dependencies: ["Results"], path: "libs/swift/macos/Fonts"),
    .target(name: "Processes", dependencies: ["Results", .product(name: "Subprocess", package: "swift-subprocess")], path: "libs/swift/macos/Processes"),
    .target(name: "Applications", dependencies: ["Files", "JSON", "Results"], path: "libs/swift/macos/Applications"),
    .target(name: "AppleEvents", dependencies: ["Applications", "Results"], path: "libs/swift/macos/AppleEvents"),
]

// --- [COMPOSITION] ---------------------------------------------------------------------

let package: Package = Package(
    name: "Rasm",
    platforms: [.macOS(.v27)],
    products: modules.map { module in .library(name: module.name, targets: [module.name]) },
    dependencies: [
        .package(url: "https://github.com/apple/swift-argument-parser", from: "1.8.2"),
        .package(url: "https://github.com/swiftlang/swift-subprocess", from: "1.0.2"),
    ],
    targets: modules + [
        .executableTarget(
            name: "watch-apps",
            dependencies: ["Applications", "Files", "Results", .product(name: "ArgumentParser", package: "swift-argument-parser")],
            path: "tools/watch-apps",
        )
    ],
)

for target: Target in package.targets {
    target.swiftSettings = settings
    target.linkerSettings = [.unsafeFlags(["-Xlinker", "-fatal_warnings"])]
}
