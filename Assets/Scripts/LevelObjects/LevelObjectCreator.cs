using LevelObjects.BrushObjects;
using LevelObjects.BrushObjects.MaterialBrushObject;
using LevelObjects.PhysicalObjects;
using LevelObjects.PhysicalObjects.MaterialObjects;
using UnityEngine;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

namespace LevelObjects
{
    public class LevelObjectCreator : MonoBehaviour
    {
        public static PhysicalGroup CreatePhysicalGroup(Vector2 pos, float angle)
        {
            PhysicalGroup p = new GameObject("brush group").AddComponent<PhysicalGroup>();
            p.Position = pos;
            p.Rotation = angle;
            return p;
        }

        public static BrushGroup CreateBrushGroup(Vector2 pos, float angle)
        {
            BrushGroup b = new GameObject("brush group").AddComponent<BrushGroup>();
            b.Position = pos;
            b.Rotation = angle;
            return b;
        }

        public static MaterialObject CreateMaterialObject(Paths shape, Vector2 pos, float angle, int frontLayer,
            int backLayer, Substance material, PhysicalGroup targetGroup)
        {
            MaterialObject m = new GameObject("material object").AddComponent<MaterialObject>();
            m.Position = pos;
            m.Rotation = angle;
            
            targetGroup.AddLevelObject(m);
            
            m.Setup(shape, frontLayer, backLayer, material);
            return m;
        }

        public static MaterialBrushObject CreateMaterialBrushObject(Paths shape, Vector2 pos, float angle, int frontLayer,
            int backLayer, Substance material, BrushGroup targetGroup)
        {
            MaterialBrushObject m = new GameObject("material object").AddComponent<MaterialBrushObject>();
            m.Position = pos;
            m.Rotation = angle;
            m.Setup(shape, frontLayer, backLayer, material);
            targetGroup.AddLevelObject(m);
            return m;
        }
    }
}