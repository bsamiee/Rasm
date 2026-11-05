# [HARNESSES]

Host questions are case files run in the installed host, every run a trace agents read in place of probing the host.

## [01]-[CASES]

Cases sit in `<product>/proof/host/<name>.cases.json` as a probe (`evaluate`), a call (`call`, `args`), or a script run, each with a precondition the host's `precondition.ts` decodes:
- `node tools/harnesses/main.ts <host> <target>... [--release <prefix>]... [--only <case>]...` runs the case files under a folder
- `node tools/harnesses/main.ts <host> compare <target>... [--module <path>]` compares recorded host answers against a rebuilt module
- Case file `limit` bounds launch, a case writing no record, and the batch-end quit, `2 minutes` when absent
- Case `presses` answer each asking window in order with `accept`, `discard`, `cancel`, or a button label, a `blocking` window halting as a dialog only when its main-thread sample holds a modal frame
- Results land beside each case file as `<name>.<version>.results.json`, one per installed release unless `--release` names one
- Hosts are `acrobat`, `blender`, `illustrator`, `indesign`, `photoshop`, `rhino`
- Hosts read and launch through `@rasm/applications` over `watch-apps serve`, in the background, behind the owner's windows and visible, and reuse the one running instance unless their spec states `several`, which Blender's alone does
- Rhino's launch arguments and environment reach only an instance launch creates, so a running Rhino refuses the run

## [02]-[TRACE]

Each run writes `.artifacts/logs/harnesses/host=<host>/run=<id>/records.jsonl` and prints one summary line with its cases, failure signatures with the run each first failed in and its first later passing run, and instances:
- `node tools/harnesses/main.ts <host> log` prints the current run's records with reports linked since it ended
- Linked reports show crash exception and frames, hang duration and main-thread frames, spin lines, and harness host log errors inline
- Facts the trace lacks are harness defects fixed in `session.ts`, `report.ts`, or the host's `host.ts`, never read from the host by hand
