using ClipperLib;
using UnityEngine;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;

namespace LevelObjects
{
	public abstract class LevelObjectBase : MonoBehaviour
	{
		public Paths Shape;

		protected PolygonCollider2D PolyCollider { get; private set; }

		private int frontLayer = 1;
		private int backLayer = 1;

		/// <summary>
		/// Changes the front layer the object occupies
		/// </summary>
		public int FrontLayer
		{
			get => frontLayer;
			set
			{
				frontLayer = Mathf.Clamp(value, 1, Global.Layers);
				backLayer = Mathf.Clamp(backLayer, frontLayer, Global.Layers);
			}
		}

		/// <summary>
		/// Changes the back layer the object occupies
		/// </summary>
		public int BackLayer
		{
			get { return backLayer; }
			set
			{
				backLayer = Mathf.Clamp(value, 1, Global.Layers);
				frontLayer = Mathf.Clamp(frontLayer, 1, backLayer);
			}
		}

		/// <summary>
		/// Describes the number of layers the object occupies
		/// </summary>
		public int Thickness
		{
			get => backLayer - frontLayer + 1;
			set => BackLayer = frontLayer + value - 1;
		}

		/// <summary>
		/// Easy access to position
		/// </summary>
		public Vector2 Position
		{
			get => transform.position;
			set => transform.position = value;
		}

		/// <summary>
		/// Rotation angle of the object
		/// </summary>
		public float Rotation
		{
			get => transform.eulerAngles.z;
			set => transform.eulerAngles = new Vector3(0, 0, value);
		}

		public void Scale(float factor)
		{
			foreach (Path p in Shape)
			{
				for (int i = 0; i < p.Count; i++)
				{
					p[i] = new IntPoint(p[i].X * factor, p[i].Y * factor);
				}
			}
		}

		protected virtual void Awake()
		{
			PolyCollider = gameObject.AddComponent<PolygonCollider2D>();
			Shape = new Paths();
		}

		/// <summary>
		/// Initializes an object's shape and occupied layers
		/// </summary>
		/// <param name="shape"></param>
		/// <param name="frontLayer"></param>
		/// <param name="backLayer"></param>
		public void Setup(Paths shape, int frontLayer, int backLayer)
		{
			SetLayers(frontLayer, backLayer);
			Shape = shape;
		}

		/// <summary>
		/// Shifts the object forward or backward a number of layers
		/// </summary>
		/// <param name="delta">Number of layers and direction in which the object is shifted</param>
		public void LayerShift(int delta)
		{
			int t = Thickness;
			FrontLayer += delta;
			Thickness = t;
		}

		/// <summary>
		/// Sets an object's occupied layers
		/// </summary>
		/// <param name="frontLayer"></param>
		/// <param name="backLayer"></param>
		private void SetLayers(int frontLayer, int backLayer)
		{
			this.backLayer = backLayer;
			FrontLayer = frontLayer;
		}

		/// <summary>
		/// Checks if another object overlaps on the layer plane
		/// </summary>
		/// <param name="l">object to compare layers with</param>
		/// <returns></returns>
		public bool OverlapsOnLayers(LevelObjectBase l)
		{
			return frontLayer <= l.backLayer && l.frontLayer <= backLayer;
		}

		/// <summary>
		/// Applies any previously called modifications to the object's shape
		/// </summary>
		public virtual void ApplyShapeModifications()
		{
			// RecenterShape();
			RebuildColliderFromObjectShape();
		}

		// private void RecenterShape()
		// {
		// 	IntRect aabb = Clipper.GetBounds(Shape);
		// 	IntPoint negBbCenter = new IntPoint(-(aabb.right + aabb.left) / 2, -(aabb.top + aabb.bottom) / 2);
		//
		// 	Shape = Global.OffsetPaths(Shape, negBbCenter);
		//
		// 	transform.localPosition -= negBbCenter;
		// }

		/// <summary>
		/// Rebuilds the shape of the object's collider
		/// </summary>
		protected virtual void RebuildColliderFromObjectShape()
		{
			SetCollisionShape(Shape);
		}

		protected void SetCollisionShape(Paths paths)
		{
			PolyCollider.pathCount = paths.Count;

			for (int i = 0; i < paths.Count; i++)
			{
				Path p = paths[i];

				Vector2[] np = new Vector2[p.Count];
				for (int j = 0; j < p.Count; j++)
					np[j] = p[j];

				PolyCollider.SetPath(i, np);
			}
		}
	}
}