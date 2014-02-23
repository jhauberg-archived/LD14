using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace LD14
{
    public class Piece
    {
        public Texture2D Icon { get; set; }

        public uint Row { get; set; }
        public uint Column { get; set; }

        public virtual int UnitsBonus
        {
            get
            {
                return 0;
            }
        }

        int units;

        public virtual int Units {
            get { 
                return units; 
            }
            set { 
                if (units != value) { 
                    units = value; 
                } 
            }
        }

        protected List<Action> actions;

        ReadOnlyCollection<Action> readonlyActions;

        protected Piece()
        {
            actions = new List<Action>();
            readonlyActions = new ReadOnlyCollection<Action>(actions);

            actions.Add(new Move(this));
        }

        public ReadOnlyCollection<Action> Actions { get { return readonlyActions; } }
    }

    public class EnemyPiece : Piece 
    {

    }

    public class RangedPiece : Piece { }

    public class AttackPiece : Piece 
    {
        public AttackPiece()
        {

        }
    }

    public class DefendPiece : Piece
    {
        public override int UnitsBonus
        {
            get
            {
                return Units;
            }
        }
    }

    public class BarracksPiece : Piece
    {
        public int ProductionUnits { get; set; }

        public override int UnitsBonus
        {
            get
            {
                return ProductionUnits;
            }
        }
    }

    public class Move : Action
    {
        public Move(Piece owner) : base(owner)
        {
            //Icon = GameContainer.ContentStore.Load<Texture2D>("whatever");
        }

        public override void Execute()
        {
            OnExecuted();
        }
    }

    public abstract class Action
    {
        readonly Piece owner;

        public Action(Piece owner)
        {
            this.owner = owner;
        }

        public Piece Owner { get { return owner; } }
        public Texture2D Icon { get; set; }

        public abstract void Execute();

        public event EventHandler Executed;

        protected void OnExecuted() { 
            if (Executed != null) { 
                Executed(this, EventArgs.Empty); 
            } 
        }
    }

    public partial class GameContainer : Microsoft.Xna.Framework.Game
    {   
        const uint Rows = 8;
        const uint Columns = 12;

        const float TileSize = 0.75f;

        const float PieceWidth = 0.5f;
        const float PieceHeight = 0.8f;

        const uint TurnQueueAmount = 10;

        public static GraphicsDeviceManager Graphics;
        public static ContentManager ContentStore;

        Matrix view, projection;

        Grid grid;

        Texture2D pieceShield, pieceSword, pieceArrow, pieceWall, pieceBarracks;
        Texture2D iconShield, iconSword, iconWall, iconArrow, iconBarracks;

        Texture2D infoRanged, infoAttacker, infoBarracks, infoDefender, infoWall;

        Texture2D currentIconCrown;

        Vector2 pieceScreenCoordinate;

        Effect fx;
        EffectParameter fxDiffuseMap;
        EffectParameter fxWorld;
        EffectParameter fxProjection;
        EffectParameter fxTint;

        VertexDeclaration pieceVertexDeclaration;
        VertexPositionTexture[] pieceVertices;

        BasicEffect bfx;

        VertexDeclaration backdropVertexDeclaration;
        VertexPositionColor[] backdropVertices;

        MouseState msLast;
        KeyboardState ksLast;

        List<Piece> pieces = new List<Piece>();

        List<Piece> enemyPieces = new List<Piece>();
        List<Piece> playerPieces = new List<Piece>();

        List<Piece> piecesToRemove = new List<Piece>();

        List<UnitCountAnimation> unitAnimations = new List<UnitCountAnimation>();

        List<Vector2> movableTiles = new List<Vector2>();

        SpriteBatch sprite;
        SpriteFont uiUnitFont, levelFont;

        Piece hoveredPiece, selectedPiece;

        float blah;

        bool gameStarted, gameEnded, gameWon;
        int playerPieceCurrentIndex;

        bool isPlacingPiece;
        Piece placingPiece;

        int level = 0;

        float levelIntroFadeRate = 0.5f;
        float levelIntroAlpha = 1;

        bool levelIntroIsFading;

        void FindMovableTiles(Piece piece)
        {
            movableTiles.Clear();

            int tileDistance = piece is AttackPiece ? 2 : 1;

            for (int col = (int)piece.Column - tileDistance; col <= piece.Column + tileDistance; col++) {
                for (int row = (int)piece.Row - tileDistance; row <= piece.Row + tileDistance; row++) {
                    movableTiles.Add(new Vector2(col, row));
                }
            }
        }

        public GameContainer()
        {
            Graphics = new GraphicsDeviceManager(this);
            Graphics.PreferredBackBufferHeight = 320;
            Graphics.PreferredBackBufferWidth = 480;
            Graphics.PreferMultiSampling = false;
            Graphics.SynchronizeWithVerticalRetrace = true;
            
            this.IsMouseVisible = true;
            this.Window.Title = "LD14 - Determined Wall";

            Content.RootDirectory = "Content";

            ContentStore = Content;
        }

        protected override void Initialize()
        {
            uiUnitFont = Content.Load<SpriteFont>("UIUnitFont");
            levelFont = Content.Load<SpriteFont>("LevelIntroFont");

            pieceWall = Content.Load<Texture2D>("piece_wall");
            pieceShield = Content.Load<Texture2D>("piece_shield");
            pieceSword = Content.Load<Texture2D>("piece_sword");
            pieceArrow = Content.Load<Texture2D>("piece_arrow");
            pieceBarracks = Content.Load<Texture2D>("piece_barracks");

            iconWall = Content.Load<Texture2D>("icon_wall");
            iconArrow = Content.Load<Texture2D>("icon_arrow");
            iconShield = Content.Load<Texture2D>("icon_shield");
            iconSword = Content.Load<Texture2D>("icon_sword");
            iconBarracks = Content.Load<Texture2D>("icon_barracks");

            currentIconCrown = Content.Load<Texture2D>("current_icon");

            infoWall = Content.Load<Texture2D>("info_wall");
            infoAttacker = Content.Load<Texture2D>("info_attacker");
            infoRanged = Content.Load<Texture2D>("info_ranged");
            infoDefender = Content.Load<Texture2D>("info_defender");
            infoBarracks = Content.Load<Texture2D>("info_barracks");

            fx = Content.Load<Effect>("ApplyTexture");
            fx.CurrentTechnique = fx.Techniques["Main"];

            fxWorld = fx.Parameters["World"];
            fxProjection = fx.Parameters["Projection"];
            fxDiffuseMap = fx.Parameters["DiffuseMap"];
            fxTint = fx.Parameters["Tint"];

            bfx = new BasicEffect(GraphicsDevice, null);

            bfx.VertexColorEnabled = true;
            bfx.LightingEnabled = false;
            bfx.TextureEnabled = false;

            msLast = Mouse.GetState();
            ksLast = Keyboard.GetState();

            FillPiecesLevelZero();
            //FillPiecesLevelFive();
            //FillPiecesLevelThree();
            //FillPiecesLevelOne();

            sprite = new SpriteBatch(GraphicsDevice);

            Vector3 bottomRight = new Vector3((Columns * TileSize), 0.0f, (Rows * TileSize));
            Vector3 middle = new Vector3((Columns / 2) * TileSize, 0.0f, (Rows / 2) * TileSize);

            //view = Matrix.CreateLookAt(middle + new Vector3(5.0f, 3.5f, 0) + (Vector3.Backward * 6.5f), middle, Vector3.Up);
            view = Matrix.CreateLookAt(middle + new Vector3(8.0f, 4.0f, 0) + (Vector3.Backward * 4.5f), middle, Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 100);

            grid = new Grid(Rows, Columns, TileSize);
            grid.MovableTiles = movableTiles;

            pieceVertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionTexture.VertexElements);

            Vector2 topleft = new Vector2(0, 0);
            Vector2 topright = new Vector2(1, 0);
            Vector2 lowerLeft = new Vector2(0, 1);
            Vector2 lowerRight = new Vector2(1, 1);

            float w = PieceWidth / 2;
            float h = PieceHeight / 2;

            pieceVertices = new VertexPositionTexture[4]
            {
                new VertexPositionTexture(new Vector3(-w, h, 0), topleft),
                new VertexPositionTexture(new Vector3(w, h, 0), topright),
                new VertexPositionTexture(new Vector3(-w, -h, 0), lowerLeft),
                new VertexPositionTexture(new Vector3(w, -h, 0), lowerRight)    
            };

            backdropVertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionColor.VertexElements);

            Color top = new Color(166, 193, 214);
            Color bottom = new Color(107, 120, 95);

            backdropVertices = new VertexPositionColor[4] {
                new VertexPositionColor(new Vector3(-1, 1, 0), bottom),
                new VertexPositionColor(new Vector3(1, 1, 0), top),
                new VertexPositionColor(new Vector3(-1, -1, 0), top),
                new VertexPositionColor(new Vector3(1, -1, 0), top)
            };

            base.Initialize();
        }

        Vector2 GetScreenSpaceCoordinate(uint row, uint column)
        {
            Vector3 source = GetWorldSpaceCoordinate(row, column);
            Vector3 worldToScreen = GraphicsDevice.Viewport.Project(source, projection, view, Matrix.Identity);

            return new Vector2(worldToScreen.X, worldToScreen.Y);
        }

        Vector3 GetWorldSpaceCoordinate(uint row, uint column)
        {
            float x = (column * TileSize) + (TileSize * 0.5f);
            float z = (row * TileSize) + (TileSize * 0.5f);

            return new Vector3(x, 0, z);
        }

        void Battle(Piece attacker, Piece defender, out int defenderShouldLoseUnits)
        {
            Debug.WriteLine(String.Format("{0} ({1}) attacks {2} ({3})", attacker, attacker.Units + attacker.UnitsBonus, defender, defender.Units));

            defenderShouldLoseUnits = 0;

            int defenderUnitsLeftAfterAttack = defender.Units - (attacker.Units + attacker.UnitsBonus);

            if (defenderUnitsLeftAfterAttack > 0) {
                defenderShouldLoseUnits = defender.Units - defenderUnitsLeftAfterAttack;
            } else {
                defenderShouldLoseUnits = defender.Units;
            }

            Debug.WriteLine(String.Format("{0} loses {1} units", defender, defenderShouldLoseUnits));
        }

        void PushBack(Piece piece)
        {
            List<Piece> piecesToMove = new List<Piece>();

            for (uint column = piece.Column; column < Columns; column++) {
                var piecesOnRowInColumn = pieces.Where(
                    p => p.Row == piece.Row && 
                         p.Column == column
                );

                if (piecesOnRowInColumn.Count() > 0) {
                    piecesToMove.Add(piecesOnRowInColumn.ElementAt(0));
                } else {
                    // hit empty tile, get the hell outta here!
                    break;
                }
            }

            foreach (Piece pieceToMove in piecesToMove) {
                pieceToMove.Column++;

                if (pieceToMove.Column >= Columns) {
                    Debug.WriteLine(String.Format("{0} was pushed out of the grid.", pieceToMove));

                    piecesToRemove.Add(pieceToMove);
                }
            }
        }

        void FlushRemovableGridPieces()
        {
            if (piecesToRemove.Count > 0) {
                foreach (Piece pieceToRemove in piecesToRemove) {
                    Debug.WriteLine(String.Format("Removing: {0}", pieceToRemove));

                    pieces.Remove(pieceToRemove);

                    if (enemyPieces.Contains(pieceToRemove)) {
                        enemyPieces.Remove(pieceToRemove);
                    } else if (playerPieces.Contains(pieceToRemove)) {
                        playerPieces.Remove(pieceToRemove);
                    }
                }

                piecesToRemove.Clear();

                if (playerPieces.Count == 0) {
                    Debug.WriteLine("Game over, bro.");

                    gameStarted = false;
                    gameEnded = true;
                    gameWon = false;

                    selectedPiece = null;

                    movableTiles.Clear();

                    grid.ShowMovableTiles = false;

                    playerPieceCurrentIndex = 0;
                } else if (enemyPieces.Count == 0) {
                    gameStarted = false;
                    gameEnded = true;
                    gameWon = true;

                    selectedPiece = null;

                    movableTiles.Clear();

                    grid.ShowMovableTiles = false;

                    playerPieceCurrentIndex = 0;
                }
            }
        }

        void Turn()
        {
            FlushRemovableGridPieces();

            playerPieceCurrentIndex++;

            if (playerPieceCurrentIndex >= playerPieces.Count) {
                playerPieceCurrentIndex = 0;

                // enemys turn, advance wall
                foreach (Piece enemyPiece in enemyPieces) {
                    // before moving, check for any possible attacks
                    var piecesToAttack = playerPieces.Where(
                        p => p.Column == enemyPiece.Column + 1 && p.Row == enemyPiece.Row
                    );

                    if (piecesToAttack.Count() > 0) {
                        Piece attacker = null;
                        Piece defender = null;

                        // do attack
                        foreach (Piece playerPiece in piecesToAttack) {
                            attacker = enemyPiece;
                            defender = playerPiece;

                            bool shouldPushBack = true;

                            int defenderShouldLoseUnits = 0;

                            Battle(attacker, defender, out defenderShouldLoseUnits);

                            unitAnimations.Add(new UnitCountAnimation(-defenderShouldLoseUnits, GetScreenSpaceCoordinate(defender.Row, defender.Column))
                            {
                                Font = uiUnitFont,
                                Sprite = sprite
                            });

                            if (defenderShouldLoseUnits == defender.Units) {
                                piecesToRemove.Add(defender);
                            } else {
                                defender.Units -= defenderShouldLoseUnits;

                                // retaliate
                                bool isDefender = defender is DefendPiece;

                                if (defender is AttackPiece || isDefender) {
                                    defenderShouldLoseUnits = 0;

                                    Battle(defender, attacker, out defenderShouldLoseUnits);

                                    unitAnimations.Add(new UnitCountAnimation(-defenderShouldLoseUnits, GetScreenSpaceCoordinate(attacker.Row, attacker.Column))
                                    {
                                        Font = uiUnitFont,
                                        Sprite = sprite
                                    });

                                    if (defenderShouldLoseUnits == attacker.Units) {
                                        piecesToRemove.Add(attacker);

                                        shouldPushBack = false;
                                    } else {
                                        attacker.Units -= defenderShouldLoseUnits;
                                    }
                                }

                                Debug.WriteLine(String.Format("{0} was pushed back", defender));

                                if (shouldPushBack) {
                                    PushBack(defender);
                                }
                            }
                        }
                    }

                    enemyPiece.Column++;

                    if (enemyPiece.Column >= Columns) {
                        gameStarted = false;
                        gameEnded = true;
                        gameWon = false;

                        selectedPiece = null;

                        movableTiles.Clear();

                        grid.ShowMovableTiles = false;

                        playerPieceCurrentIndex = 0;

                        Debug.WriteLine("Game over, bro.");
                    }
                }
            }

            FlushRemovableGridPieces();
        }

        void Reset()
        {
            gameWon = false;
            gameEnded = false;
            gameStarted = false;

            selectedPiece = null;

            movableTiles.Clear();

            pieces.Clear();
            playerPieces.Clear();
            enemyPieces.Clear();

            grid.ShowMovableTiles = false;

            playerPieceCurrentIndex = 0;

            levelIntroAlpha = 1;
        }

        protected override void Update(GameTime gameTime)
        {
            blah += 5 * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (levelIntroIsFading) {
                levelIntroAlpha -= levelIntroFadeRate * (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (levelIntroAlpha <= 0) {
                    levelIntroIsFading = false;
                }
            }

            MouseState ms = Mouse.GetState();
            KeyboardState ks = Keyboard.GetState();

            if (gameEnded) {
                if (ksLast.IsKeyDown(Keys.Enter) && ks.IsKeyUp(Keys.Enter)) {
                    if (gameWon) {
                        level++;
                    }

                    Reset();

                    switch (level) {
                        case 0: FillPiecesLevelZero(); break;

                        default:
                        case 1: {
                                level = 1;

                                FillPiecesLevelOne();
                            } break;

                        case 2: FillPiecesLevelTwo(); break;
                        case 3: FillPiecesLevelThree(); break;
                        case 4: FillPiecesLevelFour(); break;
                        case 5: FillPiecesLevelFive(); break;
                    }
                }
            }

            grid.Projection = projection;
            grid.View = view;

            grid.Update(gameTime);

            for (int i = unitAnimations.Count - 1; i >= 0; i--) {
                UnitCountAnimation unitAnim = unitAnimations[i];

                unitAnim.Update(gameTime);

                if (unitAnim.Ended) {
                    unitAnimations.Remove(unitAnim);
                }
            }

            hoveredPiece = null;

            if (grid.IsCursorWithinGridBounds) {
                var resultPieces = pieces.Where(
                        p => p.Column == grid.CursorHoveringOnColumn &&
                             p.Row == grid.CursorHoveringOnRow
                    );

                if (resultPieces.Count() > 0) {
                    hoveredPiece = resultPieces.ElementAt(0);
                }

                pieceScreenCoordinate = GetScreenSpaceCoordinate(grid.CursorHoveringOnRow, grid.CursorHoveringOnColumn);
            }

            if (gameStarted) {
                Piece previousPiece = selectedPiece;

                selectedPiece = playerPieces[playerPieceCurrentIndex];

                if (selectedPiece != previousPiece) {
                    FindMovableTiles(selectedPiece);

                    grid.ShowMovableTiles = true;
                }

                if (ksLast.IsKeyDown(Keys.Enter) && ks.IsKeyUp(Keys.Enter)) {
                    if (!isPlacingPiece) {
                        Debug.WriteLine("Skipping turn..");

                        Turn();
                    }
                } else if (selectedPiece is BarracksPiece) {
                    BarracksPiece barracks = selectedPiece as BarracksPiece;

                    if (ksLast.IsKeyDown(Keys.D1) && ks.IsKeyUp(Keys.D1)) {
                        // only toggle if the piece being placed is the same as the production of this action
                        isPlacingPiece = isPlacingPiece && placingPiece is AttackPiece ? !isPlacingPiece : true;

                        placingPiece = !isPlacingPiece ? 
                            null : 
                            new AttackPiece()
                            {
                                Icon = iconSword,
                                Units = barracks.ProductionUnits
                            };
                    } else if (ksLast.IsKeyDown(Keys.D2) && ks.IsKeyUp(Keys.D2)) {
                        isPlacingPiece = isPlacingPiece && placingPiece is RangedPiece ? !isPlacingPiece : true;

                        placingPiece = !isPlacingPiece ?
                            null :
                            new RangedPiece()
                            {
                                Icon = iconArrow,
                                Units = barracks.ProductionUnits
                            };
                    } else if (ksLast.IsKeyDown(Keys.D3) && ks.IsKeyUp(Keys.D3)) {
                        isPlacingPiece = isPlacingPiece && placingPiece is DefendPiece ? !isPlacingPiece : true;

                        placingPiece = !isPlacingPiece ?
                            null :
                            new DefendPiece()
                            {
                                Icon = iconShield,
                                Units = barracks.ProductionUnits
                            };
                    }

                    grid.ShowMovableTiles = !isPlacingPiece;
                }
            } else {
                if (!gameEnded) {
                    if (ksLast.IsKeyDown(Keys.Space) && ks.IsKeyUp(Keys.Space)) {
                        Debug.WriteLine("Game started..");

                        grid.ShowMovableTiles = true;

                        gameStarted = true;

                        levelIntroIsFading = true;
                    }
                }
            }

            if (msLast.LeftButton == ButtonState.Pressed && ms.LeftButton == ButtonState.Released) {
                if (grid.IsCursorWithinGridBounds) {
                    uint targetRow = grid.CursorHoveringOnRow;
                    uint targetColumn = grid.CursorHoveringOnColumn;

                    if (selectedPiece != null && !(selectedPiece is EnemyPiece)) {
                        bool rowInRange = true;
                        bool columnInRange = true;

                        if (gameStarted) {
                            int tileDistance = selectedPiece is AttackPiece ? (hoveredPiece != null && hoveredPiece is EnemyPiece ? 1 : 2) : 1;

                            if (targetColumn > selectedPiece.Column + tileDistance ||
                                targetColumn < selectedPiece.Column - tileDistance) {
                                columnInRange = false;
                            }

                            if (targetRow > selectedPiece.Row + tileDistance ||
                                targetRow < selectedPiece.Row - tileDistance) {
                                rowInRange = false;
                            }
                        } else {
                            if (targetColumn < 9) {
                                columnInRange = false;
                            }
                        }

                        bool didAct = false;

                        if (hoveredPiece == null) {
                            if (isPlacingPiece) {
                                placingPiece.Row = targetRow;
                                placingPiece.Column = targetColumn;

                                pieces.Add(placingPiece);
                                playerPieces.Add(placingPiece);

                                placingPiece = null;
                                isPlacingPiece = false;

                                didAct = true;
                            } else if (rowInRange && columnInRange) {
                                selectedPiece.Row = targetRow;
                                selectedPiece.Column = targetColumn;

                                didAct = true;
                            }
                        } else {
                            if (gameStarted && !isPlacingPiece) {
                                Piece attacker = selectedPiece;
                                Piece defender = hoveredPiece;

                                if (defender is EnemyPiece) {
                                    if (!(attacker is DefendPiece) && !(attacker is BarracksPiece)) {
                                        bool attackIsAllowed = 
                                            (attacker is AttackPiece && (rowInRange && columnInRange)) || 
                                            (attacker is RangedPiece && !(rowInRange && columnInRange));

                                        if (attackIsAllowed) {
                                            int defenderShouldLoseUnits = 0;

                                            Battle(attacker, defender, out defenderShouldLoseUnits);

                                            unitAnimations.Add(new UnitCountAnimation(-defenderShouldLoseUnits, GetScreenSpaceCoordinate(defender.Row, defender.Column))
                                            {
                                                Font = uiUnitFont,
                                                Sprite = sprite
                                            });

                                            if (defenderShouldLoseUnits == defender.Units) {
                                                piecesToRemove.Add(defender);
                                            } else {
                                                defender.Units -= defenderShouldLoseUnits;
                                            }

                                            didAct = true;
                                        }
                                    }
                                }
                            } else if (gameStarted && isPlacingPiece) {
                                if (!(hoveredPiece is EnemyPiece)) {
                                    if (hoveredPiece.GetType().Equals(placingPiece.GetType())) {
                                        hoveredPiece.Units += placingPiece.Units;

                                        unitAnimations.Add(new UnitCountAnimation(placingPiece.Units, GetScreenSpaceCoordinate(hoveredPiece.Row, hoveredPiece.Column))
                                        {
                                            Font = uiUnitFont,
                                            Sprite = sprite
                                        });

                                        isPlacingPiece = false;
                                        placingPiece = null;

                                        didAct = true;
                                    }
                                }
                            }
                        }

                        if (gameStarted && didAct) {
                            Turn();
                        }
                    }
                    
                    if (!gameStarted && !gameEnded && hoveredPiece != null) {
                        Piece previousPiece = selectedPiece;
                        selectedPiece = hoveredPiece;

                        if (selectedPiece != previousPiece) {
                            FindMovableTiles(selectedPiece);
                        }
                    }
                }
            }

            msLast = ms;
            ksLast = ks;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.RenderState.DepthBufferEnable = false;

            GraphicsDevice.VertexDeclaration = backdropVertexDeclaration;

            bfx.Begin();
            foreach (EffectPass pass in bfx.CurrentTechnique.Passes) {
                pass.Begin();

                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, backdropVertices, 0, 2);

                pass.End();
            }
            bfx.End();

            GraphicsDevice.RenderState.DepthBufferEnable = true;

            grid.Draw(gameTime);

            DrawGridPieces(gameTime);

            DrawUnitAnimations(gameTime);

            sprite.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
                if (!gameEnded && enemyPieces.Count > 0 && playerPieces.Count > 0) {
                    DrawTurnQueue(gameTime);
                }

                DrawActions(gameTime);

                DrawHoverInfoBubble(gameTime);

                if (gameEnded) {
                    string textResult = gameWon ? String.Format("Level {0} completed!", level) : String.Format("level {0} failed!", level);
                    string textContinue = gameWon ? "Press 'RETURN' to continue." : "Press 'ENTER' to try again.";

                    Vector2 textResultSize = uiUnitFont.MeasureString(textResult);
                    Vector2 textContinueSize = uiUnitFont.MeasureString(textContinue);

                    sprite.DrawString(uiUnitFont, textResult, new Vector2((GraphicsDevice.Viewport.Width / 2) - (textResultSize.X / 2), (GraphicsDevice.Viewport.Height / 5) - (textResultSize.Y / 2)), Color.Black);
                    sprite.DrawString(uiUnitFont, textContinue, new Vector2((GraphicsDevice.Viewport.Width / 2) - (textContinueSize.X / 2), (GraphicsDevice.Viewport.Height / 5) - (textContinueSize.Y / 2) + 18), Color.Black);
                }

                if (!gameStarted || levelIntroIsFading) {
                    Color levelIntroColor = Color.White;
                    Color levelIntroColorShadow = Color.Gray;

                    levelIntroColor.A = (byte)(255.0f * levelIntroAlpha);
                    levelIntroColorShadow.A = (byte)(255.0f * levelIntroAlpha);

                    string text = String.Format("Level {0}", level);
                    Vector2 textSize = levelFont.MeasureString(text);
                    Vector2 textPosition = new Vector2((GraphicsDevice.Viewport.Width / 2) - (textSize.X / 2), (GraphicsDevice.Viewport.Height / 2) - (textSize.Y / 2));

                    sprite.DrawString(levelFont, text, textPosition + Vector2.One, levelIntroColorShadow);
                    sprite.DrawString(levelFont, text, textPosition, levelIntroColor);
                }

            sprite.End();

            base.Draw(gameTime);
        }

        void DrawGridPieces(GameTime gameTime)
        {
            GraphicsDevice.RenderState.CullMode = CullMode.None;
            GraphicsDevice.VertexDeclaration = pieceVertexDeclaration;

            fxTint.SetValue(new Vector4(1, 1, 1, 1));
            fxProjection.SetValue(view * projection);

            Vector3 cameraPosition = Matrix.Invert(view).Translation;

            cameraPosition.Y = 0;

            foreach (Piece gridPiece in pieces) {
                if (gridPiece is EnemyPiece) {
                    fxDiffuseMap.SetValue(pieceWall);
                } else if (gridPiece is AttackPiece) {
                    fxDiffuseMap.SetValue(pieceSword);
                } else if (gridPiece is DefendPiece) {
                    fxDiffuseMap.SetValue(pieceShield);
                } else if (gridPiece is RangedPiece) {
                    fxDiffuseMap.SetValue(pieceArrow);
                } else if (gridPiece is BarracksPiece) {
                    fxDiffuseMap.SetValue(pieceBarracks);
                }

                Vector3 pieceWorldCoordinate = GetWorldSpaceCoordinate(gridPiece.Row, gridPiece.Column);

                pieceWorldCoordinate.Y = (PieceHeight * 0.5f);
                
                if (gridPiece == selectedPiece) {
                    pieceWorldCoordinate.Y += (float)Math.Abs(Math.Sin(blah)) * 0.25f;
                } else if (gridPiece.Column == grid.CursorHoveringOnColumn && gridPiece.Row == grid.CursorHoveringOnRow) {
                    pieceWorldCoordinate.Y += 0.1f;
                }

                fxWorld.SetValue(Matrix.CreateBillboard(pieceWorldCoordinate, cameraPosition, Vector3.Up, null));

                fx.Begin();
                foreach (EffectPass pass in fx.CurrentTechnique.Passes) {
                    pass.Begin();

                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, pieceVertices, 0, 2);

                    pass.End();
                }
                fx.End();
            }

            if (isPlacingPiece && hoveredPiece == null) {
                if (placingPiece is AttackPiece) {
                    fxDiffuseMap.SetValue(pieceSword);
                } else if (placingPiece is DefendPiece) {
                    fxDiffuseMap.SetValue(pieceShield);
                } else if (placingPiece is RangedPiece) {
                    fxDiffuseMap.SetValue(pieceArrow);
                }

                Vector3 pieceWorldCoordinate = GetWorldSpaceCoordinate(grid.CursorHoveringOnRow, grid.CursorHoveringOnColumn);

                pieceWorldCoordinate.Y = (PieceHeight * 0.5f);

                fxTint.SetValue(new Vector4(1, 1, 1, 0.5f));
                fxWorld.SetValue(Matrix.CreateBillboard(pieceWorldCoordinate, cameraPosition, Vector3.Up, null));

                fx.Begin();
                foreach (EffectPass pass in fx.CurrentTechnique.Passes) {
                    pass.Begin();

                    GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, pieceVertices, 0, 2);

                    pass.End();
                }
                fx.End();
            }
        }

        void DrawActions(GameTime gameTime)
        {
            Vector2 origin = new Vector2(22, GraphicsDevice.Viewport.Height - 25);
            Vector2 offset = new Vector2(0, 0);

            float keyOffset = 14;
            float actionOffset = 24;
            float heightOffset = 13;

            if (gameStarted) {
                sprite.DrawString(uiUnitFont, "[RET]", origin + offset - new Vector2(keyOffset, 0) + Vector2.One, Color.Black);
                sprite.DrawString(uiUnitFont, "[RET]", origin + offset - new Vector2(keyOffset, 0), Color.White);
                sprite.DrawString(uiUnitFont, selectedPiece is DefendPiece ? "Skip/Defend" : "Skip", origin + offset + new Vector2(actionOffset, 0), Color.Black);

                offset.Y -= heightOffset + (heightOffset * 0.55f);
                sprite.DrawString(uiUnitFont, "[LMB]", origin + offset - new Vector2(keyOffset, 0) + Vector2.One, Color.Black);
                sprite.DrawString(uiUnitFont, "[LMB]", origin + offset - new Vector2(keyOffset, 0), Color.White);
                sprite.DrawString(uiUnitFont, isPlacingPiece ? "Place" : "Move", origin + offset + new Vector2(actionOffset, 0), Color.Black);

                if (selectedPiece != null) {
                    if (selectedPiece is AttackPiece || selectedPiece is RangedPiece) {
                        offset.Y -= heightOffset;
                        sprite.DrawString(uiUnitFont, "[LMB]", origin + offset - new Vector2(keyOffset, 0) + Vector2.One, Color.Black);
                        sprite.DrawString(uiUnitFont, "[LMB]", origin + offset - new Vector2(keyOffset, 0), Color.White);
                        sprite.DrawString(uiUnitFont, "Attack", origin + offset + new Vector2(actionOffset, 0), Color.Black);
                    } else if (selectedPiece is BarracksPiece) {
                        offset.Y -= heightOffset;
                        sprite.DrawString(uiUnitFont, "[1]", origin + offset - new Vector2(keyOffset, 0) + Vector2.One, Color.Black);
                        sprite.DrawString(uiUnitFont, "[1]", origin + offset - new Vector2(keyOffset, 0), Color.White);
                        sprite.DrawString(uiUnitFont, "Produce Melee", origin + offset + new Vector2(actionOffset, 0), Color.Black);

                        offset.Y -= heightOffset;
                        sprite.DrawString(uiUnitFont, "[2]", origin + offset - new Vector2(keyOffset, 0) + Vector2.One, Color.Black);
                        sprite.DrawString(uiUnitFont, "[2]", origin + offset - new Vector2(keyOffset, 0), Color.White);
                        sprite.DrawString(uiUnitFont, "Produce Ranged", origin + offset + new Vector2(actionOffset, 0), Color.Black);

                        offset.Y -= heightOffset;
                        sprite.DrawString(uiUnitFont, "[3]", origin + offset - new Vector2(keyOffset, 0) + Vector2.One, Color.Black);
                        sprite.DrawString(uiUnitFont, "[3]", origin + offset - new Vector2(keyOffset, 0), Color.White);
                        sprite.DrawString(uiUnitFont, "Produce Defender", origin + offset + new Vector2(actionOffset, 0), Color.Black);
                    }
                }
            } else {
                if (!gameEnded) {
                    sprite.DrawString(uiUnitFont, "[SPC]", origin + offset - new Vector2(keyOffset, 0) + Vector2.One, Color.Black);
                    sprite.DrawString(uiUnitFont, "[SPC]", origin + offset - new Vector2(keyOffset, 0), Color.White);
                    sprite.DrawString(uiUnitFont, "Start", origin + offset + new Vector2(actionOffset, 0), Color.Black);
                }
            }
        }

        void DrawUnitAnimations(GameTime gameTime)
        {
            for (int i = unitAnimations.Count - 1; i >= 0; i--) {
                UnitCountAnimation unitAnim = unitAnimations[i];

                unitAnim.Draw(gameTime);
            }
        }

        void DrawHoverInfoBubble(GameTime gameTime)
        {
            if (hoveredPiece != null) {
                Texture2D tex = null;

                if (hoveredPiece is EnemyPiece) {
                    tex = infoWall;
                } else if (hoveredPiece is AttackPiece) {
                    tex = infoAttacker;
                } else if (hoveredPiece is DefendPiece) {
                    tex = infoDefender;
                } else if (hoveredPiece is RangedPiece) {
                    tex = infoRanged;
                } else if (hoveredPiece is BarracksPiece) {
                    tex = infoBarracks;
                }

                Vector2 infoPosition = new Vector2(pieceScreenCoordinate.X + 10, pieceScreenCoordinate.Y - tex.Height * 1.2f);

                if (infoPosition.X + tex.Width > GraphicsDevice.Viewport.Width) {
                    infoPosition.X -= (infoPosition.X + tex.Width) - GraphicsDevice.Viewport.Width;
                }

                sprite.Draw(tex, infoPosition, Color.White);

                if (hoveredPiece is EnemyPiece) {
                    Vector2 pos = infoPosition + new Vector2(20, tex.Height - 10);

                    sprite.DrawString(uiUnitFont, String.Format("{0}", hoveredPiece.Units), pos, Color.Black);
                }
            }
        }

        Piece Next(int currentIndex)
        {
            currentIndex++;

            Piece nextPiece = null;

            if (currentIndex >= playerPieces.Count) {
                if (enemyPieces.Count > 0) {
                    nextPiece = enemyPieces[0]; // doesnt matter which piece it is
                }
            } else {
                nextPiece = playerPieces[currentIndex];
            }

            return nextPiece;
        }

        void DrawTurnQueueItem(Piece piece, Vector2 position)
        {
            float hoverAmount = 8;

            bool pieceIsAlsoHovered = piece == hoveredPiece && !(piece is EnemyPiece);

            // nullpoint exception ved piece.icon naar alle walls er draebt
            sprite.Draw(piece.Icon, position + new Vector2(0, pieceIsAlsoHovered ? -hoverAmount : 0), Color.White);

            int units = 0;

            if (piece is EnemyPiece) {
                enemyPieces.ForEach(enemy => units += enemy.Units);
            } else {
                units = piece.Units;
            }

            string text = String.Format("{0}", units);
            Vector2 textSize = uiUnitFont.MeasureString(text);

            sprite.DrawString(uiUnitFont, text, position + new Vector2(piece.Icon.Width / 2, piece.Icon.Height) - new Vector2((textSize.X / 2), 0), Color.Black);

            if (piece.UnitsBonus > 0) {
                text = String.Format("(+{0})", piece.UnitsBonus);
                textSize = uiUnitFont.MeasureString(text);

                sprite.DrawString(uiUnitFont, text, position + new Vector2(piece.Icon.Width / 2, piece.Icon.Height + textSize.Y * 0.75f) - new Vector2((textSize.X / 2), 0), Color.Black);
            }
        }

        void DrawTurnQueue(GameTime gameTime)
        {
            int index = playerPieceCurrentIndex;
            
            Piece current = null;

            Vector2 origin = new Vector2(15, 15);

            float xoffset = 0;

            if (playerPieces.Count > 0) {
                current = playerPieces[index];

                DrawTurnQueueItem(current, origin);

                xoffset = current.Icon.Width;
            }

            for (uint i = 0; i < TurnQueueAmount; i++) {
                current = Next(index++);

                DrawTurnQueueItem(current, origin + new Vector2(xoffset, 0));

                xoffset += current.Icon.Width;

                if (current is EnemyPiece) {
                    index = -1;
                }
            }

            if (gameStarted) {
                // hack
                float hoverAmount = 8;

                bool pieceIsAlsoHovered = selectedPiece == hoveredPiece && !(current is EnemyPiece);

                sprite.Draw(currentIconCrown, origin - new Vector2(11, 5) + new Vector2(0, pieceIsAlsoHovered ? -hoverAmount : 0), Color.White);
            }
        }
    }
}
