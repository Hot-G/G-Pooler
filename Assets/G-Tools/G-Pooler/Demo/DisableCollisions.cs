using G_Pooler;
using UnityEngine;

public class DisableCollisions : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        //ADD POOL FUNCTION
        ObjectPooler.AddPool(collision.gameObject);
    }
}
