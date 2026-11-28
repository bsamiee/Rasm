# [LANGUAGE]

<Language> targets hold `<extensions>` files, `<project>` naming the folder holding `<manifest>`, `<paths>` files a pass touches, and `<dir>` session scratch folder.

## [01]-[SKILLS]

| [INDEX] | [TARGET]       | [SKILLS]               | [CONDITION]                 |
| :-----: | :------------- | :--------------------- | :-------------------------- |
|  [01]   | Any <Language> | `<language>:coding`    | `<extension>` file          |
|  [02]   | Tests          | `<language>:testing`   | Suite under `tests/`        |
|  [03]   | <Package>      | `<language>:<package>` | Manifest naming `<package>` |
|  [04]   | <Host>         | `<host>:coding`        | Code a host loads           |

## [02]-[SOURCES]

<Language> targets read their core packages, standard library, runtime, and host SDK at the version the project resolves. Package reads, one command per step:
1. Guide: core package's agent guide or entry-point index, read whole
2. Index: modules or namespaces with member counts and summaries
3. Inventory: every member of each chosen group, written to a file and counted
4. Standard library: members of the standard library and built-in types the target restates
5. Host: SDK declarations the target calls or restates
6. Workspace: repository libraries the target imports, resolved to their source
7. Catalog: central manifest candidates no project declares, read through step 3

```bash
<command per step>
```

## [03]-[MAPPING]

Maps list what each file of the target declares and exports and who reads it, one command per step, each run once over `<paths>`:
1. Exports: exported names per file with their kind and visibility level
2. Name families: names differing by case, plural, or a joined builder or runner word
3. Constants: exported constants, each traced to its reader and the source that should state it
4. Readers: reader sites by symbol identity through the language's compiler tools
5. Text: sites spelling a member, key, path, or message, and mentions across docs, skills, and manifests
6. Like work: clone pairs touching the target, ranked by lines times similarity
7. Nesting: depth per site past one level, deepest first, through the linter the repository runs

```bash
<command per step>
```

Commands read by their own rules:
- <Command>: <exit code, output shape, or limit a reader needs>

## [04]-[MECHANISMS]

<Language> <its means of stating one concept once>, so reductions here <how they mostly remove a declaration>:
- <Mechanism>: <form and what it replaces>

Host rules bind where a host checks the code, each proved by one probe of its check over a fresh build:
- <Host>: <rule and the form it forces>

## [05]-[PATTERNS]

Patterns common in <Language>, each a lead the map confirms before any rebuild, its rebuild holding under the condition beside it:

| [INDEX] | [PATTERN] | [LOOK_FOR]                | [REBUILD]       |
| :-----: | :-------- | :------------------------ | :-------------- |
|  [01]   | <Pattern> | <What a whole read shows> | <Language form> |

Patterns hold under these conditions:
- Row [01] -> <case where the rebuild does not hold>

## [06]-[CHECK]

Check per pass, each command its own background call:

```bash
hk fix <paths>
nx affected -t check --files=<paths> --exclude=rasm
```

- Scope: root project `rasm` runs repository-wide checks, which fail on other work in a shared tree, and a pass excludes it
- Manifest: `<paths>` holding `<manifest>` or `<lock>` -> same `nx affected` line without `--exclude=rasm`
- Host: <host validator over a fresh build>
