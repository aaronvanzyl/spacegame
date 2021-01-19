using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class SelectOption : MonoBehaviour
{

    [SerializeField]
    Image image;

    public SelectionGroup group;
    public int id;

    Toggle toggle;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
    }

    public void SetOn(bool isOn) {
        toggle.isOn = isOn;
        if (isOn)
        {
            toggle.OnSelect(null);
        }
        else {
            toggle.OnDeselect(null);
        }
    }

    public void SetSprite(Sprite sprite) {
        image.sprite = sprite;
    }

    public void OnToggle(bool isActive)
    {
        group.OnToggle(id, isActive);
    }
}
