import AppKit
import AsyncAlgorithms
import CasePathsCore
import Files
package import Foundation
import OrderedCollections
import Results
import Synchronization
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

extension Installation {
    package struct Listing: Encodable, Equatable, Sendable {
        let installations: [Installation]
        let unreadable: [ApplicationFailure]
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
    package func evaluate(bundleIdentifier: String?, displayName: String? = nil, bundle: URL? = nil) -> Bool {
        bundleIdentifier.map { identifier in
            evaluate(with: [NSMetadataItemCFBundleIdentifierKey: identifier, NSMetadataItemDisplayNameKey: displayName, NSMetadataItemFSNameKey: bundle?.lastPathComponent].compactMapValues(\.self))
        } == true
    }

    fileprivate func metadataQueryPredicate(within candidates: NSPredicate, negated: Bool = false) -> NSPredicate {
        let compound = self as? NSCompoundPredicate
        let inner: Bool = negated != (compound?.compoundPredicateType == .not)
        let subpredicates = (compound?.subpredicates ?? []).compactMap { subpredicate in (subpredicate as? NSPredicate)?.metadataQueryPredicate(within: candidates, negated: inner) }
        let queried: Bool =
            (self as? NSComparisonPredicate).map { (comparison: NSComparisonPredicate) -> Bool in
                let right: NSExpression = comparison.rightExpression
                let constant: Any? = right.expressionType == .constantValue ? right.constantValue : nil
                let operands: Bool = [.keyPath, .anyKey].contains(comparison.leftExpression.expressionType) && (constant is String || constant is NSNumber || constant is Date)
                return operands && ![.matches, .customSelector, .between, .in].contains(comparison.predicateOperatorType)
            } ?? false
        return switch (compound?.compoundPredicateType, subpredicates.first, subpredicates.count) {
            case (.not, let subpredicate?, _): NSCompoundPredicate(notPredicateWithSubpredicate: subpredicate)
            case (.and, _, 0): candidates
            case (.or, _, 0): NSCompoundPredicate(notPredicateWithSubpredicate: candidates)
            case (_?, let subpredicate?, 1): subpredicate
            case (let type?, _, _): NSCompoundPredicate(type: type, subpredicates: subpredicates)
            case (nil, _, _) where self == NSPredicate(value: true): candidates
            case (nil, _, _) where self == NSPredicate(value: false): NSCompoundPredicate(notPredicateWithSubpredicate: candidates)
            case (nil, _, _) where queried: self
            case (nil, _, _): negated ? NSCompoundPredicate(notPredicateWithSubpredicate: candidates) : candidates
        }
    }
}

extension Installation {
    @MainActor
    package static func search(_ predicate: NSPredicate, in directories: [URL]) -> some AsyncSequence<Listing, Never> & Sendable {
        let (listings, continuation) = AsyncStream<Listing>.makeStream(bufferingPolicy: .bufferingNewest(1))
        let (changes, changed) = AsyncStream<ApplicationFailure?>.makeStream()
        let edits = Task {
            let notices = FileEvent.events(of: directories).filter { (batch: Result<[FileEvent], FileFailure>) -> Bool in
                batch[case: \.success]?.contains { (event: FileEvent) -> Bool in
                    let moved: Bool = event.flags.contains(.itemIsDir) && !event.flags.isDisjoint(with: [.itemCreated, .itemRemoved, .itemRenamed])
                    return event.mustScanSubDirs || event.url.pathComponents.suffix(2).elementsEqual(["Contents", "Info.plist"]) || moved && directories.contains(event.url.deletingLastPathComponent())
                } != false
            }
            for await batch in notices { changed.yield(batch[case: \.failure].map { failure in ApplicationFailure(subject: failure.subject, reason: .foundation(failure as NSError)) }) }
        }
        let following = Task { @MainActor in
            defer { continuation.finish() }
            let applications = NSPredicate(format: "%K == %@", NSMetadataItemContentTypeKey, UTType.applicationBundle.identifier)
            let query = NSMetadataQuery()
            query.predicate = NSCompoundPredicate(andPredicateWithSubpredicates: [applications, predicate.metadataQueryPredicate(within: applications)])
            query.searchScopes = directories
            query.sortDescriptors = [NSSortDescriptor(key: NSMetadataItemDisplayNameKey, ascending: true)]
            query.operationQueue = .main
            var gathered = NotificationCenter.default.messages(of: query, for: .didFinishGathering, bufferSize: 1).makeAsyncIterator()
            query.start()
            defer { query.stop() }
            guard await gathered.next(isolation: #isolation) != nil else { return }
            let observation = NSWorkspace.shared.observe(\.runningApplications, options: [.old, .new]) { _, change in
                if [change.oldValue, change.newValue].compactMap(\.self).joined().contains(where: { application in application.activationPolicy == .regular }) { changed.yield(nil) }
            }
            defer { observation.invalidate() }
            let updates = merge(NotificationCenter.default.messages(of: query, for: NSMetadataQuery.DidUpdateMessage.self, bufferSize: 1).map { _ in ApplicationFailure?.none }, changes)
            var (held, unwatched): ([URL: (modified: [Date?], read: Result<Self, ApplicationFailure>)], [ApplicationFailure]) = ([:], [])
            for await update in chain([ApplicationFailure?.none].async, updates) {
                unwatched += [update].compactMap(\.self)
                query.disableUpdates()
                let indexed = (0..<query.resultCount).map { (index: Int) -> Result<URL, ApplicationFailure> in
                    let item = query.result(at: index) as? NSMetadataItem
                    let path: String? = item?.value(forAttribute: NSMetadataItemPathKey) as? String
                    let name: String? = item?.value(forAttribute: NSMetadataItemDisplayNameKey) as? String
                    let subject: String = name.map { (name: String) -> String in "Spotlight item \(name)" } ?? "Spotlight item"
                    let located: Result<URL, ApplicationFailure>? = path.map { (path: String) -> Result<URL, ApplicationFailure> in .success(URL(filePath: path, directoryHint: .isDirectory)) }
                    return located ?? .failure(ApplicationFailure(subject: subject, reason: .noPath))
                }
                query.enableUpdates()
                let bundles: [URL] = indexed.compactMap { result in result[case: \.success] }
                let running = OrderedSet(
                    NSWorkspace.shared.runningApplications.filter { application in
                        application.activationPolicy == .regular
                            && predicate.evaluate(bundleIdentifier: application.bundleIdentifier, displayName: application.localizedName, bundle: application.bundleURL)
                    }
                    .compactMap(\.bundleURL)
                )
                .subtracting(bundles)
                let (listing, reads) = await read(bundles + running, reusing: held)
                held = reads
                let matched: [Self] = listing.installations.filter { installation in
                    predicate.evaluate(bundleIdentifier: installation.identifier, displayName: installation.name, bundle: installation.url)
                }
                continuation.yield(Listing(installations: matched, unreadable: unwatched + indexed.compactMap { result in result[case: \.failure] } + listing.unreadable))
            }
        }
        continuation.onTermination = { _ in
            following.cancel()
            edits.cancel()
        }
        return listings.removeDuplicates()
    }

    package static func handlers(of document: URL, matching predicate: NSPredicate) async throws(ApplicationFailure) -> Listing {
        let subject = document.isFileURL ? document.path(percentEncoded: false) : document.absoluteString
        if document.isFileURL {
            do { _ = try document.checkResourceIsReachable() } catch { throw ApplicationFailure(subject: subject, reason: .foundation(error as NSError)) }
        }
        let (listing, _) = await read(OrderedSet([NSWorkspace.shared.urlForApplication(toOpen: document)].compactMap(\.self) + NSWorkspace.shared.urlsForApplications(toOpen: document)).elements)
        let matched = listing.installations.filter { installation in predicate.evaluate(bundleIdentifier: installation.identifier, displayName: installation.name, bundle: installation.url) }
        guard !matched.isEmpty else {
            throw ApplicationFailure(
                subject: subject,
                reason: .unmatched(Unmatched(subject: "application opening \(subject) that this server drives", query: nil, candidates: listing.installations.map(\.name))),
            )
        }
        return Listing(installations: matched, unreadable: listing.unreadable)
    }

    @concurrent
    private static func read(
        _ bundles: [URL],
        reusing held: [URL: (modified: [Date?], read: Result<Self, ApplicationFailure>)] = [:],
    ) async -> (listing: Listing, held: [URL: (modified: [Date?], read: Result<Self, ApplicationFailure>)]) {
        let finished: Mutex<[URL: (modified: [Date?], read: Result<Self, ApplicationFailure>)]> = Mutex([:])
        DispatchQueue.concurrentPerform(iterations: bundles.count) { (index: Int) in
            let (url, plist): (URL, URL) = (bundles[index], bundles[index].appending(components: "Contents", "Info.plist"))
            let modified: Result<[Date?]?, any Error> = Result { () throws -> [Date?]? in
                try ifPresent { () throws -> [Date?] in
                    let attributes: Date? = try url.resourceValues(forKeys: [.attributeModificationDateKey]).attributeModificationDate
                    let contents: Date? = try plist.resourceValues(forKeys: [.contentModificationDateKey]).contentModificationDate
                    return [attributes, contents]
                }
            }
            let kept: (modified: [Date?], read: Result<Self, ApplicationFailure>)? = held[url]
            let entry: (modified: [Date?], read: Result<Self, ApplicationFailure>)? =
                switch (modified, kept) {
                    case (.success(nil), _): nil
                    case (.success(let modified?), let kept?) where kept.modified == modified && !modified.contains(nil): kept
                    case (.success(let modified?), _): (modified, Result { () throws(ApplicationFailure) -> Self in try Self(at: url) })
                    case (.failure(let error), _): ([], .failure(ApplicationFailure(subject: url.path(percentEncoded: false), reason: .foundation(error as NSError))))
                }
            finished.withLock { finished in finished[url] = entry }
        }
        let reads = finished.withLock { finished in finished }
        let ordered: [Result<Self, ApplicationFailure>] = bundles.compactMap { url in reads[url]?.read }
        return (Listing(installations: OrderedSet(ordered.compactMap { read in read[case: \.success] }).elements, unreadable: ordered.compactMap { read in read[case: \.failure] }), reads)
    }
}
