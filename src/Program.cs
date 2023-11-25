using System;

// C# translation of https://lodev.org/cgtutor/raycasting.html#Textured_Raycaster.
// I did not actually make it.
// Updated the WorldMap to kind of look like E1 of Wolf3D.

unsafe partial class Program
{
    const int Width = 640;
    const int Height = 480;

    const int MapWidth = 24;
    const int MapHeight = 24;

    const int TexWidth = 64;
    const int TexHeight = 64;

    static ReadOnlySpan<byte> WorldMap => [
        0,0,0,0,0,0,0,0,0,0,1,1,5,1,1,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,3,0,0,0,1,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,3,0,0,0,3,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,3,0,0,0,1,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,1,0,0,0,3,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,3,0,0,0,3,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,3,0,0,0,1,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,1,0,0,0,3,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,1,1,3,1,3,1,1,0,1,1,1,1,1,3,1,0,0,0,0,
        0,0,0,0,0,3,0,0,0,0,1,0,0,0,1,0,0,0,0,3,0,0,0,0,
        0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,
        0,0,0,0,0,1,0,0,0,0,1,0,0,0,1,0,0,0,0,3,0,0,0,0,
        0,0,0,0,0,3,0,0,0,0,3,0,0,0,3,0,0,0,0,3,0,0,0,0,
        0,0,0,0,0,1,1,3,1,1,3,0,0,0,1,1,1,1,1,1,0,0,0,0,
        0,0,0,0,0,1,0,0,0,0,1,0,0,0,1,0,0,0,0,1,0,0,0,0,
        0,0,0,0,0,3,0,0,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,
        0,0,0,0,0,1,0,0,0,0,1,0,0,0,1,0,0,0,0,1,0,0,0,0,
        0,0,0,0,0,1,3,3,1,1,3,0,0,0,3,1,1,3,1,1,0,0,0,0,
        0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,
        0,0,0,0,0,3,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,
        0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,
        0,0,0,0,0,1,2,3,1,2,1,3,4,1,3,2,3,1,2,1,0,0,0,0];

    private static void RenderEffect(uint tick, byte* framebuf)
    {
        new Span<int>(framebuf, Width * Height / 2).Fill(0x222222);
        new Span<int>((int*)framebuf + Width * Height / 2, Width * Height / 2).Fill(0x555555);

        for (int x = 0; x < Width; x++)
        {
            //calculate ray position and direction
            double cameraX = 2 * x / (double)Width - 1; //x-coordinate in camera space
            double rayDirX = dirX + planeX * cameraX;
            double rayDirY = dirY + planeY * cameraX;
            //which box of the map we're in
            int mapX = (int)posX;
            int mapY = (int)posY;

            //length of ray from current position to next x or y-side
            double sideDistX;
            double sideDistY;

            //length of ray from one x or y-side to next x or y-side
            //these are derived as:
            //deltaDistX = sqrt(1 + (rayDirY * rayDirY) / (rayDirX * rayDirX))
            //deltaDistY = sqrt(1 + (rayDirX * rayDirX) / (rayDirY * rayDirY))
            //which can be simplified to abs(|rayDir| / rayDirX) and abs(|rayDir| / rayDirY)
            //where |rayDir| is the length of the vector (rayDirX, rayDirY). Its length,
            //unlike (dirX, dirY) is not 1, however this does not matter, only the
            //ratio between deltaDistX and deltaDistY matters, due to the way the DDA
            //stepping further below works. So the values can be computed as below.
            // Division through zero is prevented, even though technically that's not
            // needed in C++ with IEEE 754 floating point values.
            double deltaDistX = (rayDirX == 0) ? 1e30 : Math.Abs(1 / rayDirX);
            double deltaDistY = (rayDirY == 0) ? 1e30 : Math.Abs(1 / rayDirY);

            double perpWallDist;

            //what direction to step in x or y-direction (either +1 or -1)
            int stepX;
            int stepY;

            int hit = 0; //was there a wall hit?
            int side = 0; //was a NS or a EW wall hit?
                      //calculate step and initial sideDist
            if (rayDirX < 0)
            {
                stepX = -1;
                sideDistX = (posX - mapX) * deltaDistX;
            }
            else
            {
                stepX = 1;
                sideDistX = (mapX + 1.0 - posX) * deltaDistX;
            }
            if (rayDirY < 0)
            {
                stepY = -1;
                sideDistY = (posY - mapY) * deltaDistY;
            }
            else
            {
                stepY = 1;
                sideDistY = (mapY + 1.0 - posY) * deltaDistY;
            }
            //perform DDA
            while (hit == 0)
            {
                //jump to next map square, either in x-direction, or in y-direction
                if (sideDistX < sideDistY)
                {
                    sideDistX += deltaDistX;
                    mapX += stepX;
                    side = 0;
                }
                else
                {
                    sideDistY += deltaDistY;
                    mapY += stepY;
                    side = 1;
                }
                //Check if ray has hit a wall
                if (WorldMap[mapX * MapHeight + mapY] > 0) hit = 1;
            }
            //Calculate distance projected on camera direction. This is the shortest distance from the point where the wall is
            //hit to the camera plane. Euclidean to center camera point would give fisheye effect!
            //This can be computed as (mapX - posX + (1 - stepX) / 2) / rayDirX for side == 0, or same formula with Y
            //for size == 1, but can be simplified to the code below thanks to how sideDist and deltaDist are computed:
            //because they were left scaled to |rayDir|. sideDist is the entire length of the ray above after the multiple
            //steps, but we subtract deltaDist once because one step more into the wall was taken above.
            if (side == 0) perpWallDist = (sideDistX - deltaDistX);
            else perpWallDist = (sideDistY - deltaDistY);

            //Calculate height of line to draw on screen
            int lineHeight = (int)(Height / perpWallDist);

            int pitch = 0;

            //calculate lowest and highest pixel to fill in current stripe
            int drawStart = -lineHeight / 2 + Height / 2 + pitch;
            if (drawStart < 0) drawStart = 0;
            int drawEnd = lineHeight / 2 + Height / 2 + pitch;
            if (drawEnd >= Height) drawEnd = Height - 1;

            //texturing calculations
            int texNum = WorldMap[mapX * MapHeight + mapY] - 1; //1 subtracted from it so that texture 0 can be used!

            //calculate value of wallX
            double wallX; //where exactly the wall was hit
            if (side == 0) wallX = posY + perpWallDist * rayDirY;
            else wallX = posX + perpWallDist * rayDirX;
            wallX -= Math.Floor((wallX));

            //x coordinate on the texture
            int texX = (int)(wallX * TexWidth);
            if (side == 0 && rayDirX > 0) texX = TexWidth - texX - 1;
            if (side == 1 && rayDirY < 0) texX = TexWidth - texX - 1;

            // TODO: an integer-only bresenham or DDA like algorithm could make the texture coordinate stepping faster
            // How much to increase the texture coordinate per screen pixel
            double step = 1.0 * TexHeight / lineHeight;
            // Starting texture coordinate
            double texPos = (drawStart - pitch - Height / 2 + lineHeight / 2) * step;
            for (int y = drawStart; y < drawEnd; y++)
            {
                // Cast the texture coordinate to integer, and mask with (texHeight - 1) in case of overflow
                int texY = (int)texPos & (TexHeight - 1);
                texPos += step;
                byte texel = GetTexture(texNum)[TexHeight * texY + texX];
                int color = Palette[texel * 3] | Palette[texel * 3 + 1] << 8 | Palette[texel * 3 + 2] << 16;
                //make color darker for y-sides: R, G and B byte each divided through two with a "shift" and an "and"
                if (side == 1) color = (color >> 1) & 8355711;
                *(int*)(framebuf + (x + y * Width) * 4) = color;
            }
        }

        double frameTime = (tick - time) / 1000.0; //frameTime is the time this frame has taken, in seconds

        //speed modifiers
        double moveSpeed = frameTime * 5.0; //the constant value is in squares/second
        double rotSpeed = frameTime * 3.0; //the constant value is in radians/second

        //move forward if no wall in front of you
        if ((keyState & KeyState.Up) != 0)
        {
            if (WorldMap[(int)(posX + dirX * moveSpeed) * MapHeight + (int)posY] == 0) posX += dirX * moveSpeed;
            if (WorldMap[(int)posX * MapHeight + (int)(posY + dirY * moveSpeed)] == 0) posY += dirY * moveSpeed;
        }
        ////move backwards if no wall behind you
        if ((keyState & KeyState.Down) != 0)
        {
            if (WorldMap[(int)(posX - dirX * moveSpeed) * MapHeight + (int)posY] == 0) posX -= dirX * moveSpeed;
            if (WorldMap[(int)posX * MapHeight + (int)(posY - dirY * moveSpeed)] == 0) posY -= dirY * moveSpeed;
        }
        ////rotate to the right
        if ((keyState & KeyState.Right) != 0)
        {
            //both camera direction and camera plane must be rotated
            double oldDirX = dirX;
            dirX = dirX * Math.Cos(-rotSpeed) - dirY * Math.Sin(-rotSpeed);
            dirY = oldDirX * Math.Sin(-rotSpeed) + dirY * Math.Cos(-rotSpeed);
            double oldPlaneX = planeX;
            planeX = planeX * Math.Cos(-rotSpeed) - planeY * Math.Sin(-rotSpeed);
            planeY = oldPlaneX * Math.Sin(-rotSpeed) + planeY * Math.Cos(-rotSpeed);
        }
        ////rotate to the left
        if ((keyState & KeyState.Left) != 0)
        {
            //both camera direction and camera plane must be rotated
            double oldDirX = dirX;
            dirX = dirX * Math.Cos(rotSpeed) - dirY * Math.Sin(rotSpeed);
            dirY = oldDirX * Math.Sin(rotSpeed) + dirY * Math.Cos(rotSpeed);
            double oldPlaneX = planeX;
            planeX = planeX * Math.Cos(rotSpeed) - planeY * Math.Sin(rotSpeed);
            planeY = oldPlaneX * Math.Sin(rotSpeed) + planeY * Math.Cos(rotSpeed);
        }

        time = tick;
    }

    static double posX = 17, posY = 8;  //x and y start position
    static double dirX = -1, dirY = 0; //initial direction vector
    static double planeX = 0, planeY = 0.66; //the 2d raycaster version of camera plane
    static KeyState keyState;
    static uint time;

    enum KeyState
    {
        Left = 1,
        Up = 2,
        Right = 4,
        Down = 8,
    }
}