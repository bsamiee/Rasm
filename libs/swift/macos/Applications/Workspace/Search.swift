import AsyncAlgorithms
import CoreServices
public import Foundation
public import Results
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

extension NSMetadataQuery {
    struct DidUpdateMessage: NotificationCenter.AsyncMessage {
        typealias Subject = NSMetadataQuery

        static var name: Notification.Name { .NSMetadataQueryDidUpdate }

        static func makeMessage(_: Notification) -> Self? { Self() }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Installation {
    @MainActor
    public static func search(_ predicate: NSPredicate, in directories: [URL] = []) -> some AsyncSequence<Result<[Self], AggregateError<ApplicationFailure>>, Never> & Sendable {
        AsyncStream { continuation in
            let following: Task<Void, Never> = Task { @MainActor in
                let query: NSMetadataQuery = NSMetadataQuery()
                query.predicate = NSCompoundPredicate(andPredicateWithSubpredicates: [NSPredicate(format: "%K == %@", NSMetadataItemContentTypeKey, UTType.applicationBundle.identifier), predicate])
                query.searchScopes = directories
                query.sortDescriptors = [NSMetadataItemDisplayNameKey, NSMetadataItemVersionKey].map { key in NSSortDescriptor(key: key, ascending: true) }
                query.operationQueue = .main
                let changes: some AsyncSequence<Void, Never> = merge(
                    NotificationCenter.default.messages(of: query, for: .didFinishGathering).map { _ in () },
                    NotificationCenter.default.messages(of: query, for: NSMetadataQuery.DidUpdateMessage.self).map { _ in () },
                )
                query.start()
                defer { query.stop() }
                for await _: Void in changes {
                    query.disableUpdates()
                    let paths: [String?] = (0..<query.resultCount).map { index in (query.result(at: index) as? NSMetadataItem)?.value(forAttribute: NSMetadataItemPathKey) as? String }
                    query.enableUpdates()
                    continuation.yield(await Self.listed(paths))
                }
                continuation.finish()
            }
            continuation.onTermination = { _ in following.cancel() }
        }
        .removeDuplicates { previous, next in if case (.success(let previous), .success(let next)) = (previous, next) { previous == next } else { false } }
    }

    @MainActor
    public static func installed(named name: String, in directories: [URL] = []) async throws(AggregateError<ApplicationFailure>) -> [Self] {
        func listing(_ comparison: NSComparisonPredicate.Operator) async throws(AggregateError<ApplicationFailure>) -> [Self] {
            let matching: NSPredicate = NSCompoundPredicate(
                orPredicateWithSubpredicates: [kMDItemCFBundleIdentifier as String, NSMetadataItemDisplayNameKey].map { key in
                    NSComparisonPredicate(
                        leftExpression: NSExpression(forKeyPath: key),
                        rightExpression: NSExpression(forConstantValue: name),
                        modifier: .direct,
                        type: comparison,
                        options: [.caseInsensitive, .diacriticInsensitive],
                    )
                }
            )
            let first: Result<[Self], AggregateError<ApplicationFailure>>? = await search(matching, in: directories).first { @Sendable _ in true }
            return try (first ?? .failure(AggregateError(first: ApplicationFailure(subject: "Spotlight query for \(name)", reason: .cancelled(CancellationError()))))).get()
        }
        return switch try await listing(.equalTo) {
            case []:
                throw AggregateError(
                    first: ApplicationFailure(
                        subject: name,
                        reason: .unmatched(Unmatched(subject: "installed application", query: name, candidates: try await listing(.contains).map(\.identifier))),
                    )
                )
            case let found: found
        }
    }

    @concurrent
    static func listed(_ paths: [String?]) async -> Result<[Self], AggregateError<ApplicationFailure>> {
        traverse(paths.enumerated()) { index, path in
            (path.map { path in Result { () throws(ApplicationFailure) in try Self(at: URL(filePath: path, directoryHint: .isDirectory)) } }
                ?? .failure(ApplicationFailure(subject: "Spotlight result \(index)", reason: .noPath)))
                .aggregated(\.self)
        }
    }
}
