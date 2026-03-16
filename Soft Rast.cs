using System.Numerics;
using Raylib_cs;

public struct Soft_Triangle
{
    public Vec2i V1, V2, V3;
    public Color Shade;
}

public unsafe sealed class Soft_Rasterizer
{
    // Vars --------------------------

    private readonly Color[] Pixels;
    private RenderTexture2D Frame;

    // Funcs -------------------------

    public Soft_Rasterizer()
    {
        int Scr_size_x = Raylib.GetScreenWidth();
        int Scr_size_y = Raylib.GetScreenHeight();

        Pixels = new Color[Scr_size_y * Scr_size_x];
        Frame = Raylib.LoadRenderTexture(Scr_size_x, Scr_size_y);
    }

    private void Rasterize(Soft_Triangle A)
    {
        // loop v2 to v3
        if (A.V2.X > A.V3.X)
        Utility.Swap(ref A.V2, ref A.V3);

        // intial pos
        float Start_x = A.V1.X, End_x = A.V1.X;
        int Y_level = A.V1.Y, Line_count = Utility.ABS(A.V2.Y - A.V1.Y);
        int Index_range = 0;

        // go from V1 towards V2 and V3
        float Delta_to_v2 = (A.V2.X - A.V1.X) / (float)Line_count,
        Delta_to_v3 = (A.V3.X - A.V1.X) / (float)Line_count;
        bool Move_up = A.V2.Y < A.V1.Y;

        for (int i = 0; i <= Line_count; i++)
        {
            // pre calc index of htat x axis
            Index_range = Y_level * Frame.Texture.Width;
            Y_level += Move_up ? -1 : 1;

            for (int j = (int)Start_x; j <= (int)End_x; j++)
            Pixels[Index_range + j] = A.Shade;

            // Accumalate new val
            Start_x += Delta_to_v2;
            End_x += Delta_to_v3;
        }
    }

    public void Begin_Drawing() => Array.Clear(Pixels);

    public void Draw_Trignale(Soft_Triangle Shape)
    {
        Shape.V1 = Utility.Clamp(Shape.V1, new Vec2i(1, 1), new Vec2i(Frame.Texture.Width - 2, Frame.Texture.Height - 2));
        Shape.V2 = Utility.Clamp(Shape.V2, new Vec2i(1, 1), new Vec2i(Frame.Texture.Width - 2, Frame.Texture.Height - 2));
        Shape.V3 = Utility.Clamp(Shape.V3, new Vec2i(1, 1), new Vec2i(Frame.Texture.Width - 2, Frame.Texture.Height - 2));

        // if already 2 points are in same y so flat line can be made
        if (Shape.V1.Y == Shape.V2.Y ||
        Shape.V2.Y == Shape.V3.Y || Shape.V3.Y == Shape.V1.Y)
        {
            // V1 shall be start point
            if (Shape.V1.Y == Shape.V2.Y) Utility.Swap(ref Shape.V1, ref Shape.V3);
            else if (Shape.V1.Y == Shape.V3.Y) Utility.Swap(ref Shape.V1, ref Shape.V2);

            Rasterize(Shape);
            return;
        }

        var Top = Shape.V1.Y < Shape.V2.Y ? Shape.V1 : Shape.V2;
        Top = Top.Y < Shape.V3.Y ? Top : Shape.V3;

        var Bottom = Shape.V1.Y > Shape.V2.Y ? Shape.V1 : Shape.V2;
        Bottom = Bottom.Y > Shape.V3.Y ? Bottom : Shape.V3;

        var Mid = Shape.V1.Y != Top.Y && Shape.V1.Y != Bottom.Y
        ? Shape.V1 : Shape.V2.Y != Top.Y && Shape.V2.Y != Bottom.Y
        ? Shape.V2 : Shape.V3;

        // some maths 🤨
        var V4_ratio = (Mid.Y - Top.Y) / (float)(Bottom.Y - Top.Y);
        var V4 = new Vec2i(0, Mid.Y);
        V4.X = (int)(Top.X + (Bottom.X - Top.X) * V4_ratio);

        Soft_Triangle Trig_1 = new(), Trig_2 = new();
        Trig_1.Shade = Trig_2.Shade = Shape.Shade;

        Trig_1.V1 = Top;
        Trig_1.V2 = Mid;
        Trig_1.V3 = V4;

        Trig_2.V1 = Bottom;
        Trig_2.V2 = Mid;
        Trig_2.V3 = V4;

        Rasterize(Trig_1);
        Rasterize(Trig_2);
    }

    public void Render()
    {
        Raylib.BeginTextureMode(Frame);
        Raylib.ClearBackground(Color.Black);
        fixed (void* Ptr = Pixels) Raylib.UpdateTexture(Frame.Texture, Ptr);
        Raylib.EndTextureMode();

        Raylib.DrawTexture(Frame.Texture, 0, 0, Color.White);
    }

}
