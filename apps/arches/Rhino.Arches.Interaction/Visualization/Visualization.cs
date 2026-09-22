using Rhino.Arches.Core;
using Rhino.Arches.Core.Circular;
using Rhino.Arches.Core.HorseShoe;
using Rhino.Display;
using Rhino.Geometry;

namespace Rhino.Arches.Interaction.Visualization
{
    public static class Visualization
    {

        public static void VisualizeArc(this Arc arc, DrawEventArgs e,
            bool drawEndPoints = true,
            bool drawSpringLine = true,
            bool drawCenterPoint = true,
            bool drawArcQuadPoint = true)
        {
            var startPt = arc.StartPoint;
            var endPt = arc.EndPoint;
            e.Display.DrawArc(arc, ColorConstants.ArchArc, 2);
            if (drawEndPoints)
            {
                e.Display.DrawPoint(endPt, PointStyle.X, 3, ColorConstants.RefGeom);
                e.Display.DrawPoint(startPt, PointStyle.X, 3, ColorConstants.RefGeom);
            }

            if (drawCenterPoint)
                e.Display.DrawPoint(arc.Center, PointStyle.X, 3, ColorConstants.ArchCenterPoint);

            if (drawArcQuadPoint)
                e.Display.DrawPoint(arc.MidPoint, PointStyle.X, 3, ColorConstants.ArcQuad);


            if (drawSpringLine)
                e.Display.DrawLine(startPt, endPt, ColorConstants.RefGeom, 1);

        }

        public static void Visualize(this RhinoArchBase arch, DrawEventArgs e)
        {
            
            arch.Arcs.ForEach(a => a.VisualizeArc(e,drawEndPoints:true,drawCenterPoint:true));
            arch.Curves.ForEach(c => e.Display.DrawCurve(c, ColorConstants.ArchArc, 2));
               
           e.Display.DrawLine(arch.CenterLineResult.CenterLine, ColorConstants.RefGeom, 1);
           e.Display.DrawPoint(arch.CenterLineResult.SpringCenter, PointStyle.X, 3, ColorConstants.ArcSpringCenter);
        }



        public static void Visualize(this CircularArchBase seg, DrawEventArgs e)
        {
            seg.Arc.VisualizeArc(e);
            e.Display.DrawLine(seg.CenterLineResult.CenterLine, ColorConstants.RefGeom, 1);
            e.Display.DrawPoint(seg.CenterLineResult.SpringCenter, PointStyle.X, 3, ColorConstants.ArcSpringCenter);
        }

        public static void Visualize(this PointedArch arch, DrawEventArgs e)
        {
            arch.StartSide_Arc.VisualizeArc(e, drawEndPoints: false, drawCenterPoint: true,
                drawArcQuadPoint: false, drawSpringLine: false);
            arch.EndSide_Arc.VisualizeArc(e, drawEndPoints: false, drawCenterPoint: true,
                drawArcQuadPoint: false, drawSpringLine: false);

            e.Display.DrawLine(arch.StartSide_Arc.StartPoint, arch.EndSide_Arc.StartPoint, ColorConstants.RefGeom, 1);

            e.Display.DrawLine(arch.CenterLineResult.CenterLine, ColorConstants.RefGeom, 1);
            e.Display.DrawPoint(arch.CenterLineResult.SpringCenter, PointStyle.X, 3, ColorConstants.ArcSpringCenter);
            e.Display.DrawPoint(arch.ApexPoint, PointStyle.X, 3, ColorConstants.ArcQuad);
            e.Display.DrawCircle(arch.ApexCircle_1, ColorConstants.RefGeom, 1);
            e.Display.DrawCircle(arch.C1, ColorConstants.RefGeom, 1);
        }


        public static void VisualizeArchEndPoints(this DrawEventArgs e, Point3d startPt, Point3d endPt)
        {
            e.Display.DrawPoint(startPt, PointStyle.X, 3, ColorConstants.RefGeom);
            e.Display.DrawPoint(endPt, PointStyle.X, 3, ColorConstants.RefGeom);
            e.Display.DrawLine(startPt, endPt, ColorConstants.RefGeom, 1);
        }
    }
}
