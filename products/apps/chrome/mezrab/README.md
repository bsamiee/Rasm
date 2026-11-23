# [MEZRAB]

Mezrab is a Chrome extension: one shortcut copies a tab's link, and a toast matching Chrome's own Link copied toast confirms it.

## [01]-[NAME]

Mezrab (Persian مضراب, "mez-RAHB") names a plectrum that sounds one note per stroke, as one keystroke copies one link.

## [02]-[USE]

Mezrab's toolbar button and keyboard shortcut run one action:
- Mezrab binds `Command+Shift+U` on macOS, `Alt+Shift+U` on Linux where IBus holds `Ctrl+Shift+U` for Unicode input, and `Ctrl+Shift+U` elsewhere
- Copied links keep their query, fragment, and non-ASCII characters as the URL standard serializes them
- Each first action after Chrome stops Mezrab's service worker copies the tab's link
- Toasts draw Chrome's centered 48px pill, with text 40px from the pill's start and 16px from its end, at any page zoom
- Toasts keep Chrome's 4px from each side of a viewport too narrow for their pill, with the whole Share link button inside
- Toasts set their type from Chrome's font size setting, 13px/24px at its default 16px, whatever font size the page sets
- Toasts paint neutral tones inverse to the color scheme: dark under light text in a light scheme, light under dark text in a dark scheme
- Toasts paint a `CanvasText` background under `Canvas` text when `prefers-contrast: more` matches
- Toasts draw a solid border under forced colors and none without them
- Toasts enter over 300ms and leave over 150ms on Chrome's toast curves
- Toasts run no transition under `prefers-reduced-motion` and leave the page when their hold ends
- Toasts announce Link copied to screen readers on each action
- Toasts stay visible in their own case under page CSS that unsets every element and hides and uppercases the root element
- Toasts pass clicks on their pill to the page beneath
- Toasts show on a page that painted and still parses behind a script yet to load
- Toasts hold for 4s, or for 8s with a Share link button as Chrome's action toasts do, then leave the page
- Actions that find a toast showing keep one toast and restart its hold
- Holds pause while a hovering pointer or keyboard focus rests on the Share link button, and run again after it leaves
- Holds keep running under the hover a tap leaves on screens with no hovering pointer
- Toasts stay open with their hold paused while a share they started is pending, and close once the share settles
- Toasts on `http:` and `https:` pages end in a Share link button when `navigator.canShare` accepts the tab's URL and title
- Toasts show no Share link button where `navigator.share` or `navigator.canShare` is missing, as on Linux
- Share link buttons draw `ios_share` on macOS, `share_windows` on Windows, and `share` elsewhere
- Each click or Enter on a Share link button shares the tab's URL once through a system share sheet, with a page title where one is set
- Escape closes a toast while focus rests inside it, and reaches the page while focus rests outside it
- Toasts that close with focus inside return it to whichever element held it before, or to the page when focus came from outside
- Toasts that close with focus outside them leave focus where it is
- Toast hosts leave the page DOM once their exit motion ends or Chrome cancels it
- Pages where `activeTab` grants no access to the tab's URL, `chrome://` pages among them, copy nothing, draw no toast, and badge `✕` for 4s
- Actions whose clipboard write fails copy nothing, draw no toast, and badge `✕` for 4s
- Pages that refuse a toast, SVG documents among them, keep the copied link and badge `✓` for 4s
- Mezrab's toolbar button title names the reason behind each badge
- Each failure restarts the badge's 4s countdown
- `browser.alarms` clears each badge even when Chrome stopped Mezrab's service worker in the meantime

## [03]-[STORE]

Answers to the Chrome Web Store dashboard's listing and privacy tabs:
- Single purpose: copy a tab's link with one shortcut or click and confirm the copy on its page
- Category: Tools
- Data usage: web history, as Mezrab reads the acting tab's URL and title, with each Limited Use statement certified
- Privacy policy: `https://github.com/bsamiee/Rasm/blob/main/products/apps/chrome/mezrab/README.md#04-privacy`
- `activeTab`: reads the acting tab's link and lets a toast script run in it for one action
- `alarms`: clears the toolbar badge 4s after an action fails
- `clipboardWrite`: writes each copied link to the clipboard
- `scripting`: draws a confirmation toast in the tab
- Permissions new to `manifest.json` take a dashboard answer after their package uploads and before review, as no store API sets one

## [04]-[PRIVACY]

Mezrab handles one kind of user data, the URL and title of a tab its user acts on:
- Mezrab reads a tab's URL and title only when its user presses the shortcut or toolbar button on that tab
- Mezrab writes that URL to the clipboard, and passes URL and title to the system share sheet only when its user presses Share link
- Mezrab stores nothing, sends nothing to any server, and holds no analytics, advertising, or remotely loaded code
- Mezrab shares data with no one, and a share sheet sends a link to the app its user picks there
- Changes to these terms appear in the history of `README.md`, and questions go to the issue tracker of `bsamiee/Rasm` on GitHub
