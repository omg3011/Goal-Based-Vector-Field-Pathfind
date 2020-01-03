using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SteeringBehaviour
{
  NONE,
  SEEK,
  FLEE,
  ARRIVAL,
  WANDER,
  PURSUIT,
  EVADE,

}
public class Prey : IBoid
{
  [Header("Twerkable")]
  public float maxSpeed = 1.0f;
  public float maxForce = 1.0f;
  public SteeringBehaviour steerType;

  public Transform target;

  private SteeringManager steer;
  private Vector3 position;
  private Vector3 velocity;

  void Start()
  {
    steer = GetComponent<SteeringManager>();
  }
  void Update()
  {
    steer.ResetVariables();

    //steer.Seek(steer.target.getPosition());
    UpdateSteer(steerType);

    steer.Update();

    ApplyMovementRotation();
  }

  void UpdateSteer(SteeringBehaviour behaviour)
  {
    if (steerType == SteeringBehaviour.NONE)
      Debug.Log("Do Nothing");
    else if (steerType == SteeringBehaviour.SEEK)
      steer.Seek(target.position);
    else if (steerType == SteeringBehaviour.FLEE)
      steer.Flee(target.position);
    else if (steerType == SteeringBehaviour.ARRIVAL)
      steer.Arrival(target.position);
    else if (steerType == SteeringBehaviour.WANDER)
      steer.Wander();
    else if (steerType == SteeringBehaviour.PURSUIT)
      steer.Pursuit(steer.target);
    else if (steerType == SteeringBehaviour.EVADE)
      steer.Evade(steer.target);
  }

  void ApplyMovementRotation()
  {
    transform.position += velocity * Time.deltaTime;
    position = transform.position;

    UpdateRotate_By_MoveDir();
  }

  //----------------------------------------------------------
  // Getter(s)
  //----------------------------------------------------------
  public override float getMaxForce()
  {
    return maxForce;
  }

  public override float getMaxSpeed()
  {
    return maxSpeed;
  }

  public override Vector3 getPosition()
  {
    return position;
  }

  public override Vector3 getVelocity()
  {
    return velocity;
  }


  //----------------------------------------------------------
  // Setter(s)
  //----------------------------------------------------------
  public override void setPosition(Vector3 pos)
  {
    position = pos;
  }

  public override void setVelocity(Vector3 vel)
  {
    velocity = vel;
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

}
