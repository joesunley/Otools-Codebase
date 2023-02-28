using SharpHook;
using Sunley.Mathematics;
using S = SharpHook.Native;

namespace OTools.Common;

public static class Input
{
	public static void Start()
	{
		_Input.Start();
	}
	
	public static event Action<MouseEventArgs>? MouseMove;
	public static event Action<MouseEventArgs>? MouseDrag;
	public static event Action<MouseEventArgs>? MouseDown;
	public static event Action<MouseEventArgs>? MouseUp;
	public static event Action<MouseEventArgs>? MouseClick;
	public static event Action<MouseWheelEventArgs>? MouseWheel;

	public static event Action<KeyEventArgs>? KeyDown;
	public static event Action<KeyEventArgs>? KeyUp;
	public static event Action<KeyEventArgs>? KeyTyped;

	public static event Action? Tick;

	public static vec2 MousePosition => _Input.MousePosition;
	
	#region Invokes
	internal static void Invoke_MouseMove(MouseEventArgs e) => MouseMove?.Invoke(e);
	internal static void Invoke_MouseDrag(MouseEventArgs e) => MouseDrag?.Invoke(e);
	internal static void Invoke_MouseDown(MouseEventArgs e) => MouseDown?.Invoke(e);
	internal static void Invoke_MouseUp(MouseEventArgs e) => MouseUp?.Invoke(e);
	internal static void Invoke_MouseClick(MouseEventArgs e) => MouseClick?.Invoke(e);
	internal static void Invoke_MouseWheel(MouseWheelEventArgs e) => MouseWheel?.Invoke(e);
	internal static void Invoke_KeyDown(KeyEventArgs e) => KeyDown?.Invoke(e);
	internal static void Invoke_KeyUp(KeyEventArgs e) => KeyUp?.Invoke(e);
	internal static void Invoke_KeyTyped(KeyEventArgs e) => KeyTyped?.Invoke(e);
	
	internal static void Invoke_Tick() => Tick?.Invoke();
	#endregion
	
	public static bool HasControlFlag(this KeyModifiers modifiers)
		=> modifiers.HasFlag(KeyModifiers.LeftControl) || modifiers.HasFlag(KeyModifiers.RightControl);
}

internal static class _Input
{
	private static readonly TaskPoolGlobalHook s_hook;

	private static readonly Dictionary<KeyCode, bool> s_keys;
	private static readonly Dictionary<MouseButton, bool> s_mouseButtons;

	private static vec2 s_mousePosition;


	private const double TICK_SPEED = 5;

	static _Input()
	{
		s_hook = new();
		
		s_hook.MouseMoved += Hook_MouseMove;
		s_hook.MouseDragged += Hook_MouseDrag;
		s_hook.MousePressed += Hook_MouseDown;
		s_hook.MouseReleased += Hook_MouseUp;
		s_hook.MouseClicked += Hook_MouseClick;
		s_hook.MouseWheel += Hook_MouseWheel;
		s_hook.KeyPressed += Hook_KeyDown;
		s_hook.KeyReleased += Hook_KeyUp;
		s_hook.KeyTyped += Hook_KeyTyped;

		s_keys = new();
		KeyCode[] keys = Enum.GetValues<KeyCode>();
		foreach (KeyCode key in keys)
			s_keys.Add(key, false);

		s_mouseButtons = new();
		MouseButton[] buttons = Enum.GetValues<MouseButton>();
		foreach (MouseButton button in buttons)
			s_mouseButtons.Add(button, false);

		System.Timers.Timer ticker = new();
		ticker.Interval = TICK_SPEED;
		ticker.Elapsed += (_, _) => Input.Invoke_Tick();

		ticker.Start();
	}

	internal static void Start() => s_hook.RunAsync();
	
	#region Hooks

	private static void Hook_MouseMove(object? _, MouseHookEventArgs e)
	{
		vec2 pos = new(e.Data.X, e.Data.Y);
		MouseButton code = Convert_MouseCodes(e.Data.Button);

		s_mousePosition = pos;
		
		Input.Invoke_MouseMove(new(code, pos, e));
	}

	private static void Hook_MouseDrag(object? _, MouseHookEventArgs e)
	{
		vec2 pos = new(e.Data.X, e.Data.Y);
		MouseButton code = Convert_MouseCodes(e.Data.Button);
		
		s_mousePosition = pos;

		Input.Invoke_MouseDrag(new(code, pos, e));
		Input.Invoke_MouseMove(new(code, pos, e)); 
		
		// Also invokes MouseMove as is otherwise not invoked
	}

	private static void Hook_MouseDown(object? _, MouseHookEventArgs e)
	{
		vec2 pos = new(e.Data.X, e.Data.Y);
		MouseButton code = Convert_MouseCodes(e.Data.Button);

		s_mouseButtons[code] = true;

		Input.Invoke_MouseDown(new(code, pos, e));
	}
	
	private static void Hook_MouseUp(object? _, MouseHookEventArgs e)
	{
		vec2 pos = new(e.Data.X, e.Data.Y);
		MouseButton code = Convert_MouseCodes(e.Data.Button);

		s_mouseButtons[code] = false;

		Input.Invoke_MouseUp(new(code, pos, e));
	}

	private static void Hook_MouseClick(object? _, MouseHookEventArgs e)
	{
		vec2 pos = new(e.Data.X, e.Data.Y);
		MouseButton code = Convert_MouseCodes(e.Data.Button);

		Input.Invoke_MouseClick(new(code, pos, e));
	}

	private static void Hook_MouseWheel(object? _, MouseWheelHookEventArgs e)
	{
		vec2 pos = new(e.Data.X, e.Data.Y);
		MouseWheelDirection dir = (e.Data.Rotation > 0 ? MouseWheelDirection.Down : MouseWheelDirection.Up);

		Input.Invoke_MouseWheel(new(dir, pos, e));
	}

	private static void Hook_KeyDown(object? _, KeyboardHookEventArgs e)
	{
		KeyCode code = Convert_KeyCodes(e.Data.KeyCode);

		s_keys[code] = true;

		Input.Invoke_KeyDown(new(code, e));
	}

	private static void Hook_KeyUp(object? _, KeyboardHookEventArgs e)
	{
		KeyCode code = Convert_KeyCodes(e.Data.KeyCode);

		s_keys[code] = false;

		Input.Invoke_KeyUp(new(code, e));
	}

	private static void Hook_KeyTyped(object? _, KeyboardHookEventArgs e)
	{
		KeyCode code = Convert_KeyCodes(e.Data.KeyCode);

		Input.Invoke_KeyTyped(new(code, e));
	}
	
	#endregion
	
	private static KeyCode Convert_KeyCodes(S.KeyCode keyCode)
    {
        int val = keyCode switch
        {
            S.KeyCode.VcSpace => 32,
            S.KeyCode.VcBackquote => 39,
            S.KeyCode.VcComma => 44,
            S.KeyCode.VcMinus => 45,
            S.KeyCode.VcPeriod => 46,
            S.KeyCode.VcSlash => 47,

            S.KeyCode.Vc0 => 48,
            S.KeyCode.Vc1 => 49,
            S.KeyCode.Vc2 => 50,
            S.KeyCode.Vc3 => 51,
            S.KeyCode.Vc4 => 52,
            S.KeyCode.Vc5 => 53,
            S.KeyCode.Vc6 => 54,
            S.KeyCode.Vc7 => 55,
            S.KeyCode.Vc8 => 56,
            S.KeyCode.Vc9 => 57,

            S.KeyCode.VcSemicolon => 59,
            S.KeyCode.VcEquals => 61,

            S.KeyCode.VcA => 65,
            S.KeyCode.VcB => 66,
            S.KeyCode.VcC => 67,
            S.KeyCode.VcD => 68,
            S.KeyCode.VcE => 69,
            S.KeyCode.VcF => 70,
            S.KeyCode.VcG => 71,
            S.KeyCode.VcH => 72,
            S.KeyCode.VcI => 73,
            S.KeyCode.VcJ => 74,
            S.KeyCode.VcK => 75,
            S.KeyCode.VcL => 76,
            S.KeyCode.VcM => 77,
            S.KeyCode.VcN => 78,
            S.KeyCode.VcO => 79,
            S.KeyCode.VcP => 80,
            S.KeyCode.VcQ => 81,
            S.KeyCode.VcR => 82,
            S.KeyCode.VcS => 83,
            S.KeyCode.VcT => 84,
            S.KeyCode.VcU => 85,
            S.KeyCode.VcV => 86,
            S.KeyCode.VcW => 87,
            S.KeyCode.VcX => 88,
            S.KeyCode.VcY => 89,
            S.KeyCode.VcZ => 90,

            S.KeyCode.VcOpenBracket => 91,
            S.KeyCode.VcBackSlash => 92,
            S.KeyCode.VcCloseBracket => 93,
            S.KeyCode.VcYen => 96,

            S.KeyCode.VcEscape => 256,
            S.KeyCode.VcEnter => 257,
            S.KeyCode.VcTab => 258,
            S.KeyCode.VcBackspace => 259,
            S.KeyCode.VcInsert => 260,
            S.KeyCode.VcDelete => 261,

            S.KeyCode.VcRight => 262,
            S.KeyCode.VcLeft => 263,
            S.KeyCode.VcDown => 264,
            S.KeyCode.VcUp => 265,

            S.KeyCode.VcPageUp => 266,
            S.KeyCode.VcPageDown => 267,
            S.KeyCode.VcHome => 268,
            S.KeyCode.VcEnd => 269,

            S.KeyCode.VcCapsLock => 280,
            S.KeyCode.VcScrollLock => 281,
            S.KeyCode.VcNumLock => 282,
            S.KeyCode.VcPrintScreen => 283,
            S.KeyCode.VcPause => 284,

            S.KeyCode.VcF1 => 290,
            S.KeyCode.VcF2 => 291,
            S.KeyCode.VcF3 => 292,
            S.KeyCode.VcF4 => 293,
            S.KeyCode.VcF5 => 294,
            S.KeyCode.VcF6 => 295,
            S.KeyCode.VcF7 => 296,
            S.KeyCode.VcF8 => 297,
            S.KeyCode.VcF9 => 298,
            S.KeyCode.VcF10 => 299,
            S.KeyCode.VcF11 => 300,
            S.KeyCode.VcF12 => 301,
            S.KeyCode.VcF13 => 302,
            S.KeyCode.VcF14 => 303,
            S.KeyCode.VcF15 => 304,
            S.KeyCode.VcF16 => 305,
            S.KeyCode.VcF17 => 306,
            S.KeyCode.VcF18 => 307,
            S.KeyCode.VcF19 => 308,
            S.KeyCode.VcF20 => 309,
            S.KeyCode.VcF21 => 310,
            S.KeyCode.VcF22 => 311,
            S.KeyCode.VcF23 => 312,
            S.KeyCode.VcF24 => 313,

            S.KeyCode.VcNumPad0 => 320,
            S.KeyCode.VcNumPad1 => 321,
            S.KeyCode.VcNumPad2 => 322,
            S.KeyCode.VcNumPad3 => 323,
            S.KeyCode.VcNumPad4 => 324,
            S.KeyCode.VcNumPad5 => 325,
            S.KeyCode.VcNumPad6 => 326,
            S.KeyCode.VcNumPad7 => 327,
            S.KeyCode.VcNumPad8 => 328,
            S.KeyCode.VcNumPad9 => 329,
            S.KeyCode.VcNumPadSeparator => 330,
            S.KeyCode.VcNumPadDivide => 331,
            S.KeyCode.VcNumPadMultiply => 332,
            S.KeyCode.VcNumPadSubtract => 333,
            S.KeyCode.VcNumPadAdd => 334,
            S.KeyCode.VcNumPadEnter => 335,
            S.KeyCode.VcNumPadEquals => 336,

            S.KeyCode.VcLeftShift => 340,
            S.KeyCode.VcLeftControl => 341,
            S.KeyCode.VcLeftAlt => 342,
            S.KeyCode.VcLeftMeta => 343,
            S.KeyCode.VcRightShift => 344,
            S.KeyCode.VcRightControl => 345,
            S.KeyCode.VcRightMeta => 346,
            S.KeyCode.VcContextMenu => 347,

            _ => -1,
        };

        return (KeyCode)val;
    }

	private static MouseButton Convert_MouseCodes(S.MouseButton mouseButton)
		=> (MouseButton)mouseButton;

	internal static KeyModifiers GetKeyModifiers()
	{
		KeyModifiers mod = 0;

		if (s_keys[KeyCode.LeftShift])
			mod |= KeyModifiers.LeftShift;
		if (s_keys[KeyCode.LeftControl])
			mod |= KeyModifiers.LeftControl;
		if (s_keys[KeyCode.LeftAlt])
			mod |= KeyModifiers.LeftAlt;
		if (s_keys[KeyCode.LeftSuper])
			mod |= KeyModifiers.LeftSuper;

		if (s_keys[KeyCode.RightShift])
			mod |= KeyModifiers.RightShift;
		if (s_keys[KeyCode.RightControl])
			mod |= KeyModifiers.RightControl;
		if (s_keys[KeyCode.RightAlt])
			mod |= KeyModifiers.RightAlt;
		if (s_keys[KeyCode.RightSuper])
			mod |= KeyModifiers.RightSuper;

		return mod;
	}

	internal static IEnumerable<MouseButton> GetPressedMouseButtons()
		=> s_mouseButtons.Where(x => x.Value).Select(x => x.Key);

	internal static IEnumerable<KeyCode> GetPressedKeys()
		=> s_keys.Where(x => x.Value).Select(x => x.Key);

	internal static bool IsMouseButtonPressed(MouseButton button)
		=> s_mouseButtons[button];
	
	internal static bool IsKeyPressed(KeyCode code)
		=> s_keys[code];

	internal static vec2 MousePosition => s_mousePosition;

}

#region Keys

public enum KeyCode : short
{
    // From github.com/TheCherno/Hazel
    
    UnregisteredKey = -1,

    Space           = 32,  /*   */
    Apostrophe      = 39,  /* ' */
    Comma           = 44,  /* , */
    Minus           = 45,  /* - */
    Period          = 46,  /* . */
    Slash           = 47,  /* / */
    
    D0              = 48,  /* 0 */
    D1              = 49,  /* 1 */
    D2              = 50,  /* 2 */
    D3              = 51,  /* 3 */
    D4              = 52,  /* 4 */
    D5              = 53,  /* 5 */
    D6              = 54,  /* 6 */
    D7              = 55,  /* 7 */
    D8              = 56,  /* 8 */
    D9              = 57,  /* 9 */

    A               = 65,
    B               = 66,
    C               = 67,
    D               = 68,
    E               = 69,
    F               = 70,
    G               = 71,
    H               = 72,
    I               = 73,
    J               = 74,
    K               = 75,
    L               = 76,
    M               = 77,
    N               = 78,
    O               = 79,
    P               = 80,
    Q               = 81,
    R               = 82,
    S               = 83,
    T               = 84,
    U               = 85,
    V               = 86,
    W               = 87,
    X               = 88,
    Y               = 89,
    Z               = 90,

    Semicolon       = 59,  /* ; */
    Equal           = 61,  /* = */

    LeftBracket     = 91,  /* [ */
    Backslash       = 92,  /* \ */
    RightBracket    = 93,  /* ] */
    GraveAccent     = 96,  /* ` */

    World1          = 161, /* non-US #1 */
    World2          = 162, /* non-US #2 */

    /* Function keys */
    Escape          = 256,
    Enter           = 257,
    Tab             = 258,
    Backspace       = 259,
    Insert          = 260,
    Delete          = 261,
    Right           = 262,
    Left            = 263,
    Down            = 264,
    Up              = 265,
    PageUp          = 266,
    PageDown        = 267,
    Home            = 268,
    End             = 269,
    CapsLock        = 280,
    ScrollLock      = 281,
    NumLock         = 282,
    PrintScreen     = 283,
    Pause           = 284,
    
    F1              = 290,
    F2              = 291,
    F3              = 292,
    F4              = 293,
    F5              = 294,
    F6              = 295,
    F7              = 296,
    F8              = 297,
    F9              = 298,
    F10             = 299,
    F11             = 300,
    F12             = 301,
    F13             = 302,
    F14             = 303,
    F15             = 304,
    F16             = 305,
    F17             = 306,
    F18             = 307,
    F19             = 308,
    F20             = 309,
    F21             = 310,
    F22             = 311,
    F23             = 312,
    F24             = 313,
    F25             = 314,

    /* Numpad */
    NP0             = 320,
    NP1             = 321,
    NP2             = 322,
    NP3             = 323,
    NP4             = 324,
    NP5             = 325,
    NP6             = 326,
    NP7             = 327,
    NP8             = 328,
    NP9             = 329,
    NPDecimal       = 330,
    NPDivide        = 331,
    NPMultiply      = 332,
    NPSubtract      = 333,
    NPAdd           = 334,
    NPEnter         = 335,
    NPEqual         = 336,

    /* Modifiers */
    LeftShift       = 340,
    LeftControl     = 341,
    LeftAlt         = 342,
    LeftSuper       = 343,
    RightShift      = 344,
    RightControl    = 345,
    RightAlt        = 346,
    RightSuper      = 347,
    Menu            = 348,
}

[Flags]
public enum KeyModifiers : byte
{
    LeftShift       = 0b00000001,
    LeftControl     = 0b00000010,
    LeftAlt         = 0b00000100,
    LeftSuper       = 0b00001000,
    RightShift      = 0b00010000,
    RightControl    = 0b00100000,
    RightAlt        = 0b01000000,
    RightSuper      = 0b10000000,
}

#endregion

#region Mouse

public enum MouseButton : byte
{
    NoButton    = 0,
    Left        = 1,
    Right       = 2,
    Middle      = 3,
    XButton1    = 4,
    XButton2    = 5,
}

public enum MouseWheelDirection : sbyte
{
    Up      = -1,
    Down    = 1,
}

#endregion

#region Event Args

public interface IInputEventArgs { }

public sealed class MouseEventArgs : IInputEventArgs
{
	public readonly vec2 Position;

	public readonly MouseButton MouseButton;
	public readonly KeyModifiers Modifiers;

	public readonly DateTime TimeStamp;

	public readonly MouseHookEventArgs? OriginalArgs;

	public MouseEventArgs(MouseButton mouseButton, vec2 position, MouseHookEventArgs? originalEventArgs = null)
	{
		Position = position;

		MouseButton = (mouseButton == MouseButton.NoButton) ? 
			_Input.GetPressedMouseButtons().FirstOrDefault(MouseButton.NoButton) : 
			mouseButton;
        
		Modifiers = _Input.GetKeyModifiers();

		TimeStamp = DateTime.UtcNow;

		OriginalArgs = originalEventArgs;
	}
}

public sealed class MouseWheelEventArgs : IInputEventArgs
{
	public readonly vec2 Position;

	public readonly MouseWheelDirection Direction;
	public readonly KeyModifiers Modifiers;

	public readonly DateTime TimeStamp;

	public readonly MouseWheelHookEventArgs? OriginalArgs;

	public MouseWheelEventArgs(MouseWheelDirection direction, vec2 position, MouseWheelHookEventArgs? originalEventArgs = null)
	{
		Position = position;

		Direction = direction;
		Modifiers = _Input.GetKeyModifiers();

		TimeStamp = DateTime.UtcNow;

		OriginalArgs = originalEventArgs;
	}
}

public sealed class KeyEventArgs : IInputEventArgs
{
	public readonly KeyCode KeyCode;
	public readonly KeyModifiers Modifiers;

	public readonly DateTime TimeStamp;

	public readonly KeyboardHookEventArgs? OriginalArgs;

	public KeyEventArgs(KeyCode keyCode, KeyboardHookEventArgs? originalEventArgs = null)
	{
		KeyCode = keyCode;
		Modifiers = _Input.GetKeyModifiers();

		TimeStamp = DateTime.UtcNow;

		OriginalArgs = originalEventArgs;
	}
}

#endregion