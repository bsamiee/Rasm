# [PLUGINS]

Each `plugins/<name>` directory is one plugin, listed in the marketplace file of each harness that loads one of its components.

## [01]-[LAYOUT]

Plugin folders hold the manifest, components at the root, and the code those components run:
- `.claude-plugin/plugin.json` holds `name`, `version`, `description`, and `author`
- Components sit at plugin root: `skills/<skill>/SKILL.md`, `agents/<agent>.md`, `commands/<command>.md`, `hooks/hooks.json`, `.mcp.json`, `.lsp.json`
- Claude plugin configuration paths take `${CLAUDE_PLUGIN_ROOT}`, skill text paths are relative to the skill folder
- `CLAUDE.md` sits in the code folder it describes and loads as nested project memory when a file there is read
- TypeScript plugins hold `package.json` and a `tsconfig.json` extending `../../tsconfig.base.json`, `pnpm install` adds them to the workspace
- `userConfig` values sit in `~/.claude/settings.json` under `pluginConfigs["<name>@<marketplace>"].options` alone

## [02]-[HARNESSES]

Claude Code and Codex load one plugin folder, each through its own manifest, marketplace row, and enable row:
- Function-hook `modules`, `agents/`, and `.lsp.json` load in Claude Code alone, a plugin holding nothing else takes no Codex manifest
- Codex reads `.codex-plugin/plugin.json` (`name`, `version`, `description`, `interface.displayName`), else `.claude-plugin/plugin.json`
- Servers and hooks one harness alone loads sit inline in its manifest, a root `.mcp.json` or `hooks/hooks.json` loads in both harnesses
- `plugins/.claude-plugin/marketplace.json` rows hold `name`, `source` `./<name>`, and `description`, both harnesses read the one file
- Codex repository marketplaces take `[marketplaces.<marketplace>]` in `.codex/config.toml` with an absolute local source path
- Repository skills both harnesses run link from `.codex/skills/<skill>` to `../../.claude/skills/<skill>`
- `.claude/settings.json` enables a plugin as `"<name>@<marketplace>": true` under `enabledPlugins`
- `.codex/config.toml` enables a plugin as `[plugins."<name>@<marketplace>"]`, a copy `codex plugin add` writes to `~/.codex/config.toml` goes
- `.mcp.json` servers take `command` and no `type`, both harnesses start them as stdio in the session directory
- Manifest, marketplace, and enable rows of both harnesses change with the plugin folder in one commit

## [03]-[CONVERTING]

Components move into a plugin in one change, leaving no declaration at their old source:
1. `git mv .claude/<kind>/<item> plugins/<name>/<kind>/<item>` for each skill, agent, and command, with frontmatter `name` unchanged
2. MCP servers leave `.mcp.json` and `.codex/config.toml` for the plugin's `.mcp.json`
3. Settings hooks leave `.claude/settings.json` for `hooks/hooks.json` with `${CLAUDE_PLUGIN_ROOT}` paths
4. CLAUDE.md routing rows, agent `skills` lists, and skill cross-references name each moved skill as `<name>:<skill>`
5. `.claude/settings.json` permissions name a moved server's tools as `mcp__plugin_<name>_<server>__*`

## [04]-[VALIDATION]

Checks of added and converted plugins run at user request alone:
- `claude plugin validate plugins --strict` and `claude plugin validate plugins/<name> --strict`, every warning a failure
- `claude -p --debug 'Reply done'` then `rg '<name>' ~/.claude/debug/latest` shows the plugin's load lines
- `claude -p --debug-file <log>` with a Write of a file each changed `.lsp.json` server handles shows the server start
- `claude plugin validate` reads no `.lsp.json`, one invalid row skips every server of the file at load
- Codex sessions load the copy `codex plugin add <name>@<marketplace>` installs, a command naming a repository path reads the working tree
- `codex plugin list --marketplace <marketplace>`, `codex mcp list`, and `codex debug prompt-input` show the plugin, its servers, and its skills
- `codex --strict-config doctor --summary` validates both Codex config layers and lists merged servers
- Codex runs a hook after `/hooks` trusts its config hash, an edited hook takes a new trust
