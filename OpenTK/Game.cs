﻿using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;

namespace KgOpenTK
{
    class Game : GameWindow
    {
        [STAThread]
        static void Main()
        {
            using (var Game = new Game())
            {
                Game.Run(30);
            }
        }

        private Random Rand;
        Font serif = new Font(FontFamily.GenericSerif, 24);

        private const int MapWidth = 13; //16
        private const int MapHeight = 13;
        private const int StickLength = 4;
        private int[] StickColors;
        private Vector2 StickPosition;
        private const int ColorsCount = 5;
        private const int NominalWidth = 700;
        private const int NominalHeight = 500;
        private float ProjectionWidth;
        private float ProjectionHeight;
        private const int SolidSize = 35;
        private Color4[] Colors = { Color4.Red, Color4.Green, Color4.Blue, Color4.Brown, Color4.Yellow };
        private int[,] Map;
        private enum GameStateEnum
        {
            Fall,
            Impact,
            GameOver
        }
        private GameStateEnum GameState;
        private const float FallSpeed = 0.02f;
        private float[,] ImpactFallOffset;
        private const int DestroyableLength = 5;
        private Stack<Vector2> Destroyables = new Stack<Vector2>();
        private Texture TextureBackground;
        private Texture[] ColorTextures = new Texture[ColorsCount];
        private int Score;
        private int HighScore;
        private TextRenderer NextStickLabel, ScoreLabel, ScoreRenderer, HighScoreLabel, HighScoreRenderer, GameOverLabel, GameOverHint;
        private int[] NextStickColors;
        private int TotalDestroyedThisMove;
        private string HighScoreFilename;
        private bool Paused;
        private TextRenderer PauseLabel, UnpauseHint, PlayingGameLabel, PauseHint;

        public Game()
            : base(NominalWidth, NominalHeight, GraphicsMode.Default, "Computer's grafics 4") {
            VSync = VSyncMode.On;

            KeyDown += new EventHandler<KeyboardKeyEventArgs>(OnKeyDown);

            TextureBackground = new Texture(new Bitmap("C:/Users/User/source/repos/KgOpenGL/OpenTK/textures/background.png"));
            for (var i = 0; i < ColorsCount; i++)
            {
                string str = "C:/Users/User/source/repos/KgOpenGL/OpenTK/textures/solids/" + i + ".png";
                ColorTextures[i] = new Texture(new Bitmap(str));
            }

            var LabelFont = new Font(new FontFamily(GenericFontFamilies.SansSerif), 20, GraphicsUnit.Pixel);
            var LabelColor = Color4.SteelBlue;
            NextStickLabel = new TextRenderer(LabelFont, LabelColor, "Next");
            ScoreLabel = new TextRenderer(LabelFont, LabelColor, "Score");
            HighScoreLabel = new TextRenderer(LabelFont, LabelColor, "High score");

            var ScoreFont = new Font(new FontFamily(GenericFontFamilies.SansSerif), 50, GraphicsUnit.Pixel);
            var ScoreColor = Color4.Tomato;
            ScoreRenderer = new TextRenderer(ScoreFont, ScoreColor);
            HighScoreRenderer = new TextRenderer(ScoreFont, ScoreColor);

            var GameStateFont = new Font(new FontFamily(GenericFontFamilies.SansSerif), 30, GraphicsUnit.Pixel);
            var GameStateColor = Color4.Tomato;
            GameOverLabel = new TextRenderer(GameStateFont, GameStateColor, "Game over");
            PauseLabel = new TextRenderer(GameStateFont, GameStateColor, "Pause");
            PlayingGameLabel = new TextRenderer(GameStateFont, GameStateColor, "Playing");

            var GameStateHintFont = new Font(new FontFamily(GenericFontFamilies.SansSerif), 25, GraphicsUnit.Pixel);
            var GameStateHintColor = Color4.SteelBlue;
            GameOverHint = new TextRenderer(GameStateHintFont, GameStateHintColor, "Press Enter");
            UnpauseHint = new TextRenderer(GameStateHintFont, GameStateHintColor, "Press Space");
            PauseHint = new TextRenderer(GameStateHintFont, GameStateHintColor, "Space pauses");

            var ConfigDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "ImpressiveSolids";
            if (!Directory.Exists(ConfigDirectory))
            {
                Directory.CreateDirectory(ConfigDirectory);
            }

            HighScoreFilename = ConfigDirectory + Path.DirectorySeparatorChar + "HighScore.dat";
            if (File.Exists(HighScoreFilename))
            {
                using (var Stream = new FileStream(HighScoreFilename, FileMode.Open))
                {
                    using (var Reader = new BinaryReader(Stream))
                    {
                        try
                        {
                            HighScore = Reader.ReadInt32();
                        }
                        catch (IOException)
                        {
                            HighScore = 0;
                        }
                    }
                }
            }
            else
            {
                HighScore = 0;
            }
        }

        protected override void OnLoad(EventArgs E)
        {
            base.OnLoad(E);

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);

            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            New();
            Paused = true;
        }

        protected override void OnResize(EventArgs E)
        {
            base.OnResize(E);
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            ProjectionWidth = NominalWidth;
            ProjectionHeight = (float)ClientRectangle.Height / (float)ClientRectangle.Width * ProjectionWidth;
            if (ProjectionHeight < NominalHeight)
            {
                ProjectionHeight = NominalHeight;
                ProjectionWidth = (float)ClientRectangle.Width / (float)ClientRectangle.Height * ProjectionHeight;
            }
            if (ClientSize.Width < NominalWidth)
            {
                ClientSize = new Size(NominalWidth, ClientSize.Height);
            }
            if (ClientSize.Height < NominalHeight)
            {
                ClientSize = new Size(ClientSize.Width, NominalHeight);
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs E)
        {
            base.OnUpdateFrame(E);

            if (Paused)
            {
                return;
            }

            if (GameStateEnum.Fall == GameState)
            {
                StickPosition.Y += FallSpeed;

                var FellOnFloor = (StickPosition.Y >= MapHeight - 1);

            var FellOnBlock = false;
            if (!FellOnFloor)
            {
                var Y = (int)Math.Floor(StickPosition.Y + 1);
                for (var i = 0; i < StickLength; i++)
                {
                    var X = (int)StickPosition.X + i;
                    if (Map[X, Y] >= 0)
                    {
                        FellOnBlock = true;
                        break;
                    }
                }
            }

                if (FellOnFloor || FellOnBlock)
                {
                    var Y = (int)Math.Floor(StickPosition.Y);
                    for (var i = 0; i < StickLength; i++)
                    {
                        var X = (int)StickPosition.X + i;
                        Map[X, Y] = StickColors[i];
                    }
                    GameState = GameStateEnum.Impact;
                }
            }
            else if (GameStateEnum.Impact == GameState)
            {
                var Stabilized = true;
                for (var X = 0; X < MapWidth; X++)
                {
                    for (var Y = MapHeight - 2; Y >= 0; Y--)
                    {
                        if ((Map[X, Y] >= 0) && ((Map[X, Y + 1] < 0) || (ImpactFallOffset[X, Y + 1] > 0)))
                        {
                            Stabilized = false;
                            ImpactFallOffset[X, Y] += 1.0f;
                            if (ImpactFallOffset[X, Y] >= 1)
                            {
                                Map[X, Y + 1] = Map[X, Y];
                                Map[X, Y] = -1;
                                ImpactFallOffset[X, Y] = 0;
                            }
                        }
                    }
                }

                if (Stabilized)
                {
                    Destroyables.Clear();

                    for (var X = 0; X < MapWidth; X++)
                    {
                        for (var Y = 0; Y < MapHeight; Y++)
                        {
                            CheckDestroyableLine(X, Y, 1, 0);
                            CheckDestroyableLine(X, Y, 0, 1);
                            CheckDestroyableLine(X, Y, 1, 1);
                            CheckDestroyableLine(X, Y, 1, -1);
                        }
                    }

                    if (Destroyables.Count > 0)
                    {
                        foreach (var Coords in Destroyables)
                        {
                            Map[(int)Coords.X, (int)Coords.Y] = -1;
                        }
                        Score += (int)Math.Ceiling(Destroyables.Count + Math.Pow(1.5, Destroyables.Count - 3) - 1) + TotalDestroyedThisMove;
                        TotalDestroyedThisMove += Destroyables.Count;
                        Stabilized = false;
                    }
                }

                if (Stabilized)
                {
                    var GameOver = false;
                    for (var X = 0; X < MapWidth; X++)
                    {
                        if (Map[X, 0] >= 0)
                        {
                            GameOver = true;
                            break;
                        }
                    }

                    if (GameOver)
                    {
                        GameState = GameStateEnum.GameOver;

                        if (Score > HighScore)
                        {
                            HighScore = Score;
                            using (var Stream = new FileStream(HighScoreFilename, FileMode.Create))
                            {
                                using (var Writer = new BinaryWriter(Stream))
                                {
                                    Writer.Write(HighScore);
                                }
                            }
                        }
                    }
                    else
                    {
                        GenerateNextStick();
                        TotalDestroyedThisMove = 0;
                        GameState = GameStateEnum.Fall;
                    }
                }
            }
        }

        private void RenderPipe()
        {
            GL.Disable(EnableCap.Texture2D);
            GL.Color4(Color4.Black);

            GL.Begin(BeginMode.Quads);
            GL.Vertex2(0, 0);
            GL.Vertex2(MapWidth * SolidSize, 0);
            GL.Vertex2(MapWidth * SolidSize, MapHeight * SolidSize);
            GL.Vertex2(0, MapHeight * SolidSize);
            GL.End();

            GL.Enable(EnableCap.Texture2D);
        }

        private void RenderBackground()
        {
            TextureBackground.Bind();
            GL.Color4(Color4.White);
            GL.Begin(BeginMode.Quads);

            GL.TexCoord2(0, 0);
            GL.Vertex2(0, 0);

            GL.TexCoord2((float)ClientRectangle.Width / TextureBackground.Width, 0);
            GL.Vertex2(ProjectionWidth, 0);

            GL.TexCoord2((float)ClientRectangle.Width / TextureBackground.Width, (float)ClientRectangle.Height / TextureBackground.Height);
            GL.Vertex2(ProjectionWidth, ProjectionHeight);

            GL.TexCoord2(0, (float)ClientRectangle.Height / TextureBackground.Height);
            GL.Vertex2(0, ProjectionHeight);

            GL.End();
        }

        protected override void OnRenderFrame(FrameEventArgs E)
        {
            base.OnRenderFrame(E);

            GL.ClearColor(Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            var Projection = Matrix4.CreateOrthographic(-ProjectionWidth, -ProjectionHeight, -1, 1);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref Projection);
            GL.Translate(ProjectionWidth / 2, -ProjectionHeight / 2, 0);

            var Modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref Modelview);

            RenderBackground();

            var PipeMarginY = (ProjectionHeight - MapHeight * SolidSize) / 2f;
            var PipeMarginX = (NominalHeight - MapHeight * SolidSize) / 2f;

            var Overwidth = ProjectionWidth - ProjectionHeight * (float)NominalWidth / NominalHeight;
            if (Overwidth > 0)
            {
                GL.Translate(Math.Min(Overwidth, (ProjectionWidth - MapWidth * SolidSize) / 2f), PipeMarginY, 0);
            }
            else
            {
                GL.Translate(PipeMarginX, PipeMarginY, 0);
            }

            RenderPipe();

            for (var X = 0; X < MapWidth; X++)
            {
                for (var Y = 0; Y < MapHeight; Y++)
                {
                    if (Map[X, Y] >= 0)
                    {
                        RenderSolid(X, Y + ImpactFallOffset[X, Y], Map[X, Y]);
                    }
                }
            }

            if (GameStateEnum.Fall == GameState)
            {
                for (var i = 0; i < StickLength; i++)
                {
                    RenderSolid(StickPosition.X + i, StickPosition.Y, StickColors[i]);
                }
            }

            GL.Translate(MapWidth * SolidSize + PipeMarginX, 0, 0);

            NextStickLabel.Render();
            GL.Translate(0, NextStickLabel.Height, 0);
            RenderNextStick();
            GL.Translate(0, -NextStickLabel.Height, 0);

            GL.Translate(0, MapHeight * SolidSize / 4f, 0);
            if (GameStateEnum.GameOver == GameState)
            {
                GameOverLabel.Render();
                GL.Translate(0, GameOverLabel.Height, 0);
                GameOverHint.Render();
                GL.Translate(0, -GameOverLabel.Height, 0);
            }
            else if (Paused)
            {
                PauseLabel.Render();
                GL.Translate(0, PauseLabel.Height, 0);
                UnpauseHint.Render();
                GL.Translate(0, -PauseLabel.Height, 0);
            }
            else
            {
                PlayingGameLabel.Render();
                GL.Translate(0, PlayingGameLabel.Height, 0);
                PauseHint.Render();
                GL.Translate(0, -PlayingGameLabel.Height, 0);
            }

            GL.Translate(0, MapHeight * SolidSize / 4f, 0);
            ScoreLabel.Render();
            GL.Translate(0, ScoreLabel.Height, 0);
            ScoreRenderer.Label = Score.ToString();
            ScoreRenderer.Render();
            GL.Translate(0, -ScoreLabel.Height, 0);

            GL.Translate(0, MapHeight * SolidSize / 4f, 0);
            HighScoreLabel.Render();
            GL.Translate(0, HighScoreLabel.Height, 0);
            HighScoreRenderer.Label = HighScore.ToString();
            HighScoreRenderer.Render();


            SwapBuffers();
        }

        private void RenderSolid(float X, float Y, int Color)
        {
            ColorTextures[Color].Bind();
            GL.Color4(Color4.White);
            GL.Begin(BeginMode.Quads);

            GL.TexCoord2(0, 0);
            GL.Vertex2(X * SolidSize, Y * SolidSize);

            GL.TexCoord2(1, 0);
            GL.Vertex2((X + 1) * SolidSize, Y * SolidSize);

            GL.TexCoord2(1, 1);
            GL.Vertex2((X + 1) * SolidSize, (Y + 1) * SolidSize);

            GL.TexCoord2(0, 1);
            GL.Vertex2(X * SolidSize, (Y + 1) * SolidSize);

            GL.End();

            /*GL.Color4(Colors[Color]);
            GL.Vertex2(X * SolidSize, Y * SolidSize);
            GL.Vertex2((X + 1) * SolidSize, Y * SolidSize);
            GL.Vertex2((X + 1) * SolidSize, (Y + 1) * SolidSize);
            GL.Vertex2(X * SolidSize, (Y + 1) * SolidSize);*/
        }

        protected void OnKeyDown(object Sender, KeyboardKeyEventArgs E)
        {
            if ((GameStateEnum.Fall == GameState) && !Paused)
            {
                if ((Key.Left == E.Key) && (StickPosition.X > 0))
                {
                    --StickPosition.X;
                }
                else if ((Key.Right == E.Key) && (StickPosition.X + StickLength < MapWidth))
                {
                    ++StickPosition.X;
                }
                else if (Key.Up == E.Key)
                {
                    var T = StickColors[0];
                    for (var i = 0; i < StickLength - 1; i++)
                    {
                        StickColors[i] = StickColors[i + 1];
                    }
                    StickColors[StickLength - 1] = T;
                }
                else if (Key.Down == E.Key)
                {
                    StickPosition.Y += 1.0f;
                }
            }
            else if (GameStateEnum.GameOver == GameState)
            {
                if ((Key.Enter == E.Key) || (Key.KeypadEnter == E.Key))
                {
                    New();
                }
            }

            if (((GameStateEnum.Fall == GameState) || (GameStateEnum.Impact == GameState)) && (Key.Space == E.Key))
            {
                Paused = !Paused;
            }
        }

        private void New()
        {
            Rand = new Random();

            Map = new int[MapWidth, MapHeight];
            for (var X = 0; X < MapWidth; X++)
            {
                for (var Y = 0; Y < MapHeight; Y++)
                {
                    Map[X, Y] = -1;
                }
            }

            StickColors = new int[StickLength];
            NextStickColors = new int[StickLength];
            GenerateNextStick();
            GenerateNextStick(); // because 1st call makes current stick all zeros
            GameState = GameStateEnum.Fall;
            ImpactFallOffset = new float[MapWidth, MapHeight];
            Score = 0;
            TotalDestroyedThisMove = 0;
        }

        private void GenerateNextStick()
        {
            for (var i = 0; i < StickLength; i++)
            {
                StickColors[i] = NextStickColors[i];
                NextStickColors[i] = Rand.Next(ColorsCount);
            }
            StickPosition.X = (float)Math.Floor((MapWidth - StickLength) / 2d);
            StickPosition.Y = 0;
        }

        private void CheckDestroyableLine(int X1, int Y1, int DeltaX, int DeltaY)
        {
            if (Map[X1, Y1] < 0)
            {
                return;
            }

            int X2 = X1, Y2 = Y1;
            var LineLength = 0;
            while ((X2 >= 0) && (Y2 >= 0) && (X2 < MapWidth) && (Y2 < MapHeight) && (Map[X2, Y2] == Map[X1, Y1]))
            {
                ++LineLength;
                X2 += DeltaX;
                Y2 += DeltaY;
            }

            if (LineLength >= DestroyableLength)
            {
                for (var i = 0; i < LineLength; i++)
                {
                    Destroyables.Push(new Vector2(X1 + i * DeltaX, Y1 + i * DeltaY));
                }
            }
        }

        public void RenderNextStick()
        {
            GL.Disable(EnableCap.Texture2D);
            GL.Color4(Color4.Black);

            GL.Begin(BeginMode.Quads);
            GL.Vertex2(0, 0);
            GL.Vertex2(StickLength * SolidSize, 0);
            GL.Vertex2(StickLength * SolidSize, SolidSize);
            GL.Vertex2(0, SolidSize);
            GL.End();

            GL.Enable(EnableCap.Texture2D);

            for (var i = 0; i < StickLength; i++)
            {
                RenderSolid(i, 0, NextStickColors[i]);
            }
        }
    }
}