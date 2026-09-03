# [HOSTINGER_REACH]

Email marketing work from sender authentication to segment audiences. REST entries on `/api/reach/v1` map one-to-one onto the `hostinger` MCP `reach_*` tools, every contact, profile, and segment identifier is a UUID.

## [01]-[PROFILES_AND_DELIVERABILITY]

Sender profiles own a domain, and `getProfileDomainDNSStatusV1` is the deliverability gate: it returns the `mx`, `spf`, `dkim`, and `dmarc` record blocks, each pairing the `actual` records against the `suggested` set, a mismatch names the exact DNS record to add before a profile is trusted for sending. Profiles have plan-derived `limits` (sending quota, trial state).

```bash
curl -X GET "https://developers.hostinger.com/api/reach/v1/profiles" -H "Authorization: Bearer $HOSTINGER_API_TOKEN"
curl -X GET "https://developers.hostinger.com/api/reach/v1/profiles/{profileUuid}/domains/dns-status" -H "Authorization: Bearer $HOSTINGER_API_TOKEN"
```

## [02]-[CONTACTS]

Contacts take `email` (required) with nullable `name`, `surname`, `phone`, and `note`. Create paths differ by scope: `POST /contacts` is account-wide, and `POST /profiles/{profileUuid}/contacts` binds the contact to one sender profile, the scoped form when a contact must belong to a single profile. Under double opt-in a new contact lands `pending` and receives no campaigns until it confirms, deletion is permanent.

```bash
curl -X POST "https://developers.hostinger.com/api/reach/v1/contacts" \
  -H "Authorization: Bearer $HOSTINGER_API_TOKEN" -H "Content-Type: application/json" \
  -d '{ "email": "subscriber@example.com", "name": "Ada", "surname": "Lovelace" }'
```

Create responses hold a message only, a follow-up `GET /contacts` reads the assigned UUID and `subscription_status`. Segments supersede contact groups, and the `group_uuid` filter and `listContactGroupsV1` remain for old data.

## [03]-[SEGMENTS]

Segments are named rules of `logic` (`and` or `or`) over a `conditions[]` array, each condition a `{ field, operator, value }` triple. Behavioral operators (`opened`, `clicked`, `bounced`, `unsubscribed`, `within_last_days` and their negations, beside the scalar `equals`, `contains`, `gte`, `lte`, `exists`) turn engagement history into an audience.

```bash
curl -X POST "https://developers.hostinger.com/api/reach/v1/segmentation/segments" \
  -H "Authorization: Bearer $HOSTINGER_API_TOKEN" -H "Content-Type: application/json" \
  -d '{ "name": "Engaged Subscribers", "logic": "and",
        "conditions": [ { "field": "subscription_status", "operator": "equals", "value": "active" },
                        { "field": "email_engagement", "operator": "opened" } ] }'
```

Segment audiences read through `listSegmentContactsV1` (`GET .../segments/{segmentUuid}/contacts`) or its profile-scoped variant, paginated by `page` and `per_page` (default 25), account-wide contact listing paginates by `page` alone.

## [04]-[API_REFERENCE]

| [INDEX] | [METHOD]          | [ENDPOINT]                                            | [DESCRIPTION]                     |
| :-----: | :---------------- | :---------------------------------------------------- | :-------------------------------- |
|  [01]   | `GET`             | `/api/reach/v1/profiles[/{uuid}/domains/dns-status]`  | Profiles, sender-domain DNS gate  |
|  [02]   | `GET/POST/DELETE` | `/api/reach/v1/contacts[/{uuid}]`                     | List, create account-wide, delete |
|  [03]   | `POST`            | `/api/reach/v1/profiles/{uuid}/contacts`              | Create scoped to a sender profile |
|  [04]   | `GET`             | `/api/reach/v1/contacts/groups`                       | Legacy contact groups             |
|  [05]   | `GET/POST`        | `/api/reach/v1/segmentation/segments[/{uuid}]`        | List, create, read a segment      |
|  [06]   | `GET`             | `/api/reach/v1/segmentation/segments/{uuid}/contacts` | Segment audience, paginated       |
