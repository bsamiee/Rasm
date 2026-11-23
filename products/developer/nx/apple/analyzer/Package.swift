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
    .treatAllWarnings(as: .error),
    .treatWarning("ExplicitSendable", as: .error),
    .treatWarning("StrictLanguageFeatures", as: .error),
    .unsafeFlags(["-warn-soft-deprecated"]),
]
let linkerSettings: [LinkerSetting] = [.unsafeFlags(["-Xlinker", "-fatal_warnings"])]

// --- [COMPOSITION] ---------------------------------------------------------------------

let package = Package(
    name: "NxApple",
    platforms: [.macOS(.v27)],
    dependencies: [
        .package(url: "https://github.com/apple/swift-argument-parser", from: "1.8.2"),
        .package(url: "https://github.com/apple/xcode-project-format", from: "0.1.0"),
    ],
    targets: [
        .executableTarget(
            name: "nx-apple-analyzer",
            dependencies: [.product(name: "ArgumentParser", package: "swift-argument-parser"), .product(name: "XcodeProjectFormat", package: "xcode-project-format")],
            swiftSettings: swiftSettings + [.unsafeFlags(["-Xfrontend", "-warn-long-expression-type-checking-scopes", "-Xfrontend", "200"])],
            linkerSettings: linkerSettings,
        ),
        .testTarget(
            name: "AnalyzerTests",
            dependencies: ["nx-apple-analyzer", .product(name: "XcodeProjectFormat", package: "xcode-project-format")],
            swiftSettings: swiftSettings,
            linkerSettings: linkerSettings,
        ),
    ],
)
