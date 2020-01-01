using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager3 : MonoBehaviour
{
  // Twerkable
  public Vector2 _tileSize;
  public Vector2 _noOfTile;

  // Reference
  public GameObject Prefab_Tile;

  // Private
  private List<Tile> _listOfTile = new List<Tile>();
  private Tile _tileStart;
  private List<Tile> _listOfGoal = new List<Tile>();


  private List<Tile> _openList = new List<Tile>();
  private List<Tile> _visitedList = new List<Tile>();

  // Time
  public float _frequency = 3.0f;
  private float _nextFrequencyFinish;

  // Debug
  Transform _playerT;

  [HideInInspector]
  public bool bFoundPath = false;


  //--------------------------------------------------------------
  // Unity Function(s)
  //--------------------------------------------------------------
  private void Awake()
  {
    Init();
    _playerT = GameObject.FindGameObjectWithTag("Player").transform;
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
    //MakeStartAndGoalAuto();

    //CreateHeatMapGeneration();
    //CreateVectorFieldGeneration();
    _nextFrequencyFinish = Time.time + _frequency;
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

    _listOfTile[20].SetType(TileType.OBSTACLE);
    _listOfTile[20].SetDCost(-1);
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

    CreateFourGoal(33);
  }

  //--------------------------------------------------------------
  // Heatmap Generation Algorithm
  //--------------------------------------------------------------
  void CreateHeatMapGeneration()
  {
    _openList = new List<Tile>();
    _visitedList = new List<Tile>();
    Tile tempTile = null;

    //-- Find Goal
    int playerListID = ConvertWorldToListID_W_ModifiedCenter(_playerT.position);
    if(!CreateFourGoal(playerListID))
    {
      Debug.LogError("Player Out of Range");
      return;
    }

    //-- Add all goal tile in first
    int i = 0;
    for (i = 0; i < _listOfGoal.Count; ++i)
    {
      _openList.Add(_listOfGoal[i]);
      _visitedList.Add(_listOfGoal[i]);
    }

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
  // Heatmap Generation Algorithm (Local Optima)
  //--------------------------------------------------------------
  enum HeatmapPattern
  {
    PATTERN_1,      // (C, R, T, TR)
    PATTERN_2,      // (C, R, B, BR)
    PATTERN_3,      // (C, L, T, TL)
    PATTERN_4       // (C, L, B, BL)
  }

  bool CreateFourGoal(int listID)
  {
    if(_listOfGoal.Count > 0)
    {
      for (int i = 0; i < _listOfGoal.Count; ++i)
        _listOfGoal[i].SetType(TileType.EMPTY);
    }

    _listOfGoal = new List<Tile>();

    // Get x and y of list index
    int x = (int)(listID % _noOfTile.x);
    int y = (int)(listID / _noOfTile.x);

    //----------------------------------------
    // Check current first, 
    //----------------------------------------
    if (Check_If_Array_Within_Boundary(x, y))
    {
      SetTileTypeToGoal(ConvertArrayToListIndex(x, y));
    }
    // If no current, break
    else
    {
      return false;
    }

    
    // Find which pattern to use
    //-- Pattern 1 (C, R, T, TR)
    if(
        Check_If_Array_Within_Boundary(x+1, y) &&
        Check_If_Array_Within_Boundary(x, y+1) &&
        Check_If_Array_Within_Boundary(x+1, y+1) 
      )
    {
      Debug.Log("Pattern1");
      SetFourGoal(HeatmapPattern.PATTERN_1, x, y);
      return true;
    }

    Debug.Log("----------------------");

    //-- Pattern 2 (C, R, B, BR)
    if (
        Check_If_Array_Within_Boundary(x + 1, y) &&
        Check_If_Array_Within_Boundary(x, y - 1) &&
        Check_If_Array_Within_Boundary(x + 1, y - 1)
      )
    {
      Debug.Log("Pattern2");
      SetFourGoal(HeatmapPattern.PATTERN_2, x, y);
      return true;
    }

    Debug.Log("----------------------");
    //-- Pattern 3 (C, L, T, TL)
    if (
        Check_If_Array_Within_Boundary(x - 1, y) &&
        Check_If_Array_Within_Boundary(x, y + 1) &&
        Check_If_Array_Within_Boundary(x - 1, y + 1)
      )
    {
      Debug.Log("Pattern3");
      SetFourGoal(HeatmapPattern.PATTERN_3, x, y);
      return true;
    }

    Debug.Log("----------------------");

    //-- Pattern 4 (C, L, B, BL)
    if (
        Check_If_Array_Within_Boundary(x - 1, y) &&
        Check_If_Array_Within_Boundary(x, y - 1) &&
        Check_If_Array_Within_Boundary(x - 1, y - 1)
      )
    {
      Debug.Log("Pattern4");
      SetFourGoal(HeatmapPattern.PATTERN_4, x, y);
      return true;
    }

    //-- Custom (LOL shit)
    Debug.LogError("Shit need custom local optima");
    return true;
  }

  void SetFourGoal(HeatmapPattern pattern, int x, int y)
  {
    //-- Pattern 1 (C, R, T, TR)
    if (pattern == HeatmapPattern.PATTERN_1)
    {
      SetTileTypeToGoal(ConvertArrayToListIndex(x+1, y));
      SetTileTypeToGoal(ConvertArrayToListIndex(x, y+1));
      SetTileTypeToGoal(ConvertArrayToListIndex(x+1, y+1));
    }
    
    //-- Pattern 2 (C, R, B, BR)
    else if (pattern == HeatmapPattern.PATTERN_2)
    {
      SetTileTypeToGoal(ConvertArrayToListIndex(x + 1, y));
      SetTileTypeToGoal(ConvertArrayToListIndex(x, y - 1));
      SetTileTypeToGoal(ConvertArrayToListIndex(x + 1, y - 1));
    }

    //-- Pattern 3 (C, L, T, TL)
    else if (pattern == HeatmapPattern.PATTERN_3)
    {
      SetTileTypeToGoal(ConvertArrayToListIndex(x - 1, y));
      SetTileTypeToGoal(ConvertArrayToListIndex(x, y + 1));
      SetTileTypeToGoal(ConvertArrayToListIndex(x - 1, y + 1));
    }

    //-- Pattern 4 (C, L, B, BL)
    else if (pattern == HeatmapPattern.PATTERN_4)
    {
      SetTileTypeToGoal(ConvertArrayToListIndex(x - 1, y));
      SetTileTypeToGoal(ConvertArrayToListIndex(x, y - 1));
      SetTileTypeToGoal(ConvertArrayToListIndex(x - 1, y - 1));
    }
  }

  bool Check_If_Array_Within_Boundary(int x, int y)
  {
    bool result = (x >= 0 && x <= _noOfTile.x - 1 && y >= 0 && y <= _noOfTile.y - 1) 
      && (_listOfTile[ConvertArrayToListIndex(x, y)].GetTileType() != TileType.OBSTACLE);

    return result;
  }

  bool IsIndicesWithinList(int listID)
  {
    return (listID >= 0 && listID <= _listOfTile.Count - 1);
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

    bFoundPath = true;
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

  Vector3 ConvertArrayToWorldCoordinate_W_ModifiedCenter(int x, int y)
  {
    return new Vector3(x * _tileSize.x - (_noOfTile.x * _tileSize.x / 2) + _tileSize.x/2, y * _tileSize.y - (_noOfTile.y * _tileSize.y / 2) + _tileSize.y/2, 0);
  }
  Vector2 ConvertWorldToArray_W_ModifiedCenter(Vector3 worldPos)
  {
    return new Vector2((worldPos.x + (_noOfTile.x * _tileSize.x / 2)) / _tileSize.x,
                       (worldPos.y + (_noOfTile.y * _tileSize.y / 2)) / _tileSize.y);
  }
  int ConvertWorldToListID_W_ModifiedCenter(Vector3 worldPos)
  {
    Vector2 worldToArray = ConvertWorldToArray_W_ModifiedCenter(worldPos);
    return ConvertArrayToListIndex((int)worldToArray.x, (int)worldToArray.y);
  }

  //--------------------------------------------------------------
  // Getter(s)
  //--------------------------------------------------------------
  public Vector3 ConvertWorldToDirection(Vector3 worldPos)
  {
    int listID = ConvertWorldToListID_W_ModifiedCenter(worldPos);
    return _listOfTile[listID].GetDirection();
  }

  //--------------------------------------------------------------
  // Setter(s)
  //--------------------------------------------------------------
  void SetTileTypeToGoal(int listID)
  {
    _listOfTile[listID].SetType(TileType.GOAL);
    _listOfGoal.Add(_listOfTile[listID]);
  }

  void FindPath()
  {
    //-- Update the Pathfind 
    CreateHeatMapGeneration();
    CreateVectorFieldGeneration();
  }

  private void Update()
  {
    if(Time.time >= _nextFrequencyFinish)
    {
      _nextFrequencyFinish = Time.time + _frequency;
      FindPath();
    }
  }
}
