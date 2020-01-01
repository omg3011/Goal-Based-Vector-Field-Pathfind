using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
  public float moveSpeed = 200.0f;
  public float attackRange = 5.0f;

  Vector3 moveDir;

  MapManager3 map;

  Transform playerT;

  void Start()
  {
    map = FindObjectOfType<MapManager3>();
    playerT = GameObject.FindGameObjectWithTag("Player").transform;
  }

  void Update()
  {
    // Get Move Dir
    if (map.bFoundPath)
      moveDir = map.ConvertWorldToDirection(transform.position);
    else 
      moveDir = Vector3.zero;

    // Check if target within range
    if (Vector3.Distance(playerT.position, transform.position) < attackRange)
    {
      moveDir = Vector3.zero;// (playerT.position - transform.position).normalized;
      Debug.Log("Reached");
    }

    transform.position += moveDir * moveSpeed * Time.deltaTime;
  }
}
