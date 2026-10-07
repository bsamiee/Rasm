# [PORTFOLIO]

Portfolio publishes architectural projects and studies as one server-rendered page with an owner-only editor.

## [01]-[HOSTING]

OpenAI Sites runs the app as a Cloudflare Worker with static assets:
- `.openai/hosting.json` holds `project_id` and the D1 and R2 binding names, `vite.config.ts` binds both locally from it
- Publishing saves a version from a commit and its `dist` archive, then deploys that saved version in a second step
- Hosted runtime values (`OWNER_EMAIL`) are set in the Sites panel
- Sites forwards `oai-authenticated-user-id` and `oai-authenticated-user-email` for signed-in visitors
- `/signin-with-chatgpt` and `/signout-with-chatgpt` take a `return_to` path
- `sites()` signs in as `seedy@sites.test` during `vite` dev, the `OWNER_EMAIL` value `vite.config.ts` sets for dev alone
- First sign-in with the `OWNER_EMAIL` address enrolls its user id in `owner`, later sessions match that id

## [02]-[STORAGE]

D1 holds JSON bodies and R2 holds file bytes, with no migration:
- `documents` rows hold the `draft` and `published` portfolio, keyed by state
- `assets` rows hold asset metadata by asset id, R2 holds each file under that id with its content type
- `owner` rows hold the enrolled user id per owner email
- Each isolate creates missing tables when it builds its database layer
- Saves write `draft`, publishes write `draft` and `published`, and both fail with 409 while a placement names a missing asset
- Asset deletes fail with 409 while a saved or published document places the asset
- Uploads post an `asset` JSON field before the file, and a repeated asset id succeeds only with identical metadata

## [03]-[ROUTES]

Worker runs first for `/` and `/api/*`, every other path serves a built asset:

| [INDEX] | [ROUTE]                                    | [ACCESS]                          | [CACHE]             |
| :-----: | :----------------------------------------- | :-------------------------------- | :------------------ |
|  [01]   | `GET /`                                    | Visitor                           | `private, no-store` |
|  [02]   | `GET /api/media/:id`                       | Visitor for published, else owner | `private, no-cache` |
|  [03]   | `GET /api/portfolio`, `PUT /api/portfolio` | Owner                             | `private, no-store` |
|  [04]   | `POST /api/publish`                        | Owner                             | `private, no-store` |
|  [05]   | `POST /api/media`, `DELETE /api/media/:id` | Owner                             | `private, no-store` |

- `GET /` fetches `index.html` from `ASSETS` and rewrites its title, description, `#portfolio` markup, and `#portfolio-data` JSON
- Owner routes answer 403 to a non-`GET` request with an `Origin` other than its own
- Media reads answer range and conditional requests with the R2 `ETag` and `X-Content-Type-Options: nosniff`

## [04]-[COMMANDS]

Vite builds the client and Worker in one run from the project directory:
- `nx run architectural-portfolio:build` runs `vite build`
- `nx run architectural-portfolio:build --configuration serve` runs `vite` with the Worker in workerd and local state under `.cache/wrangler`
- Build writes `dist/client` assets, the `dist/server` Worker with its `wrangler.json`, and `dist/.openai/hosting.json`
- Sites packaging requires `dist/server/index.js`, which the `server` Vite environment name produces
- `sites()` writes to Vite's default `dist` alone, and `.wrangler/deploy/config.json` redirects Wrangler to `dist/server/wrangler.json`

## [05]-[LAYOUT]

Model schemas sit beneath every other folder, the Worker and the page both import them:

```text
apps/portfolio/
├── .openai/  # Sites project id and binding names
├── editor/   # Owner editor: entries, compositions, placements, uploads, file library, request atoms
├── media/    # Image, video, and PDF sheet figures, zoom viewer, upload inspection
├── model/    # Asset, placement, and portfolio schemas, HTTP API
├── site/     # Public page, chapters, project index, fragment navigation, hydration entry
└── worker/   # Worker entry, D1 access, session, content and media handlers, page rewrite
```
