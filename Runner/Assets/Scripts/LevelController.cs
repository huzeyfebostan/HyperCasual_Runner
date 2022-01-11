using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    public static LevelController Current; //Diðer sýnýflarýn bu objeye eriþmesi için
    public bool gameActive = false; //levelin aktif olup olmadýðý söyler

    public GameObject startMenu, gameMenu, gameOverMenu, finishMenu; //menüleri tutar
    public Text scoreText, finishScoreText, currentLevelText, nextLevelText, startingMenuMoneyText, gameOverMenuMoneyText, finishGameMenuMoneyText; //Oyun ekranýndaki text metinlerini tutar
    public Slider levelProgressBar; //Karakterin oyun içindeki ilerlemesini tutar
    public float maxDistance; //Karakterin bitiþ çizgisine olan uzaklýðýný tutar
    public GameObject finishLine; //Bitiþ çizgisini tutar
    public AudioSource gameMusicAudioSource;
    public AudioClip victoryAudioClip, gameOverAudioClip; //Kazanma ve kaybetme sesleri
    public DailyReward dailyReward;

    int currentLevel; //Güncel leveli tutar
    int score; //Skoru tutar

    void Start()
    {
        Current = this; //Curent'i levelconrollerin kendisine eþitlenir
        currentLevel = PlayerPrefs.GetInt("currentLevel"); //Oyuncunun kaçýncý levelde kaldýðýný tutar
        PlayerController.Current = GameObject.FindObjectOfType<PlayerController>();
        GameObject.FindObjectOfType<MarketController>().InitializeMarketController();
        dailyReward.InitializeDailyReward();
        currentLevelText.text = (currentLevel + 1).ToString(); //Oyun ekranýndaki leveli textini karakterin leveliyle deðiþtirir.
        nextLevelText.text = (currentLevel + 2).ToString(); //Sonraki bölüm textini bulunduðu level textine göre ayarlar
        UpdateMoneyTexts();
        gameMusicAudioSource = Camera.main.GetComponent<AudioSource>(); //Ana kameaya ulaþýp kamera üzerindeki müziði çalmaya baþlar
    }

    void Update()
    {
        if (gameActive)
        {
            PlayerController player = PlayerController.Current;
            float distance = finishLine.transform.position.z - PlayerController.Current.transform.position.z;
            levelProgressBar.value = 1 - (distance / maxDistance);
        }

    }

    public void StartLevel()
    {
        maxDistance = finishLine.transform.position.z - PlayerController.Current.transform.position.z; //Karakte ile bitiþ çizgisi arasýndaki mesafeyi bulur

        PlayerController.Current.ChangeSpeed(PlayerController.Current.runningSpeed); //playercontorllerin þu anki objesine eriþ ve hýzýný ayný objenin maksimum hýzý kadar arttýr
        startMenu.SetActive(false);
        gameMenu.SetActive(true);
        PlayerController.Current.animator.SetBool("running", true); //Ekrana basýldýðýnda karakter koþmaya baþlar.
        gameActive = true;
    }

    public void RestartLevel()
    {
        LevelLoader.Current.ChangeLevel(SceneManager.GetActiveScene().name);
    }

    public void LoadNextLevet()
    {
        LevelLoader.Current.ChangeLevel("Level " + (currentLevel + 1));
    }

    public void GameOver()
    {
        UpdateMoneyTexts();
        gameMusicAudioSource.Stop();
        gameMusicAudioSource.PlayOneShot(gameOverAudioClip);
        gameMenu.SetActive(false);
        gameOverMenu.SetActive(true);
        gameActive = false;
    }

    public void FinishGame()
    {
        GiveMoneyToPlayer(score);
        gameMusicAudioSource.Stop();
        gameMusicAudioSource.PlayOneShot(victoryAudioClip);
        PlayerPrefs.SetInt("currentLevel", currentLevel + 1);
        finishScoreText.text = score.ToString();
        gameMenu.SetActive(false);
        finishMenu.SetActive(true);
        gameActive = false;
    }

    public void ChangeScore(int increment)
    {
        score += increment;
        scoreText.text = score.ToString();
    }

    public void UpdateMoneyTexts() //Oyuncunun parasýný günceller
    {
        int money = PlayerPrefs.GetInt("money");
        startingMenuMoneyText.text = money.ToString();
        gameOverMenuMoneyText.text = money.ToString();
        finishGameMenuMoneyText.text = money.ToString();
    }

    public void GiveMoneyToPlayer(int increment)
    {
        int money = PlayerPrefs.GetInt("money");
        money = Mathf.Max(0, money + increment); //Money + increment toplamý sýfýrkan küçük bir deðer olursa para 0 olacak büyük bir deðer olursa diek money döndürecek
        PlayerPrefs.SetInt("money", money);
        UpdateMoneyTexts();
    }
}
