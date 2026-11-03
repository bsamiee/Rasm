public import AppKit
import AsyncAlgorithms
import CasePathsCore
import CoreServices
import OrderedCollections
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

    @MainActor
    public static func installed(named name: String, in directories: [URL] = []) async throws(ApplicationFailure) -> [Self] {
        func listing(_ comparison: NSComparisonPredicate.Operator) async throws(ApplicationFailure) -> [Self] {
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
            guard let listed: [Self] = await listings(matching, in: directories).first(where: { @Sendable _ in true })?.installations else {
                throw ApplicationFailure(subject: "Spotlight query for \(name)", reason: .cancelled(CancellationError()))
            }
            return listed
        }
        if name.hasPrefix("/") { return [try Self(at: URL(filePath: name, directoryHint: .isDirectory))] }
        let registered: [Self] = NSWorkspace.shared.urlsForApplications(withBundleIdentifier: name).compactMap { url in
            Result { () throws(ApplicationFailure) in try Self(at: url) }[case: \.success]
        }
        guard registered.isEmpty else { return registered }
        let exact: [Self] = try await listing(.equalTo)
        let found: [Self] = try await exact.isEmpty ? listing(.contains) : exact
        let identifiers: OrderedSet<String> = OrderedSet(found.map(\.identifier))
        guard identifiers.count != 1 else { return found }
        throw ApplicationFailure(subject: name, reason: .unmatched(Unmatched(subject: "installed application", query: name, candidates: Array(identifiers))))
    }

    @MainActor
    private static func listings(_ predicate: NSPredicate, in directories: [URL]) -> some AsyncSequence<(installations: [Self], failures: [ApplicationFailure]), Never> & Sendable {
        AsyncStream { continuation in
            let following: Task<Void, Never> = Task { @MainActor in
                let query: NSMetadataQuery = NSMetadataQuery()
                query.predicate = NSCompoundPredicate(andPredicateWithSubpredicates: [NSPredicate(format: "%K == %@", NSMetadataItemContentTypeKey, UTType.applicationBundle.identifier), predicate])
                query.searchScopes = directories
                query.sortDescriptors = [NSMetadataItemDisplayNameKey, NSMetadataItemVersionKey].map { key in NSSortDescriptor(key: key, ascending: true) }
                query.operationQueue = .main
                let (changes, change): (AsyncStream<Void>, AsyncStream<Void>.Continuation) = AsyncStream.makeStream(bufferingPolicy: .bufferingNewest(1))
                let observations: [NotificationCenter.ObservationToken] = [
                    NotificationCenter.default.addObserver(of: query, for: .didFinishGathering) { _ in change.yield() },
                    NotificationCenter.default.addObserver(of: query, for: NSMetadataQuery.DidUpdateMessage.self) { _ in change.yield() },
                ]
                query.start()
                defer {
                    query.stop()
                    observations.forEach(NotificationCenter.default.removeObserver)
                }
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
    }

    @concurrent
    static func listed(_ paths: [String?]) async -> (installations: [Self], failures: [ApplicationFailure]) {
        let reads: [Result<Self, ApplicationFailure>] = paths.enumerated().map { index, path in
            path.map { path in Result { () throws(ApplicationFailure) in try Self(at: URL(filePath: path, directoryHint: .isDirectory)) } }
                ?? .failure(ApplicationFailure(subject: "Spotlight result \(index)", reason: .absent(NSMetadataItemPathKey)))
        }
        return (reads.compactMap { read in read[case: \.success] }, reads.compactMap { read in read[case: \.failure] })
    }
}
