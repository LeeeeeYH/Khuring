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

    GameObject NowStone; // ���� �������� stone
    public bool NowOn; // ���� �߻��ϴ� stone�� ���߾�� �ϴ��� Boolean

    void Start()
    {
        GetComponent<Camera>().orthographicSize = 300;
        NowOn = true;
    }

    void Update()
    {
        if (NowOn) PointingOnCurStone();
    }



    
    public void SetNowStone() // ���� Stone�� CurStone���� ����
    {
        this.NowStone = GameManager.instance.CurStone;
    }

    void PointingOnCurStone() // ī�޶� ���� stone ����
    {
        if (NowStone)
        {
            Vector3 stonePos = this.NowStone.transform.position;
            transform.position = new Vector3(0, Mathf.Min(stonePos.y + 100f, 1710f), -1f);
        }
    }

    public void MoveCameraToStone() // ī�޶�(����)
    {
        NowOn = true;
        GetComponent<Camera>().orthographicSize = 300f;
    }
    public void MoveCameraToHouse() // ī�޶�(�Ͽ콺)
    {
        NowOn = false;
        transform.position = new Vector3(0, 1500.0f, -1f);
        GetComponent<Camera>().orthographicSize = 470f;
    }
    public void MoveCameraToScore() // ī�޶�(���ھ�)
    {
        NowOn = false;
        transform.position = new Vector3(997.0f, 0, -1f);
        GetComponent<Camera>().orthographicSize = 125f;
    }
}
