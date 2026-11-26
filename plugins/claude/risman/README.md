# [RISMAN]

Risman (ریسمان, a cord twisted from finer threads) binds each agent to its own handle on every local MCP server built on `@rasm/mcp` and puts the events of the applications that agent drove into that agent's conversation.

## [01]-[HARNESSES]

Each harness reads its manifest under `.<harness>-plugin/plugin.json` and its own hook file:

| [INDEX] | [HARNESS]   | [HOOKS]                         | [DELIVERY]                                                       |
| :-----: | :---------- | :------------------------------ | :--------------------------------------------------------------- |
|  [01]   | Claude Code | `hooks/hooks.json` mod module   | Next model request, new turn for idle main, message for subagent |
|  [02]   | Codex       | `hooks/codex.json`              | Context after each tool call and prompt, one `Stop` continuation |
|  [03]   | Antigravity | `hooks.json`, `sidecars/follow` | System message before each model call, `agentapi send-message`   |

- Codex hooks wake no idle thread, as Codex starts no turn from a hook
- Codex runs plugin hooks only after `/hooks` trusts each changed definition
- Antigravity runs sidecars in the App alone, once `~/.gemini/config/config.json` enables `risman/follow`

## [02]-[INSTALL]

`nx install @rasm/risman` puts the build where each harness on this machine loads it: Claude Code through `CLAUDE_CODE_PLUGIN_DIRS`, Codex through an entry of the personal marketplace `~/.agents/plugins/marketplace.json` and `codex plugin add`, Antigravity through `agy plugin install`.

Releases publish `risman.zip` to the `@rasm/risman@<version>` GitHub release, which the `archive` row of `.claude-plugin/marketplace.json` names. Codex reads `local`, `url`, `git-subdir`, and `npm` rows alone, so the release row reaches Claude Code alone; the row switches to `{ "source": "npm" }` once npm transfers the `@rasm` scope, which both harnesses read.
