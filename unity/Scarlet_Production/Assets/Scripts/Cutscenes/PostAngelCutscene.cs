﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostAngelCutscene : MonoBehaviour
{
    public GameObject scarlet, dh, angel, m_BanishingEffect, m_Fire;

    public void Voice1()
    {
        new FARQ().ClipName("angel").StartTime(45.3f).EndTime(56.2f).Location(Camera.main.transform).Play();
    }

    public void Voice2()
    {
        new FARQ().ClipName("angel").StartTime(56.8f).EndTime(65.8f).Location(Camera.main.transform).Play();
    }

    public void VoiceScarlet()
    {
        new FARQ().ClipName("scarlet").StartTime(61.8f).EndTime(70.2f).Location(Camera.main.transform).Play();

        m_BanishingEffect.SetActive(true);
    }

    public void VoiceGuide()
    {
        new FARQ().ClipName("theguide").StartTime(45.3f).EndTime(56.2f).Location(Camera.main.transform).Play();
    }

    public void FadeToWhite()
    {
        Camera.main.GetComponent<FadeToBlack>().StartFade(Color.white, 10);
    }

    public void FadeToColor()
    {
        Camera.main.GetComponent<FadeToBlack>().StartFade(new Color(0, 0, 0, 0), 10);
    }

    public void SwitchCharacters()
    {
        scarlet.SetActive(false);
        angel.SetActive(false);
        dh.SetActive(true);
        dh.GetComponent<Animator>().SetTrigger("DeathTrigger");

        StartCoroutine(SmotherFlames());
        StartCoroutine(TurnOutLights());
    }

    private IEnumerator SmotherFlames()
    {
        yield return new WaitForSeconds(3f);

        Component[] pss = m_Fire.GetComponentsInChildren<ParticleSystem>();

        foreach(ParticleSystem ps in pss)
        {
            ps.Stop();
        }
    }

    private IEnumerator TurnOutLights()
    {
        yield return new WaitForSeconds(9f);

        Component[] lights = m_Fire.GetComponentsInChildren<Light>();

        foreach(Light l in lights)
        {
            l.enabled = false;
        }

        AudioSource[] asrcs = m_Fire.GetComponentsInChildren<AudioSource>();

        if(asrcs.Length > 0)
        {
            LerpTimer timer = new LerpTimer(2f);
            float startVol = asrcs[0].volume;

            while (timer.GetLerpProgress() < 1)
            {
                foreach(AudioSource asrc in asrcs)
                {
                    asrc.volume = Mathf.Lerp(startVol, 0, timer.GetLerpProgress());
                }

                yield return null;
            }

            foreach(AudioSource asrc in asrcs)
            {
                asrc.Stop();
            }
        }
    }
}
