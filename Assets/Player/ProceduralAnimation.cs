using UnityEngine;

namespace Player
{
	public class ProceduralAnimation : MonoBehaviour
	{
		[SerializeField] private Transform bodyTransform;
		[SerializeField] private Transform eyesTransform;

		[SerializeField] private float maxTargetVelocity = 5;
		[SerializeField] private float bodyMaxHorizontalAngle;
		[SerializeField] private float eyeMaxDistance;

		[SerializeField] private new Rigidbody2D rigidbody;

		private Vector2 maxVelocityRatio = Vector2.zero;
	
		private void Update()
		{
			maxVelocityRatio = rigidbody.velocity / maxTargetVelocity;
			Vector2.ClampMagnitude(maxVelocityRatio, 1f);

			bodyTransform.localEulerAngles = new Vector3(0, maxVelocityRatio.x * bodyMaxHorizontalAngle, 0);
			eyesTransform.localPosition = maxVelocityRatio * eyeMaxDistance;
		}
	}
}
