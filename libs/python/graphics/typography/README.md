# rasm-typography

Builds a declared set of Persian and Arabic font faces from their original binaries through fontTools, ufo2ft, HarfBuzz, and skia-pathops, and verifies each build against the requirements its genre states.

The `foundry` command runs one action over a set declaration (a TOML file) and every face a name selects in it:

```sh
foundry build set.toml --output build
foundry run set.toml Naskh --output build
foundry publish set.toml Naskh dist --output build
```

| Action    | Effect                                                        |
| :-------- | :------------------------------------------------------------ |
| `source`  | Writes each face's UFO source from its original               |
| `build`   | Compiles each face whose inputs changed since its last build  |
| `check`   | Verifies each face's current build                            |
| `approve` | Records each face's shaping as approved                       |
| `run`     | Builds and verifies each face                                 |
| `publish` | Copies each verified build into a destination as TTF and WOFF2 |

Each command prints one JSON line per face, or one line per fault on stderr with a `sysexits` status.

## Requirements

- Python 3.15 or later
- [fontspector](https://github.com/fonttools/fontspector) on `PATH` for `check`, `run`, `approve`, and `publish`; it ships as a crate (`cargo install fontspector`) and as release binaries, not on PyPI
- Network access on the first build, which downloads `ArabicShaping.txt` of Unicode 18.0.0 into the user cache and checks its SHA-256
