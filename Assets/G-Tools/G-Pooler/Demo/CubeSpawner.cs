using UnityEngine;
using Random = UnityEngine.Random;

namespace G_Pooler.Demo
{
    public class CubeSpawner : MonoBehaviour
    {
        [SerializeField] private float spawnDelay = 1f;
        private float _spawnTimer;

        private void Update()
        {
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer < spawnDelay) return;
            _spawnTimer = 0;
            SpawnCube();
        }

        private void SpawnCube()
        {
            var cubeRb = ObjectPooler.GetPoolObject<Rigidbody>("Cube", 
                Vector3.up * Random.Range(3f, 6f), Random.rotation.eulerAngles);
            cubeRb.AddForce(Random.onUnitSphere * Random.Range(2f, 6f), ForceMode.VelocityChange); 
        }
    }
}
