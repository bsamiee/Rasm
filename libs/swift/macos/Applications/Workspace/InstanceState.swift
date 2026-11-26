public import AppKit
@unsafe @preconcurrency import ApplicationServices
import AsyncAlgorithms
public import CasePaths
import CasePathsCore
public import Encoding
public import Processes
import Subprocess
public import System

// --- [MODELS] --------------------------------------------------------------------------

@CasePathable
public enum InstanceState: CaseTagged, Equatable, Sendable {
    public struct Activity: Equatable, Sendable {
        @CasePathable
        public enum Status: CaseTagged, Equatable, Sendable {
            case launching
            case responding
            case unresponsive(thread: ProcessStatus)

            public var isIdle: Bool {
                switch self {
                    case .launching, .unresponsive(.running), .unresponsive(.runnable), .unresponsive(.throttled), .unresponsive(.uninterruptible): false
                    case .responding, .unresponsive(.sleeping), .unresponsive(.stopped): true
                }
            }
        }

        public let status: Status
        public let active: Bool
        public let windows: [Window]
        public let usage: Result<ResourceUsage, Errno>?
        public let listening: Result<[in_port_t], Errno>
        public let statusItems: Result<Int, ApplicationFailure>

        public var asking: [Window] { windows.filter(\.asking) }
    }

    public struct Window: Codable, Equatable, Sendable {
        public enum Band: String, CaseIterable, Codable, Sendable {
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

            public var level: Int {
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

            init(layer: Int) { self = Self.allCases.filter { band in band.level <= layer }.max { first, second in first.level < second.level } ?? .normal }
        }

        public let number: CGWindowID
        public let title: String?
        public let frame: CGRect
        public let layer: Int
        public let band: Band
        public let parent: CGWindowID?
        public let spaces: [UInt64]
        public let onScreen: Bool
        public let hidden: Bool
        public let shared: Bool
        public let container: Int?
        public let kind: Container.Kind?
        public let document: URL?
        public let asking: Bool
        public let presented: Bool
    }

    public struct Termination: Equatable, Sendable {
        public let exit: Result<ProcessExit, Errno>
        public let report: Result<DiagnosticReport, ProcessFailure>?
    }

    case running(Activity)
    case failed(ApplicationFailure)
    case exited(termination: Termination)
}

extension InstanceState.Window {
    static func all(_ listed: CGWindowListOption, relativeTo window: CGWindowID = kCGNullWindowID) -> [pid_t: [Self]] {
        let rows: [[String: Any]] = CGWindowListCopyWindowInfo(listed, window) as? [[String: Any]] ?? []
        let numbers: [CGWindowID] = rows.compactMap { row in row[kCGWindowNumber as String] as? CGWindowID }
        let connection: Int32 = mainConnection()
        let iterator: CFTypeRef? = unsafe queryWindows(connection, numbers as CFArray, Int32(numbers.count)).flatMap { query in unsafe queriedWindows(query.takeRetainedValue())?.takeRetainedValue() }
        let parents: [CGWindowID: CGWindowID] = Dictionary(
            iterator.map { iterator in Array(sequence(state: iterator) { iterator in advance(iterator) ? (iteratedWindow(iterator), iteratedParent(iterator)) : nil }) } ?? []
        ) { first, _ in first }
        let current: Set<UInt64> = Set((unsafe copySpaces(connection, [.current, .user])?.takeRetainedValue() as? [UInt64]) ?? [])
        return Dictionary(
            rows.compactMap { (row: [String: Any]) -> (pid_t, [Self])? in
                guard let owner = row[kCGWindowOwnerPID as String] as? pid_t, let number = row[kCGWindowNumber as String] as? CGWindowID, let layer = row[kCGWindowLayer as String] as? Int, layer >= 0,
                    let frame = (row[kCGWindowBounds as String] as? [String: Any]).flatMap({ bounds in CGRect(dictionaryRepresentation: bounds as CFDictionary) }), !frame.isEmpty
                else { return nil }
                let onScreen: Bool = row[kCGWindowIsOnscreen as String] as? Bool ?? false
                let assigned: [UInt64] = (unsafe copySpacesForWindows(connection, [.current, .others, .user], [number] as CFArray)?.takeRetainedValue() as? [UInt64]) ?? []
                let window = Self(
                    number: number,
                    title: (row[kCGWindowName as String] as? String).flatMap { title in title.isEmpty ? nil : title },
                    frame: frame,
                    layer: layer,
                    band: Band(layer: layer),
                    parent: parents[number].flatMap { parent in parent == kCGNullWindowID ? nil : parent },
                    spaces: assigned,
                    onScreen: onScreen,
                    hidden: !onScreen && !current.isDisjoint(with: assigned),
                    shared: row[kCGWindowSharingState as String] as? UInt32 != CGWindowSharingType.none.rawValue,
                    container: nil,
                    kind: nil,
                    document: nil,
                    asking: false,
                    presented: onScreen,
                )
                return (owner, [window])
            },
            uniquingKeysWith: +,
        )
    }

    func described(by container: Container?, blocking: Bool) -> Self {
        Self(
            number: number,
            title: title,
            frame: frame,
            layer: layer,
            band: band,
            parent: parent,
            spaces: spaces,
            onScreen: onScreen,
            hidden: hidden,
            shared: shared,
            container: container?.ref,
            kind: container?.kind,
            document: container?.document,
            asking: container?.asking ?? blocking,
            presented: container != nil || onScreen,
        )
    }
}

extension InstanceState.Activity {
    static func statusBars(in windows: [pid_t: [InstanceState.Window]]) -> [(host: pid_t, number: CGWindowID)] {
        windows.flatMap { host, rows in
            rows.filter { row in row.band == .mainMenu && NSRunningApplication(processIdentifier: host) != nil }.map { row in (host: host, number: row.number) }
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

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        let ended: ProcessExit? = exit[case: \.success]
        let (field, value): (Key, any Encodable) =
            switch exit.map(\.status) {
                case .success(.exited(let status)): (.status, status)
                case .success(.signaled(let signal)): (.signal, signal)
                case .failure(let error): (.statusFailure, error.description)
            }
        try fields.encode(field.stringValue, forKey: .tag)
        try fields.encode(value, forKey: field)
        if let ended, !ended.detail.isEmpty { try fields.encode(ended.detail, forKey: .detail) }
        if report == nil, ended?.producesReport == true { try fields.encode(true, forKey: .reporting) }
        try fields.encodeIfPresent(report?[case: \.success], forKey: .report)
        let reportFailure: ProcessFailure.Reason? = report?[case: \.failure]?.reason
        try fields.encodeIfPresent(reportFailure, forKey: .reportFailure)
    }
}

extension InstanceState.Activity: Encodable {
    private enum Key: CodingKey {
        case active, status, usage, usageFailure, windows, statusItems, statusItemsFailure
    }

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        try fields.encode(active, forKey: .active)
        try fields.encode(status, forKey: .status)
        try fields.encodeIfPresent(usage?[case: \.success], forKey: .usage)
        let usageFailure: String? = usage?[case: \.failure]?.description
        try fields.encodeIfPresent(usageFailure, forKey: .usageFailure)
        try fields.encode(asking + windows.filter { window in !window.asking }, forKey: .windows)
        try fields.encodeIfPresent(statusItems[case: \.success], forKey: .statusItems)
        let statusItemsFailure: String? = statusItems[case: \.failure]?.errorDescription
        try fields.encodeIfPresent(statusItemsFailure, forKey: .statusItemsFailure)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Instance {
    func lifetime(
        _ running: some AsyncSequence<InstanceState, Never> & Sendable,
        clock: some Clock<Duration> & Sendable,
    ) -> some AsyncSequence<InstanceState, Never> & Sendable {
        let (ended, ending) = AsyncStream<Result<ProcessExit, Errno>>.makeStream()
        let cut = process.exits().map { exit in
            ending.yield(exit)
            ending.finish()
            return InstanceState?.none
        }
        let lived = merge(running.map(InstanceState?.some), cut).prefix { (state: InstanceState?) in state != nil }.compactMap(\.self)
        return chain(lived, ended.flatMap { exit in self.terminations(after: exit, clock: clock) }).removeDuplicates()
    }

    private func terminations(after exit: Result<ProcessExit, Errno>, clock: some Clock<Duration> & Sendable) -> some AsyncSequence<InstanceState, Never> & Sendable {
        [false, true].async.compactMap { reported -> InstanceState? in
            switch (exit, reported) {
                case (_, false): .exited(termination: InstanceState.Termination(exit: exit, report: nil))
                case (.success(let ended), true) where ended.producesReport:
                    .exited(
                        termination: InstanceState.Termination(
                            exit: exit,
                            report: await Result { () async throws(ProcessFailure) in
                                try await DiagnosticReport.first(writtenBy: self.process, since: self.application.launchDate ?? .distantPast, clock: clock)
                            },
                        )
                    )
                case (_, true): nil
            }
        }
    }
}
