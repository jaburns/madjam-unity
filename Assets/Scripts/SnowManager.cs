using UnityEngine;
using System.Collections;

public class SnowManager : MonoBehaviour
{
    public ParticleSystem Down;
    public ParticleSystem Up;

    void Start()
    {
        Up.gameObject.SetActive(false);
        Down.gameObject.SetActive(true);
        Down.Simulate(100);
        Down.Play();

        GravitySetting.OnGravitySwitch += GravitySwitch;
    }
    void OnDestroy()
    {
        GravitySetting.OnGravitySwitch -= GravitySwitch;
    }

    void GravitySwitch()
    {
        if (GravitySetting.Reverse) {
            Down.gameObject.SetActive(false);
            Up.gameObject.SetActive(true);
            Up.Simulate(100);
            Up.Play();
        } else {
            Up.gameObject.SetActive(false);
            Down.gameObject.SetActive(true);
            Down.Simulate(100);
            Down.Play();
        }
    }
}
