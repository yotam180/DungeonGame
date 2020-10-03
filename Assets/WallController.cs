using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (Random.Range(0f, 1f) < .85f)
        {
            transform.Find("Cube").gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
