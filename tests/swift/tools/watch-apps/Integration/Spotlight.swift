import CoreServices
import Foundation
import TestSupport

// --- [OPERATIONS] ----------------------------------------------------------------------

@MainActor
func spotlightIndexed(_ bundle: URL) async throws(TestFailure) {
    guard let identifier: String = Bundle(url: bundle)?.bundleIdentifier else { throw TestFailure(subject: bundle.path(percentEncoded: false), reason: .missing) }
    let query: NSMetadataQuery = NSMetadataQuery()
    query.predicate = NSPredicate(format: "%K == %@", kMDItemCFBundleIdentifier as String, identifier)
    query.searchScopes = [bundle.deletingLastPathComponent()]
    let identity: ObjectIdentifier = ObjectIdentifier(query)
    let owned: @Sendable (Notification) -> Bool = { notification in (notification.object as AnyObject?).map(ObjectIdentifier.init) == identity }
    var gathered: AsyncFilterSequence<NotificationCenter.Notifications>.Iterator = NotificationCenter.default.notifications(named: .NSMetadataQueryDidFinishGathering).filter(owned)
        .makeAsyncIterator()
    var updated: AsyncFilterSequence<NotificationCenter.Notifications>.Iterator = NotificationCenter.default.notifications(named: .NSMetadataQueryDidUpdate).filter(owned)
        .makeAsyncIterator()
    query.start()
    defer { query.stop() }
    _ = await gathered.next()
    while query.resultCount == 0, await updated.next() != nil {}
}
