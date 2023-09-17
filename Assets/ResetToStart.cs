using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetToStart : MonoBehaviour
{
    [SerializeField] Transform start;

    void OnCollisionEnter(Collision hit){
        hit.transform.position = start.position;
    }
}
