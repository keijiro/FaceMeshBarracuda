using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    bool isMenuVisible = false;
    Texture2D invisibleCursor;

    // Start is called before the first frame update
    void Start()
    {
        invisibleCursor = new Texture2D(1, 1,TextureFormat.RGBA32,false);
        invisibleCursor.SetPixel(0, 0, Color.clear);

        SetCursorHide();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isMenuVisible)
        {
            if (Input.GetMouseButtonDown(0))
            {
                SetMenuVisible();
            }
        }
    }

    public void SetMenuVisible()
    {
        isMenuVisible = true;

        SetCursorVisible();

    }

    public void SetMenuHide()
    {
        isMenuVisible = false;

        SetCursorHide();
    }

    void SetCursorVisible()
    {
        // Show the cursor
        Cursor.SetCursor(null, new Vector2(0, 0), CursorMode.Auto);
    }

    void SetCursorHide()
    {
        // Hide the cursor
        Cursor.SetCursor(invisibleCursor, new Vector2(0, 0), CursorMode.Auto);
    }
}
