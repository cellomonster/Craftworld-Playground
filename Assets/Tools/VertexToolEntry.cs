using UnityEngine;

namespace Tools
{
	[CreateAssetMenu(fileName = "mat", menuName = "Tool Entries/Vertex tool", order = 1)]
	public class VertexToolEntry : ToolEntry
	{
		public override void ToolAction()
		{
			Debug.Log("used tool!");
		}
	}
}
