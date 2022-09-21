using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhxObjetivePopUp : PhxMenuInterface
{
    static PhxGame GAME => PhxGame.Instance;

    public Text text;
    public Text Title;
    public Text PTC;

    void Start()
    {
        Time.timeScale = 0f;
    }

    public override void Clear()
    {

    }

    public void SetText(string objetiveText, string title, string ptc)
    {
        text.text = objetiveText;
        Title.text = title;
        PTC.text = ptc;
    }

    void Update()
    {
        //Check fire or enter tu continue
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Continue();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = 1.0f;
        }
    }

    private void Continue()
    {
        Time.timeScale = 1.0f;
        GAME.ShowMenu(GAME.HUDPrefab);
    }
}
