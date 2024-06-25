using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{   
    [SerializeField] GameObject topDoor;
    [SerializeField] private int maxRooms = 15;
    [SerializeField] private int minRooms = 10;
    
    int roomWidth = 20;
    int roomHeight = 12;

    int gridSizeX=10;
    int gridSizeY=10;

    private List<GameObject> roomObjects = new List<GameObject>();

    private int [,] roomGrid;
    private int roomCount;

    private void Start(){
        roomGrid = new int[gridSizeX,gridSizeY];
        
    }
    private Vector2Int GetPositionFromGridIndex(Vector2Int gridIndex){
           int gridX = gridIndex.x;
           int gridY = gridIndex.y;
           return new Vector2Int(roomWidth*(gridX - gridSizeX / 2), roomHeight*(gridY - gridSizeY / 2)); 
        }

    private void OnDrawGizmos()
    {
        //Color gizmoColor
    }
}
