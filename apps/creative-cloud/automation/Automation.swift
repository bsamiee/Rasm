/**
 * Derives Adobe automation capabilities from installed scripting dictionaries
 * Preserves Apple Event replies and errors across the process boundary
 */

// --- [IMPORTS] -------------------------------------------------------------------------

import AppKit
import Carbon
import Foundation
import UniformTypeIdentifiers

// --- [MODELS] --------------------------------------------------------------------------

struct Descriptor: Encodable {
  let type: OSType
  let data: Data
  let text: String?
  let items: [Descriptor]
  let keywords: [AEKeyword]

  init(_ value: NSAppleEventDescriptor) {
    type = value.descriptorType
    data = value.data
    text = value.stringValue
    items = (0..<value.numberOfItems).compactMap { value.atIndex($0 + 1) }.map(Descriptor.init)
    keywords = value.isRecordDescriptor ? (0..<value.numberOfItems).map { value.keywordForDescriptor(at: $0 + 1) } : []
  }
}

struct Application: Encodable {
  let url: URL
  let identifier: String
  let name: String
  let version: String?
  let processes: [pid_t]
  let scriptable: Bool

  init?(url: URL, running: [NSRunningApplication]) {
    guard let bundle = Bundle(url: url), let identifier = bundle.bundleIdentifier, identifier.hasPrefix("com.adobe.") else { return nil }
    self.url = url.resolvingSymlinksInPath()
    self.identifier = identifier
    name = bundle.object(forInfoDictionaryKey: "CFBundleDisplayName") as? String ?? url.deletingPathExtension().lastPathComponent
    version = bundle.object(forInfoDictionaryKey: "CFBundleShortVersionString") as? String
    processes = running.filter { $0.bundleURL?.resolvingSymlinksInPath().path == url.resolvingSymlinksInPath().path }.map(\.processIdentifier)
    scriptable =
      switch Automation.definition(url) {
      case .success(let dictionary): !dictionary.commands.isEmpty
      case .failure: false
      }
  }
}

struct ScriptingDictionary {
  let document: XMLDocument
  let commands: [XMLElement]
  let enumerations: [XMLElement]

  init(data: Data) throws {
    document = try XMLDocument(data: data, options: [])
    commands = try document.nodes(forXPath: "//command").compactMap { $0 as? XMLElement }
    enumerations = try document.nodes(forXPath: "//enumeration").compactMap { $0 as? XMLElement }
  }
}

// --- [AUTOMATION] ----------------------------------------------------------------------

@main
struct Automation {
  // --- [DISCOVERY]

  static func applications() -> [Application] {
    let manager = FileManager.default
    let running = NSWorkspace.shared.runningApplications
    let installed = manager.urls(for: .applicationDirectory, in: .allDomainsMask).flatMap { directory -> [URL] in
      let entries = manager.enumerator(at: directory, includingPropertiesForKeys: [.isApplicationKey], options: [.skipsPackageDescendants, .skipsHiddenFiles])
      return entries?.allObjects.compactMap { $0 as? URL }.filter { (try? $0.resourceValues(forKeys: [.isApplicationKey]))?.isApplication == true } ?? []
    }
    return Set(installed + running.compactMap(\.bundleURL)).compactMap { Application(url: $0, running: running) }.sorted { $0.url.absoluteString < $1.url.absoluteString }
  }

  static func definition(_ application: URL) -> Result<ScriptingDictionary, NSError> {
    var data: Unmanaged<CFData>?
    let status = OSACopyScriptingDefinitionFromURL(application as CFURL, 0, &data)
    guard status == noErr, let data else { return .failure(NSError(domain: NSOSStatusErrorDomain, code: Int(status))) }
    return Result { try ScriptingDictionary(data: data.takeRetainedValue() as Data) }.mapError { $0 as NSError }
  }

  // --- [DESCRIPTORS]

  static func code<T: FixedWidthInteger & UnsignedInteger>(_ text: String, as: T.Type) -> Result<T, NSError> {
    let data = text.data(using: .macOSRoman)
    let value = text.hasPrefix("0x") ? T(text.dropFirst(2), radix: 16) : data.flatMap { $0.count == MemoryLayout<T>.size ? $0.reduce(T.zero) { ($0 << UInt8.bitWidth) | T($1) } : nil }
    return value.map(Result.success)
      ?? .failure(NSError(domain: NSCocoaErrorDomain, code: CocoaError.coderInvalidValue.rawValue, userInfo: [NSLocalizedDescriptionKey: "Invalid scripting dictionary code: \(text)"]))
  }

  static func descriptor(_ value: Any, declaration: XMLElement, dictionary: ScriptingDictionary) -> Result<NSAppleEventDescriptor, NSError> {
    switch value {
    case is NSNull:
      return .success(.null())
    case let number as NSNumber:
      return .success(CFGetTypeID(number) == CFBooleanGetTypeID() ? NSAppleEventDescriptor(boolean: number.boolValue) : NSAppleEventDescriptor(double: number.doubleValue))
    case let values as [Any]:
      return values.reduce(.success(NSAppleEventDescriptor.list())) { result, value in
        result.flatMap { list in
          descriptor(value, declaration: declaration, dictionary: dictionary).map { item in
            list.insert(item, at: list.numberOfItems + 1)
            return list
          }
        }
      }
    case let string as String:
      let types = Set(([declaration.attribute(forName: "type")?.stringValue] + declaration.elements(forName: "type").map { $0.attribute(forName: "type")?.stringValue }).compactMap { $0 })
      let enumerations = dictionary.enumerations.filter { [$0.attribute(forName: "name")?.stringValue, $0.attribute(forName: "code")?.stringValue].compactMap { $0 }.contains(where: types.contains) }
      let choices = Set(
        enumerations.flatMap { $0.elements(forName: "enumerator") }.filter { $0.attribute(forName: "name")?.stringValue == string }.compactMap { $0.attribute(forName: "code")?.stringValue })
      return enumerations.isEmpty
        ? .success(!types.isEmpty && types.isSubset(of: ["file", "alias"]) ? NSAppleEventDescriptor(fileURL: URL(filePath: string)) : NSAppleEventDescriptor(string: string))
        : (choices.count == 1 ? choices.first : nil).map { code($0, as: OSType.self).map(NSAppleEventDescriptor.init(enumCode:)) }
          ?? .failure(NSError(domain: NSCocoaErrorDomain, code: CocoaError.coderInvalidValue.rawValue, userInfo: [NSLocalizedDescriptionKey: "Enumeration value is unknown or ambiguous: \(string)"]))
    case let record as [String: Any]:
      guard let type = record["type"] as? UInt32, let encoded = record["data"] as? String, let data = Data(base64Encoded: encoded),
        let descriptor = NSAppleEventDescriptor(descriptorType: type, data: data)
      else {
        return .failure(
          NSError(domain: NSCocoaErrorDomain, code: CocoaError.coderInvalidValue.rawValue, userInfo: [NSLocalizedDescriptionKey: "Native descriptors require an unsigned type code and base64 data"]))
      }
      return .success(descriptor)
    default:
      return .failure(NSError(domain: NSCocoaErrorDomain, code: CocoaError.coderInvalidValue.rawValue))
    }
  }

  // --- [EXECUTION]

  static func event(application: URL, command: String, arguments: [String: Any], dictionary: ScriptingDictionary) -> Result<NSAppleEventDescriptor, NSError> {
    let running = NSWorkspace.shared.runningApplications.filter { $0.bundleURL?.resolvingSymlinksInPath().path == application.path }
    guard let process = running.first, running.count == 1 else {
      return .failure(NSError(domain: NSOSStatusErrorDomain, code: Int(procNotFound), userInfo: [NSLocalizedDescriptionKey: "Execution requires exactly one running instance of \(application.path)"]))
    }
    guard let declaration = dictionary.commands.first(where: { $0.attribute(forName: "name")?.stringValue == command }), let eventCode = declaration.attribute(forName: "code")?.stringValue else {
      return .failure(NSError(domain: NSOSStatusErrorDomain, code: Int(errAEEventNotHandled), userInfo: [NSLocalizedDescriptionKey: "Command is absent from the installed dictionary: \(command)"]))
    }
    let parameters = declaration.elements(forName: "parameter") + declaration.elements(forName: "direct-parameter")
    let required = parameters.filter { $0.attribute(forName: "optional")?.stringValue != "yes" }.map { $0.attribute(forName: "name")?.stringValue ?? "direct" }
    let missing = Set(required).subtracting(arguments.keys)
    let values = arguments.sorted { $0.key < $1.key }.map { argument -> Result<(AEKeyword, NSAppleEventDescriptor), NSError> in
      guard let parameter = parameters.first(where: { ($0.attribute(forName: "name")?.stringValue ?? "direct") == argument.key }) else {
        return .failure(NSError(domain: NSOSStatusErrorDomain, code: Int(errAEParamMissed), userInfo: [NSLocalizedDescriptionKey: "Unknown parameter: \(argument.key)"]))
      }
      let keyword = parameter.attribute(forName: "code")?.stringValue.map { code($0, as: AEKeyword.self) } ?? .success(AEKeyword(keyDirectObject))
      return keyword.flatMap { key in descriptor(argument.value, declaration: parameter, dictionary: dictionary).map { (key, $0) } }
    }
    let errors =
      values.compactMap { value -> NSError? in
        switch value {
        case .failure(let error): error
        case .success: nil
        }
      } + missing.sorted().map { NSError(domain: NSOSStatusErrorDomain, code: Int(errAEParamMissed), userInfo: [NSLocalizedDescriptionKey: "Missing parameter: \($0)"]) }
    return errors.isEmpty
      ? code(eventCode, as: UInt64.self).map { value in
        let event = NSAppleEventDescriptor(
          eventClass: AEEventClass(value >> AEEventID.bitWidth), eventID: AEEventID(truncatingIfNeeded: value), targetDescriptor: NSAppleEventDescriptor(processIdentifier: process.processIdentifier),
          returnID: AEReturnID(kAutoGenerateReturnID), transactionID: AETransactionID(kAnyTransactionID))
        for case .success(let parameter) in values {
          event.setParam(parameter.1, forKeyword: parameter.0)
        }
        return event
      }
      : .failure(
        NSError(
          domain: NSCocoaErrorDomain, code: CocoaError.coderInvalidValue.rawValue,
          userInfo: [NSMultipleUnderlyingErrorsKey: errors, NSLocalizedDescriptionKey: errors.map(\.localizedDescription).joined(separator: "; ")]))
  }

  static func execute(application: URL, command: String, arguments: [String: Any]) -> Result<Data, NSError> {
    definition(application)
      .flatMap { event(application: application, command: command, arguments: arguments, dictionary: $0) }
      .flatMap { event in
        Result { try event.sendEvent(options: [.waitForReply, .neverInteract], timeout: TimeInterval(kNoTimeOut)) }.mapError {
          let error = $0 as NSError
          return NSError(domain: error.domain, code: error.code, userInfo: error.userInfo.merging(["stage": "send"]) { _, value in value })
        }
      }
      .flatMap { reply in
        let encoded = Result { try JSONEncoder().encode(Descriptor(reply)) }.mapError { $0 as NSError }
        guard let failure = reply.paramDescriptor(forKeyword: AEKeyword(keyErrorNumber)), failure.int32Value != noErr else { return encoded }
        let description: [String: Any] = reply.paramDescriptor(forKeyword: AEKeyword(keyErrorString))?.stringValue.map { [NSLocalizedDescriptionKey: $0] } ?? [:]
        return encoded.flatMap { data in Result { try JSONSerialization.jsonObject(with: data) }.mapError { $0 as NSError } }.flatMap { value in
          .failure(NSError(domain: NSOSStatusErrorDomain, code: Int(failure.int32Value), userInfo: description.merging(["stage": "reply", "reply": value]) { _, item in item }))
        }
      }
  }

  // --- [PROCESS]

  static func request(_ input: [String: Any]) -> Result<Data, NSError> {
    switch input["operation"] as? String {
    case "applications":
      return Result { try JSONEncoder().encode(applications()) }.mapError { $0 as NSError }
    case "dictionary", "execute":
      guard let address = input["application"] as? String, let application = URL(string: address), application.isFileURL,
        Bundle(url: application)?.bundleIdentifier?.hasPrefix("com.adobe.") == true
      else { return .failure(NSError(domain: NSCocoaErrorDomain, code: CocoaError.fileReadInvalidFileName.rawValue)) }
      if input["operation"] as? String == "dictionary" {
        return definition(application).flatMap { dictionary in Result { try JSONEncoder().encode(dictionary.document.xmlString) }.mapError { $0 as NSError } }
      }
      guard let command = input["command"] as? String, let arguments = input["arguments"] as? [String: Any] else {
        return .failure(NSError(domain: NSCocoaErrorDomain, code: CocoaError.coderInvalidValue.rawValue))
      }
      return execute(application: application.resolvingSymlinksInPath(), command: command, arguments: arguments)
    case "artifact":
      guard let address = input["url"] as? String, let url = URL(string: address), url.isFileURL else {
        return .failure(NSError(domain: NSCocoaErrorDomain, code: CocoaError.fileReadInvalidFileName.rawValue))
      }
      return Result {
        let type = try url.resourceValues(forKeys: [.contentTypeKey]).contentType ?? .data
        return try JSONSerialization.data(withJSONObject: [
          "mimeType": type.preferredMIMEType ?? "application/octet-stream", "blob": try Data(contentsOf: url).base64EncodedString(), "uri": url.absoluteString,
        ])
      }.mapError { $0 as NSError }
    default:
      return .failure(NSError(domain: NSCocoaErrorDomain, code: CocoaError.coderInvalidValue.rawValue))
    }
  }

  static func main() {
    let result = Result { try JSONSerialization.jsonObject(with: FileHandle.standardInput.readDataToEndOfFile()) }.mapError { $0 as NSError }
      .flatMap { ($0 as? [String: Any]).map(request) ?? .failure(NSError(domain: NSCocoaErrorDomain, code: CocoaError.coderInvalidValue.rawValue)) }
    switch result {
    case .success(let data):
      FileHandle.standardOutput.write(data)
    case .failure(let error):
      let failure: [String: Any?] = [
        "_tag": "nativeError", "domain": error.domain, "code": error.code, "message": error.localizedDescription, "stage": error.userInfo["stage"] ?? "request", "reply": error.userInfo["reply"],
      ]
      let output = Result { try JSONSerialization.data(withJSONObject: failure.compactMapValues { $0 }) }
      switch output {
      case .success(let data): FileHandle.standardOutput.write(data)
      case .failure(let failure): FileHandle.standardError.write(Data(failure.localizedDescription.utf8))
      }
      exit(EXIT_FAILURE)
    }
  }
}
