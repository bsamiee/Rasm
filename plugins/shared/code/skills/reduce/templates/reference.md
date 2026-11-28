# [LANGUAGE]

<Language> targets hold `<extensions>` files of projects `<workspace file>` lists. Placeholders: `<project>` names project folder holding `<manifest>`, `<paths>` files each pass touches, `<dir>` session scratch folder, and `<roots>` folders holding <Language> code.

## [01]-[SKILLS]

Coding and core package skills load for every target, whatever it imports, and other package skills load for each job the target does by hand:

| [INDEX] | [TARGET]       | [SKILLS]                                    | [CONDITION]                                |
| :-----: | :------------- | :------------------------------------------ | :----------------------------------------- |
|  [01]   | Any <Language> | `<language>:coding`, `<language>:<package>` | Every target, whatever it imports          |
|  [02]   | <Job>          | `<language>:<package>`                      | <Job> written by hand                      |
|  [03]   | <Host>         | `<host>:coding`                             | Imports `<host module>`                    |
|  [04]   | Tests          | `<language>:testing`, `code:test`           | Suite under `tests/`, or pass changing one |

`<language>:coding` states core package forms across its references, each read whole with it.

## [02]-[SOURCES]

<Language> targets read core packages, standard library, and host SDK every session at the release their project resolves, whatever they import, as most hand-rolled code in this repository restates one of them. Package reads, one command per step:
1. Version: release `<lock>` resolves for each core package, never a cache folder holding more than one
2. Guide: each core package's agent guide, else its entry-point index, read whole
3. `<package>`: inventory of every public module, for <jobs targets do by hand>
4. `<package>`: inventory of each module naming a target job's field, read whole
5. Standard library: members of standard library and built-in types the target restates
6. Host: SDK declarations the target calls or restates
7. Workspace: repository libraries the target imports, each resolved to its source and read whole
8. Central manifest: `<manifest>` listed whole, each package relevant to the target's jobs added where it fits and its API read deeply

```bash
<command per step>
<command listing central manifest whole>
```

Reads hold under conditions below:
- <Condition>: <rule a read follows>

## [03]-[MAPPING]

Maps list what each file of the target declares and exports and who reads it, one command per step, each run once over `<paths>`:
1. Levels: declarations per file at each visibility level, the list a pass narrows
2. Name families: names differing by case, plural, or a joined builder or runner word, each family read whole before judging it
3. Constants: exported constants, each traced to its readers and to the source owning its value
4. Importers: files importing each unit module and members each reads, by symbol identity through compiler tools
5. Text: sites spelling a member, key, path, or message a declaration holds, and mentions across docs, skills, and manifests
6. Like work: clone pairs touching the target across `<roots>`, ranked by lines times similarity
7. Nesting: block depth per site past one level, deepest first

```bash
<command per step>
rg -nw '<name>' .
jscpd --config .config/jscpd.json --similarity 0.6 --min-nodes 60 --format <format> --reporters json --output <dir>/jscpd <roots>;
    jq -r --arg t "$PWD/<target>/" '[.duplicates[] | select((.firstFile.name | startswith($t)) or (.secondFile.name | startswith($t)))] | sort_by(-(.lines * .similarity))[]
    | "\(.lines)\t\(.similarity)\t\(.firstFile.name):\(.firstFile.start)\t\(.secondFile.name):\(.secondFile.start)"' <dir>/jscpd/jscpd-report.json
<nesting command>
```

Command outputs read by their own rules:
- <Step>: <exit code, output shape, or limit a reader needs>
- Clones: jscpd exits 1 whenever a pair prints, and the ranking reads its report regardless
- Nesting: <levels the checker counts, its exit code on a site past the limit>

## [04]-[MECHANISMS]

<Language> <states one concept once through its own means>. Most cuts delete a declaration by <its usual removal>. Root `<settings file>` decides which forms compile: <setting and forms it accepts or rejects>.

[NAMES]:
- <Mechanism>: <form and what it replaces>

[DERIVATION]:
- <Mechanism>: <form and what it replaces>

[RESULTS]:
- <Mechanism>: <form and what it replaces>

[<GROUP>]:
- <Mechanism>: <form and what it replaces>

Host rules apply where a host or compiler checks code, each proved by one probe build:
- <Host>: <rule and form it forces>

## [05]-[PATTERNS]

<Language> code repeats patterns below, each a candidate the map confirms before any rebuild. Rows hold what no repository checker (analyzer, lint, ast-grep rule) enforces, from language nature first and repository code second:

| [INDEX] | [PATTERN] | [LOOK_FOR]                | [REBUILD]       |
| :-----: | :-------- | :------------------------ | :-------------- |
|  [01]   | <Pattern> | <What a whole read shows> | <Language form> |

Rows hold under conditions below:
- Row [01] -> <code the rebuild leaves in place, with its reason>

## [06]-[CHECK]

Checks run per pass, each command in its own background call, with `--files` taking `<paths>` joined by commas:

```bash
hk fix <paths>
nx affected -t check --files=<paths> --exclude=rasm
<host validator over a fresh build>
```

- Scope: passes exclude root project `rasm`, as its repository-wide checks fail on other work in a shared tree
- Build: `check` runs <targets> of each project a change reaches, dependents included
- Manifest: `<paths>` holding `<manifest>` or `<lock>` -> same `nx affected` line without `--exclude=rasm`
- Host: `<host>` code in `<paths>` -> validator over its fresh build
