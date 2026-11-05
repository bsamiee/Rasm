import AppKit
import ApplicationServices
import AsyncAlgorithms
import CasePaths
import CasePathsCore
public import CoreGraphics
import Foundation

// --- [MODELS] --------------------------------------------------------------------------

public enum ElementAction: Codable, Hashable, Sendable, CustomStringConvertible {
    case perform(element: Element.ID, action: String)
    case set(element: Element.ID, value: String)
    case key(process: pid_t, code: CGKeyCode)

    public func owner() async throws(ApplicationFailure) -> pid_t { try await resolved(in: nil).owner }

    public var element: Element.ID? {
        switch self {
            case .perform(let element, _), .set(let element, _): element
            case .key: nil
        }
    }

    public var description: String {
        switch self {
            case .perform(let element, let action): "\(action) on \(element)"
            case .set(let element, let value): "set \(kAXValueAttribute) \"\(value)\" on \(element)"
            case .key(let process, let code): "key \(code) to \(process)"
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

extension ElementAction {
    public func perform(in process: pid_t? = nil, clock: some Clock<Duration> & Sendable = ContinuousClock()) async throws(ApplicationFailure) -> InstanceState {
        let target: (owner: pid_t, node: AXUIElement?) = try await resolved(in: process)
        let subject: pid_t = process ?? target.owner
        guard let instance: NSRunningApplication = NSWorkspace.shared.runningApplications.first(where: { candidate in candidate.processIdentifier == subject }) else {
            throw ApplicationFailure(subject: String(subject), reason: .noProcess)
        }
        let states: some AsyncSequence<InstanceState, Never> & Sendable = instance.states(clock: clock)
        let acted: CGWindowID?
        do throws(ApplicationFailure) { acted = try await run(on: target) } catch  where error.reason.is(\.noProcess) || error.reason == .unanswered(self) { acted = nil }
        var last: InstanceState?
        for await next: InstanceState? in merge(states.map(Optional.some), AsyncTimerSequence(interval: messagingTimeout * 2, clock: clock).map { _ in InstanceState?.none }) {
            switch (next, last) {
                case (.some(let state), _) where settled(state, acting: acted): return state
                case (.some(let state), _): last = state
                case (.none, .some(let state)): return state
                case (.none, .none): continue
            }
        }
        throw ApplicationFailure(subject: description, reason: .cancelled)
    }

    private func settled(_ state: InstanceState, acting window: CGWindowID?) -> Bool {
        switch (state, self, window) {
            case (.exited, _, _), (.failed, _, _): true
            case (.running(let running), _, _) where running.status.is(\.unresponsive): true
            case (.running(let running), .key, _): running.asking.isEmpty
            case (.running(let running), _, .some(let window)): !running.asking.contains { row in row.number == window }
            case (.running, _, .none): true
        }
    }

    @concurrent
    private func resolved(in process: pid_t?) async throws(ApplicationFailure) -> (owner: pid_t, node: AXUIElement?) {
        try await withTaskExecutorPreference(accessibility) { () throws(ApplicationFailure) -> (owner: pid_t, node: AXUIElement?) in
            guard let id: Element.ID = element else {
                guard case .key(let owner, _) = self else { throw ApplicationFailure(subject: description, reason: .unresolved) }
                return (owner, nil)
            }
            guard let node: AXUIElement = try AXUIElement.element(id.token), let owner: pid_t = node.process else { throw ApplicationFailure(subject: id.description, reason: .unresolved) }
            if let process, owner != process, !isDialogHelper(owner) { throw ApplicationFailure(subject: id.description, reason: .foreign(process)) }
            return (owner, node)
        }
    }

    @concurrent
    private func run(on target: (owner: pid_t, node: AXUIElement?)) async throws(ApplicationFailure) -> CGWindowID? {
        try await withTaskExecutorPreference(accessibility) { () throws(ApplicationFailure) -> CGWindowID? in
            guard let node: AXUIElement = target.node else {
                guard case .key(let process, let code) = self else { return nil }
                let source: CGEventSource? = CGEventSource(stateID: .privateState)
                for down: Bool in [true, false] {
                    guard let event: CGEvent = CGEvent(keyboardEventSource: source, virtualKey: code, keyDown: down) else { throw ApplicationFailure(subject: description, reason: .refused(nil)) }
                    event.postToPid(process)
                }
                return nil
            }
            let read: [String: CFTypeRef] = try node.values([kAXEnabledAttribute, kAXWindowAttribute])
            let window: CGWindowID? = try read[kAXWindowAttribute].flatMap(AXUIElement.checked)?.window()
            let outcome: AXError =
                switch (self, read[kAXEnabledAttribute] as? Bool) {
                    case (.perform, false): throw ApplicationFailure(subject: description, reason: .disabled)
                    case (.perform(_, let action), _): AXUIElementPerformAction(node, action as CFString)
                    case (.set(_, let value), _): try node.write(value as CFString, to: kAXValueAttribute)
                    case (.key, _): .success
                }
            guard outcome != .success else { return window }
            throw ApplicationFailure(outcome, subject: description, process: target.owner, answering: self) {
                ApplicationFailure(subject: description, reason: outcome == .invalidUIElement ? .unresolved : .refused(outcome))
            }
        }
    }
}
