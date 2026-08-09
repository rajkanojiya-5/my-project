using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SpawnCar : MonoBehaviour
{
    
    [SerializeField]
    private Transform car;
    //[SerializeField]
    //private CameraFollow cameraFollow;
    [SerializeField]
    private RoadMeshGenerator roadMeshGenerator;
    [SerializeField]
    private float timeDelay = 0.5f;
    IEnumerator Start()
    {
        
        yield return new WaitForSeconds(0.5f);

        Spawn();
    }

    // Update is called once per frame
  
   public void Spawn()
    {

        Vector3 spawnPos = roadMeshGenerator.SpawnPoint;

        Vector3 forward = roadMeshGenerator.SpawnForward;
        //GameObject car = Instantiate(carPrefab);
        car.transform.position =
    spawnPos + Vector3.up * 3f;

        car.transform.rotation =
            Quaternion.LookRotation(forward);
        //cameraFollow.carTarget = car.transform;
    }
}
