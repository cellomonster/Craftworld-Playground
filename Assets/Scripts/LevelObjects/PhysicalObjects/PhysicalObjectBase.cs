using ClipperLib;
using UnityEngine;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;

namespace LevelObjects.PhysicalObjects
{
	public abstract class PhysicalObjectBase : LevelObjectBase
	{
		private PhysicalGroup group;
		
		public PhysicalGroup Group
		{
			get => group;
			set => group = value;
		}

		protected override void RebuildColliderFromObjectShape()
		{
			base.RebuildColliderFromObjectShape();

			ClipperOffset offset = new ClipperOffset();

			offset.AddPaths(Shape, JoinType.jtMiter, EndType.etClosedPolygon);
			Paths offsetShapeByDefaultContactOffset = new Paths();
			offset.Execute(ref offsetShapeByDefaultContactOffset,
				-Physics2D.defaultContactOffset * Global.ClipperPrecision);

			for (int i = 0; i < offsetShapeByDefaultContactOffset.Count; i++)
			{
				Path p = offsetShapeByDefaultContactOffset[i];

				Vector2[] np = new Vector2[p.Count];
				for (int j = 0; j < p.Count; j++)
					np[j] = p[j];

				PolyCollider.SetPath(i, np);
			}

			gameObject.layer = LayerMask.NameToLayer($"Layers {FrontLayer}-{BackLayer}");
		}
	}
}