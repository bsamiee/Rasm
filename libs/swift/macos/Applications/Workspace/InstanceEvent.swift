public import CasePaths
public import CoreGraphics
public import Encoding
public import Foundation
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
        case exited(termination: InstanceState.Termination)
    }

    public let instance: ProcessInstance
    public let sequence: Int
    public let at: Date
    public let change: Change
}
