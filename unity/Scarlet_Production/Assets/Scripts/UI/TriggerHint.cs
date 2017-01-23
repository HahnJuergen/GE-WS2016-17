﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TriggerHint : MonoBehaviour {

    public GameObject arrowHint;
    public GameObject textHint;
    private Text textField;
    private Image imageButton;


    void Start()
    {
        imageButton = textHint.GetComponentInChildren<Image>();
        textField = textHint.GetComponentInChildren<Text>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(FadeTo(1.0f, 0.6f));
            if(arrowHint != null)
            {
                var particleSystems = arrowHint.GetComponentsInChildren<ParticleSystem>();
                particleSystems[0].Play();
                particleSystems[1].Play();
                particleSystems[2].Play();
            }
            textHint.GetComponentInChildren<ButtonPromptController>().IsInTriggerArea(gameObject, true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(FadeTo(0.0f, 0.6f));
            if (arrowHint != null)
            {
                var particleSystems = arrowHint.GetComponentsInChildren<ParticleSystem>();
                particleSystems[0].Stop();
                particleSystems[1].Stop();
                particleSystems[2].Stop();
            }
            textHint.GetComponentInChildren<ButtonPromptController>().IsInTriggerArea(gameObject, false);
        }
    }


    IEnumerator FadeTo(float aValue, float aTime)
    {
        float alpha = textField.color.a;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
        {
            //Color newColor = new Color(1, 1, 1, Mathf.Lerp(alpha, aValue, t));
            Color newColorRed = new Color(0.65f, 0, 0, Mathf.Lerp(alpha, aValue, t));
            //arrowHint.GetComponent<SpriteRenderer>().color = newColor;
            if(imageButton != null)
            {
                imageButton.color = newColorRed;
            }
            textField.color = newColorRed;

            yield return null;
        }
        if (imageButton != null)
        {
            imageButton.color = new Color(0.65f, 0, 0, aValue);
        }
        textField.color = new Color(0.65f, 0, 0, aValue);
    }

}
