using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public GameManager gameManager;
    public SpriteRenderer spriteRenderer;
    public bool isOpen = false; // Å¬¸¯À¸·Î ¿ÀÇÂÇÑ Å¸ÀÏÀÎ°¡?
    public bool isFlagged = false; // ±ê¹ß ²ÈÇû´Â°¡?
    public bool isWhat = false; // ¹Ì¾ÆÇÎ ²ÈÇû´Â°¡?
    public int tileType = 0; // 0~8: ¼ýÀÚ, 6036: ÆøÅº

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
