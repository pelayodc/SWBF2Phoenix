using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhxEntityMarker : MonoBehaviour
{
    public RawImage img;
    private GameObject camera;
    private static Vector3 scaleStatic = new Vector3(1.0f, 1.0f, 0);
    private static Vector3 scaleChange = new Vector3(0.1f, 0.1f, 0);

    void Start()
    {
        camera = GameObject.FindWithTag("MainCamera");
    }

    void Update()
    {

        if(camera != null)
        {
            this.gameObject.transform.LookAt(camera.transform);
            float multiplier = Vector3.Distance(gameObject.transform.position, camera.gameObject.transform.position) / 10;
            if(multiplier < 1.0f)
            {
                this.gameObject.transform.localScale = scaleStatic + scaleChange * Mathf.Sin(3 * Time.unscaledTime);
            } else
            {
                this.gameObject.transform.localScale = multiplier * (scaleStatic + scaleChange * Mathf.Sin(3 * Time.unscaledTime));
            }
        }
    }

    public void SetImg(Texture texture, Color color)
    {
        img.texture = texture;
        img.color = color;
    }
}
