using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Nothke.ProtoGUI;

public class BuildingsGUI : WindowGUI
{
    public override string WindowLabel => "Buildings";

    PlayerInput _input;
    PlayerInput input { get { if (!_input) _input = GetComponent<PlayerInput>(); return _input; } }

    protected override void Window()
    {
        foreach (var building in input.unitsDatabase.buildings)
        {
            if (Button(building.name))
            {
                input.SetBuildingForPlacing(building);
            }
        }
    }
}
