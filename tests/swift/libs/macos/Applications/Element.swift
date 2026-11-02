import ApplicationServices
import CustomDump
import Encoding
import Foundation
import Testing

@testable import Applications

// --- [CONSTANTS] -----------------------------------------------------------------------

private let token: Data = Data([1, 2, 3])
private let control: Control = Control(
    id: token,
    role: kAXPopUpButtonRole,
    subrole: kAXCloseButtonSubrole,
    label: "Size",
    identifier: "size",
    attribute: kAXDefaultButtonAttribute,
    value: .string("Large"),
    enabled: false,
    actions: [kAXPressAction],
    items: [Control(id: token, role: kAXMenuItemRole, subrole: nil, label: nil, identifier: nil, attribute: nil, value: nil, enabled: true, actions: [], items: nil)],
)
private let elements: [(element: Element, line: String)] = [
    (
        Element(
            id: token,
            window: nil,
            role: kAXMenuBarRole,
            subrole: nil,
            modal: nil,
            identifier: nil,
            description: nil,
            title: nil,
            document: nil,
            edited: nil,
            minimized: nil,
            actions: [],
            requiresAnswer: false,
            detail: nil,
        ),
        #"{"id":"AQID","role":"AXMenuBar","actions":[],"requiresAnswer":false}"#,
    ),
    (
        Element(
            id: token,
            window: 77,
            role: kAXWindowRole,
            subrole: kAXDialogSubrole,
            modal: true,
            identifier: "panel",
            description: "alert",
            title: "Save",
            document: URL(filePath: "/Document.txt"),
            edited: true,
            minimized: false,
            actions: [kAXPressAction],
            requiresAnswer: true,
            detail: Element.Detail(
                frame: CGRect(x: 1, y: 2, width: 3, height: 4),
                main: true,
                focused: false,
                text: ["Name"],
                controls: [control],
                unreadable: [FailedRead(reads: [kAXValueAttribute], role: kAXTextFieldRole, label: "Name", error: .cannotComplete)],
            ),
        ),
        #"""
        {"id":"AQID","window":77,"role":"AXWindow","subrole":"AXDialog","modal":true,"identifier":"panel","description":"alert","title":"Save","document":"file:///Document.txt",
        "edited":true,"minimized":false,"actions":["AXPress"],"requiresAnswer":true,"detail":{"frame":[[1,2],[3,4]],"main":true,"focused":false,"text":["Name"],
        "controls":[{"id":"AQID","role":"AXPopUpButton","subrole":"AXCloseButton","label":"Size","identifier":"size","attribute":"AXDefaultButton","value":"Large","enabled":false,
        "actions":["AXPress"],"items":[{"id":"AQID","role":"AXMenuItem","enabled":true,"actions":[]}]}],
        "unreadable":[{"reads":["AXValue"],"role":"AXTextField","label":"Name","error":-25204}]}}
        """#,
    ),
]

// --- [COMPOSITION] ---------------------------------------------------------------------

struct Elements {
    @Test(arguments: elements)
    func elementEncodesWireElementShape(element: Element, line: String) throws {
        expectNoDifference(try wire(element), try json(line))
    }
}
