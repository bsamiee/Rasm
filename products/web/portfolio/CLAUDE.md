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
- `Owner` is an `HttpApiMiddleware.Service` accepting owner sessions over every endpoint except asset `read`
- `Owner` refuses a request with a body when its `Sec-Fetch-Site` is not `same-origin`
- Asset `read` types its path and failures in `Api` and answers with the ranged `HttpServerResponse` from `ObjectStorage.serve`
- Rendered page is the only `HttpRouter` route
- Asset `begin` records an upload attempt and its object keys, `sign` presigns each storage request for a key of an open attempt, `complete` checks every object and publishes the asset

## [03]-[WORKER]

[HANDLERS]: Worker is one Effect `Layer` graph, `serve` of `@rasm/hosting/server` under the `workerd` condition builds it once per isolate on its first invocation:
- Operations are `Effect.fn` spans, `Effect.catchTag` turns SQL, schema, and platform failures into a logged `ServiceUnavailable`
- `SqlClient` stores complete encoded documents and assets as JSON, object storage holds media bytes
- Store reads decode joined JSON documents and asset collections through `PortfolioData` at store boundaries
- ALWAYS enforce storage invariants as conditions of writing SQL statements
- Placement references derive through one query over stored documents and the incoming draft
- Draft and published documents each store one JSON body and a revision
- `Revision` encodes draft revisions as strong ETags
- Stale `If-Match` answers the stored revision, as success when the draft holds the body, else in `PreconditionFailed`
- Draft saves, their publish, and the stored revision read share one `atomic` batch, as do each release sweep and its object list
- Drafts placing a missing, mistyped, or out-of-range file fail with `PlacementConflict` naming each placement path
- `ObjectStorage.serve` evaluates HTTP preconditions and ranges for media reads, inline with `nosniff`, an original under its asset name
- Browsers write media bytes straight to the bucket through `ObjectStorage.sign`, which states each object's type and size and completes a key once through `If-None-Match`
- Uploads are idempotent per asset id, a repeat with equal metadata succeeds and differing metadata conflicts
- Asset JSON holds upload attempts and object keys, interrupted uploads and removals remain available for owner cleanup
- Asset reads serve assets placed in published work to anyone with a one-day browser cache lifetime
- Asset reads serve every other asset to the owner alone and `NotFound` to visitors

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
- `CustomProperties` sets media values as custom properties, one hoisted `<style precedence>` rule per declaration a `data-properties` token selects
- Figures reserve their box before media loads, server rendering emits each hoisted rule ahead of the markup it sizes
- React Aria state variants come from `tailwindcss-react-aria-components`
- `react-aria-components` owns composite controls, focus, selection, and overlays sized to its `--visual-viewport-height`
- Motion owns layout animation, in-view detection, and reduced-motion preference
- ALWAYS give each interaction a keyboard path, an accessible name or live announcement, a focus target, and a reduced-motion form

## [05]-[MEDIA]

[FIGURES]: `MediaFigure` renders every placement from its resolved asset, presentation decides loading, chrome, and expansion:
- `Match.discriminatorsExhaustive` over placement `kind` selects each renderer
- Display text and alt text derive once in `media/labels.ts`, image source sets through `Inspection.sourceSet`
- Asset URLs derive from `Api` through `assetUrl` in `model/api.ts`
- Presentation, priority, and in-view proximity decide media loading and video playback
- Image renditions fill `srcSet`, expanded views load the original
- Renditions share their source's authorization, publication, and removal
- Load failures offer an original file link, with a retry when a repeat can succeed
- `react-error-boundary` fallbacks catch chunk and render failures
- `ZoomViewer` of `@rasm/media/zoom-viewer` owns image zoom
- PDF.js (`pdfjs-dist`) owns PDF inspection, rendering, and sheet viewer
- `PdfDocument` shares one PDF.js document across every sheet of a PDF, opened for range requests (`disableStream`, `disableAutoFetch`)
- `PdfViewer.useViewer` renders the sheet preview and the sheet viewer, its `AsyncResult` failure renders the original-file link
- Sheet previews draw annotations without a text layer
- Sheet viewer renders `PdfViewer.useViewer` controls, passing sheet labels and the original file URL
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
- `processUploads` runs in the `PortfolioClient` runtime atom the uploads view mounts: preprocessing begins the attempt and puts each rendition, postprocessing completes it, and its scope removes both processors
- Preprocessing failures name their remedy: an unlocked export for a protected PDF, a new export for a refused file, a retry for a failed load
- Each file's destination stays editable until its processing starts, PDF sheets are chosen after upload
- Success places compositions at their destination in queue order under concurrent uploads
- Upload recovery lists incomplete uploads and removals, each with one discard action the owner confirms
- `@uppy/aws-s3` uploads each original as a multipart upload under its attempt key, an `UploadRefused` from `sign` fails it with its reason
- Until Dashboard renders one Informer, the view passes `disableInformer` and renders Uppy `info` state as one alert per message

## [07]-[HOSTING]

[DECLARATION]: `hosting.config.ts` declares the app once as an `App`, and every runtime, projection, and deployment reads it:
- `DB` is a `sqlite` store and `BUCKET` a `bucket` with its lifecycle rules and the CORS rule direct uploads need
- `allowed` holds the addresses a sign-in accepts, `limits` the CPU count and memory of a container
- `targets` holds `Cloudflare` alone
- `cloudflare({ config: worker(hosting, config) })` in `vite.config.ts` projects the `App`, adding the assets binding and `MODE`
- `AWS_*` text bindings sign against miniflare route `CorePaths.R2_S3`, and deploy replaces them with an issued R2 token
- Worker and Node load the same `server/main.ts`, which imports `hosting.config.ts` directly
- `tsconfig.json` references one program per runtime: `tsconfig.app.json` browser code, `tsconfig.node.json` tool configs
- `tsconfig.worker.json` checks `server/main.ts` under the `workerd` condition with the server-rendered views it reaches
- `nx install @rasm/portfolio` applies stack `portfolio-disposable.local.rasm.localhost` on the local engine
- `nx run @rasm/portfolio:deploy--pulumi --stack=portfolio-disposable.cloudflare` applies one stack, `-c release` the production stack `portfolio.cloudflare`, and no `--stack` every stack
- Stack `<app>.cloudflare` deploys the built Worker with every binding its Build Output states
- Stack `<app>.cloudflare` creates each D1 database and R2 bucket a binding names, bucket lifecycle rules, and an Access application on the sign-in path
- `--operation=destroy` on a disposable stack empties its bucket and removes every resource, reading the Build Output `build--vite` wrote

[SERVERS]: `server/main.ts` passes the `App` and its routes to `serve` of `@rasm/hosting/server`, whose `workerd` condition serves the Worker and default condition the Node bundle:
- Routes receive each binding by name: `DB` a database engine layer over D1 or Node SQLite, `BUCKET` an `ObjectStorage` layer over the R2 binding or S3
- `ObjectStorage.sign` presigns through the `AWS_*` variables, resolving the endpoint against the served request and `S3_PUBLIC_ENDPOINT_URL` where browsers reach the bucket elsewhere
- Workers write one JSON log record per console line, record each span in the Workers trace, and export at invocation end
- Node servers pass the app name to `layerNodeFromConfig`, which reads `<NAME>_LOG_{LEVEL,FILTER,TARGET,DIR,SIZE,AGE,FILES}`
- Each server exports each signal its `OTEL_<SIGNAL>_EXPORTER` variable names
- `.storybook/main.ts` sets `viteConfigPath: null` and states the React and Tailwind plugins its stories read, so no `vite.config.ts` plugin runs
- `siteHead` takes `favicon.svg` and `site/theme.css` tokens, emitting the ICO, Apple touch icon, painted SVG, `theme-color`, and `color-scheme`
- `site/shell.tsx` states the content security policy of a production build, the page adds the storage origin to `connect-src`, and `Template.render` sets no header without one

[IDENTITY]: Views and handlers read the signed-in user of each request through `Identity` of `@rasm/hosting/identity`, which owns the sign-in and sign-out paths:
- `ACCESS` holds one `Access` as JSON, source `Forwarded` reading headers a proxy sets
- `Access` source `Cloudflare` verifies a Cloudflare Access token from its header or `CF_Authorization` cookie
- `vite dev` with no `ACCESS` serves a development sign-in page, and a production build with no `ACCESS` signs every request out
- Pages are public, the access source guards `Identity.paths.signin`, and owner endpoints refuse every other session
- `Enrollment.session` answers `signedOut`, `signedIn`, or `allowed`, enrolling the first user id an `allowed` address signs in with
- `ALLOWED` replaces `allowed` with a JSON list, and `nx install` sets it to the owner address of the local sign-in service
- `hosting.config.ts` states the owner address, the commit author address and no secret

[STORAGE]: `server/catalog.ts` declares document, asset, and owner tables as `Relation` values:
- Statements over the tables sit beside their handlers in `server/<aggregate>.ts`
- `Catalog.layer` builds each entry's client with snake-case identifiers and reconciles tables once per isolate or process start
- Queries name a multi-word column through `sql('objectKey')` or a `Relation` static
- With older and newer builds on one database, queries name every column and new required columns carry a default
- Asset availability is the VIRTUAL column `available` over `objectKey` and `removing`
