using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager2 : MonoBehaviour
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
        tile.SetAll(TileType.EMPTY, i, 0, _tileSize);
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
    _tileGoal = _listOfTile[_listOfTile.Count / 2 + 2];//listEmptyTile[randomIndex];
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
        for (int i = 0; i < tempSize; ++i)
        {
          if (_visitedList[i].GetListID() == newID)
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
  List<int> GetAdjacentIndices(int listID, int x, int y)
  {
    // Temp Variable
    List<int> result = new List<int>();
    int tempID = 0;

    // Search L
    if (x - 1 >= 0)
    {
      tempID = ConvertArrayToListIndex(x - 1, y);
      if (_listOfTile[tempID].GetTileType() != TileType.OBSTACLE)
        result.Add(tempID);
    }

    // Search R
    if (x + 1 <= _noOfTile.x - 1)
    {
      tempID = ConvertArrayToListIndex(x + 1, y);
      if (_listOfTile[tempID].GetTileType() != TileType.OBSTACLE)
        result.Add(tempID);
    }

    // Search Btm
    if (y - 1 >= 0)
    {
      tempID = ConvertArrayToListIndex(x, y-1);
      if (_listOfTile[tempID].GetTileType() != TileType.OBSTACLE)
        result.Add(tempID);
    }

    // Search Top
    if (y + 1 <= _noOfTile.y - 1)
    {
      tempID = ConvertArrayToListIndex(x, y+1);
      if (_listOfTile[tempID].GetTileType() != TileType.OBSTACLE)
        result.Add(tempID);
    }

    return result;
  }

  void CreateVectorFieldGeneration()
  {
    //----------------------------------------------------
    // Temp Variables 
    //----------------------------------------------------
    int tempSize = _listOfTile.Count;
    int x = 0;
    int y = 0;
   
    for(int i = 0; i < tempSize; ++i)
    {
      // Get x and y and list index
      x = (int)(i % _noOfTile.x);
      y = (int)(i / _noOfTile.x);

      // Get Neighbours of current node
      List<int> neighbourList = GetAdjacentIndices(i, x, y);
      int neighbourCount = neighbourList.Count;

      // Find lowest neighbour
      int lowestNeighbourIndex = 0;
      int lowestNeighbourValue = 65535;
      for(int j = 0; j < neighbourCount; ++j)
      {
        int check_id = neighbourList[j];
        if (_listOfTile[check_id].GetDCost() < lowestNeighbourValue)
        {
          lowestNeighbourValue = _listOfTile[check_id].GetDCost();
          lowestNeighbourIndex = check_id;
        }
      }

      // Set direction to lowest neighbour
      int lowestX = (int)(lowestNeighbourIndex % _noOfTile.x);
      int lowestY = (int)(lowestNeighbourIndex / _noOfTile.x);
      _listOfTile[i].debugMessage_top = lowestX.ToString() + " - " + x.ToString();
      _listOfTile[i].debugMessage_btm = lowestY.ToString() + " - " + y.ToString();
      _listOfTile[i].SetDirection(new Vector3(lowestX-x, lowestY-y, 0).normalized);
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
