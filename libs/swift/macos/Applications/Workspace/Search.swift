public import AppKit
import AsyncAlgorithms
import CasePathsCore
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
        listings(predicate, in: directories)
            .map { listed in AggregateError(listed.failures).map(Result.failure) ?? .success(listed.installations) }
            .removeDuplicates { previous, next in if case (.success(let previous), .success(let next)) = (previous, next) { previous == next } else { false } }
    }

    private static func metadataForm(of predicate: NSPredicate) -> NSPredicate? {
        guard predicate != NSPredicate(value: true) else { return nil }
        guard let compound = predicate as? NSCompoundPredicate, compound.compoundPredicateType != .not else { return predicate }
        let clauses = compound.subpredicates.compactMap { $0 as? NSPredicate }.compactMap(metadataForm(of:))
        return if compound.compoundPredicateType == .or, clauses.count < compound.subpredicates.count {
            nil
        } else if clauses.count > 1 {
            NSCompoundPredicate(type: compound.compoundPredicateType, subpredicates: clauses)
        } else {
            clauses.first
        }
    }

    @MainActor
    private static func listings(_ predicate: NSPredicate, in directories: [URL]) -> some AsyncSequence<(installations: [Self], failures: [ApplicationFailure]), Never> & Sendable {
        AsyncStream { continuation in
            let following = Task { @MainActor in
                let query = NSMetadataQuery()
                let application = NSPredicate(format: "%K == %@", NSMetadataItemContentTypeKey, UTType.applicationBundle.identifier)
                query.predicate = metadataForm(of: NSCompoundPredicate(andPredicateWithSubpredicates: [application, predicate])) ?? application
                query.searchScopes = directories
                query.sortDescriptors = [NSSortDescriptor(key: NSMetadataItemDisplayNameKey, ascending: true)]
                query.operationQueue = .main
                var gathered = NotificationCenter.default.messages(of: query, for: .didFinishGathering, bufferSize: 1).makeAsyncIterator()
                query.start()
                defer { query.stop() }
                guard await gathered.next(isolation: #isolation) != nil else { return continuation.finish() }
                var updates = NotificationCenter.default.messages(of: query, for: NSMetadataQuery.DidUpdateMessage.self, bufferSize: 1).makeAsyncIterator()
                repeat {
                    query.disableUpdates()
                    let paths = (0..<query.resultCount).map { index in (query.result(at: index) as? NSMetadataItem)?.value(forAttribute: NSMetadataItemPathKey) as? String }
                    query.enableUpdates()
                    continuation.yield(await Self.listed(paths))
                } while await updates.next(isolation: #isolation) != nil
                continuation.finish()
            }
            continuation.onTermination = { _ in following.cancel() }
        }
    }

    @concurrent
    static func listed(_ paths: [String?]) async -> (installations: [Self], failures: [ApplicationFailure]) {
        let reads = paths.enumerated().map { index, path in
            path.map { path in Result { () throws(ApplicationFailure) in try Self(at: URL(filePath: path, directoryHint: .isDirectory)) } }
                ?? .failure(ApplicationFailure(subject: "Spotlight result \(index)", reason: .noPath))
        }
        return (reads.compactMap { read in read[case: \.success] }, reads.compactMap { read in read[case: \.failure] })
    }
}
