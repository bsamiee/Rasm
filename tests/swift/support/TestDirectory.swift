public import Foundation
public import Testing

// --- [MODELS] --------------------------------------------------------------------------

public struct TestDirectory: SuiteTrait, TestTrait, TestScoping {
    @TaskLocal public static var current: URL?

    public var isRecursive: Bool { true }

    public func provideScope(for test: Test, testCase: Test.Case?, performing function: @concurrent @Sendable () async throws -> Void) async throws {
        let created: URL = try FileManager.default.url(for: .itemReplacementDirectory, in: .userDomainMask, appropriateFor: .temporaryDirectory, create: true)
        defer {
            do { try FileManager.default.removeItem(at: created) } catch { Issue.record(error) }
        }
        guard let canonical: String = try created.resourceValues(forKeys: [.canonicalPathKey]).canonicalPath else {
            throw TestFailure(subject: created.path(percentEncoded: false), reason: .missing)
        }
        let directory: URL = URL(filePath: canonical, directoryHint: .isDirectory)
        try await Self.$current.withValue(directory) { try await function() }
    }
}

extension Trait where Self == TestDirectory {
    public static var testDirectory: Self { Self() }
}
