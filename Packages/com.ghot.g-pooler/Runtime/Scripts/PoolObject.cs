using UnityEngine;

namespace GTools.GPooler
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
