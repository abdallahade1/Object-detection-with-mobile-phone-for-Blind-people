using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;
using System;

public class TestScript : MonoBehaviour
{
    //Get a Google API Key from https://developers.google.com/maps/documentation/geocoding/get-api-key
    public string GoogleAPIKey;

    void OnEnable()
    {
        StartCoroutine(DetermineLocation());
    }

    IEnumerator DetermineLocation()
    {
        // Check if the user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("Enable Location Service ... ");
            yield break;
        }

        // Start service before query location 
        Input.location.Start();

        // Wait until service initializes 
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service did not initialize in 20 seconds
        if (maxWait < 1)
        {
            Debug.Log("Timed out ... ");
            yield break;
        }

        // Connection has failed 
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location ... ");
        }
        else
        {
            Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude);
        }

        // Stop retrieving location 
        Input.location.Stop();


    }
}
