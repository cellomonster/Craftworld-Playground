using UnityEngine;

namespace Cam
{
	public class FollowLeadRigidbody : MonoBehaviour
	{
		[SerializeField] private new Rigidbody2D rigidbody;
		[SerializeField] private float camZDist = 7;
		[SerializeField] private float lerpStrength = 0.5f;
		[SerializeField] private float leadStrengthX = 0.5f;
		[SerializeField] private float leadStrengthY = 0.05f;

		private Vector3 followTargetPoint;
	
		// Update is called once per frame
		void Update()
		{
			followTargetPoint =
				(Vector3) (rigidbody.position) + -Vector3.forward * camZDist
				                               + Vector3.right * rigidbody.velocity.x * leadStrengthX
				                               + Vector3.up * rigidbody.velocity.y * leadStrengthY;
		
			transform.position = Vector3.Lerp(transform.position, followTargetPoint, Time.deltaTime * 60 * lerpStrength);
		}
	}
}
