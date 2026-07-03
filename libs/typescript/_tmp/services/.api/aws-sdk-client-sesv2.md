# [API_CATALOGUE] @aws-sdk/client-sesv2

`@aws-sdk/client-sesv2` is the AWS SDK v3 modular client for Amazon SES API v2, providing `SESv2Client` for command-pattern dispatch, the `SESv2` aggregated client with method-per-operation convenience, 112 typed command classes covering email send, identity, configuration, suppression, reputation, deliverability, contacts, templates, export, and import operations, and paginators for all `List*` and `GetDedicatedIps` operations.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@aws-sdk/client-sesv2`
- package: `@aws-sdk/client-sesv2`
- module: `@aws-sdk/client-sesv2`
- asset: runtime library
- rail: email, aws

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client classes
- rail: email

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]     | [RAIL]                                   |
| :-----: | :----------------------------- | :---------------- | :--------------------------------------- |
|  [01]   | `SESv2Client`                  | service client    | base command-dispatch client             |
|  [02]   | `SESv2`                        | aggregated client | extends `SESv2Client` with method-per-op |
|  [03]   | `SESv2ClientConfig`            | config interface  | constructor configuration shape          |
|  [04]   | `SESv2ClientResolvedConfig`    | resolved config   | normalized resolved configuration        |
|  [05]   | `SESv2PaginationConfiguration` | paginator config  | `client` + `pageSize` + `startingToken`  |
|  [06]   | `SESv2ServiceException`        | base exception    | base for all SESv2 service errors        |

[PUBLIC_TYPE_SCOPE]: core message models
- rail: email

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]  | [RAIL]                                       |
| :-----: | :---------------------- | :------------- | :------------------------------------------- |
|  [01]   | `SendEmailRequest`      | request model  | simple, raw, and templated send input        |
|  [02]   | `SendEmailResponse`     | response model | `MessageId` result                           |
|  [03]   | `SendBulkEmailRequest`  | request model  | bulk send with per-entry replacements        |
|  [04]   | `SendBulkEmailResponse` | response model | per-entry `BulkEmailEntryResult`             |
|  [05]   | `Destination`           | address model  | `ToAddresses`, `CcAddresses`, `BccAddresses` |
|  [06]   | `Body`                  | content model  | `Text` and `Html` content pair               |
|  [07]   | `Template`              | content model  | name/ARN + data for templated send           |
|  [08]   | `Attachment`            | content model  | inline or attached binary content            |
|  [09]   | `BulkEmailEntry`        | bulk entry     | per-recipient content and tags               |
|  [10]   | `BulkEmailEntryResult`  | bulk result    | per-entry `BulkEmailStatus` outcome          |

[PUBLIC_TYPE_SCOPE]: key enums
- rail: email

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [RAIL]                                      |
| :-----: | :----------------------------- | :------------ | :------------------------------------------ |
|  [01]   | `BulkEmailStatus`              | string enum   | per-entry bulk send outcome                 |
|  [02]   | `EventType`                    | string enum   | configuration set event notification        |
|  [03]   | `IdentityType`                 | string enum   | `EMAIL_ADDRESS \| DOMAIN \| MANAGED_DOMAIN` |
|  [04]   | `DkimStatus`                   | string enum   | DKIM signing verification state             |
|  [05]   | `SuppressionListReason`        | string enum   | `BOUNCE \| COMPLAINT`                       |
|  [06]   | `TlsPolicy`                    | string enum   | `REQUIRE \| OPTIONAL`                       |
|  [07]   | `MailType`                     | string enum   | `MARKETING \| TRANSACTIONAL`                |
|  [08]   | `DeliverabilityTestStatus`     | string enum   | `IN_PROGRESS \| COMPLETED`                  |
|  [09]   | `ReviewStatus`                 | string enum   | `DENIED \| FAILED \| GRANTED \| PENDING`    |
|  [10]   | `AttachmentContentDisposition` | string enum   | `ATTACHMENT \| INLINE`                      |

[PUBLIC_TYPE_SCOPE]: error classes
- rail: email

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [RAIL]                                 |
| :-----: | :----------------------------------- | :------------ | :------------------------------------- |
|  [01]   | `AccountSuspendedException`          | client error  | account sending permanently restricted |
|  [02]   | `AlreadyExistsException`             | client error  | resource already exists                |
|  [03]   | `BadRequestException`                | client error  | invalid input                          |
|  [04]   | `NotFoundException`                  | client error  | resource not found                     |
|  [05]   | `TooManyRequestsException`           | client error  | rate limit exceeded                    |
|  [06]   | `LimitExceededException`             | client error  | account or resource quota reached      |
|  [07]   | `MailFromDomainNotVerifiedException` | client error  | MAIL FROM domain unverified            |
|  [08]   | `MessageRejected`                    | client error  | message failed content filter          |
|  [09]   | `SendingPausedException`             | client error  | account or configuration set paused    |
|  [10]   | `ConcurrentModificationException`    | client error  | optimistic concurrency conflict        |
|  [11]   | `ConflictException`                  | client error  | resource state conflict                |
|  [12]   | `InternalServiceErrorException`      | server error  | SES internal failure                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction and dispatch
- rail: email

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]   | [RAIL]                                    |
| :-----: | :------------------------------------------- | :--------------- | :---------------------------------------- |
|  [01]   | `new SESv2Client(config: SESv2ClientConfig)` | constructor      | create client with credentials and region |
|  [02]   | `client.send(command)`                       | command dispatch | send any typed command, returns promise   |
|  [03]   | `client.destroy()`                           | lifecycle        | release underlying HTTP connections       |

[ENTRYPOINT_SCOPE]: send commands
- rail: email

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :---------------------------------------------- | :------------- | :------------------------------------- |
|  [01]   | `new SendEmailCommand(input)`                   | send command   | simple, raw, or templated send         |
|  [02]   | `new SendBulkEmailCommand(input)`               | send command   | batch send with per-entry replacements |
|  [03]   | `new SendCustomVerificationEmailCommand(input)` | send command   | custom verification email              |

[ENTRYPOINT_SCOPE]: identity management commands
- rail: email

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :------------------------------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `new CreateEmailIdentityCommand(input)`                        | identity cmd   | verify email address or domain      |
|  [02]   | `new DeleteEmailIdentityCommand(input)`                        | identity cmd   | remove verified identity            |
|  [03]   | `new GetEmailIdentityCommand(input)`                           | identity cmd   | fetch identity configuration        |
|  [04]   | `new ListEmailIdentitiesCommand(input)`                        | identity cmd   | paginated identity list             |
|  [05]   | `new PutEmailIdentityDkimAttributesCommand(input)`             | identity cmd   | enable/disable DKIM signing         |
|  [06]   | `new PutEmailIdentityDkimSigningAttributesCommand(input)`      | identity cmd   | BYODKIM signing key config          |
|  [07]   | `new PutEmailIdentityFeedbackAttributesCommand(input)`         | identity cmd   | forwarding address config           |
|  [08]   | `new PutEmailIdentityMailFromAttributesCommand(input)`         | identity cmd   | MAIL FROM domain config             |
|  [09]   | `new CreateEmailIdentityPolicyCommand(input)`                  | identity cmd   | attach sending authorization policy |
|  [10]   | `new DeleteEmailIdentityPolicyCommand(input)`                  | identity cmd   | remove sending authorization policy |
|  [11]   | `new GetEmailIdentityPoliciesCommand(input)`                   | identity cmd   | list attached policies              |
|  [12]   | `new UpdateEmailIdentityPolicyCommand(input)`                  | identity cmd   | update policy document              |
|  [13]   | `new PutEmailIdentityConfigurationSetAttributesCommand(input)` | identity cmd   | bind configuration set              |

[ENTRYPOINT_SCOPE]: configuration set commands
- rail: email

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :--------------------------------------------------------- | :------------- | :--------------------------------- |
|  [01]   | `new CreateConfigurationSetCommand(input)`                 | config cmd     | create named configuration set     |
|  [02]   | `new DeleteConfigurationSetCommand(input)`                 | config cmd     | delete configuration set           |
|  [03]   | `new GetConfigurationSetCommand(input)`                    | config cmd     | fetch configuration set settings   |
|  [04]   | `new ListConfigurationSetsCommand(input)`                  | config cmd     | paginated list                     |
|  [05]   | `new CreateConfigurationSetEventDestinationCommand(input)` | config cmd     | add event notification destination |
|  [06]   | `new DeleteConfigurationSetEventDestinationCommand(input)` | config cmd     | remove event destination           |
|  [07]   | `new GetConfigurationSetEventDestinationsCommand(input)`   | config cmd     | list event destinations            |
|  [08]   | `new UpdateConfigurationSetEventDestinationCommand(input)` | config cmd     | update event destination           |
|  [09]   | `new PutConfigurationSetDeliveryOptionsCommand(input)`     | config cmd     | TLS policy and sending pool        |
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
|  [01]   | `new PutSuppressedDestinationCommand(input)`    | suppression cmd | add address to suppression list      |
|  [02]   | `new DeleteSuppressedDestinationCommand(input)` | suppression cmd | remove address from suppression list |
|  [03]   | `new GetSuppressedDestinationCommand(input)`    | suppression cmd | fetch suppression entry              |
|  [04]   | `new ListSuppressedDestinationsCommand(input)`  | suppression cmd | paginated suppression list           |
|  [05]   | `new CreateEmailTemplateCommand(input)`         | template cmd    | create Handlebars email template     |
|  [06]   | `new DeleteEmailTemplateCommand(input)`         | template cmd    | delete template                      |
|  [07]   | `new GetEmailTemplateCommand(input)`            | template cmd    | fetch template content               |
|  [08]   | `new ListEmailTemplatesCommand(input)`          | template cmd    | paginated template list              |
|  [09]   | `new UpdateEmailTemplateCommand(input)`         | template cmd    | update template content              |
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
|  [01]   | `paginateListEmailIdentities(config, input)`        | paginator      | async iterable of identity pages       |
|  [02]   | `paginateListConfigurationSets(config, input)`      | paginator      | async iterable of config set pages     |
|  [03]   | `paginateListEmailTemplates(config, input)`         | paginator      | async iterable of template pages       |
|  [04]   | `paginateListContacts(config, input)`               | paginator      | async iterable of contact pages        |
|  [05]   | `paginateListContactLists(config, input)`           | paginator      | async iterable of contact list pages   |
|  [06]   | `paginateListSuppressedDestinations(config, input)` | paginator      | async iterable of suppression pages    |
|  [07]   | `paginateListImportJobs(config, input)`             | paginator      | async iterable of import job pages     |
|  [08]   | `paginateListExportJobs(config, input)`             | paginator      | async iterable of export job pages     |
|  [09]   | `paginateGetDedicatedIps(config, input)`            | paginator      | async iterable of dedicated IP pages   |
|  [10]   | `paginateListRecommendations(config, input)`        | paginator      | async iterable of recommendation pages |

## [04]-[IMPLEMENTATION_LAW]

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
