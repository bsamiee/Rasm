"""Preserve native Markdown references through the prose CLI."""

lazy from pathlib import Path
lazy import subprocess
lazy import sys

lazy from markdown_it import MarkdownIt
lazy from markdown_it.tree import SyntaxTreeNode

# --- [OPERATIONS] -----------------------------------------------------------------------


def _reference_state(parser: MarkdownIt, text: str) -> tuple[tuple[tuple[str, ...], ...], tuple[tuple[str, dict[str, str | int | float]], ...]]:
    """Definition source blocks and ordered native link/image attributes."""
    lines = text.splitlines()
    tree = SyntaxTreeNode(parser.parse(text))
    definitions = tuple(tuple(lines[slice(*span)]) for node in tree.walk() if node.type == "definition" and (span := node.map) is not None)
    edges = tuple((node.type, node.attrs) for node in tree.walk() if node.type in {"link", "image"})
    return definitions, edges


def test_reference_fix(tmp_path: Path) -> None:
    """Preserve reference syntax, targets and definitions through batched fixes and idempotence."""
    alpha, beta, gamma = (f"https://example.test/{name}" for name in ("alpha", "beta", "gamma"))
    cases = (
        ((), ((f"[a]: {alpha}",), (f"[b]: {beta}",)), (), ()),
        (("[Alpha][a] and [Beta][b]",), ((f"[a]: {alpha}",), (f"[b]: {beta}",)), ("[Alpha][a]", "[Beta][b]"), (alpha, beta)),
        (("[a][] and [b][]",), ((f"[a]: {alpha}",), (f"[b]: {beta}",)), ("[a][]", "[b][]"), (alpha, beta)),
        (("[a] and [b]",), ((f"[a]: {alpha}",), (f"[b]: {beta}",)), ("[a]", "[b]"), (alpha, beta)),
        (("![Image][a] and ![b][] and ![c]",), ((f"[a]: {alpha}",), (f"[b]: {beta}",), (f"[c]: {gamma}",)), ("![Image][a]", "![b][]", "![c]"), (alpha, beta, gamma)),
        ((r"[Escaped \[label\]][a] and [b\]]",), ((f"[a]: {alpha}",), (rf"[b\]]: {beta}",)), (r"[Escaped \[label\]][a]", r"[b\]]"), (alpha, beta)),
        (("[Line][line label] and [Next][b]",), (("[line", " label]:", f"  <{alpha}>", '  "Title', '   continuation"  '), (f"[b]: {beta}",)), ("[Line][line label]", "[Next][b]"), (alpha, beta)),
        (("> [Quoted][a]",), ((f"> [a]: {alpha}",),), ("[Quoted][a]",), (alpha,)),
        (("- [Listed][a]",), ((f"  [a]: {alpha}",),), ("[Listed][a]",), (alpha,)),
        (("[a]",), ((f"[a]: {alpha}",), (f"[A]: {beta}",)), ("[a]",), (alpha,)),
        (("[a][unfinished and [a](not closed",), ((f"[a]: {alpha}",),), ("[a][unfinished", "[a](not closed"), (alpha, alpha)),
        ((f'[Inline]({beta} "Title") and [Reference][a]',), ((f"[a]: {alpha}",),), (f'[Inline]({beta} "Title")', "[Reference][a]"), (beta, alpha)),
        (("**[Bold][a]** and [**Strong**][] and [`code`][a]",), ((f"[a]: {alpha}",), (f"[**Strong**]: {beta}",)), ("[Bold][a]", "[**Strong**][]", "[`code`][a]"), (alpha, beta, alpha)),
        (
            ("## [Heading][r]", "", "[R]", "", "- [R] continuation", "", "| [Header][r] | Value |", "| --- | --- |", "| [Cell][r] | [R][] |"),
            ((f"[r]: {alpha}",),),
            ("[Heading][r]", "[R]", "[Header][r]", "[Cell][r]", "[R][]"),
            (alpha, alpha, alpha, alpha, alpha, alpha),
        ),
        (("[![Image](https://example.test/a])][r]",), ((f"[r]: {alpha}",),), ("[![Image](https://example.test/a])][r]",), (alpha, "https://example.test/a%5D")),
        ((f'[![Image]({beta} "title ] bracket")][r]',), ((f"[r]: {alpha}",),), (f'[![Image]({beta} "title ] bracket")][r]',), (alpha, beta)),
        (("[![Image][b]][a]",), ((f"[a]: {alpha}",), (f"[b]: {beta}",)), ("[![Image][b]][a]",), (alpha, beta)),
        (("- [01]-[foo-bar] Text",), ((f"[foo-bar]: {alpha}",),), ("[01]-[foo-bar] Text",), (alpha,)),
        (("- [1]-[body] See [Reference][a]",), ((f"[a]: {alpha}",),), ("[01]-[BODY]: See [Reference][a]",), (alpha,)),
    )
    parser = MarkdownIt("commonmark", {"inline_definitions": True, "store_labels": True}).enable("table")
    documents = []
    for index, (body, definitions, uses, targets) in enumerate(cases):
        rows = (*(*block for block in definitions),)
        for side, (prefix, suffix) in enumerate(((rows, ()), ((), rows))):
            text = "\n".join(("# [PAGE]", "", *prefix, "", *body, "", *suffix))
            before = _reference_state(parser, text)
            assert before[0] == definitions
            path = tmp_path / f"references {index}-{side}.md"
            path.write_text(text, encoding="utf-8")
            documents.append((path, before, uses, targets))

    script = Path(__file__).resolve().parents[3] / ".claude/skills/clean-prose/scripts/prose.py"
    command = (sys.executable, str(script), "fix", *(str(path) for path, _, _, _ in documents))
    fixed = subprocess.run(command, text=True, capture_output=True, check=False)
    assert fixed.returncode == 0, fixed.stdout + fixed.stderr
    for path, before, uses, targets in documents:
        assert tuple(attrs["href" if kind == "link" else "src"] for kind, attrs in before[1]) == targets
        text = path.read_text(encoding="utf-8")
        assert _reference_state(parser, text) == before
        assert all(use in text for use in uses)
    snapshots = tuple(path.read_bytes() for path, _, _, _ in documents)
    repeated = subprocess.run(command, text=True, capture_output=True, check=False)
    assert repeated.returncode == 0, repeated.stdout + repeated.stderr
    assert tuple(path.read_bytes() for path, _, _, _ in documents) == snapshots
