using System.Collections.Generic;
using LevelObjects.PhysicalObjects;
using UnityEngine;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;

namespace LevelObjects.BrushObjects
{
	public abstract class BrushObjectBase : LevelObjectBase
	{
		public List<PhysicalObjectBase> TriggerOverlaps { get; protected set; }
		
		protected override void Awake()
		{
			base.Awake();
			PolyCollider.isTrigger = true;
			TriggerOverlaps = new List<PhysicalObjectBase>();
			gameObject.layer = LayerMask.NameToLayer("Layers 1-5");
		}

		/// <summary>
		/// Stamps a new object into the scene and adds it into the respective group
		/// </summary>
		/// <param name="groupEditing">Group to add the new object to</param>
		public abstract void Stamp(PhysicalGroup groupEditing);

		/// <summary>
		/// Cuts away at any overlapping objects
		/// </summary>
		public abstract void Cut();

		protected void OnTriggerEnter2D(Collider2D collision)
		{
			PhysicalObjectBase potentialPhysicalObject =
				collision.GetComponentInParent<PhysicalObjectBase>();
			if (potentialPhysicalObject && !TriggerOverlaps.Contains(potentialPhysicalObject))
			{
				TriggerOverlaps.Add(potentialPhysicalObject);
			}
		}

		protected void OnTriggerExit2D(Collider2D collision)
		{
			PhysicalObjectBase potentialPhysicalObject =
				collision.GetComponentInParent<PhysicalObjectBase>();
			if (potentialPhysicalObject)
			{
				TriggerOverlaps.Remove(potentialPhysicalObject);
			}
		}
	}
}