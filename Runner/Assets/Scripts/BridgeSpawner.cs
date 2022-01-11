using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeSpawner : MonoBehaviour
{
    public GameObject startReference, endReference; //Platformlarýn baþlangýç ve bitiþ noktalarýný tutacak
    public BoxCollider hiddenPlatform; //2 platform arasýndaki görünmeyen collider componentinin boyutunu tutacak

    void Start()
    {
        Vector3 direction = endReference.transform.position - startReference.transform.position; //2 nokta arasýndaki yön vektörünü elde etmiþ oluyoruz
        float distance = direction.magnitude; //2 nokta arasýndaki mesafe (magnitude = yön vektörünün aðýrlýðý oluyor, yön vektörünün aðýrlýðýda 2 nokta arasýndaki mesafeyi veriyor)
        direction = direction.normalized; //Ýþlemlerde kullanabilmek için birim vektöre dönüþtüruyoruz
        hiddenPlatform.transform.forward = direction; //2 referans noktasýnýn yönünün deðiþtiði zaman görünmez collider'ýn da yönünün deðiþmesi gerekiyor
        hiddenPlatform.size = new Vector3(hiddenPlatform.size.x, hiddenPlatform.size.y, distance); //Görünmez colliderr'ýn boyutlandýrmasý

        hiddenPlatform.transform.position = startReference.transform.position + (direction * distance / 2) + (new Vector3(0, -direction.z, direction.y) * hiddenPlatform.size.y / 2); //Görünmez collider'ýn konumlandýrýlmasý
    }

    //Update fonksiyonunu silmemizin sebebi; Bu sýnýfýn iþlemleri sadece oyun baþlayýnca 1 kere çalýþacak ve bitecek
}
