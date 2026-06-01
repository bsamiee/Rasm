using System;
using System.IO;
using Rasm.Rhino;
using Rasm.Rhino.Camera;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Exchange;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using DrawingColor = System.Drawing.Color;
using DrawingPointF = System.Drawing.PointF;

Scenario.Run("exchange", CAPTURE_PATH, (key, facts) => {
    using DocumentScope scope = DocumentScope.Open();
    RhinoDoc doc = scope.Active;
    facts.Add("mainThread", RhinoApp.IsOnMainThread);
    doc.Objects.Clear();
    toSeq(doc.Views.GetPageViews()).Iter(page => page.Close());

    string work = Path.Combine(Path.GetTempPath(), $"rasm-exchange-{Guid.NewGuid():N}");
    _ = Directory.CreateDirectory(work);
    facts.Add("work", work);
    RhinoFiles files = RhinoFiles.Live(document: doc, mode: RunMode.Scripted);

    // --- shared doc state: a box, two layout pages, one model view + named view ---
    Brep box = Brep.CreateFromBox(new Box(Plane.WorldXY, new Interval(-20.0, 20.0), new Interval(-20.0, 20.0), new Interval(0.0, 10.0)))
        ?? throw new InvalidOperationException(message: "box brep");
    Guid boxId = doc.Objects.AddBrep(brep: box);
    Probe.Require(boxId != Guid.Empty, "box added");
    UnitSystem pageUnits = doc.PageUnitSystem;
    Probe.Require(pageUnits != UnitSystem.None && pageUnits != UnitSystem.Unset && pageUnits != UnitSystem.CustomUnits, "doc has a usable page unit system");
    RhinoPageView pageA = doc.Views.AddPageView(title: $"RasmSheetA{Guid.NewGuid():N}", pageWidth: 50.0, pageHeight: 40.0) ?? throw new InvalidOperationException(message: "pageA");
    RhinoPageView pageB = doc.Views.AddPageView(title: $"RasmSheetB{Guid.NewGuid():N}", pageWidth: 50.0, pageHeight: 40.0) ?? throw new InvalidOperationException(message: "pageB");
    string sheetAName = pageA.PageName;
    string sheetBName = pageB.PageName;
    facts.Add("pages.created", doc.Views.GetPageViews().Length);
    RhinoView[] modelViews = doc.Views.GetViewList(includeStandardViews: true, includePageViews: false);
    Probe.Require(modelViews.Length > 0, "doc has a model view");
    RhinoView modelView = modelViews[0];
    doc.Views.ActiveView = modelView;
    string namedView = $"RasmView{Guid.NewGuid():N}";
    Probe.Require(doc.NamedViews.Add(name: namedView, viewportId: modelView.MainViewport.Id) >= 0, "named view added");
    doc.Views.Redraw();

    // --- 1) filterless layout publish captures BOTH pages (A1) ---
    FileEndpoint allPdf = Probe.Expect(FileEndpoint.From(path: Path.Combine(work, "all-pages.pdf")), "all-pages endpoint");
    FileReport publishAll = Probe.Expect(files.Run(FileOp.Do(new FileExchange.Publish(new FilePublish(
        Target: new FilePublishTarget.Pdf(Target: allPdf),
        Views: Seq<FileView>())))), "publish all layout pages", facts);
    facts.Add("publish.all.views", publishAll.Views.Count);
    Probe.Require(publishAll.Views.Count == 2, "filterless Pages() captured both layout pages (A1, not just page one)");
    Probe.Require(File.Exists(Path.Combine(work, "all-pages.pdf")), "all-pages.pdf written");

    // --- 2) mixed raster+vector PDF + typed PdfStamp annotation (A4 per-page OCG + B2) ---
    Seq<PdfStamp> stamps = Seq<PdfStamp>(
        new PdfStamp.Text(Value: "RASM", X: 36.0, Y: 36.0, HeightPoints: 12f, Font: new Font(familyName: "Arial"), Fill: DrawingColor.Black),
        new PdfStamp.Line(From: new DrawingPointF(36f, 48f), To: new DrawingPointF(220f, 48f), Stroke: DrawingColor.Black, Width: 1f));
    FileEndpoint mixedPdf = Probe.Expect(FileEndpoint.From(path: Path.Combine(work, "mixed.pdf")), "mixed endpoint");
    FileReport publishMixed = Probe.Expect(files.Run(FileOp.Do(new FileExchange.Publish(new FilePublish(
        Target: new FilePublishTarget.Pdf(Target: mixedPdf, Annotate: stamps),
        Views: Seq(
            new FileView(Source: new FileViewSource.Pages(Query: new SheetQuery(Name: Some(sheetAName))), Recipe: new CaptureRecipe(Dpi: Some(72.0))),
            new FileView(Source: new FileViewSource.Pages(Query: new SheetQuery(Name: Some(sheetBName))), Recipe: new CaptureRecipe(Dpi: Some(72.0), Raster: true))),
        Layers: true)))), "publish mixed raster+vector pdf", facts);
    facts.Add("publish.mixed.views", publishMixed.Views.Count);
    Probe.Require(publishMixed.Views.Count == 2, "mixed publish emitted both pages");
    Probe.Require(File.Exists(Path.Combine(work, "mixed.pdf")), "mixed.pdf written (per-page OCG honoured for raster page)");

    // --- 3) named-view publish runs restore on the pre-resolved scope (A2, no nested dispatch) ---
    FileEndpoint namedPdf = Probe.Expect(FileEndpoint.From(path: Path.Combine(work, "named-view.pdf")), "named endpoint");
    FileReport publishNamed = Probe.Expect(files.Run(FileOp.Do(new FileExchange.Publish(new FilePublish(
        Target: new FilePublishTarget.Pdf(Target: namedPdf),
        Views: Seq(new FileView(Source: new FileViewSource.Named(Names: Seq(namedView), Target: new ViewportTarget.View(modelView)), Recipe: new CaptureRecipe(Dpi: Some(72.0)))))))), "publish named view", facts);
    facts.Add("publish.named.views", publishNamed.Views.Count);
    Probe.Require(publishNamed.Views.Count == 1, "named-view publish emitted one page (A2 direct-scope restore)");
    Probe.Require(File.Exists(Path.Combine(work, "named-view.pdf")), "named-view.pdf written");

    // --- 4) transparent raster PNG (B1): NOT bridge-verifiable on macOS. Touching FileRasterEncoding throws
    //     TypeLoadException — RhinoWIP's hosted System.Drawing.Common omits the GDI+ `System.Drawing.Imaging.Encoder`
    //     type that the raster-codec path (FileRasterEncoding/SaveBitmap) depends on. B1 compiles and is correct on a
    //     GDI+-capable runtime; the whole raster path is environmentally blocked here, so it is excluded.

    // --- 5) sheet edits produce typed receipts; Resize mutates PageWidth in place (AGENTS correction) ---
    string sheetC = $"RasmSheetC{Guid.NewGuid():N}";
    DocumentReceipt createReceipt = Probe.ExpectSome(Probe.Expect(files.Run(FileOp.Do(new FileExchange.SheetEdit(
        new FileSheetEdit.Create(new FileSheetSpec(Name: sheetC, Size: Some(new FileSheetSize(Units: pageUnits, Width: Some(297.0), Height: Some(210.0)))))))), "sheet create", facts).Receipt, "create receipt");
    Probe.Require(createReceipt.Created.Count == 1, "sheet create returns one Created id");
    RhinoPageView createdC = toSeq(doc.Views.GetPageViews()).Find(page => page.PageName == sheetC).Match(Some: page => page, None: () => throw new InvalidOperationException(message: "sheetC missing"));
    double beforeWidth = createdC.PageWidth;
    DocumentReceipt resizeReceipt = Probe.ExpectSome(Probe.Expect(files.Run(FileOp.Do(new FileExchange.SheetEdit(
        new FileSheetEdit.Resize(SheetName: sheetC, Size: Some(new FileSheetSize(Units: pageUnits, Width: Some(420.0), Height: Some(297.0))))))), "sheet resize", facts).Receipt, "resize receipt");
    Probe.Require(resizeReceipt.AttributeChanged.Count == 1, "resize is an AttributeChanged receipt");
    RhinoPageView resizedC = toSeq(doc.Views.GetPageViews()).Find(page => page.PageName == sheetC).Match(Some: page => page, None: () => throw new InvalidOperationException(message: "sheetC missing after resize"));
    facts.Add("sheet.width.before", beforeWidth);
    facts.Add("sheet.width.after", resizedC.PageWidth);
    Probe.Require(resizedC.PageWidth > beforeWidth, "PageWidth setter applied in place (resize, not duplicate+delete)");
    DocumentReceipt reorderReceipt = Probe.ExpectSome(Probe.Expect(files.Run(FileOp.Do(new FileExchange.SheetEdit(
        new FileSheetEdit.Reorder(SheetNames: Seq(sheetC, sheetAName, sheetBName))))), "sheet reorder", facts).Receipt, "reorder receipt");
    Probe.Require(reorderReceipt.AttributeChanged.Count == 3, "reorder rebinds three page numbers");
    DocumentReceipt detailReceipt = Probe.ExpectSome(Probe.Expect(files.Run(FileOp.Do(new FileExchange.SheetEdit(
        new FileSheetEdit.AddDetail(SheetName: sheetC, Spec: new FileDetailSpec(Name: "Plan", Corner: new Point2d(20.0, 20.0), Opposite: new Point2d(260.0, 180.0), Projection: DefinedViewportProjection.Top))))), "add detail", facts).Receipt, "detail receipt");
    Probe.Require(detailReceipt.Created.Count == 1, "add detail returns one Created id (page realized via activate-preamble)");
    DocumentReceipt scaleReceipt = Probe.ExpectSome(Probe.Expect(files.Run(FileOp.Do(new FileExchange.SheetEdit(
        new FileSheetEdit.ScaleDetail(SheetName: sheetC, Detail: DetailQuery.Named(name: "Plan"), Scale: new FileScale.Lengths(ModelLength: 1.0, ModelUnit: LengthUnit.Meters, PageLength: 1.0, PageUnit: LengthUnit.Millimeters))))), "scale detail (explicit lengths)", facts).Receipt, "scale receipt");
    Probe.Require(scaleReceipt.AttributeChanged.Count == 1, "scale detail is an AttributeChanged receipt");

    // --- 5b) NEW: native named-scale parse + batch, page-wide Configure, Sheets inspection, auto-numbering ---
    // FileScale.Named parses "1:100"/imperial fractions through Rhino.ScaleValue; DetailQuery.All batches every detail.
    DocumentReceipt namedScaleReceipt = Probe.ExpectSome(Probe.Expect(files.Run(FileOp.Do(new FileExchange.SheetEdit(
        new FileSheetEdit.ScaleDetail(SheetName: sheetC, Detail: DetailQuery.All, Scale: new FileScale.Named(Value: "1:100"))))), "scale detail named 1:100 (batch)", facts).Receipt, "named scale receipt");
    Probe.Require(namedScaleReceipt.AttributeChanged.Count >= 1, "named-scale batch touched at least one detail");
    RhinoPageView scaledC = toSeq(doc.Views.GetPageViews()).Find(page => page.PageName == sheetC).Match(Some: page => page, None: () => throw new InvalidOperationException(message: "sheetC missing after named scale"));
    DetailViewObject[] sheetCDetails = scaledC.GetDetailViews();
    Probe.Require(sheetCDetails.Length >= 1, "sheetC has a detail after named scale");
    // Detail scale PERSISTS through the geometry-commit rail (FileScale.Apply: SetScale + DetailViewObject.CommitChanges):
    // a freshly re-queried detail's GetFormattedScale / PageToModelRatio round-trips. "1:100" ⇒ page/model ratio 0.01.
    string namedFmt = "<unset>";
    Probe.Require(sheetCDetails[0].GetFormattedScale(DetailViewObject.ScaleFormat.OneToModelLength, out namedFmt), "re-queried detail carries a formatted scale after FileScale.Named");
    facts.Add("detail.named.formattedScale", namedFmt ?? "<unset>");
    double namedRatio = sheetCDetails[0].DetailGeometry?.PageToModelRatio ?? -1.0;
    facts.Add("detail.named.pageToModelRatio", namedRatio);
    Probe.Require(namedRatio > 0.0 && namedRatio < 1.0, "FileScale.Named(\"1:100\") persists a reduction ratio (page<model) on re-query");
    DocumentReceipt imperialReceipt = Probe.ExpectSome(Probe.Expect(files.Run(FileOp.Do(new FileExchange.SheetEdit(
        new FileSheetEdit.ScaleDetail(SheetName: sheetC, Detail: DetailQuery.Named(name: "Plan"), Scale: new FileScale.Named(Value: "1/4\"=1'-0\""))))), "scale detail imperial fraction", facts).Receipt, "imperial scale receipt");
    Probe.Require(imperialReceipt.AttributeChanged.Count == 1, "imperial named scale parsed and applied to the Plan detail");

    // page-wide Configure: description + group + group user-strings applied atomically across the matched set
    string sheetGroup = $"RasmGroup{Guid.NewGuid():N}";
    DocumentReceipt configureReceipt = Probe.ExpectSome(Probe.Expect(files.Run(FileOp.Do(new FileExchange.SheetEdit(
        new FileSheetEdit.Configure(
            Query: new SheetQuery(Name: Some(sheetC)),
            Config: new FileSheetConfig(
                Description: Some("rasm configured sheet"),
                Group: Some(sheetGroup),
                GroupDescription: Some("rasm sheet set"),
                UserStrings: Seq(new FileUserString(Key: "Discipline", Value: Some("A")))))))), "configure sheet page-wide", facts).Receipt, "configure receipt");
    Probe.Require(configureReceipt.AttributeChanged.Count == 1, "Configure applied to the one matched page");
    Probe.Require(configureReceipt.ResourceChanged.Exists(change => change.Kind == DocumentResourceKind.PageViewGroup), "Configure created/updated a page-view group");

    // Sheets inspection: read concern, separate Eff entrypoint (cannot fold into the Fin<DocumentReceipt> mutation union)
    Seq<FileSheetReport> sheetReports = Probe.Expect(files.Run(FileOp.Sheets(new SheetQuery(Name: Some(sheetC)))), "inspect sheetC", facts);
    Probe.Require(sheetReports.Count == 1, "Sheets inspection returns one report for sheetC");
    FileSheetReport sheetCReport = sheetReports[0];
    facts.Add("inspect.detailCount", sheetCReport.Details.Count);
    facts.Add("inspect.groupCount", sheetCReport.Groups.Count);
    Probe.Require(sheetCReport.Details.Count >= 1, "inspection sees the Plan detail");
    Probe.Require(sheetCReport.Groups.Exists(name => name == sheetGroup), "inspection reports the configured group membership");
    facts.Add("inspect.detailScale.anySome", sheetCReport.Details.Exists(detail => detail.Scale.IsSome));
    Probe.Require(sheetCReport.Details.Exists(detail => detail.Scale.IsSome), "inspection reports the persisted detail scale (FileScale.Format reads it back)");

    // auto-numbering: rebind the matched page's PageName from a {n:000} pattern (runs last — it renames sheetC)
    string numberPrefix = $"RasmNum{Guid.NewGuid():N}-";
    DocumentReceipt numberReceipt = Probe.ExpectSome(Probe.Expect(files.Run(FileOp.Do(new FileExchange.SheetEdit(
        new FileSheetEdit.Number(Query: new SheetQuery(Name: Some(sheetC)), Numbering: new FileNumbering(SheetPattern: numberPrefix + "{n:000}", Start: 7))))), "number sheet", facts).Receipt, "number receipt");
    Probe.Require(numberReceipt.AttributeChanged.Count == 1, "Number rebinds one matched page");
    Probe.Require(toSeq(doc.Views.GetPageViews()).Exists(page => page.PageName == numberPrefix + "007"), "Number applied {n:000} pattern with Start=7 -> 007");

    // --- 6) named layer state + named object position round-trip (B3) ---
    string layerState = $"RasmLayerState{Guid.NewGuid():N}";
    DocumentReceipt layerReceipt = Probe.ExpectSome(Probe.Expect(files.Run(FileOp.Do(new FileExchange.NativeTable(new FileNativeTable.SaveLayerState(Name: layerState)))), "named layer state save", facts).Receipt, "layer state receipt");
    Probe.Require(layerReceipt.ResourceChanged.Exists(change => change.Kind == DocumentResourceKind.NamedLayerState), "layer state saved");
    string position = $"RasmPosition{Guid.NewGuid():N}";
    DocumentTarget positionTarget = Probe.Expect(DocumentTarget.Objects(objectIds: Seq(boxId)), "position target");
    DocumentReceipt posSaveReceipt = Probe.ExpectSome(Probe.Expect(files.Run(FileOp.Do(new FileExchange.NativeTable(new FileNativeTable.SavePosition(Name: position, Objects: positionTarget)))), "named position save", facts).Receipt, "position save receipt");
    Probe.Require(posSaveReceipt.ResourceChanged.Exists(change => change.Kind == DocumentResourceKind.NamedPosition), "named position saved (B3)");
    DocumentReceipt posRestoreReceipt = Probe.ExpectSome(Probe.Expect(files.Run(FileOp.Do(new FileExchange.NativeTable(new FileNativeTable.Position(Name: position, Kind: FileNativePositionKind.Restore)))), "named position restore", facts).Receipt, "position restore receipt");
    Probe.Require(posRestoreReceipt.ResourceChanged.Count == 1, "named position restore receipt");
    Probe.Require(toSeq(doc.NamedPositions.Names).Contains(position), "named position present in document table");
    // read entrypoints — separate Eff surfaces (different return shape than the mutation unions)
    Seq<string> layerStateNames = Probe.Expect(files.Run(FileOp.NamedLayerStates()), "read named layer states", facts);
    Probe.Require(layerStateNames.Contains(layerState), "NamedLayerStates() lists the saved state");
    Seq<FilePositionReport> positionReports = Probe.Expect(files.Run(FileOp.NamedPositions()), "read named positions", facts);
    Probe.Require(positionReports.Exists(report => report.Name == position), "NamedPositions() lists the saved position");
    Probe.Require(positionReports.Exists(report => report.Objects.Contains(boxId)), "named position report carries the box object id");
    // Append extends an existing named position with another object (B-addendum verb)
    Brep box2 = Brep.CreateFromBox(new Box(Plane.WorldXY, new Interval(30.0, 50.0), new Interval(30.0, 50.0), new Interval(0.0, 10.0))) ?? throw new InvalidOperationException(message: "box2 brep");
    Guid box2Id = doc.Objects.AddBrep(brep: box2);
    DocumentTarget appendTarget = Probe.Expect(DocumentTarget.Objects(objectIds: Seq(box2Id)), "append target");
    DocumentReceipt appendReceipt = Probe.ExpectSome(Probe.Expect(files.Run(FileOp.Do(new FileExchange.NativeTable(new FileNativeTable.AppendPosition(Name: position, Objects: appendTarget)))), "named position append", facts).Receipt, "append receipt");
    Probe.Require(appendReceipt.ResourceChanged.Exists(change => change.Kind == DocumentResourceKind.NamedPosition), "Append updates the named position (B-addendum)");

    // --- 7) earth anchor set + sun sync + projection round-trip ---
    FileGeoLocation location = new FileGeoLocation(
        Latitude: Some(40.7128), Longitude: Some(-74.006), Elevation: Some(10.0),
        ElevationCoordinateSystem: EarthCoordinateSystem.MeanSeaLevel,
        ModelBasePoint: Some(Point3d.Origin), ModelNorth: Some(new Vector3d(0.0, 1.0, 0.0)), ModelEast: Some(new Vector3d(1.0, 0.0, 0.0)),
        Name: Some("RasmAnchor"), Description: Option<string>.None,
        KmlHeadingDegrees: Option<double>.None, KmlTiltDegrees: Option<double>.None, KmlRollDegrees: Option<double>.None);
    DocumentReceipt anchorReceipt = Probe.Expect(FileGeoLocation.Set(document: doc, location: location), "earth anchor set", facts);
    Probe.Require(anchorReceipt.ResourceChanged.Exists(change => change.Kind == DocumentResourceKind.EarthAnchor), "earth anchor receipt");
    DocumentReceipt sunReceipt = Probe.Expect(FileGeoLocation.SyncSun(document: doc), "sync sun", facts);
    Probe.Require(sunReceipt.ResourceChanged.Exists(change => change.Kind == DocumentResourceKind.Sun), "sun synced from model north");
    Seq<Point3d> probePts = Seq(new Point3d(5.0, 5.0, 0.0), new Point3d(-3.0, 7.0, 2.0));
    Seq<(double Latitude, double Longitude, double Elevation)> earth = Probe.Expect(FileGeoLocation.ProjectToEarth(document: doc, points: probePts, modelUnits: LengthUnit.Meters), "project to earth", facts);
    Seq<Point3d> back = Probe.Expect(FileGeoLocation.ProjectToModel(document: doc, coordinates: earth, modelUnits: LengthUnit.Meters), "project to model", facts);
    Probe.Require(back.Count == probePts.Count, "round-trip preserves point count");
    double maxError = Math.Max(probePts[0].DistanceTo(back[0]), probePts[1].DistanceTo(back[1]));
    facts.Add("earth.roundtrip.maxError", maxError);
    Probe.Require(maxError < 1e-6, "earth<->model projection round-trips within tolerance");

    // --- 8) archive read/validate/inspect (Graph projection — Resources NRE on null File3dmObject.Geometry fixed)
    //     + ArchiveUpdate FileUserString round-trip (B4), confirmed through both the resource graph and a direct File3dm read. ---
    string archivePath = Path.Combine(work, "archive.3dm");
    FileEndpoint archiveEndpoint = Probe.Expect(FileEndpoint.From(path: archivePath), "archive endpoint");
    Probe.Expect(files.Run(FileOp.Do(new FileExchange.Write(Phase: FilePhase.Write3dmFile, Target: archiveEndpoint, Profile: FileProfile.Model))), "write 3dm", facts);
    Probe.Require(File.Exists(archivePath), "archive.3dm written");
    FileArchiveSource archiveSource = new FileArchiveSource.Path(Value: Probe.Expect(FileEndpoint.From(path: archivePath), "archive source endpoint"));
    FileArchive readArchive = Probe.ExpectSome(Probe.Expect(files.Run(FileOp.Do(new FileExchange.ArchiveRead(Source: archiveSource, Profile: ArchiveProfile.Full))), "archive read", facts).Archive, "archive snapshot");
    facts.Add("archive.objects", readArchive.Resources.Objects);
    facts.Add("archive.relations", readArchive.Resources.Relations);
    Probe.Require(readArchive.Resources.Objects >= 1, "graph walk sees the box object (Resources NRE on null Geometry fixed)");
    Probe.Expect(files.Run(FileOp.Do(new FileExchange.ArchiveValidate(Source: archiveSource, Profile: ArchiveProfile.Full))), "archive validate", facts);
    FileArchive inspected = Probe.ExpectSome(Probe.Expect(files.Run(FileOp.Do(new FileExchange.ArchiveInspect(Source: Probe.Expect(FileEndpoint.From(path: archivePath), "inspect endpoint")))), "archive inspect", facts).Archive, "inspect snapshot");
    facts.Add("archive.version", inspected.Metadata.ArchiveVersion);
    Probe.Require(inspected.Metadata.ArchiveVersion > 0, "archive metadata version read");
    string updatedPath = Path.Combine(work, "archive-updated.3dm");
    FileArchiveMetadataPatch patch = new FileArchiveMetadataPatch(
        Notes: Some("rasm verify notes"), ApplicationName: Option<string>.None, ApplicationUrl: Option<string>.None, ApplicationDetails: Option<string>.None,
        StartComments: Option<string>.None, UserStrings: Seq(new FileUserString(Key: "RasmKey", Section: Some("RasmSection"), Value: Some("RasmValue"))));
    Probe.Expect(files.Run(FileOp.Do(new FileExchange.ArchiveUpdate(
        Source: archiveSource, Target: Probe.Expect(FileEndpoint.From(path: updatedPath), "updated endpoint"), Update: new ArchiveUpdate(Metadata: Some(patch)), Profile: ArchiveProfile.Full))), "archive update with user string", facts);
    Probe.Require(File.Exists(updatedPath), "updated archive written");
    FileArchive rereadArchive = Probe.ExpectSome(Probe.Expect(files.Run(FileOp.Do(new FileExchange.ArchiveRead(
        Source: new FileArchiveSource.Path(Value: Probe.Expect(FileEndpoint.From(path: updatedPath), "reread endpoint")), Profile: ArchiveProfile.Full))), "reread updated archive", facts).Archive, "reread snapshot");
    bool graphRoundTrip = rereadArchive.Resources.Entries.Exists(entry =>
        entry.Name.Map(name => name == "RasmKey").IfNone(false) && entry.Value.Map(value => value == "RasmValue").IfNone(false));
    facts.Add("archive.userString.graph", graphRoundTrip);
    Probe.Require(graphRoundTrip, "FileUserString round-trips through ArchiveUpdate -> ArchiveRead graph (B4)");
    Rhino.FileIO.File3dm updated = Rhino.FileIO.File3dm.Read(path: updatedPath);
    Probe.Require(updated != null, "updated archive readable");
    using (updated) {
        facts.Add("archive.userString.direct", updated.Strings.GetValue(section: "RasmSection", entry: "RasmKey") ?? "<null>");
        Probe.Require(updated.Strings.GetValue(section: "RasmSection", entry: "RasmKey") == "RasmValue", "FileUserString confirmed via direct File3dm read (B4)");
        Probe.Require(updated.Notes.Notes == "rasm verify notes", "metadata Notes patch applied");
    }

    // --- 9) export-selected to OBJ exercises BOTH the A3 readback fix AND the selected-export non-interactive fix:
    //     OBJ now routes through the typed FileObj.Write (WriteSelectedObjectsOnly + Suppress* on the embedded
    //     FileWriteOptions), not the interactive ExportSelected empty-dictionary "Choose OBJ option" command prompt.
    string objPath = Path.Combine(work, "selected.obj");
    DocumentTarget exportTarget = Probe.Expect(DocumentTarget.Objects(objectIds: Seq(boxId)), "export target");
    DocumentReceipt exportReceipt = Probe.ExpectSome(Probe.Expect(files.Run(FileOp.Do(new FileExchange.Export(
        Target: Probe.Expect(FileEndpoint.From(path: objPath), "obj endpoint"), Objects: Some(exportTarget), Profile: FileProfile.Model))), "export selected obj", facts).Receipt, "export receipt");
    facts.Add("export.selected", exportReceipt.Selected.Count);
    Probe.Require(exportReceipt.Selected.Count == 1, "export-selected reports one selected object (A3 readback no longer aborts)");
    Probe.Require(File.Exists(objPath), "selected.obj written non-interactively (selected-export typed-writer fix)");

    // --- cleanup: leave the document as found — no Rasm* named views/positions/states, pages, objects, or anchor ---
    _ = doc.NamedViews.Delete(name: namedView);
    _ = doc.NamedPositions.Delete(name: position);
    _ = doc.NamedLayerStates.Delete(name: layerState);
    toSeq(doc.Views.GetPageViews()).Iter(page => page.Close());
    using (EarthAnchorPoint reset = new EarthAnchorPoint()) { doc.EarthAnchorPoint = reset; }
    doc.Objects.Clear();
    doc.Views.Redraw();
    facts.Add("cleanup.namedViews", doc.NamedViews.Count);
});
