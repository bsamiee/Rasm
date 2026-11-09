// swift-tools-version: 6.4
import PackageDescription

let package = Package(
    name: "NxApple",
    platforms: [.macOS(.v27)],
    dependencies: [.package(url: "https://github.com/apple/xcode-project-format", from: "0.1.0")],
    targets: [.executableTarget(name: "nx-apple-analyzer", dependencies: [.product(name: "XcodeProjectFormat", package: "xcode-project-format")])],
)
