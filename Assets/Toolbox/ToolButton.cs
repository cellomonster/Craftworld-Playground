using Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Toolbox
{
	public class ToolButton : MonoBehaviour
	{
		[SerializeField] private Button button;
		[SerializeField] private Image iconImage;
		private ToolEntry toolEntry;

		public void SetToolEntry(ToolEntry entry)
		{
			if (entry == null)
			{
				button.enabled = false;
				iconImage.enabled = false;
				return;
			}

			button.enabled = true;
			iconImage.enabled = true;
			
			iconImage.sprite = entry.Icon;

			toolEntry = entry;
		}

		public void UseTool()
		{
			toolEntry.ToolAction();
		}
	}
}
