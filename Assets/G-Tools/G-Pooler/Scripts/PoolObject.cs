using UnityEngine;

namespace G_Pooler
{
    public class PoolObject : MonoBehaviour
    {
        public string poolTag;
        public Component objectToPool;
        [Min(1)]
        public int amountToPool;
        public ExpandType expandType;
    }
}
