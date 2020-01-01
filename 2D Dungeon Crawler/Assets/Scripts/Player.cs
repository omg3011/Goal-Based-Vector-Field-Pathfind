using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
  public float moveSpeed = 20.0f;

  Vector3 _inputDir;

  private void Update()
  {
    _inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);

    transform.position += _inputDir * moveSpeed * Time.deltaTime;
  }
}
