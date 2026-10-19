// swift-tools-version: 6.4
import PackageDescription

// --- [CONSTANTS] -----------------------------------------------------------------------

let settings: [SwiftSetting] = [
    .enableUpcomingFeature("ImmutableWeakCaptures"),
    .unsafeFlags(["-warn-soft-deprecated"]),
    .strictMemorySafety(),
    .treatAllWarnings(as: .error),
    .enableUpcomingFeature("ExistentialAny"),
    .enableUpcomingFeature("InferIsolatedConformances"),
    .enableUpcomingFeature("InternalImportsByDefault"),
    .enableUpcomingFeature("MemberImportVisibility"),
    .enableUpcomingFeature("NonisolatedNonsendingByDefault"),
    .treatWarning("StrictLanguageFeatures", as: .error),
]

// --- [COMPOSITION] ---------------------------------------------------------------------

let package: Package = Package(
    name: "RasmMacOS",
    platforms: [.macOS("27.0")],
    products: [
        .library(name: "RasmMacOS", targets: ["RasmMacOS"]),
        .executable(name: "watch-apps", targets: ["watch-apps"]),
    ],
    dependencies: [
        .package(url: "https://github.com/swiftlang/swift-subprocess", from: "1.0.1")
    ],
    targets: [
        .target(
            name: "RasmMacOS",
            dependencies: [.product(name: "Subprocess", package: "swift-subprocess")],
            path: ".",
            exclude: ["Watch"],
            swiftSettings: settings,
        ),
        .executableTarget(
            name: "watch-apps",
            dependencies: ["RasmMacOS"],
            path: "Watch",
            swiftSettings: settings,
        ),
    ],
)
