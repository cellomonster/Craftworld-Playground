using ClipperLib;
using LevelObjects.PhysicalObjects;
using LevelObjects.PhysicalObjects.MaterialObjects;
using UnityEngine;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Vector3 = UnityEngine.Vector3;

namespace LevelObjects.BrushObjects.MaterialBrushObject
{
    public class MaterialBrushObject : BrushObjectBase
    {
        private DynamicMeshManager meshManager;

        private Paths smearedShape;

        private Substance material;
        
        /// <summary>
        /// The material of the material brush 
        /// </summary>
        public Substance Material
        {
            get => material;
            set
            {
                material = value;
                meshManager.Material = value.material;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            GameObject meshObject = new GameObject("mesh object");

            meshObject.transform.parent = transform;
            meshObject.transform.localPosition = Vector3.zero;
            meshObject.transform.localEulerAngles = Vector3.zero;

            meshManager = meshObject.AddComponent<DynamicMeshManager>();
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
            Setup(shape, frontLayer, backLayer);
            Material = material;
        }
        
        /// <summary>
        /// Rebuilds the mesh and trigger collider shape
        /// </summary>
        public override void ApplyShapeModifications()
        {
            meshManager.GenerateMesh(Shape, FrontLayer, BackLayer);
            RebuildColliderFromObjectShape();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupEditing"></param>
        public override void Stamp(PhysicalGroup groupEditing)
        {
            MaterialObject stampedObject = null;
            for (int i = TriggerOverlaps.Count - 1; i > -1; i--)
            {
                if (TriggerOverlaps[i].GetType() == typeof(MaterialObject))
                {
                    MaterialObject m = (MaterialObject) TriggerOverlaps[i];

                    if (FrontLayer <= m.BackLayer &&
                        m.FrontLayer <= BackLayer)
                    {
                        if (m.FrontLayer != FrontLayer || m.BackLayer != BackLayer ||
                            m.Material != Material)
                        {
                            m.Subtract(smearedShape, transform);
                            if (m.Shape.Count == 0)
                            {
                                TriggerOverlaps.RemoveAt(i);
                                groupEditing.ContainedObjects.Remove(m);
                                Destroy(m.gameObject);
                            }
                        }
                        else if (stampedObject == null)
                        {
                            m.Add(smearedShape, transform);
                            stampedObject = m;
                        }
                        else
                        {
                            stampedObject.Add(m.Shape, m.transform);
                            TriggerOverlaps.RemoveAt(i);
                            groupEditing.ContainedObjects.Remove(m);
                            Destroy(m.gameObject);
                        }
                    }
                }
            }

            if (!stampedObject)
            {
                MaterialObject m = LevelObjectCreator.CreateMaterialObject(Shape, transform.position,
                    transform.eulerAngles.z,
                    FrontLayer, BackLayer, material, groupEditing);
            }
        }

        public override void Cut()
        {
            for (int i = TriggerOverlaps.Count - 1; i > -1; i--)
            {
                PhysicalObjectBase physicalObject = TriggerOverlaps[i];
                if (physicalObject.GetType() == typeof(MaterialObject) && FrontLayer <= physicalObject.BackLayer &&
                    physicalObject.FrontLayer <= BackLayer)
                {
                    MaterialObject m = (MaterialObject) physicalObject;
                    m.Subtract(smearedShape, transform);
                    if (m.Shape.Count == 0)
                    {
                        TriggerOverlaps.RemoveAt(i);
                        m.Group.ContainedObjects.Remove(m);
                        Destroy(m.gameObject);
                    }
                }
            }
        }

        public void SmearShape(Path smearPath)
        {
            smearedShape = Clipper.MinkowskiSum(smearPath, Shape, true);

            SetCollisionShape(smearedShape);
        }

        public void UnsmearShape()
        {
            smearedShape = Shape;
            RebuildColliderFromObjectShape();
        }
    }
}