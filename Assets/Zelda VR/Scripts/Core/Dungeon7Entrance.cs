﻿using UnityEngine;


public class Dungeon7Entrance : MonoBehaviour
{
    public GameObject dryLakeOverlay, fullLakeOverlay;


    GameObject[] _waterBlocks;


    public bool LakeIsFull { get; private set; }


    void Start()
    {
        _waterBlocks = GameObject.FindGameObjectsWithTag(ZeldaTags.DUNGEON_7_LAKE_BLOCK);

        LakeIsFull = false;
        FillLake();
    }


    public void EmptyLake()
    {
        if (!LakeIsFull) { return; }

        dryLakeOverlay.SetActive(true);
        fullLakeOverlay.SetActive(false);

        for (int i = _waterBlocks.Length - 1; i >= 0; i--)
        {
            _waterBlocks[i].SetActive(false);
        }

        PlaySecretSound();

        LakeIsFull = false;
    }

    void PlaySecretSound()
    {
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.secret);
    }

    public void FillLake()
    {
        if (LakeIsFull) { return; }

        dryLakeOverlay.SetActive(false);
        fullLakeOverlay.SetActive(true);

        for (int i = _waterBlocks.Length - 1; i >= 0; i--)
        {
            _waterBlocks[i].SetActive(true);
        }

        LakeIsFull = true;
    }
}