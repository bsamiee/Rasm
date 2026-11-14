public import AppKit
import ApplicationServices
import AsyncAlgorithms
import CasePathsCore
public import CoreGraphics

// --- [MODELS] --------------------------------------------------------------------------

public enum ElementAction: Codable, Hashable, Sendable, CustomStringConvertible {
    case perform(element: Element.ID, action: String)
    case set(element: Element.ID, value: String)
    case key(process: pid_t, code: CGKeyCode)

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
    @concurrent
    public func perform(
        in process: pid_t? = nil,
        clock: some Clock<Duration> & Sendable = ContinuousClock(),
    ) async throws(ApplicationFailure) -> (instance: Instance, state: InstanceState) {
        if case .perform(_, kAXRaiseAction) = self {
            throw ApplicationFailure(subject: "\(self), raising a window takes focus from the frontmost application", reason: .refused(nil))
        }
        let (instance, target) = try await withTaskExecutorPreference(accessibility) { () throws(ApplicationFailure) in
            let target = try resolved(in: process)
            let subject = process ?? target.owner
            guard let application = NSRunningApplication(processIdentifier: subject) else { throw ApplicationFailure(subject: String(subject), reason: .noProcess) }
            return (try Instance(application), target)
        }
        let states = instance.states(clock: clock)
        let acted = try await withTaskExecutorPreference(accessibility) { () throws(ApplicationFailure) in
            do throws(ApplicationFailure) { return try run(on: target) } catch  where error.reason.is(\.noProcess) || error.reason == .unanswered(self) { return nil }
        }
        var last: InstanceState?
        for await next in merge(states.map(Optional.some), AsyncTimerSequence(interval: messagingTimeout * 2, clock: clock).map { _ in InstanceState?.none }) {
            if let next, !settled(next, acting: acted) {
                last = next
                continue
            }
            if let state = next ?? last { return (instance, state) }
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

    private func resolved(in process: pid_t?) throws(ApplicationFailure) -> (owner: pid_t, node: AXUIElement?) {
        switch self {
            case .key(let owner, _):
                if let process, owner != process { throw ApplicationFailure(subject: String(owner), reason: .foreign(process)) }
                return (owner, nil)
            case .perform(let id, _), .set(let id, _):
                guard let node = try AXUIElement.element(id.token), let owner = node.process else {
                    throw ApplicationFailure(subject: id.description, reason: .unresolved)
                }
                if let process, owner != process, NSRunningApplication(processIdentifier: owner)?.isDialogHelper != true {
                    throw ApplicationFailure(subject: id.description, reason: .foreign(process))
                }
                return (owner, node)
        }
    }

    private func run(on target: (owner: pid_t, node: AXUIElement?)) throws(ApplicationFailure) -> CGWindowID? {
        guard let node = target.node else {
            guard case .key(let process, let code) = self else { return nil }
            let source = CGEventSource(stateID: .privateState)
            for down in [true, false] {
                guard let event = CGEvent(keyboardEventSource: source, virtualKey: code, keyDown: down) else { throw ApplicationFailure(subject: description, reason: .refused(nil)) }
                event.postToPid(process)
            }
            return nil
        }
        let read = try node.values([kAXEnabledAttribute, kAXWindowAttribute])
        let window = try read.values[kAXWindowAttribute].flatMap(AXUIElement.checked).flatMap { window throws(ApplicationFailure) in
            AXUIElement.window(framed: try window.values(frameAttributes), among: InstanceState.Window.owned(by: target.owner))
        }
        let outcome: AXError =
            switch (self, read.values[kAXEnabledAttribute] as? Bool) {
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
