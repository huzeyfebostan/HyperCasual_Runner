using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RidingCylinder : MonoBehaviour
{
    private bool _filled; //Silindirin tam olarak dolup dolmadýðýný tutacak
    private float _value; //Silindirin sayýsal olarak ne kadar dolduðunu tutacak

    public void IncrementCylinderVolume(float value) //Silindirin boyutunu arttýracak yada küçültecek
    {
        _value += value; //Aldýðýmýz boyutu silindirin boyutuna ekliyor

        if (_value > 1) //Silindiirin boyutunu tam olarak 1 yap ve 1'den ne karad büyükse o büyüklükte yeni bir silindir oluþtur
        {
            float leftValue = _value - 1; //1'den kalan deðer
            int cylinderCount = PlayerController.Current.cylinders.Count;
            transform.localPosition = new Vector3(transform.localPosition.x, -0.5f  * (cylinderCount - 1) - 0.25f , transform.localPosition.z); //(silindirSayýsý - 1) * -0.5 + büyüklükDeðeri * -0.25
            transform.localScale = new Vector3(0.5f, transform.localScale.y, 0.5f);
            PlayerController.Current.CreateCylinder(leftValue);
        }
        else if (_value < 0) //Karkterimize bu silindiri yok etmesini söyleyeceðiz
        {
            PlayerController.Current.DestroyCylinder(this);
        }
        else //Silindirin boyutunu güncelle
        {
            int cylinderCount = PlayerController.Current.cylinders.Count;
            transform.localPosition = new Vector3(transform.localPosition.x, -0.5f * (cylinderCount - 1) - 0.25f * _value, transform.localPosition.z);
            transform.localScale = new Vector3(0.5f * _value, transform.localScale.y, 0.5f * _value);
        }
    }
}
