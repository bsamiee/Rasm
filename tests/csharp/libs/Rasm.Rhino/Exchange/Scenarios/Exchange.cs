using System.Diagnostics.CodeAnalysis;
using Rasm.TestKit.Scenarios;
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

namespace Rasm.Rhino.Tests.Exchange.Scenarios;

// --- [OPERATIONS] ---------------------------------------------------------------------------

// Ownership: the Exchange theme — layout publish (filterless/mixed/named-view), sheet-edit
// receipts with native scale persistence, named layer states and positions, earth anchor + sun
// sync round-trip, archive read/validate/update with user-string proof, and non-interactive
// selected-object export. The transparent-raster PNG lane stays excluded: RhinoWIP's hosted
// System.Drawing.Common omits the GDI+ Encoder type the raster codec path depends on.
internal static class ExchangeScenarios {
    [RhinoScenario(theme: "exchange")]
    internal static Fin<Unit> Exchange(ScenarioContext ctx) =>
        from scope in DocumentScope.Open(ctx: ctx)
        let doc = scope.Doc
        let onMain = Note(ctx: ctx, key: "mainThread", value: RhinoApp.IsOnMainThread)
        let pagesClosed = ClosePages(doc: doc)
        let work = WorkDirectory()
        let workFact = Note(ctx: ctx, key: "work", value: work)
        let files = RhinoFiles.Live(document: doc, mode: RunMode.Scripted)
        from box in BoxBrep(x0: -20.0, x1: 20.0, y0: -20.0, y1: 20.0, z0: 0.0, z1: 10.0)
        let boxId = doc.Objects.AddBrep(brep: box)
        from boxAdded in ctx.Require(label: "box added", observed: boxId != Guid.Empty)
        let pageUnits = doc.PageUnitSystem
        from unitsLaw in ctx.Require(label: "usable page unit system", observed: pageUnits is not (UnitSystem.None or UnitSystem.Unset or UnitSystem.CustomUnits))
        from pageA in ctx.Expect(label: "pageA", projection: Optional(doc.Views.AddPageView(title: Stamp(stem: "RasmSheetA"), pageWidth: 50.0, pageHeight: 40.0)).ToFin(Fail: Error.New(message: "pageA add failed")))
        from pageB in ctx.Expect(label: "pageB", projection: Optional(doc.Views.AddPageView(title: Stamp(stem: "RasmSheetB"), pageWidth: 50.0, pageHeight: 40.0)).ToFin(Fail: Error.New(message: "pageB add failed")))
        let sheetAName = pageA.PageName
        let sheetBName = pageB.PageName
        let pagesFact = Note(ctx: ctx, key: "pages.created", value: doc.Views.GetPageViews().Length)
        let modelViews = doc.Views.GetViewList(filter: ViewTypeFilter.Model)
        from modelViewLaw in ctx.Require(label: "doc has a model view", observed: modelViews.Length > 0)
        let modelView = modelViews[0]
        let activated = Activate(doc: doc, view: modelView)
        let namedView = Stamp(stem: "RasmView")
        from namedViewLaw in ctx.Require(label: "named view added", observed: doc.NamedViews.Add(name: namedView, viewportId: modelView.MainViewport.Id) >= 0)
        let redrawn = Redraw(doc: doc)

        // 1) filterless layout publish captures BOTH pages (A1)
        from allPdf in ctx.Expect(label: "all-pages endpoint", projection: FileEndpoint.From(path: Path.Combine(path1: work, path2: "all-pages.pdf")))
        from publishAll in ctx.Expect(label: "publish all layout pages", projection: files.Run(operation: FileOp.Do(exchange: new FileExchange.Publish(new FilePublish(
            Target: new FilePublishTarget.Pdf(Target: allPdf),
            Views: Seq<FileView>())))))
        let publishAllFact = Note(ctx: ctx, key: "publish.all.views", value: publishAll.Views.Count)
        from publishAllLaw in ctx.Require(label: "filterless Pages() captured both layout pages", observed: publishAll.Views.Count == 2)
        from allWritten in ctx.Require(label: "all-pages.pdf written", observed: File.Exists(path: Path.Combine(path1: work, path2: "all-pages.pdf")))

            // 2) mixed raster+vector PDF + typed PdfStamp annotation (A4 per-page OCG + B2)
        let stamps = Seq<PdfStamp>(
            new PdfStamp.Text(Value: "RASM", X: 36.0, Y: 36.0, HeightPoints: 12f, Font: new Font(familyName: "Arial"), Fill: DrawingColor.Black),
            new PdfStamp.Line(From: new DrawingPointF(36f, 48f), To: new DrawingPointF(220f, 48f), Stroke: DrawingColor.Black, Width: 1f))
        from mixedPdf in ctx.Expect(label: "mixed endpoint", projection: FileEndpoint.From(path: Path.Combine(path1: work, path2: "mixed.pdf")))
        from publishMixed in ctx.Expect(label: "publish mixed raster+vector pdf", projection: files.Run(operation: FileOp.Do(exchange: new FileExchange.Publish(new FilePublish(
            Target: new FilePublishTarget.Pdf(Target: mixedPdf, Annotate: stamps),
            Views: Seq(
                new FileView(Source: new FileViewSource.Pages(Query: new SheetQuery(Name: Some(sheetAName))), Recipe: new CaptureRecipe(Dpi: Some(72.0))),
                new FileView(Source: new FileViewSource.Pages(Query: new SheetQuery(Name: Some(sheetBName))), Recipe: new CaptureRecipe(Dpi: Some(72.0), Raster: true))),
            Layers: true)))))
        let publishMixedFact = Note(ctx: ctx, key: "publish.mixed.views", value: publishMixed.Views.Count)
        from publishMixedLaw in ctx.Require(label: "mixed publish emitted both pages", observed: publishMixed.Views.Count == 2)
        from mixedWritten in ctx.Require(label: "mixed.pdf written", observed: File.Exists(path: Path.Combine(path1: work, path2: "mixed.pdf")))

            // 3) named-view publish runs restore on the pre-resolved scope (A2, no nested dispatch)
        from namedPdf in ctx.Expect(label: "named endpoint", projection: FileEndpoint.From(path: Path.Combine(path1: work, path2: "named-view.pdf")))
        from publishNamed in ctx.Expect(label: "publish named view", projection: files.Run(operation: FileOp.Do(exchange: new FileExchange.Publish(new FilePublish(
            Target: new FilePublishTarget.Pdf(Target: namedPdf),
            Views: Seq(new FileView(Source: new FileViewSource.Named(Names: Seq(namedView), Target: new ViewportTarget.View(modelView)), Recipe: new CaptureRecipe(Dpi: Some(72.0)))))))))
        let publishNamedFact = Note(ctx: ctx, key: "publish.named.views", value: publishNamed.Views.Count)
        from publishNamedLaw in ctx.Require(label: "named-view publish emitted one page", observed: publishNamed.Views.Count == 1)
        from namedWritten in ctx.Require(label: "named-view.pdf written", observed: File.Exists(path: Path.Combine(path1: work, path2: "named-view.pdf")))

            // 5) sheet edits produce typed receipts; Resize mutates PageWidth in place
        let sheetC = Stamp(stem: "RasmSheetC")
        from createReceipt in Receipt(ctx: ctx, label: "sheet create", outcome: files.Run(operation: FileOp.Do(exchange: new FileExchange.SheetEdit(
            new FileSheetEdit.Create(new FileSheetSpec(Name: sheetC, Size: Some(new FileSheetSize(Units: pageUnits, Width: Some(297.0), Height: Some(210.0)))))))))
        from createLaw in ctx.Require(label: "sheet create one Created id", observed: createReceipt.Created.Count == 1)
        from createdC in PageByName(doc: doc, name: sheetC)
        let beforeWidth = createdC.PageWidth
        from resizeReceipt in Receipt(ctx: ctx, label: "sheet resize", outcome: files.Run(operation: FileOp.Do(exchange: new FileExchange.SheetEdit(
            new FileSheetEdit.Resize(SheetName: sheetC, Size: Some(new FileSheetSize(Units: pageUnits, Width: Some(420.0), Height: Some(297.0))))))))
        from resizeLaw in ctx.Require(label: "resize is AttributeChanged receipt", observed: resizeReceipt.AttributeChanged.Count == 1)
        from resizedC in PageByName(doc: doc, name: sheetC)
        let widthBeforeFact = Note(ctx: ctx, key: "sheet.width.before", value: beforeWidth)
        let widthAfterFact = Note(ctx: ctx, key: "sheet.width.after", value: resizedC.PageWidth)
        from widthLaw in ctx.Require(label: "PageWidth setter applied in place", observed: resizedC.PageWidth > beforeWidth)
        from reorderReceipt in Receipt(ctx: ctx, label: "sheet reorder", outcome: files.Run(operation: FileOp.Do(exchange: new FileExchange.SheetEdit(
            new FileSheetEdit.Reorder(SheetNames: Seq(sheetC, sheetAName, sheetBName))))))
        from reorderLaw in ctx.Require(label: "reorder rebinds three page numbers", observed: reorderReceipt.AttributeChanged.Count == 3)
        from detailReceipt in Receipt(ctx: ctx, label: "add detail", outcome: files.Run(operation: FileOp.Do(exchange: new FileExchange.SheetEdit(
            new FileSheetEdit.AddDetail(SheetName: sheetC, Spec: new FileDetailSpec(Name: "Plan", Corner: new Point2d(20.0, 20.0), Opposite: new Point2d(260.0, 180.0), Projection: DefinedViewportProjection.Top))))))
        from detailLaw in ctx.Require(label: "add detail one Created id", observed: detailReceipt.Created.Count == 1)
        from scaleReceipt in Receipt(ctx: ctx, label: "scale detail (explicit lengths)", outcome: files.Run(operation: FileOp.Do(exchange: new FileExchange.SheetEdit(
            new FileSheetEdit.ScaleDetail(SheetName: sheetC, Detail: DetailQuery.Named(name: "Plan"), Scale: new FileScale.Lengths(ModelLength: 1.0, ModelUnit: LengthUnit.Meters, PageLength: 1.0, PageUnit: LengthUnit.Millimeters))))))
        from scaleLaw in ctx.Require(label: "scale detail is AttributeChanged receipt", observed: scaleReceipt.AttributeChanged.Count == 1)

            // 5b) native named-scale parse + batch, page-wide Configure, Sheets inspection, numbering
        from namedScaleReceipt in Receipt(ctx: ctx, label: "scale detail named 1:100 (batch)", outcome: files.Run(operation: FileOp.Do(exchange: new FileExchange.SheetEdit(
            new FileSheetEdit.ScaleDetail(SheetName: sheetC, Detail: DetailQuery.All, Scale: new FileScale.Named(Value: "1:100"))))))
        from namedScaleLaw in ctx.Require(label: "named-scale batch touched a detail", observed: namedScaleReceipt.AttributeChanged.Count >= 1)
        from scaleReadback in NamedScaleReadback(ctx: ctx, doc: doc, sheetName: sheetC)
        from imperialReceipt in Receipt(ctx: ctx, label: "scale detail imperial fraction", outcome: files.Run(operation: FileOp.Do(exchange: new FileExchange.SheetEdit(
            new FileSheetEdit.ScaleDetail(SheetName: sheetC, Detail: DetailQuery.Named(name: "Plan"), Scale: new FileScale.Named(Value: "1/4\"=1'-0\""))))))
        from imperialLaw in ctx.Require(label: "imperial named scale applied", observed: imperialReceipt.AttributeChanged.Count == 1)
        let sheetGroup = Stamp(stem: "RasmGroup")
        from configureReceipt in Receipt(ctx: ctx, label: "configure sheet page-wide", outcome: files.Run(operation: FileOp.Do(exchange: new FileExchange.SheetEdit(
            new FileSheetEdit.Configure(
                Query: new SheetQuery(Name: Some(sheetC)),
                Config: new FileSheetConfig(
                    Description: Some("rasm configured sheet"),
                    Group: Some(sheetGroup),
                    GroupDescription: Some("rasm sheet set"),
                    UserStrings: Seq(new FileUserString(Key: "Discipline", Value: Some("A")))))))))
        from configureLaw in ctx.Require(label: "Configure applied to one matched page", observed: configureReceipt.AttributeChanged.Count == 1)
        from configureGroupLaw in ctx.Require(label: "Configure created page-view group", observed: configureReceipt.ResourceChanged.Exists(change => change.Kind == DocumentResourceKind.PageViewGroup))
        from sheetReports in ctx.Expect(label: "inspect sheetC", projection: files.Run(operation: FileOp.Sheets(query: new SheetQuery(Name: Some(sheetC)))))
        from inspectionLaw in ctx.Require(label: "Sheets inspection returns one report", observed: sheetReports.Count == 1)
        let sheetCReport = sheetReports[0]
        let inspectDetailFact = Note(ctx: ctx, key: "inspect.detailCount", value: sheetCReport.Details.Count)
        let inspectGroupFact = Note(ctx: ctx, key: "inspect.groupCount", value: sheetCReport.Groups.Count)
        from inspectDetailLaw in ctx.Require(label: "inspection sees the Plan detail", observed: sheetCReport.Details.Count >= 1)
        from inspectGroupLaw in ctx.Require(label: "inspection reports the configured group", observed: sheetCReport.Groups.Exists(name => string.Equals(a: name, b: sheetGroup, comparisonType: StringComparison.Ordinal)))
        let inspectScaleFact = Note(ctx: ctx, key: "inspect.detailScale.anySome", value: sheetCReport.Details.Exists(detail => detail.Scale.IsSome))
        from inspectScaleLaw in ctx.Require(label: "inspection reports persisted detail scale", observed: sheetCReport.Details.Exists(detail => detail.Scale.IsSome))
        let numberPrefix = $"{Stamp(stem: "RasmNum")}-"
        from numberReceipt in Receipt(ctx: ctx, label: "number sheet", outcome: files.Run(operation: FileOp.Do(exchange: new FileExchange.SheetEdit(
            new FileSheetEdit.Number(Query: new SheetQuery(Name: Some(sheetC)), Numbering: new FileNumbering(SheetPattern: $"{numberPrefix}{{n:000}}", Start: 7))))))
        from numberLaw in ctx.Require(label: "Number rebinds one matched page", observed: numberReceipt.AttributeChanged.Count == 1)
        from numberApplied in ctx.Require(label: "Number applied pattern with Start=7", observed: toSeq(doc.Views.GetPageViews()).Exists(page => string.Equals(a: page.PageName, b: $"{numberPrefix}007", comparisonType: StringComparison.Ordinal)))

            // 6) named layer state + named object position round-trip (B3)
        let layerState = Stamp(stem: "RasmLayerState")
        from layerReceipt in Receipt(ctx: ctx, label: "named layer state save", outcome: files.Run(operation: FileOp.Do(exchange: new FileExchange.NativeTable(new FileNativeTable.SaveLayerState(Name: layerState)))))
        from layerLaw in ctx.Require(label: "layer state saved", observed: layerReceipt.ResourceChanged.Exists(change => change.Kind == DocumentResourceKind.NamedLayerState))
        let position = Stamp(stem: "RasmPosition")
        from positionTarget in ctx.Expect(label: "position target", projection: DocumentTarget.Objects(objectIds: Seq(boxId)))
        from posSaveReceipt in Receipt(ctx: ctx, label: "named position save", outcome: files.Run(operation: FileOp.Do(exchange: new FileExchange.NativeTable(new FileNativeTable.SavePosition(Name: position, Objects: positionTarget)))))
        from posSaveLaw in ctx.Require(label: "named position saved", observed: posSaveReceipt.ResourceChanged.Exists(change => change.Kind == DocumentResourceKind.NamedPosition))
        from posRestoreReceipt in Receipt(ctx: ctx, label: "named position restore", outcome: files.Run(operation: FileOp.Do(exchange: new FileExchange.NativeTable(new FileNativeTable.Position(Name: position, Kind: FileNativePositionKind.Restore)))))
        from posRestoreLaw in ctx.Require(label: "named position restore receipt", observed: posRestoreReceipt.ResourceChanged.Count == 1)
        from posPresentLaw in ctx.Require(label: "named position present in table", observed: toSeq(doc.NamedPositions.Names).Exists(name => string.Equals(a: name, b: position, comparisonType: StringComparison.Ordinal)))
        from layerStateNames in ctx.Expect(label: "read named layer states", projection: files.Run(operation: FileOp.NamedLayerStates()))
        from layerListedLaw in ctx.Require(label: "NamedLayerStates lists the saved state", observed: layerStateNames.Exists(name => string.Equals(a: name, b: layerState, comparisonType: StringComparison.Ordinal)))
        from positionReports in ctx.Expect(label: "read named positions", projection: files.Run(operation: FileOp.NamedPositions()))
        from posListedLaw in ctx.Require(label: "NamedPositions lists the saved position", observed: positionReports.Exists(report => string.Equals(a: report.Name, b: position, comparisonType: StringComparison.Ordinal)))
        from posObjectLaw in ctx.Require(label: "named position report carries the box id", observed: positionReports.Exists(report => report.Objects.Exists(id => id == boxId)))
        from box2 in BoxBrep(x0: 30.0, x1: 50.0, y0: 30.0, y1: 50.0, z0: 0.0, z1: 10.0)
        let box2Id = doc.Objects.AddBrep(brep: box2)
        from appendTarget in ctx.Expect(label: "append target", projection: DocumentTarget.Objects(objectIds: Seq(box2Id)))
        from appendReceipt in Receipt(ctx: ctx, label: "named position append", outcome: files.Run(operation: FileOp.Do(exchange: new FileExchange.NativeTable(new FileNativeTable.AppendPosition(Name: position, Objects: appendTarget)))))
        from appendLaw in ctx.Require(label: "Append updates the named position", observed: appendReceipt.ResourceChanged.Exists(change => change.Kind == DocumentResourceKind.NamedPosition))

            // 7) earth anchor set + sun sync + projection round-trip
        let location = new FileGeoLocation(
            Latitude: Some(40.7128), Longitude: Some(-74.006), Elevation: Some(10.0),
            ElevationCoordinateSystem: EarthCoordinateSystem.MeanSeaLevel,
            ModelBasePoint: Some(Point3d.Origin), ModelNorth: Some(new Vector3d(0.0, 1.0, 0.0)), ModelEast: Some(new Vector3d(1.0, 0.0, 0.0)),
            Name: Some("RasmAnchor"), Description: Option<string>.None,
            KmlHeadingDegrees: Option<double>.None, KmlTiltDegrees: Option<double>.None, KmlRollDegrees: Option<double>.None)
        from anchorReceipt in ctx.Expect(label: "earth anchor set", projection: FileGeoLocation.Set(document: doc, location: location))
        from anchorLaw in ctx.Require(label: "earth anchor receipt", observed: anchorReceipt.ResourceChanged.Exists(change => change.Kind == DocumentResourceKind.EarthAnchor))
        from sunReceipt in ctx.Expect(label: "sync sun", projection: FileGeoLocation.SyncSun(document: doc))
        from sunLaw in ctx.Require(label: "sun synced from model north", observed: sunReceipt.ResourceChanged.Exists(change => change.Kind == DocumentResourceKind.Sun))
        let probePts = Seq(new Point3d(5.0, 5.0, 0.0), new Point3d(-3.0, 7.0, 2.0))
        from earth in ctx.Expect(label: "project to earth", projection: FileGeoLocation.ProjectToEarth(document: doc, points: probePts, modelUnits: LengthUnit.Meters))
        from back in ctx.Expect(label: "project to model", projection: FileGeoLocation.ProjectToModel(document: doc, coordinates: earth, modelUnits: LengthUnit.Meters))
        from countLaw in ctx.Require(label: "round-trip preserves point count", observed: back.Count == probePts.Count)
        let maxError = Math.Max(val1: probePts[0].DistanceTo(other: back[0]), val2: probePts[1].DistanceTo(other: back[1]))
        let errorFact = Note(ctx: ctx, key: "earth.roundtrip.maxError", value: maxError)
        from roundTripLaw in ctx.Require(label: "earth<->model round-trips within tolerance", observed: maxError < 1e-6)

            // 8) archive read/validate/inspect + ArchiveUpdate FileUserString round-trip (B4)
        let archivePath = Path.Combine(path1: work, path2: "archive.3dm")
        from archiveEndpoint in ctx.Expect(label: "archive endpoint", projection: FileEndpoint.From(path: archivePath))
        from written in ctx.Expect(label: "write 3dm", projection: files.Run(operation: FileOp.Do(exchange: new FileExchange.Write(Phase: FilePhase.Write3dmFile, Target: archiveEndpoint, Profile: FileProfile.Model))))
        from archiveWrittenLaw in ctx.Require(label: "archive.3dm written", observed: File.Exists(path: archivePath))
        from archiveSourceEndpoint in ctx.Expect(label: "archive source endpoint", projection: FileEndpoint.From(path: archivePath))
        let archiveSource = (FileArchiveSource)new FileArchiveSource.Path(Value: archiveSourceEndpoint)
        from readReport in ctx.Expect(label: "archive read", projection: files.Run(operation: FileOp.Do(exchange: new FileExchange.ArchiveRead(Source: archiveSource, Profile: ArchiveProfile.Full))))
        from readArchive in ctx.Expect(label: "archive snapshot", projection: readReport.Archive.ToFin(Fail: Error.New(message: "archive snapshot missing")))
        let archiveObjectsFact = Note(ctx: ctx, key: "archive.objects", value: readArchive.Resources.Objects)
        let archiveRelationsFact = Note(ctx: ctx, key: "archive.relations", value: readArchive.Resources.Relations)
        from graphLaw in ctx.Require(label: "graph walk sees the box object", observed: readArchive.Resources.Objects >= 1)
        from validated in ctx.Expect(label: "archive validate", projection: files.Run(operation: FileOp.Do(exchange: new FileExchange.ArchiveValidate(Source: archiveSource, Profile: ArchiveProfile.Full))))
        from inspectEndpoint in ctx.Expect(label: "inspect endpoint", projection: FileEndpoint.From(path: archivePath))
        from inspectReport in ctx.Expect(label: "archive inspect", projection: files.Run(operation: FileOp.Do(exchange: new FileExchange.ArchiveInspect(Source: inspectEndpoint))))
        from inspected in ctx.Expect(label: "inspect snapshot", projection: inspectReport.Archive.ToFin(Fail: Error.New(message: "inspect snapshot missing")))
        let versionFact = Note(ctx: ctx, key: "archive.version", value: inspected.Metadata.ArchiveVersion)
        from versionLaw in ctx.Require(label: "archive metadata version read", observed: inspected.Metadata.ArchiveVersion > 0)
        let updatedPath = Path.Combine(path1: work, path2: "archive-updated.3dm")
        let patch = new FileArchiveMetadataPatch(
            Notes: Some("rasm verify notes"), ApplicationName: Option<string>.None, ApplicationUrl: Option<string>.None, ApplicationDetails: Option<string>.None,
            StartComments: Option<string>.None, UserStrings: Seq(new FileUserString(Key: "RasmKey", Section: Some("RasmSection"), Value: Some("RasmValue"))))
        from updatedEndpoint in ctx.Expect(label: "updated endpoint", projection: FileEndpoint.From(path: updatedPath))
        from updated in ctx.Expect(label: "archive update with user string", projection: files.Run(operation: FileOp.Do(exchange: new FileExchange.ArchiveUpdate(
            Source: archiveSource, Target: updatedEndpoint, Update: new ArchiveUpdate(Metadata: Some(patch)), Profile: ArchiveProfile.Full))))
        from updatedWrittenLaw in ctx.Require(label: "updated archive written", observed: File.Exists(path: updatedPath))
        from rereadEndpoint in ctx.Expect(label: "reread endpoint", projection: FileEndpoint.From(path: updatedPath))
        from rereadReport in ctx.Expect(label: "reread updated archive", projection: files.Run(operation: FileOp.Do(exchange: new FileExchange.ArchiveRead(
            Source: new FileArchiveSource.Path(Value: rereadEndpoint), Profile: ArchiveProfile.Full))))
        from rereadArchive in ctx.Expect(label: "reread snapshot", projection: rereadReport.Archive.ToFin(Fail: Error.New(message: "reread snapshot missing")))
        let graphRoundTrip = rereadArchive.Resources.Entries.Exists(entry =>
            entry.Name.Exists(name => string.Equals(a: name, b: "RasmKey", comparisonType: StringComparison.Ordinal))
            && entry.Value.Exists(value => string.Equals(a: value, b: "RasmValue", comparisonType: StringComparison.Ordinal)))
        let graphFact = Note(ctx: ctx, key: "archive.userString.graph", value: graphRoundTrip)
        from graphRoundTripLaw in ctx.Require(label: "FileUserString round-trips through the graph", observed: graphRoundTrip)
        from directRead in DirectArchiveRead(ctx: ctx, path: updatedPath)

            // 9) export-selected to OBJ via the typed FileObj writer (A3 + non-interactive fix)
        let objPath = Path.Combine(path1: work, path2: "selected.obj")
        from exportTarget in ctx.Expect(label: "export target", projection: DocumentTarget.Objects(objectIds: Seq(boxId)))
        from objEndpoint in ctx.Expect(label: "obj endpoint", projection: FileEndpoint.From(path: objPath))
        from exportReceipt in Receipt(ctx: ctx, label: "export selected obj", outcome: files.Run(operation: FileOp.Do(exchange: new FileExchange.Export(
            Target: objEndpoint, Objects: Some(exportTarget), Profile: FileProfile.Model))))
        let exportFact = Note(ctx: ctx, key: "export.selected", value: exportReceipt.Selected.Count)
        from exportLaw in ctx.Require(label: "export-selected reports one object", observed: exportReceipt.Selected.Count == 1)
        from objWrittenLaw in ctx.Require(label: "selected.obj written non-interactively", observed: File.Exists(path: objPath))

            // cleanup: leave the document as found
        let cleaned = Cleanup(doc: doc, namedView: namedView, position: position, layerState: layerState)
        let cleanupFact = Note(ctx: ctx, key: "cleanup.namedViews", value: doc.NamedViews.Count)
        select Done(scope: scope);

    private static Unit Activate(RhinoDoc doc, RhinoView view) {
        doc.Views.ActiveView = view;
        return unit;
    }

    [SuppressMessage(category: "Reliability", checkId: "CA2000", Justification = "Ownership transfers into the Fin rail; the document copies the geometry and the transient brep is finalizer-released.")]
    private static Fin<Brep> BoxBrep(double x0, double x1, double y0, double y1, double z0, double z1) =>
        Optional(Brep.CreateFromBox(new Box(
            Plane.WorldXY,
            new Interval(t0: x0, t1: x1),
            new Interval(t0: y0, t1: y1),
            new Interval(t0: z0, t1: z1))))
        .ToFin(Fail: Error.New(message: "box brep construction failed"));

    // BOUNDARY ADAPTER — document restoration: deletes the Rasm-stamped named resources, closes
    // pages, resets the earth anchor, and clears objects so the long-lived host doc stays clean.
    private static Unit Cleanup(RhinoDoc doc, string namedView, string position, string layerState) {
        _ = doc.NamedViews.Delete(name: namedView);
        _ = doc.NamedPositions.Delete(name: position);
        _ = doc.NamedLayerStates.Delete(name: layerState);
        _ = ClosePages(doc: doc);
        using (EarthAnchorPoint reset = new()) {
            doc.EarthAnchorPoint = reset;
        }
        doc.Objects.Clear();
        doc.Views.Redraw();
        return unit;
    }

    private static Unit ClosePages(RhinoDoc doc) {
        _ = toSeq(doc.Views.GetPageViews()).Iter(page => page.Close());
        return unit;
    }

    // BOUNDARY ADAPTER — File3dm.Read returns a null-on-failure native handle; the bracket owns
    // disposal across the user-string and notes confirmation reads.
    private static Fin<Unit> DirectArchiveRead(ScenarioContext ctx, string path) {
        using global::Rhino.FileIO.File3dm? updated = global::Rhino.FileIO.File3dm.Read(path: path);
        if (updated is null) {
            return Fin.Fail<Unit>(error: Error.New(message: $"updated archive unreadable: {path}"));
        }
        string direct = updated.Strings.GetValue(section: "RasmSection", entry: "RasmKey") ?? "<null>";
        _ = Note(ctx: ctx, key: "archive.userString.direct", value: direct);
        return ctx.Require(label: "FileUserString confirmed via direct File3dm read", observed: string.Equals(a: direct, b: "RasmValue", comparisonType: StringComparison.Ordinal))
            .Bind(ignored => ctx.Require(label: "metadata Notes patch applied", observed: string.Equals(a: updated.Notes.Notes, b: "rasm verify notes", comparisonType: StringComparison.Ordinal)));
    }

    private static Unit Done(DocumentScope scope) {
        scope.Dispose();
        return unit;
    }

    // BOUNDARY ADAPTER — GetFormattedScale carries an out parameter; the kernel facts the
    // formatted scale and the page/model ratio from a freshly re-queried detail.
    private static Fin<Unit> NamedScaleReadback(ScenarioContext ctx, RhinoDoc doc, string sheetName) =>
        PageByName(doc: doc, name: sheetName).Bind(page =>
            ctx.Require(label: "sheetC has a detail after named scale", observed: page.GetDetailViews().Length >= 1)
                .Bind(ignored => ScaleFacts(ctx: ctx, detail: page.GetDetailViews()[0])));

    private static T Note<T>(ScenarioContext ctx, string key, T value) {
        ctx.Fact(key: key, value: value);
        return value;
    }

    private static Fin<RhinoPageView> PageByName(RhinoDoc doc, string name) =>
        toSeq(doc.Views.GetPageViews())
            .Find(page => string.Equals(a: page.PageName, b: name, comparisonType: StringComparison.Ordinal))
            .ToFin(Fail: Error.New(message: $"page missing: {name}"));

    private static Fin<DocumentReceipt> Receipt(ScenarioContext ctx, string label, Fin<FileReport> outcome) =>
        ctx.Expect(label: label, projection: outcome)
            .Bind(report => report.Receipt.ToFin(Fail: Error.New(message: $"{label}: receipt missing")));

    private static Unit Redraw(RhinoDoc doc) {
        doc.Views.Redraw();
        return unit;
    }

    private static Fin<Unit> ScaleFacts(ScenarioContext ctx, DetailViewObject detail) {
        bool formatted = detail.GetFormattedScale(format: DetailViewObject.ScaleFormat.OneToModelLength, value: out string namedFmt);
        _ = Note(ctx: ctx, key: "detail.named.formattedScale", value: namedFmt ?? "<unset>");
        double namedRatio = detail.DetailGeometry?.PageToModelRatio ?? -1.0;
        _ = Note(ctx: ctx, key: "detail.named.pageToModelRatio", value: namedRatio);
        return ctx.Require(label: "re-queried detail carries a formatted scale", observed: formatted)
            .Bind(ignored => ctx.Require(label: "FileScale.Named persists a reduction ratio", observed: namedRatio is > 0.0 and < 1.0));
    }

    private static string Stamp(string stem) =>
        $"{stem}{Guid.NewGuid():N}";


    private static string WorkDirectory() {
        string work = Path.Combine(path1: Path.GetTempPath(), path2: $"rasm-exchange-{Guid.NewGuid():N}");
        _ = Directory.CreateDirectory(path: work);
        return work;
    }
}
