using GTools.GPooler;
using UnityEngine;

namespace GPooler.Samples.CubeSpawner
{
    public class DisableCollisions : MonoBehaviour
    {
        private void OnCollisionEnter(Collision collision)
        {
            //ADD POOL FUNCTION
            ObjectPooler.AddPool(collision.gameObject);
        }
    }
}