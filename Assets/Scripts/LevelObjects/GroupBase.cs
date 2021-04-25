using UnityEngine;
using System.Collections.Generic;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;

namespace LevelObjects
{
    [SelectionBase]
    public abstract class GroupBase<TLevelObjectType> : MonoBehaviour where TLevelObjectType : LevelObjectBase
    {
        public List<TLevelObjectType> ContainedObjects { get; protected set; }

        protected Rigidbody2D Rigidbody { get; set; }

        private int backLayer;
        private int frontLayer;
        
        public int FrontLayer
        {
            get => frontLayer;
            set
            {
                frontLayer = Mathf.Clamp(value, 1, Global.Layers);
                backLayer = Mathf.Clamp(backLayer, frontLayer, Global.Layers);
            }
        }

        public int BackLayer
        {
            get => backLayer;
            set
            {
                backLayer = Mathf.Clamp(value, 1, Global.Layers);
                frontLayer = Mathf.Clamp(frontLayer, 1, backLayer);
            }
        }
        
        public int Thickness
        {
            get => backLayer - frontLayer + 1;
            set => BackLayer = frontLayer + value - 1;
        }
        
        public Vector2 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        public float Rotation
        {
            get => transform.eulerAngles.z;
            set => transform.eulerAngles = new Vector3(0, 0, value);
        }

        protected virtual void Awake()
        {
            ContainedObjects = new List<TLevelObjectType>();

            Rigidbody = gameObject.AddComponent<Rigidbody2D>();
            Rigidbody.isKinematic = true;
        }

        public virtual void Rebuild()
        {
            frontLayer = 5;
            backLayer = 1;
            foreach (TLevelObjectType levelObject in ContainedObjects)
            {
                levelObject.ApplyShapeModifications();
                if (levelObject.FrontLayer < frontLayer)
                {
                    frontLayer = levelObject.FrontLayer;
                }
                if (levelObject.BackLayer > backLayer)
                {
                    backLayer = levelObject.BackLayer;
                }
            }
        }

        public virtual void AddLevelObject(TLevelObjectType levelObject)
        {
            if (levelObject.transform.parent == transform) return;
            
            if (levelObject.transform.parent != null)
            {
                GroupBase<TLevelObjectType> g = levelObject.GetComponentInParent<GroupBase<TLevelObjectType>>();
                g.RemoveLevelObject(levelObject);
            }

            ContainedObjects.Add(levelObject);
            levelObject.transform.parent = transform;
        }

        protected virtual void RemoveLevelObject(TLevelObjectType levelObject)
        {
            if (levelObject.transform.parent != transform) return;
            ContainedObjects.Remove(levelObject);
            levelObject.transform.parent = null;
        }

        public void LayerShift(int change)
        {
            change = Mathf.Clamp(change, -frontLayer, Global.Layers - backLayer);
            int t = Thickness;
            FrontLayer += change;
            Thickness = t;
            foreach (TLevelObjectType l in ContainedObjects)
            {
                l.LayerShift(change);
            }
        }

        public void Scale(float factor)
        {
            foreach (TLevelObjectType levelObject in ContainedObjects)
            {
                levelObject.Scale(factor);
                levelObject.transform.localPosition *= factor;
            }
        }
    }
}