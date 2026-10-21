import Files
public import Foundation
public import Results

// --- [OPERATIONS] ----------------------------------------------------------------------

public func crashReports(for identifier: String) -> Result<[URL], AggregateError<(url: URL, error: any Error)>> {
    let keys: Set<URLResourceKey> = [.isDirectoryKey, .isRegularFileKey, .contentModificationDateKey]
    let header: Data = Data("\"bundleID\":\"\(identifier)\"".utf8)
    func reports(at url: URL) -> Result<[(url: URL, date: Date?)?], AggregateError<(url: URL, error: any Error)>> {
        switch ifPresent({ try url.resourceValues(forKeys: keys) }) {
            case .failure(let error): .failure(AggregateError(first: (url, error)))
            case .success(.some(let values)) where values.isDirectory == true:
                ifPresent { try FileManager.default.contentsOfDirectory(at: url, includingPropertiesForKeys: Array(keys), options: [.skipsHiddenFiles]) }
                    .aggregated { error in (url, error) }
                    .flatMap { entries in traverse(entries ?? [], reports).map { found in found.flatMap(\.self) } }
            case .success(.some(let values)) where values.isRegularFile == true:
                ifPresent { try FileHandle(forReadingFrom: url).read(upToCount: 4096)?.range(of: header).map { _ in (url, values.contentModificationDate) } }
                    .aggregated { error in (url, error) }
                    .map { found in [found].compactMap(\.self) }
            case .success: .success([])
        }
    }
    return reports(at: URL.libraryDirectory.appending(path: "Logs")).map { found in
        let reports: [(url: URL, date: Date?)] = found.compactMap(\.self)
        return reports.sorted(using: KeyPathComparator(\.date, order: .reverse)).map(\.url)
    }
}
