using UnityEngine;

namespace Cam
{
	public class FollowLerpTransform : CamModeBehavior
	{
		public Transform targetTransform;
		public float camZDist = 10;
		[SerializeField] private float lerpStrength = 0.5f;

		private void Update()
		{
			transform.position =
				Vector3.Lerp(transform.position, 
					targetTransform.position + Vector3.back * camZDist, 
					Time.deltaTime * 60 * lerpStrength);
		}

	}
}
