# [API_CATALOGUE] @aws-sdk/client-sesv2

`@aws-sdk/client-sesv2` is the AWS SDK v3 modular client for Amazon SES API v2: `SESv2Client` dispatches any of the 111 `*Command` classes through `client.send(cmd)`, `SESv2` is the aggregated method-per-operation client, and the package exports the send/identity/configuration-set/suppression/template/contact/tenant/export/import/account command families, the `paginate*` list generators, the model types, and the `SESv2ServiceException` hierarchy. `services` uses it as the raw email provider under `nodemailer`'s `SESTransport` (and for the templated/bulk/tenant operations `nodemailer` does not model); there is no admitted `@effect-aws/client-sesv2`, so the mail owner dispatches SES commands at an `Effect.tryPromise` boundary — unlike S3, which rides the `@effect-aws/client-s3` `S3Service` tag.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@aws-sdk/client-sesv2`
- package: `@aws-sdk/client-sesv2` (target 3.1078.0 — the trio-lockstep line, Apache-2.0, © Amazon.com)
- module format: dual ESM (`dist-es`) / CJS (`dist-cjs`), `.d.ts` at `dist-types/index.d.ts`; one flat barrel — command classes individually importable for tree-shaking
- runtime target: Node `>=20.0.0`; the credentialed client is node-only
- [SOURCE_DERIVED]: reflection blocked — this package is NOT installed and NOT in the workspace catalog; the surface is transcribed from the published `@aws-sdk/client-sesv2` `.d.ts` declarations (111 `*Command` classes, 19 paginators, zero waiters; the barrel mirrors `@aws-sdk/client-s3` — `SESv2Client`/`SESv2`/`commands`/`pagination`/`models/{enums,errors,models_0,models_1}`/`SESv2ServiceException`). On admission, pin it at the `@aws-sdk/client-s3` 3.1078.0 trio-lockstep line, then re-verify via `api resolve`
- consumer: `nodemailer` `SESTransport` (`nodemailer.md`); no Effect-native wrapper — the mail owner (`persistence/work`) bridges at a `tryPromise`
- rail: email / aws

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client, config, and exception base
- rail: email / aws

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]     | [CAPABILITY]                                  |
| :-----: | :----------------------------- | :---------------- | :-------------------------------------------- |
|  [01]   | `SESv2Client`                  | service client    | credential + region holder; `send(cmd)` dispatch |
|  [02]   | `SESv2`                        | aggregated client | `extends SESv2Client` with a method per operation |
|  [03]   | `SESv2ClientConfig`            | config interface  | `region`, `credentials`, `endpoint`, `maxAttempts`, `logger`, `requestHandler` |
|  [04]   | `SESv2ClientResolvedConfig`    | resolved config   | fully resolved config after construction      |
|  [05]   | `SESv2PaginationConfiguration` | paginator config  | `{ client, pageSize?, startingToken? }` (`extends PaginationConfiguration`) |
|  [06]   | `SESv2ServiceException`        | base exception    | base of every SESv2 service error             |

[PUBLIC_TYPE_SCOPE]: the send model — `EmailContent` is the Simple/Raw/Template carrier
- rail: email / aws

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]  | [CAPABILITY]                                                    |
| :-----: | :------------------------ | :------------- | :------------------------------------------------------------- |
|  [01]   | `SendEmailRequest`        | request model  | `{ FromEmailAddress?, Destination?, Content: EmailContent, ListManagementOptions?, … }` |
|  [02]   | `EmailContent`            | union carrier  | `{ Simple?: Message; Raw?: RawMessage; Template?: Template }` — exactly one set per send |
|  [03]   | `Message` / `Content` / `Body` | content models | `Message = { Subject: Content, Body: Body, Attachments? }`; `Body = { Text?, Html? }` |
|  [04]   | `Template`                | content model  | `{ TemplateName? \| TemplateArn?, TemplateData, Attachments? }` templated send |
|  [05]   | `Attachment`              | content model  | inline/attached binary via `AttachmentContentDisposition`      |
|  [06]   | `Destination`            | address model  | `ToAddresses` / `CcAddresses` / `BccAddresses` (≤ 50 total)     |
|  [07]   | `ListManagementOptions`   | send option    | binds `ContactListName` + `TopicName` for one-click unsubscribe |
|  [08]   | `SendBulkEmailRequest`    | request model  | `DefaultContent` + per-entry `BulkEmailEntry`                   |
|  [09]   | `BulkEmailEntry` / `ReplacementEmailContent` | bulk entry | per-recipient `Destination` + `ReplacementEmailContent` + `ReplacementTags` |
|  [10]   | `SendEmailResponse` / `SendBulkEmailResponse` / `BulkEmailEntryResult` | response models | `MessageId`; per-entry `BulkEmailStatus` outcome |

[PUBLIC_TYPE_SCOPE]: key enums (`const` value-maps in v3)
- rail: email / aws

| [INDEX] | [SYMBOL]                          | [CAPABILITY]                                |
| :-----: | :-------------------------------- | :------------------------------------------ |
|  [01]   | `BulkEmailStatus`                 | per-entry bulk send outcome                 |
|  [02]   | `IdentityType`                    | `EMAIL_ADDRESS \| DOMAIN \| MANAGED_DOMAIN` |
|  [03]   | `DkimStatus` / `SuppressionListReason` | DKIM state; `BOUNCE \| COMPLAINT`      |
|  [04]   | `TlsPolicy` / `MailType`          | `REQUIRE \| OPTIONAL`; `MARKETING \| TRANSACTIONAL` |
|  [05]   | `EventType`                       | configuration-set event notification kind   |
|  [06]   | `DeliverabilityTestStatus` / `ReviewStatus` | `IN_PROGRESS \| COMPLETED`; `DENIED \| FAILED \| GRANTED \| PENDING` |
|  [07]   | `AttachmentContentDisposition`    | `ATTACHMENT \| INLINE`                      |
|  [08]   | `ContactLanguage` / `SubscriptionStatus` | contact-list localization; `OPT_IN \| OPT_OUT` |

[PUBLIC_TYPE_SCOPE]: exception hierarchy (narrow by `instanceof` or `error.name`)
- rail: email / aws

| [INDEX] | [SYMBOL]                                                  | [CAPABILITY]                             |
| :-----: | :-------------------------------------------------------- | :--------------------------------------- |
|  [01]   | `MessageRejected`                                         | message failed the content filter        |
|  [02]   | `MailFromDomainNotVerifiedException`                      | MAIL FROM domain unverified              |
|  [03]   | `AccountSuspendedException` / `SendingPausedException`    | account/config-set sending halted        |
|  [04]   | `TooManyRequestsException` / `LimitExceededException`     | throttling / quota reached               |
|  [05]   | `BadRequestException` / `NotFoundException` / `AlreadyExistsException` | invalid input / missing / duplicate |
|  [06]   | `ConflictException` / `ConcurrentModificationException`   | resource-state / optimistic-concurrency conflict |
|  [07]   | `InvalidNextTokenException`                               | malformed pagination token               |
|  [08]   | `InternalServiceErrorException`                           | SES internal failure                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: client construction and dispatch
- rail: email / aws

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]   | [CAPABILITY]                              |
| :-----: | :------------------------------------------- | :--------------- | :---------------------------------------- |
|  [01]   | `new SESv2Client(config: SESv2ClientConfig)` | constructor      | one client per scope; reusable            |
|  [02]   | `client.send(command)`                       | dispatch         | the single entry — any `*Command` → `Promise<Output>` |
|  [03]   | `client.destroy()`                           | lifecycle        | release the underlying HTTP connections   |

[ENTRYPOINT_SCOPE]: command families — `new XxxCommand(input)` + `send`; import per operation (representative)
- rail: email / aws

| [INDEX] | [FAMILY]        | [COMMANDS]                                                                                       |
| :-----: | :-------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | send            | `SendEmailCommand`, `SendBulkEmailCommand`, `SendCustomVerificationEmailCommand`                  |
|  [02]   | identity        | `CreateEmailIdentityCommand`, `DeleteEmailIdentityCommand`, `GetEmailIdentityCommand`, `ListEmailIdentitiesCommand`, `PutEmailIdentityDkimAttributesCommand`, `PutEmailIdentityDkimSigningAttributesCommand`, `PutEmailIdentityMailFromAttributesCommand`, `{Create,Delete,Get,Update}EmailIdentityPolicyCommand` |
|  [03]   | configuration set | `CreateConfigurationSetCommand`, `{Get,List,Delete}ConfigurationSetCommand`, `{Create,Get,Update,Delete}ConfigurationSetEventDestinationCommand`, `PutConfigurationSetDeliveryOptionsCommand`, `PutConfigurationSet{Sending,Suppression,Reputation,Tracking,Vdm,Archiving}OptionsCommand` |
|  [04]   | suppression     | `PutSuppressedDestinationCommand`, `{Get,Delete,List}SuppressedDestinationCommand`                |
|  [05]   | template        | `CreateEmailTemplateCommand`, `{Get,List,Update,Delete}EmailTemplateCommand`, `TestRenderEmailTemplateCommand` |
|  [06]   | contact         | `CreateContactListCommand`, `{Create,Update,Delete,Get,List}ContactCommand`, `ListContactListsCommand`, `UpdateContactListCommand` |
|  [07]   | tenant (multi-tenant SES) | `CreateTenantCommand`, `CreateTenantResourceAssociationCommand`, `ListTenantsCommand`, `ListTenantResourcesCommand`, `ListResourceTenantsCommand` |
|  [08]   | account         | `GetAccountCommand`, `PutAccountDetailsCommand`, `PutAccountSendingAttributesCommand`, `PutAccountSuppressionAttributesCommand`, `PutAccountVdmAttributesCommand`, `PutAccountDedicatedIpWarmupAttributesCommand` |
|  [09]   | export / import | `CreateExportJobCommand` / `CreateImportJobCommand` (+ `Get*`/`List*`)                            |

[ENTRYPOINT_SCOPE]: paginators (representative — `paginate*(config: SESv2PaginationConfiguration, input)` → `Paginator`; 19 total — 18 `List*` plus `paginateGetDedicatedIps`)
- rail: email / aws

| [INDEX] | [SURFACE]                                    | [CAPABILITY]                          |
| :-----: | :------------------------------------------- | :------------------------------------ |
|  [01]   | `paginateListEmailIdentities`                | async iterable of identity pages      |
|  [02]   | `paginateListConfigurationSets`              | async iterable of config-set pages    |
|  [03]   | `paginateListEmailTemplates`                 | async iterable of template pages      |
|  [04]   | `paginateListContacts` / `paginateListContactLists` | contact / contact-list pages   |
|  [05]   | `paginateListSuppressedDestinations`         | suppression-list pages                |
|  [06]   | `paginateListImportJobs` / `paginateListExportJobs` | import / export job pages       |
|  [07]   | `paginateGetDedicatedIps` / `paginateListDedicatedIpPools` | dedicated-IP pages / pools |
|  [08]   | `paginateListRecommendations`                | Virtual Deliverability Manager recommendation pages |
|  [09]   | `paginateListTenants` / `paginateListTenantResources` / `paginateListResourceTenants` | multi-tenant enumeration |
|  [10]   | `paginateList{CustomVerificationEmailTemplates,DeliverabilityTestReports,DomainDeliverabilityCampaigns,MultiRegionEndpoints,ReputationEntities}` | deliverability / endpoint / reputation pages |

## [04]-[IMPLEMENTATION_LAW]

[CLIENT_TOPOLOGY]:
- `SESv2Client` is the low-level dispatch surface; `SESv2` extends it with a method per command. All 111 commands follow `new XxxCommand(input)` + `client.send(cmd)`; import only the commands sent. `SESv2ClientConfig`: `region` (required), `credentials` (`AwsCredentialIdentityProvider`), `endpoint`, `maxAttempts`, `logger`, `requestHandler`. No waiters ship for SESv2.

[SEND_FAMILY]:
- `SendEmailRequest.Content` is an `EmailContent` union discriminated by which of `Simple`/`Raw`/`Template` is set — exactly one per request. `Simple` (a `Message`) carries `Subject` (`Content`) + `Body` (`Text` and/or `Html`) + optional `Attachments`; `Raw` carries a MIME `Data` blob; `Template` carries `TemplateName`/`TemplateArn` + `TemplateData`.
- `Destination` accepts ≤ 50 recipients across `ToAddresses`/`CcAddresses`/`BccAddresses`. Bulk send is `DefaultContent` + per-entry `BulkEmailEntry` (`Destination` + `ReplacementEmailContent` + `ReplacementTags`), each row yielding a `BulkEmailEntryResult` with a `BulkEmailStatus`.
- `ListManagementOptions` binds a `ContactListName`/`TopicName` for one-click unsubscribe — the send-side hook into the contact/tenant surface.

[PAGINATION_AND_ERROR_DISCIPLINE]:
- `paginate*` take `SESv2PaginationConfiguration` and return an `AsyncIterable`; `pageSize` maps to the command's `PageSize`, and command-level `NextToken` is managed internally — never set it manually under a paginator. A malformed token surfaces as `InvalidNextTokenException`.
- every service error extends `SESv2ServiceException`; `MessageRejected` and `MailFromDomainNotVerifiedException` are domain-logic failures needing bounce handling; `TooManyRequestsException` is throttling (back off via `maxAttempts`); `AccountSuspendedException`/`SendingPausedException` need account remediation, not retry.

[STACKING]:
- `nodemailer` transport: `nodemailer.createTransport({ SES: { sesClient: new SESv2Client(config), SendEmailCommand } })` builds an `SESTransport` (`nodemailer.md`), so a `nodemailer` `Mail` message renders and sends through SES; the raw `SendBulkEmailCommand`/`SendCustomVerificationEmailCommand`/templated `Template` sends are the operations `nodemailer` does not model, called directly.
- no Effect-native wrapper: unlike `@effect-aws/client-s3`, SESv2 has no admitted `@effect-aws/*` binding, so the notification owner in `persistence/work` bridges at an `Effect.tryPromise({ try: () => client.send(cmd), catch: … })` boundary, mapping `SESv2ServiceException` subclasses into the folder's typed mail-fault rail — this is the honest composition-root shape, not a `Service` tag.
- multi-tenant email: the tenant command/paginator family (`CreateTenant`, `ListTenantResources`) and `ListManagementOptions`/contact lists stack with the `persistence/tenancy` axis so a tenant's sending identity, suppression list, and contacts are scoped to `app.current_tenant`; credentials resolve through `security/secret` `SecretStore`, never a bare env read.

[RAIL_LAW]:
- package: `@aws-sdk/client-sesv2`
- owns: the typed SES API v2 client, the 111 command classes, paginators, and the model/enum/error types
- accept: `client.send(new XxxCommand(input))` for every operation; paginators for list operations; `EmailContent` as the Simple/Raw/Template discriminant; dispatch bridged at a `tryPromise` boundary or through `nodemailer`'s `SESTransport`
- reject: hand-rolled HTTP to SES endpoints; `@aws-sdk/client-ses` (the v1 API) in new code; a manual `NextToken` loop where a paginator applies; a phantom `@effect-aws/client-sesv2` import (no such admitted wrapper)
