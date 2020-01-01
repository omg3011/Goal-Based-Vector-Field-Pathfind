using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public enum TileType
{
  EMPTY,
  OBSTACLE,
  START,
  GOAL,
  TOTAL
}

public class Tile : MonoBehaviour
{
  // Private
  TileType _type;                                         // What is the Tile Type?
  int _listID;                                            // What is it's index in the list?        
  int _dCost;                                             // What is the d cost?
  bool _visited;                                          // Have this tile been visited?
  Vector3 _direction;                                     // What is the vector field direction?
  
  // Cache References
  SpriteRenderer _sr;

  // Debug
  public string debugMessage_top = "";
  public string debugMessage_btm = "";

  // Gizmo
  private Vector3 _gizmo_Offset_ListID;                   // What is offset for gizmo listID?
  private Vector3 _gizmo_Offset_DCost;                    // What is offset for gizmo dCost?
  private Color _gizmo_Color_DCost = Color.black;         // What is color for gizmo dCost?
  private GUIStyle _gizmo_Style_DCost = new GUIStyle();   // Style
  private float _gizmo_line_length = 20.0f;               // What is the length for gizmo line?
  private float _gizmo_circle_radius = 5.0f;              // What is the length for gizmo line?
  private Vector3 _gizmo_Offset_debug;                    // What is the offset for gizmo debug message?

  //--------------------------------------------------------------
  // Unity(s)
  //--------------------------------------------------------------
  private void Awake()
  {
    _sr = GetComponent<SpriteRenderer>();
  }
  private void OnDrawGizmos()
  {
    // Draw Text
    Handles.Label(transform.position + _gizmo_Offset_DCost, _dCost.ToString(), _gizmo_Style_DCost);
    //Handles.Label(transform.position + _gizmo_Offset_ListID, _listID.ToString(), _gizmo_Style_DCost);
    Handles.Label(transform.position + _gizmo_Offset_debug + new Vector3(0, 20, 0), debugMessage_top, _gizmo_Style_DCost);
    Handles.Label(transform.position + _gizmo_Offset_debug, debugMessage_btm, _gizmo_Style_DCost);

    // Draw Circle
    Gizmos.color = Color.red;
    Gizmos.DrawSphere(transform.position, _gizmo_circle_radius);

    // Draw Line
    if(_type != TileType.OBSTACLE)
    {
      Gizmos.color = Color.blue;
      Gizmos.DrawLine(transform.position, transform.position + _direction.normalized * _gizmo_line_length);
    }
  }

  //--------------------------------------------------------------
  // Setter(s)
  //--------------------------------------------------------------
  public void SetAll(TileType type, int listID, int dCost, Vector2 tileSize)
  {
    //-- Tile Data
    _type = type;
    _listID = listID;
    _sr.color = ChooseTileColor(type);
    _dCost = dCost;
    _direction = Vector3.up;

    //-- Gizmo Data
    _gizmo_Offset_DCost = new Vector3(tileSize.x / 8, tileSize.y / 3, 0);
    _gizmo_Style_DCost.normal.textColor = _gizmo_Color_DCost;
    _gizmo_Style_DCost.fontSize = (int)(tileSize.x / 6);
    _gizmo_Offset_debug = new Vector3(-10, -10, 0);
    _gizmo_Offset_ListID = new Vector3(-20, 0, 0);
  }
  public void SetType(TileType type)
  {
    _type = type;
    _sr.color = ChooseTileColor(type);
  }
  public void SetDCost(int dCost)
  {
    _dCost = dCost;
  }
  public void SetVisited(bool flag)
  {
    _visited = flag;
  }

  public void SetDirection(Vector3 dir)
  {
    _direction = dir;
  }


  //--------------------------------------------------------------
  // Getter(s)
  //--------------------------------------------------------------
  public TileType GetTileType()
  {
    return _type;
  }

  public int GetDCost()
  {
    return _dCost;
  }
  public int GetListID()
  {
    return _listID;
  }
  public bool GetVisited()
  {
    return _visited;
  }
  public Vector3 GetDirection()
  {
    return _direction;
  }

  //--------------------------------------------------------------
  // Shortcut(s)
  //--------------------------------------------------------------
  Color ChooseTileColor(TileType type)
  {
    if (type == TileType.EMPTY) return Color.white;
    else if (type == TileType.OBSTACLE) return Color.yellow;
    else if (type == TileType.START) return Color.green;
    else if (type == TileType.GOAL) return Color.red;

    return Color.black;
  }
}
