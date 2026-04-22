using System;
using System.Numerics;
using Silk.NET.GLFW;
using Silk.NET.OpenGL;
using Logos.Input;

using Shader = SilkNetDemo.Shader;
unsafe
{
    /*
     * GLFW STUFF (You can ignore the beginning section of this code. It has nothing to do with the Logos.Input library.
     * Its just normal GLFW and OpenGL boilerplate code to set up and render a triangle to the screen.
     */
    // #################################################################################################################
    float[] vertices =
    {
        // positions         // colors
        0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f, // bottom right
        -0.5f, -0.5f, 0.0f, 0.0f, 1.0f, 0.0f, // bottom left
        0.0f, 0.5f, 0.0f, 0.0f, 0.0f, 1.0f // top
    };
    
    Glfw glfw = Glfw.GetApi();

    if (!glfw.Init())
        throw new Exception("GLFW failed to initialize.");
    
    // Tell GLFW we want an OpenGL 3.3 core context
    glfw.WindowHint(WindowHintInt.ContextVersionMajor, 3);
    glfw.WindowHint(WindowHintInt.ContextVersionMinor, 3);
    glfw.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);

    // Initialize window to demo Logos.Input, GLFW, and Silk.NET
    WindowHandle* window = glfw.CreateWindow(1280, 720, "GLFW Demo", null, null);

    if (window == null)
        throw new Exception("GLFW failed to create window.");

    glfw.MakeContextCurrent(window);

    GL gl = GL.GetApi(new GlfwContext(glfw, window));

    string baseDir = AppContext.BaseDirectory;
    
    // Create Shader
    using SilkNetDemo.Shader shader = new Shader(gl, Path.Combine(baseDir, "Shaders/shader.vert"), Path.Combine(baseDir, "Shaders/shader.frag"));
    
    // Generate VBOs
    uint VBO, VAO;
    gl.GenVertexArrays(1, out VAO);
    gl.GenBuffers(1, out VBO);
    gl.BindVertexArray(VAO);
    
    gl.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);
    gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), vertices, BufferUsageARB.StaticDraw);
    
    gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 6 * sizeof(float), (void*)0);
    gl.EnableVertexAttribArray(0);
    gl.VertexAttribPointer(1, 3, GLEnum.Float, false, 6 * sizeof(float), (void*)(3 * sizeof(float)));
    gl.EnableVertexAttribArray(1);

    Vector4 clearColor = new Vector4(0.1f, 0.1f, 0.1f, 1.0f);
    bool isRunning = true;
    // #################################################################################################################
    
    
    /*
     * Initialize the input provider, which sets up the necessary GLFW callbacks and allows access to the listeners and mappers.
     */
    using GlfwInputProvider provider = new GlfwInputProvider(glfw, window);

    /*
     * Get the listeners using the provider. These listeners will be passed into the mappers to bind controls
     *  You can also subscribe directly to the listener events if you'd like.
     */
    IKeyboardListener keyboard = provider.GetListener<IKeyboardListener>();
    IMouseListener mouse = provider.GetListener<IMouseListener>();

    /* For this example, we only work with the keyboard mapper although working with the mouse mapper is largely the same.
     *  In this example, we bind the space key to a control that toggles "disco mode" on and off. When disco mode is on, the background color changes randomly every frame.
     *  You can view the implementation of DiscoModeControl at the bottom of this file. It simply sets its state to true when the bounded key is pressed, and false when it's released.
     */
    KeyboardMapper keyMapper = new KeyboardMapper(keyboard);
    DiscoModeControl discoModeControl = new DiscoModeControl();
    keyMapper.Bind(new KeyGesture(KeyCode.Space, KeyAction.Press), discoModeControl);
    keyMapper.Bind(new KeyGesture(KeyCode.Space, KeyAction.Release), discoModeControl);
    
    // MAKE SURE YOU ENABLE THE MAPPER. ITS DISABLED BY DEFAULT
    keyMapper.IsEnabled = true;



    // Subscribes DIRECTLY to the listener events rather than using a control and maappers. 
    //  This is to demonstrate that you can still use the raw events if you want, and that they work alongside the mappers/controls.
    keyboard.KeyPressed += (_, e) =>
    {
        switch (e.Key)
        {
            case KeyCode.Escape:
                isRunning = false;
                break;
            case KeyCode.R:
                clearColor = new Vector4(0.5f, 0.1f, 0.1f, 1.0f);
                Console.WriteLine("Background: Red");
                break;
            case KeyCode.G:
                clearColor = new Vector4(0.1f, 0.5f, 0.1f, 1.0f);
                Console.WriteLine("Background: Green");
                break;
            case KeyCode.B:
                clearColor = new Vector4(0.1f, 0.1f, 0.5f, 1.0f);
                Console.WriteLine("Background: Blue");
                break;
        }
        
        Console.WriteLine($"Key Pressed: {e.Key}");
    };

    mouse.ButtonPressed += (_, e) =>
    {
        Console.WriteLine($"Mouse button {e.Button} pressed at {e.Device.Position}");
    };

    mouse.MouseMoved += (_, e) =>
    {
        // Console.WriteLine($"Mouse Moved: {e.Device.Position} with a velocity of {e.Velocity}");
    };

    mouse.WheelMoved += (_, e) =>
    {
        Console.WriteLine($"Scroll: {e.Scroll.X}, {e.Scroll.Y}");
    };

    Console.WriteLine("R/G/B to change background. Escape to quit.");

    while (isRunning && !glfw.WindowShouldClose(window))
    {
        /*
         * DispatchEvents calls glfw.PollEvents internally,
         * which fires all registered callbacks synchronously
         */
        provider.DispatchEvents();

        gl.ClearColor(clearColor.X, clearColor.Y, clearColor.Z, clearColor.W);
        gl.Clear(ClearBufferMask.ColorBufferBit);
        
        shader.Use();
        gl.BindVertexArray(VAO);
        gl.DrawArrays(GLEnum.Triangles, 0, (uint)vertices.Length);
        
        /*
         * Directly query control state to change the background color randomly when disco mode is active.
         */
        if (discoModeControl.State == true)
        {
            clearColor = new Vector4((float)Random.Shared.NextDouble(), (float)Random.Shared.NextDouble(), (float)Random.Shared.NextDouble(), 1.0f);
        }
        
        glfw.SwapBuffers(window);
    }
    
    glfw.DestroyWindow(window);
    glfw.Terminate();
}

public sealed class DiscoModeControl : KeyControl<bool>
{
    public DiscoModeControl() : base() {}

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