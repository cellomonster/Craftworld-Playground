using Tools;
using UnityEngine;

namespace Toolbox
{
	public class TooltrayTabButton : MonoBehaviour
	{
		[SerializeField] private Tooltray tooltray;

		[SerializeField] private TooltayUIManager tooltayUIManager;

		public void OpenTooltray()
		{
			tooltayUIManager.SetActiveTooltray(tooltray);
		}
	}
}
