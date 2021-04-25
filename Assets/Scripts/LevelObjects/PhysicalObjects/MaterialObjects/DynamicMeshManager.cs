using System.Collections.Generic;
using ClipperLib;
using UnityEngine;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

namespace LevelObjects.PhysicalObjects.MaterialObjects
{
    //[RequireComponent(typeof(StickersRenderer))]


    /// <summary>
    /// Generates a mesh from a <see cref="ClipperLib"/> shape
    /// Responsible for triangulating the front face using <see cref="Poly2Mesh"/> and generating the bevel
    /// </summary>
    public class DynamicMeshManager : MonoBehaviour
    {
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;

        public Material Material
        {
            get => meshRenderer.sharedMaterial;
            set => meshRenderer.sharedMaterial = value;
        }

        //private MaterialData _materialData;
        //public MaterialData MaterialData
        //{
        //	get { return _materialData; }
        //	set
        //	{
        //		_materialData = value;
        //		GetComponent<StickersRenderer>().SetMaterial(value.Material);
        //	}
        //}

        private void Awake()
        {
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;

            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshCollider = gameObject.AddComponent<MeshCollider>();

            meshFilter.sharedMesh = new Mesh();
            meshCollider.sharedMesh = meshFilter.sharedMesh;
        }

        // private Vector2 CalculateCentroidOfFlatMesh(Mesh mesh)
        // {
        // 	Vector2 result = Vector2.zero;
        // 	for (int i = 0; i < mesh.triangles.Length; i += 3)
        // 	{
        // 		Vector2 v1 = mesh.vertices[mesh.triangles[i]];
        // 		Vector2 v2 = mesh.vertices[mesh.triangles[i + 1]];
        // 		Vector2 v3 = mesh.vertices[mesh.triangles[i + 2]];
        //
        // 		result += (v1 + v2 + v3) / 3;
        // 	}
        //
        // 	result /= mesh.triangles.Length / 3;
        // 	return result;
        // }

        public void GenerateMesh(Paths shape, int frontLayer, int backLayer)
        {
            float frontFacePosition = frontLayer - 1.5f;
            float backFacePosition = backLayer - 0.5f;

            // front face is inset by the x value of bevelPoints[0]
            // x value of 0 is the original vertex position
            // bool roundCorners = false;
            Vector3[]
                bevelPoints =
                {
                    new Vector3(0.1f, frontFacePosition, 0), new Vector3(0, frontFacePosition + 0.1f, 0),
                    new Vector3(0, backFacePosition, 1)
                };

            // poly tree is used because there may be multiple 'outside' shapes
            // poly2tri only supports 1 outside shape at a time, so each outside shape
            // must be triangulated independently 
            ClipperOffset offset = new ClipperOffset();
            PolyTree offsetTree = new PolyTree();

            offset.AddPaths(shape, JoinType.jtMiter, EndType.etClosedPolygon);

            // todo: figure out what the units of clipper.offset are compared to normal clipper units!
            offset.Execute(ref offsetTree, -707);

            Paths offsetPaths = Clipper.PolyTreeToPaths(offsetTree);

            List<Vector3> verts = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            int kSum = 0;

            //bevels
            for (int l = 0; l < offsetPaths.Count; l++)
            {
                Path path = offsetPaths[l];

                // number of vertices to 'skip over' till the next path point
                int c = (bevelPoints.Length - 1) * 4;
                //number of total vertices minus number of path points
                int k = path.Count * (bevelPoints.Length - 1) * 4;

                //front face offset inward
                float o = bevelPoints[0].x;

                for (int i = 0; i < path.Count; i++)
                {
                    //front face vertex position
                    Vector3 v = path[i];

                    //next index
                    int j = (i + 1) % path.Count;

                    //normal A
                    Vector2 na = Vector2.Perpendicular(new Vector2(path[j].X - path[i].X, path[j].Y - path[i].Y))
                        .normalized;

                    //previous index
                    j = (int) Mathf.Repeat(i - 1, path.Count);

                    //normal B
                    Vector2 nb = Vector2.Perpendicular(new Vector2(path[i].X - path[j].X, path[i].Y - path[j].Y))
                        .normalized;

                    //bevel direction and magnitude
                    Vector3 b = (na + nb).normalized;
                    b *= 1 / Mathf.Sqrt(1 + Vector2.Dot(na, nb));

                    //smooth normal bevels (used by soft material)
                    /*if (MaterialData.Bevel == 1)
                    {
                        // number of vertices to 'skip over' till the next path point
                        int c = (bevelPoints.Length);
                        //number of total vertices minus number of path points
                        int k = path.Count * (bevelPoints.Length);

                        for (int n = 0; n < bevelPoints.Length - 1; n++)
                        {
                            //z offset
                            Vector3 z = new Vector3(0, 0, bevelPoints[n].y);

                            //add triangles
                            tris.Add((verts.Count + c) % k);
                            tris.Add((verts.Count + 1) % k);
                            tris.Add((verts.Count + 0) % k);

                            tris.Add((verts.Count + 1) % k);
                            tris.Add((verts.Count + c) % k);
                            tris.Add((verts.Count + c + 1) % k);

                            verts.Add(v + b * (bevelPoints[n].x - o) + z);
                            uvs.Add(verts[verts.Count - 1]);
                        }

                        //add last vertex
                        verts.Add(v + b * (bevelPoints[bevelPoints.Length - 1].x - o) + new Vector3(0, 0, bevelPoints[bevelPoints.Length - 1].y));
                        uvs.Add(v - b * (bevelPoints[bevelPoints.Length - 1].y - bevelPoints[bevelPoints.Length - 2].y));
                    }
                    else
                    //hard normal bevels (used by hard material)
                    {*/

                    //hard normals
                    for (int n = 0; n < bevelPoints.Length - 1; n++)
                    {
                        //z offset
                        Vector3 z = new Vector3(0, 0, bevelPoints[n].y);
                        //z offset for next bevel point
                        Vector3 z2 = new Vector3(0, 0, bevelPoints[n + 1].y);

                        //add triangles
                        tris.Add(kSum + (verts.Count + c) % k);
                        tris.Add(kSum + (verts.Count + 3) % k);
                        tris.Add(kSum + (verts.Count + 2) % k);

                        tris.Add(kSum + (verts.Count + 3) % k);
                        tris.Add(kSum + (verts.Count + c) % k);
                        tris.Add(kSum + (verts.Count + c + 1) % k);

                        //add vertices
                        verts.Add(v + b * (bevelPoints[n].x - o) + z);
                        verts.Add(v + b * (bevelPoints[n + 1].x - o) + z2);

                        verts.Add(v + b * (bevelPoints[n].x - o) + z);
                        verts.Add(v + b * (bevelPoints[n + 1].x - o) + z2);

                        //add uvs
                        //if bevelPoint.z = 1, then uvs face sideways
                        //if not, uvs face frontward 
                        if (bevelPoints[n + 1].z == 1)
                        {
                            uvs.Add(new Vector2(verts[verts.Count - 2].x, verts[verts.Count - 2].y));
                            Vector3 nnb = v + (Vector3) nb * (bevelPoints[n + 1].y - bevelPoints[n].y);
                            uvs.Add(new Vector2(nnb.x, nnb.y));
                            uvs.Add(new Vector2(verts[verts.Count - 2].x, verts[verts.Count - 2].y));
                            Vector3 nna = v + (Vector3) na * (bevelPoints[n + 1].y - bevelPoints[n].y);
                            uvs.Add(new Vector2(nna.x, nna.y));
                        }
                        else
                        {
                            uvs.Add(new Vector2(verts[verts.Count - 2].x, verts[verts.Count - 2].y));
                            uvs.Add(new Vector2(verts[verts.Count - 1].x, verts[verts.Count - 1].y));
                            uvs.Add(new Vector2(verts[verts.Count - 2].x, verts[verts.Count - 2].y));
                            uvs.Add(new Vector2(verts[verts.Count - 1].x, verts[verts.Count - 1].y));
                        }

                        // }
                    }
                }

                kSum += k;
            }

            // // front faces
            // // each first child is an outside path
            // foreach (PolyNode outsideNode in offsetTree.Childs)
            // {
            //     Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();
            //     foreach (IntPoint point in outsideNode.Contour)
            //     {
            //         Vector3 v = point;
            //         v += new Vector3(0, 0, frontFacePosition);
            //         poly.outside.Add(v);
            //     }
            //
            //     // add holes to poly
            //     foreach (PolyNode insideNode in outsideNode.Childs)
            //     {
            //         poly.holes.Add(new List<Vector3>());
            //         foreach (IntPoint point in insideNode.Contour)
            //         {
            //             Vector3 v = point;
            //             v += new Vector3(0, 0, frontFacePosition);
            //             poly.holes[poly.holes.Count - 1].Add(v);
            //         }
            //     }
            //
            //     int initialCount = verts.Count;
            //
            //     poly.planeNormal = -Vector3.forward;
            //     MeshData result = Poly2Mesh.CreateMesh(poly);
            //
            //     verts.AddRange(result.vertices);
            //
            //     foreach (Vector2 v in result.vertices)
            //     {
            //         uvs.Add(new Vector2(v.x, v.y));
            //     }
            //
            //     foreach (int index in result.triangles)
            //         tris.Add(index + initialCount);
            //     
            //     _meshFilter.sharedMesh.Clear();
            //     _meshFilter.sharedMesh.SetVertices(verts);
            //     _meshFilter.sharedMesh.SetTriangles(tris, 0);
            //     _meshFilter.sharedMesh.SetUVs(0, uvs);
            // }

            MeshData result = PathToMesh.PathsToMesh(Clipper.ClosedPathsFromPolyTree(offsetTree), frontFacePosition);

            int initialCount = verts.Count;
            
            verts.AddRange(result.Vertices);
            
            foreach (Vector2 v in result.Vertices)
            {
                uvs.Add(new Vector2(v.x, v.y));
            }
            
            foreach (int index in result.Triangles)
                tris.Add(index + initialCount);
            
            meshFilter.sharedMesh.Clear();
            meshFilter.sharedMesh.SetVertices(verts);
            meshFilter.sharedMesh.SetTriangles(tris, 0);
            meshFilter.sharedMesh.SetUVs(0, uvs);
            meshFilter.sharedMesh.RecalculateTangents();
            meshFilter.sharedMesh.RecalculateNormals();
            meshCollider.sharedMesh = meshFilter.sharedMesh;
        }

        public void SetMaterial(Material material)
        {
            meshRenderer.sharedMaterial = material;
        }

        private void OnDestroy()
        {
            Destroy(meshFilter.mesh);
        }
    }
}