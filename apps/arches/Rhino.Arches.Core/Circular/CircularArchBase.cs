
using Rhino.Arches.Core.Wrappers;
using Rhino.Geometry;

namespace Rhino.Arches.Core.Circular
{
    public abstract class CircularArchBase : RhinoArchBase
    {
        public Arc Arc => this.Arcs[0];

        public CircularArchBase(Arc _arc, ArcCenterLineResult _result)
        {
            this.Arcs.Add(_arc);
            CenterLineResult = _result;
        }

    }
}
