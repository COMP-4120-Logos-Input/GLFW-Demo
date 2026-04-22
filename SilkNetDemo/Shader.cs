using System.IO;
using Silk.NET.GLFW;
using Silk.NET.OpenGL;

namespace SilkNetDemo;

public sealed class Shader : IDisposable
{
    private readonly GL _gl;
    public uint ID { get; }

    public Shader(GL gl, string vertexPath, string fragmentPath)
    {
        _gl = gl;

        string vertexSource = File.ReadAllText(vertexPath);
        string fragmentSource = File.ReadAllText(fragmentPath);

        uint vertex = CompileShader(ShaderType.VertexShader, vertexSource);
        uint fragment = CompileShader(ShaderType.FragmentShader, fragmentSource);
        
        ID = _gl.CreateProgram();
        _gl.AttachShader(ID, vertex);
        _gl.AttachShader(ID, fragment);
        _gl.LinkProgram(ID);

        _gl.GetProgram(ID, GLEnum.LinkStatus, out int success);
        if (success == 0)
        {
            string outbuf = _gl.GetProgramInfoLog(ID);
            throw new Exception($"Shader link error:\n{outbuf}");
        }
        
        _gl.DetachShader(ID, vertex);
        _gl.DetachShader(ID, fragment);
        _gl.DeleteShader(vertex);
        _gl.DeleteShader(fragment);
    }
    
    private uint CompileShader(ShaderType type, string source)
    {
        uint shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);

        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out int success);
        if (success == 0)
        {
            string outbuf = _gl.GetShaderInfoLog(shader);
            throw new Exception($"{type} compile error:\n{outbuf}");
        }

        return shader;
    }

    public void Use()
    {
        _gl.UseProgram(ID);
    }
    
    public void Dispose()
    {
        _gl.DeleteProgram(ID);
    }
}