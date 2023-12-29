using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class End_CameraController : MonoBehaviour
{
    void Start()
    {
        transform.position = new Vector3(-2000f + Variables.WinTeam * 2000f, 0, -1f);
    }
}
