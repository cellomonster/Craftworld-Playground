using Cam;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Toolbox
{
	public class ToolboxUIManager : MonoBehaviour
	{
		[SerializeField] private Canvas inventoryCanvas;
		[SerializeField] private RectTransform rectTransform;
		[SerializeField] private PlayerInput input;
		[SerializeField] private GameObject canvasObject;
		[SerializeField] private CamModeManager camModeManager;

		enum InventoryPage
		{
			Materials,
			Tools,
		}

		private InventoryPage page;

		private void Awake()
		{
			canvasObject.SetActive(false);
		}

		public void OpenInventoryAction(InputAction.CallbackContext context)
		{
			OpenInventory();
		}

		public void CloseInventoryAction(InputAction.CallbackContext context)
		{
			CloseInventory();
		}

		public void OpenInventory()
		{
			input.SwitchCurrentActionMap("Inventory");
			canvasObject.SetActive(true);
			camModeManager.SetCamMode(CamMode.FocusOnInventory);
		}

		public void CloseInventory()
		{
			input.SwitchCurrentActionMap("Player");
			canvasObject.SetActive(false);
			camModeManager.SetCamMode(CamMode.FollowPlayer);
		}
	}
}
