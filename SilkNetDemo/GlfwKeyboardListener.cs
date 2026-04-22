using System;
using System.Collections.Generic;
using Silk.NET.GLFW;
using Logos.Input;

public sealed class GlfwKeyboardListener : IKeyboardListener
{
    private readonly GlfwKeyboardDevice _device;

    public GlfwKeyboardListener()
    {
        _device = new GlfwKeyboardDevice();
    }

    public IEnumerable<IKeyboardDevice> Devices
    {
        get { yield return _device; }
    }

    IEnumerable<IInputDevice> IInputListener.Devices
    {
        get { yield return _device; }
    }
    
    public event EventHandler<InputEventArgs>? DeviceConnected;
    public event EventHandler<InputEventArgs>? DeviceDisconnected;
    public event EventHandler<KeyEventArgs>? KeyPressed;
    public event EventHandler<KeyEventArgs>? KeyRepeated;
    public event EventHandler<KeyEventArgs>? KeyReleased;

    internal void OnKey(Keys glfwKey, InputAction action)
    {
        KeyCode key = GlfwKeyToKeyCode(glfwKey);
        TimeSpan timestamp = GetTimestamp();

        switch (action)
        {
            case InputAction.Press:
                _device.OnKeyDown(key);
                KeyPressed?.Invoke(this, new KeyEventArgs(_device, timestamp, key));
                break;
            case InputAction.Repeat:
                KeyRepeated?.Invoke(this, new KeyEventArgs(_device, timestamp, key));
                break;
            case InputAction.Release:
                _device.OnKeyUp(key);
                KeyReleased?.Invoke(this, new KeyEventArgs(_device, timestamp, key));
                break;
        }
    }

    private static TimeSpan GetTimestamp()
    {
        return TimeSpan.FromSeconds(Glfw.GetApi().GetTime());
    }

    // thank god for this shorthand return switch statement.
    private static KeyCode GlfwKeyToKeyCode(Keys key)
    {
        return key switch
        {
            Keys.A => KeyCode.A,
            Keys.B => KeyCode.B,
            Keys.C => KeyCode.C,
            Keys.D => KeyCode.D,
            Keys.E => KeyCode.E,
            Keys.F => KeyCode.F,
            Keys.G => KeyCode.G,
            Keys.H => KeyCode.H,
            Keys.I => KeyCode.I,
            Keys.J => KeyCode.J,
            Keys.K => KeyCode.K,
            Keys.L => KeyCode.L,
            Keys.M => KeyCode.M,
            Keys.N => KeyCode.N,
            Keys.O => KeyCode.O,
            Keys.P => KeyCode.P,
            Keys.Q => KeyCode.Q,
            Keys.R => KeyCode.R,
            Keys.S => KeyCode.S,
            Keys.T => KeyCode.T,
            Keys.U => KeyCode.U,
            Keys.V => KeyCode.V,
            Keys.W => KeyCode.W,
            Keys.X => KeyCode.X,
            Keys.Y => KeyCode.Y,
            Keys.Z => KeyCode.Z,
            Keys.Number0 => KeyCode.D0,
            Keys.Number1 => KeyCode.D1,
            Keys.Number2 => KeyCode.D2,
            Keys.Number3 => KeyCode.D3,
            Keys.Number4 => KeyCode.D4,
            Keys.Number5 => KeyCode.D5,
            Keys.Number6 => KeyCode.D6,
            Keys.Number7 => KeyCode.D7,
            Keys.Number8 => KeyCode.D8,
            Keys.Number9 => KeyCode.D9,
            Keys.Space => KeyCode.Space,
            Keys.Escape => KeyCode.Escape,
            Keys.Enter => KeyCode.Return,
            Keys.Tab => KeyCode.Tab,
            Keys.Backspace => KeyCode.Backspace,
            Keys.Left => KeyCode.LeftArrow,
            Keys.Right => KeyCode.RightArrow,
            Keys.Up => KeyCode.UpArrow,
            Keys.Down => KeyCode.DownArrow,
            Keys.F1 => KeyCode.F1,
            Keys.F2 => KeyCode.F2,
            Keys.F3 => KeyCode.F3,
            Keys.F4 => KeyCode.F4,
            Keys.F5 => KeyCode.F5,
            Keys.F6 => KeyCode.F6,
            Keys.F7 => KeyCode.F7,
            Keys.F8 => KeyCode.F8,
            Keys.F9 => KeyCode.F9,
            Keys.F10 => KeyCode.F10,
            Keys.F11 => KeyCode.F11,
            Keys.F12 => KeyCode.F12,
            Keys.ShiftLeft => KeyCode.LeftShift,
            Keys.ShiftRight => KeyCode.RightShift,
            Keys.ControlLeft => KeyCode.LeftCtrl,
            Keys.ControlRight => KeyCode.RightCtrl,
            Keys.AltLeft => KeyCode.LeftAlt,
            Keys.AltRight => KeyCode.RightAlt,
            _ => KeyCode.None
        };
    }

    private sealed class GlfwKeyboardDevice : IKeyboardDevice
    {
        private readonly HashSet<KeyCode> _pressedKeys = new();

        public bool IsConnected => true;

        public IEnumerable<KeyCode> PressedKeys => _pressedKeys;

        public bool IsKeyPressed(KeyCode key) => _pressedKeys.Contains(key);

        public void OnKeyDown(KeyCode key) => _pressedKeys.Add(key);

        public void OnKeyUp(KeyCode key) => _pressedKeys.Remove(key);
    }
}