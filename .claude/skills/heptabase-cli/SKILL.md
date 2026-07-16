---
name: heptabase-cli
description: Interact with Heptabase using the CLI to manage knowledge base content, search cards, edit properties, read parsed PDF and media transcript content, export local files, manage whiteboard cards, and browse AI Tutor goals, courses, and lessons.
allowed-tools: Bash(heptabase *) Bash(jq *) Bash(mktemp *)
metadata:
    heptabase-cli-version-range: '0.4.x'
---

# [HEPTABASE_CLI]

Manage Heptabase knowledge base content through the local CLI; every read and write enters through the `heptabase` command and returns JSON on stdout for `jq` parsing or downstream piping.

## [01]-[ROUTING]

- [01]-[CARD_CONTENT_SCHEMA](references/card-content-schema.md): ProseMirror JSON, card and whiteboard mentions, dates, videos, math extensions
- [02]-[PROPERTY_VALUES](references/property-values.md): property value formats by type, relation write semantics
- [03]-[FILE_READING](references/file-reading.md): file listing and export from PDF and media cards
- [04]-[PDF_READING](references/pdf-reading.md): parsed PDF page metadata and page-range reads
- [05]-[TRANSCRIPT_READING](references/transcript-reading.md): audio and video transcript metadata and time-range reads
- [06]-[CODEX_SANDBOX](references/codex-sandbox.md): local CLI server access under the Codex sandbox

## [02]-[PREREQUISITES]

- Desktop app installs the CLI: `heptabase` on macOS/Linux, `heptabase.cmd` on Windows for cmd/PowerShell beside a `heptabase` POSIX shim.
- `heptabase --version` gates use: a version outside this skill's range `0.4.x` halts work — ask the user to update the desktop app or skill package.

## [03]-[COMMAND_DISCOVERY]

Run `heptabase help` for the live top-level command list; each command carries `--help` for detailed usage:

```bash copy-safe
heptabase help
heptabase note --help
heptabase note create --help
```

## [04]-[COMMON_RECIPES]

Use these as quick recipes for frequent requests. For less common flags or if a command fails, run `heptabase help` or `<command> --help` to discover the correct syntax.

- [RECENT_CARDS]: `heptabase card list --sort createdTime --direction descending --limit 20`
- [TODAYS_JOURNAL]: `heptabase journal read $(date +%Y-%m-%d)`
- [SEARCH_CARDS_BY_KEYWORD]: `heptabase card list -q "<keyword>" --limit 20`
- [CREATE_A_NOTE_FROM_MARKDOWN]: `heptabase note create --content "# Title\n\nBody"`
- [APPEND_MARKDOWN_TO_A_NOTE]: `heptabase note append <cardId> --content "More content"`
- [READ_NOTE_JSON]: read `card-content-schema.md`, then `heptabase note read <cardId>` for the card's ProseMirror JSON.
- [EDIT_NOTE_CONTENT_WITH_JSON_SAVE]: edit the returned JSON, then `heptabase note save <cardId> --content-md5 <contentMd5> --content-file <path>`.
- [LIST_TAG_PROPERTIES]: `heptabase tag properties <tagId>`
- [LIST_CARDS_WITH_PROPERTY_VALUES]: `heptabase tag cards <tagId> --include-properties`
- [READ_CARD_PROPERTIES]: `heptabase card properties <cardIdOrDate>`
- [SET_CARD_PROPERTY]: read `property-values.md`, then `heptabase card set-property <cardIdOrDate> --property-id <propertyId> --value "Published"`.
- [SET_TYPED_CARD_PROPERTY]: swap `--value` for `--json-value ...` when the property takes typed JSON rather than a string or option.
- [READ_PARSED_PDF_CONTENT]: read `pdf-reading.md`, then `heptabase pdf metadata <pdfCardId>` for `totalPages`.
- [READ_PDF_PAGE_RANGE]: `heptabase pdf read <pdfCardId> --start-page N --end-page N`.
- [READ_AUDIO_TRANSCRIPT]: read `transcript-reading.md`, then `heptabase audio metadata <audioCardId>` for `transcriptStatus` and `durationSeconds`.
- [READ_VIDEO_TRANSCRIPT]: read `transcript-reading.md`, then `heptabase video metadata <videoCardId>` for `transcriptStatus` and `durationSeconds`.
- [READ_AUDIO_TIME_RANGE]: `heptabase audio read <audioCardId> --start-seconds 0 --end-seconds 300` returns every overlapping entry.
- [READ_VIDEO_TIME_RANGE]: `heptabase video read <videoCardId> --start-seconds 0 --end-seconds 300` returns every overlapping entry.
- [READ_A_FILE_FROM_A_PDF_MEDIA_CARD]: read `file-reading.md`, then `heptabase file list --card-id <cardId>` for the file `id`.
- [READ_A_FILE_BY_FILEID]: read `file-reading.md`, run `mktemp -d`, then `heptabase file export <fileId> --output-dir <scratchDir>`.
- [READ_AN_EXPORTED_FILE]: read the `path` the export returns with your native file-reading tool.
- [LIST_CARDS_ON_A_WHITEBOARD]: `heptabase whiteboard cards <whiteboardId>`
- [ADD_A_CARD_TO_A_WHITEBOARD]: `heptabase whiteboard add-card --whiteboard-id <whiteboardId> --card-id <cardIdOrDate>`

## [05]-[CONTENT_EDITING]

Use `create` / `append` with Markdown for ordinary writing. Reading `card-content-schema.md` is mandatory before calling `heptabase note save` / `heptabase journal save` with ProseMirror JSON, and before generating Markdown that uses Heptabase-specific extensions such as card mentions, whiteboard mentions, dates, videos, math, or `toggle`/`todo` lists.

## [06]-[PROPERTY_EDITING]

Setting a property value requires reading `property-values.md` first and inspecting the target property with `heptabase card properties <cardIdOrDate>` and/or `heptabase tag properties <tagId>`. Property formats vary by type, and relation writes replace the full relation value. For relation properties, use `heptabase tag properties <sourceTagId>` to get the property definition's `relationTargetTagId`, then list valid related cards before writing.

## [07]-[TROUBLESHOOTING]

- [DESKTOP_APP_MUST_BE_RUNNING]: Every command reaches a local server in the app, so a closed app fails all; `heptabase start` launches to readiness.
- [CODEX_SANDBOX_MAY_BLOCK_LOCAL_CLI_SERVER]: An unready CLI server under Codex routes to `codex-sandbox.md`; escalate and retry outside the sandbox.
- [MUTATIONS_ARE_SERIALIZED]: Writes run one at a time to prevent conflicts; reads are concurrent.
- [WRITE_OPERATIONS]: `create` `save` `append` `trash` `restore` `tag add/remove` `card set-property` `file export` `whiteboard add-card/remove-card`.
- [REQUEST_BODY_SIZE_LIMIT]: Any request body larger than 1 MB is rejected by the server.
- [REQUEST_TIMEOUT]: Any request taking longer than 10 seconds to send its body times out.

## [08]-[BOUNDARIES]

- [CLI_ONLY_ACCESS]: Never reach app data through local database files, app storage, cache files, internal endpoints, or any non-CLI mechanism.
- [UNSUPPORTED_OPERATION]: An operation the CLI omits stops and reports as unsupported.
- [LOCAL_SERVER_SETUP]: This skill never repairs local CLI wiring; ask the user to enable Local CLI Server and CLI install in desktop settings.
- [LOCAL_FILES_ONLY]: `heptabase file export` reaches only metadata and raw files local to the desktop app, never downloading from cloud storage.
- [BINARY_UPLOAD]: This skill is for JSON/text operations on notes/journals/tags/cards and AI Tutor reads, not file upload or media-processing APIs.
- [WHITEBOARD_MUTATION]: Listing whiteboards and adding, listing, or removing their cards works; creating, renaming, moving, or deleting one does not.
- [PROPERTY_FILTERING]: Reading tag property schemas and values and setting one value on a card works; querying cards by property value does not.
