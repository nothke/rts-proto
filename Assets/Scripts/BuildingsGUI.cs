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
        //windowRect.height = 10;

        Label("Money: " + input.money);

        if (Button("MOAR MONEY!"))
            input.money += 12;

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

        if (input.constructableUnits.Count > 0)
        {
            //if (unitsProduced)
            //    GUI.enabled = false;





            foreach (var unit in input.constructableUnits)
            {
                string unitStr = unit.name;

                int counter = 0;
                foreach (var unitInQueue in input.unitBuildQueue)
                {
                    if (unitInQueue == unit)
                        counter++;
                }

                string percentStr = "";
                if (input.unitBeingProduced == unit)
                {
                    counter++;
                    int percent = Mathf.FloorToInt(input.unitProgress / unit.constructable.timeToBuild * 100.0f);
                    percentStr = " " + percent + "%";
                }

                if (counter > 0)
                {
                    unitStr += ", producing: " + counter + percentStr;
                }

                if (Button(unitStr))
                {
                    input.EnqueueUnit(unit);
                }
            }

            //if (unitsProduced)
            //    GUI.enabled = true;
        }

        Label("Unit Queue:");

        foreach (var unit in input.unitBuildQueue)
        {
            Label(unit.name);
        }

    }
}
