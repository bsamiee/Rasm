# [PORTFOLIO]

Portfolio publishes architectural projects and studies as one server-rendered page with an owner-only editor

## [01]-[OWNERSHIP]

[ORDER]: Each feature takes the first available form: web platform API, then owning dependency member, then app code
- `Intl` formats numbers and lists, CSS counters number list items, and History holds page location
- Views read dependency state through its hooks and events, hydration reads the dependency's server snapshot
- Editor, viewers, upload inspection, and PDF code load as lazy chunks at first need
- React Compiler memoizes every component, code holds no manual memoization

## [02]-[MODEL]

[SCHEMA]: Effect `Schema` under `model/` defines every value Worker, page, and editor exchange
- Media kinds are `Schema.Class` members of `Asset`, accepted media types derive from member `mime` literals
- ALWAYS derive options, upload restrictions, and `Match.is` arms from schema `.literals` and `.members`
- Stored forms are the `Schema.toEncoded` side of resolved schemas, with no second declaration
- `Placement` resolves stored asset ids through `Schema.decodeTo` and `SchemaGetter.transformEffect` against an `Assets` service
- Dangling or mistyped asset references fail decoding with `SchemaIssue.InvalidValue` and an owner-readable message
- Placement union cases hold exactly the fields their renderer reads
- Collection invariants are `Schema.makeFilter` checks on their owning struct, each issue at its path
- Publication-only rules are a `Schema.check` on publish payload, draft payloads skip them
- Schema checks run on encode, an invalid editor request fails before leaving the browser

[CONTRACT]: `Api` declares one `HttpApi` over `StoredPortfolio` that `HttpApiBuilder` serves and `AtomHttpApi` calls, the editor encodes its resolved `Portfolio` into each payload
- Expected failures are `HttpApiError` variants and `Schema.TaggedError` classes with `httpApiStatus`, clients match `_tag`
- `Owner` is an `HttpApiMiddleware.Service` over every endpoint, accepting owner sessions and rejecting cross-origin writes
- Answers a contract cannot type (rendered page, ranged media bytes) are `HttpRouter` routes beside API groups

## [03]-[WORKER]

[HANDLERS]: Worker is one Effect `Layer` graph that `HttpRouter.toWebHandler` turns into a fetch handler
- Operations are `Effect.fn` spans, `Effect.catchTag` turns SQL, schema, and platform failures into a logged `ServiceUnavailable`
- `SqlClient` stores complete encoded documents and assets as JSON (D1 in the Worker, SQLite under Node), object storage holds media bytes
- `SqlSchema` decodes joined JSON documents and asset collections through `PortfolioData` at store boundaries
- ALWAYS enforce storage invariants as conditions of writing SQL statements
- Placement references derive from stored JSON through one query
- Draft and published documents each store one JSON body and a revision
- `Revision` encodes the draft revision as a strong ETag, a write whose `If-Match` revision is stale fails with `PreconditionFailed`
- Draft and publication writes share one `atomic` batch: the draft upsert compares and sets the revision on available asset references and returns the revision it wrote, publication copies that draft
- `partial-content` evaluates HTTP preconditions and ranges for media reads
- Uploads are idempotent per asset id, a repeat with equal metadata succeeds and differing metadata conflicts
- Asset JSON holds upload attempts and object keys, interrupted uploads and removals remain available for owner cleanup
- Media reads serve assets placed in published work to anyone, every other asset to the owner alone

## [04]-[PAGE]

[RENDER]: Worker and Node entry render `Site` into built `index.html` through `@rasm/hosting` `renderPage`, browsers hydrate the same `Site`
- Served markup holds published document and session alone, drafts reach a browser through owner endpoints
- `shell` names the root, data script, and `Bootstrap` schema `@rasm/hosting` renders in Worker and hydrates in browser
- Fragments name active project and selected composition, user selection pushes history and scrolling replaces it
- `popstate` and `hashchange` restore selection and scroll position from the fragment
- Scrolling at the page end keeps the fragment's entry while its section is in view, the tracking line cannot reach the last sections there
- Side index and phone select are each the `Work index` navigation at their width

[STYLE]: `site/theme.css` owns Tailwind tokens, shared utilities, and layout variants
- Class names take theme tokens and spacing steps, a value one utility reads stays a declaration in that utility
- Presentation components name visual roles, typed `data-*` attributes select composition layouts
- Elements carry no `style` attribute, theme rules hold visual choices and the formulas over media values
- Media values reach CSS as custom properties through `@rasm/ui` `CustomProperties`, a hoisted `<style precedence>` rule its `data-properties` attribute selects
- Figures reserve their box before media loads, server rendering emits each hoisted rule ahead of the markup it sizes
- React Aria state variants come from `tailwindcss-react-aria-components`
- `react-aria-components` owns composite controls, focus, selection, and overlays sized to its `--visual-viewport-height`
- Motion owns layout animation, in-view detection, and reduced-motion preference
- ALWAYS give each interaction a keyboard path, an accessible name or live announcement, a focus target, and a reduced-motion form

## [05]-[MEDIA]

[FIGURES]: `MediaFigure` renders every placement from its resolved asset, presentation decides loading, chrome, and expansion
- `Match.discriminatorsExhaustive` over placement `kind` selects each renderer
- Labels, alt text, media URLs, and image source sets derive once in `media/display.ts`
- Presentation, priority, and in-view proximity decide media loading and video playback
- Image renditions fill `srcSet`, expanded views load the original
- Renditions share their source's authorization, publication, and removal
- Load failures offer an original file link, with a retry when a repeat can succeed
- `react-error-boundary` fallbacks catch chunk and render failures
- `react-zoom-pan-pinch` owns image zoom
- PDF.js (`pdfjs-dist`) owns PDF inspection, rendering, and sheet viewer
- `@rasm/media` `pdfDocument` shares one PDF.js document across every sheet of a PDF
- Videos take no text track, owners burn captions in and write transcripts as descriptions

## [06]-[EDITOR]

[STATE]: Editor state is Effect `Atom` values over `AsyncResult`, with one writable draft atom every request atom updates
- Mutations are `runtime.fn` atoms of an `AtomHttpApi.Service` runtime, their `AsyncResult` drives busy state, status, and alerts
- Draft, mutation, and upload atoms are `Atom.keepAlive`, closing the editor keeps edits, results, and queued files
- Draft edits are `Optic` modifications returning a new portfolio, identity against the last saved or loaded snapshot marks unsaved work
- Save failures hold their submitted snapshot, schema issue paths select the failing entry in it
- Precondition failures keep local edits and offer a confirmed reload
- Pending uploads block publish and reload
- ALWAYS map typed failures to owner text in the view, stating what was kept and which step comes next
- ALWAYS state in owner copy each consequence code cannot enforce (whole PDF public, captions burned in, tab open during uploads)

[UPLOADS]: Uppy owns the upload queue from one instance in an `Atom.keepAlive` atom with a finalizer
- Preprocessing measures each file in the browser, renders image renditions, and decodes an `Asset` before upload
- Each file's destination stays editable until its processing starts, PDF sheets are chosen after upload
- Success places compositions at their destination in queue order under concurrent uploads
- Upload recovery lists incomplete uploads and removals, each with one discard action the owner confirms
- Uppy retries a failed request three times before the queue shows a failure and its retry action
- Dashboard 6.0.2 renders its Informer twice, so the Dashboard takes `disableInformer` and the view renders Uppy `info` state as one alert per message

## [07]-[HOSTING]

[SITES]: OpenAI Sites runs the built Worker with D1 and R2 bindings `.openai/hosting.json` names
- `cloudflare.config.ts` declares the Worker, its asset routing, and its bindings through `cf/config` builders
- Worker binding types are the global `Env` that `cf workers types` generates from `cloudflare.config.ts`
- Worker `fetch` supplies bindings to memoized handler construction inside request context
- `worker/app.ts` composes the routes both entries serve, `worker/index.ts` provides D1 and `r2` storage, `worker/node.ts` provides SQLite and `s3` storage
- `nx run rasm:hosting -- apps/web/portfolio <cloudflare|docker> <operation>` deploys the same build to a Cloudflare account or a Docker host
- Signed-in user id and email arrive as the `@rasm/hosting/sites.ts` `contract` headers from Sites, Cloudflare Access through `@rasm/hosting/worker.ts`, or the Docker target's proxy, requests without an id are signed-out visitors
- First sign-in with an email equal to `OWNER_EMAIL` ignoring case enrolls its user id, later owner checks compare user ids alone
- Hosted configuration supplies the `OWNER_EMAIL` secret, every mode other than `production` binds it to the `sites()` local sign-in email
- `sites()` copies `.openai/hosting.json` into project-local `dist` and exposes no relocation setting

[STORAGE]: `worker/database.ts` declares the document, asset, and owner tables as `@rasm/database` tables beside the statements over them
- `@rasm/database` `store` builds each entry's client with the library's column naming and reconciles the tables once per isolate or process start, adding what they declare and keeping every undeclared column and table
- Queries reach a multi-word table column through that naming (`sql.insert`, `sql('userId')`, library `select`)
- Columns and tables leave only through a declared `retired` entry, and change type only through a declared `retype` entry
- Queries name their columns and new required columns carry a default, so an older or newer build keeps reading and writing
- Effect schemas define JSON content, each document or asset updates in place
- Migration files, schema history, and request-time table creation are absent
