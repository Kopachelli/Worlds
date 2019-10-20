using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

[XmlType(TypeName = "CellAlt")]
public class TerrainCellAlteration
{
    [XmlAttribute("Lon")]
    public int Longitude;
    [XmlAttribute("Lat")]
    public int Latitude;

    [XmlAttribute("BA")]
    public float BaseAltitudeValue;
    [XmlAttribute("BT")]
    public float BaseTemperatureValue;
    [XmlAttribute("BR")]
    public float BaseRainfallValue;

    [XmlAttribute("BTO")]
    public float BaseTemperatureOffset;
    [XmlAttribute("BRO")]
    public float BaseRainfallOffset;

    [XmlAttribute("A")]
    public float Altitude;
    [XmlAttribute("OA")]
    public float OriginalAltitude;
    [XmlAttribute("T")]
    public float Temperature;
    [XmlAttribute("OT")]
    public float OriginalTemperature;
    [XmlAttribute("R")]
    public float Rainfall;
    [XmlAttribute("W")]
    public float WaterAccumulation;
    [XmlAttribute("RId")]
    public int RiverId = -1;
    [XmlAttribute("RL")]
    public float RiverLength = 0;

    [XmlAttribute("Fp")]
    public float FarmlandPercentage = 0;
    [XmlAttribute("Ar")]
    public float Arability = 0;
    [XmlAttribute("Acc")]
    public float Accessibility = 0;

    [XmlAttribute("M")]
    public bool Modified;

    public List<CellLayerData> LayerData = new List<CellLayerData>();

    [XmlIgnore]
    public WorldPosition Position;

    public TerrainCellAlteration()
    {
        Manager.UpdateWorldLoadTrackEventCount();
    }

    public TerrainCellAlteration(TerrainCell cell, bool addLayerData = true)
    {
        Longitude = cell.Longitude;
        Latitude = cell.Latitude;

        Position = cell.Position;

        BaseAltitudeValue = cell.BaseAltitudeValue;
        BaseTemperatureValue = cell.BaseTemperatureValue;
        BaseRainfallValue = cell.BaseRainfallValue;

        BaseTemperatureOffset = cell.BaseTemperatureOffset;
        BaseRainfallOffset = cell.BaseRainfallOffset;

        OriginalAltitude = cell.OriginalAltitude;
        Altitude = cell.Altitude;
        Temperature = cell.Temperature;
        OriginalTemperature = cell.OriginalTemperature;
        Rainfall = cell.Rainfall;
        WaterAccumulation = cell.WaterAccumulation;
        RiverId = cell.RiverId;
        RiverLength = cell.RiverLength;

        FarmlandPercentage = cell.FarmlandPercentage;
        Accessibility = cell.Accessibility;
        Arability = cell.Arability;

        if (addLayerData)
        {
            foreach (CellLayerData data in cell.LayerData)
            {
                if (data.Offset == 0) continue;

                LayerData.Add(new CellLayerData(data));
            }
        }

        Modified = cell.Modified;
    }
}
