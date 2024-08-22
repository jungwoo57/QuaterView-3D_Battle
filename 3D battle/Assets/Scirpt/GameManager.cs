using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public GameObject menuCamera;
    public GameObject gameCamera;
    public GameObject itemShop;
    public GameObject weaponShop;
    public GameObject startZone;
    

    public Player player;
    public Boss boss;
    public int stage;
    public float playTime;
    public bool isBattle;
    public int enemyCntA;
    public int enemyCntB;
    public int enemyCntC;
    public int enemyCntD;

    public Transform[] enemySpawn;
    public GameObject[] enemies;
    public List<int> enemyList;

    public GameObject menuPanel;
    public GameObject gamePanel;
    public GameObject gameOverPanel;

    public Text maxScoreText;
    public Text scoreText;
    public Text stageText;
    public Text playTimeText;
    public Text playerHealthText;
    public Text playerCoinText;
    public Image weaponImg1;
    public Image weaponImg2;
    public Image weaponImg3;
    public Text enemyAtext;
    public Text enemyBtext;
    public Text enemyCtext;
    public RectTransform bossHealthGroup;
    public RectTransform bossHealthbar;
    public Text curScoreText;
    public Text bestScoreText;

    private void Awake()
    {
        enemyList =  new List<int>();
        maxScoreText.text = string.Format("{0:n0}", PlayerPrefs.GetInt("MaxScore"));
    }

    public void GameStart() 
    {
        menuCamera.SetActive(false);
        gameCamera.SetActive(true);
        menuPanel.SetActive(false);
        gamePanel.SetActive(true);

        player.gameObject.SetActive(true);
    }

    public void GameOver() 
    {
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(true);
        curScoreText.text = scoreText.text;

        int maxScore = PlayerPrefs.GetInt("MaxScore");
        if (player.score > maxScore)
        {
            bestScoreText.gameObject.SetActive(true);
            PlayerPrefs.SetInt("MaxScore", player.score);
        }
    }

    public void ReStart() 
    {
        SceneManager.LoadScene(0);
    }

    public void StageStart() 
    {
        itemShop.SetActive(false);
        weaponShop.SetActive(false);
        startZone.SetActive(false);

        foreach (Transform zone in enemySpawn)
        {
            zone.gameObject.SetActive(true);
        }
        isBattle = true;
        StartCoroutine(InBattle());
    }
    public void StageEnd() 
    {
        player.transform.position = new Vector3(30.6f,2.37f,-0.95f);

        foreach (Transform zone in enemySpawn)
        {
            zone.gameObject.SetActive(false);
        }
        itemShop.SetActive(true);
        weaponShop.SetActive(true);
        startZone.SetActive(true);
        isBattle = false;
        stage++;
    }

    IEnumerator InBattle()
    {
        if (stage % 5 == 0)
        {
            enemyCntD++;
            GameObject instantEnemy = Instantiate(enemies[3], enemySpawn[0].position,
               enemySpawn[0].rotation);
            Enemy enemy = instantEnemy.GetComponent<Enemy>();
            enemy.target = player.transform;
            enemy.manager = this;
            boss = instantEnemy.GetComponent<Boss>();
        }
        else
        {
            for (int index = 0; index < stage; index++)
            {
                int ran = Random.Range(0, 3);
                enemyList.Add(ran);

                switch (ran)
                {
                    case 0:
                        enemyCntA++;
                        break;
                    case 1:
                        enemyCntB++;
                        break;
                    case 2:
                        enemyCntC++;
                        break;
                }
            }

            while (enemyList.Count > 0)
            {
                int ranZone = Random.Range(0, 3);
                GameObject instantEnemy = Instantiate(enemies[enemyList[0]], enemySpawn[ranZone].position,
                    enemySpawn[ranZone].rotation);
                Enemy enemy = instantEnemy.GetComponent<Enemy>();
                enemy.target = player.transform;
                enemy.manager = this;
                enemyList.RemoveAt(0);
                yield return new WaitForSeconds(3f);
            }
        }

        while (enemyCntA + enemyCntB+ enemyCntC + enemyCntD > 0) 
        {
            yield return null;
        }
        yield return new WaitForSeconds(3);
        StageEnd();
    }
    private void Update()
    {
        if (isBattle)
            playTime += Time.deltaTime;
    }

    private void LateUpdate()
    {
        scoreText.text = string.Format("{0:n0}", player.score);
        stageText.text = "Stage" + stage;


        int hour = (int)(playTime / 3600);
        int min = (int)((playTime - hour * 3600) / 60);
        int second = (int)(playTime % 60);
        playTimeText.text = string.Format("{0:00}", hour) + " : " + string.Format("{0:00}", min) + " : " 
                            + string.Format("{0:00}", second);


        playerHealthText.text = player.HP + " / " + player.maxHP;
        playerCoinText.text = string.Format("{0:n0}", player.coin);

        weaponImg1.color = new Color(1, 1, 1, player.hasWeapon[0] ? 1 : 0);
        weaponImg2.color = new Color(1, 1, 1, player.hasWeapon[1] ? 1 : 0);
        

        enemyAtext.text = enemyCntA.ToString();

        if(boss !=null)
            bossHealthbar.localScale = new Vector3(boss.curHealth / boss.maxHealth, 1, 1);
    }
}
