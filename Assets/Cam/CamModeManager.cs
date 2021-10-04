using UnityEngine;

namespace Cam
{
	public enum CamMode
	{
		FollowPlayer = 0,
		FocusOnInventory,
	}

	public class CamModeManager : MonoBehaviour
	{
		[SerializeField] private CamModeBehavior followPlayerBehavior;
		[SerializeField] private CamModeBehavior focusOnInventoryBehavior;

		private CamModeBehavior[] camModeBehaviors;

		private void Awake()
		{
			camModeBehaviors = new CamModeBehavior[2];

			camModeBehaviors[0] = followPlayerBehavior;
			camModeBehaviors[1] = focusOnInventoryBehavior;
			
			SetCamMode(CamMode.FollowPlayer);
		}

		public void SetCamMode(CamMode mode)
		{
			foreach (CamModeBehavior behavior in camModeBehaviors)
			{
				behavior.enabled = false;
			}

			camModeBehaviors[(int)mode].enabled = true;
		}
	}
}