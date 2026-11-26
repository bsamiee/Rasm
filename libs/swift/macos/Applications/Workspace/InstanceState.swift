public import AppKit
import AsyncAlgorithms
public import CasePaths
import CasePathsCore
public import Encoding
public import Processes
import Subprocess
public import System

// --- [MODELS] --------------------------------------------------------------------------

@CasePathable
public enum InstanceState: CaseKeyed, Equatable, Sendable {
    public struct Activity: Equatable, Sendable {
        @CasePathable
        public enum Status: CaseLabeled, Equatable, Sendable {
            case launching
            case responding
            case unresponsive(ProcessStatus)

            public var isIdle: Bool {
                switch self {
                    case .launching, .unresponsive(.running), .unresponsive(.runnable): false
                    case .responding, .unresponsive(.sleeping), .unresponsive(.stopped): true
                }
            }
        }

        public let status: Status
        public let active: Bool
        public let windows: [Window]
        public let usage: Result<ResourceUsage, Errno>?
        public let listening: Result<[in_port_t], Errno>

        public var asking: [Window] { windows.filter(\.asking) }
    }

    public struct Window: Codable, Equatable, Sendable {
        public let number: CGWindowID
        public let title: String?
        public let frame: CGRect
        public let layer: Int
        public let onScreen: Bool
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
    case exited(Termination)
}

extension InstanceState.Window {
    init?(_ row: [String: Any]) {
        guard let number = row[kCGWindowNumber as String] as? CGWindowID, let layer = row[kCGWindowLayer as String] as? Int, layer >= 0,
            let frame = (row[kCGWindowBounds as String] as? [String: Any]).flatMap({ bounds in CGRect(dictionaryRepresentation: bounds as CFDictionary) }), !frame.isEmpty
        else { return nil }
        let onScreen = row[kCGWindowIsOnscreen as String] as? Bool ?? false
        self.init(
            number: number,
            title: (row[kCGWindowName as String] as? String).flatMap { title in title.isEmpty ? nil : title },
            frame: frame,
            layer: layer,
            onScreen: onScreen,
            container: nil,
            kind: nil,
            document: nil,
            asking: false,
            presented: onScreen,
        )
    }

    static func all(_ listed: CGWindowListOption) -> [pid_t: [Self]] {
        let rows: [[String: Any]] = CGWindowListCopyWindowInfo(listed, kCGNullWindowID) as? [[String: Any]] ?? []
        return Dictionary(rows.compactMap { row in (row[kCGWindowOwnerPID as String] as? pid_t).flatMap { owner in Self(row).map { window in (owner, [window]) } } }, uniquingKeysWith: +)
    }

    func described(by container: Container?, blocking: Bool) -> Self {
        Self(
            number: number,
            title: title,
            frame: frame,
            layer: layer,
            onScreen: onScreen,
            container: container?.ref,
            kind: container?.kind,
            document: container?.document,
            asking: container?.asking ?? blocking,
            presented: container != nil || onScreen,
        )
    }
}

extension InstanceState.Termination: Encodable {
    private enum Key: CodingKey {
        case status, signal, detail, statusFailure, report, reportFailure, reporting
    }

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        let ended: ProcessExit? = exit[case: \.success]
        switch exit.map(\.status) {
            case .success(.exited(let status)): try fields.encode(status, forKey: .status)
            case .success(.signaled(let signal)): try fields.encode(signal, forKey: .signal)
            case .failure(let error): try fields.encode(error.description, forKey: .statusFailure)
        }
        if let ended, !ended.detail.isEmpty { try fields.encode(ended.detail, forKey: .detail) }
        if report == nil, ended?.producesReport == true { try fields.encode(true, forKey: .reporting) }
        try fields.encodeIfPresent(report?[case: \.success], forKey: .report)
        let reportFailure: ProcessFailure.Reason? = report?[case: \.failure]?.reason
        try fields.encodeIfPresent(reportFailure, forKey: .reportFailure)
    }
}

extension InstanceState.Activity: Encodable {
    private enum Key: CodingKey {
        case active, process, status, usage, usageFailure, windows
    }

    public func encode(to encoder: any Encoder) throws {
        var fields = encoder.container(keyedBy: Key.self)
        try fields.encode(active, forKey: .active)
        try fields.encode(status, forKey: .status)
        try fields.encodeIfPresent(status[case: \.unresponsive], forKey: .process)
        try fields.encodeIfPresent(usage?[case: \.success], forKey: .usage)
        let usageFailure: String? = usage?[case: \.failure]?.description
        try fields.encodeIfPresent(usageFailure, forKey: .usageFailure)
        try fields.encode(asking + windows.filter { window in !window.asking }, forKey: .windows)
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Instance {
    func lifetime(
        _ running: some AsyncSequence<InstanceState, Never> & Sendable,
        clock: some Clock<Duration> & Sendable,
    ) -> some AsyncSequence<InstanceState, Never> & Sendable {
        let cut = process.exits().map { _ in InstanceState?.none }
        let lived = merge(running.map(InstanceState?.some), cut).prefix { (state: InstanceState?) in state != nil }.compactMap(\.self)
        return chain(lived, process.exits().flatMap { exit in self.terminations(after: exit, clock: clock) }).removeDuplicates()
    }

    private func terminations(after exit: Result<ProcessExit, Errno>, clock: some Clock<Duration> & Sendable) -> some AsyncSequence<InstanceState, Never> & Sendable {
        [false, true].async.compactMap { reported -> InstanceState? in
            switch (exit, reported) {
                case (_, false): .exited(InstanceState.Termination(exit: exit, report: nil))
                case (.success(let ended), true) where ended.producesReport:
                    .exited(
                        InstanceState.Termination(
                            exit: exit,
                            report: await DiagnosticReport.first(writtenBy: self.process, since: self.application.launchDate ?? .distantPast, clock: clock),
                        )
                    )
                case (_, true): nil
            }
        }
    }
}

extension NSWorkspace {
    public func instances(of matching: (NSRunningApplication) -> Bool) -> [Instance] {
        runningApplications.filter(matching).compactMap { application in try? Instance(application) }
    }
}
