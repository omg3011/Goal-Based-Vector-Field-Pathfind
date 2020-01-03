using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringManager : MonoBehaviour
{
  [Header("Arrival Twerkable")]
  public IBoid host;
  public IBoid target;

  [Header("Wander Twerkable")]
  public float circleDistance = 5.0f;
  public float circleRadius = 3.0f;
  public float wanderAngle = 15.0f;
  public float ANGLE_CHANGE = 0.5f;
  public float wanderFrequency = 3.0f;
  private float nextWanderTime;
  private Vector3 wanderTargetPos;

  [Header("Debug")]
  public Vector3 steeringValue;

  private void Start()
  {
  }


  //------------------------------------------
  // Steering Behaviours [Public]
  //------------------------------------------
  public void Seek(Vector3 targetPos)
  {
    steeringValue += toSeek(targetPos);
  }

  public void Arrival(Vector3 targetPos, int slowingRadius = 20)
  {
    steeringValue += toArrival(targetPos, slowingRadius);
  }

  public void Flee(Vector3 targetPos)
  {
    steeringValue += toFlee(targetPos);
  }

  public void Wander()
  {
    steeringValue += toWander();
  }

  public void Evade(IBoid target)
  {
    steeringValue += toEvade(target);
  }

  public void Pursuit(IBoid target)
  {
    steeringValue += toPursuit(target);
  }

  //------------------------------------------
  // Steering Behaviours [Private]
  //------------------------------------------
  Vector3 toSeek(Vector3 targetPos)
  {
    Vector3 desiredVel = (targetPos - host.getPosition()).normalized * host.getMaxSpeed();
    Vector3 steering = desiredVel - host.getVelocity();
    
    //steering = Vector3.ClampMagnitude(steering, maxForce);

    return steering;
  }

  Vector3 toArrival(Vector3 targetPos, int slowingRadius = 20)
  {
    Vector3 desiredVel = (targetPos - host.getPosition());
    float distance = desiredVel.magnitude;

    //-- Check dist to determine inside/outside slow radius
    // Inside radius
    if (distance < slowingRadius)
    {
      desiredVel = desiredVel.normalized * host.getMaxSpeed() * (distance / slowingRadius);
    }
    // Outside Radius
    else
    {
      desiredVel = desiredVel.normalized * host.getMaxSpeed();
    }

    Vector3 steering = desiredVel - host.getVelocity();
    //steering = Vector3.ClampMagnitude(steering, maxForce);

    return steering;
  }

  Vector3 toFlee(Vector3 targetPos)
  {
    return -toSeek(targetPos);
  }

  Vector3 toWander()
  {
    // Choose a random position for target
    if (Time.time >= nextWanderTime)
    {
      // Reset wander
      nextWanderTime = Time.time + wanderFrequency;

      // Calculate the circle center
      Vector3 circleCenter = host.getVelocity().normalized * circleDistance;

      // Calculate the displacement force
      Vector3 displacement = Vector3.right * circleRadius;

      // Randomly change vector direction, by making it change its current angle
      displacement = setAngle(displacement, wanderAngle);

      // Change wander angle just a bit,
      // so it won't have the same value next game frame
      wanderAngle += Random.Range(0, 1.0f) * ANGLE_CHANGE - ANGLE_CHANGE * 0.5f;

      // Final computation
      wanderTargetPos = displacement;
      Vector3 wanderForce = circleCenter + displacement;
      return wanderForce;
    }
    // Seek towards target
    else
    {
      return toArrival(wanderTargetPos);
    }
  }

  Vector3 setAngle(Vector3 displacement, float angle)
  {
    float len = displacement.magnitude;

    displacement.x = Mathf.Cos(angle) * len;
    displacement.y = Mathf.Sin(angle) * len;

    return displacement;
  }

  Vector3 toEvade(IBoid target)
  {
    Vector3 distance = target.getPosition() - host.getPosition();
    int time = (int)(distance.magnitude / host.getMaxSpeed());
    Vector3 futurePos = target.getPosition() + target.getVelocity() * time;
    return toFlee(futurePos);
  }

  Vector3 toPursuit(IBoid target)
  {
    Vector3 distance = target.getPosition() - host.getPosition();
    int time = (int)(distance.magnitude / host.getMaxSpeed());
    Vector3 futurePos = target.getPosition() + target.getVelocity() * time;
    return toSeek(futurePos);
  }

  public void ResetVariables()
  {
    steeringValue = Vector3.zero;
  }

  public void Update()
  {
    Vector3 velocity = host.getVelocity();
    Vector3 position = host.getPosition();

    // Clamp Steering
    steeringValue = Vector3.ClampMagnitude(steeringValue, host.getMaxForce());

    // Clamp Velocity
    velocity = Vector3.ClampMagnitude(velocity + steeringValue, host.getMaxSpeed());

    host.setVelocity(velocity);
  }
}
