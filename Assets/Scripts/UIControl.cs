using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIControl : MonoBehaviour
{
    public void SwitchWindow()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
