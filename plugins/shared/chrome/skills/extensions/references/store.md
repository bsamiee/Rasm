# [STORE]

## [01]-[POLICIES]

Chrome Web Store review reads a package against its program policies:
- Extensions serve one narrow purpose
- Manifests request the narrowest permission set that works, optional ones included
- Every script and Wasm module an extension runs sits in its package, and JSON, CSS, images, and configuration can load from outside it
- Obfuscated code fails review, and minified code passes
- Files a manifest names exist in its package under matching letter case
- Affiliate links and ads are disclosed in listing and UI, each tied to a user action
- Keyword lists in a description and a missing icon, screenshot, or description fail review

User data rules reach further than data that leaves a device:
- User data includes web browsing activity (each URL or domain a browser visits) and website content
- Handling means collecting, transmitting, using, or sharing, and data processed or stored on a device counts
- Disclosures match what code does, and a mismatch is itself a violation
- Limited Use holds data to an extension's single purpose
- Extensions that read no user data declare none and need no policy

## [02]-[REVIEW]

Chrome Web Store reviews each upload before it reaches users:
- Review takes days for most items and weeks at its longest
- New publishers, broad host patterns, `tabs`, `cookies`, `downloads`, `webRequest`, and large minified code lengthen it
- Uploads run install tests before submission
- Deferred publishing (`publishType: STAGED_PUBLISH`) holds an approved item until a later `:publish`, and returns it to draft for a new review after 30 days
- Staged rollout takes a percentage that only rises, for items with more than 10,000 weekly users
- Rollback restores an earlier published version under a new version number with no review
- Updates changing only safe static rules of `declarativeNetRequest` rulesets can skip review
- Publishers cancel a pending review up to 6 times a day and appeal a rejection once per violation from the dashboard

## [03]-[API]

Chrome Web Store API v2 at `https://chromewebstore.googleapis.com` acts on items a dashboard created:

| [INDEX] | [CALL]                                                       | [DOES]                                                   |
| :-----: | :----------------------------------------------------------- | :------------------------------------------------------- |
|  [01]   | `POST /upload/v2/publishers/<publisher>/items/<item>:upload` | Takes a zip or CRX with a higher `version`               |
|  [02]   | `GET /v2/publishers/<publisher>/items/<item>:fetchStatus`    | Reads upload state and each revision's state and version |
|  [03]   | `POST ...:publish`                                           | Submits for review, with `publishType` and `deployInfos` |
|  [04]   | `POST ...:setPublishedDeployPercentage`                      | Raises a staged rollout                                  |
|  [05]   | `POST ...:cancelSubmission`                                  | Cancels a pending review                                 |
|  [06]   | `POST ...:rollback`                                          | Restores earlier published version                       |

Calls and tokens follow dashboard state:
- API calls create no item and change no visibility, and a first version and each visibility change go through the dashboard
- Uploads report `IN_PROGRESS`, `SUCCEEDED`, or `FAILED` in `lastAsyncUploadState`
- Revisions hold state `PENDING_REVIEW`, `STAGED`, `PUBLISHED`, `PUBLISHED_TO_TESTERS`, `REJECTED`, or `CANCELLED`
- Tokens take scope `https://www.googleapis.com/auth/chromewebstore`, from a service account its publisher added in the dashboard

## [04]-[LISTING]

Dashboard tabs take one value per field:

| [INDEX] | [FIELD]            | [VALUE]                                                              |
| :-----: | :----------------- | :------------------------------------------------------------------- |
|  [01]   | Description        | Plain text stating behavior                                          |
|  [02]   | Category           | One dashboard category (Tools, Developer Tools, Workflow & Planning) |
|  [03]   | Icon               | 128px PNG with 96px of drawing inside 16px padding                   |
|  [04]   | Screenshots        | 1 to 5 at 1280 by 800px or 640 by 400px                              |
|  [05]   | Small promo tile   | 440 by 280px PNG or JPEG, ranking a listing ahead of ones without    |
|  [06]   | Marquee promo tile | 1400 by 560px PNG or JPEG, for store marquee                         |
|  [07]   | Single purpose     | One sentence naming its narrow purpose                               |
|  [08]   | Permission reasons | One sentence per manifest permission naming its feature              |
|  [09]   | Remote code        | Whether any code loads from outside its package                      |
|  [10]   | Data usage         | Each data type code handles, and each Limited Use certification      |
|  [11]   | Privacy policy     | URL, for any extension that handles user data                        |
|  [12]   | Visibility         | Public, unlisted, or private to testers, a group, or an organization |

Accounts publish under fixed terms:
- Registration costs a one-time fee and needs 2-Step Verification
- Listings show a verified contact email and a trader or non-trader declaration
- Publishers hold a limited count of items until they request more
- Members join a publisher as viewer, item manager, editor, or admin
- Verified uploads bind an item to a signing key, after which each upload is a CRX signed with it
