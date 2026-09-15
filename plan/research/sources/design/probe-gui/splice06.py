import re,io
p="../gui-indesign.md"
doc=open(p,encoding="utf-8").read()
def splice(doc,start_marker,end_marker,new):
    a=doc.index(start_marker); b=doc.index(end_marker,a)
    return doc[:a]+new+doc[b:]

# ---- paragraph styles table: keep Base row verbatim
m=re.search(r"^\|  \[01\]   \| Base .*$",doc,re.M); base_row=m.group(0)
para_rows=[
("Body","Base","Body","`hyphenation=true` `firstLineIndent=\"15pt\"`"),
("Body First","Body","Body","`firstLineIndent=\"0pt\"`"),
("Body Listed","Body","Body Listed","`leftIndent=\"30pt\"` `firstLineIndent=\"0pt\"`"),
("Body Small","Base","Body Small","`pointSize=\"9.5625pt\"` `leading=\"11.25pt\"` `gridAlignFirstLineOnly=true` `hyphenation=true`"),
("Heading","Base","Body First","`fontStyle=\"Bold\"` `spaceBefore=\"15pt\"` `keepWithNext=2` `keepAllLinesTogether=true` `balanceRaggedLines=true`; the third heading level itself (12.75/15), so no separate H3 exists"),
("H1","Heading","Body First","`pointSize=\"25.5pt\"` `leading=\"30pt\"` `spaceBefore=\"30pt\"`; also the endnote title style (space before is dropped at the top of the endnote frame)"),
("H2","Heading","Body First","`pointSize=\"19.125pt\"` `leading=\"22.5pt\"` `gridAlignFirstLineOnly=true`"),
("Display","Heading","Title","`pointSize=\"51pt\"` `leading=\"60pt\"` `alignToBaseline=false` `spaceBefore=\"0pt\"` `ignoreEdgeAlignment=true` `tracking=-10`"),
("Title","Heading","Subtitle","`pointSize=\"38.25pt\"` `leading=\"45pt\"` `spaceBefore=\"0pt\"` `ignoreEdgeAlignment=true`"),
("Subtitle","Base","Body First","`pointSize=\"19.125pt\"` `leading=\"22.5pt\"` `gridAlignFirstLineOnly=true` `spaceAfter=\"15pt\"` `keepWithNext=2` `keepAllLinesTogether=true`"),
("Caption","Base","Caption","`pointSize=\"9.5625pt\"` `leading=\"11.25pt\"` `gridAlignFirstLineOnly=true` `keepAllLinesTogether=true`; figure captions (the `Caption` object style) and free captions"),
("Note","Body Small","Note","`leftIndent=\"15pt\"` `firstLineIndent=\"-15pt\"` `tabList=[{alignment:TabStopAlignment.LEFT_ALIGN, position:\"15pt\", leader:\"\"}]`; the footnote and endnote text style (`footnoteTextStyle`, `endnoteTextStyle`) and the hanging note in body text"),
("Running Header","Caption","Running Header","`otfFigureStyle=OTFFigureStyle.TABULAR_LINING` `keepLinesTogether=false` `keepAllLinesTogether=false`"),
("Section Indicator","Running Header","Section Indicator","`justification=Justification.RIGHT_ALIGN`; GREP rows [07], [08] of the GREP table; the folio frame's style (`N / N`)"),
("Table Body","Base","Table Body","`pointSize=\"9.5625pt\"` `leading=\"15pt\"` `alignToBaseline=false` `otfFigureStyle=OTFFigureStyle.TABULAR_LINING` `keepLinesTogether=false`"),
("Table Header","Table Body","Table Body","`fontStyle=\"Bold\"`"),
("Table Numeric","Table Body","Table Numeric","`justification=Justification.RIGHT_ALIGN` (tabular figures align integers; equal decimal precision per column aligns fractions, no decimal tab)"),
("Table Total","Table Numeric","Table Total","`fontStyle=\"Bold\"`"),
("Contents Title","H1","Contents L1","`spaceBefore=\"0pt\"` `spaceAfter=\"15pt\"`"),
("Contents L1","Base","Contents L1","`fontStyle=\"Bold\"` `tabList=[{alignment:TabStopAlignment.RIGHT_ALIGN, position:\"522pt\", leader:\"\"}]` (the measure, [07] geometry row 20) `otfFigureStyle=OTFFigureStyle.TABULAR_LINING` `keepWithNext=1`"),
("Contents L2","Contents L1","Contents L2","`fontStyle=\"Regular\"` `leftIndent=\"15pt\"` `keepWithNext=0`"),
("Contents L3","Contents L1","Contents L3","`fontStyle=\"Regular\"` `leftIndent=\"30pt\"` `keepWithNext=0`"),
("List Bullet","Base","List Bullet","`hyphenation=true` `leftIndent=\"15pt\"` `firstLineIndent=\"-15pt\"` `tabList=[{alignment:TabStopAlignment.LEFT_ALIGN, position:\"15pt\", leader:\"\"}]` `bulletsAndNumberingListType=ListType.BULLET_LIST` `bulletsCharacterStyle=\"[None]\"` `bulletsTextAfter=\"^t\"` `bulletsAlignment=ListAlignment.LEFT_ALIGN`"),
("List Numbered","Base","List Numbered","`hyphenation=true` `leftIndent=\"30pt\"` `firstLineIndent=\"-30pt\"` `tabList=[{alignment:TabStopAlignment.LEFT_ALIGN, position:\"30pt\", leader:\"\"}]` `bulletsAndNumberingListType=ListType.NUMBERED_LIST` `appliedNumberingList=\"Body List\"` `numberingExpression=\"^#.^t\"` `numberingFormat=\"1, 2, 3, 4...\"` `numberingAlignment=ListAlignment.RIGHT_ALIGN` `numberingCharacterStyle=\"Tabular Figures\"` `numberingContinue=true` `numberingLevel=1`"),
("List Numbered First","List Numbered","List Numbered","`numberingContinue=false` `numberingStartAt=1`"),
("Pull Quote","Base","Body First","`pointSize=\"19.125pt\"` `leading=\"22.5pt\"` `gridAlignFirstLineOnly=true` `appliedFont=families.latin.serif` `fontStyle=\"Italic\"` `leftIndent=\"30pt\"` `spaceBefore=\"15pt\"` `spaceAfter=\"15pt\"` `keepAllLinesTogether=true` `ignoreEdgeAlignment=true` `otfFigureStyle=OTFFigureStyle.PROPORTIONAL_OLDSTYLE` `otfDiscretionaryLigature=true`"),
("Code","Base","Code","`appliedFont=families.latin.mono` `pointSize=\"9.5625pt\"` `leading=\"11.25pt\"` `gridAlignFirstLineOnly=true` `ligatures=false` `otfContextualAlternate=false` `leftIndent=\"15pt\"` `spaceBefore=\"15pt\"` `spaceAfter=\"15pt\"` `keepFirstLines=3` `keepLastLines=3` `paragraphShadingOn=true` `paragraphShadingColor=\"Black\"` `paragraphShadingTint=6` `paragraphShadingLeftOffset=\"7.5pt\"` `paragraphShadingRightOffset=\"7.5pt\"` `paragraphShadingTopOffset=\"0pt\"` `paragraphShadingBottomOffset=\"0pt\"` `paragraphShadingWidth=ParagraphShadingWidthEnum.COLUMN_WIDTH` `otfFigureStyle=OTFFigureStyle.TABULAR_LINING` `appliedLanguage=app.languagesWithVendors.item(\"[No Language]\")`"),
("Table Caption","Caption","Body First","`keepWithNext=1` `spaceBefore=\"15pt\"`"),
("Form Label","Caption","Form Label","`capitalization=Capitalization.CAP_TO_SMALL_CAP` `tracking=50` `otfFigureStyle=OTFFigureStyle.TABULAR_LINING`"),
("Body Fa","Body","Body Fa","`appliedFont=families.persian` `composer=\"Adobe World-Ready Paragraph Composer\"` `paragraphDirection=ParagraphDirectionOptions.RIGHT_TO_LEFT_DIRECTION` `justification=Justification.RIGHT_JUSTIFIED` `digitsType=DigitsTypeOptions.FARSI_DIGITS` `kashidas=KashidasOptions.KASHIDAS_OFF` `paragraphKashidaWidth=0` `appliedLanguage=app.languagesWithVendors.item(\"[No Language]\")` `hyphenation=false` `otfMark=true` `otfLocale=true` `ignoreEdgeAlignment=true`"),
("Heading Fa","Heading","Body Fa","`appliedFont=families.persian` `composer=\"Adobe World-Ready Paragraph Composer\"` `paragraphDirection=RIGHT_TO_LEFT_DIRECTION` `justification=Justification.RIGHT_ALIGN` `digitsType=FARSI_DIGITS` `appliedLanguage=item(\"[No Language]\")` `otfMark=true` `otfLocale=true` `ignoreEdgeAlignment=true` `balanceRaggedLines=false`"),
("Caption Fa","Caption","Caption Fa","`appliedFont=families.persian` `composer=\"Adobe World-Ready Paragraph Composer\"` `paragraphDirection=RIGHT_TO_LEFT_DIRECTION` `justification=Justification.RIGHT_ALIGN` `digitsType=FARSI_DIGITS` `appliedLanguage=item(\"[No Language]\")` `otfMark=true` `otfLocale=true` `ignoreEdgeAlignment=true`"),
("Body Ar","Body","Body Ar","as Body Fa except `appliedFont=families.arabic` `digitsType=DigitsTypeOptions.HINDI_DIGITS` `kashidas=KashidasOptions.DEFAULT_KASHIDAS` `paragraphKashidaWidth=2` `appliedLanguage=app.languagesWithVendors.item(\"Arabic\")`"),
("Heading Ar","Heading","Body Ar","as Heading Fa except `appliedFont=families.arabic` `digitsType=HINDI_DIGITS` `appliedLanguage=item(\"Arabic\")`"),
("Caption Ar","Caption","Caption Ar","as Caption Fa except `appliedFont=families.arabic` `digitsType=HINDI_DIGITS` `appliedLanguage=item(\"Arabic\")`"),
("White/Title","Title","Subtitle","`fillColor=\"Paper\"`; group `White` (`paragraphStyleGroups.add({name:\"White\"})`), the deck's text on `Ink` fields and images"),
("White/Contents L1","Contents L1","White/Contents L1","`fillColor=\"Paper\"`"),
("White/Running Header","Running Header","White/Running Header","`fillColor=\"Paper\"`"),
("White/Section Indicator","Section Indicator","White/Section Indicator","`fillColor=\"Paper\"` (inherits the two GREP rows; `Muted` renders 50 % Paper)"),
]
lines=["Paragraph styles (`styles-graph.json` `paragraphStyles[]`; a row lists only what differs from its parent; 40 named styles, depth ≤ 3 below `Base`, plus `[Basic Paragraph]`):","",
"| [INDEX] | [NAME]              | [BASED ON]            | [NEXT]              | [PROPERTIES] |",
"| :-----: | :------------------ | :-------------------- | :------------------ | :----------- |",
base_row]
for i,(n,b,nx,pr) in enumerate(para_rows,start=2):
    lines.append(f"|  [{i:02d}]   | {n:<19} | {b:<21} | {nx:<19} | {pr} |")
lines.append(f"|  [{len(para_rows)+2:02d}]   | `[Basic Paragraph]` | —                     | self                | the [05] text-default values; never a parent |")
lines.append("")
new_para="\n".join(lines)+"\n"
doc=splice(doc,"Paragraph styles (`styles-graph.json`","Digits: Arabic roles take",new_para)

# ---- character styles
char_rows=[("Emphasis","`fontStyle=\"Italic\"`"),("Strong","`fontStyle=\"Bold\"`"),("Strong Emphasis","`fontStyle=\"Bold Italic\"`"),("Small Caps","`capitalization=Capitalization.CAP_TO_SMALL_CAP`"),("Note Reference","`position=Position.OT_SUPERSCRIPT` (footnote and endnote markers and every superscript)"),("Code","`appliedFont=families.latin.mono`"),("Tabular Figures","`otfFigureStyle=OTFFigureStyle.TABULAR_LINING`"),("No Break","`noBreak=true`"),("Hyperlink","`underline=true`"),("Accent","`fillColor=\"Accent\"` (the palette's primary accent swatch, created before the styles); the indicator's left half, the agenda's current item"),("Muted","`fillTint=50`; the indicator's right half"),("Digits Fa","`digitsType=DigitsTypeOptions.FARSI_DIGITS`"),("Latin in RTL","`characterDirection=CharacterDirectionOptions.LEFT_TO_RIGHT_DIRECTION`")]
lines=["Character styles (`[None]` base, one attribute each, every other attribute `NothingEnum.NOTHING`; 13):","",
"| [INDEX] | [NAME]          | [ATTRIBUTE] |","| :-----: | :-------------- | :---------- |"]
for i,(n,a) in enumerate(char_rows,start=1): lines.append(f"|  [{i:02d}]   | {n:<15} | {a} |")
lines.append("")
new_char="\n".join(lines)+"\n"
doc=splice(doc,"Character styles (`[None]` base","Indicator mechanism (",new_char)

# ---- indicator mechanism
new_ind='''Indicator mechanism (`N / N` with two colors from one paragraph style and the built-in text variables, no nested style; the two GREP expressions are disjoint, the left one excludes `/` and the right one starts after it, so no overlap or precedence question arises inside the indicator):

| [INDEX] | [ELEMENT]           | [BUILD] |
| :-----: | :------------------ | :------ |
|  [01]   | current page        | `ip.contents = SpecialCharacters.AUTO_PAGE_NUMBER` (a marker, one character) |
|  [02]   | separator           | literal `" / "` |
|  [03]   | last page           | the built-in `Last Page Number` (`doc.textVariables.itemByName("Last Page Number")`, one of the eight built-ins read live at app level: Chapter Number, Creation Date, File Name, Image Name, Last Page Number, Modification Date, Output Date, Running Header): `v.variableOptions.scope = VariableScopes.DOCUMENT_SCOPE; v.variableOptions.format = VariableNumberingStyles.ARABIC; ip.textVariableInstances.add({associatedTextVariable: v})` |
|  [04]   | section variant     | the built-in `Running Header` re-pointed: `rh.variableOptions.appliedParagraphStyle = doc.paragraphStyles.item("H1"); rh.variableOptions.searchStrategy = SearchStrategies.FIRST_ON_PAGE; rh.variableOptions.deleteEndPunctuation = true; rh.variableOptions.changeCase = ChangeCaseOptions.NONE` + `" / "` + the built-in `Chapter Number` |
|  [05]   | left color          | `ps.nestedGrepStyles.add({appliedCharacterStyle: doc.characterStyles.item("Accent"), grepExpression: "^[^/]+(?=/)"})` |
|  [06]   | right color         | `ps.nestedGrepStyles.add({appliedCharacterStyle: doc.characterStyles.item("Muted"), grepExpression: "(?<=/).+$"})` |
|  [07]   | nested form, unused | `nestedStyles.add({appliedCharacterStyle: "Accent", delimiter: NestedStyleDelimiters.AUTO_PAGE_NUMBER, repetition: 1, inclusive: true})` then `{…"Muted", delimiter: NestedStyleDelimiters.TABS, repetition: 1, inclusive: true}` (TABS with no tab runs to the paragraph end) |
|  [08]   | limits              | variables never break across lines (Adobe text-variables); Running Header takes the first or last styled occurrence on the page, else the last earlier one; Last Page Number reports the last page's number, not a count; one chapter number per document; GREP styles evaluate per paragraph; nested styles run in list order; a nested style beats a nested line style on the same attribute; GREP versus nested precedence in the general case is [10] row U1 (Adobe's GREP-styles page rendered only its navigation through every allowed channel on 2026-09-11); ZWNJ is stripped inside variables |

'''
doc=splice(doc,"Indicator mechanism (","GREP styles (`nestedGrepStyles.add(",new_ind)

# ---- GREP table rows
doc=doc.replace("|  [02]   | Body, Body Small, Footnote, Caption                       | `\\d+ ?(mm|cm|m|km|pt|pc|px|in|ft|kg|g|%|°)\\b`                  | No Break          | number stays with its unit                                         |",
"|  [02]   | Body, Body Small, Note, Caption                           | `\\d+ ?(mm|cm|m|km|pt|pc|px|in|ft|kg|g|%|°)\\b`                  | No Break          | number stays with its unit                                         |")
doc=doc.replace("|  [03]   | Body, Body Small, Footnote, Caption                       | `(§|No\\.|Nos\\.|Fig\\.|Figs\\.|Tab\\.|p\\.|pp\\.) ?\\d+`               | No Break          | sign or abbreviation stays with its number                         |",
"|  [03]   | Body, Body Small, Note, Caption                           | `(§|No\\.|Nos\\.|Fig\\.|Figs\\.|Tab\\.|p\\.|pp\\.) ?\\d+`               | No Break          | sign or abbreviation stays with its number                         |")
doc=doc.replace("|  [07]   | Section Indicator                                         | `^[^/]+(?=/)`                                                  | Indicator Left    | indicator left half                                                |",
"|  [07]   | Section Indicator                                         | `^[^/]+(?=/)`                                                  | Accent            | indicator left half                                                |")
doc=doc.replace("|  [08]   | Section Indicator                                         | `(?<=/).+$`                                                    | Indicator Right   | indicator right half                                               |",
"|  [08]   | Section Indicator                                         | `(?<=/).+$`                                                    | Muted             | indicator right half                                               |")
open(p,"w",encoding="utf-8").write(doc)
print("ok", doc.count("Indicator Left"), doc.count("Footnote  "), len(doc))
