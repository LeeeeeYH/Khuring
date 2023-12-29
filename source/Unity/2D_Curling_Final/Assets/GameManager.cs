using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null; // Singleton
    private void Awake() // Singleton
    {
        if (instance == null) instance = this;
        else
        {
            if (instance != this) Destroy(this.gameObject);
        }

    }

    public GameObject MainCamera;

    int Round;
    // 0:Red team, 1:Blue team
    public int StoneNum; // 1~16 stone number -> 0~15 in code
    public GameObject CurStone; // 현재 던지는 stone
    GameObject[,] StoneList = new GameObject[2, 8];

    bool Delivering; // 공이 delivery되는 중인지 확인하는 bool 변수

    float[,] StoneDis = new float[2, 9]; // Round 종료후 모든 공의 하우스 중앙과의 거리 { rowVal : minVal, 1 ~ 8 }
    public int[,] ScoreArr = new int[2, 10] { { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },   // Round 점수가 저장된 Array --> 라운드별 점수를 저장할 필요성 없다면 Total저장만 하도록 변경
                                              { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } }; // { rowVal : Total, 1 ~ 8, EE }
    int RoundWinner; // Current Round Winner
    int RoundScore;  // Current Round Score
    bool Gameover; // 게임이 끝인지 판별 Boolean

    // Use in for loop
    string[] TeamNames = new string[2] { "Red", "Blue" }; // { RedTeam, BlueTeam }

    // Replay 변수
    bool ReplayMode;
    public struct Rb2D
    {
        public Vector2 position;
        public float rotation;
        public Vector2 velocity;
        public float angularVelocity;

        public Rb2D(Vector2 _position, float _rotation, Vector2 _velocity, float _angularVelocity)
        {
            this.position = _position;
            this.rotation = _rotation;
            this.velocity = _velocity;
            this.angularVelocity = _angularVelocity;
        }
    }
    public struct ReplayValue
    {
        public int StoneNum, NowStoneNum;
        public bool Delivering;
        public float StartingPower;
        public Vector2 StartingVector;
        public float StartingTorque;
        public List<float> SweepingY; // 스위핑을 했던 시간리스트
        public Rb2D[,] StoneR2D;
        public Rb2D[,] NowR;
        public Vector2 GuideLine;

        void InitValue()
        {
            StartingPower = 0;
            StartingVector = new Vector2(0, 0);
            StartingTorque = 0;
            SweepingY = new List<float>();
            StoneR2D = new Rb2D[2, 8];
            NowR = new Rb2D[2, 8];
            GuideLine = new Vector2(0, 0);
        }
    }
    public ReplayValue Last;

    void Start()
    {
        this.Round = 1;
        this.StoneNum = 0;
        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 8; j++)
                StoneList[i, j] = GameObject.Find(TeamNames[i] + "Stone" + (j + 1).ToString());
        this.Delivering = false;

        this.RoundWinner = Variables.FirstTeam;

        this.Gameover = false;

        this.ReplayMode = false;
        this.Last.SweepingY = new List<float>();
        this.Last.StoneR2D = new Rb2D[2, 8];
        this.Last.NowR = new Rb2D[2, 8];
    }

    void Update()
    {
        if (!Gameover) // 게임중
            InGame();
        else // 게임종료
            GameOvered();
    }


    void RoundInit() // 라운드 시작시 초기화
    {
        StoneNum = 0;
        Round++;
        UIManager.instance.NowRound.GetComponentInChildren<Text>().text = "Round " + Round;
        // 플레이한 스톤 원위치
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                StoneList[i, j].transform.position = new Vector3((float)(-130 + 260 * i + (j / 4) * (-100 + 200 * i)), (float)(-2050 - 60 * (j % 4)));
                StoneList[i, j].GetComponent<StoneController>().StoneInit();
                StoneList[i, j].GetComponent<StoneController>().enabled = false;
                StoneList[i, j].GetComponent<StoneController>().CollisionOn(); // 이전 라운드에서 아웃되어 해제된 충돌판정 복구
                UIManager.instance.LeftStones[i, j].SetActive(true);
            }
        }
    }

    public int GetNowTeam(){ return ((StoneNum % 2) + RoundWinner) % 2; }

    void InGame()
    {
        if (StoneNum < 16) PlayingGame(); // 게임실행, 현재 첫라운드 선공 RedTeam 고정
        else if(StoneNum == 16) // 라운드 종료
        {
            CalRoundScore(); // 라운드 종료후 승패판정, 점수계산, 저장

            UIManager.instance.WriteScore(Round, RoundWinner, RoundScore); // UI에서 점수판에 Write
            UIManager.instance.WriteScore(0, RoundWinner, ScoreArr[RoundWinner, 0]); // UI에서 점수판에 TotalScore Write

            UIManager.instance.CameraChangeScore();
            StoneNum++;
        } // StoneNum == 17 일땐 카메라 Score에서 대기중 돌아오면 18로되면서 라운드초기화
        else if(StoneNum >= 18)
        {
            RoundInit(); // Round Info reset
            if (Round == Variables.MaxRound + 1)
            {
                if (ScoreArr[0, 0] != ScoreArr[1, 0]) Gameover = true; // 8라운드 진행 후 점수차O(연장전X) -> 게임종료
                else Round = 9; // EE
            }
            else if (Round > 9) Gameover = true; // 연장전 1회이후 무조건 게임종료
        }
        if (Input.GetKeyUp(KeyCode.A) && UIManager.instance.NextButton.activeSelf)
            UIManager.instance.ProceedGame();
        else if (Input.GetKeyDown(KeyCode.S) && UIManager.instance.ReplayButton.activeSelf)
            UIManager.instance.WatchReplay();
        else if (Input.GetKeyDown(KeyCode.Q) && UIManager.instance.HouseButton.activeSelf)
            UIManager.instance.CameraChangeHouse();
        else if (Input.GetKeyDown(KeyCode.W) && UIManager.instance.ScoreButton.activeSelf)
            UIManager.instance.CameraChangeScore();
    }

    void PlayingGame() // 게임 진행
    {
        if (!Delivering) // 던지기전 던질 스톤 Setting
        {
            // 매 throw마다 증가하면서 팀구분을 위한 과정
            if (StoneNum % 2 == 0) CurStone = StoneList[RoundWinner, StoneNum / 2];
            else CurStone = StoneList[1 - RoundWinner, StoneNum / 2];

            CurStone.transform.position = new Vector3(0, -2065.0f, 0);
            MainCamera.GetComponent<CameraController>().SetNowStone();
            CurStone.GetComponent<StoneController>().enabled = true;
            CurStone.GetComponent<StoneController>().CollisionOn();
            UIManager.instance.SliderPower.SetActive(true);
            UIManager.instance.GuideLineInit(); // GuideLine 초기화

            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 8; j++)
                    Last.StoneR2D[i, j] = GetRigidBody2D(StoneList[i, j].GetComponent<Rigidbody2D>());

            UIManager.instance.LeftStones[GetNowTeam(), StoneNum / 2].SetActive(false);
            Delivering = true;
        }
        else // After Setting ~ Stop
        {
            if (CurStone.GetComponent<StoneController>().Stopped) // 던진 stone이 멈춤
            {
                if (!ReplayMode)
                {
                    Last.StoneNum = StoneNum;
                    UIManager.instance.ReplayButton.SetActive(true);
                    UIManager.instance.NextButton.SetActive(true);
                }
                else // ★ ReplayMode ★
                {
                    UIManager.instance.SetGuideLine(Last.GuideLine);
                    UIManager.instance.ReplayButton.SetActive(true);
                }
            }
        }
    }

    void CalRoundScore() // 승패판정, 점수계산, List저장 ( ※ 점수인정 최대거리 200 )
    {
        StoneDis[0, 0] = 201.0f; StoneDis[1, 0] = 201.0f;
        RoundScore = 0;

        // 배열에 각 공의 하우스 중앙에서의 거리 계산후 저장
        for (int i = 0; i < 2; i++)
        {
            for (int j = 1; j <= 8; j++)
            {
                Vector3 dis = GameObject.Find(TeamNames[i] + "Stone" + j.ToString()).transform.position;
                StoneDis[i, j] = Mathf.Sqrt(dis.x * dis.x + (dis.y - 1710.0f) * (dis.y - 1710.0f));

                if (StoneDis[i, 0] > StoneDis[i, j]) StoneDis[i, 0] = StoneDis[i, j];
            }
        }
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Vector3 dis = StoneList[i, j].transform.position;
                StoneDis[i, j + 1] = Mathf.Sqrt(dis.x * dis.x + (dis.y - 1710.0f) * (dis.y - 1710.0f));

                if (StoneDis[i, 0] > StoneDis[i, j + 1]) StoneDis[i, 0] = StoneDis[i, j + 1];
            }
        }

        // 무승부( 두팀다 하우스에 돌X == 둘다거리 201.0f )인 경우도 UI Write시 이긴팀에 0점이 부여되므로 구현불필요
        if (StoneDis[0, 0] < StoneDis[1, 0]) RoundWinner = 0;
        else RoundWinner = 1;

        // 진팀의 최소거리보다 작은 이긴팀의 스톤들을 모두 점수처리
        for (int i = 1; i <= 8; i++)
            if (StoneDis[RoundWinner, i] < StoneDis[1 - RoundWinner, 0]) RoundScore++;


        ScoreArr[RoundWinner, 0] += RoundScore; // Total에 추가
        ScoreArr[RoundWinner, Round] += RoundScore; // Round에 추가
    }

    public void NextStoneNum() { StoneNum++; }
    public void StopDelivery() { Delivering = false; }


    Rb2D GetRigidBody2D(Rigidbody2D From){ return new Rb2D(From.position, From.rotation, From.velocity, From.angularVelocity); }
    void SetRigidBody2D(Rb2D From, Rigidbody2D To)
    {
        To.position = From.position;
        To.rotation = From.rotation;
        To.velocity = From.velocity;
        To.angularVelocity = From.angularVelocity;
    }

    public void DoReplay()
    {
        if (!ReplayMode) // NormalMode -> ★ ReplayMode ★
        {
            Last.NowR = new Rb2D[2, 8];
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Last.NowR[i, j] = GetRigidBody2D(StoneList[i, j].GetComponent<Rigidbody2D>());
                    SetRigidBody2D(Last.StoneR2D[i, j], StoneList[i, j].GetComponent<Rigidbody2D>());
                }
            }
            Last.NowStoneNum = StoneNum;
            StoneNum = Last.StoneNum;
            if (StoneNum % 2 == 0) CurStone = StoneList[RoundWinner, StoneNum / 2];
            else CurStone = StoneList[1 - RoundWinner, StoneNum / 2];

            Last.Delivering = Delivering;
            Delivering = false;
            CurStone.GetComponent<StoneController>().ReplayModeOn();
            ReplayMode = true;
        }
        else // ★ ReplayMode ★ -> NormalMode
        {
            CurStone.GetComponent<StoneController>().ReplayModeOff();
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 8; j++)
                    SetRigidBody2D(Last.NowR[i, j], StoneList[i, j].GetComponent<Rigidbody2D>());
            StoneNum = Last.NowStoneNum;
            if (StoneNum % 2 == 0) CurStone = StoneList[RoundWinner, StoneNum / 2];
            else CurStone = StoneList[1 - RoundWinner, StoneNum / 2];

            Delivering = Last.Delivering;
            ReplayMode = false;
        }
    }

    void GameOvered() // 게임 종료
    {
        if (ScoreArr[0, 0] > ScoreArr[1, 0]) // RedTeam Win
            Variables.WinTeam = 0;
        else if (ScoreArr[0, 0] < ScoreArr[1, 0])
            Variables.WinTeam = 1;
        else // Draw
            Variables.WinTeam = 2;
        SceneManager.LoadScene(2);
    }
}
