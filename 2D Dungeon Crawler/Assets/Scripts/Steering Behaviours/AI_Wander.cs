using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Wander : MonoBehaviour
{
    // twerkable
    [Header("Movement Twerkable")]
    public float maxSpeed = 10.0f;
    public float maxForce = 10.0f;

    [Header("Arrival Twerkable")]
    public float slowRadius = 5.0f;

    [Header("Wander Twerkable")]
    public float circleDistance = 5.0f;
    public float circleRadius = 3.0f;
    public float wanderAngle = 15.0f;
    public float ANGLE_CHANGE = 0.5f;
    public float wanderFrequency = 3.0f;

    // Private
    Vector3 moveDir = Vector3.right;
    Vector3 targetPos;
    Vector3 velocity;
    Vector3 acceleration;

    // Private: Wander
    float nextWanderTime;

    private void Start()
    {
        // Get initial velocity
        velocity = moveDir * maxSpeed;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(targetPos, 0.2f);
    }

    private void Update()
    {
        //---------------------------------------
        // Control(s)
        //---------------------------------------
        CheckForPause();

        // Get Mouse Position
        //GetMousePosition();


        //---------------------------------------
        // Reset(s)
        //---------------------------------------
        acceleration = Vector3.zero;


        //---------------------------------------
        // Add Steering Behaviours Acceleration
        //---------------------------------------
        acceleration += Wander(ref velocity);


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
        if (Input.GetKeyDown(KeyCode.Space))
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
    Vector3 Flee(ref Vector3 vel)
    {
        return -Seek(ref vel);
    }
    Vector3 Arrival(ref Vector3 vel)
    {
        Vector3 desiredVel = (targetPos - transform.position);
        float distance = desiredVel.magnitude;

        //-- Check dist to determine inside/outside slow radius
        // Inside radius
        if (distance < slowRadius)
        {
            desiredVel = desiredVel.normalized * maxSpeed * (distance / slowRadius);
        }
        // Outside Radius
        else
        {
            desiredVel = desiredVel.normalized * maxSpeed;
        }

        Vector3 steering = desiredVel - vel;
        steering = Vector3.ClampMagnitude(steering, maxForce);

        return steering;
    }

    Vector3 Wander(ref Vector3 vel)
    {
        // Choose a random position for target
        if(Time.time >= nextWanderTime)
        {
            // Reset wander
            nextWanderTime = Time.time + wanderFrequency;

            // Calculate the circle center
            Vector3 circleCenter = vel.normalized * circleDistance;

            // Calculate the displacement force
            Vector3 displacement = Vector3.right * circleRadius;

            // Randomly change vector direction, by making it change its current angle
            setAngle(ref displacement, wanderAngle);

            // Change wander angle just a bit,
            // so it won't have the same value next game frame
            wanderAngle += Random.Range(0, 1.0f) * ANGLE_CHANGE - ANGLE_CHANGE * 0.5f;

            // Final computation
            targetPos = displacement;
            Vector3 wanderForce = circleCenter + displacement;
            return wanderForce;
        }
        // Seek towards target
        else
        {
            return Arrival(ref vel);
        }
        

    }

    void setAngle(ref Vector3 displacement, float angle)
    {
        float len = displacement.magnitude;

        displacement.x = Mathf.Cos(angle) * len;
        displacement.y = Mathf.Sin(angle) * len;
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
