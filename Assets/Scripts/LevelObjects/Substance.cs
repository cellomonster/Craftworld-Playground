using UnityEngine;

namespace LevelObjects
{
	[CreateAssetMenu(fileName = "Material", menuName = "Substance", order = 1)]
	public class Substance : ScriptableObject
	{
		public Material material;
		public float density;
		public PhysicsMaterial2D physicsMaterial;
		public Texture2D icon;
	}
}