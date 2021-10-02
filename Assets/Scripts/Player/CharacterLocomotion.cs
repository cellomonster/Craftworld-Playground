using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
	public class CharacterLocomotion : MonoBehaviour
	{
		[SerializeField] private new Rigidbody2D rigidbody;
		[SerializeField] private WheelJoint2D wheel;
		[SerializeField] private NumCollisionTracker groundedTriggerTracker;
		[SerializeField] private float speed;
		[SerializeField] private float jumpImpulse;

		private JointMotor2D motor;
		private bool grounded => groundedTriggerTracker.NumCollisions > 0;
		private int framesInAir;
		private float direction = 0;

		private void Awake()
		{
			motor = wheel.motor;
		}

		public void Run(InputAction.CallbackContext context)
		{
			direction = -context.ReadValue<float>();
		}

		public void Jump(InputAction.CallbackContext context)
		{
			if (context.performed && grounded)
			{
				rigidbody.AddForce(Vector2.up * jumpImpulse, ForceMode2D.Impulse);
			}
		}

		private void FixedUpdate()
		{
			motor.motorSpeed = direction * speed;
			wheel.motor = motor;
		}
	}
}
