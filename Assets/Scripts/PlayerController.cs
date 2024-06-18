using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    private Entity moveScript;

    private Camera _camera;
    private Bounds cameraBounds;

    public Tilemap terrain;

    void Awake() => _camera = Camera.main;

    void Start()
    {
        moveScript = GetComponent<Entity>();

        // Bound camera to map bounds
        var camHei = _camera.orthographicSize;
        var camWid = camHei * _camera.aspect;

        cameraBounds = new Bounds();
        cameraBounds.SetMinMax(
            new Vector3(Globals.mapBounds.min.x + camWid, Globals.mapBounds.min.y + camHei, 0),
            new Vector3(Globals.mapBounds.max.x - camWid, Globals.mapBounds.max.y - camHei, 0)
        );

    }

    void Update()
    {
        //Dont do anything if game is paused
        if (PauseController.gameIsPaused)
        {
            return;
        }

        float leftKey = Input.GetKey(KeyCode.LeftArrow) ? 1 : 0;
        float rightKey = Input.GetKey(KeyCode.RightArrow) ? 1 : 0;

        float upKey = Input.GetKey(KeyCode.Space) ? 1 : 0;
        float downKey = Input.GetKey(KeyCode.DownArrow) ? 1 : 0;

        float jumpKey = Input.GetKeyDown(KeyCode.Space) ? 1 : 0;

        float hThrottle = rightKey - leftKey;
        float vThrottle = downKey - upKey;

        moveScript.TryMove(hThrottle, vThrottle, jumpKey);
        transform.position = boundPlayer(transform.position);

        // Camera follows player
        _camera.transform.position = boundCamera(transform.position + new Vector3(0, 0, -14));

        // Turn around if pressing other direction
        Vector3 scale = transform.localScale;
        if ((scale.x > 0 && hThrottle < 0) || (scale.x < 0 && hThrottle > 0))
            transform.localScale = new Vector3(-scale.x, scale.y, scale.z);

        UpdateInteraction();
    }


    const float INTERACT_DISTANCE = 1;
    private Interactable currentInteraction = null;
    private void UpdateInteraction()
    {
        // Find closest interactable object
        Interactable interact = null;
        float dist = INTERACT_DISTANCE;

        foreach (Interactable i in Object.FindObjectsOfType<Interactable>())
        {
            float iDist = Vector3.Distance(i.transform.position, transform.position);
            if (iDist < dist)
            {
                interact = i;
                dist = iDist;
            }
        }

        // Update current selected interactable
        if (interact != currentInteraction)
        {
            if (currentInteraction != null) currentInteraction.disableInteraction();
            if (interact != null) interact.enableInteraction();
            currentInteraction = interact;
        }

        if (currentInteraction != null && Input.GetKeyDown(KeyCode.F))
            currentInteraction.interact();

    }

    private Vector3 boundPlayer(Vector3 pos)
    {

        return new Vector3(Mathf.Clamp(pos.x, Globals.mapBounds.min.x, Globals.mapBounds.max.x),
        Mathf.Clamp(pos.y, Globals.mapBounds.min.y, Globals.mapBounds.max.y),
        pos.z
        );
    }

    private Vector3 boundCamera(Vector3 pos)
    {
        return new Vector3(Mathf.Clamp(pos.x, cameraBounds.min.x, cameraBounds.max.x),
        Mathf.Clamp(pos.y, cameraBounds.min.y, cameraBounds.max.y),
        pos.z
        );
    }
}
