using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Variables : MonoBehaviour
{
    static public int MaxRound;
    static public int FirstTeam; // 0 : RedTeam, 1 : BlueTeam
    static public int WinTeam; // 0 : RedTeam, 1 : BlueTeam, 2 : Draw

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        MaxRound = 2;
    }
}
