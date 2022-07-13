using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class Opning : MonoBehaviour
{
    [SerializeField]
    private VideoPlayer player;

    [SerializeField]
    string nextScene;

    public VideoClip[] clips;

    // Start is called before the first frame update
    void Start()
    {
        if (clips.Length == 0)
        {
            SceneManager.LoadScene(nextScene);
            this.gameObject.SetActive(false);
            return;
        }
        int rnd = Random.Range(0, clips.Length);
        player.clip = clips[rnd];
        player.Play();
        Debug.Log("play : " + player.clip.name);
    }

    // Update is called once per frame
    void Update()
    {
        var frameCount = player.clip.frameCount;
        if (player.frame < 0)
        {
            return;
        }

            if ((ulong)player.frame >= frameCount)
        {
            Debug.Log("stop");
            player.Stop();
            SceneManager.LoadScene(nextScene);
            this.gameObject.SetActive(false);
        }
    }
}