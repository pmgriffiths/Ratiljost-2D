using System;
using UnityEngine;

public abstract class Trackable : MonoBehaviour
{
    public enum Direction { Forwards, Backwards };

    public Direction facingDirection;
}
