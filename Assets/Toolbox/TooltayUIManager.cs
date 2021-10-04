using System.Collections.Generic;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Toolbox
{
	public class TooltayUIManager : MonoBehaviour
	{
		[SerializeField] private GameObject gridItemPrefab;

		[SerializeField] private RectTransform viewportRectTransform;
		[SerializeField] private RectTransform contentRectTransform;
		[SerializeField] private GridLayoutGroup gridLayoutGroup;
		[SerializeField] private int numCols;
		private float gridItemSideLength;
		
		[SerializeField] private Tooltray activeTooltray;

		private List<ToolButton> gridItems = new List<ToolButton>();

		private void Awake()
		{
			var rect = viewportRectTransform.rect;

			gridItemSideLength = rect.width / numCols;
			gridLayoutGroup.cellSize = new Vector2(gridItemSideLength, gridItemSideLength);
			int numRows = (int)(rect.height / gridItemSideLength) + 2;

			for (int i = 0; i < numRows * numCols; i++)
			{
				ToolButton toolButton = Instantiate(gridItemPrefab, Vector3.zero,
						Quaternion.identity, gridLayoutGroup.transform).GetComponent<ToolButton>();

				gridItems.Add(toolButton);
			}

			//temp
			SetActiveTooltray(activeTooltray);
		}

		public void SetActiveTooltray(Tooltray tooltray)
		{
			activeTooltray = tooltray;
			
			Vector2 contentRectSizeDelta = new Vector2(0, tooltray.Count * gridItemSideLength / numCols);
			contentRectTransform.sizeDelta = contentRectSizeDelta;

			contentRectTransform.localPosition = new Vector3(0, 0, 0);
			UpdateIcons(0);
		}

		private void UpdateIcons(int offset)
		{
			for (int i = 0; i < gridItems.Count; i++)
			{
				int offsetI = offset + i;
				if (offsetI < activeTooltray.Count)
				{
					gridItems[i].gameObject.SetActive(true);
					gridItems[i].SetToolEntry(activeTooltray.GetEntry(offsetI));
				}
				else
					gridItems[i].gameObject.SetActive(false);
			}
		}

		private int gridShifts = 0;
		private int lastGridShifts = 0;
		private void Update()
		{
			lastGridShifts = gridShifts;
			gridShifts = (int) (-contentRectTransform.localPosition.y / gridItemSideLength);
			gridLayoutGroup.transform.localPosition = new Vector3(0, gridShifts * gridItemSideLength);
			
			if(gridShifts != lastGridShifts)
				UpdateIcons(-gridShifts * numCols);
		}
	}
}
