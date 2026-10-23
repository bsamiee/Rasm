# [ACTIONS]

Operations a reader takes use their listed verb across a project, in place of the words beside it.

## [01]-[VERBS]

| [INDEX] | [OPERATION]                         | [VERB]                 | [REPLACES]                                                         |
| :-----: | :---------------------------------- | :--------------------- | :----------------------------------------------------------------- |
|  [01]   | Item into a collection              | `add`                  | `put`, `insert` (no position), `include` (reader action)           |
|  [02]   | Item out of a collection, item kept | `remove`               | `take out`, `drop`, `get rid of`                                   |
|  [03]   | Stored data or file out of storage  | `delete`               | `erase`, `wipe`, `purge`, `destroy` (outside a command name)       |
|  [04]   | One item in place of another        | `replace`              | `remove and add`, `swap out`, `switch out`, `substitute`           |
|  [05]   | Same call after a transient failure | `retry`                | `try again`, `reattempt`                                           |
|  [06]   | Command or check after a change     | `rerun`                | `run again`, `re-execute`                                          |
|  [07]   | Process or app, stop then start     | `restart`              | `relaunch`, `reboot`, `reload` (app)                               |
|  [08]   | Page or file content from source    | `reload`               | `refresh` (page), `re-read`                                        |
|  [09]   | Service, session, or process        | `start`, `stop`        | `launch` (service), `kill` (outside a signal), `halt`              |
|  [10]   | Operation in progress               | `cancel`               | `abort` (reader action), `stop` (operation)                        |
|  [11]   | App                                 | `open`, `quit`         | `launch` (app), `close` (app), `exit` (app), `terminate`           |
|  [12]   | Window, document, panel, or page    | `open`, `close`        | `launch` (document), `bring up`, `dismiss` (window)                |
|  [13]   | Configuration value                 | `set`, `unset`         | `configure` (one value), `specify`, `define`, `clear`              |
|  [14]   | Boolean feature or rule             | `enable`, `disable`    | `turn on`, `turn off`, `activate` (feature), `deactivate`          |
|  [15]   | Package, app, or plugin onto a host | `install`, `uninstall` | `deploy` (local), `add` (app)                                      |
|  [16]   | Dependency or app, newer release    | `update`               | `upgrade` (outside a command name), `bump` (dependency)            |
|  [17]   | Account with a service or app       | `sign in`, `sign out`  | `log in` (account), `login` (verb), `authenticate` (reader action) |
|  [18]   | Shell or SSH session on a machine   | `log in`, `log out`    | `login` (verb), `sign in` (shell), `log on`                        |
|  [19]   | Permission                          | `grant`, `revoke`      | `allow access`, `give permission`, `authorize` (reader action)     |
|  [20]   | Interface item                      | `select`               | `pick`, `choose`, `click on` (selection)                           |
|  [21]   | Event the reader waits for          | `wait for`             | `hold on`, `be patient`                                            |

## [02]-[FORMS]

Each action names one operation that resolves its event:
- Actions name verb, object, and any value the reader needs from the event (`retry after <time>`)
- Verb pairs reaching one state become the verb naming it (`stop and start` becomes `restart`)
- Requirements become the action that meets them (`make sure`, `ensure`, `check that`, `verify that` go)
- Alternatives stay when each resolves a different cause and names it
- Persistent failures take the action that fixes them in place of `retry`

## [03]-[PAIRS]

- BEFORE: `"Browser cannot read image. Remove it and add new export"`
- AFTER: `"Browser cannot read image, replace it with new export"`
- WRONG: `"Browser cannot read image, remove it and add new export"` (period swapped for comma, verb pair kept)

- BEFORE: `"Browser cannot read file. Retry or add it again"`
- AFTER: `"Browser cannot read file, add it again"`
- WRONG: `"Browser cannot read file, retry or add it again"` (retry and add name one action)

- BEFORE: `"Account switching requires the <app> app to be closed"`
- AFTER: `"Quit <app> to switch accounts"` (requirement became action meeting it)

- BEFORE: `"Something went wrong, please try again later"`
- AFTER: `"<service> rate limit reached, retry after <time>"` (event stated, softener gone, wait derived from response)

- BEFORE: `"Request failed, retry"` (server rejected an expired token)
- AFTER: `"<service> rejected expired token, sign in again"`
- WRONG: `"<service> rejected expired token, retry"` (persistent failure, retry repeats it)

- BEFORE: `"Please login to your <service> account to continue"`
- AFTER: `"Sign in to <service> to continue"` (account takes `sign in`, `login` is a noun)
- WRONG: `"Log in to <service> to continue"` (shell session verb on an account)

- KEEP: `"Log in to <server> over SSH as <user>"` (shell session on a machine)
- KEEP: `"<tool> cannot write <file>, quit <app> holding it or free disk space"` (each alternative names its cause)
- KEEP: `"<file> holds no <section> table"` (no action reader can take)
