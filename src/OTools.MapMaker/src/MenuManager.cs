using Avalonia.Controls;
using OTools.ObjectRenderer2D;
using TerraFX.Interop.Windows;

namespace OTools.MapMaker;

public class MenuManager
{
    private MenuItem? _active;

    public bool Call(MenuItem m)
    {
        _active = m;

        bool ret = m.Header switch
        {
            "Wireframe View" => WireframeView(),
            "Uncrossable View" => UncrossableView(),
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
            Manager.MapRenderer = new MapRenderer2D(Manager.Map!);
            Manager.ReRender();
            _isWireframe = false;

            _active!.Icon = CreateCheckBox();
        }
        else
        {
            Manager.MapRenderer = new WireframeMapRenderer2D(Manager.Map!);
            Manager.ReRender();
            _isWireframe = true;

            _active!.Icon = CreateCheckBox(true);
        }

        return true;
    }

    private bool UncrossableView()
    {
        if (_isUncrossable)
        {
            Manager.MapRenderer = new MapRenderer2D(Manager.Map!);
            Manager.ReRender();
            _isUncrossable = false;

            _active!.Icon = CreateCheckBox();
        }
        else
        {
            Manager.MapRenderer = new UncrossableMapRenderer2D(Manager.Map!);
            Manager.ReRender();
            _isUncrossable = true;

            _active!.Icon = CreateCheckBox(true);
        }

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