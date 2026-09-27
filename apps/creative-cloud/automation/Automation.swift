import AppKit
import Carbon
import System
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

  init?(url: URL, running: [NSRunningApplication]) throws {
    guard let bundle = Bundle(url: url), let identifier = Application.identifier(bundle) else { return nil }
    self.url = url
    self.identifier = identifier
    name = bundle.object(forInfoDictionaryKey: "CFBundleDisplayName") as? String ?? url.deletingPathExtension().lastPathComponent
    version = bundle.object(forInfoDictionaryKey: "CFBundleShortVersionString") as? String
    processes = Application.instances(of: url, in: running).map(\.processIdentifier)
    scriptable = try url.resourceValues(forKeys: [.applicationIsScriptableKey]).applicationIsScriptable == true
  }

  static func identifier(_ bundle: Bundle) -> String? {
    bundle.bundleIdentifier.flatMap { $0.hasPrefix("com.adobe.") ? $0 : nil }
  }

  static func instances(of url: URL, in running: [NSRunningApplication]) -> [NSRunningApplication] {
    running.filter { $0.activationPolicy != .prohibited && $0.bundleURL?.resolvingSymlinksInPath().path == url.path }
  }
}

struct ScriptingDictionary {
  let document: XMLDocument
  let commands: [XMLElement]
  let enumerations: [XMLElement]

  init(data: Data) throws {
    document = try XMLDocument(data: data)
    commands = try document.nodes(forXPath: "//command").compactMap { $0 as? XMLElement }
    enumerations = try document.nodes(forXPath: "//enumeration").compactMap { $0 as? XMLElement }
  }
}

struct Artifact: Encodable {
  let uri: URL
  let mimeType: String?
  let blob: Data
}

// --- [ERRORS] --------------------------------------------------------------------------

enum Failure: Error, Encodable {
  case request(any Error)
  case send(any Error)
  case reply(any Error, Descriptor)

  enum CodingKeys: String, CodingKey {
    case tag = "_tag"
    case stage, domain, code, message, reply
  }

  func encode(to encoder: any Encoder) throws {
    let (stage, error, reply): (String, NSError, Descriptor?) =
      switch self {
      case .request(let error): ("request", error as NSError, nil)
      case .send(let error): ("send", error as NSError, nil)
      case .reply(let error, let reply): ("reply", error as NSError, reply)
      }
    var container = encoder.container(keyedBy: CodingKeys.self)
    try container.encode("nativeError", forKey: .tag)
    try container.encode(stage, forKey: .stage)
    try container.encode(error.domain, forKey: .domain)
    try container.encode(error.code, forKey: .code)
    try container.encode(error.localizedDescription, forKey: .message)
    try container.encodeIfPresent(reply, forKey: .reply)
  }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DISCOVERY]

func applications() throws -> [Application] {
  let manager = FileManager.default
  let running = NSWorkspace.shared.runningApplications
  let installed = try manager.urls(for: .applicationDirectory, in: .allDomainsMask).flatMap { directory -> [URL] in
    let entries = manager.enumerator(at: directory, includingPropertiesForKeys: [.isApplicationKey], options: [.skipsPackageDescendants, .skipsHiddenFiles])
    return try entries?.allObjects.compactMap { $0 as? URL }.filter { try $0.resourceValues(forKeys: [.isApplicationKey]).isApplication == true } ?? []
  }
  return try Set((installed + running.compactMap(\.bundleURL)).map { $0.resolvingSymlinksInPath() }).compactMap { try Application(url: $0, running: running) }.sorted {
    $0.url.absoluteString < $1.url.absoluteString
  }
}

func definition(_ application: URL) -> Result<ScriptingDictionary, any Error> {
  var data: Unmanaged<CFData>?
  let status = OSACopyScriptingDefinitionFromURL(application as CFURL, 0, &data)
  guard status == noErr, let data else { return .failure(NSError(domain: NSOSStatusErrorDomain, code: Int(status))) }
  return Result { try ScriptingDictionary(data: data.takeRetainedValue() as Data) }
}

// --- [DESCRIPTORS]

func code<T: FixedWidthInteger & UnsignedInteger>(_ text: String, as: T.Type) -> Result<T, any Error> {
  let value =
    text.hasPrefix("0x") ? T(text.dropFirst(2), radix: 16) : text.data(using: .macOSRoman).flatMap { $0.count == MemoryLayout<T>.size ? $0.reduce(T.zero) { ($0 << UInt8.bitWidth) | T($1) } : nil }
  return value.map(Result.success) ?? .failure(CocoaError(.coderInvalidValue, userInfo: [NSLocalizedDescriptionKey: "Invalid scripting dictionary code: \(text)"]))
}

func descriptor(_ value: Any, declaration: XMLElement, dictionary: ScriptingDictionary) -> Result<NSAppleEventDescriptor, any Error> {
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
    switch (enumerations.isEmpty, !types.isEmpty && types.isSubset(of: ["file", "alias"])) {
    case (true, true):
      return FilePath(string).isAbsolute
        ? .success(NSAppleEventDescriptor(fileURL: URL(filePath: string)))
        : .failure(CocoaError(.fileReadInvalidFileName, userInfo: [NSLocalizedDescriptionKey: "File parameters require an absolute path: \(string)"]))
    case (true, false):
      return .success(NSAppleEventDescriptor(string: string))
    case (false, _):
      return (choices.count == 1 ? choices.first : nil).map { code($0, as: OSType.self).map(NSAppleEventDescriptor.init(enumCode:)) }
        ?? .failure(CocoaError(.coderInvalidValue, userInfo: [NSLocalizedDescriptionKey: "Enumeration value is unknown or ambiguous: \(string)"]))
    }
  case let record as [String: Any]:
    guard let type = record["type"] as? UInt32, let encoded = record["data"] as? String, let data = Data(base64Encoded: encoded),
      let descriptor = NSAppleEventDescriptor(descriptorType: type, data: data)
    else {
      return .failure(CocoaError(.coderInvalidValue, userInfo: [NSLocalizedDescriptionKey: "Native descriptors require an unsigned type code and base64 data"]))
    }
    return .success(descriptor)
  default:
    return .failure(CocoaError(.coderInvalidValue))
  }
}

// --- [EXECUTION]

func instance(of application: URL) -> Result<NSRunningApplication, any Error> {
  let running = Application.instances(of: application, in: NSWorkspace.shared.runningApplications)
  guard let process = running.first, running.count == 1 else {
    return .failure(NSError(domain: NSOSStatusErrorDomain, code: Int(procNotFound), userInfo: [NSLocalizedDescriptionKey: "Execution requires exactly one running instance of \(application.path)"]))
  }
  let observations = [\NSRunningApplication.isFinishedLaunching, \.isTerminated].map { process.observe($0) { _, _ in CFRunLoopStop(CFRunLoopGetMain()) } }
  withExtendedLifetime(observations) {
    if !(process.isFinishedLaunching || process.isTerminated) { CFRunLoopRun() }
  }
  return process.isTerminated
    ? .failure(NSError(domain: NSOSStatusErrorDomain, code: Int(procNotFound), userInfo: [NSLocalizedDescriptionKey: "\(application.path) terminated before it finished launching"]))
    : .success(process)
}

func event(process: NSRunningApplication, command: String, arguments: [String: Any], dictionary: ScriptingDictionary) -> Result<NSAppleEventDescriptor, any Error> {
  guard let declaration = dictionary.commands.first(where: { $0.attribute(forName: "name")?.stringValue == command }), let eventCode = declaration.attribute(forName: "code")?.stringValue else {
    return .failure(NSError(domain: NSOSStatusErrorDomain, code: Int(errAEEventNotHandled), userInfo: [NSLocalizedDescriptionKey: "Command is absent from the installed dictionary: \(command)"]))
  }
  let parameters = Dictionary(
    (declaration.elements(forName: "parameter") + declaration.elements(forName: "direct-parameter")).map { ($0.attribute(forName: "name")?.stringValue ?? "direct", $0) },
    uniquingKeysWith: { first, _ in first })
  let missing = parameters.filter { $0.value.attribute(forName: "optional")?.stringValue != "yes" && arguments[$0.key] == nil }.keys
  let values = arguments.sorted { $0.key < $1.key }.map { argument -> Result<(AEKeyword, NSAppleEventDescriptor), any Error> in
    guard let parameter = parameters[argument.key] else {
      return .failure(NSError(domain: NSOSStatusErrorDomain, code: Int(errAEParamMissed), userInfo: [NSLocalizedDescriptionKey: "Unknown parameter: \(argument.key)"]))
    }
    let keyword = parameter.attribute(forName: "code")?.stringValue.map { code($0, as: AEKeyword.self) } ?? .success(AEKeyword(keyDirectObject))
    return keyword.flatMap { key in descriptor(argument.value, declaration: parameter, dictionary: dictionary).map { (key, $0) } }
  }
  let errors =
    values.compactMap { value -> (any Error)? in
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
      for case .success(let (keyword, value)) in values {
        event.setParam(value, forKeyword: keyword)
      }
      return event
    }
    : .failure(
      CocoaError(.coderInvalidValue, userInfo: [NSMultipleUnderlyingErrorsKey: errors, NSLocalizedDescriptionKey: errors.map(\.localizedDescription).joined(separator: "; ")]))
}

func execute(application: URL, command: String, arguments: [String: Any]) -> Result<any Encodable, Failure> {
  instance(of: application)
    .flatMap { process in definition(application).flatMap { event(process: process, command: command, arguments: arguments, dictionary: $0) } }
    .mapError(Failure.request)
    .flatMap { event in Result { try event.sendEvent(options: [.waitForReply, .neverInteract], timeout: TimeInterval(kNoTimeOut)) }.mapError(Failure.send) }
    .flatMap { reply -> Result<any Encodable, Failure> in
      switch reply.paramDescriptor(forKeyword: AEKeyword(keyErrorNumber))?.int32Value {
      case .some(let status) where status != noErr:
        let message = reply.paramDescriptor(forKeyword: AEKeyword(keyErrorString))?.stringValue.map { [NSLocalizedDescriptionKey: $0] }
        return .failure(.reply(NSError(domain: NSOSStatusErrorDomain, code: Int(status), userInfo: message), Descriptor(reply)))
      default:
        return .success(Descriptor(reply))
      }
    }
}

// --- [REQUESTS]

func request(_ input: [String: Any]) -> Result<any Encodable, Failure> {
  let application = (input["application"] as? String).flatMap(URL.init(string:)).flatMap { url in
    url.isFileURL && Bundle(url: url).flatMap(Application.identifier) != nil ? url.resolvingSymlinksInPath() : nil
  }
  switch (input["operation"] as? String, application) {
  case ("applications", _):
    return Result { try applications() }.mapError(Failure.request)
  case ("dictionary", .some(let application)):
    return definition(application).map { $0.document.xmlString }.mapError(Failure.request)
  case ("execute", .some(let application)):
    guard let command = input["command"] as? String, let arguments = input["arguments"] as? [String: Any] else {
      return .failure(.request(CocoaError(.coderReadCorrupt)))
    }
    return execute(application: application, command: command, arguments: arguments)
  case ("dictionary", .none), ("execute", .none):
    return .failure(.request(CocoaError(.fileReadInvalidFileName)))
  case ("artifact", _):
    guard let address = input["url"] as? String, let url = URL(string: address), url.isFileURL else {
      return .failure(.request(CocoaError(.fileReadInvalidFileName)))
    }
    return Result { try Artifact(uri: url, mimeType: url.resourceValues(forKeys: [.contentTypeKey]).contentType?.preferredMIMEType, blob: Data(contentsOf: url)) }.mapError(Failure.request)
  default:
    return .failure(.request(CocoaError(.coderReadCorrupt)))
  }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

@main
struct Automation {
  static func main() throws {
    let result = Result { try FileHandle.standardInput.readToEnd().flatMap { try JSONSerialization.jsonObject(with: $0) as? [String: Any] } }.mapError(Failure.request)
      .flatMap { $0.map(request) ?? .failure(.request(CocoaError(.coderReadCorrupt))) }
      .flatMap { value in Result { try JSONEncoder().encode(value) }.mapError(Failure.request) }
    switch result {
    case .success(let data):
      try FileHandle.standardOutput.write(contentsOf: data)
    case .failure(let failure):
      try FileHandle.standardOutput.write(contentsOf: JSONEncoder().encode(failure))
      exit(EXIT_FAILURE)
    }
  }
}
