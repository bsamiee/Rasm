public import AppKit
import AsyncAlgorithms
import CasePathsCore
import OrderedCollections
import Results
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

extension Installation {
    public struct Listing: Encodable, Equatable, Sendable {
        public let installations: [Installation]
        public let unreadable: [ApplicationFailure]
    }
}

extension NSMetadataQuery {
    struct DidUpdateMessage: NotificationCenter.AsyncMessage {
        typealias Subject = NSMetadataQuery

        static var name: Notification.Name { .NSMetadataQueryDidUpdate }

        static func makeMessage(_: Notification) -> Self? { Self() }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension NSPredicate {
    public func evaluate(bundleIdentifier: String?) -> Bool {
        bundleIdentifier.map { identifier in evaluate(with: [NSMetadataItemCFBundleIdentifierKey: identifier]) } == true
    }
}

extension Installation {
    @MainActor
    public static func search(_ predicate: NSPredicate, in directories: [URL] = []) -> some AsyncSequence<Listing, Never> & Sendable {
        AsyncStream { continuation in
            let following = Task { @MainActor in
                defer { continuation.finish() }
                let query = NSMetadataQuery()
                let application = NSPredicate(format: "%K == %@", NSMetadataItemContentTypeKey, UTType.applicationBundle.identifier)
                query.predicate = metadataForm(of: predicate).map { form in NSCompoundPredicate(andPredicateWithSubpredicates: [application, form]) } ?? application
                query.searchScopes = directories
                query.sortDescriptors = [NSSortDescriptor(key: NSMetadataItemDisplayNameKey, ascending: true)]
                query.operationQueue = .main
                var gathered = NotificationCenter.default.messages(of: query, for: .didFinishGathering, bufferSize: 1).makeAsyncIterator()
                query.start()
                defer { query.stop() }
                guard await gathered.next(isolation: #isolation) != nil else { return }
                let (changes, changed) = AsyncStream<Void>.makeStream(bufferingPolicy: .bufferingNewest(1))
                let observation = NSWorkspace.shared.observe(\.runningApplications, options: [.old, .new]) { _, change in
                    if [change.oldValue, change.newValue].compactMap(\.self).joined().contains(where: { application in application.activationPolicy == .regular }) { changed.yield() }
                }
                defer { observation.invalidate() }
                var updates = merge(NotificationCenter.default.messages(of: query, for: NSMetadataQuery.DidUpdateMessage.self, bufferSize: 1).map { _ in }, changes).makeAsyncIterator()
                repeat {
                    query.disableUpdates()
                    let indexed: [URL?] = (0..<query.resultCount).map { index in
                        ((query.result(at: index) as? NSMetadataItem)?.value(forAttribute: NSMetadataItemPathKey) as? String).map { path in URL(filePath: path, directoryHint: .isDirectory) }
                    }
                    query.enableUpdates()
                    let running = OrderedSet(
                        NSWorkspace.shared.runningApplications.filter { application in
                            application.activationPolicy == .regular && predicate.evaluate(bundleIdentifier: application.bundleIdentifier)
                        }
                        .compactMap(\.bundleURL)
                    )
                    continuation.yield(await read(indexed + running.subtracting(indexed.compactMap(\.self)).map(Optional.some)))
                } while await updates.next(isolation: #isolation) != nil
            }
            continuation.onTermination = { _ in following.cancel() }
        }
        .removeDuplicates()
    }

    public static func handlers(of document: URL, matching predicate: NSPredicate) async throws(ApplicationFailure) -> Listing {
        let subject = document.isFileURL ? document.path(percentEncoded: false) : document.absoluteString
        if document.isFileURL {
            do { _ = try document.checkResourceIsReachable() } catch { throw ApplicationFailure(subject: subject, reason: .foundation(error as NSError)) }
        }
        let listing = await read(OrderedSet([NSWorkspace.shared.urlForApplication(toOpen: document)].compactMap(\.self) + NSWorkspace.shared.urlsForApplications(toOpen: document)).elements)
        let matched = listing.installations.filter { installation in predicate.evaluate(bundleIdentifier: installation.identifier) }
        guard !matched.isEmpty else {
            throw ApplicationFailure(
                subject: subject,
                reason: .unmatched(Unmatched(subject: "application opening \(subject)", query: predicate.predicateFormat, candidates: listing.installations.map(\.name))),
            )
        }
        return Listing(installations: matched, unreadable: listing.unreadable)
    }

    private static func metadataForm(of predicate: NSPredicate) -> NSPredicate? {
        guard predicate != NSPredicate(value: true) else { return nil }
        guard let compound = predicate as? NSCompoundPredicate, compound.compoundPredicateType != .not else { return predicate }
        let clauses = compound.subpredicates.compactMap { clause in (clause as? NSPredicate).flatMap(metadataForm(of:)) }
        return switch (compound.compoundPredicateType, clauses.count) {
            case (.or, ..<compound.subpredicates.count), (_, 0): nil
            case (_, 1): clauses.first
            default: NSCompoundPredicate(type: compound.compoundPredicateType, subpredicates: clauses)
        }
    }

    @concurrent
    private static func read(_ bundles: [URL?]) async -> Listing {
        let reads = bundles.enumerated().map { index, bundle in
            bundle.map { bundle in Result { () throws(ApplicationFailure) in try Self(at: bundle) } } ?? .failure(ApplicationFailure(subject: "Spotlight result \(index)", reason: .noPath))
        }
        return Listing(installations: OrderedSet(reads.compactMap { read in read[case: \.success] }).elements, unreadable: reads.compactMap { read in read[case: \.failure] })
    }
}
