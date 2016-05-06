﻿using UnityEngine;
using System.Diagnostics;
using System.Collections;
using Immersio.Utility;

public class EnemySpawnManager : MonoBehaviour
{
    [SerializeField]
    float _updateInterval_ms = 500;
    [SerializeField]
    float _enemyRemovalDistance = 32;      // How far away Enemy must be from player before it is destroyed (Overworld only)


    Transform _enemiesContainer;
    float _enemyRemovalDistanceSq;


	void Start () 
    {
        _enemiesContainer = GameObject.Find("Enemies").transform;

        _enemyRemovalDistanceSq = _enemyRemovalDistance * _enemyRemovalDistance;

        StartCoroutine("Update_Coroutine");
    }


    IEnumerator Update_Coroutine()
    {
        // TODO: Use InvokeRepeating instead

        while (true)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            {
                Tick();
            }
            stopWatch.Stop();

            int elapsedTime = stopWatch.Elapsed.Milliseconds;
            float waitTime = 0.001f * Mathf.Max(0, _updateInterval_ms - elapsedTime);
            yield return new WaitForSeconds(waitTime);
        }
    }

    void Tick()
    {
        foreach (Transform child in transform)
        {
            child.GetComponent<EnemySpawnPoint>().DoUpdate();
        }

        Vector3 playerPos = CommonObjects.PlayerController_G.transform.position;

        foreach (Transform child in _enemiesContainer)
        {
            Vector3 toPlayer = playerPos - child.position;
            float distToPlayerSqr = toPlayer.sqrMagnitude;
            if (distToPlayerSqr > _enemyRemovalDistanceSq)
            {
                Destroy(child.gameObject);  
            }
        }   
    }
}