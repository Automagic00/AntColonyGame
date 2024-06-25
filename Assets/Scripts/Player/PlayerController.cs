using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : Entity
{

    private Camera _camera;
    private Bounds cameraBounds;


    public override void Awake()
    {
        _camera = Camera.main;
        base.Awake();
    }

    public override void Start()
    {

        // Bound camera to map bounds
        var camHei = _camera.orthographicSize;
        var camWid = camHei * _camera.aspect;

        cameraBounds = new Bounds();
        cameraBounds.SetMinMax(
            new Vector3(Globals.mapBounds.min.x + camWid, Globals.mapBounds.min.y + camHei, 0),
            new Vector3(Globals.mapBounds.max.x - camWid, Globals.mapBounds.max.y - camHei, 0)
        );

        base.Start();

    }

    private float hThrottle = 0, vThrottle = 0;

    private bool bufferUseJump, bufferUseAttack, bufferUseDodge, bufferUseMagic;

    void Update()
    {

        float leftKey = Input.GetKey(KeyCode.LeftArrow) ? 1 : 0;
        float rightKey = Input.GetKey(KeyCode.RightArrow) ? 1 : 0;

        float upKey = Input.GetKey(KeyCode.Space) ? 1 : 0;
        float downKey = Input.GetKey(KeyCode.DownArrow) ? 1 : 0;

        hThrottle = rightKey - leftKey;
        vThrottle = downKey - upKey;

        if (Input.GetKeyDown(KeyCode.Space)) bufferUseJump = true;
        if (Input.GetKeyDown(KeyCode.X)) bufferUseAttack = true;
        if (Input.GetKeyDown(KeyCode.Z)) bufferUseDodge = true;
        if (Input.GetKeyDown(KeyCode.C)) bufferUseMagic = true;

        UpdateInteraction();
    }

    public override void FixedUpdate()
    {
        if (PauseController.gameIsPaused)
            return;


        Move(hThrottle, vThrottle, bufferUseJump);

        if (bufferUseAttack)
            Attack();
        if (bufferUseDodge)
            Dodge();
        if (bufferUseMagic)
            FireProjectile(projectiles[0]);


        transform.position = boundPlayer(transform.position);

        // Camera follows player
        _camera.transform.position = boundCamera(transform.position + new Vector3(0, 0, -14));

        // Turn around if pressing other direction
        /*Vector3 scale = transform.localScale;
        if ((scale.x > 0 && hThrottle < 0) || (scale.x < 0 && hThrottle > 0))
            transform.localScale = new Vector3(-scale.x, scale.y, scale.z);*/

        base.FixedUpdate();

        // Inputs have been consumed
        bufferUseJump = false;
        bufferUseAttack = false;
        bufferUseDodge = false;
        bufferUseMagic = false;
    }


    private Interactive currentInteraction = null;
    private void UpdateInteraction()
    {
        Interactive interact = Interactive.closestInteractable(transform.position);
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
