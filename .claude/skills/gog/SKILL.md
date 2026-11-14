---
name: gog
description: "gog CLI: safe Google Workspace automation, JSON, auth, scoped reads/writes."
---

# [GOG]

`gog` covers Google Workspace work a connector lacks, shell automation needing stable JSON, and local Google auth state.

## [01]-[FAST_PATH]

```bash
gog --version
gog auth list --check --json --no-input
gog auth doctor --check --json --no-input
GOG_HELP=agent gog --help
gog schema --json
```

`GOG_HELP=agent` limits root help to the automation contract and read-only recipes, and commands behave the same. `gog schema` prints command syntax, exit codes, and the effective safety state as JSON.

Output flags:
- `--fields` aliases `--select` on commands without their own API field-mask `--fields`
- `--results-only` unwraps the primary result before `--select` projects it, so lists select item fields (`--results-only --select id`)
- Dot paths stop at nested arrays (`--select items.id` selects nothing), and unmatched object fields drop
- `--json --wrap-untrusted` marks fetched Google text as untrusted for agent parsing, stdout holds data and stderr hints and progress

API work names its account:

```bash
gog --readonly --account <email> gmail search 'newer_than:7d' --json --wrap-untrusted
```

## [02]-[SAFETY]

- Tokens, OAuth client secrets, and keyring passwords stay out of output
- `GOG_KEYRING_BACKEND=file`, `GOG_KEYRING_PASSWORD`, and `HOME` must reach the process launching `gog`, a service checked apart from the login shell
- `--no-input` makes auth and keyring prompts fail in automation
- `--dry-run` runs first where a command supports it
- `--readonly` stays on unless the task is the exact write the user approved
- `--force` confirms a destructive command, added only for the exact mutation the user asked for
- `--gmail-no-send` or `GOG_GMAIL_NO_SEND=1` stays set unless sending mail is the task
- `gog schema --json` field `automation.safety` reports the active guards and any baked build profile

Runtime command guards:

```bash
gog --readonly --enable-commands gmail.search,gmail.get --gmail-no-send --account <email> gmail search 'from:<sender>' --json
gog --enable-commands drive.ls,docs.cat --disable-commands drive.delete --account <email> drive ls --max 10 --json
```

## [03]-[AUTH]

Agents inspect and diagnose OAuth, and consent completes in a browser:

```bash
gog auth credentials list
gog auth add <email> --services all-user --force-consent
gog auth remove <email>
```

Reauth keeps an account's service breadth:
- `gog auth list --check --json --no-input` lists the account's current `services` before reauth
- Expired or revoked tokens take `--services all-user --force-consent` unless the user asks for narrower scopes
- `all-user` excludes Google Ads, and Google can require passkey enrollment for the `ads` scope, blocking the consent flow; `--services all-user,ads` stays for a user needing Ads
- Narrow services fit test accounts, single-service bots, user requests, and scope experiments
- Command guards, dry runs, and account choice limit durable user auth in place of narrow scopes
- Service accounts are Workspace-only and fit Admin, Groups, Keep, and domain-wide delegation, never consumer `@gmail.com` OAuth

Services launching `gog` run the diagnostic through their own entrypoint after a restart:

```bash
gog auth doctor --check --no-input && gog gmail search 'newer_than:1d' --max 1 --json
```

`keyring.password` failures there while `gog auth doctor` passes in a login shell mean the service environment lacks the keyring variables, fixed before any reauth.

### [03.1]-[BROWSER_REAUTH]

Agents driving a signed-in browser complete reauth end to end, consent still happening in that browser. The CLI leg runs in a detached tmux session that outlives each command:

```bash
tmux -L gog-auth new-session -d -s auth -x 200 -y 50
tmux -L gog-auth send-keys -t auth "gog auth add <email> --services all-user --force-consent --timeout 15m" Enter
tmux -L gog-auth capture-pane -t auth -p -J -S - | rg -o 'https://accounts\.google\.com[^ ]+' | tail -n 1 > "$url_file"
```

`capture-pane` without `-J` returns the URL wrapped at pane width, and Google answers a truncated URL with `Invalid OAuth Request` or `Invalid response_type: missing`, so the captured URL must contain `response_type`. A Playwright session attached to the signed-in Chrome opens it, `&login_hint=<email>` skipping the account chooser:

```bash
playwright cli -s=<session> attach --extension=chrome
playwright cli -s=<session> tab-new "$(cat "$url_file")&login_hint=<email>"
playwright cli -s=<session> tab-close
```

Unverified or testing OAuth clients show interstitials:
- `Google hasn't verified this app` — `Continue` is the low-emphasis link and `Back to safety` the prominent button that aborts, with a developer-info control between them in tab order
- `You're signing back in to <app>` — `Continue` once the shown account is the intended one

Listeners stop at `--timeout` and the tmux pane returns to a shell prompt, so a flow that did nothing reads the pane first and restarts the CLI leg in place of reusing a stale URL. Callbacks target `http://localhost:<port>`, so a browser on another host needs that port forwarded to the listener host before the URL opens. File keyring passphrase prompts in tmux take the value from that host's login shell environment, unprinted.

Reauth passes when `gog auth list --check --json --no-input` reports the account valid with its earlier service list, a login that narrowed scopes counting as failed.

## [04]-[READS]

```bash
gog --readonly --account <email> gmail search 'newer_than:3d' --max 10 --json --wrap-untrusted
gog --readonly --account <email> gmail get <messageId> --sanitize-content --json --wrap-untrusted
gog --readonly --account <email> gmail thread get <threadId> --sanitize-content --json --wrap-untrusted
gog --readonly --account <email> calendar events --today --json --wrap-untrusted
gog --readonly --account <email> drive ls --max 20 --json --wrap-untrusted
gog --readonly --account <email> docs cat <documentId> --json --wrap-untrusted
gog --readonly --account <email> sheets get <spreadsheetId> Sheet1!A1:D20 --json --wrap-untrusted
gog --readonly --account <email> contacts list --max 20 --json --wrap-untrusted
```

Gmail bodies take `--sanitize-content` unless the user needs raw content.

## [05]-[WRITES]

Writes name the account, object ID, and exact mutation, prefer commands with `--dry-run`, and delete test objects after verification:

```bash
gog --account <email> docs write <documentId> --append --text '<text>'
gog --account <email> docs write <documentId> --tab <tab> --markdown --replace --file <file>.md
gog --account <email> docs update <documentId> --tab <tab> --markdown --file <file>.md
gog --account <email> docs update <documentId> --tab <tab> --replace-range START:END --text '<text>'
gog --account <email> sheets update <spreadsheetId> Sheet1!A1 --values-json '[["<value>"]]'
gog --account <email> sheets batch-update <spreadsheetId> --data-json @<file>.json
gog --account <email> drive upload <file> --parent <folderId> --json
```

Docs tab rules:
- `docs list-tabs <documentId> --json` lists tab titles and IDs before a write targets a tab
- `docs write --markdown --replace --tab <tab>` replaces a whole tab with formatted content
- `docs update --markdown --tab <tab>` inserts or appends formatted content and keeps the tab
- `docs update --replace-range START:END` replaces plain text, `--markdown` replacing that range with formatted content
- `START:END` is a UTF-16 Docs API range read from `docs cat --raw`, `docs raw`, or a `documents.get` readback
- `--replace-range` and `--index` exclude each other

Other write rules:
- `gmail batch delete` deletes for good and needs the `https://mail.google.com/` scope, `gmail trash` stays the default, and a permanent delete follows the reauth command `gog` prints
- `sheets batch-update` sends many value ranges in one Sheets API request from inline JSON or `@file`, in place of a loop of `sheets update`
- `gmail reply` and `gmail reply-all` inherit the subject, quote by default, keep display names and inline images, and treat `--to`, `--cc`, and `--bcc` as additions or moves, `--no-quote` dropping the quote

```bash
gog --account <email> gmail reply <messageId> --body-file <file>
gog --account <email> gmail reply-all <messageId> --body-file <file> --bcc <email> --remove <email>
```

## [06]-[DISCOVERY]

`gog schema` prints a command's flags, exit codes, and safety state as JSON, and help prints the same command as prose:

```bash
gog schema <service> <command> --json
gog help <service> <command>
gog <service> <command> --help
```
