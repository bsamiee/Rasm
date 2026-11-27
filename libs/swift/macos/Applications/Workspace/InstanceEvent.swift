@unsafe @preconcurrency import ApplicationServices
import CasePaths
import Encoding
import Foundation
import OrderedCollections
import Processes

// --- [MODELS] --------------------------------------------------------------------------

package struct InstanceEvent: Encodable, Sendable {
    struct Answer: Encodable, Sendable {
        let ref: Int
        let label: String?
    }

    @CasePathable
    enum Change: CaseTagged, Sendable {
        enum Priority: CaseLabeled, Sendable {
            case low
            case medium
            case high

            init?(_ priority: AXPriority) {
                switch priority {
                    case .low: self = .low
                    case .medium: self = .medium
                    case .high: self = .high
                    @unknown default: return nil
                }
            }
        }

        case started(identifier: String?, name: String?)
        case opened(container: Int, kind: Container.Kind, title: String?)
        case closed(container: Int, kind: Container.Kind, title: String?, front: String?)
        case asking(
            container: Int,
            kind: Container.Kind,
            title: String?,
            text: [String],
            answers: [Answer],
            default: Int?,
            cancel: Int?,
            close: Int?,
            secure: Answer?,
            presentation: Watch.Presentation?,
        )
        case consenting(granted: Bool?)
        case focused(control: Int, kind: Control.Kind)
        case value(control: Int, kind: Control.Kind, fact: Fact)
        case selection(control: Int, kind: Control.Kind, evidence: Control.Evidence)
        case moved(container: Int, kind: Container.Kind, frame: CGRect)
        case window(change: WindowNotification, window: InstanceState.Window)
        case frontmost(active: Bool)
        case hidden(hidden: Bool)
        case status(status: InstanceState.Activity.Status)
        case announced(text: String, priority: Priority?)
        case dragging(notification: String, control: Int?, kind: Control.Kind?)
        case proven(followed: Int, drawn: Bool)
        case reported(report: DiagnosticReport)
        case sampled(report: URL)
        case exited(termination: InstanceState.Termination)
    }

    let instance: ProcessInstance
    let sequence: Int
    let at: Date
    let change: Change

    package var isPushed: Bool {
        switch change {
            case .moved, .window, .frontmost, .dragging: false
            case .started, .opened, .closed, .asking, .consenting, .focused, .value, .selection, .hidden, .status, .announced, .proven, .reported, .sampled, .exited: true
        }
    }
}

extension InstanceEvent.Change {
    init(asking container: Container, secure: InstanceEvent.Answer?, presentation: Watch.Presentation?) {
        let labels: [Int: String] = Dictionary(container.controls.compactMap { control in control.label.map { label in (control.ref, label.text) } }) { first, _ in first }
        let marked: OrderedSet<Int> = OrderedSet([container.default, container.cancel].compactMap(\.self))
        self = .asking(
            container: container.ref,
            kind: container.kind,
            title: container.title,
            text: container.text,
            answers: marked.map { answer in InstanceEvent.Answer(ref: answer, label: labels[answer]) },
            default: container.default,
            cancel: container.cancel,
            close: container.close,
            secure: secure,
            presentation: presentation,
        )
    }
}
