using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD14
{
    public sealed class Grid : IDrawable, IUpdateable
    {
        readonly uint rows;
        readonly uint columns;
        readonly float tileSize;

        VertexDeclaration gridVertexDeclaration;

        VertexPositionColor[] gridVertices;
        VertexPositionColor[] gridFillVertices;
        VertexPositionColor[] gridSelectionVertices;

        BasicEffect bfx;

        GraphicsDevice device;

        bool isHoveringOnTile;
        uint tx, tz;

        public bool IsCursorWithinGridBounds { get { return isHoveringOnTile; } }

        public uint CursorHoveringOnColumn { get { return tx; } }
        public uint CursorHoveringOnRow { get { return tz; } }

        public uint Columns { get { return columns; } }
        public uint Rows { get { return rows; } }

        public Matrix View { get; set; }
        public Matrix Projection { get; set; }

        public List<Vector2> MovableTiles { get; set; }
        public bool ShowMovableTiles { get; set; }

        public Grid(uint rows, uint columns, float tileSize)
        {
            this.rows = rows;
            this.columns = columns;

            this.tileSize = tileSize;

            device = GameContainer.Graphics.GraphicsDevice;

            ConstructGrid();

            gridVertexDeclaration = new VertexDeclaration(device, VertexPositionColor.VertexElements);

            bfx = new BasicEffect(device, null);

            bfx.TextureEnabled = false;
            bfx.LightingEnabled = false;
            bfx.VertexColorEnabled = true;

            //ShowMovableTiles = true;
        }

        void ConstructGrid()
        {
            uint rowVertexCount = (rows + 1) * 2;
            uint columnVertexCount = (columns + 1) * 2;

            gridVertices = new VertexPositionColor[rowVertexCount + columnVertexCount];

            float width = columns * tileSize;
            float height = rows * tileSize;

            Color vertexColor = Color.Black;

            uint n = 0;
            for (uint row = 0; row <= rows; row++) {
                float z = row * tileSize;

                Vector3 a = new Vector3(0, 0, z);
                Vector3 b = new Vector3(width, 0, z);

                gridVertices[n] = new VertexPositionColor(a, vertexColor);
                gridVertices[n + 1] = new VertexPositionColor(b, vertexColor);

                n += 2;
            }

            n = 0;
            for (uint column = 0; column <= columns; column++) {
                float x = column * tileSize;

                Vector3 a = new Vector3(x, 0, 0);
                Vector3 b = new Vector3(x, 0, height);

                gridVertices[rowVertexCount + n] = new VertexPositionColor(a, vertexColor);
                gridVertices[rowVertexCount + n + 1] = new VertexPositionColor(b, vertexColor);

                n += 2;
            }

            Color vertexFillColor = Color.Green;

            gridFillVertices = new VertexPositionColor[4] {
                new VertexPositionColor(new Vector3(0, -0.002f, 0), vertexFillColor),
                new VertexPositionColor(new Vector3(width, -0.002f, 0), vertexFillColor),
                new VertexPositionColor(new Vector3(0, -0.002f, height), vertexFillColor),
                new VertexPositionColor(new Vector3(width, -0.002f, height), vertexFillColor)
            };

            Color vertexSelectionFillColor = Color.Green;

            gridSelectionVertices = new VertexPositionColor[4] {
                new VertexPositionColor(new Vector3(0, -0.001f, 0), vertexSelectionFillColor),
                new VertexPositionColor(new Vector3(tileSize, -0.001f, 0), vertexSelectionFillColor),
                new VertexPositionColor(new Vector3(0, -0.001f, tileSize), vertexSelectionFillColor),
                new VertexPositionColor(new Vector3(tileSize, -0.001f, tileSize), vertexSelectionFillColor)
            };
        }

        #region IDrawable Members

        public void Draw(GameTime gameTime)
        {
            device.VertexDeclaration = gridVertexDeclaration;

            device.RenderState.AlphaBlendEnable = true;
            device.RenderState.SourceBlend = Blend.SourceAlpha;
            device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;

            bfx.Projection = Projection;
            bfx.View = View;

            bfx.World = Matrix.Identity;

            bfx.Alpha = 0.35f;

            bfx.Begin();
            foreach (EffectPass pass in bfx.CurrentTechnique.Passes) {
                pass.Begin();

                device.DrawUserPrimitives(PrimitiveType.LineList, gridVertices, 0, gridVertices.Length / 2);
                device.DrawUserPrimitives(PrimitiveType.TriangleStrip, gridFillVertices, 0, 2);

                pass.End();
            }
            bfx.End();

            bfx.Alpha = 0.45f;

            if (MovableTiles.Count > 0 && ShowMovableTiles) {
                foreach (Vector2 tile in MovableTiles) {
                    if (tile.X >= columns || tile.X < 0 || tile.Y >= rows || tile.Y < 0) {
                        continue;
                    }

                    bfx.World = Matrix.CreateTranslation(new Vector3(tile.X * tileSize, 0, tile.Y * tileSize));

                    bfx.Begin();
                    foreach (EffectPass pass in bfx.CurrentTechnique.Passes) {
                        pass.Begin();

                        device.DrawUserPrimitives(PrimitiveType.TriangleStrip, gridSelectionVertices, 0, 2);

                        pass.End();
                    }
                    bfx.End();
                }
            }

            bfx.Alpha = 0.35f;

            if (isHoveringOnTile) {
                bfx.World = Matrix.CreateTranslation(new Vector3(tx * tileSize, 0, tz * tileSize));

                bfx.Begin();
                foreach (EffectPass pass in bfx.CurrentTechnique.Passes) {
                    pass.Begin();

                    device.DrawUserPrimitives(PrimitiveType.TriangleStrip, gridSelectionVertices, 0, 2);

                    pass.End();
                }
                bfx.End();
            }
        }

        public int DrawOrder
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler DrawOrderChanged;

        public bool Visible
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler VisibleChanged;

        #endregion

        #region IUpdateable Members

        public bool Enabled
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler EnabledChanged;

        public void Update(GameTime gameTime)
        {
            MouseState ms = Mouse.GetState();

            Vector3 nearsource = new Vector3((float)ms.X, (float)ms.Y, 0f);
            Vector3 farsource = new Vector3((float)ms.X, (float)ms.Y, 1f);

            Matrix world = Matrix.CreateTranslation(0, 0, 0);

            Vector3 nearPoint = device.Viewport.Unproject(nearsource, Projection, View, world);
            Vector3 farPoint = device.Viewport.Unproject(farsource, Projection, View, world);

            Vector3 direction = Vector3.Normalize(farPoint - nearPoint);

            Ray ray = new Ray(nearPoint, direction);

            Plane plane = new Plane(Vector3.Up, 0);

            float? distance = ray.Intersects(plane);

            if (distance.HasValue) {
                Vector3 hit = ray.Position + (direction * distance.Value);

                tx = (uint)(hit.X / tileSize);
                tz = (uint)(hit.Z / tileSize);

                isHoveringOnTile = tx < columns &&
                                      tz < rows;
            }
        }

        public int UpdateOrder
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler UpdateOrderChanged;

        #endregion
    }
}
