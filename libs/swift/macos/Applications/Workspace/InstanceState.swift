import AppKit
@unsafe @preconcurrency import ApplicationServices
import AsyncAlgorithms
package import CasePaths
import CasePathsCore
package import Encoding
import Processes
import SPI
import Subprocess
import System

// --- [MODELS] --------------------------------------------------------------------------

@CasePathable
package enum InstanceState: CaseTagged, Equatable, Sendable {
    package struct Activity: Equatable, Sendable {
        @CasePathable
        enum Status: CaseTagged, Equatable, Sendable {
            case launching
            case responding
            case unresponsive(thread: ProcessStatus)

            var isIdle: Bool {
                switch self {
                    case .launching, .unresponsive(.running), .unresponsive(.runnable), .unresponsive(.throttled), .unresponsive(.uninterruptible): false
                    case .responding, .unresponsive(.sleeping), .unresponsive(.stopped), .unresponsive(.traced): true
                }
            }
        }

        let status: Status
        let pendingEventAge: Duration?
        let active: Bool
        let windows: [Window]
        let usage: Result<ResourceUsage.Interval, Errno>?
        let statusItems: Result<Int, ApplicationFailure>?

        var asking: [Window] { windows.filter(\.asking) }
    }

    struct Window: Codable, Equatable, Sendable {
        enum Band: String, CaseIterable, Codable, Sendable {
            case normal
            case floating
            case modalPanel
            case utility
            case dock
            case mainMenu
            case status
            case popUpMenu
            case overlay
            case help
            case dragging
            case screenSaver
            case assistiveTechHigh
            case cursor

            var level: Int {
                let key: CGWindowLevelKey =
                    switch self {
                        case .normal: .normalWindow
                        case .floating: .floatingWindow
                        case .modalPanel: .modalPanelWindow
                        case .utility: .utilityWindow
                        case .dock: .dockWindow
                        case .mainMenu: .mainMenuWindow
                        case .status: .statusWindow
                        case .popUpMenu: .popUpMenuWindow
                        case .overlay: .overlayWindow
                        case .help: .helpWindow
                        case .dragging: .draggingWindow
                        case .screenSaver: .screenSaverWindow
                        case .assistiveTechHigh: .assistiveTechHighWindow
                        case .cursor: .cursorWindow
                    }
                return Int(CGWindowLevelForKey(key))
            }

            private static let floors: [(level: Int, band: Self)] = allCases.map { band in (band.level, band) }.sorted { first, second in first.level > second.level }

            init(layer: Int) { self = Self.floors.first { floor in floor.level <= layer }?.band ?? .normal }
        }

        let number: CGWindowID
        let title: String?
        let frame: CGRect
        let layer: Int
        let band: Band
        let parent: CGWindowID?
        let spaces: [UInt64]
        let onScreen: Bool
        let hidden: Bool
        let shared: Bool
        let remote: CGWindowID?
        var container: Int?
        var kind: Container.Kind?
        var document: URL?
        var asking: Bool
        var presented: Bool
    }

    package struct Termination: Equatable, Sendable {
        let exit: ProcessExit
        let report: Result<DiagnosticReport, ProcessFailure>?
    }

    case running(Activity)
    case failed(ApplicationFailure)
    case exited(termination: Termination)
}

extension InstanceState.Window {
    static func all(_ listed: CGWindowListOption, relativeTo window: CGWindowID = kCGNullWindowID, of owner: pid_t? = nil) -> [pid_t: [Self]] {
        let information: [[String: Any]] = CGWindowListCopyWindowInfo(listed, window) as? [[String: Any]] ?? []
        let listing: [(owner: pid_t, number: CGWindowID, title: String?, frame: CGRect, layer: Int, onScreen: Bool, shared: Bool)] = information.compactMap {
            (row: [String: Any]) -> (owner: pid_t, number: CGWindowID, title: String?, frame: CGRect, layer: Int, onScreen: Bool, shared: Bool)? in
            guard let owner = row[kCGWindowOwnerPID as String] as? pid_t, let number = row[kCGWindowNumber as String] as? CGWindowID, let layer = row[kCGWindowLayer as String] as? Int, layer >= 0,
                let bounds = row[kCGWindowBounds as String] as? [String: Any], let frame = CGRect(dictionaryRepresentation: bounds as CFDictionary), !frame.isEmpty
            else { return nil }
            let named: String? = row[kCGWindowName as String] as? String
            let title: String? = named?.isEmpty == false ? named : nil
            let onScreen: Bool = row[kCGWindowIsOnscreen as String] as? Bool ?? false
            let shared: Bool = row[kCGWindowSharingState as String] as? UInt32 != CGWindowSharingType.none.rawValue
            return (owner: owner, number: number, title: title, frame: frame, layer: layer, onScreen: onScreen, shared: shared)
        }
        let owners: Set<pid_t> = Set(listing.map(\.owner))
        let responsibilities: [(server: pid_t, responsible: pid_t)] = owners.compactMap { (server: pid_t) -> (server: pid_t, responsible: pid_t)? in
            let responsible: pid_t = responsiblePID(server)
            return responsible == server || responsible == -1 ? nil : (server: server, responsible: responsible)
        }
        let servers: [pid_t: pid_t] = Dictionary(uniqueKeysWithValues: responsibilities)
        let hosted: [(owner: pid_t, number: CGWindowID, title: String?, frame: CGRect, layer: Int, onScreen: Bool, shared: Bool)] = listing.filter {
            (row: (owner: pid_t, number: CGWindowID, title: String?, frame: CGRect, layer: Int, onScreen: Bool, shared: Bool)) -> Bool in servers[row.owner] != nil
        }
        let served: [CGRect: [(owner: pid_t, number: CGWindowID, title: String?, frame: CGRect, layer: Int, onScreen: Bool, shared: Bool)]] = Dictionary(grouping: hosted) {
            (row: (owner: pid_t, number: CGWindowID, title: String?, frame: CGRect, layer: Int, onScreen: Bool, shared: Bool)) -> CGRect in row.frame
        }
        let rows: [(owner: pid_t, number: CGWindowID, title: String?, frame: CGRect, layer: Int, onScreen: Bool, shared: Bool)] = listing.filter {
            (row: (owner: pid_t, number: CGWindowID, title: String?, frame: CGRect, layer: Int, onScreen: Bool, shared: Bool)) -> Bool in
            owner.map { (owner: pid_t) -> Bool in row.owner == owner } ?? true
        }
        let numbers: [CGWindowID] = rows.map(\.number)
        let iterator: WindowIterator? = queryWindows(mainConnection(), numbers as CFArray, .spaces).flatMap(WindowIterator.init)
        let queried: [(CGWindowID, (parent: CGWindowID?, spaces: [UInt64], shown: Bool))] = Array(
            sequence(state: iterator) { (iterator: inout WindowIterator?) -> (CGWindowID, (parent: CGWindowID?, spaces: [UInt64], shown: Bool))? in
                guard let current = iterator, current.advance() else { return nil }
                let spaces: [UInt64] = Array(sequence(state: current.spaces) { (spaces: inout SpaceIterator?) -> UInt64? in spaces?.advance() == true ? spaces?.space : nil })
                let parent: CGWindowID? = current.parent == kCGNullWindowID ? nil : current.parent
                return (current.window, (parent: parent, spaces: spaces, shown: current.spaceAttributes.contains(.orderedIn)))
            }
        )
        let placements: [CGWindowID: (parent: CGWindowID?, spaces: [UInt64], shown: Bool)] = Dictionary(queried) {
            (first: (parent: CGWindowID?, spaces: [UInt64], shown: Bool), _: (parent: CGWindowID?, spaces: [UInt64], shown: Bool)) -> (parent: CGWindowID?, spaces: [UInt64], shown: Bool) in first
        }
        let windows: [(owner: pid_t, window: Self)] = rows.map {
            (row: (owner: pid_t, number: CGWindowID, title: String?, frame: CGRect, layer: Int, onScreen: Bool, shared: Bool)) -> (owner: pid_t, window: Self) in
            let placement: (parent: CGWindowID?, spaces: [UInt64], shown: Bool)? = placements[row.number]
            let twin: CGWindowID? = served[row.frame]?.first { (twin: (owner: pid_t, number: CGWindowID, title: String?, frame: CGRect, layer: Int, onScreen: Bool, shared: Bool)) -> Bool in
                servers[twin.owner] == row.owner && twin.title == row.title
            }?.number
            let built = Self(
                number: row.number,
                title: row.title,
                frame: row.frame,
                layer: row.layer,
                band: Band(layer: row.layer),
                parent: placement?.parent,
                spaces: placement?.spaces ?? [],
                onScreen: row.onScreen,
                hidden: !row.onScreen && placement?.shown == true,
                shared: row.shared,
                remote: twin,
                asking: false,
                presented: row.onScreen,
            )
            return (owner: row.owner, window: built)
        }
        let grouped: [pid_t: [(owner: pid_t, window: Self)]] = Dictionary(grouping: windows) { (entry: (owner: pid_t, window: Self)) -> pid_t in entry.owner }
        return grouped.mapValues { (owned: [(owner: pid_t, window: Self)]) -> [Self] in owned.map(\.window) }
    }

    func described(by container: Container?, blocking: Bool) -> Self {
        var described = self
        (described.container, described.kind, described.document) = (container?.ref, container?.kind, container?.document)
        (described.asking, described.presented) = (container?.asking ?? blocking, container != nil || onScreen)
        return described
    }
}

extension InstanceState.Activity {
    static func statusBars(in windows: [pid_t: [InstanceState.Window]]) -> [(host: pid_t, number: CGWindowID)] {
        let hosts: [pid_t: [InstanceState.Window]] = windows.filter { (entry: (key: pid_t, value: [InstanceState.Window])) -> Bool in NSRunningApplication(processIdentifier: entry.key) != nil }
        return hosts.flatMap { (entry: (key: pid_t, value: [InstanceState.Window])) -> [(host: pid_t, number: CGWindowID)] in
            let bars: [InstanceState.Window] = entry.value.filter { (row: InstanceState.Window) -> Bool in row.band == .mainMenu }
            return bars.map { (row: InstanceState.Window) -> (host: pid_t, number: CGWindowID) in (host: entry.key, number: row.number) }
        }
    }

    static func statusItems(hostedIn windows: [pid_t: [InstanceState.Window]]) throws(ApplicationFailure) -> [pid_t: Int] {
        let counts: [[pid_t: Int]] = try statusBars(in: windows).map { bar throws(ApplicationFailure) -> [pid_t: Int] in
            let shown: [AXUIElement] = try AXUIElementCreateApplication(bar.host).elements(kAXWindowsAttribute).filter { window in window.window == bar.number }
            let items: [AXUIElement] = Array(try shown.map { window throws(ApplicationFailure) -> [AXUIElement] in try window.elements(kAXChildrenAttribute) }.joined())
            let owners: [pid_t?] = try items.map { item throws(ApplicationFailure) -> pid_t? in try (item.elements(kAXChildrenAttribute).first ?? item).process }
            return Dictionary(owners.compactMap(\.self).map { owner in (owner, 1) }, uniquingKeysWith: +)
        }
        return counts.reduce(into: [:]) { merged, bar in merged.merge(bar, uniquingKeysWith: max) }
    }
}

extension InstanceState.Termination: Encodable {
    private enum Key: String, CodingKey {
        case tag = "_tag"
        case status, signal, detail, statusFailure, report, reportFailure, reporting
    }

    package func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        let (field, value): (Key, any Encodable) =
            switch exit.status {
                case .success(.exited(let status)): (.status, status)
                case .success(.signaled(let signal)): (.signal, signal)
                case .failure(let error): (.statusFailure, error.description)
            }
        try fields.encode(field.stringValue, forKey: .tag)
        try fields.encode(value, forKey: field)
        if !exit.detail.isEmpty { try fields.encode(exit.detail, forKey: .detail) }
        if report == nil, exit.producesReport { try fields.encode(true, forKey: .reporting) }
        try fields.encodeIfPresent(report?[case: \.success], forKey: .report)
        let reportFailure: ProcessFailure.Reason? = report?[case: \.failure]?.reason
        try fields.encodeIfPresent(reportFailure, forKey: .reportFailure)
    }
}

extension InstanceState.Activity: Encodable {
    private enum Key: CodingKey {
        case active, status, pendingEventAge, usage, usageFailure, windows, statusItems, statusItemsFailure
    }

    package func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        try fields.encode(active, forKey: .active)
        try fields.encode(status, forKey: .status)
        let age: Double? = pendingEventAge.map { (held: Duration) -> Double in held / Duration.seconds(1) }
        try fields.encodeIfPresent(age, forKey: .pendingEventAge)
        try fields.encodeIfPresent(usage?[case: \.success], forKey: .usage)
        let usageFailure: String? = usage?[case: \.failure]?.description
        try fields.encodeIfPresent(usageFailure, forKey: .usageFailure)
        try fields.encode(asking + windows.filter { window in !window.asking }, forKey: .windows)
        try fields.encodeIfPresent(statusItems?[case: \.success], forKey: .statusItems)
        let statusItemsFailure: String? = statusItems?[case: \.failure]?.errorDescription
        try fields.encodeIfPresent(statusItemsFailure, forKey: .statusItemsFailure)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Instance {
    func lifetime(clock: some Clock<Duration> & Sendable) -> some AsyncSequence<InstanceState, Never> & Sendable {
        let (ended, ending) = AsyncStream<ProcessExit?>.makeStream()
        let (terminated, terminating) = AsyncStream<InstanceState?>.makeStream()
        let subject: String = "Exit events of \(application.localizedName ?? application.bundleIdentifier ?? "the application")"
        let cut = process.exits().map { (exit: Result<ProcessExit, Errno>) -> InstanceState? in
            guard case .failure(let error) = exit else {
                ending.yield(exit[case: \.success])
                ending.finish()
                return nil
            }
            let observation: NSKeyValueObservation = self.application.observe(\.isTerminated, options: [.initial]) {
                (application: NSRunningApplication, _: NSKeyValueObservedChange<Bool>) -> Void in
                guard application.isTerminated else { return }
                ending.yield(nil)
                ending.finish()
                terminating.yield(nil)
            }
            terminating.onTermination = { (_: AsyncStream<InstanceState?>.Continuation.Termination) -> Void in observation.invalidate() }
            return .failed(ApplicationFailure(subject: subject, reason: .system(error)))
        }
        let lived = merge(cut, terminated).prefix { (state: InstanceState?) -> Bool in state != nil }.compactMap(\.self)
        let exits = ended.flatMap { (exit: ProcessExit?) -> AsyncStream<Result<ProcessExit, Errno>> in
            exit.map { (exit: ProcessExit) -> AsyncStream<Result<ProcessExit, Errno>> in
                AsyncStream { (continuation: AsyncStream<Result<ProcessExit, Errno>>.Continuation) -> Void in
                    continuation.yield(.success(exit))
                    continuation.finish()
                }
            } ?? self.process.exits()
        }
        let exited = exits.compactMap { (exit: Result<ProcessExit, Errno>) -> ProcessExit? in exit[case: \.success] }
        return chain(lived, exited.flatMap { (exit: ProcessExit) in self.terminations(after: exit, clock: clock) }).removeDuplicates()
    }

    private func terminations(after exit: ProcessExit, clock: some Clock<Duration> & Sendable) -> some AsyncSequence<InstanceState, Never> & Sendable {
        [false, true].async.compactMap { (reported: Bool) -> InstanceState? in
            switch (reported, exit.producesReport) {
                case (false, _): .exited(termination: InstanceState.Termination(exit: exit, report: nil))
                case (true, true):
                    .exited(
                        termination: InstanceState.Termination(
                            exit: exit,
                            report: await Result { () async throws(ProcessFailure) in
                                try await DiagnosticReport.first(writtenBy: self.process, since: self.application.launchDate ?? .distantPast, clock: clock)
                            },
                        )
                    )
                case (true, false): nil
            }
        }
    }
}
