# [MEZRAB]

Mezrab is a Chrome extension: one shortcut copies the current tab's link and confirms it with a toast matching Chrome's own "Link copied".

## [01]-[NAME]

Mezrab, Persian مضراب, pronounced "mez-RAHB", names the plectrum that strikes a string once to sound a note, as one keystroke copies one link.

## [02]-[USE]

Each trigger copies the tab's link from the service worker, then draws the toast in the page:
- `Command+Shift+U` on macOS, `Alt+Shift+U` on Linux where IBus holds `Ctrl+Shift+U` for Unicode input, and `Ctrl+Shift+U` elsewhere
- Toolbar button and a rebinding at `chrome://extensions/shortcuts` run the same action
- Toast matches Chrome's toast in size, type, motion, 4s lifetime, and inverse neutral colors, and holds still under reduced motion
- Toasts on `http:` and `https:` pages that `navigator.canShare` accepts end in a Share link button opening the system share sheet
- Linux lacks Web Share and shows no Share link button
- Share link shares the tab's URL, with the page title only where the page declares one
- Toasts with a Share link button hold 8s as Chrome's action toasts do, pause on hover or focus, and close on Escape
- Each close returns focus to the element focused before the toast
- Toast host leaves the page DOM once its exit motion ends or Chrome cancels it
- Pages `activeTab` cannot read (`chrome://`, Web Store, `view-source:`, a new origin before first paint) copy nothing and badge `✕` for 4s
- Pages refusing the toast keep the copy and badge `✓` for 4s
- Toolbar button title names the reason behind each badge
- `_locales` holds name, description, toast, and title text for `chrome.i18n`

## [03]-[DELIVERY]

Nx targets build, version, and publish Mezrab:
- `nx build @rasm/mezrab` writes the unpacked extension to `.artifacts/vite/@rasm/mezrab` for Load unpacked at `chrome://extensions`
- `nx release` versions `manifest.json`, and `nx release publish` uploads the zip and submits it for review
- Publish reads its access token from `CHROME_WEB_STORE_TOKEN` and its publisher from `CHROME_WEB_STORE_PUBLISHER`
- `manifest.json` `key` holds the item's public key, and publish derives the store item from it
- Screenshots and promo tiles from `pack--playwright` of `@rasm/test-mezrab` attach to each `@rasm/mezrab@<version>` GitHub release

## [04]-[STORE]

Chrome Web Store dashboard answers for listing and privacy tabs:
- Single purpose: copy the current tab's link with one shortcut or click and confirm the copy on the page
- Category: Tools
- Data usage: none, and the store shows its no-data statement with no privacy policy URL
- `activeTab`: reads the acting tab's link and opens the tab to the toast script for one action
- `clipboardWrite`: writes the link to the clipboard
- `scripting`: draws the confirmation toast in the tab
