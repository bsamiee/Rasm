public import AppleEvents
public import Applications
public import Foundation

// --- [OPERATIONS] ----------------------------------------------------------------------

extension Installation {
    private static let interferences: KeyValuePairs<String, [(code: String, clause: EventClause, reason: Interference)]> = [
        "com.adobe.InDesign": [
            ("UIUIshow", .always, .activation),
            ("K2LKrvlO", .always, .activation),
            ("K2LKrvlB", .always, .activation),
            ("UIUIInvk", .always, .menuAction),
            ("K2  dosc", .always, .script),
            ("TeXTSTed", .always, .interface),
            ("K2  TpsV", .always, .interface),
            ("K2  casc", .always, .interface),
            ("K2  tile", .always, .interface),
            ("K2  rstr", .direct(of: ["cwin", "Lwin", "Swin"]), .interface),
            ("aevtpdoc", .parameter("prno", .among(["true"])), .interface),
            ("K2  ePrB", .parameter("BPBD", .among(["true"])), .interface),
            ("CoReclos", .parameter("svop", .among([nil, "ask "])), .interface),
            ("aevtquit", .parameter("savo", .among([nil, "ask "])), .interface),
            ("coresetd", .property("pvis", of: ["CPNS", "pgsp", "cLbp"], .among(["true"])), .interface),
            ("coresetd", .property("UIAc", of: nil, .except("eNvr")), .interface),
        ]
            + [
                "K2  expt", "InBKexpt", "InESexpt", "K2PIexpt", "K2XLexpt", "TeXTexpt", "K2  exAs", "InESexAs", "K2PIexAs", "K2XLexAs", "TeXTexAs", "K2PIplac", "InESplac", "TeXTplac",
                "K2  LCDr", "K2  LPGn", "K2PIcopl", "InEScopl", "TeXTcopl", "K2PIplli", "TeXTplli",
            ]
            .map { code in (code, .parameter("imot", .among(["true"])), .interface) },
        "com.adobe.illustrator": [
            ("miscDjxM", .always, .script),
            ("miscdosc", .always, .script),
            ("miscaEMC", .always, .menuAction),
            ("misckCPk", .always, .interface),
            ("ART5kLWC", .always, .interface),
            ("aevtodoc", .parameter("DLOG", .among([nil, "true"])), .interface),
            ("coresetd", .property("pUIL", of: nil, .except("e45a")), .interface),
        ],
        "com.adobe.Photoshop": [
            ("miscDjxM", .always, .script),
            ("adSUdJsc", .always, .script),
            ("miscDoAc", .always, .script),
            ("8BIMpsop", .always, .interface),
            ("8BIMptmg", .always, .interface),
            ("PsCSodoc", .parameter("ShwD", .except("Nevr")), .interface),
            ("PsCSclos", .parameter("savo", .among([nil, "ask "])), .interface),
            ("coresetd", .property("AUiL", of: nil, .except("Nevr")), .interface),
        ],
        "com.adobe.Acrobat.Pro": [
            ("coresetd", .property("pisf", of: nil, .any), .activation),
            ("CARObfrt", .always, .activation),
            ("CAROexec", .always, .menuAction),
            ("miscdosc", .always, .script),
            ("coreclos", .parameter("savo", .among(["ask "])), .interface),
            ("aevtquit", .parameter("savo", .among(["ask "])), .interface),
            ("coresetd", .property("pskw", of: nil, .among(["fals"])), .interface),
        ],
        "com.adobe.AfterEffects": [
            ("miscdosc", .always, .script),
            ("miscfile", .always, .script),
            ("aevtpdoc", .parameter("pdlg", .among(["true"])), .interface),
            ("coresetd", .property("pvis", of: nil, .among(["true"])), .interface),
            ("coresetd", .property("pmnd", of: nil, .among(["fals"])), .interface),
        ],
        "com.adobe.PremierePro": [
            ("CR8RCPAE", .always, .interface),
            ("CR8RCRPr", .always, .activation),
        ],
    ]

    private static let scripts: KeyValuePairs<String, (code: String, runtime: String)> = [
        "com.adobe.illustrator": ("miscDjxM", "ExtendScript"),
        "com.adobe.Photoshop": ("miscDjxM", "ExtendScript"),
        "com.adobe.Acrobat.Pro": ("miscdosc", "Acrobat JavaScript"),
        "com.adobe.AfterEffects": ("miscdosc", "ExtendScript"),
    ]

    public var script: (code: String, runtime: String)? {
        Self.scripts.first { stem, _ in identifier.hasPrefix(stem) }?.value
    }

    public func interference(of command: ScriptingDictionary.Command, sending event: NSAppleEventDescriptor) -> Interference? {
        Self.interferences.first { stem, _ in identifier.hasPrefix(stem) }.flatMap { _, rows in command.interference(for: event, among: rows) }
    }
}
