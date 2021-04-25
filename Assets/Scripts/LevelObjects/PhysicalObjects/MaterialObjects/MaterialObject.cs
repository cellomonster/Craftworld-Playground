using System;
using System.Threading;
using ClipperLib;
using UnityEngine;
using UnityEngine.Assertions;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

namespace LevelObjects.PhysicalObjects.MaterialObjects
{
    /// <summary>
    /// Represents a 'material' level object.
    /// Manages shape of the object which can be changed by
    /// 'brush' objects. 
    /// See <see cref="DynamicMeshManager"/> for the mesh generation
    /// code.
    /// </summary>
    public class MaterialObject : PhysicalObjectBase
    {
        //public MaterialData MaterialData
        //{
        //	get { return meshManager.MaterialData; }
        //	set { meshManager.MaterialData = value; }
        //}

        private DynamicMeshManager _meshManager;

        private Substance substance;
        
        public Substance Material
        {
            get => substance;
            set
            {
                substance = value;
                _meshManager.Material = value.material;
                PolyCollider.sharedMaterial = value.physicsMaterial;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            GameObject meshObject = new GameObject("mesh object");
            meshObject.transform.parent = transform;
            _meshManager = meshObject.AddComponent<DynamicMeshManager>();
        }

        /// <summary>
        /// Sets up the shape, occupied layers, and material
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="frontLayer"></param>
        /// <param name="backLayer"></param>
        /// <param name="material"></param>
        public void Setup(Paths shape, int frontLayer, int backLayer, Substance material)
        {
            base.Setup(shape, frontLayer, backLayer);
            Material = material;

            if (!Group)
            {
                Debug.LogError("MaterialObject was not a member of a group, so density was not applied to collider");
            }
        }

        public void Add(Paths paths, Transform brushTransform)
        {
            Modify(ClipType.ctUnion, paths, brushTransform);
        }

        public void Subtract(Paths paths, Transform brushTransform)
        {
            Modify(ClipType.ctDifference, paths, brushTransform);
        }

        /// <summary>
        /// Changes the shape of the object by adding or subtracting the shape of the brush
        /// Parameters:
        ///		clipType is the boolean operation to be done.
        ///		brushMesh is the mesh of the shape to be added/subtracted
        ///		offset is the position of the shape *in world space*
        /// </summary>
        private void Modify(ClipType clipType, Paths brushShapeGlobalPosition, Transform brushTransform)
        {
            Paths paths = new Paths();

            for (int i = 0; i < brushShapeGlobalPosition.Count; i++)
            {
                paths.Add(new Path());
                for (int j = 0; j < brushShapeGlobalPosition[i].Count; j++)
                {
                    Vector2 v = brushShapeGlobalPosition[i][j];
                    v = brushTransform.TransformPoint(v);
                    v = transform.InverseTransformPoint(v);
                    paths[i].Add(v);
                }
            }

            Clipper c = new Clipper();
            c.AddPaths(Shape, PolyType.ptSubject, true);
            c.AddPaths(paths, PolyType.ptClip, true);
            c.Execute(clipType, Shape,
                PolyFillType.pftEvenOdd, PolyFillType.pftNonZero);
        }

        public override void ApplyShapeModifications()
        {
            if (Shape.Count == 0)
            {
                Destroy(this.gameObject);
            }

            // convert Shape path list to a tree structure
            // this makes things a tad easier to handle
            Clipper c = new Clipper();
            c.AddPaths(Shape, PolyType.ptSubject, true);
            PolyTree shapeTree = new PolyTree();
            c.Execute(ClipType.ctUnion, shapeTree, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

            // find all objects within holes within the main object
            foreach (PolyNode holeNode in shapeTree.Childs[0].Childs)
            {
                foreach (PolyNode separateObjectNode in holeNode.Childs)
                {
                    CreateSeparatedObject(separateObjectNode);
                }

                holeNode.Childs.Clear();
            }

            // any extra shape islands should be turned into separate shapes!
            for (int i = 1; i < shapeTree.Childs.Count; i++)
            {
                CreateSeparatedObject(shapeTree.Childs[i]);
            }

            // remove all those extra shape islands from this object's shape
            // those islands are now all separated shapes
            shapeTree.Childs.RemoveRange(1, shapeTree.Childs.Count - 1);

            // convert paths back from tree to list
            Shape = Clipper.PolyTreeToPaths(shapeTree);

            // rebuild everything now
            base.ApplyShapeModifications();
            _meshManager.GenerateMesh(Shape, FrontLayer, BackLayer);
            
            // group rigidbody NEEDS to be dynamic for this to work (argh!)
            PolyCollider.density = substance.density * Thickness;
        }

        /// <summary>
        /// Creates a separate material object who's shape is derived from a polynode
        /// </summary>
        /// <param name="separateOutsideNode"></param>
        private void CreateSeparatedObject(PolyNode separateOutsideNode)
        {
            Paths separatedShape = new Paths();
            Clipper.AddPolyNodeToPaths(separateOutsideNode, Clipper.NodeType.ntClosed, separatedShape);

            MaterialObject separatedMaterialObject = LevelObjectCreator.CreateMaterialObject(separatedShape, Position, Rotation, FrontLayer,
                BackLayer, Material, Group);
            
            Group.AddLevelObject(separatedMaterialObject);
        }
    }
}