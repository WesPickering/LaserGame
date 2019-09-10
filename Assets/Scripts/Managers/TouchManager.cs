using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchManager : MonoBehaviour
{
    #region References
    Collider2D last_coll;

    Collider2D coll;

    LayerMask hittable;

    Touch touch;

    [SerializeField]
    float buffer_time;

    float touch_timer;

    bool isSelected;

    Manipulator curr_selected_obj;

    BoardManager bm;
    #endregion

    private void Start()
    {
        hittable = LayerMask.NameToLayer("Hittable");

        bm = GameObject.FindWithTag("BoardManager").GetComponent<BoardManager>();
    }

    void Update()
    {
        if (bm.gameover)
        {
            return;
        }
        CheckTouch();
        CheckMouse();
        return;
    }

    //Recreatee drag handler based on mouse
    void CheckMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            coll = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (coll == null)
            {
                return;
            }

            if (coll.CompareTag("Manipulator"))
            {
                curr_selected_obj = coll.GetComponent<Manipulator>();
            }
            touch_timer = buffer_time;
            isSelected = false;
        }

        if (Input.GetMouseButton(0))
        {
            if (curr_selected_obj == null)
            {
                return;
            }
            touch_timer -= Time.deltaTime;
            //Start the drag mechanic
            if (touch_timer <= 0 && !isSelected)
            {
                isSelected = true;
                curr_selected_obj.StartDrag();
            }
            else if (touch_timer <= 0 && isSelected)
            {
                curr_selected_obj.Drag();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (curr_selected_obj == null)
            {
                return;
            }
            //Case 1 : Tap
            if (touch_timer >= 0)
            {
                curr_selected_obj.Rotate_Right();
            }

            //Case 2 : Drag and release
            if (touch_timer <= 0)
            {
                curr_selected_obj.EndDrag();
            }

            curr_selected_obj = null;
        }
    }

    //Recreate drag handler based on touchphase
    void CheckTouch()
    {
        if (Input.touchCount > 0)
        {
            touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                coll = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(touch.position));
                if (coll == null)
                {
                    return;
                }

                if (coll.CompareTag("Manipulator"))
                {
                    curr_selected_obj = coll.GetComponent<Manipulator>();
                }
                touch_timer = buffer_time;
                isSelected = false;
            }

            if (touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved)
            {
                if (curr_selected_obj == null)
                {
                    return;
                }
                touch_timer -= Time.deltaTime;

                //Start the drag mechanic
                if (touch_timer <= 0 && !isSelected)
                {
                    isSelected = true;
                    curr_selected_obj.StartDrag();
                }
                else if (touch_timer <= 0 && isSelected)
                {
                    curr_selected_obj.Drag();
                }
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                if (curr_selected_obj == null)
                {
                    return;
                }
                //Case 1 : Tap
                if (touch_timer >= 0 )
                {
                    curr_selected_obj.Rotate_Right();
                }

                //Case 2 : Drag and release
                if (touch_timer <= 0)
                {
                    curr_selected_obj.EndDrag();
                }

                curr_selected_obj = null;
            }


        }
    }

}
