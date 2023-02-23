using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using TMPro;

public class BasePointerUI : MonoBehaviour
{
    public Vector3 basePosition;

    private Transform arrow;

    Vector3 screenPos;
    Vector2 onScreenPos;
    float max;
    private Camera mainCamera;

    private RectTransform pointerRectTransform;// Transform of entire object (text+arrow)
    private RectTransform arrowRectTransform; //Transform of just arrow

    //Text and Sprite components to enable disable on show
    private TextMeshProUGUI text;
    private Image arrowImg;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        arrow = transform.Find("Arrow");
        pointerRectTransform = GetComponent<RectTransform>();
        arrowRectTransform = arrow.GetComponent<RectTransform>();

        text = GetComponent<TextMeshProUGUI>();
        arrowImg = arrowRectTransform.GetComponentInChildren<Image>();

    }

    private void Update()
    {
        //bool isOffScreen;

        screenPos = mainCamera.WorldToViewportPoint(basePosition); //get viewport positions

        if (screenPos.x >= 0 && screenPos.x <= 1 && screenPos.y >= 0 && screenPos.y <= 1)
        {
            Debug.Log("already on screen, don't bother with the rest!");
            //gameObject.SetActive(false);
            Show(false);
        }
        else
        {
            float borderSize = 175f;
            //gameObject.SetActive(true);

            RotatePointerTowardsTargetPosition();
            onScreenPos = new Vector2(screenPos.x - 0.5f, screenPos.y - 0.5f) * 2; //2D version, new mapping
            max = Mathf.Max(Mathf.Abs(onScreenPos.x), Mathf.Abs(onScreenPos.y)); //get largest offset
            onScreenPos = (onScreenPos / (max * 2)) + new Vector2(0.5f, 0.5f); //undo mapping

            float clampedX = Mathf.Clamp(onScreenPos.x * Screen.width, 0+borderSize, Screen.width - borderSize);
            float clampedY = Mathf.Clamp(onScreenPos.y * Screen.height, 0+borderSize, Screen.height - borderSize);
            Vector3 adjustedPosition = new Vector3(clampedX, clampedY, 0);
            pointerRectTransform.position = adjustedPosition;
            Show(true);
        }
    }

    private void RotatePointerTowardsTargetPosition()
    {
        Vector3 adustedScreenPos = new Vector3(screenPos.x * Screen.width, screenPos.y * Screen.height, 0);
        arrowRectTransform.LookAt(adustedScreenPos);

        Vector3 toPosition = adustedScreenPos;
        Vector3 fromPosition = pointerRectTransform.position; 
        fromPosition.z = 0f;
        Vector3 dir = (toPosition - fromPosition).normalized;

        /*
        Debug.Log("toPosition: " + toPosition);
        Debug.Log("fromPosition: " + fromPosition);
        */

        float angle = UtilsClass.GetAngleFromVectorFloat(dir);
        arrowRectTransform.localEulerAngles = new Vector3(0, 0, angle);
    }

    private void Show(bool enable)
    {
        text.enabled = enable;
        arrowImg.enabled = enable;
    }
}
