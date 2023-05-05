using OTools.Maps;
using OTools.ObjectRenderer2D;
using System;

namespace OTools.MapMaker;

public class MapEdit
{
    private (PointEdit point, PathEdit path) draws;

    private bool isActive;
    private Active active;

    public MapEdit()
    {
        Manager.ActiveToolChanged += args => isActive = (args == Tool.Edit);
    }

    private enum Active { Point, Path }

}

public class PointEdit
{

}

public class PathEdit
{

}