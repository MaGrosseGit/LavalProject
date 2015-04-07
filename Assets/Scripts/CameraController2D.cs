using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CameraController2D : MonoBehaviour
{
    #region public variables

    public LerpTypes lerptype;

    public float zValCamera = 15f;
    public float zValBlur = 17f;
    public float layerSpeed = 5;

    public GameObject layersGo;
    public bool loopLayers = false;
    public Canvas blurCanvas;
    [Range(0,50)]
    public float blurSize = 10f;
    public List<Transform> layersList = new List<Transform>();
    public int curLayerId = 0;
    #endregion

    #region private variables
    private Vector3 defaultLayer;

    public enum LerpTypes { Lerp, MoveTowards}

    private Camera thisCamera;
    #endregion

    void Start()
    {
        thisCamera = GetComponent<Camera>();
        defaultLayer = transform.position;

        layersList.Clear();
        for (int i = 0; i < layersGo.transform.childCount; i++)
        {
            Transform curGo = layersGo.transform.GetChild(i);
            layersList.Add(curGo);
            Canvas layerCanvasGo = Instantiate(blurCanvas, Vector3.zero, Quaternion.identity) as Canvas;
            layerCanvasGo.transform.parent = curGo;
            layerCanvasGo.transform.position = thisCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, curGo.transform.position.z));
            Image layerPanel = layerCanvasGo.transform.GetChild(0).GetComponent<Image>();
            layerPanel.material = new Material(Shader.Find("Custom/ImplifiedBlur"));
            layerPanel.material.SetFloat("_Size", 10);
        }

    }


    void Update()
    {
        UpdateLayersPos();
        HandleLayersChange();
        transform.position = HandleCurLayer(curLayerId, zValCamera);
    }

    void UpdateLayersPos()
    {
        for (int i = 0; i < layersGo.transform.childCount; i++)
        {
            Transform curGo = layersGo.transform.GetChild(i);
            Canvas layerCanvasGo = curGo.transform.GetChild(0).GetComponent<Canvas>();
            Vector3 canvasPosZ = thisCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, curGo.transform.position.z));
            layerCanvasGo.transform.position = new Vector3(canvasPosZ.x, canvasPosZ.y, curGo.transform.position.z - zValBlur);
            Image layerPanel = layerCanvasGo.transform.GetChild(0).GetComponent<Image>();
            //layerPanel.material = new Material(Shader.Find("Custom/ImplifiedBlur"));
            layerPanel.material.SetFloat("_Size", blurSize);
        }
    }

    void HandleLayersChange()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            curLayerId++;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            curLayerId--;

        if (curLayerId >= layersList.Count)
            curLayerId = (loopLayers) ? 0 : layersList.Count - 1;

        if (curLayerId < 0)
            curLayerId = (loopLayers) ? layersList.Count - 1 : 0;
    }

    Vector3 HandleCurLayer(int curLayer, float zVal = 15f, bool backToDefault = false)
    {
        if (layersList.Count == 0)
            backToDefault = true;
        Vector3 usedVal = (backToDefault) ? defaultLayer : layersList[curLayerId].position;
        usedVal = (!thisCamera.orthographic) ? usedVal - new Vector3(0, 0, zVal) : usedVal;

        var time = Mathfx.Hermite(0.0f, 1.0f, Time.deltaTime);
        time *= (lerptype == LerpTypes.Lerp) ? layerSpeed : layerSpeed * 5;
        //Vector3 returnedVal = (lerptype == LerpTypes.Lerp) ? Vector3.Lerp(transform.position, new Vector3(transform.position.x, transform.position.y, usedVal.z), time) : Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, transform.position.y, usedVal.z), time);
        Vector3 returnedVal = (lerptype == LerpTypes.Lerp) ? Vector3.Lerp(transform.position, new Vector3(transform.position.x, transform.position.y, usedVal.z), time) : Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, transform.position.y, usedVal.z), time);

        return new Vector3(0, 0, returnedVal.z);
    }

    public float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
