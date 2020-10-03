using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallController : MonoBehaviour
{
    Color myColor;
    Color myRealColor;

    float timeLeft = 1;

    static readonly float EMISSION_MAX = .2f;

    // Start is called before the first frame update
    void Start()
    {
        // if (Random.Range(0f, 1f) < .85f)
        // {
        //     transform.Find("Cube").gameObject.SetActive(false);
        // }

        myColor = new Color(Random.Range(0f, EMISSION_MAX), Random.Range(0f, EMISSION_MAX), Random.Range(0f, EMISSION_MAX));
        myRealColor = myColor;
    }

    // Update is called once per frame
    void Update()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0)
        {
            timeLeft = Random.Range(1, 5);
            myColor = new Color(Random.Range(0f, EMISSION_MAX), Random.Range(0f, EMISSION_MAX), Random.Range(0f, EMISSION_MAX));
        }
        // myColor += new Color(Random.Range(-0.0001f, 0.0001f), Random.Range(-0.0001f, 0.0001f), Random.Range(-0.0001f, 0.0001f));

        myRealColor = Color.Lerp(myRealColor, myColor, .92f * Time.deltaTime);
        GetComponent<Renderer>().material.SetColor("_EmissiveColor", myRealColor);
    }
}
