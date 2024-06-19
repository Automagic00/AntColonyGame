using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Entity entity;

    // Start is called before the first frame update
    void Start()
    {
        entity = GetComponent<Entity>();
    }

    // Update is called once per frame
    void Update()
    {
        //Dont do anything if game is paused
        if (PauseController.gameIsPaused)
        {
            return;
        }

        entity.TryMove(0, 0, 0);
    }
}
