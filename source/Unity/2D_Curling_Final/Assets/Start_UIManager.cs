using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class Start_UIManager : MonoBehaviour
{
    public static Start_UIManager instance = null; // Singleton
    private void Awake() // Singleton
    {
        if (instance == null) instance = this;
        else
        {
            if (instance != this) Destroy(this.gameObject);
        }
    }

    public Camera MainCamera;
    enum State { Main, RoundSelect, HowTo, Info, FirstTeamSelect, AfFirstTeamSelect };
    State state;
    public GameObject SelectArrow;

    public struct Pos
    {
        public float x, y;
        public Pos(float _x, float _y) { this.x = _x; this.y = _y; }
        public Vector2 V2() { return new Vector2(x, y); }
    }
    Pos[] ArrowPosArr;
    int ArrowNum;

    public GameObject Title;
    public GameObject PressA;
    public GameObject StartButton; // 1 ( SelectArrow순서 )
    public GameObject HowToButton; // 2
    public GameObject InfoButton; // 3
    public GameObject Round;
    public GameObject RoundSelect; // 4
    public GameObject TeamSelectButton; // 5
    public GameObject Coin;
    int TurnNum;

    int HowNum;
    public GameObject BeforeButton;
    public GameObject HomeButton;
    public GameObject AfterButton;
    public GameObject SpeedRaw;
    public GameObject TorqueRaw;
    public GameObject SweepingRaw;
    public VideoPlayer SpeedVideo;
    public VideoPlayer TorqueVideo;
    public VideoPlayer SweepingVideo;

    float[] C = new float[3] { 0, 255f, 0 };
    int Index = 0;


    void Start()
    {
        ArrowPosArr = new Pos[6] { new Pos(0,0), new Pos(0, -25f), new Pos(0, -60f), 
            new Pos(0, -95f),  new Pos(0, -70f), new Pos(0, -100f) };
        MainScreen();
    }

    void Update()
    {
        if(Index % 2 == 0)
        {
            C[Index % 3]++;
            if (C[Index % 3] > 255f) Index++;
        }
        else
        {
            C[Index % 3]--;
            if (C[Index % 3] < 0) Index++;
        }
        PressA.GetComponent<Text>().color = new Color(C[0]/255f, C[1]/255f, C[2]/255f);

        if (state == State.Main) // 메인화면
        {
            if (Input.GetKeyDown(KeyCode.DownArrow) && ArrowNum < 3)
                SelectArrow.GetComponent<RectTransform>().anchoredPosition = ArrowPosArr[++ArrowNum].V2();
            else if (Input.GetKeyDown(KeyCode.UpArrow) && ArrowNum > 1)
                SelectArrow.GetComponent<RectTransform>().anchoredPosition = ArrowPosArr[--ArrowNum].V2();
            else if (Input.GetKeyDown(KeyCode.A))
            {
                switch (ArrowNum)
                {
                    case 1: // 게임시작
                        state = State.RoundSelect;
                        RoundSelectScreen();
                        break;
                    case 2: // 게임방법
                        SelectArrow.SetActive(false);
                        Title.SetActive(false);
                        PressA.SetActive(false);
                        StartButton.SetActive(false);
                        HowToButton.SetActive(false);
                        InfoButton.SetActive(false);

                        state = State.HowTo;
                        HowNum = 1;
                        HowToScreen();
                        break;
                    case 3: // 게임정보
                        state = State.Info;
                        InfoScreen();
                        break;

                }
            }
        }
        else if(state == State.RoundSelect) // 최대라운드설정
        {
            if (Input.GetKeyDown(KeyCode.E)) // 홈
                MainScreen();
            else if (Input.GetKeyDown(KeyCode.RightArrow) && Variables.MaxRound < 8)
            {
                Variables.MaxRound += 2;
                RoundSelect.GetComponentInChildren<Text>().text = Variables.MaxRound.ToString();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) && Variables.MaxRound > 2)
            {
                Variables.MaxRound -= 2;
                RoundSelect.GetComponentInChildren<Text>().text = Variables.MaxRound.ToString();
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                state = State.FirstTeamSelect;
                FirstTeamSelectScreen();
            }
        }
        else if (state == State.FirstTeamSelect)
        {
            if (Input.GetKeyDown(KeyCode.E)) // 홈
                MainScreen();
            else if (Input.GetKeyDown(KeyCode.A))
            {
                TeamSelectButton.GetComponentInChildren<Text>().text = "SPINNING...";
                StartCoroutine(TurnCoin());
                state = State.AfFirstTeamSelect;
            }
        }
        else if (state == State.AfFirstTeamSelect)
        {   
            if (Input.GetKeyDown(KeyCode.E)) // 홈
                MainScreen();
            else if (Input.GetKeyDown(KeyCode.A)) SceneManager.LoadScene(1);
        }
        else if (state == State.HowTo)
        {
            if (Input.GetKeyDown(KeyCode.E)) // 홈
            {
                SpeedVideo.Stop();
                SweepingVideo.Stop();
                TorqueVideo.Stop();
                MainScreen();
            }
            else if (Input.GetKeyDown(KeyCode.W) && HowNum < 6) // 다음설명
            {
                HowNum++;
                HowToScreen();
            }
            else if (Input.GetKeyDown(KeyCode.Q) && HowNum > 1) // 이전설명
            {
                HowNum--;
                HowToScreen();
            }
        }
        else if (state == State.Info)
        {
            if (Input.GetKeyDown(KeyCode.E)) // 홈
                MainScreen();
        }
    }

    
    public void MainScreen() // 메인화면
    {
        MainCamera.transform.position = new Vector3(0, 0, -1f);
        state = State.Main;

        SelectArrow.SetActive(true);
        SelectArrow.GetComponentInChildren<Text>().text = "▶                           ◀";
        ArrowNum = 1;
        SelectArrow.GetComponent<RectTransform>().anchoredPosition = ArrowPosArr[ArrowNum].V2();

        Title.SetActive(true);
        PressA.SetActive(true);
        PressA.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100f, 30f);
        StartButton.SetActive(true);
        HowToButton.SetActive(true);
        InfoButton.SetActive(true);
        Round.SetActive(false);
        RoundSelect.SetActive(false);
        Coin.SetActive(false);
        TeamSelectButton.SetActive(false);
        BeforeButton.SetActive(false);
        HomeButton.SetActive(false);
        AfterButton.SetActive(false);
        SpeedRaw.SetActive(false);
        TorqueRaw.SetActive(false);
        SweepingRaw.SetActive(false);
    }

    // 시작버튼 - 라운드선택화면
    public void RoundSelectScreen()
    {
        ArrowNum = 4;
        SelectArrow.GetComponent<RectTransform>().anchoredPosition = ArrowPosArr[ArrowNum].V2();
        SelectArrow.GetComponentInChildren<Text>().text = "◀                           ▶";

        StartButton.SetActive(false);
        HowToButton.SetActive(false);
        InfoButton.SetActive(false);
        HomeButton.SetActive(true);
        Round.SetActive(true);
        RoundSelect.SetActive(true);
    }

    public void FirstTeamSelectScreen() // 팀 선택 화면
    {
        ArrowNum = 5;
        SelectArrow.GetComponent<RectTransform>().anchoredPosition = ArrowPosArr[ArrowNum].V2();
        SelectArrow.GetComponentInChildren<Text>().text = "▶                           ◀";

        Title.SetActive(false);
        Round.SetActive(false);
        RoundSelect.SetActive(false);
        Coin.SetActive(true);
        TeamSelectButton.SetActive(true);
        TeamSelectButton.GetComponentInChildren<Text>().text = "SPIN !!";
    }

    public IEnumerator TurnCoin()
    {
        TurnNum = Random.Range(7, 13);
        for (int i = 0; i < TurnNum; i++)
        {
            if (i % 2 == 0) Coin.GetComponent<Animator>().SetBool("InRed", true);
            else Coin.GetComponent<Animator>().SetBool("InRed", false);
            yield return new WaitForSeconds(0.2f);
        }
        if (TurnNum % 2 == 0)
        {
            TeamSelectButton.GetComponentInChildren<Text>().text = "RedTeam Start !!";
            Variables.FirstTeam = 0;
        }
        else
        {
            TeamSelectButton.GetComponentInChildren<Text>().text = "BlueTeam Start !!";
            Variables.FirstTeam = 1;
        }
    }

    // 게임방법버튼 - 게임방법화면
    public void HowToScreen()
    {
        switch(HowNum)
        {
            case 1:
                MainCamera.transform.position = new Vector3(-1600f, -1000f, -1f);
                BeforeButton.SetActive(false);
                HomeButton.SetActive(true);
                AfterButton.SetActive(true);

                SpeedRaw.SetActive(false);

                SpeedVideo.Stop();
                break;
            case 2:
                MainCamera.transform.position = new Vector3(2000f, 0f, -1f);
                BeforeButton.SetActive(true);

                SpeedRaw.SetActive(true);
                TorqueRaw.SetActive(false);

                SpeedVideo.Play();
                TorqueVideo.Stop();
                break;
            case 3:
                SpeedRaw.SetActive(false);
                TorqueRaw.SetActive(true);
                SweepingRaw.SetActive(false);

                SpeedVideo.Stop();
                SweepingVideo.Stop();
                TorqueVideo.Play();
                break;
            case 4:
                MainCamera.transform.position = new Vector3(2000f, 0, -1f);

                SweepingRaw.SetActive(true);
                TorqueRaw.SetActive(false);

                TorqueVideo.Stop();
                SweepingVideo.Play();
                break;
            case 5:
                MainCamera.transform.position = new Vector3(0, -1000f, -1f);

                SweepingRaw.SetActive(false);

                SweepingVideo.Stop();
                AfterButton.SetActive(true);
                break;
            case 6:
                MainCamera.transform.position = new Vector3(1600f, -1000f, -1f);
                AfterButton.SetActive(false);
                break;
        }
    }

    // 게임정보버튼 - 게임정보
    public void InfoScreen()
    {
        SelectArrow.SetActive(false);
        Title.SetActive(false);
        PressA.SetActive(false);
        StartButton.SetActive(false);
        HowToButton.SetActive(false);
        InfoButton.SetActive(false);
        HomeButton.SetActive(true);

        MainCamera.transform.position = new Vector3(0, -2000f, -1f);
    }
}
