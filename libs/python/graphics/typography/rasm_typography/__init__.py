"""Font capability over fontTools, ufo2ft, HarfBuzz and skia-pathops, with the pipeline that reconciles a declared font set from its originals."""

lazy from rasm_typography._builds import Action, Built, performed, Report
lazy from rasm_typography._cli import app, exited
lazy from rasm_typography._faces.coverage import Coverage, Excluded
lazy from rasm_typography._faces.declaration import (
    Band,
    BlackNaskh,
    Contour,
    Derived,
    Diwani,
    DotMatrix,
    Em,
    Face,
    Faces,
    Genre,
    Genres,
    Kufi,
    OrnamentFace,
    Ratio,
    Recipe,
    Ringed,
    Ruqah,
    Script,
    Segmented,
    Shadow,
    Sheared,
    TextNaskh,
    Thuluth,
    Treatment,
    Treatments,
    Values,
)
lazy from rasm_typography._faces.glyphs import Control, Digit, Letter, Ligature, Mark, Ornament, recorded, Sign, Space
lazy from rasm_typography._faces.layout import Contextual, Lifts, Ligatures, Lookups, Pairs, Positions, Single, Tag
lazy from rasm_typography._outlines.measure import perimeter
lazy from rasm_typography._outlines.nib import extruded, monoline, swept
lazy from rasm_typography._rules.anchors import Anchors
lazy from rasm_typography._rules.digits import Digits
lazy from rasm_typography._rules.fits import Outlines
lazy from rasm_typography._rules.joins import Joins
lazy from rasm_typography._rules.locale import Locale
lazy from rasm_typography._rules.marks import Marks, QuranicMarks, WordSigns
lazy from rasm_typography._rules.measures import bounded_glyph, Rule
lazy from rasm_typography._rules.metrics import Metrics, Records
lazy from rasm_typography._rules.ornaments import Ornaments
lazy from rasm_typography._rules.scale import Scale
lazy from rasm_typography._rules.spacing import Spacing
lazy from rasm_typography._shaping.expectations import decoded
lazy from rasm_typography._verification.requirements import (
    Covered,
    Distinct,
    Expectations,
    Identity,
    JoinParity,
    LigatureArea,
    LineMetrics,
    MarkClearance,
    MarkLine,
    Profile,
    Reachable,
    Sanitized,
    Seam,
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Action",
    "Anchors",
    "Band",
    "BlackNaskh",
    "Built",
    "Contextual",
    "Contour",
    "Control",
    "Coverage",
    "Covered",
    "Derived",
    "Digit",
    "Digits",
    "Distinct",
    "Diwani",
    "DotMatrix",
    "Em",
    "Excluded",
    "Expectations",
    "Face",
    "Faces",
    "Genre",
    "Genres",
    "Identity",
    "JoinParity",
    "Joins",
    "Kufi",
    "Letter",
    "Lifts",
    "Ligature",
    "LigatureArea",
    "Ligatures",
    "LineMetrics",
    "Locale",
    "Lookups",
    "Mark",
    "MarkClearance",
    "MarkLine",
    "Marks",
    "Metrics",
    "Ornament",
    "OrnamentFace",
    "Ornaments",
    "Outlines",
    "Pairs",
    "Positions",
    "Profile",
    "QuranicMarks",
    "Ratio",
    "Reachable",
    "Recipe",
    "Records",
    "Report",
    "Ringed",
    "Rule",
    "Ruqah",
    "Sanitized",
    "Scale",
    "Script",
    "Seam",
    "Segmented",
    "Shadow",
    "Sheared",
    "Sign",
    "Single",
    "Space",
    "Spacing",
    "Tag",
    "TextNaskh",
    "Thuluth",
    "Treatment",
    "Treatments",
    "Values",
    "WordSigns",
    "app",
    "bounded_glyph",
    "decoded",
    "exited",
    "extruded",
    "monoline",
    "performed",
    "perimeter",
    "recorded",
    "swept",
]
