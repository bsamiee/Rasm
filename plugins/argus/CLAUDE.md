# [ARGUS]

Each harness reaches the language servers `.lsp.json` declares through its own manifest:
- Claude Code loads `.lsp.json` through `.claude-plugin/plugin.json` and runs each server natively
- Codex loads an HTTP MCP server and `PostToolUse` and `Stop` hooks calling it through `.codex-plugin/plugin.json`
- `argus` launchd row in `mise.toml` runs `main.ts` as that server
- `.lsp.json` changes reach both harnesses, `main.ts` and `.codex-plugin/plugin.json` changes reach Codex alone
