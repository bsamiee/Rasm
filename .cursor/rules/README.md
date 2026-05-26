# Rasm Cursor Rules

Project rules for the Rasm monorepo live in `.cursor/rules/*.mdc`. They are concise routing and invariant files; canonical procedures remain in `CLAUDE.md`, `AGENTS.md`, nested `AGENTS.md`, `.claude/skills/`, `docs/`, and bridge docs.

## Activation

| Rule mode | Where | Use |
| --- | --- | --- |
| Always | `rasm-core.mdc` | Repo router, required reads, quality rails. |
| Auto attached | Rule `globs` | File-family guidance when matching files are open. |
| Agent requested | Description with empty `globs` | Navigation guidance when useful. |
| User Rules | Cursor Settings | Optional personal bootstrap from `user-rules-bootstrap.txt`. |

Cursor precedence is Team > Project > User. This repo does not define Cursor skills because `.claude/skills/` owns full procedures.

## Rule Index

| File | Scope |
| --- | --- |
| `rasm-core.mdc` | Always-on router. |
| `rasm-monorepo-navigation.mdc` | Discovery and topology. |
| `rasm-csharp-production.mdc` | Production C# under `libs/`, `apps/`, `tools/`. |
| `rasm-csharp-tests.mdc` | C# specs, testkit, and Rhino scenarios. |
| `rasm-rhino-bridge.mdc` | Bridge operator and `*.verify.csx` files. |
| `rasm-bash-scripts.mdc` | Bash scripts. |
| `rasm-docs-markdown.mdc` | Markdown and Cursor docs. |
| `rasm-dependencies.mdc` | Package manifests and build props. |
| `rasm-analysis-domain.mdc` | `Rasm/Analysis` plus `Rasm/Domain`. |
| `rasm-vectors.mdc` | `Rasm/Vectors`. |
| `rasm-grasshopper.mdc` | GH2 components and UI boundary. |
| `rasm-rhino-ui.mdc` | Rhino/GH2/Eto UI rails. |
| `rasm-rhino-exchange-camera-blocks.mdc` | Rhino Exchange, Camera, Blocks. |

## Non-Rule Artifacts

- `.cursor/BUGBOT.md` gives PR-review expectations for Bugbot.
- `.cursor/research/rasm-cursor-infrastructure.md` captures the source-grounded synthesis behind this rule pack.
