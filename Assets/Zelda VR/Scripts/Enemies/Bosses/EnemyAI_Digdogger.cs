﻿using UnityEngine;


public class EnemyAI_Digdogger : EnemyAI
{
    const float OFFSCREEN_OFFSET = -30;


    public int numBabies = 1;
    public GameObject babyPrefab;


    public bool HasSplit { get; private set; }


    public void SplitIntoBabies()
    {
        if (HasSplit) { return; }

        for (int i = 0; i < numBabies; i++)
        {
            Vector3 pos = transform.position;
            if (i == 1) { pos.x++; }
            else if (i == 2) { pos.z++; }

            SpawnBaby(pos);
        }

        transform.AddToY(OFFSCREEN_OFFSET);
        GetComponent<EnemyAI_Random>().enabled = false;

        HasSplit = true;
    }


    void SpawnBaby(Vector3 position)
    {
        GameObject g = Instantiate(babyPrefab) as GameObject;
        g.name = babyPrefab.name;
        g.transform.parent = transform.parent;
        g.transform.position = position;
        g.transform.SetY(WorldOffsetY + 0.5f);

        EnemyAI_DigdoggerSmall dd = g.GetComponent<EnemyAI_DigdoggerSmall>();
        dd.ParentDigdogger = this;
    }

    void OnBabyDied()
    {
        if (--numBabies == 0)
        {
            _healthController.isIndestructible = false;
            _healthController.Kill(null, true);
        }
    }
}