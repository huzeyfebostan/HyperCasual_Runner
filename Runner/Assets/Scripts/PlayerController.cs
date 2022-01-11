using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Current;

    public float limitX; //Saða sola hareket

    public float runningSpeed; //Karakterin maks hýzýný tutacak,
    public float xSpeed; //Public yaptýk çünkü editörden hýzlýca ayarlayabilmek için
    private float _currentRunningSpeed; //Karakterimizin editörden gelen mevcut koþma hýzýný tutacak

    public GameObject ridingCylinderPrefab; //Silindir prefabýný tutacak
    public List<RidingCylinder> cylinders; //Karakterin ayaðýndaki silindirleri tutacak

    private bool _spawningBridge; //True ise köprü oluþturuyor olacak false ise oluþturmuyor
    public GameObject bridgePiecePrefab; //Köprü parçalarýný tutacak
    private BridgeSpawner _bridgeSpawner; //Kopru olustur sýnýfýna eriþiyoruz
    private float _creatingBridgeTimer; //Nesneleri olusturmak için beklenilmesi gereken zaman

    private bool _finished; //Karakterin bitiþ çizgisine gelip gelmediðini tutacal

    private float _scoreTimer = 0; //Bitiþ çizgisinden sonra skor kazanacaðý süreyi tutar

    public Animator animator;

    private float _lastTouchedX;
    private float _dropSoundTimer;

    public AudioSource cylinderAudioSource, triggerAudioSource, itemAudioSource; //Silindir sesi, bir nesneye temas ettiðinde çýkacak ses, item seslerini tutacak kaynaklar
    public AudioClip gatherAudioClip, dropAudioClip, coinAudioClip, buyAudioClip, equipItemAudioClip, unequipItemAudioClip; //Silindir toplama sesi, silindirin hacmi küçülürken çýkacak  ses, altýnlarý topladýðýnda çýkacak ses, item satýn aldýðýnda çýkacak ses, eþya giyme ve çýkarma sesleri

    public List<GameObject> wearSpots;

    void Update()
    {
        if (LevelController.Current == null || !LevelController.Current.gameActive)
        {
            return;
        }

        float newX = 0; //Karakterin x eksenindeki yeni konumunu tutacak
        float touchXDelta = 0; //Kulanýcýnýn parmaðýný ya da fareyi ne kadar saða sola kaydýrdýðýný tutacak

        if (Input.touchCount > 0) //Kullanýcýnýn telefon ekranýna dokunduðunun kontrolunu yapýyor
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                _lastTouchedX = Input.GetTouch(0).position.x;
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                touchXDelta = (Input.GetTouch(0).position.x - _lastTouchedX) / Screen.width;
                _lastTouchedX = Input.GetTouch(0).position.x;
            }
            touchXDelta = Input.GetTouch(0).deltaPosition.x / Screen.width;
        } else if (Input.GetMouseButton(0)) //Farede tuþa basýlýyor mu diye kontrol ediyor
        {
            touchXDelta = Input.GetAxis("Mouse X"); //Farenin x düzleminde ne kadar hareket ettiðini dokunXDelta ya atýyoruz
        }

        newX = transform.position.x + xSpeed * touchXDelta * Time.deltaTime; //Karakterin x düzleminde hareketini tutacak
        newX = Mathf.Clamp(newX, -limitX, limitX); //Yeni pozisyonu sað ve sol limitlerde sýnýrlandýrmak için Mathf.Clamp 

        Vector3 newPosition = new Vector3(newX, transform.position.y, transform.position.z + _currentRunningSpeed * Time.deltaTime); //Karakterin bir sonraki pozisyonunu tutacak 
        transform.position = newPosition; //Karakterin ilerlemesini saðlar

        if (_spawningBridge) //True ise köprüyü oluþturmaya baþlar
        {
            PlayDropSound();
            _creatingBridgeTimer -= Time.deltaTime;
            if (_creatingBridgeTimer < 0)
            {
                _creatingBridgeTimer = 0.01f;
                IncrementCylinderVolume(-0.01f); //Silindirlerin hacmini küçültüyoruz

                GameObject createdBridgePiece = Instantiate(bridgePiecePrefab, this.transform); //Yeni köprü parçasý oluþturur
                createdBridgePiece.transform.SetParent(null);
                Vector3 direction = _bridgeSpawner.endReference.transform.position - _bridgeSpawner.startReference.transform.position; //2 nokta arasýndaki yön vektörünü elde etmiþ oluyoruz
                float distance = direction.magnitude; //2 nokta arasýndaki mesafe (magnitude = yön vektörünün aðýrlýðý oluyor, yön vektörünün aðýrlýðýda 2 nokta arasýndaki mesafeyi veriyor)

                direction = direction.normalized; //Ýþlemlerde kullanabilmek için birim vektöre dönüþtüruyoruz
                createdBridgePiece.transform.forward = direction;


                float characterDistance = transform.position.z - _bridgeSpawner.startReference.transform.position.z; //Karakterimiz baþlangýçdan ne kadar uzakda
                characterDistance = Mathf.Clamp(characterDistance, 0, distance); //0 ve maksimum uzaklýk arasýnda sýnýrlandýrýyoruz

                Vector3 newPiecePosition = _bridgeSpawner.startReference.transform.position + direction * characterDistance; //Oluþturulan objenin konumunu tutar ve karakterimizle ayný yönde ilerler
                newPiecePosition.x = transform.position.x; //Karakterimiz saða sola ne kadar gittiyse oluþturulan parça da o kadar saða sola gitsin
                createdBridgePiece.transform.position = newPiecePosition; //Olulturulan parçanýn pozisyonunu yeni vektöre eþitliyoruz

                if (_finished)
                {
                    _scoreTimer -= Time.deltaTime;
                    if (_scoreTimer < 0)
                    {
                        _scoreTimer = 0.3f;
                        LevelController.Current.ChangeScore(1);
                    }
                }
            }
        }
    }

    public void ChangeSpeed(float value) //levelcontrollerin playercontrollerin hýzýný deðiþtirir
    {
        _currentRunningSpeed = value;
    }
    private void OnTriggerEnter(Collider other) //Karakterimiz IsTrigger seçeneði iþaretli olan bir collider ile çarpýþtýðý zaman bu fonksiyonumuz çalýþacak
    {
        if (other.tag == "AddCylinder") //Eðer çarðýþtýðýmýz objenin etiketi AddSilinidr ile belli miktar silindirleri büyüt ve çarpýþtýðýmýz objeyi yok et
        {
            cylinderAudioSource.PlayOneShot(gatherAudioClip, 0.1f);
            IncrementCylinderVolume(0.1f);
            Destroy(other.gameObject);
        } 
        else if (other.tag == "SpawnBridge") //Karakterin çarptýðý collider KopruOlusturucu ise BaslaKopru fonksiyonunu  çalýþtýrýr
        {
            StartSpawningBridge(other.transform.parent.GetComponent<BridgeSpawner>());
        }
        else if (other.tag == "StopSpawnBridge")
        {
            StopSpawningBridge();
            if (_finished)
            {
                LevelController.Current.FinishGame();
            }
        }
        else if (other.tag == "Finish")
        {
            _finished = true;
            StartSpawningBridge(other.transform.parent.GetComponent<BridgeSpawner>());
        }
        else if (other.tag == "Coin")
        {
            triggerAudioSource.PlayOneShot(coinAudioClip, 0.1f);
            other.tag = "Untagged";
            LevelController.Current.ChangeScore(10);
            Destroy(other.gameObject);
        }

    }

    private void OnTriggerStay(Collider other) //Karakteimiz IsTrigger seçeneði açýk olan bi collider'ýn üstünde gittiði süre boyunca bu fonksiyon çalýþacak
    {
        if (LevelController.Current.gameActive)
        {
            if (other.tag == "Trap")
            {
                PlayDropSound();
                IncrementCylinderVolume(-Time.fixedDeltaTime);
            }
        }
    }

    public void IncrementCylinderVolume(float value) //Karakterimizin altýnda silindir yoksa silindir oluþturacak, silindiri büyütecek, silindir yeteri kadar büyüdüyse yeni silinidr oluþturacak
    {
        if (cylinders.Count == 0) //Karakterin ayaðýnýn altýnda silindir yoksa
        {
            if (value > 0)
            {
                CreateCylinder(value); //Karakterin altýnda silindir oluþturacak
            }
            else
            {
                if (_finished) //Eðer karakter bitiþ çizgisine ulaþtýysa diðer levele geç
                {
                    LevelController.Current.FinishGame();
                }
                else
                {
                    Die();
                }
            }
        }
        else //En alttaki silindirin boyutunu günceller
        {
            cylinders[cylinders.Count - 1].IncrementCylinderVolume(value);
        }

    }

    public void Die()
    {
        animator.SetBool("dead", true); //Karakter öldüðünde ölme animasyonu çalýþýr
        gameObject.layer = 6; //Karakter öldüðünde layer'ý 6.layer'a eþitlenir(6.layer CharacterDead)
        Camera.main.transform.SetParent(null);
        LevelController.Current.GameOver();
    }

    public void CreateCylinder(float value)  //Silindir Oluþturur
    {
        RidingCylinder createdCylinder = Instantiate(ridingCylinderPrefab, transform).GetComponent<RidingCylinder>();
        cylinders.Add(createdCylinder);
        createdCylinder.IncrementCylinderVolume(value);
    }

    public void DestroyCylinder(RidingCylinder cylinder) //Oluþturulan silindiri yok eder
    {
        cylinders.Remove(cylinder);
        Destroy(cylinder.gameObject);
    }

    public void StartSpawningBridge(BridgeSpawner spawner) //Köprü oluþturmaya baþlar
    {
        _bridgeSpawner = spawner;
        _spawningBridge = true;
    }

    public void StopSpawningBridge() //Köprü oluþturmayý bitirir
    {
        _spawningBridge = false;
    }

    public void PlayDropSound()
    {
        _dropSoundTimer -= Time.deltaTime;
        if (_dropSoundTimer < 0)
        {
            _dropSoundTimer = 0.15f;
            cylinderAudioSource.PlayOneShot(dropAudioClip, 0.1f);
        }
    }
}
