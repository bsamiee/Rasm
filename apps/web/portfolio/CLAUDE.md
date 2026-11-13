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

[HANDLERS]: Worker is one Effect `Layer` graph, `Worker` of `@rasm/workers` builds it once per isolate on its first invocation:
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
- Asset reads serve assets placed in published work to anyone with a one-day browser cache lifetime, every other asset to the owner alone and `NotFound` to visitors

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

[DECLARATION]: `hosting.config.ts` declares the app once as an `App`: `DB` a `sqlite` store, `BUCKET` a `bucket` with its lifecycle rules, `OWNER_EMAIL` a `text` value, `allowed` the addresses a provider sign-in accepts, and `limits` the CPU count and memory of the hosted container:
- `cloudflare.config.ts` projects it through `worker(hosting, config)`, and the hosting row names each deployed Worker and resource by its mode
- `server/main.ts` passes it to `serveNode`, which reads each store variable from the environment and each `text` value from the declaration
- Worker binding types are the global `Env` that `cf workers types` generates from `cloudflare.config.ts`
- `tsconfig.json` references one program per runtime: `tsconfig.app.json` browser code, `tsconfig.node.json` Node entry and tool configs, `tsconfig.worker.json` Worker entry with `cloudflare.config.ts` and `.cloudflare/types`
- `.openai/hosting.json` names the D1 and R2 bindings OpenAI Sites supplies, written by hand, and the host assigns `project_id`
- `nx install @rasm/portfolio` applies the app on the local engine, `nx run @rasm/portfolio:deploy--pulumi --stack=<row>` on one hosting row
- `-c release --stack=<row>` applies the production stack of that row, and no `--stack` applies every stack that exists
- Row `cloudflare` deploys the built Worker with every binding its Build Output states, creating each D1 database and R2 bucket a binding names, bucket lifecycle rules, and an Access application on the sign-in path
- `--destroy` on a disposable row empties its bucket and removes every resource, reading the Build Output `build--vite` wrote

[SERVERS]: `server/app.ts` composes routes over passed services, D1 and `r2Storage` from the Worker, SQLite and path-style `s3Storage` from Node:
- `server/worker.ts` passes its services and `Worker.fetch` over its routes to `Worker`, with `Template` from the assets binding and `layerTelemetry` of `@rasm/workers/execution`
- Worker `layerTelemetry` writes one JSON log record per console line, records each span in the Workers trace, and exports each signal its `OTEL_<SIGNAL>_EXPORTER` variable names at the end of each invocation
- `server/main.ts` passes `Sites.identity` and an `env` callback to `serveNode`, then the HTTP server on `PORT` and the app name as `serviceName` to `layerNodeFromConfig` of `@rasm/telemetry/node` with target `stderr`
- `layerNodeFromConfig` reads `<NAME>_LOG_{LEVEL,FILTER,TARGET,DIR,SIZE,AGE,FILES}`, writes one JSON log record per line to stderr, and exports each signal its `OTEL_<SIGNAL>_EXPORTER` variable names through the OpenTelemetry SDK
- `.storybook/main.ts` sets `viteConfigPath: null` and states the React and Tailwind plugins its stories read, so no plugin of `vite.config.ts` runs in Storybook
- `siteHead` takes `favicon.svg` and `site/theme.css` tokens, emitting the ICO, Apple touch icon, painted SVG, `theme-color`, and `color-scheme`
- `site/shell.tsx` states the whole content security policy of a production build, and `Template.render` sets no header without one

[IDENTITY]: Views and handlers read `Sites.identity` headers, which each server sets from one trusted source after removing any a client sent:
- OpenAI Sites signs users in at its own boundary, a Worker with no `ACCESS` variable serving its headers unchanged
- `ACCESS` holds one `Access` as JSON, source `Cloudflare` verifying a Cloudflare Access token from its header or `CF_Authorization` cookie and source `Forwarded` reading headers a proxy sets
- `serveNode` fails at start without `ACCESS`
- Pages are public, the trusted source guards the sign-in path of `Sites.identity.paths`, and owner endpoints refuse every other session
- Requests without a user id are signed-out visitors
- First sign-in with an email equal to `OWNER_EMAIL` ignoring case enrolls its user id, later owner checks read and compare user ids alone
- `hosting.config.ts` states the owner address, the commit author address and no secret, and `vite dev` binds the `sites()` local sign-in address

[STORAGE]: `server/catalog.ts` declares document, asset, and owner tables as `Relation` values, with statements over them beside their handlers in `server/<aggregate>.ts`:
- `Catalog.layer` builds each entry's client with snake-case identifiers and reconciles tables once per isolate or process start
- Queries name a multi-word column through `sql('objectKey')` or a `Relation` static
- With older and newer builds on one database, queries name every column and new required columns carry a default
- Asset availability is the VIRTUAL column `available` over `objectKey` and `removing`
