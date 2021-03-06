using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public interface ICellGroupEventGenerator : IWorldEventGenerator
{
    bool CanAssignEventTypeToGroup(CellGroup group);
    CellGroupEvent GenerateAndAssignEvent(CellGroup group);

    string GetEventSetFlag();
}
