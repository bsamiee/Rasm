---
name: google
description: "Use when work reads or writes any Google account or service (Gmail, Calendar, Drive, Docs, Sheets, Slides, Contacts, Tasks, Forms, Chat, Keep, YouTube, Apps Script, Cloud APIs, Workspace admin), covering account routing across gog, GAM, and Drive mounts, output shapes, edits, and misleading errors"
---

# [TOOL_GOOGLE]

Google work runs through `gog` for every account and service, GAM for Workspace administration `gog` lacks, and Drive for desktop mounts for file reads. Every account holds every scope, so commands need no consent, guard, or read-only flag.

## [01]-[ACCOUNTS]

Accounts route by flag:
- No `-a` -> `b.samiee93@gmail.com`, OAuth with every user service
- `-a work` -> `b.samiee@mzn-group.com`, `-a work-info` -> `info@mzn-group.com`, service accounts with domain-wide delegation and no OAuth
- Chat, Keep, Groups, Admin, directory, Drive labels, and Gmail delegates answer under `-a work` alone, the personal account refuses them
- Google connectors of claude.ai reach the personal account alone, a work file through them returns `Permission denied`
- `~/Library/CloudStorage/GoogleDrive-<email>/` holds `My Drive` and `Shared drives` of each account as plain files, native Docs files as stubs

Errors naming a missing token or `gog auth add` have causes other than an expired token:
- `GOG_KEYRING_PASSWORD` absent from the process -> `gog auth doctor --check --json --no-input` names it in row `keyring.password`, the fix is the variable
- `api call` under `-a work` without `--scope` -> asks for the narrowest Discovery scope delegation lacks, pass the scope the method needs
- Expired or revoked personal token -> browser reauth in [06]-[REAUTH]

## [02]-[ROUTING]

| [INDEX] | [JOB]                                                         | [TOOL]                     | [REASON]                                |
| :-----: | :------------------------------------------------------------ | :------------------------- | :-------------------------------------- |
|  [01]   | Gmail, Calendar, Drive, Docs, Sheets, Slides, Contacts, Tasks | `gog`                      | Typed commands for every account        |
|  [02]   | Chat, Keep, Groups, users, org units                          | `gog -a work`              | Workspace services under delegation     |
|  [03]   | Aliases, licenses, domains, devices, reports, tokens          | GAM                        | `gog admin` holds none of them          |
|  [04]   | Other users' mail settings, calendar ACLs, bulk CSV changes   | GAM                        | Admin-side and bulk across users        |
|  [05]   | Policy writes                                                 | `gog -a work api call`     | GAM OAuth holds read-only policy scope  |
|  [06]   | Any Google API without a command                              | `gog api call`             | Discovery methods with `cloud-platform` |
|  [07]   | Reading a Drive file's bytes                                  | CloudStorage mount         | No API call                             |
|  [08]   | Visual check of a slide                                       | Slides connector thumbnail | Personal account files alone            |
|  [09]   | YouTube download, transcript                                  | `yt-dlp`, NotebookLM       | `gog youtube` holds no media            |

## [03]-[DISCOVERY]

Command contracts come from the tools at their installed version:
- `GOG_HELP=agent gog --help` prints the automation contract and exit codes, `gog schema <command path> --json` one command's flags and output
- `gog schema --json` without a path prints every command at once, a size no context holds
- Exit codes decide the next step: 3 empty, 4 auth, 5 not found, 6 denied, 7 rate limited, 8 retryable
- GAM syntax sits in `GamCommands.txt` beside its binary, read with `rg '<command>' "$(dirname "$(readlink -f "$(command -v gam)")")/GamCommands.txt"`
- GAM `ERROR: Invalid argument` -> a word sits out of order, compare the command with its `GamCommands.txt` line

## [04]-[OUTPUT]

Output shapes differ from the Google APIs:
- `gog` JSON renames API fields (`admin users list` prints `email`, `admin`, never `primaryEmail`, `isAdmin`), so one record is read before `--select` names fields
- `raw` subcommands and `--raw` flags print the API's own shape
- `--results-only` drops `nextPageToken`, `--all` reads every page, `gmail search --count` totals a query, `--ids-only` skips per-thread fetches
- Bulk Gmail reads meet per-user quota as exit 7 (`rateLimitExceeded`), smaller pages and a pause clear it
- GAM `print ... formatjson` prints CSV with key columns and one `JSON` column, read by `duckdb -c "select * from read_csv('<file>')"` then `json` functions, `info` and `show` with `formatjson` print JSON

## [05]-[EDITS]

Edits target stable anchors in place of computed positions:
- Docs: `docs update --at '<literal text>'` replaces matched text, `--occurrence <n>` picks one match of many, `docs find-range` prints a UTF-16 range for `--replace-range`
- Docs: `docs cat` reads the default tab alone, `--all-tabs` reads every tab, `docs list-tabs` names tab IDs that `--tab` takes on reads and writes
- Docs: `docs write --markdown --replace --tab <tab>` replaces a whole tab with formatted content, `docs update --markdown` inserts and keeps the tab
- Slides: element object IDs come from `slides raw`, `slides read-slide` prints text without them
- Sheets: `sheets batch-update --data-json @<file>` sends every range in one request
- `api call` non-GET methods take `--allow-write --force`, Drive `about.get` takes `--params '{"fields":"*"}'`
- Gmail `reply` and `reply-all` inherit subject and quote, `--to`, `--cc`, and `--bcc` add recipients and `--remove` drops one

## [06]-[REAUTH]

Personal OAuth reauth keeps every service, consent completing in the signed-in Chrome:

```bash
tmux -L gog-auth new-session -d -s auth -x 200 -y 50
tmux -L gog-auth send-keys -t auth "gog auth add b.samiee93@gmail.com --services all-user --force-consent --timeout 15m" Enter
tmux -L gog-auth capture-pane -t auth -p -J -S - | rg -o 'https://accounts\.google\.com[^ ]+' | tail -n 1 > "$url_file"
playwright cli -s=<session> attach --extension=chrome --config ~/.playwright/extension.config.json
playwright cli -s=<session> tab-new "$(cat "$url_file")&login_hint=b.samiee93@gmail.com"
```

- `capture-pane` without `-J` wraps the URL at pane width, and Google answers a URL missing `response_type` with `Invalid OAuth Request`
- Unverified app screen -> `Continue` is the low-emphasis link, `Back to safety` aborts
- Listener stops at `--timeout` and the pane returns to a prompt, so a stalled flow restarts the CLI leg in place of reusing its URL
