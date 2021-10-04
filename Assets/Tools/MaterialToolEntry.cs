using Tools;
using UnityEngine;

namespace LevelObjects
{
	[CreateAssetMenu(fileName = "mat", menuName = "Tool Entries/material", order = 1)]
	public class MaterialToolEntry : ToolEntry
	{
		[SerializeField] private Material material;
		[SerializeField] private float density;
		[SerializeField] private PhysicsMaterial2D physicsMaterial;

		public Material Material => material;
		public float Density => density;
		public PhysicsMaterial2D PhysicsMaterial => physicsMaterial;

		public override void ToolAction()
		{
			
		}
	}
}