using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    public GameObject Spawner;
    public GameObject Enemy;
    
    public void SpawnEnemyTank()
    {
        Instantiate(Enemy, Spawner.transform.position, Spawner.transform.rotation);
    }
}
