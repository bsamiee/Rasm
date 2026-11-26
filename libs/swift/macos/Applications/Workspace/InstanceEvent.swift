@unsafe @preconcurrency public import ApplicationServices
public import CasePaths
public import Encoding
public import Foundation
import OrderedCollections
public import Processes

// --- [MODELS] --------------------------------------------------------------------------

public struct InstanceEvent: Encodable, Sendable {
    public struct Answer: Encodable, Sendable {
        public let ref: Int
        public let label: String?
    }

    @CasePathable
    public enum Change: CaseTagged, Sendable {
        case started(identifier: String?, name: String?)
        case opened(container: Int, kind: Container.Kind, title: String?)
        case closed(container: Int, kind: Container.Kind, title: String?)
        case asking(container: Int, kind: Container.Kind, title: String?, text: [String], answers: [Answer], default: Int?, cancel: Int?, close: Int?)
        case focused(control: Int, kind: Control.Kind)
        case value(control: Int, kind: Control.Kind, fact: Fact)
        case selection(control: Int, kind: Control.Kind, evidence: Control.Evidence)
        case moved(container: Int, kind: Container.Kind, frame: CGRect)
        case window(change: WindowNotification, window: InstanceState.Window)
        case frontmost(active: Bool)
        case hidden(hidden: Bool)
        case status(status: InstanceState.Activity.Status)
        case announced(text: String, priority: Lenient<AXPriority>?)
        case dragging(notification: String, control: Int?, kind: Control.Kind?)
        case logged(entry: LogEntry)
        case reported(report: DiagnosticReport)
        case sampled(report: URL)
        case exited(termination: InstanceState.Termination)
    }

    public let instance: ProcessInstance
    public let sequence: Int
    public let at: Date
    public let change: Change
}

extension InstanceEvent.Change {
    init(asking container: Container) {
        let labels: [Int: String] = Dictionary(container.controls.compactMap { control in control.label.map { label in (control.ref, label.text) } }) { first, _ in first }
        let marked: OrderedSet<Int> = OrderedSet(
            [container.default, container.cancel, container.close].compactMap(\.self) + container.controls.filter { control in control.kind == .action }.map(\.ref)
        )
        self = .asking(
            container: container.ref,
            kind: container.kind,
            title: container.title,
            text: container.text,
            answers: marked.map { answer in InstanceEvent.Answer(ref: answer, label: labels[answer]) },
            default: container.default,
            cancel: container.cancel,
            close: container.close,
        )
    }
}
