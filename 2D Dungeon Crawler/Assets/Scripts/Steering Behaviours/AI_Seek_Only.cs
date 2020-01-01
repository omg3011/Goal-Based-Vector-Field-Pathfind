using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Seek_Only : MonoBehaviour
{
  // twerkable
  public float maxSpeed = 10.0f;
  public float maxForce = 10.0f;

  // Private
  Vector3 moveDir = Vector3.right;
  Vector3 targetPos;
  Vector3 velocity;
  Vector3 acceleration;

  private void Start()
  {
    // Get initial velocity
    velocity = moveDir * maxSpeed;
  }

  private void Update()
  {
    //---------------------------------------
    // Control(s)
    //---------------------------------------
    CheckForPause();

    // Get Mouse Position
    GetMousePosition();


    //---------------------------------------
    // Reset(s)
    //---------------------------------------
    acceleration = Vector3.zero;


    //---------------------------------------
    // Add Steering Behaviours Acceleration
    //---------------------------------------
    acceleration += Seek(ref velocity);


    //---------------------------------------
    // Apply to Transform
    //---------------------------------------
    // Compute Final Velocity
    velocity += acceleration;
    velocity.z = 0;
    velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

    // Apply Movement
    transform.position += velocity * Time.deltaTime;

    // Apply Rotation
    UpdateRotate_By_MoveDir();
  }

  void CheckForPause()
  {
    if(Input.GetKeyDown(KeyCode.Space))
    {
      if (Time.timeScale == 0)
        Time.timeScale = 1;
      else
        Time.timeScale = 0;
    }
  }

  /////////////////////////////////////////////////////////////////////////////////////////////
  //
  //  Steering Behaviours Functions
  //
  /////////////////////////////////////////////////////////////////////////////////////////////
  Vector3 Seek(ref Vector3 vel)
  {
    Vector3 desiredVel = (targetPos - transform.position).normalized * maxSpeed;
    Vector3 steering = desiredVel - vel;
    steering = Vector3.ClampMagnitude(steering, maxForce);

    return steering;
  }

  /////////////////////////////////////////////////////////////////////////////////////////////
  //
  //  Rotate Functions
  //
  /////////////////////////////////////////////////////////////////////////////////////////////
  public static float Angle(Vector2 p_vector2)
  {
    if (p_vector2.x < 0)
    {
      return 360 - (Mathf.Atan2(p_vector2.y, p_vector2.x) * Mathf.Rad2Deg * -1);
    }
    else
    {
      return Mathf.Atan2(p_vector2.y, p_vector2.x) * Mathf.Rad2Deg;
    }
  }
  void UpdateRotate_By_MoveDir()
  {
    float angle = Angle(velocity.normalized);
    transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, angle);
  }


  /////////////////////////////////////////////////////////////////////////////////////////////
  //
  //  Get Mouse World Position Functions
  //
  /////////////////////////////////////////////////////////////////////////////////////////////
  void GetMousePosition()
  {
    // Get Mouse Position
    targetPos = cursorWorldPosOnNCP();
  }
  Vector3 cursorWorldPosOnNCP()
  {
    return Camera.main.ScreenToWorldPoint(
        new Vector3(Input.mousePosition.x,
        Input.mousePosition.y,
        Camera.main.nearClipPlane));
  }
}
