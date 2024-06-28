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

    //�ΰ��� ���� ���� ������
    //Ÿ�� ���� ����ü
    public struct TileObject
    {
        public GameObject gameObject;
        public Tile tile;
    }
    //���� ����Ʈ ���� ����ü
    [Serializable]
    public struct Explosives
    {
        public RuntimeAnimatorController animation;
        public float explosionTime;
    }

    public int xSize;//���� ���λ�����
    public int ySize;//���� ���λ�����
    public int mineCount;//��ź ����

    public bool Interactable;//���콺 ���ͷ��� �¿���
    public bool firstClick;//ùŬ�� ����
    public int StartSpaceSize;//�ʹݾ������� ũ�����


    public int touchedX;//x Ŭ�� ���� �˻�
    public int touchedY;//y Ŭ�� ���� �˻�


    [SerializeField]
    public Sprite[] sprites; //������� ����, ����(1~8), ��ź, ��Ÿ��, ����Ÿ��, ���, �̾���
    [SerializeField]
    public Explosives[] explosion;//���� �ִϸ��̼ǵ�

    public TileObject[,] TileBoard = new TileObject[50,50];//���Ӻ���

    public GameObject verticalObject;//������ ����
    public GameObject horizontalPrefab;//������ ����
    public GameObject tilePrefab;//Ÿ��
    public GameObject explosionCircle;//�������߿� Ʈ����
    public GameObject explosionPrefab;//���� ������Ʈ
    public int explosionSpeed;//�������� �ӵ�

    GameObject pressedObj;//Ŭ���� Ÿ�Ͽ�����Ʈ


    public InformationLoader infoLoader;//���ΰ������� ���� �����ϴ� ������Ʈ ����


    //UI ���� ������
    public float timer;
    public bool pause;
    public string[] gameTime = new string[3];
    public TMP_Text timerText;//Ÿ�̸� �ؽ�Ʈ
    public GameObject getGray;//��׶��� ���� ������Ʈ
    public GameObject menuButton;//���� ��ư
    public GameObject uiBase;//ui ���̽�
    public GameObject clearUi;//���� Ŭ����
    public GameObject gameOverUi;//���ӿ���
    public GameObject menuUi;//���� Ui
    public GameObject realExitUi;//�����ðڽ��ϱ�? ui

    //Ŭ���� �� ���ӿ���
    public TMP_Text gameOverTimer;//���ӿ����� Ÿ�̸�
    public TMP_Text gameOverBombFoundCount;//���ӿ����� ��ź���� ī��Ʈ

    public TMP_Text clearTimer;//Ŭ����� Ÿ�̸�
    public TMP_Text clearBombFoundCount;//Ŭ����� ��ź���� ����Ʈ



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

        //������ ����
        CreateBoard();



    }

    void Update()
    {

        //Ŭ�� ���ͷ��ǵ�
        if (Interactable)
        {
            GameObject clickedObj = null;
            Tile currentTile;
            int selectedX = 0, selectedY = 0;
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, 0f);

            //����ĳ��Ʈ
            if (hit.collider != null)
            {
                clickedObj = hit.transform.gameObject;
                for (int repeatY = 0; repeatY < ySize; repeatY++)
                {
                    for (int repeatX = 0; repeatX < xSize; repeatX++)
                    {
                        if (TileBoard[repeatY, repeatX].gameObject == clickedObj)//����ĳ��Ʈ�� ������Ʈ�� �迭���� ã�� 
                        {
                            selectedX = repeatX;
                            selectedY = repeatY;
                        }
                    }
                }
            }
            currentTile = TileBoard[selectedY, selectedX].tile;

            //���� Ÿ���� ����������
            if (!currentTile.isOpen)
            {

                //��Ŭ�� ���ͷ��� Ŭ����
                if (Input.GetMouseButtonDown(0))
                {
                    if (!currentTile.isFlagged)//����� �Ȳ���������...
                    {
                        currentTile.spriteRenderer.sprite = sprites[11];//Ÿ���� ����������� �ٲٱ�
                        pressedObj = clickedObj;//���� ������ ���� Ŭ���� ������Ʈ ����

                    }
                }
                //��Ŭ�� ���ͷ��� ��ư �� �� (�¿� ���� Ŭ���� ���� ���ҷ� �ۿ��ϵ��� ��)
                if (Input.GetMouseButtonUp(0) || (Input.GetMouseButton(0) && Input.GetMouseButton(1)))
                {
                    if (!currentTile.isFlagged)// ����� �Ȳ���������...
                    {
                        if (pressedObj == clickedObj)//�Ʊ� ������ ��� �ٲ۳��̶� ���������� Ȯ��
                        {
                            if (firstClick)//ùŬ���̸�..
                            {
                                MakeMap(selectedX, selectedY);//���Ӻ��� ����
                                pause = false;//���� ����
                                firstClick = false;
                            }
                            OpenScanning(selectedX, selectedY);//�������̸� ���� ����
                        }
                        else//�ƴϸ� �Ʊ� ������ ���� ������� �ٲ����
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
                        pressedObj = null;//�ն����ϱ� �Ʊ� �� �������� �ؾ�Ա�
                    }
                }

            }
            else
            {
                //�¿�Ŭ�� ���ÿ� ���� ��
                if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
                {
                    int scannedFlagCount = 0;
                    int startScanY = -1;
                    int stopScanY = 1;
                    if (selectedY == 0) startScanY++; //���� ���� �پ������� �������۹���+
                    else if (selectedY == ySize - 1) stopScanY--; //�Ʒ��� ���� �پ������� �����������-
                    int startScanX = -1;
                    int stopScanX = 1;
                    if (selectedX == 0) startScanX++; //���� ���� �پ������� �������۹���+
                    else if (selectedX == xSize - 1) stopScanX--; //������ ���� �پ������� �����������-
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

            //��Ŭ�� ���ͷ���
            if (Input.GetMouseButtonDown(1))
            {
                if (!currentTile.isOpen)//���� Ÿ���� ����������...
                {
                    if (currentTile.isFlagged)//���� ����̸�
                    {
                        currentTile.isFlagged = false;
                        currentTile.isWhat = true;
                        currentTile.spriteRenderer.sprite = sprites[13];//����ǥ�� �ٲٱ�
                    }
                    else if (currentTile.isWhat)//����ǥ��
                    {
                        currentTile.isWhat = false;
                        currentTile.spriteRenderer.sprite = sprites[10];//�⺻���� �ٲٱ�
                    }
                    else if (!currentTile.isOpen)//���¾ȵ�(�⺻)�̸�
                    {
                        currentTile.isFlagged = true;
                        currentTile.spriteRenderer.sprite = sprites[12];//��߷� �ٲٱ�
                    }
                }
                ClearScan();
            }
        }

        //Ÿ�̸�
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

    //ùŬ���� �������κ��� �������� ���� �� ���Ӻ��� �ǽð� ���� ����
    void MakeMap(int startX, int startY)
    {
        int TileMinimumX = startX - StartSpaceSize;
        int TileMaximumX = startX + StartSpaceSize;
        int TileMinimumY = startY - StartSpaceSize;
        int TileMaximumY = startY + StartSpaceSize;


        //��ź ��ġ
        for (int repeat = 0; repeat < mineCount; repeat++)
        {
            int randomX = UnityEngine.Random.Range(0, xSize);
            int randomY = UnityEngine.Random.Range(0, ySize);


            if((TileMinimumX < randomX && randomX < TileMaximumX)&&(TileMinimumY < randomY && randomY < TileMaximumY)) //�������� ���� ��ź ��ȯ�� �ٽ� ����
            {
                repeat--;
            }
            else if (TileBoard[randomY, randomX].tile.tileType == 6036) //�̹� ��ġ�� ��ź�� ������ �ٽ� ����
            {
                repeat--;
            }
            else
            {
                TileBoard[randomY, randomX].tile.tileType = 6036;
            }
        }

        //���� ��ġ
        for (int repeatY = 0; repeatY < ySize; repeatY++)
        {
            int startScanY = -1;
            int stopScanY = 1;
            if (repeatY == 0) startScanY++; //���� ���� �پ������� �������۹���+
            else if (repeatY == ySize - 1) stopScanY--; //�Ʒ��� ���� �پ������� �����������-

            for (int repeatX = 0; repeatX < xSize; repeatX++)
            {

                if (TileBoard[repeatY, repeatX].tile.tileType == 6036)//���� �˻����� Ÿ���� ��ź�� ���...
                {
                    continue;//�˻� �ѱ��
                }
                int startScanX = -1;
                int stopScanX = 1;
                if (repeatX == 0) startScanX++; //���� ���� �پ������� �������۹���+
                else if (repeatX == xSize - 1) stopScanX--; //������ ���� �پ������� �����������-


                for (int scanY = startScanY; scanY <= stopScanY; scanY++)
                {
                    for (int scanX = startScanX; scanX <= stopScanX; scanX++)
                    {
                        if (TileBoard[repeatY + scanY, repeatX + scanX].tile.tileType == 6036) //���� �������� Ÿ���� ��ź�̸�...
                        {
                            TileBoard[repeatY, repeatX].tile.tileType++; //���� +
                        }
                    }
                }


            }
        }
    }

    //Ŭ���� ���µǴ� �Լ�(���)
    void OpenScanning(int scanX, int scanY)
    {
        if((scanX == -1 || scanX == xSize)|| (scanY == -1 || scanY == ySize))
        {
            return;
        }

        Tile CurrentTile = TileBoard[scanY, scanX].tile;


        if (!(CurrentTile.isOpen || CurrentTile.isFlagged))
        {
            if (CurrentTile.tileType == 6036)//������ ��ź�̸�
            {
                GameOver(CurrentTile.gameObject);
            }
            else if (CurrentTile.tileType == 0)//������ �����̸�
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
            else//������ ���ڸ�
            {
                CurrentTile.isWhat = false;
                CurrentTile.isOpen = true;
                CurrentTile.spriteRenderer.sprite = sprites[CurrentTile.tileType];
            }
        }
        
    }

    //���ӿ��� �ڵ�
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

    //�������� �ڷ�ƾ
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
        //���ӿ��� ui ���
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
    
    //Ŭ���� �ڵ�
    void ClearScan()
    {
        bool isClear = true;
        int flagBombCount = 0;
        for(int yScan = 0; yScan < ySize; yScan++)
        {
            for(int xScan = 0; xScan < xSize; xScan++)
            {
                if (TileBoard[yScan,xScan].tile.tileType == 6036 && !(TileBoard[yScan, xScan].tile.isFlagged))//��� ��ź ���� ����� ���� ��
                {
                    isClear = false;
                    flagBombCount++;
                }
            }
        }
        Debug.Log(flagBombCount);
        if(isClear == true)//Ŭ���� ������
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
            GameObject spawnedHorizontal = Instantiate(horizontalPrefab, verticalObject.transform); //�� ������Ʈ ���� �� �� ������Ʈ�� ����
            spawnedHorizontal.GetComponent<RectTransform>().sizeDelta = new Vector2(17.45f,tileSize);
            spawnedHorizontal.name = "Horizontal_" + repeatY;
            for (int repeatX = 0; repeatX < xSize; repeatX++)
            {
                GameObject spawnedTile = Instantiate(tilePrefab, spawnedHorizontal.transform); //Ÿ�� ������Ʈ ���� �� �� ������Ʈ�� ����
                spawnedTile.GetComponent<RectTransform>().localScale = new Vector2(tileSize, tileSize);
                spawnedTile.GetComponent<RectTransform>().sizeDelta = new Vector2(tileSize, tileSize);
                spawnedTile.name = "Tile_" + repeatY + ", " + repeatX;
                TileBoard[repeatY, repeatX].gameObject = spawnedTile;
                TileBoard[repeatY, repeatX].tile = spawnedTile.GetComponent<Tile>();
            }
        }
    }
    
    //��߲��� ��ź ���� return
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

    public void ReturnToMenu()//��������
    {
        uiBase.SetActive(false);
        realExitUi.SetActive(true);
        menuUi.SetActive(false);
    }
    public void NoReturn() //�Ǽ��� ������ �����׿� ����
    {
        uiBase.SetActive(true);
        realExitUi.SetActive(false);
        menuUi.SetActive(true);
    }
    public void RealReturn()//���θ޴���
    {
        SceneManager.LoadScene("MainMenu");
    }
}
