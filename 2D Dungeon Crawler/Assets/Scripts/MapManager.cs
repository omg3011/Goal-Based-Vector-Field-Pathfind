using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    // Twerkable
    public Vector2 _tileSize;
    public Vector2 _noOfTile;

    // Reference
    public GameObject Prefab_Tile;

    // Private
    private List<Tile> _listOfTile = new List<Tile>();
    private Tile _tileStart, _tileGoal;


    //--------------------------------------------------------------
    // Unity Function(s)
    //--------------------------------------------------------------
    private void Awake()
    {
        Init();
    }

    //--------------------------------------------------------------
    // Init(s)
    //--------------------------------------------------------------
    void Init()
    {
        int totalTile = (int)(_noOfTile.x * _noOfTile.y);
        GameObject go = null;
        float x = 0;
        float y = 0;
        Vector3 offsetToCenter = new Vector3((_noOfTile.x-1) * _tileSize.x / 2, (_noOfTile.y-1) * _tileSize.y / 2, 0);

        for(int i = 0; i < totalTile; ++i)
        {
            //--------------------------------------
            // Spawn Tile
            //--------------------------------------
            go = Instantiate(Prefab_Tile, new Vector3(x * _tileSize.x - offsetToCenter.x, y * _tileSize.y - offsetToCenter.y, 0), Quaternion.identity);
            go.transform.localScale = new Vector3(_tileSize.x, _tileSize.y, 0);

            //--------------------------------------
            // Increment X and Y for next tile
            //--------------------------------------
            ++x;

            // If reached end of row, 
            if (x % _noOfTile.x == 0)
            {
                ++y;
                x = 0;
            }


            //--------------------------------------
            // Adjust Script
            //--------------------------------------
            Tile tile = go.GetComponent<Tile>();
            if(tile)
            {
                tile.SetAll(TileType.EMPTY, i, 0, _tileSize);
                _listOfTile.Add(tile);
            }
        }


        // Manual Change TileType to obstacle
        MakeBorderObstacle();
        MakeMiddleObstacle();
        MakeMiddleLeaveGap(2);
        MakeStartAndGoalAuto();
    }

    //--------------------------------------------------------------
    // Tile Plotting(s)
    //--------------------------------------------------------------
    void MakeBorderObstacle()
    {
        for(int i = 0; i < _listOfTile.Count; ++i)
        {
            if (
                (i % _noOfTile.x == 0) ||                       // First Column
                (i % _noOfTile.x == _noOfTile.x - 1) ||         // Last  Column
                ((int)(i / _noOfTile.x) == 0) ||                // First Row
                ((int)(i / _noOfTile.x) == _noOfTile.y - 1)     // Last  Row
              )
            {
                _listOfTile[i].SetType(TileType.OBSTACLE);
                _listOfTile[i].SetDCost(-1);
            }
        }
    }

    void MakeMiddleObstacle()
    {
        int middle = (int)(_noOfTile.x / 2);

        for (int i = 0; i < _listOfTile.Count; ++i)
        {
            if (i % _noOfTile.x == middle)
            {
                _listOfTile[i].SetType(TileType.OBSTACLE);
                _listOfTile[i].SetDCost(-1);
            }
        }
    }

    void MakeMiddleLeaveGap(int gap)
    {
        int middle = (int)(_noOfTile.x / 2);

        for (int i = 0; i < _listOfTile.Count; ++i)
        {
            // If at border, skip
            if(
                ((int)(i / _noOfTile.x) == 0) ||                // First Row
                ((int)(i / _noOfTile.x) == _noOfTile.y - 1)     // Last  Row
              )
            {
                continue;
            }


            // If at middle, we see whether we can make a gap
            if (i % _noOfTile.x == middle)
            {
                if (gap > 0)
                {   
                    --gap;
                    _listOfTile[i].SetType(TileType.EMPTY);
                    _listOfTile[i].SetDCost(0);
                }
                else
                {
                    _listOfTile[i].SetType(TileType.OBSTACLE);
                    _listOfTile[i].SetDCost(-1);
                }
            }
        }
    }

    void MakeStartAndGoalAuto()
    {
        List<Tile> listEmptyTile = new List<Tile>();

        for(int i = 0; i < _listOfTile.Count; ++i)
        {
            if(_listOfTile[i].GetTileType() == 0)
            {
                listEmptyTile.Add(_listOfTile[i]);
            }
        }

        int randomIndex = Random.Range(0, listEmptyTile.Count);
        _tileStart = listEmptyTile[randomIndex];
        _tileStart.SetType(TileType.START);
        listEmptyTile.RemoveAt(randomIndex);

        randomIndex = Random.Range(0, listEmptyTile.Count);
        _tileGoal = listEmptyTile[randomIndex];
        _tileGoal.SetType(TileType.GOAL);
        listEmptyTile.RemoveAt(randomIndex);

    }

    //--------------------------------------------------------------
    // Algorithm
    //--------------------------------------------------------------
    void CreateHeatMapGeneration()
    {
        List<Tile> openList = new List<Tile>();
        int i = 0;
        Tile tempTile = null;

        //-- Add first index in first
        openList.Add(_tileGoal);

        while(openList.Count > 0)
        {
            // Get 1 of the current node from openlist
            tempTile = openList[openList.Count - 1];

            // Check left of current Node
            Find_Available_Neighbour_Nodes(tempTile.GetListID());

            // Remove current node from openlist
            openList.Remove(tempTile);
        }
    }
    void Find_Available_Neighbour_Nodes(int listID)
    {
    }
    
}
