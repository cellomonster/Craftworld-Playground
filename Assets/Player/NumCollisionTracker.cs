using System;
using UnityEngine;

namespace Player
{
	[RequireComponent(typeof(Collider2D))]
	public class NumCollisionTracker : MonoBehaviour
	{
		public int NumCollisions { get; private set; }

		private void OnCollisionEnter2D()
		{
			NumCollisions++;
		}

		private void OnTriggerEnter2D(Collider2D col)
		{
			NumCollisions++;
		}

		private void OnCollisionExit2D()
		{
			NumCollisions--;
		}

		private void OnTriggerExit2D(Collider2D other)
		{
			NumCollisions--;
		}
	}
}