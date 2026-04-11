using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MineralSpawner : MonoBehaviour
{
    public Mineral[] minerals;
    //[Header("광물 설정")]
    //public GameObject mineralPrefab;
    //public Transform[] spawnPoints; // Inspector에서 위치 지정

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
        //foreach (var point in spawnPoints)
        //{
        //    GameObject obj = Instantiate(mineralPrefab, point.position, point.rotation);

        //    Mineral mineral = obj.GetComponent<Mineral>();

        //    if (mineral != null)
        //    {
        //        mineral.Init(this);
        //    }
        //    else
        //    {
        //        Debug.LogError($"[MineralSpawner] {mineralPrefab.name}에 Mineral.cs가 없어요!");
        //    }
        //}
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
