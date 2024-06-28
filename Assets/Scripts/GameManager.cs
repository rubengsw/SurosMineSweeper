using JetBrains.Annotations;
using UnityEditor;

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{

    //인게임 구현 관련 변수들
    //타일 구현 구조체
    public struct TileObject
    {
        public GameObject gameObject;
        public Tile tile;
    }
    //폭발 이펙트 관련 구조체
    [Serializable]
    public struct Explosives
    {
        public RuntimeAnimatorController animation;
        public float explosionTime;
    }

    public int xSize;//보드 가로사이즈
    public int ySize;//보드 세로사이즈
    public int mineCount;//폭탄 개수

    public bool Interactable;//마우스 인터랙션 온오프
    public bool firstClick;//첫클릭 감지
    public int StartSpaceSize;//초반안전지대 크기규정


    public int touchedX;//x 클릭 지점 검사
    public int touchedY;//y 클릭 지점 검사


    [SerializeField]
    public Sprite[] sprites; //순서대로 공백, 숫자(1~8), 폭탄, 맨타일, 누른타일, 깃발, 미아핑
    [SerializeField]
    public Explosives[] explosion;//폭발 애니메이션들

    public TileObject[,] TileBoard = new TileObject[50,50];//게임보드

    public GameObject verticalObject;//정렬판 메인
    public GameObject horizontalPrefab;//정렬판 가로
    public GameObject tilePrefab;//타일
    public GameObject explosionCircle;//연쇄폭발용 트리거
    public GameObject explosionPrefab;//폭발 오브젝트
    public int explosionSpeed;//연쇄폭발 속도

    GameObject pressedObj;//클릭한 타일오브젝트


    public InformationLoader infoLoader;//메인게임으로 정보 전송하는 오브젝트 로직


    //UI 관련 변수들
    public float timer;
    public bool pause;
    public string[] gameTime = new string[3];
    public TMP_Text timerText;//타이머 텍스트
    public GameObject getGray;//백그라운드 암전 오브젝트
    public GameObject menuButton;//퍼즈 버튼
    public GameObject uiBase;//ui 베이스
    public GameObject clearUi;//게임 클리어
    public GameObject gameOverUi;//게임오버
    public GameObject menuUi;//퍼즈 Ui
    public GameObject realExitUi;//나가시겠습니까? ui

    //클리어 및 게임오버
    public TMP_Text gameOverTimer;//게임오버시 타이머
    public TMP_Text gameOverBombFoundCount;//게임오버시 폭탄개수 카운트

    public TMP_Text clearTimer;//클리어시 타이머
    public TMP_Text clearBombFoundCount;//클리어시 폭탄개수 가운트



    void Start()
    {
        infoLoader = FindObjectOfType<InformationLoader>();

        xSize = infoLoader.xInput;
        ySize = infoLoader.yInput;
        mineCount = infoLoader.bombInput;
        StartSpaceSize = infoLoader.safeAreaInput;

        getGray.SetActive(false);
        uiBase.SetActive(false);
        clearUi.SetActive(false);
        gameOverUi.SetActive(false);
        menuUi.SetActive(false);
        menuButton.SetActive(true);
        realExitUi.SetActive(false);
        timer = 0;
        pause = true;
        Interactable = true;
        firstClick = true;
        explosionCircle.SetActive(false);

        //게임판 제작
        CreateBoard();



    }

    void Update()
    {

        //클릭 인터랙션들
        if (Interactable)
        {
            GameObject clickedObj = null;
            Tile currentTile;
            int selectedX = 0, selectedY = 0;
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, 0f);

            //레이캐스트
            if (hit.collider != null)
            {
                clickedObj = hit.transform.gameObject;
                for (int repeatY = 0; repeatY < ySize; repeatY++)
                {
                    for (int repeatX = 0; repeatX < xSize; repeatX++)
                    {
                        if (TileBoard[repeatY, repeatX].gameObject == clickedObj)//레이캐스트된 오브젝트를 배열에서 찾기 
                        {
                            selectedX = repeatX;
                            selectedY = repeatY;
                        }
                    }
                }
            }
            currentTile = TileBoard[selectedY, selectedX].tile;

            //만약 타일이 닫혀있으면
            if (!currentTile.isOpen)
            {

                //좌클릭 인터랙션 클릭시
                if (Input.GetMouseButtonDown(0))
                {
                    if (!currentTile.isFlagged)//깃발이 안꽂혀있으면...
                    {
                        currentTile.spriteRenderer.sprite = sprites[11];//타일을 누른모양으로 바꾸기
                        pressedObj = clickedObj;//다음 연산을 위해 클릭한 오브젝트 저장

                    }
                }
                //좌클릭 인터랙션 버튼 뗄 시 (좌우 동시 클릭도 같은 역할로 작용하도록 함)
                if (Input.GetMouseButtonUp(0) || (Input.GetMouseButton(0) && Input.GetMouseButton(1)))
                {
                    if (!currentTile.isFlagged)// 깃발이 안꽂혀있으면...
                    {
                        if (pressedObj == clickedObj)//아까 눌러서 모양 바꾼놈이랑 같은놈인지 확인
                        {
                            if (firstClick)//첫클릭이면..
                            {
                                MakeMap(selectedX, selectedY);//게임보드 구성
                                pause = false;//퍼즈 해제
                                firstClick = false;
                            }
                            OpenScanning(selectedX, selectedY);//같은놈이면 오픈 실행
                        }
                        else//아니면 아까 누른거 원래 모양으로 바꿔놓기
                        {
                            if (pressedObj.GetComponent<Tile>().isWhat)
                            {
                                pressedObj.GetComponent<Tile>().spriteRenderer.sprite = sprites[13];
                            }
                            else
                            {
                                pressedObj.GetComponent<Tile>().spriteRenderer.sprite = sprites[10];
                            }
                        }
                        pressedObj = null;//손뗐으니까 아까 뭐 눌렀는지 잊어먹기
                    }
                }

            }
            else
            {
                //좌우클릭 동시에 누를 시
                if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
                {
                    int scannedFlagCount = 0;
                    int startScanY = -1;
                    int stopScanY = 1;
                    if (selectedY == 0) startScanY++; //위쪽 끝에 붙어있으면 감지시작범위+
                    else if (selectedY == ySize - 1) stopScanY--; //아래쪽 끝에 붙어있으면 감지종료범위-
                    int startScanX = -1;
                    int stopScanX = 1;
                    if (selectedX == 0) startScanX++; //왼쪽 끝에 붙어있으면 감지시작범위+
                    else if (selectedX == xSize - 1) stopScanX--; //오른쪽 끝에 붙어있으면 감지종료범위-
                    for (int scanY = startScanY; scanY <= stopScanY; scanY++)
                    {
                        for (int scanX = startScanX; scanX <= stopScanX; scanX++)
                        {
                            if (TileBoard[selectedY + scanY, selectedX + scanX].tile.isFlagged)
                            {
                                scannedFlagCount++;
                            }
                        }
                    }
                    if (TileBoard[selectedY, selectedX].tile.tileType == scannedFlagCount)
                    {
                        for (int scanY = startScanY; scanY <= stopScanY; scanY++)
                        {
                            for (int scanX = startScanX; scanX <= stopScanX; scanX++)
                            {
                                if (!(TileBoard[selectedY + scanY, selectedX + scanX].tile.isOpen || TileBoard[selectedY + scanY, selectedX + scanX].tile.isFlagged))
                                {
                                    if (TileBoard[selectedY + scanY, selectedX + scanX].tile.tileType == 6036)
                                    {
                                        GameOver(TileBoard[selectedY + scanY, selectedX + scanX].gameObject);
                                    }
                                    else
                                    {
                                        OpenScanning(selectedX + scanX, selectedY + scanY);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //우클릭 인터랙션
            if (Input.GetMouseButtonDown(1))
            {
                if (!currentTile.isOpen)//만약 타일이 닫혀있으면...
                {
                    if (currentTile.isFlagged)//지금 깃발이면
                    {
                        currentTile.isFlagged = false;
                        currentTile.isWhat = true;
                        currentTile.spriteRenderer.sprite = sprites[13];//물음표로 바꾸기
                    }
                    else if (currentTile.isWhat)//물음표면
                    {
                        currentTile.isWhat = false;
                        currentTile.spriteRenderer.sprite = sprites[10];//기본으로 바꾸기
                    }
                    else if (!currentTile.isOpen)//오픈안된(기본)이면
                    {
                        currentTile.isFlagged = true;
                        currentTile.spriteRenderer.sprite = sprites[12];//깃발로 바꾸기
                    }
                }
                ClearScan();
            }
        }

        //타이머
        if (!pause)
        {
            timer += Time.deltaTime;
            gameTime[0] = ((int)timer / 3600).ToString();
            gameTime[1] = ((int)(timer / 60) % 60).ToString();
            gameTime[2] = ((int)timer % 60).ToString();

            if (gameTime[0] != "0")
            {
                timerText.text = gameTime[0] + ":"+gameTime[1] + ":" +gameTime[2];
            }
            else
            {
                timerText.text = gameTime[1] + ":" + gameTime[2];
            }
        }

    }

    //첫클릭시 지점으로부터 안전구역 설정 및 게임보드 실시간 제작 로직
    void MakeMap(int startX, int startY)
    {
        int TileMinimumX = startX - StartSpaceSize;
        int TileMaximumX = startX + StartSpaceSize;
        int TileMinimumY = startY - StartSpaceSize;
        int TileMaximumY = startY + StartSpaceSize;


        //폭탄 배치
        for (int repeat = 0; repeat < mineCount; repeat++)
        {
            int randomX = UnityEngine.Random.Range(0, xSize);
            int randomY = UnityEngine.Random.Range(0, ySize);


            if((TileMinimumX < randomX && randomX < TileMaximumX)&&(TileMinimumY < randomY && randomY < TileMaximumY)) //안전범위 내에 폭탄 소환시 다시 실행
            {
                repeat--;
            }
            else if (TileBoard[randomY, randomX].tile.tileType == 6036) //이미 위치에 폭탄이 있으면 다시 실행
            {
                repeat--;
            }
            else
            {
                TileBoard[randomY, randomX].tile.tileType = 6036;
            }
        }

        //숫자 배치
        for (int repeatY = 0; repeatY < ySize; repeatY++)
        {
            int startScanY = -1;
            int stopScanY = 1;
            if (repeatY == 0) startScanY++; //위쪽 끝에 붙어있으면 감지시작범위+
            else if (repeatY == ySize - 1) stopScanY--; //아래쪽 끝에 붙어있으면 감지종료범위-

            for (int repeatX = 0; repeatX < xSize; repeatX++)
            {

                if (TileBoard[repeatY, repeatX].tile.tileType == 6036)//만약 검사중인 타일이 폭탄일 경우...
                {
                    continue;//검사 넘기기
                }
                int startScanX = -1;
                int stopScanX = 1;
                if (repeatX == 0) startScanX++; //왼쪽 끝에 붙어있으면 감지시작범위+
                else if (repeatX == xSize - 1) stopScanX--; //오른쪽 끝에 붙어있으면 감지종료범위-


                for (int scanY = startScanY; scanY <= stopScanY; scanY++)
                {
                    for (int scanX = startScanX; scanX <= stopScanX; scanX++)
                    {
                        if (TileBoard[repeatY + scanY, repeatX + scanX].tile.tileType == 6036) //현재 감지중인 타일이 폭탄이면...
                        {
                            TileBoard[repeatY, repeatX].tile.tileType++; //숫자 +
                        }
                    }
                }


            }
        }
    }

    //클릭시 오픈되는 함수(재귀)
    void OpenScanning(int scanX, int scanY)
    {
        if((scanX == -1 || scanX == xSize)|| (scanY == -1 || scanY == ySize))
        {
            return;
        }

        Tile CurrentTile = TileBoard[scanY, scanX].tile;


        if (!(CurrentTile.isOpen || CurrentTile.isFlagged))
        {
            if (CurrentTile.tileType == 6036)//누른게 폭탄이면
            {
                GameOver(CurrentTile.gameObject);
            }
            else if (CurrentTile.tileType == 0)//누른게 공백이면
            {
                CurrentTile.isWhat = false;
                CurrentTile.isOpen = true;
                CurrentTile.spriteRenderer.sprite = sprites[CurrentTile.tileType];
                OpenScanning(scanX, scanY - 1);
                OpenScanning(scanX, scanY + 1);
                OpenScanning(scanX - 1, scanY);
                OpenScanning(scanX + 1, scanY);
                OpenScanning(scanX + 1, scanY - 1);
                OpenScanning(scanX + 1, scanY + 1);
                OpenScanning(scanX - 1, scanY - 1);
                OpenScanning(scanX - 1, scanY + 1);
            }
            else//누른게 숫자면
            {
                CurrentTile.isWhat = false;
                CurrentTile.isOpen = true;
                CurrentTile.spriteRenderer.sprite = sprites[CurrentTile.tileType];
            }
        }
        
    }

    //게임오버 코드
    void GameOver(GameObject ExplosedBomb)
    {
        Interactable = false;
        pause = true;
        float MinRadius = 0.001f;
        float MaxRadius = 20f;
        GameObject MainExplosion = Instantiate(explosionPrefab);
        MainExplosion.transform.position = ExplosedBomb.transform.position;
        MainExplosion.GetComponent<Animator>().runtimeAnimatorController = explosion[4].animation;
        Destroy(MainExplosion, explosion[4].explosionTime);
        explosionCircle.transform.position = ExplosedBomb.transform.position;
        explosionCircle.SetActive(true);
        StartCoroutine(ExplosionCoroutine(MinRadius, MaxRadius));
    }

    //연쇄폭발 코루틴
    public IEnumerator ExplosionCoroutine(float MinRad, float MaxRad)
    {
        explosionCircle.GetComponent<CircleCollider2D>().radius = MinRad;
        while (explosionCircle.GetComponent<CircleCollider2D>().radius < MaxRad)
        {
            explosionCircle.GetComponent<CircleCollider2D>().radius += explosionSpeed/10f;
            yield return new WaitForSeconds(0.1f);
        }
        explosionCircle.SetActive(false);
        yield return new WaitForSeconds(0.3f);
        //게임오버 ui 출력
        getGray.SetActive(true);
        uiBase.SetActive(true);
        clearUi.SetActive(false);
        gameOverUi.SetActive(true);
        menuUi.SetActive(false);
        menuButton.SetActive(false);
        if (gameTime[0] != "0")
        {
            gameOverTimer.text = "Time " + gameTime[0] + ":" + gameTime[1] + ":" + gameTime[2];
        }
        else
        {
            gameOverTimer.text = "Time " + gameTime[1] + ":" + gameTime[2];
        }
        gameOverBombFoundCount.text = "Founded Bomb "+flaggedBombCount().ToString();

    }
    
    //클리어 코드
    void ClearScan()
    {
        bool isClear = true;
        int flagBombCount = 0;
        for(int yScan = 0; yScan < ySize; yScan++)
        {
            for(int xScan = 0; xScan < xSize; xScan++)
            {
                if (TileBoard[yScan,xScan].tile.tileType == 6036 && !(TileBoard[yScan, xScan].tile.isFlagged))//모든 폭탄 위에 깃발이 있을 시
                {
                    isClear = false;
                    flagBombCount++;
                }
            }
        }
        Debug.Log(flagBombCount);
        if(isClear == true)//클리어 맞을시
        {
            Interactable = false;
            pause = true;
            getGray.SetActive(true);
            uiBase.SetActive(true);
            clearUi.SetActive(true);
            gameOverUi.SetActive(false);
            menuUi.SetActive(false);
            menuButton.SetActive(false);
            if (gameTime[0] != "0")
            {
                clearTimer.text = "Time " + gameTime[0] + ":" + gameTime[1] + ":" + gameTime[2];
            }
            else
            {
                clearTimer.text = "Time " + gameTime[1] + ":" + gameTime[2];
            }
            clearBombFoundCount.text = "Founded Bomb " + flaggedBombCount().ToString();
        }
    }

    public void CreateBoard()
    {
        float xMinSize;
        float yMinSize;
        float tileSize;
        xMinSize = verticalObject.GetComponent<RectTransform>().sizeDelta.x / xSize;
        yMinSize = verticalObject.GetComponent<RectTransform>().sizeDelta.y / ySize;
        tileSize = xMinSize < yMinSize ? xMinSize : yMinSize;

        for (int repeatY = 0; repeatY < ySize; repeatY++)
        {
            GameObject spawnedHorizontal = Instantiate(horizontalPrefab, verticalObject.transform); //열 오브젝트 제작 및 행 오브젝트에 투입
            spawnedHorizontal.GetComponent<RectTransform>().sizeDelta = new Vector2(17.45f,tileSize);
            spawnedHorizontal.name = "Horizontal_" + repeatY;
            for (int repeatX = 0; repeatX < xSize; repeatX++)
            {
                GameObject spawnedTile = Instantiate(tilePrefab, spawnedHorizontal.transform); //타일 오브젝트 제작 및 열 오브젝트에 투입
                spawnedTile.GetComponent<RectTransform>().localScale = new Vector2(tileSize, tileSize);
                spawnedTile.GetComponent<RectTransform>().sizeDelta = new Vector2(tileSize, tileSize);
                spawnedTile.name = "Tile_" + repeatY + ", " + repeatX;
                TileBoard[repeatY, repeatX].gameObject = spawnedTile;
                TileBoard[repeatY, repeatX].tile = spawnedTile.GetComponent<Tile>();
            }
        }
    }
    
    //깃발꽂힌 폭탄 개수 return
    int flaggedBombCount()
    {
        int FlaggedBomb = 0;
        for(int yScan = 0; yScan < ySize;yScan++)
        {
            for(int xScan = 0; xScan < xSize; xScan++)
            {
                if(TileBoard[yScan, xScan].tile.tileType == 6036 && (TileBoard[yScan, xScan].tile.isFlagged))
                {
                    FlaggedBomb++;
                }
            }
        }
        return FlaggedBomb;
    }

    public void menuButtonClick()
    {
        Interactable = false;
        pause = true;
        getGray.SetActive(true);
        uiBase.SetActive(true);
        clearUi.SetActive(false);
        gameOverUi.SetActive(false);
        menuUi.SetActive(true);
        menuButton.SetActive(false);
        realExitUi.SetActive(false);
    }

    public void Resum()
    {
        Interactable = true;
        pause = false;
        getGray.SetActive(false);
        uiBase.SetActive(false);
        clearUi.SetActive(false);
        gameOverUi.SetActive(false);
        menuUi.SetActive(false);
        menuButton.SetActive(true);
        realExitUi.SetActive(false);
    }

    public void ReturnToMenu()//나갈래요
    {
        uiBase.SetActive(false);
        realExitUi.SetActive(true);
        menuUi.SetActive(false);
    }
    public void NoReturn() //실수로 나가기 눌렀네요 ㅋㅋ
    {
        uiBase.SetActive(true);
        realExitUi.SetActive(false);
        menuUi.SetActive(true);
    }
    public void RealReturn()//메인메뉴로
    {
        SceneManager.LoadScene("MainMenu");
    }
}
