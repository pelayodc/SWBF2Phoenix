using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PhxUIMap : MonoBehaviour
{
    PhxGame GAME => PhxGame.Instance;
    PhxScene SCENE => PhxGame.GetScene();
    PhxMatch MATCH => PhxGame.GetMatch();


    public enum PhxUIMapMode
    {
        Static,
        StaticClickable,
        Dynamic
    }

    [Header("References")]
    public Button CPButtonPrefab;

    [Header("Settings")]
    public PhxUIMapMode Mode = PhxUIMapMode.Static;
    public float CPUpdateFreq = 10f;
    public float CPSelectAnimSpeed = 1f;
    public float Zoom = 300f;
    public Vector2 MapOffset = Vector2.zero;
    public Vector2 MapTexOffset = new Vector2(-0.01f, 0.045f);

    public Action<PhxCommandpost> OnCPSelect;

    Material MapMat;
    float RefreshTimer;

    PhxCommandpost[] CommandPosts;
    Vector4[] CPPositions = new Vector4[32];
    Vector4[] ObjPositions = new Vector4[8];
    Color[] CPColors = new Color[64];
    Color ObjectiveColor = Color.yellow;
    bool[] CPSelected = new bool[64];
    float[] CPSelectAnim = new float[64];
    int CPCount = 0;

    Vector2 InitMapTexOffset = Vector2.zero;

    AudioClip CPSelectSound;
    Button[] CPButtons = new Button[64];


    public void SelectCP(PhxCommandpost cp)
    {
        int idx = Array.IndexOf(CommandPosts, cp);
        if (idx >= 0)
        {
            SelectCP(idx);
        }
    }

    void SelectCP(int idx)
    {
        Debug.Assert(idx >= 0 && idx < CommandPosts.Length);
        Array.Clear(CPSelected, 0, CPSelected.Length);
        CPSelected[idx] = true;
        OnCPSelect?.Invoke(CommandPosts[idx]);

        GAME.PlayUISound(CPSelectSound, 1.4f);
    }

    void Awake()
    {
        CommandPosts = SCENE.GetCommandPosts();
        RefreshTimer = CPUpdateFreq;
    }

    void Start()
    {
        RawImage image = GetComponent<RawImage>();
        Debug.Assert(image != null);

        MapMat = image.materialForRendering;
        Debug.Assert(MapMat != null);

        CPSelectSound = SoundLoader.Instance.LoadSound("ui_menumove");

        // TODO: Load texture from "MapTexture" property specified in PhxCommandpost class
        Texture2D cpTexture = TextureLoader.Instance.ImportUITexture("hud_flag_icon");
        MapMat.SetTexture("_CPTex", cpTexture);

        Texture objTexture = TextureLoader.Instance.ImportUITexture("hud_objective_icon_circle");
        MapMat.SetTexture("_ObjTex", objTexture);

        if (SCENE.MapTexture != null)
        {
            Debug.Assert(SCENE.MapTexture.width == SCENE.MapTexture.height);
            MapMat.SetTexture("_MapTex", SCENE.MapTexture);
        }

        RectTransform rt = transform as RectTransform;
        MapMat.SetVector("_SpriteSize", new Vector4(rt.sizeDelta.x, rt.sizeDelta.y));

        if (Mode == PhxUIMapMode.StaticClickable)
        {
            for (int i = 0; i < CPButtons.Length; ++i)
            {
                CPButtons[i] = Instantiate(CPButtonPrefab, transform);
                CPButtons[i].gameObject.SetActive(false);

                RectTransform t = CPButtons[i].transform as RectTransform;
                t.sizeDelta = new Vector2(cpTexture.width, cpTexture.height);

                int idx = i;
                Button btn = CPButtons[i].GetComponent<Button>();
                btn.onClick.AddListener(() =>
                {
                    PhxCommandpost cp = CommandPosts[idx];
                    if (cp.Team == MATCH.Player.Team)
                    {
                        SelectCP(idx);
                    }
                });
            }
        }
    }

    void Update()
    {
        RefreshTimer += Time.deltaTime;
        if (CPUpdateFreq != 0f && RefreshTimer > CPUpdateFreq)
        {
            //Was StaticClickable but I think is an error
            if (Mode == PhxUIMapMode.Static)
            {
                for (int i = 0; i < CPCount; ++i)
                {
                    CPButtons[i].gameObject.SetActive(false);
                }
            }

            CommandPosts = SCENE.GetCommandPosts();
            CPCount = CommandPosts.Length;

            Vector2 positionSum = Vector2.zero;
            Vector2 posMin = Vector2.positiveInfinity;
            Vector2 posMax = Vector2.negativeInfinity;
            for (int i = 0; i < CPCount; ++i)
            {
                int cpIdx = i / 2;
                int cpVecIdx = (i % 2) * 2;

                if (CommandPosts[i].gameObject.activeSelf)
                {
                    Vector2 pos = new Vector2(CommandPosts[i].transform.position.x, CommandPosts[i].transform.position.z);
                    positionSum += pos;

                    posMin.x = Mathf.Min(posMin.x, pos.x);
                    posMin.y = Mathf.Min(posMin.y, pos.y);
                    posMax.x = Mathf.Max(posMax.x, pos.x);
                    posMax.y = Mathf.Max(posMax.y, pos.y);

                    CPPositions[cpIdx][cpVecIdx + 0] = pos.x;
                    CPPositions[cpIdx][cpVecIdx + 1] = pos.y;
                } else
                {
                    CPPositions[cpIdx][cpVecIdx + 0] = 0;
                    CPPositions[cpIdx][cpVecIdx + 1] = 0;
                }
            }

            for (int i = 0; i < GAME.markers.Count; ++i)
            {
                int objIdx = i / 2;
                int objVecIdx = (i % 2) * 2;

                //if (CommandPosts[i].gameObject.activeSelf)
               // {
                    Vector2 pos = new Vector2(GAME.markers[i].transform.position.x, GAME.markers[i].transform.position.z);

                    ObjPositions[objIdx][objVecIdx + 0] = pos.x;
                    ObjPositions[objIdx][objVecIdx + 1] = pos.y;
               // }
                //else
               // {
                 //   CPPositions[objIdx][objVecIdx + 0] = 0;
                 //   CPPositions[objIdx][objVecIdx + 1] = 0;
               // }
            }


            // Take the mean of all cp positions and flip the axis
            Vector2 worldOffset = positionSum / -CPCount;

            if (Mode != PhxUIMapMode.Dynamic)
            {
                // zoom is in world units per UV
                Zoom = (posMax - posMin).magnitude * 1.15f;

                MapOffset = worldOffset;
            }

            InitMapTexOffset = -worldOffset / Zoom;

            if (Mode == PhxUIMapMode.StaticClickable)
            {
                RectTransform rectT = transform as RectTransform;

                for (int i = 0; i < CPCount; ++i)
                {
                    if (CommandPosts[i].gameObject.activeSelf)
                    {
                        int cpIdx = i / 2;
                        int cpVecIdx = (i % 2) * 2;

                        Vector2 cpWorldPos = new Vector2(CPPositions[cpIdx][cpVecIdx], CPPositions[cpIdx][cpVecIdx + 1]);
                        Vector2 cpMapPos = ((cpWorldPos + MapOffset) / Zoom) + new Vector2(0.5f, 0.5f);

                        CPButtons[i].gameObject.SetActive(true);
                        RectTransform t = CPButtons[i].transform as RectTransform;
                        t.anchoredPosition = new Vector3(cpMapPos.x * rectT.rect.width, cpMapPos.y * rectT.rect.height, 0f);
                    }
                }
            }

            if (SCENE.MapTexture != null)
            {
                float mapTexSizeFactor = Zoom / SCENE.MapTexture.width;
                MapMat.SetFloat("_MapTexSize", mapTexSizeFactor);
            }

            MapMat.SetVectorArray("_CPPositions", CPPositions);
            MapMat.SetFloat("_CPCount", CPCount);
            MapMat.SetFloat("_ObjCount", GAME.markers.Count);
            //MapMat.SetVector("_MapTexOffset", InitMapTexOffset + MapTexOffset);

            RefreshTimer = 0f;
        }

        for (int i = 0; i < CPCount; ++i)
        {
            CPColors[i] = MATCH.GetTeamColor(CommandPosts[i].Team);

            if (CPSelected[i])
            {
                CPSelectAnim[i] = Mathf.Clamp01(CPSelectAnim[i] + Time.deltaTime / CPSelectAnimSpeed);
            }
            else
            {
                CPSelectAnim[i] = Mathf.Clamp01(CPSelectAnim[i] - Time.deltaTime / CPSelectAnimSpeed);
            }
        }

        MapMat.SetVector("_MapTexOffset", InitMapTexOffset + MapTexOffset);

        MapMat.SetFloat("_Zoom", Zoom);
        MapMat.SetVector("_MapOffset", MapOffset);

        MapMat.SetColorArray("_CPColors", CPColors);
        MapMat.SetColor("_ObjColor", ObjectiveColor);
        MapMat.SetFloatArray("_CPSelected", CPSelectAnim);
    }
}
