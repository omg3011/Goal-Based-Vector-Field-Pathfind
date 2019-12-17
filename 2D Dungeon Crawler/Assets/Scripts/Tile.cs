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
    TileType        _type;                                  // What is the Tile Type?
    int             _listID;                                // What is it's index in the list?        
    int             _dCost;                                 // What is the d cost?
    bool            _visited;                               // Have this tile been visited?

    // Cache References
    SpriteRenderer  _sr;

    // Gizmo
    private Vector3 _gizmo_Offset_DCost;                    // What is offset for gizmo dCost?
    private Color   _gizmo_Color_DCost = Color.black;       // What is color for gizmo dCost?
    private GUIStyle _gizmo_Style_DCost = new GUIStyle();


    //--------------------------------------------------------------
    // Unity(s)
    //--------------------------------------------------------------
    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }
    private void OnDrawGizmos()
    {
        Handles.Label(transform.position + _gizmo_Offset_DCost, _dCost.ToString(), _gizmo_Style_DCost);
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

        //-- Gizmo Data
        _gizmo_Offset_DCost = new Vector3(tileSize.x / 8, tileSize.y / 3, 0);
        _gizmo_Style_DCost.normal.textColor = _gizmo_Color_DCost;
        _gizmo_Style_DCost.fontSize = (int)(tileSize.x / 3);

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

    //--------------------------------------------------------------
    // Shortcut(s)
    //--------------------------------------------------------------
    Color ChooseTileColor(TileType type)
    {
        if (type == TileType.EMPTY) return Color.white;
        else if (type == TileType.OBSTACLE) return Color.yellow;
        else if (type == TileType.START) return Color.blue;
        else if (type == TileType.GOAL) return Color.red;

        return Color.black;
    }
}
