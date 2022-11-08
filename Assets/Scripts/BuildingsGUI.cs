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
        Faction faction = input.faction;

        Event e = Event.current;
        bool RMB = e.type == EventType.MouseUp && e.button == 1;

        //windowRect.height = 10;

        Label("Money: " + faction.money);

        if (Button("MOAR MONEY!"))
            faction.money += 12;

        Label("Buildings:");

        bool guiDisabled = faction.buildingBeingConstructed || faction.buildingPrefabBeingPlaced;

        if (guiDisabled)
            GUI.enabled = false;

        foreach (var building in faction.unitsDatabase.buildings)
        {


            bool beingConstructed = building == faction.buildingBeingConstructed;
            bool finished = building == faction.buildingPrefabBeingPlaced;

            string buttonText = building.name;
            if (beingConstructed)
            {
                int percent = Mathf.FloorToInt(faction.buildingProgress / building.constructable.timeToBuild * 100.0f);
                buttonText += " " + percent + "%";
            }
            else if (finished)
                buttonText += ": DONE";

            if (Button(buttonText))
            {
                faction.StartConstructingBuilding(building);
            }
        }

        if (guiDisabled)
            GUI.enabled = true;


        // Units

        bool unitsProduced = faction.unitBeingProduced;

        Label("Units:");

        if (faction.constructableUnits.Count > 0)
        {
            //if (unitsProduced)
            //    GUI.enabled = false;


            foreach (var unit in faction.constructableUnits)
            {
                string unitStr = unit.name;

                int counter = 0;
                foreach (var unitInQueue in faction.unitBuildQueue)
                {
                    if (unitInQueue == unit)
                        counter++;
                }

                string percentStr = "";
                if (faction.unitBeingProduced == unit)
                {
                    counter++;
                    int percent = Mathf.FloorToInt(faction.unitProgress / unit.constructable.timeToBuild * 100.0f);
                    percentStr = " " + percent + "%";
                }

                if (counter > 0)
                {
                    unitStr += ", producing: " + counter + percentStr;
                }

                if (Button(unitStr))
                {
                    if (!RMB)
                        faction.EnqueueUnit(unit);
                    else
                    {
                        faction.DequeueUnit(unit);
                    }
                }
            }

            //if (unitsProduced)
            //    GUI.enabled = true;
        }

        Label("Unit Queue:");

        foreach (var unit in faction.unitBuildQueue)
        {
            Label(unit.name);
        }

    }
}
