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
        let (listings, continuation) = AsyncStream<Listing>.makeStream()
        let following = Task { @MainActor in
            defer { continuation.finish() }
            let query = NSMetadataQuery()
            query.predicate = NSPredicate(format: "%K == %@", NSMetadataItemContentTypeKey, UTType.applicationBundle.identifier)
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
            var held: [URL: (changed: Date?, read: Result<Self, ApplicationFailure>)] = [:]
            repeat {
                query.disableUpdates()
                let indexed: [(url: URL?, changed: Date?)] = (0..<query.resultCount).compactMap { index in query.result(at: index) as? NSMetadataItem }.filter { item in
                    predicate.evaluate(with: item.values(forAttributes: item.attributes))
                }.map { item in
                    (
                        url: (item.value(forAttribute: NSMetadataItemPathKey) as? String).map { path in URL(filePath: path, directoryHint: .isDirectory) },
                        changed: item.value(forAttribute: NSMetadataItemFSContentChangeDateKey) as? Date,
                    )
                }
                query.enableUpdates()
                let running = OrderedSet(
                    NSWorkspace.shared.runningApplications.filter { application in
                        application.activationPolicy == .regular && predicate.evaluate(bundleIdentifier: application.bundleIdentifier)
                    }
                    .compactMap(\.bundleURL)
                )
                .subtracting(indexed.compactMap(\.url))
                .map { url in (url: Optional(url), changed: (try? url.resourceValues(forKeys: [.contentModificationDateKey]))?.contentModificationDate) }
                let (listing, reads) = await read(indexed + running, reusing: held)
                held = reads
                continuation.yield(listing)
            } while await updates.next(isolation: #isolation) != nil
        }
        continuation.onTermination = { _ in following.cancel() }
        return listings.removeDuplicates()
    }

    public static func handlers(of document: URL, matching predicate: NSPredicate) async throws(ApplicationFailure) -> Listing {
        let subject = document.isFileURL ? document.path(percentEncoded: false) : document.absoluteString
        if document.isFileURL {
            do { _ = try document.checkResourceIsReachable() } catch { throw ApplicationFailure(subject: subject, reason: .foundation(error as NSError)) }
        }
        let (listing, _) = await read(
            OrderedSet([NSWorkspace.shared.urlForApplication(toOpen: document)].compactMap(\.self) + NSWorkspace.shared.urlsForApplications(toOpen: document)).map { url in
                (url: Optional(url), changed: nil)
            }
        )
        let matched = listing.installations.filter { installation in predicate.evaluate(bundleIdentifier: installation.identifier) }
        guard !matched.isEmpty else {
            throw ApplicationFailure(
                subject: subject,
                reason: .unmatched(Unmatched(subject: "application opening \(subject)", query: predicate.predicateFormat, candidates: listing.installations.map(\.name))),
            )
        }
        return Listing(installations: matched, unreadable: listing.unreadable)
    }

    @concurrent
    private static func read(
        _ bundles: [(url: URL?, changed: Date?)],
        reusing held: [URL: (changed: Date?, read: Result<Self, ApplicationFailure>)] = [:],
    ) async -> (listing: Listing, held: [URL: (changed: Date?, read: Result<Self, ApplicationFailure>)]) {
        let reads: [(url: URL?, changed: Date?, read: Result<Self, ApplicationFailure>)] = bundles.enumerated().map { index, bundle in
            switch (bundle.url, bundle.url.flatMap { url in held[url] }) {
                case (.some(let url), .some(let kept)) where kept.changed != nil && kept.changed == bundle.changed: (url, bundle.changed, kept.read)
                case (.some(let url), _): (url, bundle.changed, Result { () throws(ApplicationFailure) in try Self(at: url) })
                case (.none, _): (nil, nil, .failure(ApplicationFailure(subject: "Spotlight result \(index)", reason: .noPath)))
            }
        }
        return (
            Listing(installations: OrderedSet(reads.compactMap { read in read.read[case: \.success] }).elements, unreadable: reads.compactMap { read in read.read[case: \.failure] }),
            Dictionary(reads.compactMap { read in read.url.map { url in (url, (changed: read.changed, read: read.read)) } }) { first, _ in first },
        )
    }
}
