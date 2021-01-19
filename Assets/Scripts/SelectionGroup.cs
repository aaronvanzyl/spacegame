using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ToggleGroup))]
public class SelectionGroup : MonoBehaviour
{
    public bool controlWithNumKeys = false;
    public int currentlySelected = -1;
    public SelectOption selectOptionPrefab;
    int numSelectOptions = 0;
    List<SelectOption> selectOptions = new List<SelectOption>();

    ToggleGroup toggleGroup;

    private void Awake()
    {
        toggleGroup = GetComponent<ToggleGroup>();
    }

    private void Update()
    {
        if (controlWithNumKeys) {
            if (Input.inputString != "")
            {
                int number;
                bool isNum = Int32.TryParse(Input.inputString, out number);
                if (isNum && number >= 0 && number <= 9)
                {
                    number = number == 0 ? 10 : (number - 1);
                    Select(number);
                }
            }
        }
    }

    public void OnToggle(int id, bool isActive)
    {
        if (isActive) {
            Select(id);
        }
        else if (currentlySelected == id)
        {
            currentlySelected = -1;
        }
    }

    public void AddSelectOption(Sprite sprite)
    {
        SelectOption option = Instantiate(selectOptionPrefab, transform);
        selectOptions.Add(option);
        option.SetSprite(sprite);
        option.id = numSelectOptions;
        option.group = this;
        numSelectOptions++;
    }

    public void Select(int index)
    {
        currentlySelected = index;
        foreach (SelectOption option in selectOptions) {
            option.SetOn(option.id == currentlySelected);
        }
    }
}
