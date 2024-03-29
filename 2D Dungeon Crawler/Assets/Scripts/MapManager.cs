﻿using System.Collections;
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


  private List<Tile> _openList = new List<Tile>();
  private List<Tile> _visitedList = new List<Tile>();


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
    Vector3 offsetToCenter = new Vector3((_noOfTile.x - 1) * _tileSize.x / 2, (_noOfTile.y - 1) * _tileSize.y / 2, 0);

    for (int i = 0; i < totalTile; ++i)
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
      if (tile)
      {
        tile.SetAll(TileType.EMPTY, i, 65535, _tileSize);
        _listOfTile.Add(tile);
      }
    }


    // Manual Change TileType to obstacle
    MakeBorderObstacle();
    MakeCustomMiddleObstacle();
    //MakeMiddleObstacle();
    //MakeMiddleLeaveGap(2);
    MakeStartAndGoalAuto();

    CreateHeatMapGeneration();
    CreateVectorFieldGeneration();
  }

  //--------------------------------------------------------------
  // Tile Plotting(s)
  //--------------------------------------------------------------
  void MakeBorderObstacle()
  {
    for (int i = 0; i < _listOfTile.Count; ++i)
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

  void MakeCustomMiddleObstacle()
  {
    int middle = (int)(_noOfTile.x / 2);

    for (int i = 0; i < _listOfTile.Count; ++i)
    {
      if (i % _noOfTile.x == middle)
      {
        int x = (int)(i % _noOfTile.x);
        int y = (int)(i / _noOfTile.x);

        if (y == 1 || y == _noOfTile.y - 2)
          continue;

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
      if (
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

    for (int i = 0; i < _listOfTile.Count; ++i)
    {
      if (_listOfTile[i].GetTileType() == 0)
      {
        listEmptyTile.Add(_listOfTile[i]);
      }
    }

    int randomIndex = 21;//Random.Range(0, listEmptyTile.Count);
    _tileStart = listEmptyTile[randomIndex];
    _tileStart.SetType(TileType.START);
    listEmptyTile.RemoveAt(randomIndex);

    randomIndex = Random.Range(0, listEmptyTile.Count);
    _tileGoal = _listOfTile[_listOfTile.Count/2 + 2];//listEmptyTile[randomIndex];
    _tileGoal.SetType(TileType.GOAL);
    listEmptyTile.RemoveAt(randomIndex);

  }

  //--------------------------------------------------------------
  // Heatmap Generation Algorithm
  //--------------------------------------------------------------
  void CreateHeatMapGeneration()
  {
    _openList = new List<Tile>();
    _visitedList = new List<Tile>();
    int i = 0;
    Tile tempTile = null;

    //-- Add first tile in first
    _openList.Add(_tileGoal);
    _visitedList.Add(_tileGoal);

    //-- Loop until nothing left in openList
    while (_openList.Count > 0)
    {
      // Get next openlist cell
      tempTile = _openList[0];
      _visitedList.Add(tempTile);

      // Check left/right/top/bottom of current Node, if not visited add to openList
      Find_Available_Neighbour_Nodes(tempTile.GetDCost(), tempTile.GetListID());

      // Remove current node from openlist
      _openList.Remove(tempTile);
    }
  }
  void Find_Available_Neighbour_Nodes(int prev_dCost, int listID)
  {
    int x = (int)(listID % _noOfTile.x);
    int y = (int)(listID / _noOfTile.x);
    int newID = 0;
    int tempSize = 0;
    bool visited = false;


    for (int xx = -1; xx <= 1; ++xx)
    {
      for (int yy = -1; yy <= 1; ++yy)
      {
        // Don't check, out of boundary or myself
        if (
            (xx == 0 && yy == 0) ||                     // Myself
            (x + xx < 0 || x + xx > _noOfTile.x - 1) || // x boundary
            (y + yy < 0 || y + yy > _noOfTile.y - 1)    // y boundary
           // (xx == -1 && yy == -1) ||                   // Reject diagonal check
           // (xx == -1 && yy == 1)  ||                   // Reject diagonal check
           // (xx == 1 && yy == -1)  ||                   // Reject diagonal check
           // (xx == 1 && yy == 1)                        // Reject diagonal check
          )
        {
          continue;
        }

        // Convert the next check position to listIndex
        newID = ConvertArrayToListIndex(x + xx, y + yy);

        // Check if is obstacle, we continue;
        if (_listOfTile[newID].GetTileType() == TileType.OBSTACLE)
          continue;

        // Check if visited
        tempSize = _visitedList.Count;
        visited = false;
        for(int i = 0; i < tempSize; ++i)
        {
          if(_visitedList[i].GetListID() == newID)
          {
            visited = true;
            break;
          }
        }

        // If not visited, add this to openlist
        if (!visited)
        {
          _listOfTile[newID].SetDCost(prev_dCost + 1);
          _openList.Add(_listOfTile[newID]);
          _visitedList.Add(_listOfTile[newID]);
        }
      }
    }
  }

  //--------------------------------------------------------------
  // Vector Field Generation Algorithm
  //--------------------------------------------------------------
  enum VectorFieldDirection
  {
    LEFT,
    RIGHT,
    TOP,
    BOTTOM,
    TOTAL
  }
  int Get_NeighbourDCost_VectorField(int x, int y, int listID, VectorFieldDirection dir)
  {
    int resultDCost = 0;
    int newListID = 0;
    //--------------------------------------------
    // Left
    //--------------------------------------------
    if (dir == VectorFieldDirection.LEFT)
    {
      // Check if left is out of boundary, use current tile for dCost
      if(x - 1 < 0)
      {
        resultDCost = _listOfTile[listID].GetDCost();
      }
      // Check if left is within boundary, use Left tile dCost
      else
      {
        // Get listID of LEFT tile
        newListID = ConvertArrayToListIndex(x - 1, y);

        // Check if tile type is walkable, get DCost of left tile of the current tile
        if (_listOfTile[newListID].GetTileType() != TileType.OBSTACLE)
          resultDCost = _listOfTile[newListID].GetDCost();

        // Check if tile type is obstacle, use current tile for dCost
        else
          resultDCost = _listOfTile[listID].GetDCost();
      }
    }

    //--------------------------------------------
    // RIGHT
    //--------------------------------------------
    else if (dir == VectorFieldDirection.RIGHT)
    {
      // Check if right is out of boundary, use current tile for dCost
      if (x + 1 > _noOfTile.x - 1)
      {
        resultDCost = _listOfTile[listID].GetDCost();
      }
      // Check if right is within boundary,
      else
      {
        // Get listID of RIGHT tile
        newListID = ConvertArrayToListIndex(x + 1, y);

        // Check if tile type is walkable, get DCost of right tile of the current tile
        if (_listOfTile[newListID].GetTileType() != TileType.OBSTACLE)
          resultDCost = _listOfTile[newListID].GetDCost();

        // Check if tile type is obstacle, use current tile for dCost
        else
          resultDCost = _listOfTile[listID].GetDCost();
      }
    }

    //--------------------------------------------
    // TOP
    //--------------------------------------------
    else if (dir == VectorFieldDirection.TOP)
    {
      // Check if TOP is out of boundary, use current tile for dCost
      if (y + 1 > _noOfTile.y - 1)
      {
        resultDCost = _listOfTile[listID].GetDCost();
      }
      // Check if TOP is within boundary,
      else
      {
        // Get listID of TOP tile
        newListID = ConvertArrayToListIndex(x, y+1);

        // Check if tile type is walkable, get DCost of TOP tile of the current tile
        if (_listOfTile[newListID].GetTileType() != TileType.OBSTACLE)
          resultDCost = _listOfTile[newListID].GetDCost();

        // Check if tile type is obstacle, use current tile for dCost
        else
          resultDCost = _listOfTile[listID].GetDCost();
      }
    }

    //--------------------------------------------
    // BOTTOM
    //--------------------------------------------
    else if (dir == VectorFieldDirection.BOTTOM)
    {
      // Check if BOTTOM is out of boundary, use current tile for dCost
      if (y - 1 < 0)
      {
        resultDCost = _listOfTile[listID].GetDCost();
      }
      // Check if BOTTOM is within boundary,
      else
      {
        // Get listID of BOTTOM tile
        newListID = ConvertArrayToListIndex(x, y - 1);

        // Check if tile type is walkable, get DCost of BOTTOM tile of the current tile
        if (_listOfTile[newListID].GetTileType() != TileType.OBSTACLE)
          resultDCost = _listOfTile[newListID].GetDCost();

        // Check if tile type is obstacle, use current tile for dCost
        else
          resultDCost = _listOfTile[listID].GetDCost();//_listOfTile[ConvertArrayToListIndex(x, y + 1)].GetDCost();
      }
    }

    return resultDCost;
  }

  void CreateVectorFieldGeneration()
  {
    int tempSize = _listOfTile.Count;
    int x = 0;
    int y = 0;
    int listID = 0;
    float L_dist, R_dist, Top_dist, Bottom_dist;

    for (int i = 0; i < tempSize; ++i)
    {
      // Get x and y and list index
      x = (int)(i % _noOfTile.x);
      y = (int)(i / _noOfTile.x);
      listID = i;

      //----------------------------------------------------
      // Get dCost #If adj tile is non-walker, use current 
      //----------------------------------------------------
      L_dist = Get_NeighbourDCost_VectorField(x, y, listID, VectorFieldDirection.LEFT);
      R_dist = Get_NeighbourDCost_VectorField(x, y, listID, VectorFieldDirection.RIGHT);
      Bottom_dist = Get_NeighbourDCost_VectorField(x, y, listID, VectorFieldDirection.BOTTOM);
      Top_dist = Get_NeighbourDCost_VectorField(x, y, listID, VectorFieldDirection.TOP);

      //----------------------------------------
      // Update the new vector field direction
      //----------------------------------------
      _listOfTile[i].debugMessage_top = L_dist.ToString() + " - " + R_dist.ToString();
      _listOfTile[i].debugMessage_btm = Top_dist.ToString() + " - " + Bottom_dist.ToString();
      _listOfTile[i].SetDirection(new Vector3(L_dist - R_dist, Top_dist - Bottom_dist).normalized);
    }
  }

  //--------------------------------------------------------------
  // Converter
  //--------------------------------------------------------------
  Vector2 ConvertListToArrayCoordinate(int listID)
  {
    return new Vector2((int)(listID % _noOfTile.x), (int)(listID / _noOfTile.x));
  }
  int ConvertArrayToListIndex(int x, int y)
  {
    return (int)(y * _noOfTile.x + x);
  }


  //--------------------------------------------------------------
  // Getter(s)
  //--------------------------------------------------------------

}
