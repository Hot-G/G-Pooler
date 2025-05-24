using GTools.GPooler;
using UnityEngine;

namespace GPooler.Samples.CubeSpawner
{
    public class DisableCollisions : MonoBehaviour
    {
        private void OnCollisionEnter(Collision collision)
        {
            ObjectPooler.AddPool(collision.gameObject);
        }
    }
}