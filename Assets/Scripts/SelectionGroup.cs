using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SelectionGroup : MonoBehaviour
{
    public bool controlWithNumKeys = false;
    public int currentlySelected = 0;
    public SelectOption selectOptionPrefab;
    public bool canSelectNothing;
    int numSelectOptions = 0;
    List<SelectOption> selectOptions = new List<SelectOption>();

    private void Update()
    {
        if (controlWithNumKeys) {
            if (Input.inputString != "")
            {
                int number;
                bool isNum = Int32.TryParse(Input.inputString, out number);
                if (isNum && number >= 0 && number <= numSelectOptions)
                {
                    number = number == 0 ? 10 : (number - 1);
                    OnToggle(number);
                }
            }
        }
    }

    public void OnToggle(int id)
    {
        bool setOn = canSelectNothing ? !selectOptions[id].IsOn : true;
        foreach (SelectOption option in selectOptions)
        {
            option.SetOn(setOn && option.id == id);
        }
        if (setOn) {
            currentlySelected = id;
        }
        else
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
}
