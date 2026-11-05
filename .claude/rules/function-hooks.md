---
paths:
  - "plugins/function-hooks/**"
---

# [FUNCTION_HOOKS]

Hooks correct wrong commands and track app instances each session owns, a Node server deciding for Claude Code and `codex.ts` per call for Codex.

- Use `plugin-authoring` skill for writing or changing `hooks/register.ts`

## [01]-[TOOL_CALLS]

Hooks fix a wrong call and let it run, the model keeps working:
- ALWAYS rewrite a call when its repository form follows from the call, and refuse only when no form follows
- ALWAYS add a command as a `programs` row in each policy with a fix for it, one row holding its grammar, `shell/programs.ts` holding rows with no fix
- ALWAYS take grammar from `carapace <program> export` or a dump, `flags` matching it to the installed binary
- ALWAYS state a grammar neither supplies in `flags` and per-subcommand `commands`, listing each option taking a value or read by a row
- ALWAYS route a checker call with options to the target its group forwards to, options on `--args`, a call no target reproduces staying direct
- ALWAYS name an option by any spelling in a row, `Site.has` resolves it through the grammar
- ALWAYS read shell structure from `shfmt --to-json` and Nx targets from `readCachedProjectGraph`, a target's commands deciding its route
- Rewrites edit the call's own command, in a nested body or script a rewrite with a notice becomes a refusal naming it
- Calls the hook cannot parse or judge are refused
- App rows stop harm to the user's desktop and other sessions' processes
- Products under test run as themselves, background-only and `LSUIElement` bundles included
- `osascript` scripting a `tools/harnesses/<host>` app past a launch is refused toward `harness <host> <target>`, a product under test exempt
- Computer-use tools and `execute_script` bodies run unchecked, an agent's `request_access` alone refused for waiting on a person with no timeout

## [02]-[OUTPUT]

Texts reaching a model or harness appear only when they change the next action:
- ALWAYS write one sentence stating the fact, then after a comma the action naming the replacement, under 10 words and passing `clean-prose`
- ALWAYS build a text from its values and point to the owner (skill, file, target) of the rest
- `context` is a note Claude reads after a tool result, and users never see it

## [03]-[RUNTIME]

`hooks/register.ts` runs in the engine with no DOM and no Node, everything else under Node with Effect:
- ALWAYS limit `register.ts` to the server, its calls, and store rows, importing types alone from `claude-code` and `decision.ts`
- ALWAYS pass `$` whole to functions declared at top level of `register.ts`, any other use of `$` fails module load
- ALWAYS give a gating registration its `.catch`, a hook that fails otherwise lets the call run
- ALWAYS write `decision.ts` against engine types alone, `typecheck:claude` checks it under them
- `server.ts` starts once per Claude Code process and exits when its host dies or changed sources decide a probe call, the next call starting a new one
- Codex runs a call unless its hook prints deny JSON or exits 2 with stderr, `codex.ts` encoding every failure as a denial behind `|| exit 2`
- Subagent `turn.complete` and `session.end` send each owned instance a plain quit, `--force` left to an agent's command
- `session.end` runs its quits in one detached `sh`, the one form outliving its 1.5 s limit
- Sessions run one `watch-apps watch` per app they hold or drive through an MCP server, its notes reaching the owning agent through `$.session.send`, else the session through `$.prompt.submit`
- Modules that fail to load register no hook, every call running unchecked
