using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Movement moveScript;

    float leftKey;
    float rightKey;
    float downKey;
    float upKey;

    float hThrottle;
    float vThrottle;

    float jumpKey;

    // Start is called before the first frame update
    void Start()
    {
        moveScript = GetComponent<Movement>();
    }

    // Update is called once per frame
    void Update()
    {
        leftKey = Input.GetKey(KeyCode.LeftArrow) ? 1 : 0;
        rightKey = Input.GetKey(KeyCode.RightArrow) ? 1 : 0;

        upKey = Input.GetKey(KeyCode.UpArrow) ? 1 : 0;
        downKey = Input.GetKey(KeyCode.DownArrow) ? 1 : 0;

        jumpKey = Input.GetKeyDown(KeyCode.Space) ? 1 : 0;

        hThrottle = rightKey - leftKey;
        vThrottle = downKey - upKey;

        moveScript.TryMove(hThrottle,vThrottle,jumpKey);
    }
}
