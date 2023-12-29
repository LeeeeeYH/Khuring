using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    public static CameraController instance = null; // Singleton
    private void Awake() // Singleton
    {
        if (instance == null) instance = this;
        else
        {
            if (instance != this) Destroy(this.gameObject);
        }
    }

    GameObject NowStone; // 현재 진행중인 stone
    public bool NowOn; // 지금 발사하는 stone을 비추어야 하는지 Boolean

    void Start()
    {
        GetComponent<Camera>().orthographicSize = 300;
        NowOn = true;
    }

    void Update()
    {
        if (NowOn) PointingOnCurStone();
    }



    
    public void SetNowStone() // 비출 Stone을 CurStone으로 설정
    {
        this.NowStone = GameManager.instance.CurStone;
    }

    void PointingOnCurStone() // 카메라 현재 stone 고정
    {
        if (NowStone)
        {
            Vector3 stonePos = this.NowStone.transform.position;
            transform.position = new Vector3(0, Mathf.Min(stonePos.y + 100f, 1710f), -1f);
        }
    }

    public void MoveCameraToStone() // 카메라(스톤)
    {
        NowOn = true;
        GetComponent<Camera>().orthographicSize = 300f;
    }
    public void MoveCameraToHouse() // 카메라(하우스)
    {
        NowOn = false;
        transform.position = new Vector3(0, 1500.0f, -1f);
        GetComponent<Camera>().orthographicSize = 470f;
    }
    public void MoveCameraToScore() // 카메라(스코어)
    {
        NowOn = false;
        transform.position = new Vector3(997.0f, 0, -1f);
        GetComponent<Camera>().orthographicSize = 125f;
    }
}
