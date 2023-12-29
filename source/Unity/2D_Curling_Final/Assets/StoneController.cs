using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class StoneController : MonoBehaviour
{
    bool Shot; // throw�� �ѹ� �ϸ� �ٽ� force�� ���� �ʱ����� ����
    public bool Stopped; // stone�� ������� �Ǵ��ϴ� ����
    Rigidbody2D Rigid2D;
    //float delay;
    float StartingPower; // ���濡 ó�� �������� ��
    float StartingForce; // ��ũ���� ���
    Vector2 StartingVector;
    float CurrentForce; // ���� �ӵ� ���� ����
    bool Rotating;
    Vector2 TempVector; // ȣ�׶��� ���޽� ��� �����ϴ� ����
    float StartingTorque; //���� ó�� ȸ����
    float OscillateTorque;
    float CurrentTorque; // ���� ������ ȸ����
    bool BroomLocation; // �¿� �����ư��鼭 �������ϱ����� ���� (false: ��, true: ��)

    // Replay����
    bool ReplayMode;
    float ReplayStartingPower;
    float ReplayStartingTorque;
    float ReplayOscillateTorque;
    int ReplaySweepingIndex;
    float SweepingDelta;

    bool LeftSweepingFlag;
    bool RightSweepingFlag;

    // 0 ~ Size ������ ���ڸ� �ݺ�
    //float OscillateNum(float Num, int Size) { return Mathf.Abs(Mathf.Abs(Num % (Size * 2 + 1) - Size) - Size); }
    //float OscillateNum(float Num, int Size) { return Mathf.Abs((Num + Size) % (Size * 2 + 1) - Size); } // �� ������ ���
    float OscillateNum(float Num, int Min, int Max) { return Mathf.Abs((Num + Max - Min) % (2 * (Max - Min) + 1) - Max + Min) + Min; } // �ּ�ġ �߰� (Min ~ Max)

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
            if (!Shot) // �����
                SetBfShot();
            else // �� �� --> ȣ�׶���, ȣ�׶��� ��
            {
                if (!Rotating) OnHogLine(); // �� �� ȣ�׶��ο� ���� ���(ȸ���� �Է���)
                else AfHogLine(); // ȸ���� �Է� �� ��������(�������Է�)
            }
        }
    }

    void FixedUpdate()
    {
        if (Rotating) AfHogLineFixed(); // ȸ���� �Է� �� ��������(�������)
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

    void SetBfShot() // ���� �� �� Power Setting
    {
        if (UIManager.instance.NowSweeper != null) UIManager.instance.SweeperOff();
        if (!ReplayMode)
        {
            if (CameraController.instance.NowOn)
            {
                // ���� �Է��� �ǽð����� ����Ǳ� ������ ���⼱ �Ŀ� ��ġ�� ����
                // �����̽��� ������ �ִ� ���� �Ŀ� ��ġ�� ���� �ö�
                if (Input.GetKeyDown(KeyCode.A))
                    UIManager.instance.ReplayButton.SetActive(false);
                else if (Input.GetKey(KeyCode.A)) // A ������ ����
                {
                    this.StartingPower += 0.1f; // ������ �ִ� ���� �Ŀ� +0.1��
                    UIManager.instance.SliderPower.GetComponent<Slider>().value = (OscillateNum(StartingPower, 40, 60) - 40) / 20;
                }
                else if (Input.GetKeyUp(KeyCode.A)) // A ���� �߻�
                {
                    if (OscillateNum(StartingPower, 40, 60) <= 57f) // ���� ������ ���� ����
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
        else // �� ReplayMode ��
        {
            if (ReplayStartingPower < StartingPower)
            {
                ReplayStartingPower += 0.1f; // ������ �ִ� ���� �Ŀ� +0.1��
                UIManager.instance.SliderPower.GetComponent<Slider>().value = (OscillateNum(ReplayStartingPower, 40, 60) - 40) / 20;
            }
            else ApplyPower();
        }
    }

    void ApplyPower()
    {
        Rigid2D.AddForce(StartingVector);
        StartingForce = Mathf.Sqrt(StartingVector.x * StartingVector.x + StartingVector.y * StartingVector.y); // ȸ���� ������ ����ϴ� ��Į��

        UIManager.instance.SliderPower.GetComponent<Slider>().value = 0; // �Ŀ� ������ �ʱ�ȭ
        UIManager.instance.SliderPower.SetActive(false); // ������ ��Ȱ��ȭ

        Shot = true;
    }

    void OnHogLine() // HogLine ���޽� Torque Setting
    {
        if (transform.position.y > -1100) // ȣ�׶��ο� ���� ���
        {
            if (Rigid2D.velocity.y != 0) // �ϴ� ������ ����
            {
                TempVector = Rigid2D.velocity; // ���� �̵����� ���� ����
                Rigid2D.velocity = new Vector2(0, 0); // �ϴ� ������Ŵ
                UIManager.instance.SliderTorque.SetActive(true); // ������ Ȱ��ȭ
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
                                UIManager.instance.SliderTorque.GetComponent<Image>().fillClockwise = false; // �ݽð�
                                UIManager.instance.SliderTorque.GetComponent<Image>().fillAmount = OscillateTorque / 500f;
                            }
                            else
                            {
                                UIManager.instance.SliderTorque.GetComponent<Image>().fillClockwise = true; // �ð�
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
                else // �� ReplayMode ��
                {
                    if (ReplayStartingTorque < StartingTorque)
                    {
                        ReplayStartingTorque += 10f;
                        ReplayOscillateTorque = OscillateNum(ReplayStartingTorque, -500, 500);
                        if (ReplayOscillateTorque > 0)
                        {
                            UIManager.instance.SliderTorque.GetComponent<Image>().fillClockwise = false; // �ݽð�
                            UIManager.instance.SliderTorque.GetComponent<Image>().fillAmount = ReplayOscillateTorque / 500f;
                        }
                        else
                        {
                            UIManager.instance.SliderTorque.GetComponent<Image>().fillClockwise = true; // �ð�
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
        // �Է¹��� �� �������� ȸ�� �߰� & �ٽ� ���
        Rigid2D.velocity = TempVector;
        Rigid2D.AddTorque(OscillateTorque * 150); // ��߽� ������ ȸ���� ���� ȸ���� Setting
        UIManager.instance.SliderTorque.SetActive(false);
        Rotating = true;
        UIManager.instance.SweeperOn();
        SweepingDelta = 0;
    }

    void AfHogLine()
    {
        if (!ReplayMode)
        {
            if (Input.GetKeyUp(KeyCode.LeftArrow) && !BroomLocation) // Broom�� ������(false)�� �ְ� ����Ű�� ������
                LeftSweepingFlag = true;
            else if (Input.GetKeyUp(KeyCode.RightArrow) && BroomLocation) // Broom�� ����(true)�� �ְ� ������Ű�� ������
                RightSweepingFlag = true;
        }
    }

    void AfHogLineFixed() // Delivery�� ����ȭ
    {
        SweepingDelta += Time.deltaTime;
        CurrentTorque = this.Rigid2D.angularVelocity; // ��ũ�� ���� ȸ���� �Է�
        // ������ ���� �̵��ϴ� �ӷ� ���
        CurrentForce = Mathf.Sqrt(Rigid2D.velocity.x * Rigid2D.velocity.x + Rigid2D.velocity.y * Rigid2D.velocity.y);

        // ȸ���¿� ���� �ش� �������� ���� ������
        // ������ �������� ���� ȸ������ ���⵵ �پ�鵵�� ����
        // -2�� ȸ���� ����� ���
        Rigid2D.AddForce(new Vector2(CurrentTorque * -2 * CurrentForce / StartingForce, 0));

        if (!ReplayMode)
        {
            if (LeftSweepingFlag) // Broom�� ������(false)�� �ְ� ����Ű�� ������
            {
                BroomLocation = true;
                Rigid2D.drag = Mathf.Max(Rigid2D.drag - 0.02f, 0.01f);
                UIManager.instance.SweepingLeft();
                GameManager.instance.Last.SweepingY.Add(SweepingDelta);
                LeftSweepingFlag = false;
            }
            else if (RightSweepingFlag) // Broom�� ����(false)�� �ְ� ������Ű�� ������
            {
                BroomLocation = false;
                Rigid2D.drag = Mathf.Max(Rigid2D.drag - 0.02f, 0.01f);
                UIManager.instance.SweepingRight();
                GameManager.instance.Last.SweepingY.Add(SweepingDelta);
                RightSweepingFlag = false;
            }
            // ������ �ϰ� ���� ������ �ٽ� ���������� �������� ���Ƿ� ����
            if (Rigid2D.drag < 0.1f)
                Rigid2D.drag += 0.01f;
        }
        else // �� ReplayMode ��
        {
            if (ReplaySweepingIndex < GameManager.instance.Last.SweepingY.Count && SweepingDelta >= GameManager.instance.Last.SweepingY[ReplaySweepingIndex])
            {
                Rigid2D.drag = Mathf.Max(Rigid2D.drag - 0.02f, 0.01f);
                if(ReplaySweepingIndex % 2 ==0) UIManager.instance.SweepingLeft();
                else UIManager.instance.SweepingRight();
                ReplaySweepingIndex++;
            }
            // ������ �ϰ� ���� ������ �ٽ� ���������� �������� ���Ƿ� ����
            if (Rigid2D.drag < 0.1f)
                Rigid2D.drag += 0.01f;
        }

        if (this.Rigid2D.velocity.y < 0.5f)  // ����� �������� ����
        {
            if (transform.position.y <= 1105.0f) CollisionOff();
            Rigid2D.angularVelocity = 0;
            Stopped = true;
        }

        // �ƿ�
        if (transform.position.x < -230.0f || transform.position.x > 230.0f || transform.position.y > 1910.0f)
        {
            CollisionOff(); // �ƿ� ���ڸ��� ������ �ٸ� ����� �浹���� �ʵ��� ����
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