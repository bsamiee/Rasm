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

// --- [COMPOSITION] ---------------------------------------------------------------------

let package: Package = Package(
    name: "RasmMacOS",
    platforms: [.macOS(.v27)],
    products: [
        .library(name: "RasmMacOS", targets: ["RasmMacOS"]),
        .executable(name: "watch-apps", targets: ["watch-apps"]),
    ],
    dependencies: [
        .package(url: "https://github.com/apple/swift-argument-parser", from: "1.8.2"),
        .package(url: "https://github.com/swiftlang/swift-subprocess", from: "1.0.2"),
    ],
    targets: [
        .target(
            name: "RasmMacOS",
            dependencies: [.product(name: "Subprocess", package: "swift-subprocess")],
            path: ".",
            exclude: ["Watch"],
        ),
        .executableTarget(
            name: "watch-apps",
            dependencies: ["RasmMacOS", .product(name: "ArgumentParser", package: "swift-argument-parser")],
            path: "Watch",
            linkerSettings: [.unsafeFlags(["-Xlinker", "-fatal_warnings"])],
        ),
    ],
)

for target: Target in package.targets {
    target.swiftSettings = settings
}
