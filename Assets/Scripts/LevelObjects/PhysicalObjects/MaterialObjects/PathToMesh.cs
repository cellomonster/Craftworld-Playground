using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using ClipperLib;
using UnityEngine;

namespace LevelObjects.PhysicalObjects.MaterialObjects
{
	public struct MeshData
	{
		public List<Vector3> Vertices;
		public List<int> Triangles;
	}

	public class PathToMesh : MonoBehaviour
	{
		private new static PolygonCollider2D collider;

		private static Transform _tr;

		private void Awake()
		{
			collider = gameObject.AddComponent<PolygonCollider2D>();
			transform.position = new Vector2(999, 999);
			_tr = transform;
		}

		public static MeshData PathsToMesh(List<List<IntPoint>> paths, float facePositionOnZ)
		{
			_tr.position = new Vector3(999, 999, -facePositionOnZ);

			collider.pathCount = paths.Count;

			for (int i = 0; i < paths.Count; i++)
			{
				List<IntPoint> p = paths[i];

				Vector2[] np = new Vector2[p.Count];
				for (int j = 0; j < p.Count; j++)
					np[j] = p[j];

				collider.SetPath(i, np);
			}

			Mesh tempMesh = collider.CreateMesh(false, false);

			MeshData meshData = new MeshData();

			if (!tempMesh)
			{
				meshData.Triangles = new List<int>();
				meshData.Vertices = new List<Vector3>();
				return meshData;
			}

			meshData.Vertices = new List<Vector3>(tempMesh.vertices);
			meshData.Triangles = new List<int>(tempMesh.triangles);

			for (int i = 0; i < meshData.Vertices.Count; i++)
			{
				meshData.Vertices[i] = _tr.InverseTransformPoint(meshData.Vertices[i]);
			}

			Destroy(tempMesh);
			return meshData;
		}
	}
}