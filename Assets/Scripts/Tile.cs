using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public GameManager gameManager;
    public SpriteRenderer spriteRenderer;
    public bool isOpen = false; // Ŭ������ ������ Ÿ���ΰ�?
    public bool isFlagged = false; // ��� �����°�?
    public bool isWhat = false; // �̾��� �����°�?
    public int tileType = 0; // 0~8: ����, 6036: ��ź

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameManager = FindObjectOfType<GameManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("ShockWave"))
        {
            isOpen = true;
            if(tileType == 6036 && !isFlagged)
            {
                spriteRenderer.sprite = gameManager.sprites[9];
                GameObject thisExplosion = Instantiate(gameManager.explosionPrefab);
                thisExplosion.transform.position = this.transform.position;
                int randomExplosion = Random.Range(0, 4);
                thisExplosion.GetComponent<Animator>().runtimeAnimatorController = gameManager.explosion[randomExplosion].animation;
                Destroy(thisExplosion, gameManager.explosion[randomExplosion].explosionTime);
            }
            else if(!isFlagged)
            {
                spriteRenderer.sprite = gameManager.sprites[tileType];
            }
        }
    }
}
