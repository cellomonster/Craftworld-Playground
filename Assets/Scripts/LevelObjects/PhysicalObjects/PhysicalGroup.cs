using ClipperLib;
using UnityEngine;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;

namespace LevelObjects.PhysicalObjects
{
	public enum PhysicalGroupState
	{
		Physical = 0,
		Frozen,
		Held
	}
	
	public class PhysicalGroup : GroupBase<PhysicalObjectBase>
	{
		protected PolyTree MetaShape;
		
		private PhysicalGroupState state;

		public PhysicalGroupState State
		{
			get => state;
			set
			{
				state = value;
				switch (state)
				{
					case PhysicalGroupState.Physical:
						Rigidbody.isKinematic = false;
						Rigidbody.gravityScale = 1;
						break;
					case PhysicalGroupState.Frozen:
						Rigidbody.isKinematic = true;
						Rigidbody.velocity = Vector2.zero;
						Rigidbody.angularVelocity = 0;
						break;
					case PhysicalGroupState.Held:
						Rigidbody.isKinematic = false;
						Rigidbody.gravityScale = 0;
						Rigidbody.velocity = Vector2.zero;
						Rigidbody.angularVelocity = 0;
						break;
				}
			}
		}

		protected override void Awake()
		{
			base.Awake();
			MetaShape = new PolyTree();
			Rigidbody.isKinematic = false;
			Rigidbody.useAutoMass = true;
			Rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		}
		
		public void Glue()
		{
			// PhysicalGroup targetGroup;
			// List<PhysicalGroup> overlappingGroups = RetrieveOverlappingGroups();
			// for (int i = overlappingGroups.Count - 1; i > -1; i--)
			// {
			// 	MergeWithGroup(overlappingGroups[i]);
			// }
		}
		
		public void MergeWithGroup(PhysicalGroup group)
		{
			if (group != this)
			{
				for (int i = group.ContainedObjects.Count - 1; i > -1; i--)
				{
					AddLevelObject(group.ContainedObjects[i]);
				}

				Destroy(group.gameObject);
			}
		}

		public override void Rebuild()
		{
			// Rebuild MetaShape
			MetaShape.Clear();
			
			// rigidbody MUST be dynamic, as materialobject.applyshapemodifications
			// updates the density of colliders, which requires a dynamic rigidbody (ugh)
			bool wasKinematic = Rigidbody.isKinematic;
			Rigidbody.isKinematic = false;
			
			for (int i = 0; i < ContainedObjects.Count; i++)
			{
				PhysicalObjectBase containedObject = ContainedObjects[i];

				containedObject.ApplyShapeModifications();
			}
			
			// MoveGroupOriginToCenterOfMass();

			Clipper c = new Clipper();
			Paths newMetaShapeNotOffset = new Paths();
			
			foreach (PhysicalObjectBase containedObject in ContainedObjects)
			{
				Paths transformedPaths = new Paths(containedObject.Shape.Count);
				foreach (Path path in containedObject.Shape)
				{
					Path transformedPath = new Path(path.Count);
					transformedPaths.Add(transformedPath);
					foreach (IntPoint intPoint in path)
					{
						Vector2 v = intPoint;
						v = containedObject.transform.TransformPoint(v);
						v = transform.InverseTransformPoint(v);
						transformedPath.Add(v);
					}
				}
				
				c.AddPaths(transformedPaths, PolyType.ptClip, true);
			}
			
			c.Execute(ClipType.ctUnion, newMetaShapeNotOffset, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
			
			ClipperOffset offset = new ClipperOffset();
			offset.AddPaths(newMetaShapeNotOffset, JoinType.jtMiter, EndType.etClosedPolygon);
			offset.Execute(ref MetaShape, 0.005f * Global.ClipperPrecision);

			Rigidbody.isKinematic = wasKinematic;

			// any separated paths mean that a new group should be created
			for (int i = MetaShape.ChildCount - 1; i > 0; i--)
			{
				PolyNode removedNode = MetaShape.Childs[i];
				MetaShape.Childs.RemoveAt(i);
				PhysicalGroup physicalGroup = LevelObjectCreator.CreatePhysicalGroup(Position, Rotation);
				physicalGroup.State = State;
				PolyTree p = new PolyTree();
				p.AddChild(removedNode);
				physicalGroup.MetaShape = p;

				// this is incredibly complicated. too bad!
				// loop through each object to find which ones belong in the newly created group
				for (int j = ContainedObjects.Count - 1; j > -1; j--)
				{
					PhysicalObjectBase containedObject = ContainedObjects[j];
					// check if one of the vertices of each levelobject sit within
					// the shape of the newly created group
					IntPoint intPoint = new IntPoint(containedObject.Shape[0][0].X,
						containedObject.Shape[0][0].Y);
					// transform the point to be local to the newly created group
					Vector2 v = intPoint;
					v = ContainedObjects[j].transform.TransformPoint(v);
					v = physicalGroup.transform.InverseTransformPoint(v);
					if (Clipper.PointInPolygon(v, removedNode.Contour) != 0)
					{
						physicalGroup.AddLevelObject(ContainedObjects[j]);
					}
				}
				physicalGroup.Rebuild();
			}
		}

		private void MoveGroupOriginToCenterOfMass()
		{
			// recenter all objects around the group's center of mass
			Vector2 centerOfMassAndPositionDifference = Rigidbody.worldCenterOfMass - Position;
			foreach (PhysicalObjectBase levelObject in ContainedObjects)
			{
				levelObject.Position -= centerOfMassAndPositionDifference;
			}

			Position += centerOfMassAndPositionDifference;
		}

		public void MoveTo(Vector2 position)
		{
			Rigidbody.velocity = Vector2.zero;
			Rigidbody.MovePosition(position);
		}
		
		public void RotateTo(float angle)
		{
			Rigidbody.angularVelocity = 0;
			Rigidbody.MoveRotation(angle);
		}

		public void MoveBy(Vector2 displacement)
		{
			Rigidbody.velocity = Vector2.zero;
			Rigidbody.MovePosition((Vector2)transform.position + displacement);
		}
		
		public void RotateBy(float displacement)
		{
			Rigidbody.angularVelocity = 0;
			Rigidbody.MoveRotation(transform.eulerAngles.z + displacement);
		}

		public override void AddLevelObject(PhysicalObjectBase physicalObject)
		{
			base.AddLevelObject(physicalObject);
			physicalObject.Group = this;
		}

		protected override void RemoveLevelObject(PhysicalObjectBase physicalObject)
		{
			base.RemoveLevelObject(physicalObject);
			physicalObject.Group = null;
		}
		
		#if UNITY_EDITOR
		private void Update()
		{
			if (MetaShape.Childs.Count == 0) return;

			Vector2 v1, v2;

			for (int i = 0; i < MetaShape.Childs[0].Contour.Count - 1; i++)
			{
				v1 = MetaShape.Childs[0].Contour[i];
				v2 = MetaShape.Childs[0].Contour[i + 1];

				v1 = transform.TransformPoint(v1);
				v2 = transform.TransformPoint(v2);
				Debug.DrawLine(v1, v2, Color.red);
			}
			v1 = MetaShape.Childs[0].Contour[0];
			v2 = MetaShape.Childs[0].Contour[MetaShape.Childs[0].Contour.Count - 1];

			v1 = transform.TransformPoint(v1);
			v2 = transform.TransformPoint(v2);

			Debug.DrawLine(v1, v2, Color.red);
		}
		#endif
	}
}
