using System;
using System.Collections.Generic;
using System.Numerics;
using Silk.NET.GLFW;
using Logos.Input;

using MouseButton = Logos.Input.MouseButton;

public sealed class GlfwMouseListener : IMouseListener
{
    private readonly GlfwMouseDevice _device;

    public IEnumerable<IMouseDevice> Devices
    {
        get { yield return _device; }
    }

    IEnumerable<IInputDevice> IInputListener.Devices
    {
        get { yield return _device; }
    }
    
    public GlfwMouseListener()
    {
        _device = new GlfwMouseDevice();
    }
    
    public event EventHandler<InputEventArgs>? DeviceConnected;
    public event EventHandler<InputEventArgs>? DeviceDisconnected;
    public event EventHandler<MouseButtonEventArgs>? ButtonPressed;
    public event EventHandler<MouseButtonEventArgs>? ButtonReleased;
    public event EventHandler<MouseMotionEventArgs>? MouseMoved;
    public event EventHandler<MouseWheelEventArgs>? WheelMoved;

    internal void OnMouseButton(MouseButton button, InputAction action)
    {
        TimeSpan timestamp = GetTimestamp();

        switch (action)
        {
            case InputAction.Press:
                _device.OnButtonDown(button);
                ButtonPressed?.Invoke(this, new MouseButtonEventArgs(_device, timestamp, button));
                break;
            case InputAction.Release:
                _device.OnButtonUp(button);
                ButtonReleased?.Invoke(this, new MouseButtonEventArgs(_device, timestamp, button));
                break;
        }
    }

    internal void OnMouseMoved(double x, double y)
    {
        Vector2 position = new Vector2((float)x, (float)y);
        Vector2 velocity;
        _device.OnMouseMoved(position);
        MouseMoved?.Invoke(this, new MouseMotionEventArgs(_device, GetTimestamp(), position));
    }

    internal void OnMouseWheel(double x, double y)
    {
        Vector2 delta = new Vector2((float)x, (float)y);
        _device.OnMouseWheel(delta);
        WheelMoved?.Invoke(this, new MouseWheelEventArgs(_device, GetTimestamp(), delta));
    }

    private static TimeSpan GetTimestamp()
    {
        return TimeSpan.FromSeconds(Glfw.GetApi().GetTime());
    }

    // We used to need this but not anymore since we made our own MouseButton enum that matches GLFW's.
    /*
    private static MouseButton GlfwButtonToMouseButton(MouseButton button)
    {
        switch (button)
        {
            case MouseButton.Left:
                return MouseButton.Left;
            case MouseButton.Right:
                return MouseButton.Right;
            case MouseButton.Middle:
                return MouseButton.Middle;
            default:
                return MouseButton.Left;
        }
    }
    */

    private sealed class GlfwMouseDevice : IMouseDevice
    {
        private readonly HashSet<MouseButton> _pressedButtons = new();

        public bool IsConnected => true;

        public Vector2 Position { get; private set; }

        public Vector2 ScrollWheel { get; private set; }

        public IEnumerable<MouseButton> PressedButtons => _pressedButtons;

        public bool IsButtonPressed(MouseButton button) => _pressedButtons.Contains(button);

        public void OnButtonDown(MouseButton button) => _pressedButtons.Add(button);

        public void OnButtonUp(MouseButton button) => _pressedButtons.Remove(button);

        public void OnMouseMoved(Vector2 position) => Position = position;

        public void OnMouseWheel(Vector2 delta) => ScrollWheel = delta;
    }
}