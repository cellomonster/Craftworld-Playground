using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
	[CreateAssetMenu(fileName = "Dataset", menuName = "Dataset", order = 1)]
	public class Tooltray : ScriptableObject
	{
		[SerializeField] private List<ToolEntry> entries = new List<ToolEntry>();

		public ToolEntry GetEntry(int id)
		{
			return entries[id];
		}

		public int Count => entries.Count;
	}
}
