using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private int playerId;
    public void MovePlayer(Vector3 position)
    {
        transform.position = position;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
