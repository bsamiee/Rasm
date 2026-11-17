---
name: arena
description: "Use when a task reads or writes Are.na channels, blocks, users, or groups, covering search, channel reads, image and file retrieval, and connections"
argument-hint: "[query or channel]"
---

# [SEARCH_ARENA]

`arena` (`@aredotna/cli`) calls Are.na's v3 API with `ARENA_TOKEN` from the environment, and `arena` MCP tools call the same API.

- Output: pass `--json` to every call and project fields with `jq`, each block holds every image size, link, and user object
- Paging: `--per` accepts 1–100, `meta.has_more_pages` and `meta.next_page` decide the next `--page`
- Identity: channels, users, and groups take a slug or ID, blocks, connections, and comments take a numeric ID
- Failure: `--json` writes `{"error","code","type"}` to stderr, exit `2` unauthorized, `3` not found, `4` invalid, `5` rate limited, `6` forbidden
- Version: Nix pins the release `arena` runs, `arena update` changes nothing it runs
- Routing: MCP tools serve writes into many channels at once and channel summaries, the CLI serves every other task

| [INDEX] | [TASK]                                  | [OPERATION]                                                 |
| :-----: | :-------------------------------------- | :---------------------------------------------------------- |
|  [01]   | Channels, blocks, users, groups by text | `arena search '<query>' --type <type> --json`               |
|  [02]   | Own content                             | `arena search '<query>' --scope my --json`                  |
|  [03]   | Files of one extension                  | `arena search '*' --type Attachment --ext <ext> --json`     |
|  [04]   | Channel metadata and counts             | `arena channel <slug> --json`                               |
|  [05]   | Channel items in owner order            | `arena channel contents <slug> --sort position_asc --json`  |
|  [06]   | Block with image and attachment URLs    | `arena block <id> --json`                                   |
|  [07]   | Channels holding a block or channel     | `arena block connections <id>`, `arena channel connections` |
|  [08]   | User or group content                   | `arena user contents <slug>`, `arena group contents <slug>` |
|  [09]   | Channel files on disk                   | `arena download <slug> <dir> --json`                        |
|  [10]   | Channel summary from a sample           | `channelDigest`                                             |
|  [11]   | Text or URL block                       | `arena add <channel> '<value>' --json`                      |
|  [12]   | Local file or directory as blocks       | `arena upload <file> --channel <channel>`, `arena import`   |
|  [13]   | Existing block or channel in a channel  | `arena connect <id> <channel> --json`                       |
|  [14]   | Block or connection in many channels    | `createBlock`, `createConnection`, `batchCreateBlocks`      |

## [01]-[SEARCH]

`search` requires Are.na Premium on the token's account, and `403` means the account lacks it. `--type` takes one type or a comma list, `--channel-id`, `--user-id`, and `--group-id` take numeric IDs from a prior read, and `--sort random` with `--seed` repeats one order across pages.

```bash
# Channels matching a query, one row each
arena search '<query>' --type Channel --per 20 --json \
    | jq -r '.data[] | [.id, .slug, .title, .counts.contents] | @tsv'

# Image and link blocks with source and preview URLs
arena search '<query>' --type Image,Link --per 20 --json \
    | jq -r '.data[] | [.id, .type, .title, .source.url, .image.medium.src] | @tsv'

# Blocks inside one channel
arena search '*' --channel-id <id> --type Image --json | jq -r '.data[] | [.id, .title] | @tsv'

# PDFs across Are.na
arena search '*' --type Attachment --ext pdf --json | jq -r '.data[] | [.id, .title, .attachment.url] | @tsv'

# Repeatable random sample
arena search '*' --type Image --sort random --seed <n> --json
```

## [02]-[READING]

`channel <slug>` returns metadata alone, items come from `channel contents`. Image blocks hold `image.src` (original) and `image.<size>.src` for `small`, `medium`, `large`, and `square`, link blocks hold `source.url` and a preview image, and attachment blocks hold `attachment.url`. Downloaded images open with `Read`, and PDFs follow `search:web` PDF reading.

```bash
# Channel metadata and counts
arena channel <slug> --json | jq '{id, title, visibility, counts, owner: .owner.slug}'

# Items in owner order, connection IDs kept for moves
arena channel contents <slug> --sort position_asc --per 100 --page <n> --json \
    | jq -r '.data[] | [.id, .type, .title, .connection.id] | @tsv'

# Block image at a viewable size
curl -fsSL --create-dirs -o '.artifacts/search/arena/<id>.jpg' \
    "$(arena block <id> --json | jq -r '.image.medium.src')"

# Channels holding a block, own channels excluded
arena block connections <id> --filter EXCLUDE_OWN --json | jq -r '.data[] | [.id, .slug, .title] | @tsv'

# Channels a user or group owns, newest first
arena user contents <slug> --type Channel --sort updated_at_desc --json | jq -r '.data[] | [.id, .title] | @tsv'
arena group contents <slug> --json | jq -r '.data[] | [.id, .type, .title] | @tsv'
```

`download` writes images, attachments, text blocks as Markdown `.txt`, link shortcuts, `<slug>.csv`, `index.html`, and `manifest.json`, and a rerun downloads only changed files from Are.na. `--type` limits written files and refuses `--prune`, `--link-format url` writes portable shortcuts, and connected channels stay shortcuts.

```bash
# Channel images on disk, run summary
arena download <slug> '.artifacts/search/arena/<slug>' --type Image --size medium --json \
    | jq -c 'select(.type == "completed") | .summary'
```

`channelDigest` returns channel counts, a type breakdown from up to 100 recent items, and active contributors in one call, and fits a large channel before a full `channel contents` read.

```text
channelDigest {"channel_id": "<slug>", "sample_size": 100}
```

## [03]-[WRITING]

Writes publish to the token's account, and public channels show them to every viewer. Delete commands confirm unless `--yes` is passed, and `--metadata` takes JSON or `key=value` pairs.

```bash
# Text or URL block at a position, with attribution
arena add <channel> '<value>' --title '<title>' --original-source-url '<url>' --insert-at 1 --json

# Local file or directory as blocks
arena upload '<file>' --channel <channel> --alt-text '<text>' --json
arena import <channel> --dir '<dir>' --recursive --json

# URLs with shared title, then batch state
arena batch <channel> '<url>' '<url>' --json
arena batch status <batch_id> --json

# Existing channel into another channel, then moved to the top
arena connect <channel-id> <channel> --type Channel --json
arena connection move <connection-id> --movement move_to_top --json
```

MCP writes take up to 20 channels in one call, each entry in `channels` with its own `position` and `metadata`. `batchCreateBlocks` takes per-block `title`, `description`, and source fields, where `arena batch` applies one title to every block.

```text
createBlock {"value": "<url>", "title": "<title>", "channels": [{"id": "<slug>", "position": 1}, {"id": "<slug>"}]}
createConnection {"connectable_id": <id>, "connectable_type": "Block", "channel_ids": ["<slug>", "<slug>"]}
batchCreateBlocks {"channel_ids": ["<slug>"], "blocks": [{"value": "<url>", "title": "<title>"}]}
```
