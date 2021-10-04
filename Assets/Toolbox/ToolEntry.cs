using UnityEngine;
using UnityEngine.Serialization;

namespace Tools
{
	public abstract class ToolEntry : ScriptableObject
	{
		[FormerlySerializedAs("name")] [SerializeField] protected new string toolName;
		[SerializeField] protected string description;
		[SerializeField] protected Sprite icon;

		public string ToolName => toolName;
		public string Description => description;
		public Sprite Icon => icon;

		public abstract void ToolAction();
	}
}
