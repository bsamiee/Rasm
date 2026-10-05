# [RESEARCH]

Research and archive folders hold gitignored material work reads, and repository builds read tracked files alone.

## [01]-[TREE]

`docs/research/` places each folder by the one distinction it adds, area, then subject, then content kind:
- Areas name a domain of work (applications, rendering, coding, a product the repository builds)
- Subjects name one application, source, or topic of their area, a source repository as `<owner>-<repo>`
- Subjects sharing one role inside an area group under a folder naming the role (`reference/`, `sources/`)
- Products extending a subject (extensions, plug-ins) take `extensions/<id>/` inside it in the subject form
- Kind folders separate a subject's content by form: `facts/`, `decompiled/`, `inventories/` (store dumps, string tables, presets), `captures/`
- Facts every subject of an area shares sit in the area's own `facts/`
- Levels adding no distinction fold into their parent
- Folder and file names state a product, module, or topic in lowercase hyphenated words, a facts file as `<topic>.txt`

## [02]-[USE]

Work on a matter the research holds reads that research and uses it as written, every time:
- Facts the work needs and the research lacks are read from source and join before code uses them
- Logic a decompiled source or archive shows is the starting point code takes as shown
- Names change to real software or domain terms and the project's form, and the change stays minor
- Sources showing one capability integrate into one stronger construct

## [03]-[FACTS]

Facts files are plain text with one fact per line and a title line naming their sections:
- Fact lines state in domain terms the member, key, value, order, limit, or answer code uses
- Narration, meta information, sources, citations, provenance, line or page references, paths, URLs, ids, dates, and plans stay out
- Content states the installed build alone, names and text free of version numbers
- New facts join the file owning their topic
- Corrections rewrite a wrong, weak, stale, superseded, or contradicted line whole in its place
- Overlapping lines merge into one line
- Validation and clean-prose passes run over repository files alone

## [04]-[SOURCES]

Facts come from source, its decompile, and documentation at the installed build:
- Behavior (poll order, event routing, draw and scale formulas) comes from source
- Documentation of the installed release decides over older posts and community sources
- Use `use-ghidra` for native code
- Use `search-code` for managed code, its project form writing one file per type into `decompiled/<assembly>/`
- Decompiles cover in full depth what the work reads, and that alone
- Tarballs and loose readme and license files are deleted from decompiled sources
- Project and configuration files of a source stay to show its packages and settings, repository files deciding every binding
- Decompiled sources hold the newest version alone, obsolete, legacy, and compatibility content deleted per file
- Version-conditional code stays where the functionality needs it

Acquisition of a product:
1. Install the product normally
2. Decompile what the work reads into its subject's `decompiled/`
3. Write facts per topic into `facts/`

## [05]-[PLANS_AND_ARCHIVES]

Plans and earlier code generations stay where they are:
- `plan/` holds plans and research for future projects and stays read-only
- `.archive/` folders at the repository root, `apps/creative-cloud/.archive/`, and `libs/typescript/.archive/` hold earlier generations of own code

## [06]-[CLEANUP]

Finished research and decompile work leaves the machine holding the research it wrote and what stood before:
- Old content, cache buildout, machine litter, installers, logs, history, and empty folders are deleted in full
- Applications, binaries, extensions, and plug-ins installed before the work stay
- Applications, extensions, and plug-ins a study installed are uninstalled with their user data, one kept as a tool excepted
- Running hosts hold what the interface declares alone
