using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Drawing : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] GameObject brush;
    [SerializeField] Manager manager;

    LineRenderer lineRend;
    Vector2 lastPos;

    bool drawing = false;
    float timer = 0;

    public void OnDrawStart()
    {
        if (Manager.canDraw)
        {
            drawing = true;
            CreateBrush();
        }
    }

    public void OnDrawEnd()
    {
        if (Manager.canDraw)
        {
            drawing = false;

            manager.DrawEnded(lineRend, timer);
            timer = 0;
            lineRend = null;
        }
        
    }

    private void FixedUpdate()
    {
        if (Manager.canDraw)
        {
            timer += Time.deltaTime;
        }
        if (drawing)
        {
            Draw();
        }
    }

    void Draw()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        if(mousePos != lastPos)
        {
            AddPoint(mousePos);
            lastPos = mousePos;
        }
    }

    void CreateBrush()
    {
        GameObject brushInstance = Instantiate(brush);
        lineRend = brushInstance.GetComponent<LineRenderer>();

        Vector2 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        lineRend.SetPosition(0, mousePos);
        lineRend.SetPosition(1, mousePos);
    }

    void AddPoint(Vector2 pointPos)
    {
        lineRend.positionCount++;
        int positionIndex = lineRend.positionCount - 1;
        lineRend.SetPosition(positionIndex, pointPos);
    }
}
