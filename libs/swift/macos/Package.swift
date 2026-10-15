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
    .treatAllWarnings(as: .error),
    .treatWarning("StrictLanguageFeatures", as: .error),
    .unsafeFlags(["-warn-soft-deprecated"]),
]

// --- [COMPOSITION] ---------------------------------------------------------------------

let package: Package = Package(
    name: "RasmMacOS",
    platforms: [.macOS("27.0")],
    products: [.library(name: "RasmMacOS", targets: ["RasmMacOS"])],
    dependencies: [.package(url: "https://github.com/swiftlang/swift-subprocess", from: "1.0.1")],
    targets: [
        .target(
            name: "RasmMacOS",
            dependencies: [.product(name: "Subprocess", package: "swift-subprocess")],
            path: ".",
            swiftSettings: settings,
        )
    ],
)
