using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class StoneController : MonoBehaviour
{
    bool Shot; // throw를 한번 하면 다시 force를 주지 않기위한 변수
    public bool Stopped; // stone이 멈췄는지 판단하는 변수
    Rigidbody2D Rigid2D;
    //float delay;
    float StartingPower; // 스톤에 처음 가해지는 힘
    float StartingForce; // 토크계산시 사용
    Vector2 StartingVector;
    float CurrentForce; // 현재 속도 저장 변수
    bool Rotating;
    Vector2 TempVector; // 호그라인 도달시 잠시 저장하는 벡터
    float StartingTorque; //스톤 처음 회전력
    float OscillateTorque;
    float CurrentTorque; // 현재 스톤의 회전력
    bool BroomLocation; // 좌우 번갈아가면서 스위핑하기위한 변수 (false: 우, true: 좌)

    // Replay변수
    bool ReplayMode;
    float ReplayStartingPower;
    float ReplayStartingTorque;
    float ReplayOscillateTorque;
    int ReplaySweepingIndex;
    float SweepingDelta;

    bool LeftSweepingFlag;
    bool RightSweepingFlag;

    // 0 ~ Size 까지의 숫자를 반복
    //float OscillateNum(float Num, int Size) { return Mathf.Abs(Mathf.Abs(Num % (Size * 2 + 1) - Size) - Size); }
    //float OscillateNum(float Num, int Size) { return Mathf.Abs((Num + Size) % (Size * 2 + 1) - Size); } // 더 간단한 방법
    float OscillateNum(float Num, int Min, int Max) { return Mathf.Abs((Num + Max - Min) % (2 * (Max - Min) + 1) - Max + Min) + Min; } // 최소치 추가 (Min ~ Max)

    void Start()
    {
        this.Rigid2D = GetComponent<Rigidbody2D>();
        StoneInit();
        //this.KeyFlag = 0;
        this.BroomLocation = false;
        this.ReplayMode = false;
    }

    void Update()
    {
        if (!Stopped)
        {
            if (!Shot) // 쏘기전
                SetBfShot();
            else // 쏜 후 --> 호그라인, 호그라인 후
            {
                if (!Rotating) OnHogLine(); // 샷 후 호그라인에 들어섰을 경우(회전력 입력전)
                else AfHogLine(); // 회전력 입력 후 딜리버링(스위핑입력)
            }
        }
    }

    void FixedUpdate()
    {
        if (Rotating) AfHogLineFixed(); // 회전력 입력 후 딜리버링(물리계산)
    }


    public void StoneInit()
    {
        this.Rigid2D.velocity = new Vector3(0, 0, 0);
        this.Rigid2D.angularVelocity = 0;
        this.transform.localEulerAngles = new Vector3(0, 0, 0);
        this.Rigid2D.drag = 0;
        this.Shot = false;
        this.Stopped = false;
        //this.delay = 0;
        this.StartingPower = 0;
        this.StartingForce = 0;
        this.StartingVector = new Vector2(0, 0);
        this.CurrentForce = 0;
        this.Rotating = false;
        this.TempVector = new Vector2(0, 0);
        this.StartingTorque = 0;
        this.CurrentTorque = 0f;
        this.SweepingDelta = 0;
        this.LeftSweepingFlag = false;
        this.RightSweepingFlag = false;
    }

    void SetBfShot() // 스톤 샷 전 Power Setting
    {
        if (UIManager.instance.NowSweeper != null) UIManager.instance.SweeperOff();
        if (!ReplayMode)
        {
            if (CameraController.instance.NowOn)
            {
                // 방향 입력은 실시간으로 적용되기 때문에 여기선 파워 수치만 조절
                // 스페이스를 누르고 있는 동안 파워 수치가 점점 올라감
                if (Input.GetKeyDown(KeyCode.A))
                    UIManager.instance.ReplayButton.SetActive(false);
                else if (Input.GetKey(KeyCode.A)) // A 누르는 동안
                {
                    this.StartingPower += 0.1f; // 누르고 있는 동안 파워 +0.1씩
                    UIManager.instance.SliderPower.GetComponent<Slider>().value = (OscillateNum(StartingPower, 40, 60) - 40) / 20;
                }
                else if (Input.GetKeyUp(KeyCode.A)) // A 떼면 발사
                {
                    if (OscillateNum(StartingPower, 40, 60) <= 57f) // 스톤 던지는 힘과 방향
                        StartingVector = UIManager.instance.GetGuideLineVector() * OscillateNum(StartingPower, 40, 60) * 3; 
                    else // PowerShot
                        StartingVector = UIManager.instance.GetGuideLineVector() * OscillateNum(StartingPower, 40, 60) * 4; 
                    ApplyPower();

                    GameManager.instance.Last.StartingPower = StartingPower;
                    GameManager.instance.Last.StartingVector = StartingVector;
                    GameManager.instance.Last.GuideLine = UIManager.instance.GetGLEndVector();
                }
            }
        }
        else // ★ ReplayMode ★
        {
            if (ReplayStartingPower < StartingPower)
            {
                ReplayStartingPower += 0.1f; // 누르고 있는 동안 파워 +0.1씩
                UIManager.instance.SliderPower.GetComponent<Slider>().value = (OscillateNum(ReplayStartingPower, 40, 60) - 40) / 20;
            }
            else ApplyPower();
        }
    }

    void ApplyPower()
    {
        Rigid2D.AddForce(StartingVector);
        StartingForce = Mathf.Sqrt(StartingVector.x * StartingVector.x + StartingVector.y * StartingVector.y); // 회전력 조절시 사용하는 스칼라값

        UIManager.instance.SliderPower.GetComponent<Slider>().value = 0; // 파워 게이지 초기화
        UIManager.instance.SliderPower.SetActive(false); // 게이지 비활성화

        Shot = true;
    }

    void OnHogLine() // HogLine 도달시 Torque Setting
    {
        if (transform.position.y > -1100) // 호그라인에 들어섰을 경우
        {
            if (Rigid2D.velocity.y != 0) // 일단 정지후 저장
            {
                TempVector = Rigid2D.velocity; // 현재 이동중인 벡터 저장
                Rigid2D.velocity = new Vector2(0, 0); // 일단 정지시킴
                UIManager.instance.SliderTorque.SetActive(true); // 게이지 활성화
                UIManager.instance.SliderTorque.GetComponent<Image>().fillAmount = 0;
            }
            else
            {
                if (!ReplayMode)
                {
                    if (CameraController.instance.NowOn)
                    {
                        if (Input.GetKey(KeyCode.A))
                        {
                            StartingTorque += 10f;
                            OscillateTorque = OscillateNum(StartingTorque, -500, 500);
                            if (OscillateTorque > 0)
                            {
                                UIManager.instance.SliderTorque.GetComponent<Image>().fillClockwise = false; // 반시계
                                UIManager.instance.SliderTorque.GetComponent<Image>().fillAmount = OscillateTorque / 500f;
                            }
                            else
                            {
                                UIManager.instance.SliderTorque.GetComponent<Image>().fillClockwise = true; // 시계
                                UIManager.instance.SliderTorque.GetComponent<Image>().fillAmount = -OscillateTorque / 500f;
                            }
                        }
                        else if (Input.GetKeyUp(KeyCode.A))
                        {
                            GameManager.instance.Last.StartingTorque = StartingTorque;
                            ApplyTorque();
                        }
                    }
                }
                else // ★ ReplayMode ★
                {
                    if (ReplayStartingTorque < StartingTorque)
                    {
                        ReplayStartingTorque += 10f;
                        ReplayOscillateTorque = OscillateNum(ReplayStartingTorque, -500, 500);
                        if (ReplayOscillateTorque > 0)
                        {
                            UIManager.instance.SliderTorque.GetComponent<Image>().fillClockwise = false; // 반시계
                            UIManager.instance.SliderTorque.GetComponent<Image>().fillAmount = ReplayOscillateTorque / 500f;
                        }
                        else
                        {
                            UIManager.instance.SliderTorque.GetComponent<Image>().fillClockwise = true; // 시계
                            UIManager.instance.SliderTorque.GetComponent<Image>().fillAmount = -ReplayOscillateTorque / 500f;
                        }
                    }
                    else ApplyTorque();
                }
            }
        }
    }

    void ApplyTorque()
    {
        // 입력받은 값 바탕으로 회전 추가 & 다시 출발
        Rigid2D.velocity = TempVector;
        Rigid2D.AddTorque(OscillateTorque * 150); // 출발시 가해진 회전에 따라 회전력 Setting
        UIManager.instance.SliderTorque.SetActive(false);
        Rotating = true;
        UIManager.instance.SweeperOn();
        SweepingDelta = 0;
    }

    void AfHogLine()
    {
        if (!ReplayMode)
        {
            if (Input.GetKeyUp(KeyCode.LeftArrow) && !BroomLocation) // Broom이 오른쪽(false)에 있고 왼쪽키를 누를때
                LeftSweepingFlag = true;
            else if (Input.GetKeyUp(KeyCode.RightArrow) && BroomLocation) // Broom이 왼쪽(true)에 있고 오른쪽키를 누를때
                RightSweepingFlag = true;
        }
    }

    void AfHogLineFixed() // Delivery중 값변화
    {
        SweepingDelta += Time.deltaTime;
        CurrentTorque = this.Rigid2D.angularVelocity; // 토크에 현재 회전력 입력
        // 스톤이 현재 이동하는 속력 계산
        CurrentForce = Mathf.Sqrt(Rigid2D.velocity.x * Rigid2D.velocity.x + Rigid2D.velocity.y * Rigid2D.velocity.y);

        // 회전력에 따라 해당 방향으로 힘을 가해줌
        // 스톤이 느려짐에 따라 회전력의 영향도 줄어들도록 구현
        // -2는 회전의 영향력 계수
        Rigid2D.AddForce(new Vector2(CurrentTorque * -2 * CurrentForce / StartingForce, 0));

        if (!ReplayMode)
        {
            if (LeftSweepingFlag) // Broom이 오른쪽(false)에 있고 왼쪽키를 누를때
            {
                BroomLocation = true;
                Rigid2D.drag = Mathf.Max(Rigid2D.drag - 0.02f, 0.01f);
                UIManager.instance.SweepingLeft();
                GameManager.instance.Last.SweepingY.Add(SweepingDelta);
                LeftSweepingFlag = false;
            }
            else if (RightSweepingFlag) // Broom이 왼쪽(false)에 있고 오른쪽키를 누를때
            {
                BroomLocation = false;
                Rigid2D.drag = Mathf.Max(Rigid2D.drag - 0.02f, 0.01f);
                UIManager.instance.SweepingRight();
                GameManager.instance.Last.SweepingY.Add(SweepingDelta);
                RightSweepingFlag = false;
            }
            // 스위핑 하고 있지 않으면 다시 울퉁불퉁한 빙판으로 들어서므로 복구
            if (Rigid2D.drag < 0.1f)
                Rigid2D.drag += 0.01f;
        }
        else // ★ ReplayMode ★
        {
            if (ReplaySweepingIndex < GameManager.instance.Last.SweepingY.Count && SweepingDelta >= GameManager.instance.Last.SweepingY[ReplaySweepingIndex])
            {
                Rigid2D.drag = Mathf.Max(Rigid2D.drag - 0.02f, 0.01f);
                if(ReplaySweepingIndex % 2 ==0) UIManager.instance.SweepingLeft();
                else UIManager.instance.SweepingRight();
                ReplaySweepingIndex++;
            }
            // 스위핑 하고 있지 않으면 다시 울퉁불퉁한 빙판으로 들어서므로 복구
            if (Rigid2D.drag < 0.1f)
                Rigid2D.drag += 0.01f;
        }

        if (this.Rigid2D.velocity.y < 0.5f)  // 충분히 느려지면 멈춤
        {
            if (transform.position.y <= 1105.0f) CollisionOff();
            Rigid2D.angularVelocity = 0;
            Stopped = true;
        }

        // 아웃
        if (transform.position.x < -230.0f || transform.position.x > 230.0f || transform.position.y > 1910.0f)
        {
            CollisionOff(); // 아웃 되자마자 스톤이 다른 스톤과 충돌하지 않도록 조절
            Stopped = true;
        }
    }

    public void ReplayModeOn()
    {
        ReplayStartingPower = 0;
        StartingPower = GameManager.instance.Last.StartingPower;
        StartingVector = GameManager.instance.Last.StartingVector;
        ReplayStartingTorque = 0;
        StartingTorque = GameManager.instance.Last.StartingTorque;
        ReplayOscillateTorque = 0;
        ReplaySweepingIndex = 0;
        Rigid2D.drag = 0;

        Shot = false;
        Rotating = false;
        Stopped = false;

        ReplayMode = true;
    }

    public void ReplayModeOff() { ReplayMode = false; }

    public void CollisionOn()
    {
        this.GetComponent<CircleCollider2D>().enabled = true;
        this.GetComponentInChildren<CircleCollider2D>().enabled = true;
        this.GetComponent<StoneCollision>().enabled = true;
        this.GetComponentInChildren<StoneTrigger>().enabled = true;
    }
    public void CollisionOff()
    {
        this.GetComponent<CircleCollider2D>().enabled = false;
        this.GetComponentInChildren<CircleCollider2D>().enabled = false;
        this.GetComponent<StoneCollision>().enabled = false;
        this.GetComponentInChildren<StoneTrigger>().enabled = false;
    }
}