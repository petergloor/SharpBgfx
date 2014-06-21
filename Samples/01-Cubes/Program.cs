﻿using System;
using Common;
using SharpBgfx;

static class Program {
    static void Main () {
        // create a UI thread and kick off a separate render thread
        var sample = new Sample("Cubes", 1280, 720);
        sample.Run(RenderThread);
    }

    static void RenderThread (Sample sample) {
        // initialize the renderer
        Bgfx.Init(RendererType.OpenGL, IntPtr.Zero, IntPtr.Zero);
        Bgfx.Reset(sample.WindowWidth, sample.WindowHeight, ResetFlags.Vsync);

        // enable debug text
        Bgfx.SetDebugFlags(DebugFlags.DisplayText);

        // set view 0 clear state
        Bgfx.SetViewClear(0, ClearFlags.ColorBit | ClearFlags.DepthBit, 0x303030ff, 1.0f, 0);

        // create vertex and index buffers
        PosColorVertex.Init();
        var vbh = Bgfx.CreateVertexBuffer(Memory.Copy(cubeVertices), ref PosColorVertex.Decl);
        var ibh = Bgfx.CreateIndexBuffer(Memory.Copy(cubeIndices));

        // load shaders
        var program = ResourceLoader.LoadProgram("vs_cubes", "fs_cubes");

        // main loop
        while (sample.ProcessEvents(ResetFlags.Vsync)) {
            // set view 0 viewport
            Bgfx.SetViewRect(0, 0, 0, (ushort)sample.WindowWidth, (ushort)sample.WindowHeight);

            // dummy draw call to make sure view 0 is cleared if no other draw calls are submitted
            Bgfx.Submit(0, 0);

            // write some debug text
            Bgfx.DebugTextClear(0, false);
            Bgfx.DebugTextWrite(0, 1, 0x4f, "SharpBgfx/Samples/01-Cubes");
            Bgfx.DebugTextWrite(0, 2, 0x6f, "Description: Rendering simple static mesh.");
            Bgfx.DebugTextWrite(0, 3, 0x6f, string.Format("Frame: {0} ms", 0.0f));

            // submit 11x11 cubes
            for (int y = 0; y < 11; y++) {
                for (int x = 0; x < 11; x++) {
                    // set pipeline states
                    Bgfx.SetProgram(program);
                    Bgfx.SetVertexBuffer(vbh, 0, -1);
                    Bgfx.SetIndexBuffer(ibh, 0, -1);
                    Bgfx.SetRenderState(RenderState.Default, 0);

                    // submit primitives
                    Bgfx.Submit(0, 0);
                }
            }

            // advance to the next frame. Rendering thread will be kicked to
            // process submitted rendering primitives.
            Bgfx.Frame();
        }

        // clean up
        Bgfx.Shutdown();
    }

    static readonly PosColorVertex[] cubeVertices = {
        new PosColorVertex(-1.0f,  1.0f,  1.0f, 0xff000000),
        new PosColorVertex( 1.0f,  1.0f,  1.0f, 0xff0000ff),
        new PosColorVertex(-1.0f, -1.0f,  1.0f, 0xff00ff00),
        new PosColorVertex( 1.0f, -1.0f,  1.0f, 0xff00ffff),
        new PosColorVertex(-1.0f,  1.0f, -1.0f, 0xffff0000),
        new PosColorVertex( 1.0f,  1.0f, -1.0f, 0xffff00ff),
        new PosColorVertex(-1.0f, -1.0f, -1.0f, 0xffffff00),
        new PosColorVertex( 1.0f, -1.0f, -1.0f, 0xffffffff)
    };

    static readonly ushort[] cubeIndices = {
        0, 1, 2, // 0
        1, 3, 2,
        4, 6, 5, // 2
        5, 6, 7,
        0, 2, 4, // 4
        4, 2, 6,
        1, 5, 3, // 6
        5, 7, 3,
        0, 4, 1, // 8
        4, 5, 1,
        2, 3, 6, // 10
        6, 3, 7
    };
}

struct PosColorVertex {
    float x;
    float y;
    float z;
    uint abgr;

    public PosColorVertex (float x, float y, float z, uint abgr) {
        this.x = x;
        this.y = y;
        this.z = z;
        this.abgr = abgr;
    }

    public static VertexDecl Decl;

    public static void Init () {
        Decl = new VertexDecl();
        Bgfx.VertexDeclBegin(ref Decl, RendererType.Null);
        Bgfx.VertexDeclAdd(ref Decl, VertexAttribute.Position, 3, VertexAttributeType.Float, false, false);
        Bgfx.VertexDeclAdd(ref Decl, VertexAttribute.Color0, 4, VertexAttributeType.UInt8, true, false);
        Bgfx.VertexDeclEnd(ref Decl);
    }
}