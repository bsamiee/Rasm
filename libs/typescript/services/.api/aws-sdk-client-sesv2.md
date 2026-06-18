# [API_CATALOGUE] @aws-sdk/client-sesv2

`@aws-sdk/client-sesv2` is the AWS SDK v3 modular client for Amazon SES API v2, providing `SESv2Client` for command-pattern dispatch, the `SESv2` aggregated client with method-per-operation convenience, 112 typed command classes covering email send, identity, configuration, suppression, reputation, deliverability, contacts, templates, export, and import operations, and paginators for all `List*` and `GetDedicatedIps` operations.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@aws-sdk/client-sesv2`
- package: `@aws-sdk/client-sesv2`
- module: `@aws-sdk/client-sesv2`
- asset: runtime library
- rail: email, aws

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client classes
- rail: email

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]     | [RAIL]                                   |
| :-----: | :----------------------------- | :---------------- | :--------------------------------------- |
|   [1]   | `SESv2Client`                  | service client    | base command-dispatch client             |
|   [2]   | `SESv2`                        | aggregated client | extends `SESv2Client` with method-per-op |
|   [3]   | `SESv2ClientConfig`            | config interface  | constructor configuration shape          |
|   [4]   | `SESv2ClientResolvedConfig`    | resolved config   | normalized resolved configuration        |
|   [5]   | `SESv2PaginationConfiguration` | paginator config  | `client` + `pageSize` + `startingToken`  |
|   [6]   | `SESv2ServiceException`        | base exception    | base for all SESv2 service errors        |

[PUBLIC_TYPE_SCOPE]: core message models
- rail: email

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]  | [RAIL]                                       |
| :-----: | :---------------------- | :------------- | :------------------------------------------- |
|   [1]   | `SendEmailRequest`      | request model  | simple, raw, and templated send input        |
|   [2]   | `SendEmailResponse`     | response model | `MessageId` result                           |
|   [3]   | `SendBulkEmailRequest`  | request model  | bulk send with per-entry replacements        |
|   [4]   | `SendBulkEmailResponse` | response model | per-entry `BulkEmailEntryResult`             |
|   [5]   | `Destination`           | address model  | `ToAddresses`, `CcAddresses`, `BccAddresses` |
|   [6]   | `Body`                  | content model  | `Text` and `Html` content pair               |
|   [7]   | `Template`              | content model  | name/ARN + data for templated send           |
|   [8]   | `Attachment`            | content model  | inline or attached binary content            |
|   [9]   | `BulkEmailEntry`        | bulk entry     | per-recipient content and tags               |
|  [10]   | `BulkEmailEntryResult`  | bulk result    | per-entry `BulkEmailStatus` outcome          |

[PUBLIC_TYPE_SCOPE]: key enums
- rail: email

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [RAIL]                                      |
| :-----: | :----------------------------- | :------------ | :------------------------------------------ |
|   [1]   | `BulkEmailStatus`              | string enum   | per-entry bulk send outcome                 |
|   [2]   | `EventType`                    | string enum   | configuration set event notification        |
|   [3]   | `IdentityType`                 | string enum   | `EMAIL_ADDRESS \| DOMAIN \| MANAGED_DOMAIN` |
|   [4]   | `DkimStatus`                   | string enum   | DKIM signing verification state             |
|   [5]   | `SuppressionListReason`        | string enum   | `BOUNCE \| COMPLAINT`                       |
|   [6]   | `TlsPolicy`                    | string enum   | `REQUIRE \| OPTIONAL`                       |
|   [7]   | `MailType`                     | string enum   | `MARKETING \| TRANSACTIONAL`                |
|   [8]   | `DeliverabilityTestStatus`     | string enum   | `IN_PROGRESS \| COMPLETED`                  |
|   [9]   | `ReviewStatus`                 | string enum   | `DENIED \| FAILED \| GRANTED \| PENDING`    |
|  [10]   | `AttachmentContentDisposition` | string enum   | `ATTACHMENT \| INLINE`                      |

[PUBLIC_TYPE_SCOPE]: error classes
- rail: email

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [RAIL]                                 |
| :-----: | :----------------------------------- | :------------ | :------------------------------------- |
|   [1]   | `AccountSuspendedException`          | client error  | account sending permanently restricted |
|   [2]   | `AlreadyExistsException`             | client error  | resource already exists                |
|   [3]   | `BadRequestException`                | client error  | invalid input                          |
|   [4]   | `NotFoundException`                  | client error  | resource not found                     |
|   [5]   | `TooManyRequestsException`           | client error  | rate limit exceeded                    |
|   [6]   | `LimitExceededException`             | client error  | account or resource quota reached      |
|   [7]   | `MailFromDomainNotVerifiedException` | client error  | MAIL FROM domain unverified            |
|   [8]   | `MessageRejected`                    | client error  | message failed content filter          |
|   [9]   | `SendingPausedException`             | client error  | account or configuration set paused    |
|  [10]   | `ConcurrentModificationException`    | client error  | optimistic concurrency conflict        |
|  [11]   | `ConflictException`                  | client error  | resource state conflict                |
|  [12]   | `InternalServiceErrorException`      | server error  | SES internal failure                   |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction and dispatch
- rail: email

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]   | [RAIL]                                    |
| :-----: | :------------------------------------------- | :--------------- | :---------------------------------------- |
|   [1]   | `new SESv2Client(config: SESv2ClientConfig)` | constructor      | create client with credentials and region |
|   [2]   | `client.send(command)`                       | command dispatch | send any typed command, returns promise   |
|   [3]   | `client.destroy()`                           | lifecycle        | release underlying HTTP connections       |

[ENTRYPOINT_SCOPE]: send commands
- rail: email

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :---------------------------------------------- | :------------- | :------------------------------------- |
|   [1]   | `new SendEmailCommand(input)`                   | send command   | simple, raw, or templated send         |
|   [2]   | `new SendBulkEmailCommand(input)`               | send command   | batch send with per-entry replacements |
|   [3]   | `new SendCustomVerificationEmailCommand(input)` | send command   | custom verification email              |

[ENTRYPOINT_SCOPE]: identity management commands
- rail: email

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :------------------------------------------------------------- | :------------- | :---------------------------------- |
|   [1]   | `new CreateEmailIdentityCommand(input)`                        | identity cmd   | verify email address or domain      |
|   [2]   | `new DeleteEmailIdentityCommand(input)`                        | identity cmd   | remove verified identity            |
|   [3]   | `new GetEmailIdentityCommand(input)`                           | identity cmd   | fetch identity configuration        |
|   [4]   | `new ListEmailIdentitiesCommand(input)`                        | identity cmd   | paginated identity list             |
|   [5]   | `new PutEmailIdentityDkimAttributesCommand(input)`             | identity cmd   | enable/disable DKIM signing         |
|   [6]   | `new PutEmailIdentityDkimSigningAttributesCommand(input)`      | identity cmd   | BYODKIM signing key config          |
|   [7]   | `new PutEmailIdentityFeedbackAttributesCommand(input)`         | identity cmd   | forwarding address config           |
|   [8]   | `new PutEmailIdentityMailFromAttributesCommand(input)`         | identity cmd   | MAIL FROM domain config             |
|   [9]   | `new CreateEmailIdentityPolicyCommand(input)`                  | identity cmd   | attach sending authorization policy |
|  [10]   | `new DeleteEmailIdentityPolicyCommand(input)`                  | identity cmd   | remove sending authorization policy |
|  [11]   | `new GetEmailIdentityPoliciesCommand(input)`                   | identity cmd   | list attached policies              |
|  [12]   | `new UpdateEmailIdentityPolicyCommand(input)`                  | identity cmd   | update policy document              |
|  [13]   | `new PutEmailIdentityConfigurationSetAttributesCommand(input)` | identity cmd   | bind configuration set              |

[ENTRYPOINT_SCOPE]: configuration set commands
- rail: email

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :--------------------------------------------------------- | :------------- | :--------------------------------- |
|   [1]   | `new CreateConfigurationSetCommand(input)`                 | config cmd     | create named configuration set     |
|   [2]   | `new DeleteConfigurationSetCommand(input)`                 | config cmd     | delete configuration set           |
|   [3]   | `new GetConfigurationSetCommand(input)`                    | config cmd     | fetch configuration set settings   |
|   [4]   | `new ListConfigurationSetsCommand(input)`                  | config cmd     | paginated list                     |
|   [5]   | `new CreateConfigurationSetEventDestinationCommand(input)` | config cmd     | add event notification destination |
|   [6]   | `new DeleteConfigurationSetEventDestinationCommand(input)` | config cmd     | remove event destination           |
|   [7]   | `new GetConfigurationSetEventDestinationsCommand(input)`   | config cmd     | list event destinations            |
|   [8]   | `new UpdateConfigurationSetEventDestinationCommand(input)` | config cmd     | update event destination           |
|   [9]   | `new PutConfigurationSetDeliveryOptionsCommand(input)`     | config cmd     | TLS policy and sending pool        |
|  [10]   | `new PutConfigurationSetSendingOptionsCommand(input)`      | config cmd     | enable/disable sending             |
|  [11]   | `new PutConfigurationSetSuppressionOptionsCommand(input)`  | config cmd     | override account suppression       |
|  [12]   | `new PutConfigurationSetReputationOptionsCommand(input)`   | config cmd     | reputation tracking toggle         |
|  [13]   | `new PutConfigurationSetTrackingOptionsCommand(input)`     | config cmd     | click/open tracking domain         |
|  [14]   | `new PutConfigurationSetVdmOptionsCommand(input)`          | config cmd     | VDM engagement settings            |
|  [15]   | `new PutConfigurationSetArchivingOptionsCommand(input)`    | config cmd     | archiving options                  |

[ENTRYPOINT_SCOPE]: suppression, template, and contact commands
- rail: email

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY]  | [RAIL]                               |
| :-----: | :---------------------------------------------- | :-------------- | :----------------------------------- |
|   [1]   | `new PutSuppressedDestinationCommand(input)`    | suppression cmd | add address to suppression list      |
|   [2]   | `new DeleteSuppressedDestinationCommand(input)` | suppression cmd | remove address from suppression list |
|   [3]   | `new GetSuppressedDestinationCommand(input)`    | suppression cmd | fetch suppression entry              |
|   [4]   | `new ListSuppressedDestinationsCommand(input)`  | suppression cmd | paginated suppression list           |
|   [5]   | `new CreateEmailTemplateCommand(input)`         | template cmd    | create Handlebars email template     |
|   [6]   | `new DeleteEmailTemplateCommand(input)`         | template cmd    | delete template                      |
|   [7]   | `new GetEmailTemplateCommand(input)`            | template cmd    | fetch template content               |
|   [8]   | `new ListEmailTemplatesCommand(input)`          | template cmd    | paginated template list              |
|   [9]   | `new UpdateEmailTemplateCommand(input)`         | template cmd    | update template content              |
|  [10]   | `new TestRenderEmailTemplateCommand(input)`     | template cmd    | preview rendered template output     |
|  [11]   | `new CreateContactListCommand(input)`           | contact cmd     | create contact list with topics      |
|  [12]   | `new CreateContactCommand(input)`               | contact cmd     | add contact to list                  |
|  [13]   | `new UpdateContactCommand(input)`               | contact cmd     | update contact attributes            |
|  [14]   | `new DeleteContactCommand(input)`               | contact cmd     | remove contact from list             |
|  [15]   | `new ListContactsCommand(input)`                | contact cmd     | paginated contact list               |
|  [16]   | `new ListContactListsCommand(input)`            | contact cmd     | paginated contact list index         |

[ENTRYPOINT_SCOPE]: paginators
- rail: email

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :-------------------------------------------------- | :------------- | :------------------------------------- |
|   [1]   | `paginateListEmailIdentities(config, input)`        | paginator      | async iterable of identity pages       |
|   [2]   | `paginateListConfigurationSets(config, input)`      | paginator      | async iterable of config set pages     |
|   [3]   | `paginateListEmailTemplates(config, input)`         | paginator      | async iterable of template pages       |
|   [4]   | `paginateListContacts(config, input)`               | paginator      | async iterable of contact pages        |
|   [5]   | `paginateListContactLists(config, input)`           | paginator      | async iterable of contact list pages   |
|   [6]   | `paginateListSuppressedDestinations(config, input)` | paginator      | async iterable of suppression pages    |
|   [7]   | `paginateListImportJobs(config, input)`             | paginator      | async iterable of import job pages     |
|   [8]   | `paginateListExportJobs(config, input)`             | paginator      | async iterable of export job pages     |
|   [9]   | `paginateGetDedicatedIps(config, input)`            | paginator      | async iterable of dedicated IP pages   |
|  [10]   | `paginateListRecommendations(config, input)`        | paginator      | async iterable of recommendation pages |

## [4]-[IMPLEMENTATION_LAW]

[CLIENT_TOPOLOGY]:
- `SESv2Client` is the low-level dispatch surface; `SESv2` extends it with a method per command for ergonomic use
- all commands follow the `new XxxCommand(input: XxxCommandInput)` + `client.send(cmd)` pattern
- `SESv2ClientConfig` fields: `region` (required), `credentials` (`AwsCredentialIdentityProvider`), `endpoint`, `maxAttempts`, `logger`, `httpHandler`
- SDK v3 modular design: import only the commands needed; tree-shaking removes unused command bundles

[SEND_FAMILY]:
- `SendEmailRequest.Content` is a union discriminated by presence of `Simple`, `Raw`, or `Template`; only one is set per request
- `Simple` carries `Subject` (`Content`) + `Body` (`Text` and/or `Html`); `Raw` carries a `Data` blob with MIME headers; `Template` carries `TemplateName`/`TemplateArn` + `TemplateData`
- `Destination` accepts up to 50 recipients across `ToAddresses`, `CcAddresses`, `BccAddresses`
- bulk send requires a `DefaultContent` plus per-entry `ReplacementEmailContent` and `ReplacementTags`
- `Attachments` field on `SendEmailRequest` supports inline or attached binary payloads via `AttachmentContentDisposition`

[PAGINATION_LAW]:
- all `paginate*` helpers accept `SESv2PaginationConfiguration` (`client` + optional `pageSize` + `startingToken`) and return `AsyncIterable`
- `pageSize` maps to the underlying command's `PageSize` input field
- command-level `NextToken` is managed internally by paginators; do not set it manually when using paginators

[ERROR_DISCIPLINE]:
- all service errors extend `SESv2ServiceException`; narrow on `instanceof XxxException` or `error.name`
- `MessageRejected` and `MailFromDomainNotVerifiedException` are domain-logic errors requiring bounce handling
- `TooManyRequestsException` signals throttling; apply exponential backoff via SDK retry config (`maxAttempts`)
- `AccountSuspendedException` and `SendingPausedException` require account-level remediation, not retry

[RAIL_LAW]:
- package: `@aws-sdk/client-sesv2`
- owns: typed SES API v2 client, command classes, paginators, model and error types
- accept: `client.send(new XxxCommand(input))` for all operations; paginators for list operations
- reject: hand-rolled HTTP calls to SES endpoints, `@aws-sdk/client-ses` (v1 API) for new code
