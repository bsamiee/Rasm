using Arches;
using Rhino.Arches.Core.Extensions;
using Rhino.Arches.Core.Wrappers;
using Rhino.Geometry;


namespace Rhino.Arches.Core
{
    public abstract class RhinoArchBase : ArchBase
    {
        public Point3d ApexPoint { get; protected set; }
        public Point3d StartPoint { get; protected set; }
        public Point3d EndPoint { get; protected set; }
        public ArcCenterLineResult CenterLineResult { get; protected set; }
        public List<Arc> Arcs { get; protected set; } = new List<Arc>();
        public List<Curve> Curves { get; protected set; } = new List<Curve>();

        public override void DrawProfile()
        {
            var archCurves = JoinArchParts();
            archCurves?.ForEach(ac =>
            {

                RhinoDoc.ActiveDoc.Objects.AddCurve(ac);

            });

            //Arcs.ForEach(ac => {

            //    RhinoDoc.ActiveDoc.Objects.AddPoint(ac.Center);

            //});
        }

        List<Curve> JoinArchParts()
        {
            if (Arcs.Count > 0)
            {
                var curves = this.Arcs.Select(a => a.ToNurbsCurve());

                var joinedCurves = Curve.JoinCurves(curves, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

                return joinedCurves.ToList();
            }
            else if (Curves.Count > 0)
            {
                var joinedCurves = Curve.JoinCurves(Curves, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

                return joinedCurves.ToList();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spanStart"></param>
        /// <param name="spanEnd"></param>
        /// <param name="sideArcs">all arcs placed on left or right side, these will be mirrored</param>
        /// <param name="centerResult"></param>
        protected void MirrorBuild(Point3d spanStart, Point3d spanEnd, List<Arc> sideArcs, ArcCenterLineResult centerResult)
        {
            this.CenterLineResult = centerResult;


            foreach (var a in sideArcs)
            {
                var mirrorArc = a.MirrorArc(this.CenterLineResult.SpringCenter, spanStart, spanEnd);

                if (mirrorArc.HasValue)
                {
                    this.Arcs.Add(a);
                    this.Arcs.Add(mirrorArc.Value);
                }
            }
        }

    }
}
