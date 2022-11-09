using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Nothke.ProtoGUI;

public class UnitInfoGUI : WindowGUI
{
    public override string WindowLabel => "Unit";

    private void Start()
    {
        windowRect.width = 200;
        windowRect.x = Screen.width;
    }

    protected override void Window()
    {


        var faction = PlayerInput.instance.faction;

        if (faction.selectedUnits.Count == 1)
        {
            var unit = faction.selectedUnits[0];

            Label("Health: " + unit.entity.hp);
            if (unit.TryGetComponent<Turret>(out var turret))
                Label("Firepower: " + turret.damage);
        }
        else
            Label("Nothing selected");
    }
}
