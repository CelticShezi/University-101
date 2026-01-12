using System.Collections.Generic;
using UnityEngine;

public class IDable : MonoBehaviour
{
    static Dictionary<string, int> counters = new Dictionary<string, int>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (!counters.ContainsKey(tag))
        {
            counters[tag] = 0;
        }
        counters[tag]++;
        ID = $"{tag}_{counters[tag]}";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        counters[tag]--;
    }
    public string ID { get; set; }
}
