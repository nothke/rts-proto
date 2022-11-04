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
        Label("Money: " + input.money);

        Label("Buildings:");

        bool guiDisabled = input.buildingBeingConstructed || input.buildingPrefabBeingPlaced;

        if (guiDisabled)
            GUI.enabled = false;

        foreach (var building in input.unitsDatabase.buildings)
        {
            bool beingConstructed = building == input.buildingBeingConstructed;
            bool finished = building == input.buildingPrefabBeingPlaced;

            string buttonText = building.name;
            if (beingConstructed)
                buttonText += ": " + input.buildingProgress.ToString("F2");
            else if (finished)
                buttonText += ": DONE";

            if (Button(buttonText))
            {
                input.StartConstructingBuilding(building);
            }
        }

        if (guiDisabled)
            GUI.enabled = true;


        // Units

        bool unitsProduced = input.unitBeingProduced;

        Label("Units:");

        if (input.activeBarracks)
        {
            if (unitsProduced)
                GUI.enabled = false;

            foreach (var unit in input.unitsDatabase.units)
            {
                if (Button(unit.name))
                {
                    input.ProduceUnit(unit);
                }
            }

            if (unitsProduced)
                GUI.enabled = true;
        }
    }
}
