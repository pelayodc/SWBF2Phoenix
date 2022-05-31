using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhxObjetivePopUp : PhxMenuInterface
{
    static PhxGame GAME => PhxGame.Instance;

    public Text text;

    void Start()
    {
        //Time.timeScale = 0f;
    }

    public override void Clear()
    {

    }

    public void SetText(string objetiveText)
    {
        text.text = objetiveText;
    }

    void Update()
    {
        //Check fire or enter tu continue
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Continue();
        }
    }

    private void Continue()
    {
        Time.timeScale = 1.0f;
        GAME.ShowMenu(GAME.HUDPrefab);
    }
}
