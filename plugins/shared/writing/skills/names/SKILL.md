---
name: names
description: "Use when naming or renaming a repository, product, app, package, command, or folder, covering identity and structural names"
---

# [WRITING_NAMES]

Kind is decided before a word is chosen: identity names belong to what people install, launch, search for, or cite, structural names belong to places inside one. Each kind fails in the other's form.

## [01]-[IDENTITY]

Identity names pass each test:
- True: meaning is the subject, its defining act, or an object related to it in a way one sentence states
- Specific: meaning fits no neighboring product, and holds after scope grows, code is rewritten, or an upstream is renamed
- Distinct: name beside its field word returns no other product in search, and no trademark in that field holds it
- Sayable: 1 to 3 syllables and at most 9 letters, spelled after one hearing
- Typed: one lowercase ASCII word serves each id the product has (package, import, command, URL), capitalized in prose alone
- Clean: dictionary lookup confirms meaning, with no vulgar, religious, or political sense in another major language
- Free: exact id is open on each registry of publication and collides with no common binary, a domain can add a field suffix

Daily commands can take a short form of 2 to 4 letters (`rg`).

Sources, strongest first:
- Word for the subject in another language (`youki`, `Kysely`, `Knip`), from a language of the owner, of the field, or in rare use
- Object from the subject's field, or from a craft with matching structure (`Zellij`, `Hanko`)
- Plain concrete noun, or a joined pair (`Garage`, `Iceberg`, `Ladybird`)
- Real word changed by one visible step (`Traefik`, `Vitess`), where its unchanged form passes True and fails Sayable, Distinct, or Free
- Acronym with an expansion that states the subject (`witr`)
- Related products share a source where words relate as products do (`Sunshine` host, `Moonlight` client), otherwise form alone

Forms that fail:
- Fits any product: `forge`, `foundry`, `atelier`, `nexus`, `hub`, `studio`, `core`, `engine`, `relay`
- Description: `<verb>-<noun>`, `<field>-tool`, `<adjective><noun>`
- Formula affix: `open-`, `super-`, `turbo-`, `tiny-`, `-ify`, `-ly`, `-kit`, `-craft`, `-flow`, `-ai`
- Borrowed identity: another product, vendor, language, or version inside a name
- Borrowed theme: stock theme of a field (nautical for containers), or a language picked for sound over meaning
- Unanchored: invented syllables, mascot, private reference

Each row names one subject with a failing and a passing name:

| [INDEX] | [SUBJECT]                       | [FAILS]                      | [PASSES]                | [REASON]                               |
| :-----: | :------------------------------ | :--------------------------- | :---------------------- | :------------------------------------- |
|  [01]   | Query builder                   | `open-querykit`              | `Kysely`                | Finnish word for query, no affix       |
|  [02]   | Tracer of a process's cause     | `ProcessInsight`             | `witr`                  | Acronym of "why is this running"       |
|  [03]   | Stream host and its client      | `StreamCore`, `StreamClient` | `Sunshine`, `Moonlight` | Client reflects what host emits        |
|  [04]   | Request router                  | `Rahdar`                     | `Hashti`                | Road keeper also means highwayman      |
|  [05]   | Infrastructure engine           | `Zirsa`                      | `Zirbana`               | Clipped while whole word was free      |
|  [06]   | Layout built from few shapes    | `LayoutKit`                  | `Girih`                 | Five tile shapes compose every pattern |
|  [07]   | Message bus under every service | `DataFlow`                   | `Qanat`                 | Buried channel feeds all above it      |
|  [08]   | Platform base                   | `Substrate`                  | `Oinarri`, `Darsena`    | Basque foundation, and a harbor basin  |

## [02]-[STRUCTURE]

Folders, categories, modules, and packages inside a product take plain terms their field and spec use:
- Packages with a registry id are `<product>` then field terms broad to narrow, in ecosystem case and separator (`<product>-<language>-parser`)
- Closing words name a part's role to its consumer (`cli`, `client`, `worker`, `types`), status words (`impl`, `internal`, `misc`) fail
- Mechanism words (`runtime`) close a name only where the field uses them as that part's term
- One word names one concept across languages, and one axis orders one directory level
- Children omit their parent's word, except a folder spelled as its registry id (`crates/<product>-cli`)
- Parts take an identity name once used alone outside their parent (`wry`), elsewhere identity words appear as a registry id prefix alone

Each row names one place with a failing and a passing name:

| [INDEX] | [FAILS]                      | [PASSES]                   | [REASON]                              |
| :-----: | :--------------------------- | :------------------------- | :------------------------------------ |
|  [01]   | `graphs/graph-layout`        | `graphs/layout`            | Child repeats parent                  |
|  [02]   | `<product>-runtime`          | `<product>-worker`         | Consumers add a worker, not a runtime |
|  [03]   | `apps/cli`, `apps/developer` | `apps/cli`, `apps/desktop` | Form and audience on one level        |
|  [04]   | `<product>/foundry`          | `<product>/build`          | Figurative word inside structure      |

## [03]-[PROCESS]

1. State subject and defining act in one sentence
2. Collect candidates from every source past its obvious first answers, then run tests from lookups, never memory
3. Present survivors with meaning, source, flags, and availability, state one pick, and record its origin and pronunciation in README
4. Rename only where a test fails on fact (holder, trademark, collision), with every reference updated and its old id kept as a redirect
