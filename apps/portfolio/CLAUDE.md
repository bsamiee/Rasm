# [PORTFOLIO]

Portfolio publishes architectural projects and studies as one server-rendered page with an owner-only editor

## [01]-[OWNERSHIP]

[ORDER]: Each feature takes the first available form: web platform API, then owning dependency member, then app code
- `Intl` formats numbers and lists, CSS counters number list items, and History holds page location
- Views read dependency state through its hooks and events, hydration reads the dependency's server snapshot
- ALWAYS fill a dependency gap through an extension point the dependency exposes
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

[CONTRACT]: `makeApi` declares one `HttpApi` over a portfolio schema, `HttpApiBuilder` serving its stored form and `AtomHttpApi` its resolved form
- Expected failures are `HttpApiError` variants and `Schema.TaggedError` classes with `httpApiStatus`, clients match `_tag`
- `Owner` is an `HttpApiMiddleware.Service` over every endpoint, accepting owner sessions and rejecting cross-origin writes
- Answers a contract cannot type (rendered page, ranged media bytes) are `HttpRouter` routes beside API groups

## [03]-[WORKER]

[HANDLERS]: Worker is one Effect `Layer` graph that `HttpRouter.toWebHandler` turns into a fetch handler
- Operations are `Effect.fn` spans, `Effect.catchTag` turns SQL, schema, and platform failures into a logged `ServiceUnavailable`
- `D1Client` from `@effect/sql-d1` keeps documents and asset metadata as JSON, R2 keeps media bytes
- ALWAYS enforce storage invariants as conditions of writing SQL statements
- Placement references are a SQL view over stored JSON, no reference row is written or kept in sync
- Portfolio versions are content-addressed by digest, draft and published states each point at one
- Draft writes send the digest their last read or write returned, a stale digest fails with `PreconditionFailed` and writes nothing
- `partial-content` evaluates HTTP preconditions and ranges for draft writes and media reads
- Uploads are idempotent per asset id, a repeat with equal metadata succeeds and differing metadata conflicts
- Every stored object has a row naming its upload attempt, interrupted uploads and removals stay listed until the owner discards them
- Media reads serve assets placed in published work to anyone, every other asset to the owner alone

## [04]-[PAGE]

[RENDER]: Worker renders `Site` into built `index.html` through `HTMLRewriter`, browsers hydrate the same `Site`
- Served markup holds published document and session alone, drafts reach a browser through owner endpoints
- `Bootstrap` encodes in Worker and decodes in browser through one `Schema.fromJsonString`, hydration removes its script
- Fragments name active project and selected composition, user selection pushes history and scrolling replaces it
- `popstate` and `hashchange` restore selection and scroll position from the fragment

[STYLE]: Tailwind owns every style through `theme.css`, with React Aria state variants from `tailwindcss-react-aria-components`
- Figures reserve their box before media loads through CSS custom properties computed from uploaded dimensions
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
- `Atom.family` keyed by URL shares one PDF.js document across every sheet of a PDF, scoped through `Effect.acquireRelease`
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

## [07]-[HOSTING]

[SITES]: OpenAI Sites runs the built Worker with D1 and R2 bindings `.openai/hosting.json` names
- Sites forwards signed-in user id and email headers, requests without an id are signed-out visitors
- First sign-in with an email equal to `OWNER_EMAIL` ignoring case enrolls its user id, later owner checks compare user ids alone
- Hosted configuration supplies `OWNER_EMAIL`, `vite.config.ts` sets it to the `sites()` local sign-in email for `serve` alone
- `sites()` copies `.openai/hosting.json` into project-local `dist` and exposes no relocation setting
