# [PLUGINS]

Each `plugins/<name>` directory is one plugin, listed in the marketplace file of each harness that loads one of its components.

## [01]-[LAYOUT]

Plugin folders hold the manifest, components at the root, and the code those components run:
- `.claude-plugin/plugin.json` holds `name`, `version` `1.0.0`, `description`, and `author`
- Harnesses load local marketplace plugins in place with no version pin
- Components sit at plugin root: `skills/<skill>/SKILL.md`, `agents/<agent>.md`, `commands/<command>.md`, `hooks/hooks.json`, `.mcp.json`, `.lsp.json`
- One invalid `.lsp.json` row skips every server of its file at load, `claude plugin validate` reads no `.lsp.json`
- `.lsp.json` rows point each server's cache at repository `.cache` through `args` or `settings`, `${CLAUDE_PROJECT_DIR}` expands in `args`
- Claude plugin configuration paths take `${CLAUDE_PLUGIN_ROOT}`, skill text paths are relative to the skill folder
- TypeScript plugins hold `package.json` and a `tsconfig.json` extending `../../tsconfig.base.json`, `pnpm install` adds them to the workspace
- `userConfig` values sit in `~/.claude/settings.json` under `pluginConfigs["<name>@<marketplace>"].options` alone

## [02]-[HARNESSES]

Claude Code and Codex load one plugin folder, each through its own manifest, marketplace row, and enable row:
- Function-hook `modules`, `agents/`, and `.lsp.json` load in Claude Code alone
- Plugins with Claude Code components alone take no Codex manifest
- Plugins loading different components in each harness state what each harness loads in that harness's manifest `description`
- Plugin-root `CLAUDE.md` loads as no installed plugin's context, `claude plugin validate --strict` fails it
- Guidance for a plugin's code sits in `.claude/rules/<name>.md` with `paths` over its folders, plugin-root `AGENTS.md` links to it
- Command hooks Codex alone runs sit in `.codex/hooks/`
- Codex runs a hook after `/hooks` trusts its config hash, an edited hook takes a new trust
- Codex reads `.codex-plugin/plugin.json` (`name`, `description`, `interface.displayName`), else `.claude-plugin/plugin.json`
- Servers and hooks one harness alone loads sit inline in its manifest, a root `.mcp.json` or `hooks/hooks.json` loads in both harnesses
- `plugins/.claude-plugin/marketplace.json` rows hold `name` and `source` `./<name>`, both harnesses read the one file
- Harnesses show each manifest's `description` in their marketplace listing
- Codex repository marketplaces take `[marketplaces.<marketplace>]` in `.codex/config.toml` with an absolute local source path
- Repository skills both harnesses run link from `.codex/skills/<skill>` to `../../.claude/skills/<skill>`
- Plugin skills take no `.codex/skills` link, Codex lists them once from the installed plugin copy
- Codex plugins load no agent, a plugin's Codex agents sit in `.codex/agents/<agent>.toml` as files, Codex spawns no symlinked agent
- `.claude/settings.json` enables a plugin as `"<name>@<marketplace>": true` under `enabledPlugins`
- `.codex/config.toml` enables a plugin as `[plugins."<name>@<marketplace>"]`, a copy `codex plugin add` writes to `~/.codex/config.toml` goes
- Codex sessions load the copy `codex plugin add <name>@<marketplace>` installs, a command naming a repository path reads the working tree
- `.mcp.json` and manifest `mcpServers` rows take `command` and no `type` for stdio in the session directory, `"type": "http"` and `url` for HTTP
- Manifest, marketplace, and enable rows of both harnesses change with the plugin folder in one commit

## [03]-[CONVERTING]

Components move into a plugin in one change, leaving no declaration at their old source:
- Skills, agents, and commands move by `git mv` into the plugin's component folders with frontmatter `name` unchanged
- MCP servers and settings hooks become rows of the plugin's `.mcp.json` and `hooks/hooks.json`, hook paths under `${CLAUDE_PLUGIN_ROOT}`
- Routing rows, agent `skills` lists, and skill cross-references name each moved skill as `<name>:<skill>`
