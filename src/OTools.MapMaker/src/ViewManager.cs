using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using OTools.ObjectRenderer2D;
using System;
using System.Collections.Generic;
using System.Linq;
using OTools.AvaCommon;
using System.ComponentModel.Design.Serialization;

namespace OTools.MapMaker;

public static class ViewManager
{
    private static bool s_isSet = false;
    private static readonly List<Guid> s_ids = new();

    private static PaintBox s_paintBox;

    public static void Set(PaintBox box, MainWindow mainWindow)
    {
        s_paintBox = box;
        s_isSet = true;

        //SetEvents(mainWindow);

        //s_paintBox.canvas.PointerMoved += (_, args) =>
        //{
        //    var point = args.GetCurrentPoint(s_paintBox.canvas).Position;
        //    MousePosition = (point.X, point.Y);
        //    MouseMove?.Invoke(MousePosition);
        //};
    }

    private static void SetEvents(MainWindow mainWindow)
    {
        //mainWindow.PointerPressed += (_, e) => MouseDown?.Invoke(e);
        //mainWindow.PointerReleased += (_, e) => MouseUp?.Invoke(e);

        //mainWindow.KeyDown += (_, e) => KeyDown?.Invoke(e);
        //mainWindow.KeyUp += (_, e) => KeyUp?.Invoke(e);
    }

    //public static vec4 Bounds
    //{
    //    get
    //    {
    //        var main = GetMainWindow();
            
    //        var l = main.PointToScreen(new Point(s_paintBox.Bounds.X, s_paintBox.Bounds.Y));
    //        var r = main.PointToScreen(new Point(s_paintBox.Bounds.X + s_paintBox.Bounds.Width, s_paintBox.Bounds.Y + s_paintBox.Bounds.Height));

    //        // var fL = s_paintBox.canvas.PointToClient(l);
    //        // var fR = s_paintBox.canvas.PointToClient(r);

    //        // return (fL.X, fL.Y, fR.X, fR.Y);
    //        throw new NotImplementedException();
    //    }
    //}



    public static event Action<KeyEventArgs>? KeyDown;
    public static event Action<KeyEventArgs>? KeyUp;


    private static Window? GetMainWindow()
    {
        if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;
        throw null;
    }
}