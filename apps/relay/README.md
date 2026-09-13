# [RELAY]

Relay keeps Claude and OpenAI account usage in the macOS menu bar. Add accounts in Settings, select an account to use its native client, and click its provider symbol to start an eligible subscription session. Session countdowns come from the provider’s reset time; Weekly and Fable retain their reported deadlines. Manual start is the default.

`pnpm nx run Relay:build` from the repository root builds the app, and Xcode's `Relay` scheme runs it.

Claude uses the installed Claude Code client. Selecting a Claude account transfers its native login grant into Claude Code’s shared default store. Existing CLI processes stay open: in-flight requests retain their original credentials, and later requests adopt the shared login through Claude’s native reload path, including its cache interval.

Claude’s native login and Relay’s account record do not share an atomic transaction. Relay reconciles an interrupted switch against the current native login; an interrupted credential rotation can require signing in again.

OpenAI uses the app-server bundled with the installed Codex desktop app, resolved by its bundle identifier. Each account has its own native Codex home and desktop data directory. Selecting it opens or focuses that account’s Codex instance and leaves other instances open. Relay checks the stored account identity through the native protocol before showing a selection. Quit that account’s Codex app before signing out, reconnecting, or removing it: a separate app-server cannot refresh the running desktop’s authentication cache. OpenAI’s app-server and desktop environment integration include experimental interfaces.

Starting a session sends one `hi` through the native subscription client using the least costly eligible model in Relay’s current provider policy. The deadline comes from the provider. An uncertain start remains pending across reopening; Relay checks usage without sending another greeting.

Adapted from [Franciskid/LLMCodeBar](https://github.com/Franciskid/LLMCodeBar) at commit `1a816cdb39f68d4eb07c5bdacd968fdd32aba98c`. The upstream MIT notice is preserved in [LICENSE](LICENSE).

Claude's symbol comes from [Claude](https://claude.com), OpenAI's from [Simple Icons](https://simpleicons.org) under CC0 1.0, and Relay's app artwork and menu bar symbol are original.
