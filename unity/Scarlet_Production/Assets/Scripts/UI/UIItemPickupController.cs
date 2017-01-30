﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIItemPickupController : MonoBehaviour {
    
    public GameObject textHint;
    private Text textField;
    private Image[] images;


    void Start()
    {
        images = textHint.GetComponentsInChildren<Image>();
        textField = textHint.GetComponentInChildren<Text>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(FadeTo(1.0f, 0.6f));
            textHint.GetComponentInChildren<ButtonPromptController>().IsInTriggerArea(gameObject, true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(FadeTo(0.0f, 0.6f));
            textHint.GetComponentInChildren<ButtonPromptController>().IsInTriggerArea(gameObject, false);
        }
    }

    public void OnItemPickedUp()
    {
        //StartCoroutine(FadeTo(0.0f, 0.6f));
        //Fade doesn't work because Gameobject is destroyed/invisible

        images[0].color = new Color(0, 0, 0, 0);
        images[1].color = new Color(0.65f, 0, 0, 0);
        textField.color = new Color(0.65f, 0, 0, 0);

        textHint.GetComponentInChildren<ButtonPromptController>().IsInTriggerArea(gameObject, false);
    }

    IEnumerator FadeTo(float aValue, float aTime)
    {
        float alpha = textField.color.a;
        float alphaBlack = images[0].color.a;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
        {
            //Color newColor = new Color(1, 1, 1, Mathf.Lerp(alpha, aValue, t));
            Color newColorRed = new Color(0.65f, 0, 0, Mathf.Lerp(alpha, aValue, t));
            Color newColorBlack = new Color(0, 0, 0, Mathf.Lerp(alphaBlack, aValue / 1.75f, t));
            //arrowHint.GetComponent<SpriteRenderer>().color = newColor;
            images[0].color = newColorBlack;
            images[1].color = newColorRed;
            textField.color = newColorRed;

            yield return null;
        }
        images[0].color = new Color(0, 0, 0, aValue / 1.75f);
        images[1].color = new Color(0.65f, 0, 0, aValue);
        textField.color = new Color(0.65f, 0, 0, aValue);
    }

}
