using UnityEngine;

namespace Cam
{
	public class FollowLeadRigidbody : CamModeBehavior
	{
		[SerializeField] private new Rigidbody2D rigidbody;
		public float camZDist = 10;
		[SerializeField] private float lerpStrength = 0.5f;
		[SerializeField] private float leadStrengthX = 0.5f;
		[SerializeField] private float leadStrengthY = 0.05f;

		private Vector3 followTargetPoint;
	
		// Update is called once per frame
		void Update()
		{
			Vector2 velocity;
			followTargetPoint =
				(Vector3) (rigidbody.position) + -Vector3.forward * camZDist
				                               + Vector3.right * (velocity = rigidbody.velocity).x * leadStrengthX
				                               + Vector3.up * velocity.y * leadStrengthY;
		
			transform.position = Vector3.Lerp(transform.position, followTargetPoint, Time.deltaTime * 60 * lerpStrength);
		}
	}
}
