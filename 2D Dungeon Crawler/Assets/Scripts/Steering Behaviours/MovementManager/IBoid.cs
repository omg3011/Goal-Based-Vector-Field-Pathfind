using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]  // You need to have this line in there
public abstract class IBoid : MonoBehaviour
{
  public abstract Vector3 getVelocity();

  public abstract float getMaxSpeed();

  public abstract float getMaxForce();

  public abstract Vector3 getPosition();


  public abstract void setVelocity(Vector3 vel);
  public abstract void setPosition(Vector3 pos);

}