using OTools.Maps;

namespace OTools.CoursePlanner;

public struct OverprintSettings {

    public Colour OverprintColour { get; set; }
    public Colour Overprint50Colour { get; set; }
    public Colour WhiteColour { get; set; }

    // Start

    public float StartLineLength { get; set; }
    public float StartLineWidth { get; set; }

    // Map Issue

    public float MapIssueLength { get; set; }
    public float MapIssueWidth { get; set; }

    // Control Point

    public float ControlCircleDiameter { get; set; }
    public float ControlLineWidth { get; set; }

    // Control Number

    public float ControlNumberHeight { get; set; }
    public string ControlNumberFont { get; set; } // Will be set to a FONT object when implemented

    // Course Line

    public float CourseLineWidth { get; set; }

    // Finish

    public float FinishInnerDiameter { get; set; }
    public float FinishOuterDiameter { get; set; }
    public float FinishLineWidth { get; set; }

    // Marked Route

    public DashStyle MarkedRouteDashStyle { get; set; }
    public float MarkedRouteLineWidth { get; set; }

    // OOB Boundary

    public float BoundaryWidth { get; set; }
    
    // OOB Area

    public float AreaLineSpacing { get; set; }
    public float AreaLineRotation { get; set; }
    public float AreaLineWidth { get; set; }

    // OOB Line - Solid

    public float BorderWidth { get; set; }

    // OOB Line - Dashed

    public DashStyle BorderDashStyle { get; set; }

    // Crossing Point

    public float CrossingPointLineWidth { get; set; }
    public float CrossingPointSpacing { get; set; }
    public float CrossingPintLineLength { get; set; }

    // Crossing Section

    public float CrossingSectionLineWidth { get; set; }
    
    // Temp OOB - Line

    public float TempLineWidth { get; set; }


}