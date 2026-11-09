# rasm-records

Typed results and record storage over [Expression](https://pypi.org/project/expression/) and [msgspec](https://pypi.org/project/msgspec/):

- `Fault`, `Outcome`, `Reason`, and `paired` or `accumulated` to combine independent results and keep every fault
- Bounded field types (`Unit`, `Positive`, `Count`, `Name`) that every msgspec decode enforces
- `RecordFile` to read and write one JSON or TOML record, `committed` for atomic file and folder writes, and `content_hash` for stable hashes over canonical CBOR
- `Tree`, `diff`, and `merge` for keyed record trees and the edits between two of them

```python
from pathlib import Path

import msgspec
from rasm_records import Name, Outcome, RecordFile


class Settings(msgspec.Struct, frozen=True, kw_only=True):
    title: Name


saved: Outcome[Path] = RecordFile(path=Path("settings.toml"), form=Settings).written(Settings(title="Draft"))
```

Requires Python 3.13 or later.
