using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : Entity
{

    private Camera _camera;
    private Bounds cameraBounds;

    private Inventory inventory;
    private Transform uiInteractButton;


    public override void Awake()
    {
        _camera = Camera.main;
        inventory = GetComponent<Inventory>();
        uiInteractButton = GameObject.Find("UI_Canvas").transform.Find("Show Interact");
        base.Awake();
    }

    public override void Start()
    {
        updateCameraBounds();
        base.Start();

    }
    private void updateCameraBounds()
    {
        var camHei = _camera.orthographicSize;
        var camWid = camHei * _camera.aspect;

        cameraBounds = new Bounds();
        cameraBounds.SetMinMax(
            new Vector3(Globals.mapBounds.min.x + camWid, Globals.mapBounds.min.y + camHei, 0),
            new Vector3(Globals.mapBounds.max.x - camWid, Globals.mapBounds.max.y - camHei, 0)
        );

    }

    public float hThrottle = 0, vThrottle = 0, vThrottleJump = 0;

    private bool bufferUseJump, bufferUseAttack, bufferUseDodge, bufferUseMagic;

    void Update()
    {
        if (PauseController.gameIsPaused)
            return;

        var camHei = _camera.orthographicSize;
        var camWid = camHei * _camera.aspect;
        cameraBounds.SetMinMax(
            new Vector3(Globals.mapBounds.min.x + camWid, Globals.mapBounds.min.y + camHei, 0),
            new Vector3(Globals.mapBounds.max.x - camWid, Globals.mapBounds.max.y - camHei, 0)
        );

        float leftKey = (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) ? 1 : 0;
        float rightKey = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) ? 1 : 0;

        float upKey = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) ? 1 : 0;
        float jumpKey = Input.GetKey(KeyCode.Space) ? 1 : 0;
        float downKey = (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) ? 1 : 0;

        hThrottle = rightKey - leftKey;
        vThrottle = downKey - upKey;
        vThrottleJump = downKey - jumpKey;

        // Buffer inputs
        if (Input.GetKeyDown(KeyCode.Space)) bufferUseJump = true;
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Z)) bufferUseAttack = true;
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.X)) bufferUseDodge = true;
        // if (Input.GetKeyDown(KeyCode.C)) bufferUseMagic = true;

        UpdateInteraction();
    }

    public override void FixedUpdate()
    {
        if (PauseController.gameIsPaused)
            return;

        // TODO is this needed every update?
        updateCameraBounds();

        Move(hThrottle, vThrottleJump, bufferUseJump);

        if (bufferUseAttack)
            Attack();
        if (bufferUseDodge)
            Dodge();
        /*if (bufferUseMagic)
            FireProjectile(projectiles[0]);*/


        transform.position = boundPlayer(transform.position);

        // Camera follows player
        _camera.transform.position = boundCamera(transform.position + new Vector3(0, 0, -14));

        // Turn around if pressing other direction
        base.FixedUpdate();

        // Inputs have been consumed
        bufferUseJump = false;
        bufferUseAttack = false;
        bufferUseDodge = false;
        bufferUseMagic = false;
    }

    // Drop item on attack
    public override void Attack(float x = 0, float y = 0)
    {
        base.Attack(x, y);
        if (inventory.weapon == null)
            inventory.dropCarry(false);
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

        // Show interact on UI
        if (uiInteractButton != null) uiInteractButton.gameObject.SetActive(currentInteraction != null);

        if (Input.GetKeyDown(KeyCode.F))
            Interact();
    }
    private void Interact()
    {
        if (currentInteraction != null)
        {
            currentInteraction.interact();
            return;
        }

        Item drop = inventory.carry;
        if (drop != null)
        {
            Vector2 throwDirection = new Vector2(hThrottle, -vThrottle);
            // Don't throw non-throwables horizontally
            if (inventory.carry.throwDamage <= 0) throwDirection.x = 0;
            // Arc throwables up slightly
            else if (throwDirection.x != 0) throwDirection.y += 0.4f;

            // Only care about the direction
            throwDirection = throwDirection.normalized;
            Vector2 pushDirection = -throwDirection;

            // Bias against gravity slightly
            if (throwDirection.y != 0)
            {
                throwDirection.y += 0.25f;
                pushDirection.y += 0.15f;
            }

            if (throwDirection == Vector2.zero)
                inventory.dropCarry(false);
            else
            {
                inventory.throwCarry(throwDirection);

                rb.velocity += 3 * (1.5f + drop.weight / 3) * pushDirection;

                // Cancel upwards momentum on toss up
                if (pushDirection.y < 0 && rb.velocity.y > 0)
                    rb.velocity = new Vector3(rb.velocity.x, 0);
            }
        }
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
        Vector3 camPos = _camera.transform.position;
        Vector3 camGoTo;

        float cameraSmoothingFactor = 0.1f;

        Vector3 direction = -(camPos - pos).normalized;
        float dist = Vector3.Distance(camPos, pos);


        if (Mathf.Abs(dist) > 0.01f)
        {
            camGoTo = camPos + (direction * cameraSmoothingFactor * dist);
        }
        else
        {
            camGoTo = pos;
        }

        return new Vector3(Mathf.Clamp(camGoTo.x, cameraBounds.min.x, cameraBounds.max.x),
        Mathf.Clamp(camGoTo.y, cameraBounds.min.y, cameraBounds.max.y),
        pos.z
        );
    }
}
