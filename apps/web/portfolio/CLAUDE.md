# [PORTFOLIO]

Portfolio publishes architectural projects and studies as one server-rendered page with an owner-only editor

## [01]-[OWNERSHIP]

[ORDER]: Each feature takes the first available form: web platform API, then owning dependency member, then app code:
- `Intl` formats numbers and lists, CSS counters number list items, and History holds page location
- Views read dependency state through its hooks and events, hydration reads the dependency's server snapshot
- Editor, viewers, upload inspection, and PDF code load as lazy chunks at first need
- React Compiler memoizes every component, code holds no manual memoization

## [02]-[MODEL]

[SCHEMA]: Effect `Schema` under `model/` defines every value Worker, page, and editor exchange:
- Media kinds are `Schema.Class` members of `Asset` tagged by `kind`, accepted media types derive from member `mime` literals
- ALWAYS derive options and upload restrictions from schema `.literals` and `.members`, and fold assets through `Asset.match` and `Asset.guards`
- `AssetId`, `EntryId`, and `CompositionId` brand every id, a swapped id fails to compile
- Document fields a later build adds take `Schema.withDecodingDefaultKey`, bodies older builds stored still decode
- Stored forms are the `Schema.toEncoded` side of resolved schemas, with no second declaration
- `Placement` resolves stored asset ids through `Schema.decodeTo` and `SchemaGetter.transformEffect` against an `Assets` service
- Dangling or mistyped asset references fail decoding with `SchemaIssue.InvalidValue` and an owner-readable message
- Placement union cases hold exactly the fields their renderer reads
- Collection invariants are `Schema.makeFilter` checks on their owning struct, each issue at its path
- Publication-only rules are a `Schema.check` on publish payload, draft payloads skip them
- Schema checks run on encode, an invalid editor request fails before leaving the browser

[CONTRACT]: `HttpApiBuilder` serves and `AtomHttpApi` calls one `Api` over `StoredPortfolio`, editor encodes resolved `Portfolio` into each payload:
- Expected failures are `HttpApiError` variants and `Schema.TaggedError` classes with `httpApiStatus`, clients match `_tag`
- `Owner` is an `HttpApiMiddleware.Service` over every endpoint except asset `read`, accepting owner sessions and refusing a body-carrying request whose `Sec-Fetch-Site` is not `same-origin`
- Asset `read` types its path and failures in `Api` and answers with the ranged `HttpServerResponse` from `ObjectStorage.serve`, the rendered page alone is an `HttpRouter` route
- Multipart field names are `uploadField` in `model/api.ts`, editor and Worker read them, refused parts fail with an `UploadRefused` reason

## [03]-[WORKER]

[HANDLERS]: Worker is one Effect `Layer` graph, `serveWorker` builds its handler once per isolate on its first request:
- Operations are `Effect.fn` spans, `Effect.catchTag` turns SQL, schema, and platform failures into a logged `ServiceUnavailable`
- `SqlClient` stores complete encoded documents and assets as JSON, object storage holds media bytes
- Store reads decode joined JSON documents and asset collections through `PortfolioData` at store boundaries
- ALWAYS enforce storage invariants as conditions of writing SQL statements
- Placement references derive through one query over stored documents and the incoming draft
- Draft and published documents each store one JSON body and a revision
- `Revision` encodes draft revisions as strong ETags, a stale `If-Match` answers the stored revision, as success when the draft holds the body, else in `PreconditionFailed`
- Draft saves, their publish, and the stored revision read share one `atomic` batch, as do each release sweep and its object list
- Drafts placing a missing, mistyped, or out-of-range file fail with `PlacementConflict` naming each placement path
- `ObjectStorage.serve` evaluates HTTP preconditions and ranges for media reads, inline with `nosniff`, an original under its asset name
- Upload bodies send the asset field, then each rendition named by its width in asset order, then the original, each typed with its media type
- Uploads are idempotent per asset id, a repeat with equal metadata succeeds and differing metadata conflicts
- Asset JSON holds upload attempts and object keys, interrupted uploads and removals remain available for owner cleanup
- Asset reads serve assets placed in published work to anyone, every other asset to the owner alone and `NotFound` to visitors

## [04]-[PAGE]

[RENDER]: Worker and Node entry render `Site` into built `index.html` through `Template.render`, browsers hydrate the same `Site` through `hydrate`:
- Served markup holds published document and session alone, drafts reach a browser through owner endpoints
- Fragments name active project and selected composition, user selection pushes history and scrolling replaces it
- `popstate` and `hashchange` restore selection and scroll position from the fragment
- At page end, where the tracking line cannot reach last sections, scrolling keeps current fragment while its section stays in view
- Side index and phone select are each the `Work index` navigation at their width

[STYLE]: `site/theme.css` owns Tailwind tokens, shared utilities, and layout variants:
- Class names take theme tokens and spacing steps, a value one utility reads stays a declaration in that utility
- Presentation components name visual roles, typed `data-*` attributes select composition layouts
- Elements carry no `style` attribute, theme rules hold visual choices and the formulas over media values
- `CustomProperties` sets media values as custom properties, one hoisted `<style precedence>` rule per declaration that a `data-properties` token selects
- Figures reserve their box before media loads, server rendering emits each hoisted rule ahead of the markup it sizes
- React Aria state variants come from `tailwindcss-react-aria-components`
- `react-aria-components` owns composite controls, focus, selection, and overlays sized to its `--visual-viewport-height`
- Motion owns layout animation, in-view detection, and reduced-motion preference
- ALWAYS give each interaction a keyboard path, an accessible name or live announcement, a focus target, and a reduced-motion form

## [05]-[MEDIA]

[FIGURES]: `MediaFigure` renders every placement from its resolved asset, presentation decides loading, chrome, and expansion:
- `Match.discriminatorsExhaustive` over placement `kind` selects each renderer
- Display text and alt text derive once in `media/labels.ts`, asset URLs from `Api` through `assetUrl` in `model/api.ts`, and image source sets in `media/media-element.tsx`
- Presentation, priority, and in-view proximity decide media loading and video playback
- Image renditions fill `srcSet`, expanded views load the original
- Renditions share their source's authorization, publication, and removal
- Load failures offer an original file link, with a retry when a repeat can succeed
- `react-error-boundary` fallbacks catch chunk and render failures
- `react-zoom-pan-pinch` owns image zoom
- PDF.js (`pdfjs-dist`) owns PDF inspection, rendering, and sheet viewer
- `PdfDocument` shares one PDF.js document across every sheet of a PDF, opened for range requests (`disableStream`, `disableAutoFetch`)
- `PdfViewer.useViewer` renders the sheet preview and the sheet viewer, its `AsyncResult` failure renders the original-file link
- Sheet previews draw annotations without a text layer
- Sheet viewer writes page and scale through `PdfViewer` state `assign`, rotation through `rotate`, and find highlights through `find` when Find reopens
- Videos take no text track, owners burn captions in and write transcripts as descriptions

## [06]-[EDITOR]

[STATE]: Editor state is Effect `Atom` values over `AsyncResult` in `editor/state.ts`, with one writable draft atom every request atom updates:
- Mutations are `runtime.fn` atoms of an `AtomHttpApi.Service` runtime, their `AsyncResult` drives busy state, status, and alerts
- Draft, mutation, and upload atoms are `Atom.keepAlive`, closing the editor keeps edits, results, and queued files
- Draft edits are `Optic` modifications returning a new portfolio, identity against the last saved or loaded snapshot marks unsaved work
- Save failures hold their submitted snapshot, schema issue paths select the failing entry in it
- Precondition failures keep local edits and offer a confirmed reload or a save over the stored revision
- Pending uploads block publish and reload
- ALWAYS map typed failures to owner text in the view, stating what was kept and which step comes next
- ALWAYS state in owner copy each consequence code cannot enforce (whole PDF public, captions burned in, tab open during uploads)

[UPLOADS]: Uppy owns the upload queue from one instance in an `Atom.keepAlive` atom with a finalizer:
- Preprocessing measures each file and renders image renditions through `Inspection`, then decodes an `Asset` before upload
- `preprocessUploads` runs in a `SecureRandom` runtime atom the uploads view mounts, its scope removes the preprocessor
- Preprocessing failures name their remedy: an unlocked export for a protected PDF, a new export for a refused file, a retry for a failed load
- Each file's destination stays editable until its processing starts, PDF sheets are chosen after upload
- Success places compositions at their destination in queue order under concurrent uploads
- Upload recovery lists incomplete uploads and removals, each with one discard action the owner confirms
- Uppy retries a request failing without a status or with a 5xx, an `UploadRefused` body fails it with its reason
- Until Dashboard renders one Informer, the view passes `disableInformer` and renders Uppy `info` state as one alert per message

## [07]-[HOSTING]

[SITES]: OpenAI Sites runs the built Worker with D1 and R2 bindings `.openai/hosting.json` names, sync generator `hosting-bindings` writes both names and the host assigns `project_id`:
- `cloudflare.config.ts` declares the Worker name, entrypoint, and data bindings through `hostedWorker`
- Worker binding types are the global `Env` that `cf workers types` generates from `cloudflare.config.ts`
- `server/worker.ts` passes `cloudflare:workers` `env` and its `Layer` to `serveWorker`, `server/main.ts` passes `cloudflare.config.ts` and an `env` callback to `serveNode`, each host providing `Template` from the assets binding
- `server/app.ts` composes routes over passed services, D1 and `ObjectStorage.r2` from the Worker, SQLite and path-style `ObjectStorage.s3` from Node
- `nx run @rasm/portfolio:infra [-- docker]` prints the stack difference and `--apply` builds and deploys to a Cloudflare account or a Docker host
- Untagged `HEAD` takes Cloudflare mode `disposable` with its own Worker and resources, and Docker mode `production` under a `-disposable` name
- Sites, `CloudflareAccess.middleware`, or Docker proxy send user id and email as `User.sites` headers
- `hosting` takes `favicon.svg` and `site/theme.css` tokens, `siteHead` emitting the ICO, Apple touch icon, painted SVG, `theme-color`, and the `color-scheme` the tokens hold
- Requests without a user id are signed-out visitors
- First sign-in with an email equal to `OWNER_EMAIL` ignoring case enrolls its user id, later owner checks read and compare user ids alone
- `cloudflare.config.ts` states the owner address as `hostedWorker`'s `owner`, the commit author address and no secret, modes `production`, `cloudflare`, and `disposable` bind it to `OWNER_EMAIL` as text, every other mode binds the `sites()` local sign-in address, `cloudflare` and `disposable` also declare the `CloudflareAccess.binding` JSON secret
- `site/shell.tsx` states the page's content security sources beside the policy `Template.render` holds

[STORAGE]: `server/catalog.ts` declares document, asset, and owner tables as `Relation` values, with statements over them beside their handlers in `server/<aggregate>.ts`:
- `Catalog.layer` builds each entry's client with snake-case identifiers and reconciles tables once per isolate or process start
- Queries name a multi-word column through `sql('objectKey')` or a `Relation` static
- With older and newer builds on one database, queries name every column and new required columns carry a default
- Asset availability is the VIRTUAL column `available` over `objectKey` and `removing`
