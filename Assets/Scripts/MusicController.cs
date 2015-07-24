using UnityEngine;

public class MusicController : MonoBehaviour
{
    static public MusicController Instance { get {
        return GameObject.FindObjectOfType<MusicController>();
    } }

    public AudioSource[] Musics;

    void Awake()
    {
        reset();
    }

    void reset()
    {
        foreach (var src in Musics) {
            src.volume = 0;
        }
    }

    public void SetMusic(int index)
    {
        reset();
        Musics[index].volume = 1;
    }
}
