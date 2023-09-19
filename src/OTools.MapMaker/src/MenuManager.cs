using Avalonia.Controls;
using OTools.ObjectRenderer2D;

namespace OTools.MapMaker;

public class MenuManager
{
    private MapMakerInstance _instance;
    private MenuItem? _active;

    public MenuManager(MapMakerInstance instance)
    {
        _instance = instance;
    }

    public bool Call(MenuItem m)
    {
        _active = m;

        bool ret = m.Header switch
        {
            "Wireframe View" => WireframeView(),
            "Uncrossable View" => UncrossableView(),
            "Select" => SetEdit(),
            "Draw Path" => SetPathDraw(),
            _ => false
        };

        _active = null;
        return ret;
    }

    // View Modes

    private bool _isWireframe = false,
        _isUncrossable = false;

    private bool WireframeView()
    {
        if (_isWireframe)
        {
            _instance.MapRenderer = new MapRenderer2D(_instance.Map);
            _instance.ReRender();
            _isWireframe = false;

            _active!.Icon = CreateCheckBox();
        }
        else
        {
            _instance.MapRenderer = new WireframeMapRenderer2D(_instance.Map);
            _instance.ReRender();
            _isWireframe = true;

            _active!.Icon = CreateCheckBox(true);
        }

        return true;
    }

    private bool UncrossableView()
    {
        if (_isUncrossable)
        {
            _instance.MapRenderer = new MapRenderer2D(_instance.Map!);
            _instance.ReRender();
            _isUncrossable = false;

            _active!.Icon = CreateCheckBox();
        }
        else
        {
            _instance.MapRenderer = new UncrossableMapRenderer2D(_instance.Map!);
            _instance.ReRender();
            _isUncrossable = true;

            _active!.Icon = CreateCheckBox(true);
        }

        return true;
    }

    // Draw Edit

    private bool SetEdit()
    {
        _instance.ActiveTool = Tool.Edit;
        _instance.MapEdit.Start();
        _instance.MapDraw.Stop();

        return true;
    }

    private bool SetPathDraw()
    {
        _instance.ActiveTool = Tool.Path;
        _instance.MapDraw!.Start();
        _instance.MapEdit!.Stop();

        return true;
    }

    private static CheckBox CreateCheckBox(bool isChecked = false)
    {
        return new()
        {
            IsChecked = isChecked,
            IsHitTestVisible = false,
            BorderThickness = new(0),
        };
    }
}