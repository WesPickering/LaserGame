using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIItemScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    #region UI Values
    SpriteRenderer button_spr;

    [SerializeField]
    Text count_text;
    #endregion

    #region Drag Variables
    //Manipulator spawned when dragging this iconx
    GameObject manipulator_obj;

    //Max Uses of manipulator
    int num_charges;

    //The current object that is being dragged into the scene
    GameObject curr_obj_selected;

    //Sprite of thee current object
    SpriteRenderer obj_spr;

    //used to store current mouse position
    Vector3 mouse_world_pos;

    //ints for indexing into the board
    int x;
    int y;
    BoardManager.tile t;
    #endregion

    public void InitializeUI( GameObject obj, int num_uses)
    {
        manipulator_obj = obj;

        num_charges = num_uses;
        count_text.text = num_charges.ToString();

        GetComponent<Image>().sprite = manipulator_obj.GetComponent<SpriteRenderer>().sprite;
        GetComponent<Image>().color = manipulator_obj.GetComponent<SpriteRenderer>().color;
    }


    #region Drag Functions
    //Called on first click
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (num_charges <= 0)
        {
            return;
        }
        //Create copy of manipulator
        curr_obj_selected = Instantiate(manipulator_obj, Camera.main.ScreenToWorldPoint(Input.mousePosition), transform.rotation);
        obj_spr = curr_obj_selected.GetComponent<SpriteRenderer>();
        //position manipulator correctly under mouse
        mouse_world_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse_world_pos.z = 0f;
        curr_obj_selected.transform.position = mouse_world_pos;
    }

    //called every frame while mousebutton is held down
    public void OnDrag(PointerEventData eventData)
    {
        if (curr_obj_selected == null)
        {
            return;
        }
        mouse_world_pos = Camera.main.ScreenToWorldPoint(eventData.position);

        x = Mathf.FloorToInt((Camera.main.ScreenToWorldPoint(Input.mousePosition).x - BoardManager.static_start_loc.x) * 100);
        x = Mathf.FloorToInt(x / (BoardManager.offset.x * 100));
        y = Mathf.FloorToInt((Camera.main.ScreenToWorldPoint(Input.mousePosition).y - BoardManager.static_start_loc.y) * -100);
        y = Mathf.FloorToInt(y / (BoardManager.offset.y * 100));
        if (x < 0 || x > BoardManager.board.GetLength(1) -1 || y < 0 || y > BoardManager.board.GetLength(0) - 1)
        {
            return;
        }
        t = BoardManager.board[y, x];
        curr_obj_selected.transform.position = t.center;

        if (t.available)
        {
            obj_spr.color = Color.green;
        }
        else if (!t.available)
        {
            obj_spr.color = Color.red;
        }
    }

    //called on release of mouse button
    public void OnEndDrag(PointerEventData eventData)
    {


        if (curr_obj_selected == null)
        {
            return;
        }

        obj_spr.color = Color.white;

        if (x < 0 || x > BoardManager.board.GetLength(1) - 1 || y < 0 || y > BoardManager.board.GetLength(0) - 1 || !t.available)
        {
            Destroy(curr_obj_selected);
        }
        else
        {
            BoardManager.board[y, x].tile_obj = curr_obj_selected;
            BoardManager.board[y, x].available = false;

            curr_obj_selected.GetComponent<Manipulator>().UI_Item = this;
            curr_obj_selected.GetComponent<Manipulator>().Activate_Manipulator();

            num_charges -= 1;
            count_text.text = num_charges.ToString();
        }
        curr_obj_selected = null;
        obj_spr = null;
    }
    #endregion


    public void IncrementUseCount(int x)
    {
        num_charges += x;
        count_text.text = num_charges.ToString();
    }

}
