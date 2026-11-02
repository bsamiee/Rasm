public import Foundation
public import Testing

// --- [MODELS] --------------------------------------------------------------------------

public struct TestDirectory: SuiteTrait, TestTrait, TestScoping {
    @TaskLocal public static var current: URL?

    public func provideScope(for test: Test, testCase: Test.Case?, performing function: @concurrent @Sendable () async throws -> Void) async throws {
        let directory: URL = try FileManager.default.url(for: .itemReplacementDirectory, in: .userDomainMask, appropriateFor: .temporaryDirectory, create: true)
            .resolvingSymlinksInPath()
        defer {
            do { try FileManager.default.removeItem(at: directory) } catch { Issue.record(error) }
        }
        try await Self.$current.withValue(directory) { try await function() }
    }
}

extension Trait where Self == TestDirectory {
    public static var testDirectory: Self { Self() }
}
