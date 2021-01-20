using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SelectOption : MonoBehaviour
{
    public Color onColor;
    public Color offColor;

    [SerializeField]
    Image backgroundImage;

    [SerializeField]
    Image image;

    public SelectionGroup group;
    public int id;

    public bool IsOn { get; private set; }

    public void SetOn(bool isOn) {
        backgroundImage.color = isOn ? onColor : offColor;
        IsOn = isOn;
    }

    public void SetSprite(Sprite sprite) {
        image.sprite = sprite;
    }

    public void OnClick()
    {
        group.OnToggle(id);
    }
}
