using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Fallthrough : MonoBehaviour
{
    private TilemapCollider2D _collider;
    private PlayerController player;
    void Start()
    {
        _collider = GetComponent<TilemapCollider2D>();
        player = Object.FindObjectOfType<PlayerController>();
    }

    private float fallthroughTime = 0;
    void Update()
    {
        // Disable on down press
        if (player.vThrottle > 0)
        {
            _collider.enabled = false;
            fallthroughTime = 0.2f;
        }

        // Re-enable after time has passed
        if (fallthroughTime > 0)
        {
            fallthroughTime -= Time.deltaTime;
            if (fallthroughTime <= 0) _collider.enabled = true;
        }
    }
}
