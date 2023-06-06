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

        SetEvents(mainWindow);

        // s_paintBox.canvas.PointerMoved += (_, args) =>
        // {
        //     var point = args.GetCurrentPoint(s_paintBox.canvas).Position;
        //     MousePosition = (point.X, point.Y);
        //     MouseMove?.Invoke(MousePosition);
        // };
    }

    private static void SetEvents(MainWindow mainWindow)
    {
        mainWindow.PointerPressed += (_, e) => MouseDown?.Invoke(e);
        mainWindow.PointerReleased += (_, e) => MouseUp?.Invoke(e);

        mainWindow.KeyDown += (_, e) => KeyDown?.Invoke(e);
        mainWindow.KeyUp += (_, e) => KeyUp?.Invoke(e);
    }

    public static void Add(Guid id, IEnumerable<IShape> objects)
    {
        Assert(s_isSet);
        WriteLine($"Added: {id}");

        IEnumerable<Shape> conv = ObjConvert.ConvertCollection(objects).Select(x => { x.Tag = id.ToString(); return x; });

        // s_paintBox.canvas.Children.AddRange(conv);
        s_ids.Add(id);
    }

    public static void Update(Guid id, IEnumerable<IShape> objects)
    {
        Assert(s_isSet);
        WriteLine($"Updated: {id}");


        IEnumerable<Shape> conv = ObjConvert.ConvertCollection(objects).Select(x => { x.Tag = id.ToString(); return x; });

        // var els = s_paintBox.canvas.Children.Select(x => (Shape)x).Where(x => x.Tag is string s && s.Contains(id.ToString()));
        // s_paintBox.canvas.Children.RemoveAll(els);
        //
        // s_paintBox.canvas.Children.AddRange(conv);
    }

    public static void AddOrUpdate(Guid id, IEnumerable<IShape> objects)
    {
        if (s_ids.Contains(id))
            Update(id, objects);
        else
            Add(id, objects);
    }

    public static void Remove(Guid id)
    {
        Assert(s_isSet);

        // var els = s_paintBox.canvas.Children.Select(x => (Shape)x).Where(x => x.Tag is string s && s.Contains(id.ToString()));
        // s_paintBox.canvas.Children.RemoveAll(els);

        s_ids.Remove(id);
    }

    public static void Clear()
    {
        Assert(s_isSet);

        foreach (Guid id in s_ids)
        {
            // var els = s_paintBox.canvas.Children.Select(x => (Shape)x).Where(x => x.Tag is string s && s.Contains(id.ToString()));
            // s_paintBox.canvas.Children.RemoveAll(els);

            s_ids.Remove(id);
        }

        Assert(!s_ids.Any());
    }

    public static vec4 Bounds
    {
        get
        {
            var main = GetMainWindow();
            
            var l = main.PointToScreen(new Point(s_paintBox.Bounds.X, s_paintBox.Bounds.Y));
            var r = main.PointToScreen(new Point(s_paintBox.Bounds.X + s_paintBox.Bounds.Width, s_paintBox.Bounds.Y + s_paintBox.Bounds.Height));

            // var fL = s_paintBox.canvas.PointToClient(l);
            // var fR = s_paintBox.canvas.PointToClient(r);

            // return (fL.X, fL.Y, fR.X, fR.Y);
            throw new NotImplementedException();
        }
    }

    public static vec2 Zoom
    {
        get
        {
            Assert(s_isSet);
            return s_paintBox.Zoom;
        }
    }
    public static vec2 Offset
    {
        get
        {
            Assert(s_isSet);
            return s_paintBox.Offset;
        }
    }

    public static vec2 MousePosition { get; private set; } 

    public static event Action<vec2>? MouseMove;

    public static event Action<PointerPressedEventArgs>? MouseDown;
    public static event Action<PointerReleasedEventArgs>? MouseUp;

    public static event Action<KeyEventArgs>? KeyDown;
    public static event Action<KeyEventArgs>? KeyUp;

    public static bool IsMouseOutsideBounds()
    {
        var m = MousePosition;
        var b = Bounds;

        return !(m.X < b.X || m.X > b.Z 
              || m.Y < b.Y || m.Y > b.Z);
    }

    private static Window? GetMainWindow()
    {
        if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;
        throw null;
    }
}