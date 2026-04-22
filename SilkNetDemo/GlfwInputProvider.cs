using System;
using System.Collections.Generic;
using Silk.NET.GLFW;
using Logos.Input;

public sealed unsafe class GlfwInputProvider : IInputProvider, IDisposable
{
    private readonly Glfw _glfw;
    private readonly GlfwKeyboardListener _keyboard;
    private readonly GlfwMouseListener _mouse;

    // We must hold references to the delegates to prevent GC collection
    // since GLFW holds only an unmanaged function pointer
    private readonly GlfwCallbacks.KeyCallback _keyCallback;
    private readonly GlfwCallbacks.MouseButtonCallback _mouseButtonCallback;
    private readonly GlfwCallbacks.CursorPosCallback _cursorPosCallback;
    private readonly GlfwCallbacks.ScrollCallback _scrollCallback;

    public GlfwInputProvider(Glfw glfw, WindowHandle* window)
    {
        _glfw = glfw;
        _keyboard = new GlfwKeyboardListener();
        _mouse = new GlfwMouseListener();

        // Create delegates and hold references before registering
        _keyCallback = OnKey;
        _mouseButtonCallback = OnMouseButton;
        _cursorPosCallback = OnCursorPos;
        _scrollCallback = OnScroll;

        // Register callbacks with GLFW
        _glfw.SetKeyCallback(window, _keyCallback);
        _glfw.SetMouseButtonCallback(window, _mouseButtonCallback);
        _glfw.SetCursorPosCallback(window, _cursorPosCallback);
        _glfw.SetScrollCallback(window, _scrollCallback);
    }

    public IEnumerable<IInputListener> Listeners
    {
        get
        {
            yield return _keyboard;
            yield return _mouse;
        }
    }

    public T GetListener<T>() where T : IInputListener
    {
        if (_keyboard is T k) return k;
        if (_mouse is T m) return m;
        throw new NotSupportedException();
    }

    // DispatchEvents just tells GLFW to process pending events,
    // which fires our registered callbacks synchronously
    public void DispatchEvents()
    {
        _glfw.PollEvents();
    }

    private void OnKey(WindowHandle* window, Keys key, int scancode,
        InputAction action, KeyModifiers mods)
    {
        _keyboard.OnKey(key, action);
    }

    private void OnMouseButton(WindowHandle* window, Silk.NET.GLFW.MouseButton button, InputAction action, KeyModifiers mods)
    {
        _mouse.OnMouseButton(Convert(button), action);
    }

    // GLFW callback only returns cursor position, we'd have to calculate velocity internally in here (but im not doing that so we just pass position)
    private void OnCursorPos(WindowHandle* window, double x, double y)
    {
        _mouse.OnMouseMoved(x, y);
    }

    private void OnScroll(WindowHandle* window, double x, double y)
    {
        _mouse.OnMouseWheel(x, y);
    }

    public void Dispose() { }
    
    private Logos.Input.MouseButton Convert(Silk.NET.GLFW.MouseButton button)
    {
        switch (button)
        {
            case Silk.NET.GLFW.MouseButton.Left:
                return Logos.Input.MouseButton.Left;
            case Silk.NET.GLFW.MouseButton.Right:
                return Logos.Input.MouseButton.Right;
            case Silk.NET.GLFW.MouseButton.Middle:
                return Logos.Input.MouseButton.Middle;
            default:
                return Logos.Input.MouseButton.None;
                
        }
    }
}