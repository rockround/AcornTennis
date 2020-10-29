using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedSpawner : MonoBehaviour
{
    public GameObject seed;
    GameObject currentSeed;
    Vector3 previousPosition;
    public bool pitch;
    public Vector3 velocity;
    // Start is called before the first frame update
    void Start()
    {
        currentSeed = Instantiate(seed,transform.position,transform.rotation);
        previousPosition = currentSeed.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(1) || currentSeed.transform.position.y < -1)
        {
            Destroy(currentSeed);
            currentSeed = Instantiate(seed, transform.position, transform.rotation);
            if (pitch)
            {
                currentSeed.GetComponent<Rigidbody>().isKinematic = false;
                currentSeed.GetComponent<Rigidbody>().velocity = velocity;
            }
        }
    }
}
