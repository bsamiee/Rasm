public import Foundation

// --- [OPERATIONS] ----------------------------------------------------------------------

public func crashReports(for identifier: String) -> Result<[URL], any Error> {
    let keys: Set<URLResourceKey> = [.isRegularFileKey, .contentModificationDateKey]
    let header: Data = Data("\"bundleID\":\"\(identifier)\"".utf8)
    return Result {
        try (FileManager.default.enumerator(at: URL.libraryDirectory.appending(path: "Logs"), includingPropertiesForKeys: Array(keys), options: [.skipsHiddenFiles])?.allObjects ?? [])
            .compactMap { entry in entry as? URL }
            .map { url in (url: url, values: try url.resourceValues(forKeys: keys)) }
            .filter { entry in try entry.values.isRegularFile == true && ifPresent { try Data(contentsOf: entry.url, options: .mappedIfSafe) }.get()?.firstRange(of: header) != nil }
            .sorted { left, right in (left.values.contentModificationDate ?? .distantPast) > (right.values.contentModificationDate ?? .distantPast) }
            .map(\.url)
    }
}
