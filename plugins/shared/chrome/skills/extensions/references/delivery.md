# [DELIVERY]

## [01]-[PROJECT]

Extensions are products at `products/apps/chrome/<name>/`, a private `@rasm/<name>` package with no `project.json`:
- Root files: `manifest.json`, `_locales/`, `service-worker.ts`, each injected function with its CSS, `icons/`, `icon.css`, `vite.config.ts`, `README.md`
- `imports` rows `#/*.ts` and `#/*` resolve `#/toast.ts`, `#/manifest.json`, and `#/toast.css?inline`
- App program `tsconfig.app.json` takes `types: ["chrome", "vite/client", "web"]` and includes `_locales/*/messages.json`
- Node program `tsconfig.node.json` holds `vite.config.ts` and `manifest.json`
- `README.md` holds name, use, and dashboard answers under `[03]-[STORE]`
- `icons/` holds `mark.svg` and `mark-16.svg`, its drawing on a 16-unit grid for small widths
- `icon.css` sets brand color for action icon and promo tiles, its one place in a product
- Use `docs/Icons.md` for marks and their host forms

## [02]-[BUILD]

`vite.config.ts` derives every value from the imported manifest:

```ts
build: { target: `chrome${manifest.minimum_chrome_version}`, assetsInlineLimit: () => true, rolldownOptions: { input: 'service-worker.ts', output: { entryFileNames: manifest.background.service_worker } } },
```

- `assetsInlineLimit: () => true` inlines every asset as a data URL, as injected functions and CSS masks reach no package file
- One build plugin emits `manifest.json` from the imported JSON, each `_locales/<lang>/messages.json`, and each `icons` PNG
- `paint(drawings, stylesheet, { width, padding })` of `@rasm/mark` rasterizes each icon width from the drawing whose grid serves it
- `checks()` of `@rasm/vite` fails the build on Rolldown warnings whose output runs wrong
- `nx build @rasm/<name>` writes `.artifacts/vite/@rasm/<name>`, the folder `loaded` launches
- Owner's Chrome loads the folder through Load unpacked on `chrome://extensions`

## [03]-[RELEASE]

`@rasm/nx-distribution` holds a `chrome` host row in `products/developer/nx/distribution/host-packages.ts`:
- Rows whose glob matches `manifest.json` each try to decode it, first success winning, as UXP plugins claim that file name
- `build--chrome` zips the build folder's files in sorted order with their modes
- `nx release` writes `version` as `major.minor.patch`, refusing prerelease and build tags
- `nx release publish` skips a version whose published or submitted revision lists it as `crxVersion` in `:fetchStatus`
- Publishing uploads the zip to `upload/v2/publishers/<publisher>/items/<item>:upload`, polls `:fetchStatus` while `IN_PROGRESS`, then calls `:publish`
- Item IDs derive from manifest `key`, and a manifest with no `key` reports that its first version uploads by hand at the dashboard
- `CHROME_WEB_STORE_TOKEN` and `CHROME_WEB_STORE_PUBLISHER` reach the row from job `release`
- `attach--gh` uploads zip and suite media to the GitHub release of each `<project>@<version>` tag
- Use `docs/Infrastructure.md` for the keyless Google identity and `docs/Workflows.md` for job `release`

Chrome Web Store rules the release meets:
- `version` takes 1 to 4 dot-separated integers from 0 to 65535 without leading zeros, rising on every upload
- Packages stay unchanged after upload, so a fix takes a new version and a new review
- Review takes days, and an approved deferred publish goes live within 30 days
- Packages hold no hidden files, and every file the manifest names exists

## [04]-[LISTING]

Dashboard tabs ask for values, each answered from code:

| [INDEX] | [FIELD]            | [RULE]                                                                       |
| :-----: | :----------------- | :--------------------------------------------------------------------------- |
|  [01]   | Description        | Plain text within 16,000 characters, behavior with no API or framework names |
|  [02]   | Category           | One dashboard category                                                       |
|  [03]   | Icon               | 128px PNG from manifest                                                      |
|  [04]   | Screenshots        | 1 to 5 at 1280x800, shown at 640x400, no device mockups or trademarks        |
|  [05]   | Small promo tile   | 440x280 PNG, listings with one rank ahead of those without                   |
|  [06]   | Marquee promo tile | 1400x560 PNG, needed for featuring                                           |
|  [07]   | Single purpose     | One sentence naming its job                                                  |
|  [08]   | Permission reasons | One sentence per manifest permission naming its calling feature              |
|  [09]   | Remote code        | No                                                                           |
|  [10]   | Data usage         | Each category code handles, and each Limited Use certification               |
|  [11]   | Privacy policy     | Live URL where code handles user data, none otherwise                        |
|  [12]   | Test instructions  | Steps a reviewer follows to reach each feature                               |

Listing answers follow code:
- Permission reasons cover exactly the manifest's permissions, as unused ones and "needed to work" fail review
- Data usage, privacy policy, and code agree
- Extensions reading one URL at the user's request and storing and sending nothing declare no data and take no policy URL
- Promo tiles carry mark, name, and saturated color with little text, and Google reviews them apart from packages
- Accounts publish after 2-Step Verification, a verified contact email the listing shows, and a trader or non-trader declaration
