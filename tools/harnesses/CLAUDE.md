# [HARNESSES]

Host questions are case files run in the installed host, every run a trace agents read in place of probing the host.

## [01]-[CASES]

Cases sit in `<product>/proof/host/<name>.cases.json` as a probe (`evaluate`), a call (`call`, `args`), a script run, or a panel (`panel`, `evaluate`), each with a precondition the host's `hosts/**/<host>/precondition.ts` decodes:
- `harness <host> <target>... [--release <prefix>]... [--only <case>]...` runs the case files under a folder, ExtendScript engines and InDesign's UXPScript bundle building through Vite first
- `harness <host> compare <target>... [--module <path>]` compares recorded host answers against a rebuilt module
- Case file `limit` bounds launch, a case writing no record, and the batch-end quit, `2 minutes` when absent
- Case `presses` answer each asking window in order with `accept`, `discard`, `cancel`, or a button label, a `blocking` window halting as a dialog only when its main-thread sample holds a modal frame, any other sample recorded as window busy and left to limit
- Results land beside each case file as `<name>.<version>.results.json`, for the newest installed release unless `--release` names others
- Hosts are `acrobat`, `blender`, `illustrator`, `indesign`, `photoshop`, `rhino`
- Hosts read and launch through `@rasm/applications` over `watch-apps serve` without activation, reusing the running instance unless their spec states `several` (Blender alone)
- Rhino's launch arguments and environment reach only an instance launch creates, so a running Rhino refuses the run
- InDesign panel cases build the batch `plugin` config to dial a link server on a free loopback port, add it for the session, and show `panel`
- Panel `evaluate`, `until`, and `read` run as `execute_script` over the panel `document` and `app`, `capture` writing each window to the case folder

## [02]-[TRACE]

Each run writes `.artifacts/logs/harnesses/host=<host>/run=<id>/records.jsonl` and prints one summary line with its cases, failure signatures with the run each first failed in and its first later passing run, and instances:
- `harness <host> log` prints the current run's records with reports linked since it ended
- Linked reports show crash exception and frames, hang duration and main-thread frames, spin lines, and harness host log errors inline
- Facts the trace lacks are harness defects fixed in `session/harness.ts`, `trace/reports.ts`, or the host's `host.ts`, never read from the host by hand
