using UnityEngine;

namespace LevelEditor
{
    public class SingletonBase: MonoBehaviour
    {
        protected virtual void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
 
        public static SingletonBase Instance{ get; private set; }
    }
}