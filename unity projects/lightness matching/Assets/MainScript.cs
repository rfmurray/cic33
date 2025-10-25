using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MainScript : MonoBehaviour
{
    public GameObject preApparatus, experimentApparatus;
    public GameObject referencePlane;
    public Material referenceMaterial, matchMaterial;

    string filename;

    enum Phase { Instructions, ShowStimulus, GetResponse };
    Phase phase = Phase.Instructions;

    int trial = 0, ntrials = 40;
    float startTime = 0;

    float[] referenceReflectances = { 0.2f, 0.4f, 0.6f };
    float[] referenceOrientations = { -45f, -30f, -15f, 0f, 15f, 30f, 45f };
    float referenceReflectance, referenceOrientation, matchReflectance;

    void Start()
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        filename = $"data_{timestamp}.txt";
        using (StreamWriter writer = new StreamWriter(filename, append: true))
            writer.WriteLine("#trial,referenceReflectance,referenceOrientation,matchReflectance,responseTime");

        preApparatus.SetActive(true);
        experimentApparatus.SetActive(false);
    }

    void Update()
    {
        bool responseQuit = Input.GetKeyDown(KeyCode.Q);
        if (responseQuit)
            Quit();

        if(phase == Phase.Instructions)
        {
            bool responseStart = Input.GetKeyDown(KeyCode.Return);
            if (responseStart)
            {
                preApparatus.SetActive(false);
                experimentApparatus.SetActive(true);
                phase = Phase.ShowStimulus;
            }
            return;
        }
        else if (phase == Phase.ShowStimulus)
        {
            int k = UnityEngine.Random.Range(0, referenceReflectances.Length);
            referenceReflectance = referenceReflectances[k];
            SetMaterialGrey(referenceMaterial, referenceReflectance);

            k = UnityEngine.Random.Range(0, referenceOrientations.Length);
            referenceOrientation = referenceOrientations[k];
            referencePlane.transform.rotation = Quaternion.Euler(-90f, -referenceOrientation, 0f);

            phase = Phase.GetResponse;
            startTime = Time.time;
            return;
        }
        else if (phase == Phase.GetResponse)
        {
            bool responseDone = Input.GetKeyDown(KeyCode.Space);
            if (responseDone)
            {
                float rt = Time.time - startTime;

                string dataline = $"{trial},{referenceReflectance},{referenceOrientation},{matchReflectance},{rt}";
                using (StreamWriter writer = new StreamWriter(filename, append: true))
                    writer.WriteLine(dataline);

                trial = trial + 1;
                if (trial > ntrials)
                    Quit();
                phase = Phase.ShowStimulus;
                return;
            }

            Vector3 mousexy = Input.mousePosition;
            matchReflectance = mousexy.y / Screen.height;
            matchReflectance = Math.Clamp(matchReflectance, 0f, 1f);
            SetMaterialGrey(matchMaterial, matchReflectance);
            return;
        }
    }

    void SetMaterialGrey(Material m, float reflectance)
    {
        float g = sRGBfn.sRGBinv(reflectance);
        Color c = new Color(g, g, g);
        m.SetColor("_BASE_COLOR", c);
    }

    void Quit()
    {
        UnityEditor.EditorApplication.isPlaying = false;
    }
    
}
