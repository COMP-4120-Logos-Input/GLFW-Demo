# Logos.Input + GLFW

This tutorial will walk you through integrating Logos.Input into a GLFW project using Silk.NET's C# bindings. By the end you will have a working OpenGL window that responds to keyboard and mouse input through the Logos.Input library.

## Table of Contents

- [Dependencies](#dependencies)
- [How GLFW Input Works](#how-glfw-input-works)
- [General Steps to Implementing an Input Backend](#general-steps-to-implementing-an-input-backend)
    - [Step 1 - Initialize GLFW and Create a Window](#step-1---initialize-glfw-and-create-a-window)
    - [Step 2 - Create the Input Provider](#step-2---create-the-input-provider)
    - [Step 3 - Get Listeners](#step-3---get-listeners)
    - [Step 4 - Subscribe to Events](#step-4---subscribe-to-events)
    - [Step 5 - Set Up OpenGL](#step-5---set-up-opengl)
    - [Step 6 - The Game Loop](#step-6---the-game-loop)
    - [Step 7 - Clean Up](#step-7---clean-up)
    - [Complete Example](#complete-example)
- [Going Further with Mappers](#going-further-with-mappers)
- [Querying Device and Control State](#querying-device-and-control-state)
- [Important Note on GLFW Callbacks](#important-note-on-glfw-callbacks)

---

## Dependencies

- .NET 10 SDK or later
- The following NuGet packages installed:

```
Silk.NET.GLFW
Silk.NET.OpenGL
```

- Since this project is meant to show off Logos.Input, you must also have the Logos.Input binary somewhere on your machine. Then, in your IDE, you must reference the `Logos.Input.dll` file that is built by Logos.Input. Information on building Logos.Input can be found in the main repo, [Logos.Input Repository](https://github.com/COMP-4120-Logos-Input/Logos.Input)
---

## How GLFW Input Works

Before writing any code it helps to understand how GLFW delivers input, because it is fundamentally different from a polling-based system like SDL.

With SDL you call `SDL_PollEvent` every frame and it hands you events one at a time. With GLFW, however, you register **callbacks** functions that GLFW calls automatically when input occurs. When you call `glfwPollEvents()`, GLFW processes its event queue and invokes your registered callbacks before returning.

This means:
- GLFW will tell us when an event occurred rather than us asking if the event occurs.
- All your input handling happens inside the callbacks, not after `PollEvents` returns.
- `DispatchEvents()` in `GlfwInputProvider` will simply call `glfwPollEvents()`. Your Logos.Input events will be invoked inside that call.
---

## General Steps to Implementing an Input Backend
Recall that in [quickstart.md](https://github.com/COMP-4120-Logos-Input/Logos.Input/blob/main/quickstart.md) which is located in the Logos.Input repository, there are 4 main steps to follow when integrating Logos.Input with a program. Those are:
```
1. Create a provider
2. Get a listener from the provider
3. Subscribe to events on the listener
4. Call DispatchEvents() every frame
``` 
In this tutorial, we will follow this same sequence.

## Step 1 - Initialize GLFW and Create a Window

Before we start creating a provider, its good to start by getting the GLFW API and creating a window. GLFW needs to be initialized before any other GLFW function is called.

```csharp
using Silk.NET.GLFW;
using Silk.NET.OpenGL;

unsafe
{
    Glfw glfw = Glfw.GetApi();

    if (!glfw.Init())
        throw new Exception("GLFW failed to initialize.");

    // OpenGL 3.3 core context
    glfw.WindowHint(WindowHintInt.ContextVersionMajor, 3);
    glfw.WindowHint(WindowHintInt.ContextVersionMinor, 3);
    glfw.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);

    // Creates the window handle
    WindowHandle* window = glfw.CreateWindow(1280, 720, "Logos.Input Tutorial", null, null);

    if (window == null)
        throw new Exception("GLFW failed to create a window.");

    glfw.MakeContextCurrent(window);
}
```

`glfw.CreateWindow` returns a raw `WindowHandle*` pointer. This pointer is what GLFW uses to identify your window in every subsequent call. Note that this means the code will have to be marked as `unsafe`. You will then pass this window handle to `GlfwInputProvider` in the next step. 

---

## Step 2 - Create the Input Provider
This is likely to be the most complicated step as you need to map GLFW inputs to Logos.Input format as well as using our library's event system to simulate GLFW callbacks. In this case the `GlfwInputProvider` is already provided so you do not need to worry about implementing it.

`GlfwInputProvider` is the bridge between GLFW and Logos.Input. Pass it the `Glfw` API instance and the window handle. Upon construction of the `GlfwInputProvider`, it registers GLFW callbacks for key events, mouse button events, cursor movement, and scroll wheel movement.

```csharp
using Logos.Input;

// Inside your unsafe block, after creating the window:
using GlfwInputProvider provider = new GlfwInputProvider(glfw, window);
```

`GlfwInputProvider` implements `IDisposable`, thus the `using` statement ensures it is cleaned up properly when the application exits.

At this point GLFW is fully wired to Logos.Input. Any key or mouse event that GLFW receives will flow through the provider automatically.

---

## Step 3 - Get Listeners

Listeners will give you access to input events for each device type. Get them from the provider using `GetListener<T>()`.

```csharp
IKeyboardListener keyboard = provider.GetListener<IKeyboardListener>();
IMouseListener mouse = provider.GetListener<IMouseListener>();
```

`IKeyboardListener` exposes `KeyPressed`, `KeyRepeated`, and `KeyReleased` events along with `Devices` so you can query connected *keyboards*.

`IMouseListener` exposes `ButtonPressed`, `ButtonReleased`, `MouseMoved`, and `WheelMoved` events along with `Devices` so you can query connected *mice*.

---

## Step 4 - Subscribe to Events

Now subscribe to the events using the listeners you just obtained. These subscriptions will be invoked when `provider.DispatchEvents()` is called in the game loop.

```csharp
// Close the window when the Escape key is pressed
keyboard.KeyPressed += (_, e) =>
{
    if (e.Key == KeyCode.Escape)
        glfw.SetWindowShouldClose(window, true);
};

// Print the key that is being held down
keyboard.KeyRepeated += (_, e) =>
{
    Console.WriteLine($"Key held: {e.Key}");
};

// Print the mouse button that was clicked and its position in the window
mouse.ButtonPressed += (_, e) =>
{
    Console.WriteLine($"{e.Button} clicked at {e.Device.Position}");
};

// Print cursor's position whenever the mouse moves
mouse.MouseMoved += (_, e) =>
{
    Console.WriteLine($"Cursor: {e.Device.Position}");
};

// Print the movement of the scroll wheel
mouse.WheelMoved += (_, e) =>
{
    Console.WriteLine($"Scroll: ({e.Scroll.X}, {e.Scroll.Y})");
};
```

---

## Step 5 - Set Up OpenGL

We've essentially done everything involving Logos.Input and GLFW. The rest of this tutorial will set up OpenGL.

Get the OpenGL API through the GLFW context. This gives you a fully initialized `GL` instance.

```csharp
GL gl = GL.GetApi(new GlfwContext(glfw, window));
```

---

## Step 6 - The Game Loop

Call `provider.DispatchEvents()` once per frame. This calls `glfwPollEvents()` internally, which fires all registered GLFW callbacks, which in turn fires your Logos.Input event subscriptions. Everything happens synchronously inside that one call.

```csharp
while (!glfw.WindowShouldClose(window))
{
    provider.DispatchEvents();

    gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
    gl.Clear(ClearBufferMask.ColorBufferBit);

    glfw.SwapBuffers(window);
}
```

---

## Step 7 - Clean Up

After the loop exits, destroy the window and terminate GLFW. The `using` block on the provider handles its own cleanup automatically so don't worry about cleaning up the input provider.

```csharp
glfw.DestroyWindow(window);
glfw.Terminate();
```

---

## Complete Example

Here is the full program with everything from the steps above assembled together:

```csharp
using System;
using Silk.NET.GLFW;
using Silk.NET.OpenGL;
using Logos.Input;

unsafe
{
    // Step 1 — Initialize GLFW and create a window
    Glfw glfw = Glfw.GetApi();

    if (!glfw.Init())
        throw new Exception("GLFW failed to initialize.");

    glfw.WindowHint(WindowHintInt.ContextVersionMajor, 3);
    glfw.WindowHint(WindowHintInt.ContextVersionMinor, 3);
    glfw.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);

    WindowHandle* window = glfw.CreateWindow(1280, 720, "Logos.Input Tutorial", null, null);

    if (window == null)
        throw new Exception("GLFW failed to create a window.");

    glfw.MakeContextCurrent(window);

    // Step 2 — Create the input provider
    using GlfwInputProvider provider = new GlfwInputProvider(glfw, window);

    // Step 3 — Get listeners
    IKeyboardListener keyboard = provider.GetListener<IKeyboardListener>();
    IMouseListener mouse = provider.GetListener<IMouseListener>();

    // Step 4 — Subscribe to events
    keyboard.KeyPressed += (_, e) =>
    {
        if (e.Key == KeyCode.Escape)
            glfw.SetWindowShouldClose(window, true);

        Console.WriteLine($"Key pressed: {e.Key}");
    };

    keyboard.KeyRepeated += (_, e) =>
    {
        Console.WriteLine($"Key held: {e.Key}");
    };

    keyboard.KeyReleased += (_, e) =>
    {
        Console.WriteLine($"Key released: {e.Key}");
    };

    mouse.ButtonPressed += (_, e) =>
    {
        Console.WriteLine($"{e.Button} clicked at {e.Device.Position}");
    };

    mouse.MouseMoved += (_, e) =>
    {
        Console.WriteLine($"Cursor: {e.Translation}");
    };

    mouse.WheelMoved += (_, e) =>
    {
        Console.WriteLine($"Scroll: {e.Delta}");
    };

    // Step 5 — Set up OpenGL
    GL gl = GL.GetApi(new GlfwContext(glfw, window));

    // Step 6 — Game loop
    while (!glfw.WindowShouldClose(window))
    {
        provider.DispatchEvents();

        gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        gl.Clear(ClearBufferMask.ColorBufferBit);

        glfw.SwapBuffers(window);
    }

    // Step 7 — Clean up
    glfw.DestroyWindow(window);
    glfw.Terminate();
}
```

---

## Going Further with Mappers

Subscribing directly to listener events is one way to react to input and it works fine, but for larger projects, you will likely want to use `KeyboardMapper` and `MouseMapper`. Mappers let you bind specific gestures to observer objects and swap input contexts efficiently, for example, disabling game input while a menu is open.

More information on mappers can be found in the [quickstart.md](https://github.com/COMP-4120-Logos-Input/Logos.Input/blob/main/quickstart.md) file in our Logos.Input repository

### Writing a Control
Before we use mappers, we must first create a *Control*. `KeyControl<T>` lets you track state that changes in response to input and notify subscribers when it does. Controls are templated meaning you may choose the type of the state. Here is a control that tracks whether a fire button is held using a boolean type:

```csharp
public sealed class FireControl : KeyControl<bool>
{
    public FireControl() : base() { }

    public override void OnKeyPressed(object? sender, KeyEventArgs e)
    {
        State = true;
    }

    public override void OnKeyRepeated(object? sender, KeyEventArgs e)
    {
    }

    public override void OnKeyReleased(object? sender, KeyEventArgs e)
    {
        State = false;
    }
}

// You can also subscribe to state changes anywhere in your code
FireControl fireControl = new FireControl();

fireControl.StateChanged += (_, isFiring) =>
{
    if (isFiring)
        Console.WriteLine("Started firing");
    else
        Console.WriteLine("Stopped firing");
};
```

You now initialize a `KeyboardMapper` and a `KeyControl<T>` object. You may then bind a `KeyGesture` to a control using the input mappers `Bind()` method.
Note that the `IsEnabled` property of an `IInputMapper` object is set to `false` on initialization.

### Binding Key Gestures

```csharp
// Make a KeyboardMapper
KeyboardMapper keyMapper = new KeyboardMapper();

// Bind Space press and release to the same observer
// FireControl handles both OnKeyPressed and OnKeyReleased internally
FireControl fireControl = new FireControl();
keyMapper.Bind(new KeyGesture(KeyCode.Space, KeyAction.Press), fireControl);
keyMapper.Bind(new KeyGesture(KeyCode.Space, KeyAction.Release), fireControl);

// Start routing events from the provider to the mapper
keyMapper.IsEnabled = true;
```

### Binding Mouse Gestures

Binding mouse gestures to a control follow a similar method to key controls.

```csharp
MouseMapper mouseMapper = new MouseMapper();

// Fire on left click
mouseMapper.Bind(new MouseButtonGesture(MouseButton.Left, MouseButtonAction.Press), fireObserver);

mouseMapper.IsEnabled = true
```

### Switching Input Contexts

Set `IsEnabled` to `false` to stop a mapper from receiving input, for example when pausing, and `isEnabled` to `true` to restore it:

```csharp
// Player opens pause menu
keyMapper.isEnabled = false;
mouseMapper.isEnabled = false;

pauseMenuMapper.IsEnabled = true;

// Player resumes
pauseMenuMapper.IsEnabled = false

keyMapper.IsEnabled = true;
mouseMapper.IsEnabled = true;
```



---

## Querying Device and Control State

In addition to events you can query device state directly each frame.

```csharp
while (!glfw.WindowShouldClose(window))
{
    provider.DispatchEvents();

    foreach (IKeyboardDevice device in keyboard.Devices)
    {
        if (device.IsKeyPressed(KeyCode.W))
            Console.WriteLine("Moving forward");

        if (device.IsKeyPressed(KeyCode.LeftShift))
            Console.WriteLine("Sprinting");
    }

    foreach (IMouseDevice device in mouse.Devices)
    {
        if (device.IsButtonPressed(MouseButton.Left))
            Console.WriteLine("Firing");

        Console.WriteLine($"Cursor at: {device.Position}");
    }

    gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
    gl.Clear(ClearBufferMask.ColorBufferBit);
    glfw.SwapBuffers(window);
}
```

Note that `provider.DispatchEvents()` should be called before querying device state. It is what processes the GLFW event queue and updates the device state that `IsKeyPressed` and `IsButtonPressed` read from.

You may also query the **Control State** as follows:

```csharp
while (!glfw.WindowShouldClose(window))
{
    provider.DispatchEvents();

    foreach (IKeyboardDevice device in keyboard.Devices)
    {
        if (device.IsKeyPressed(KeyCode.W))
            Console.WriteLine("Moving forward");

        if (device.IsKeyPressed(KeyCode.LeftShift))
            Console.WriteLine("Sprinting");
    }

    foreach (IMouseDevice device in mouse.Devices)
    {
        if (device.IsButtonPressed(MouseButton.Left))
            Console.WriteLine("Firing");

        Console.WriteLine($"Cursor at: {device.Position}");
    }

    if (fireControl.State == true) 
    {
        Console.WriteLine($"Fire Control State is true");
    }

    gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
    gl.Clear(ClearBufferMask.ColorBufferBit);
    glfw.SwapBuffers(window);
}
```
---

## Important Note on GLFW Callbacks

`GlfwInputProvider` stores references to its callback delegates as fields. This is important so do not remove them. GLFW holds raw unmanaged function pointers to these callbacks. If the delegates were passed as inline lambdas without being stored, the garbage collector could collect them while GLFW still holds the pointer, causing a crash. Holding the references as fields keeps them alive for the lifetime of the provider.