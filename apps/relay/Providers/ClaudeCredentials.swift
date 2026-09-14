import CryptoKit
import Foundation

nonisolated struct ClaudeOAuthToken: Sendable {
  let value: JSONValue
  let accessToken: String
  let expiresAt: Date?
  let refreshTokenExpiresAt: Date?
  let plan: String?

  var fingerprint: SHA256Digest { SHA256.hash(data: Data(accessToken.utf8)) }

  func needsRefresh(at now: Date) -> Bool {
    let accessExpiring: Bool =
      expiresAt.map { expiry in expiry <= now.addingTimeInterval(300) } ?? true
    let refreshTokenExpiring: Bool =
      refreshTokenExpiresAt.map { expiry in expiry <= now.addingTimeInterval(86_400) } ?? false
    return accessExpiring || refreshTokenExpiring
  }

  static func make(_ value: JSONValue) -> Result<ClaudeOAuthToken?, ClaudeFailure> {
    guard let token: String = value["accessToken"]?.stringValue,
      let scopeValues: [JSONValue] = value["scopes"]?.arrayValue
    else {
      return .failure(.invalidCredentials)
    }
    let refresh: String? = value["refreshToken"]?.stringValue
    guard !token.isEmpty, refresh != "" else { return .success(nil) }
    let scopes: [String] = scopeValues.compactMap { scope in
      scope.stringValue.flatMap { name in name.isEmpty ? nil : name }
    }
    let checkedScopes: Result<Void, ClaudeFailures> =
      scopes.count == scopeValues.count
      ? .success(()) : .failure(ClaudeFailures(.invalidCredentials))
    return combine(
      checkedScopes, milliseconds(value["expiresAt"]), milliseconds(value["refreshTokenExpiresAt"])
    )
    .mapError { failure in failure.first }
    .map { _, expiry, refreshTokenExpiry in
      ClaudeOAuthToken(
        value: value,
        accessToken: token,
        expiresAt: expiry,
        refreshTokenExpiresAt: refreshTokenExpiry,
        plan: value["subscriptionType"]?.stringValue
      )
    }
  }

  private static func milliseconds(_ value: JSONValue?) -> Result<Date?, ClaudeFailures> {
    switch value {
    case .some(.number(let milliseconds)) where milliseconds.isFinite && milliseconds > 0:
      .success(Date(timeIntervalSince1970: milliseconds / 1000))
    case .some(.number), .some(.null), .none: .success(nil)
    case .some: .failure(ClaudeFailures(.invalidCredentials))
    }
  }
}

nonisolated struct ClaudeFailures: AggregateError {
  let first: ClaudeFailure
  let remaining: [ClaudeFailure]
}

nonisolated struct ClaudeCredential: Sendable {
  let item: [String: JSONValue]
  let token: ClaudeOAuthToken
  let account: JSONValue
  let identity: AccountIdentity
}

nonisolated enum ClaudeStoreContent: Sendable {
  case empty
  case signedOut(AccountIdentity?)
  case credential(ClaudeCredential)

  var credential: ClaudeCredential? {
    if case .credential(let credential) = self { credential } else { nil }
  }
}

nonisolated struct ClaudeCredentialStore: Sendable {
  let directory: URL
  let configFile: URL
  let directoryConfigFile: URL
  let configDirectoryPath: String?
  let service: String
  let username: String

  init(
    directory: URL, configFile: URL, directoryConfigFile: URL, configDirectoryPath: String?,
    username: String
  ) {
    self.directory = directory
    self.configFile = configFile
    self.directoryConfigFile = directoryConfigFile
    self.configDirectoryPath = configDirectoryPath
    service =
      configDirectoryPath.map { value in
        "Claude Code-credentials-" + SHA256.hash(data: Data(value.utf8)).prefix(4).hexEncoded
      } ?? "Claude Code-credentials"
    self.username = username
  }

  func read() async -> Result<ClaudeStoreContent, ClaudeFailure> {
    await readItem().flatMap(content(of:))
  }

  private func content(
    of item: [String: JSONValue]?
  ) -> Result<ClaudeStoreContent, ClaudeFailure> {
    guard let item, let value: JSONValue = item["claudeAiOauth"], value != .null else {
      return identity().map { identity in identity.map(ClaudeStoreContent.signedOut) ?? .empty }
    }
    return ClaudeOAuthToken.make(value).flatMap { token in
      readAccount().flatMap { account in Self.content(item: item, token: token, account: account) }
    }
  }

  private static func content(
    item: [String: JSONValue], token: ClaudeOAuthToken?, account: JSONValue?
  ) -> Result<ClaudeStoreContent, ClaudeFailure> {
    account.map { account in
      identity(of: account, plan: token?.plan).map { identity in
        token.map { token in
          .credential(
            ClaudeCredential(item: item, token: token, account: account, identity: identity))
        } ?? .signedOut(identity)
      }
    } ?? .success(.empty)
  }

  func readItem() async -> Result<[String: JSONValue]?, ClaudeFailure> {
    await Keychain.readGenericPassword(service: service, account: username)
      .claude()
      .flatMap { data in
        data.map { data in Self.decodeJSONObject(data).map(Optional.some) } ?? .success(nil)
      }
  }

  func writeItem(_ object: [String: JSONValue]) async -> Result<Void, ClaudeFailure> {
    await Result { try JSONEncoder().encode(JSONValue.object(object)) }
      .mapError(ClaudeFailure.filesystem)
      .bind { data in
        await Keychain.writeGenericPassword(service: service, account: username, data: data)
          .claude()
      }
  }

  func deleteItem() async -> Result<Void, ClaudeFailure> {
    await Keychain.deleteGenericPassword(service: service, account: username).claude()
  }

  func identity() -> Result<AccountIdentity?, ClaudeFailure> {
    readAccount().flatMap { account in
      account.map { account in Self.identity(of: account, plan: nil).map(Optional.some) }
        ?? .success(nil)
    }
  }

  func readAccount() -> Result<JSONValue?, ClaudeFailure> {
    Self.readJSONObject(at: existingConfigFile, missingIsEmpty: true).map { configuration in
      configuration["oauthAccount"].flatMap { value in value == .null ? nil : value }
    }
  }

  func writeAccount(_ account: JSONValue) -> Result<Void, ClaudeFailure> {
    let file: URL = existingConfigFile
    return Self.readJSONObject(at: file, missingIsEmpty: true).flatMap { configuration in
      var updated: [String: JSONValue] = configuration
      updated["oauthAccount"] = account
      updated["hasCompletedOnboarding"] = .bool(true)
      return Self.writeJSONObject(updated, to: file)
    }
  }

  static func identity(of account: JSONValue, plan: String?) -> Result<
    AccountIdentity, ClaudeFailure
  > {
    guard let accountID: String = account["accountUuid"]?.stringValue,
      let email: String = account["emailAddress"]?.stringValue
    else {
      return .failure(.invalidCredentials)
    }
    return AccountIdentity.make(
      accountID: accountID,
      organizationID: account["organizationUuid"]?.stringValue,
      email: email,
      plan: plan ?? account["organizationType"]?.stringValue
    ).mapError(ClaudeFailure.invalidIdentity)
  }

  private var existingConfigFile: URL {
    FileManager.default.fileExists(atPath: directoryConfigFile.path)
      ? directoryConfigFile : configFile
  }

  private static func readJSONObject(at url: URL, missingIsEmpty: Bool) -> Result<
    [String: JSONValue], ClaudeFailure
  > {
    Result { try Data(contentsOf: url) }.map(Optional.some)
      .flatMapError { error -> Result<Data?, ClaudeFailure> in
        missingIsEmpty && (error as? CocoaError)?.code == .fileReadNoSuchFile
          ? .success(nil) : .failure(.filesystem(error))
      }
      .flatMap { data in data.map(decodeJSONObject) ?? .success([:]) }
  }

  private static func decodeJSONObject(_ data: Data) -> Result<[String: JSONValue], ClaudeFailure> {
    Result { try JSONDecoder().decode(JSONValue.self, from: data) }
      .mapError { _ in ClaudeFailure.invalidCredentials }
      .flatMap { value -> Result<[String: JSONValue], ClaudeFailure> in
        value.objectValue.map(Result.success) ?? .failure(.invalidCredentials)
      }
  }

  private static func writeJSONObject(_ object: [String: JSONValue], to url: URL) -> Result<
    Void, ClaudeFailure
  > {
    Result {
      try FileManager.default.createDirectory(
        at: url.deletingLastPathComponent(), withIntermediateDirectories: true)
      try JSONEncoder().encode(JSONValue.object(object)).write(to: url, options: .atomic)
    }
    .mapError(ClaudeFailure.filesystem)
  }
}
