using OpenTK.Windowing.Common;
using StackAttack.Engine;
using StackAttack.Engine.Helpers;
using StackAttack.Engine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack.Objects
{
    internal class Exit : GameObject
    {
        TileData td;
        bool open = false;

        public bool IsOpen()
        {
            return open;
        }

        public Exit() : base(0,0,0,Headings.North, null, "")
        {
        }

        public Exit(int x, int y, float rotation, Headings heading, Game? game, string spriteID) : base(x, y, rotation, heading, game, spriteID)
        {
        }

        public override GameObject CreateNew(int x, int y, float rotation, Headings heading, Game? game, string spriteID)
        {
            rotation = (float)((int)Heading * (Math.PI / 2));
            Exit e = new Exit(x, y, rotation, heading, game, spriteID);
            if (game is null || game.CurrentScene is null || game.CurrentScene.GetType() != typeof(Scenes.GameScene))
            {
                Logger.Log(Logger.Levels.Fatal, "Cannot create exit.");
                return e;
            }
            Scenes.GameScene? scene =(Scenes.GameScene?)game.CurrentScene;
            if (scene is null)
            {
                Logger.Log(Logger.Levels.Fatal, "Cannot create exit.");
                return e;
            }
            td = new("Exit", x/4, y/4, (float)((int)Heading * (Math.PI / 2)));
            scene.Foreground.Tiles.Add(td);
            e.td = td;
            return e;
        }

        public override void Draw(FrameEventArgs args)
        {
            if (Parent is null)
                return; 
            if (Parent.CurrentScene is null)
                return;
            if (Parent.CurrentScene.GetType() != typeof(Scenes.GameScene))
                return;
            Scenes.GameScene? scene = (Scenes.GameScene)Parent.CurrentScene;
            if (scene is null)
                return;
            if (scene.Player is null)
                return;

            (bool returnState, Tile? returnTile) = ContentManager.Get<Tile>(td.TileID);
            if (returnState == true && returnTile is not null)
            {
                returnTile.Draw(new OpenTK.Mathematics.Vector2i(X - scene.CameraX, Y - scene.CameraY), Rotation);
            }
        }

        public void Open()
        {
            open = true;
            (bool returnState, Tile? returnTile) = ContentManager.Get<Tile>(td.TileID); if (returnState == true && returnTile is not null)
            {
                if (Parent is null)
                    return;
                if (Parent.CurrentScene is null)
                    return;
                if (Parent.CurrentScene.GetType() != typeof(Scenes.GameScene))
                    return;
                Scenes.GameScene? scene = (Scenes.GameScene)Parent.CurrentScene;
                if (scene is null)
                    return;
                returnTile.ShaderID = "BaseShader";
                if (!scene.Foreground.Tiles.Contains(td))
                {
                    scene.Foreground.Tiles.Add(td);
                }
            }
        }

        public void Close()
        {
            open = false;
            (bool returnState, Tile? returnTile) = ContentManager.Get<Tile>(td.TileID); if (returnState == true && returnTile is not null)
            {
                if (Parent is null)
                    return;
                if (Parent.CurrentScene is null)
                    return;
                if (Parent.CurrentScene.GetType() != typeof(Scenes.GameScene))
                    return;
                Scenes.GameScene? scene = (Scenes.GameScene)Parent.CurrentScene;
                if (scene is null)
                    return;
                returnTile.ShaderID = "Desaturated";
                if (!scene.Foreground.Tiles.Contains(td))
                {
                    scene.Foreground.Tiles.Add(td);
                }
            }
        }

        public override void Update(FrameEventArgs args)
        {
            //throw new NotImplementedException();
        }
    }
}
