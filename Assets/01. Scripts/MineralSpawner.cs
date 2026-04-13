using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MineralSpawner : MonoBehaviour
{
    public Mineral[] minerals;

    private void Awake()
    {
        foreach (var mineral in minerals)
        {
            if (mineral != null)
            {
                mineral.Init(this);
            }
            else
            {
                Debug.LogError("[MineralSpawner] minerals 배열에 null이 있어요!");
            }
        }
    }

    public void StartRespawn(Mineral mineral, float delay)
    {
        StartCoroutine(RespawnRoutine(mineral, delay));
    }

    IEnumerator RespawnRoutine(Mineral mineral, float delay)
    {
        yield return new WaitForSeconds(delay);
        mineral.Respawn();
    }
}
