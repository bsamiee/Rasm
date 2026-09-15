"""Derive a panel/dock layout from an ax-dump window tree.

Adobe's Illustrator tree is flat: every panel tab, every panel control and every
toolbar button is a direct child of AXWindow carrying an absolute AXFrame. So the
layout is recovered geometrically, not structurally.

Two traps in the real data drive the rules here:

* An ``AXTabGroup`` is a single panel tab chip, not a container. Chips that share a
  y form one tab row, which is one panel group.
* A collapsed or scrolled-out group reports a negative y or a zero size, and one
  Photoshop popup reports a frame far outside its window. Such nodes are recorded
  under ``offscreenTabs`` and ``rejected`` rather than plotted.

Anything that cannot be attributed to a panel goes to ``unattributed``. Nothing is
attributed to a nearest guess.
"""

import json
import sys

WATCH = ("AI", "Assistant", "Generative", "Discover", "Learn",
         "Comments", "Share", "Home", "Firefly", "Whats", "WhatsNew")

TAB_ROLE = "AXTabGroup"
COLUMN_GAP = 60      # px between distinct dock columns
ROW_TOLERANCE = 6    # px of y jitter still counted as one tab row


def flatten(node, out):
    """Collects every node of the tree into a flat list."""
    out.append(node)
    for child in node.get("children", ()):
        flatten(child, out)
    return out


def label(node):
    """Adobe puts the human-readable name in AXDescription far more often than AXTitle."""
    for key in ("AXDescription", "AXTitle", "AXValue", "AXRoleDescription"):
        value = node.get(key)
        if isinstance(value, str) and value.strip():
            return value.strip()
    return ""


def frame(node):
    f = node.get("AXFrame")
    if not isinstance(f, dict):
        return None
    return (f["x"], f["y"], f["w"], f["h"])


def inside(f, win, margin=200):
    x, y, w, h = f
    wx, wy, ww, wh = win
    return (wx - margin <= x <= wx + ww + margin
            and wy - margin <= y <= wy + wh + margin)


def cluster_columns(nodes):
    """Groups nodes into dock columns by gaps in their x positions."""
    xs = sorted({round(frame(n)[0]) for n in nodes})
    if not xs:
        return []
    groups, current = [], [xs[0]]
    for x in xs[1:]:
        if x - current[-1] > COLUMN_GAP:
            groups.append(current)
            current = [x]
        else:
            current.append(x)
    groups.append(current)
    return [(g[0], g[-1]) for g in groups]


def derive(dump):
    windows = dump.get("windows") or []
    if not dump.get("ok") or not windows:
        return {"ok": False, "error": dump.get("error", "no windows"),
                "note": "AX returned nothing for this application"}

    win = windows[0]
    win_frame = frame(win) or (0, 0, 0, 0)
    nodes = flatten(win, [])

    placed, rejected, offscreen = [], [], []
    for n in nodes:
        f = frame(n)
        if f is None:
            continue
        if not inside(f, win_frame):
            rejected.append({"path": n["path"], "role": n.get("AXRole"),
                             "label": label(n), "frame": f,
                             "reason": "frame outside window bounds"})
        elif f[2] == 0 or f[3] == 0 or f[1] < win_frame[1]:
            offscreen.append({"path": n["path"], "role": n.get("AXRole"),
                              "label": label(n), "frame": f,
                              "reason": "zero size or above window top: collapsed or scrolled out"})
        else:
            placed.append(n)

    # Toolbar: the narrow button column hard against the window's left edge.
    # The window's own close/minimise/zoom buttons sit in exactly that band, so
    # they are excluded by subrole rather than by guessing at a y threshold.
    CHROME = {"AXCloseButton", "AXMinimizeButton", "AXZoomButton", "AXFullScreenButton"}
    toolbar = sorted(
        (n for n in placed
         if n.get("AXRole") == "AXButton"
         and n.get("AXSubrole") not in CHROME
         and frame(n)[0] < win_frame[0] + 70
         and frame(n)[2] <= 40),
        key=lambda n: (frame(n)[1], frame(n)[0]))
    toolbar_paths = {n["path"] for n in toolbar}

    # Dock columns are built from visible tab chips only.
    tabs = [n for n in placed if n.get("AXRole") == TAB_ROLE and label(n)]
    columns = []
    for lo, hi in cluster_columns(tabs) if tabs else []:
        col_tabs = [t for t in tabs if lo <= frame(t)[0] <= hi + 400]
        rows = {}
        for t in col_tabs:
            key = round(frame(t)[1] / ROW_TOLERANCE)
            rows.setdefault(key, []).append(t)

        col_left = min(frame(t)[0] for t in col_tabs)
        col_right = max(frame(t)[0] + frame(t)[2] for t in col_tabs)
        # Content that sits in this column's x band and is not a tab or a tool.
        content = [n for n in placed
                   if n["path"] not in toolbar_paths
                   and n.get("AXRole") != TAB_ROLE
                   and col_left - 20 <= frame(n)[0] <= col_right + 320]

        groups = []
        row_ys = sorted(min(frame(t)[1] for t in r) for r in rows.values())
        for key in sorted(rows, key=lambda k: min(frame(t)[1] for t in rows[k])):
            row = sorted(rows[key], key=lambda t: frame(t)[0])
            top = min(frame(t)[1] for t in row)
            below = [y for y in row_ys if y > top + ROW_TOLERANCE]
            bottom = below[0] if below else win_frame[1] + win_frame[3]
            body = [n for n in content
                    if top + ROW_TOLERANCE < frame(n)[1] < bottom]
            # The front tab is the one whose group actually has content under it.
            groups.append({
                "tabs": [label(t) for t in row],
                "frontTab": label(row[0]) if body else None,
                "tabRowY": top,
                "pixelHeight": round(bottom - top),
                "contentNodes": len(body),
                "controls": [
                    {"role": n.get("AXRole"), "label": label(n),
                     "value": n.get("AXValue"), "frame": frame(n)}
                    for n in body
                    if n.get("AXRole") in ("AXButton", "AXTextField", "AXCheckBox",
                                           "AXRadioButton", "AXPopUpButton", "AXComboBox",
                                           "AXSlider", "AXIncrementor")
                ][:40],
            })
        columns.append({
            "xRange": [col_left, col_right],
            "groupCount": len(groups),
            "groups": groups,
        })
    columns.sort(key=lambda c: c["xRange"][0])

    attributed = {c["path"] for col in columns for g in col["groups"] for c in []}
    footer_y = win_frame[1] + win_frame[3] - 60
    footer = [{"role": n.get("AXRole"), "label": label(n), "value": n.get("AXValue"),
               "frame": frame(n)}
              for n in placed if frame(n)[1] >= footer_y and label(n)]

    watch = [{"path": n["path"], "role": n.get("AXRole"), "label": label(n),
              "frame": frame(n)}
             for n in placed
             if any(w.lower() in label(n).lower() for w in WATCH)]

    flyouts = [{"path": n["path"], "label": label(n)}
               for n in placed if n.get("AXRole") == "AXMenuButton"]

    # The dock model is Adobe's panel metaphor. A window with only a couple of tab
    # chips is a different shape (a web view, or bare chrome), and saying
    # "dock columns" about it would be a mis-attribution, so it is flagged instead.
    dock_applies = len(tabs) >= 3

    return {
        "ok": True,
        "windowFrame": {"x": win_frame[0], "y": win_frame[1],
                        "w": win_frame[2], "h": win_frame[3]},
        "dockModelApplies": dock_applies,
        "dockModelNote": ("panel dock derived geometrically from AXTabGroup chips"
                          if dock_applies else
                          "fewer than three tab chips: this window is not an Adobe panel dock, "
                          "columns below are reported only as found"),
        "totalNodes": len(nodes),
        "placedNodes": len(placed),
        "toolbar": [{"order": i, "label": label(n),
                     "selected": n.get("AXValue") == "Selected",
                     "frame": frame(n)}
                    for i, n in enumerate(toolbar)],
        "dockColumns": columns,
        "footer": footer,
        "rulersVisible": any(n.get("AXRole") == "AXRuler" for n in nodes),
        "scrollBars": sum(1 for n in placed if n.get("AXRole") == "AXScrollBar"),
        "panelFlyoutMenuButtons": flyouts,
        "flyoutNote": ("no AXMenuButton exposed: panel flyout menus require computer-use"
                       if not flyouts else "AXMenuButton present"),
        "watchlist": watch,
        "offscreenTabs": offscreen,
        "rejected": rejected,
        "unattributed": len(placed) - len(toolbar)
                        - sum(g["contentNodes"] for c in columns for g in c["groups"]),
    }


def main():
    dump = json.load(open(sys.argv[1]))
    json.dump(derive(dump), open(sys.argv[2], "w"), indent=2)
    print(sys.argv[2])


if __name__ == "__main__":
    main()
