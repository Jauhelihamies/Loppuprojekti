using UnityEngine;

public class AkvaarioScript : MonoBehaviour
{
    private int maara = 20;
    private int nytMaara = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }

    public void Add(int lisaa)
    {
        nytMaara += lisaa;
    }
}
