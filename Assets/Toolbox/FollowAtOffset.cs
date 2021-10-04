using UnityEngine;

namespace Toolbox
{
	public class FollowAtOffset : MonoBehaviour
	{
		[SerializeField] private Transform target;
		[SerializeField] private Vector3 offset;

		private void Update()
		{
			transform.position = target.position + offset;
		}
	}
}
