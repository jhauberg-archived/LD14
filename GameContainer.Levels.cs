using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LD14
{
    public partial class GameContainer
    {
        void FillPiecesLevelZero()
        {
            EnemyPiece e = new EnemyPiece()
            {
                Column = 0,
                Row = 6,
                Units = 1,
                Icon = iconWall
            };

            pieces.Add(e);
            enemyPieces.Add(e);

            EnemyPiece e2 = new EnemyPiece()
            {
                Column = 0,
                Row = 5,
                Units = 1,
                Icon = iconWall
            };

            pieces.Add(e2);
            enemyPieces.Add(e2);

            RangedPiece ra = new RangedPiece()
            {
                Column = 10,
                Row = 7,
                Units = 2,
                Icon = iconArrow
            };

            pieces.Add(ra);
            playerPieces.Add(ra);

            AttackPiece a = new AttackPiece()
            {
                Column = 10,
                Row = 5,
                Units = 5,
                Icon = iconSword
            };

            pieces.Add(a);
            playerPieces.Add(a);
        }

        void FillPiecesLevelOne()
        {
            EnemyPiece e = new EnemyPiece()
            {
                Column = 0,
                Row = 6,
                Units = 1,
                Icon = iconWall
            };

            pieces.Add(e);
            enemyPieces.Add(e);

            EnemyPiece e2 = new EnemyPiece()
            {
                Column = 0,
                Row = 5,
                Units = 1,
                Icon = iconWall
            };

            pieces.Add(e2);
            enemyPieces.Add(e2);

            EnemyPiece e3 = new EnemyPiece()
            {
                Column = 0,
                Row = 4,
                Units = 1,
                Icon = iconWall
            };

            pieces.Add(e3);
            enemyPieces.Add(e3);

            DefendPiece d = new DefendPiece()
            {
                Column = 9,
                Row = 5,
                Units = 5,
                Icon = iconShield
            };

            pieces.Add(d);
            playerPieces.Add(d);

            DefendPiece d2 = new DefendPiece()
            {
                Column = 9,
                Row = 4,
                Units = 5,
                Icon = iconShield
            };

            pieces.Add(d2);
            playerPieces.Add(d2);
        }

        void FillPiecesLevelTwo()
        {
            int fillRows = (int)Rows;
            int fillColumns = 2;
            int enemyUnitsPerTile = 2;
            int totalEnemyUnits = enemyUnitsPerTile * (fillColumns * fillRows);

            for (uint row = 0; row < fillRows; row++) {
                for (uint column = 0; column < fillColumns; column++) {
                    EnemyPiece wallPiece = new EnemyPiece()
                    {
                        Column = column,
                        Row = row,
                        Units = enemyUnitsPerTile,
                        Icon = iconWall
                    };

                    pieces.Add(wallPiece);
                    enemyPieces.Add(wallPiece);
                }
            }

            BarracksPiece b = new BarracksPiece()
            {
                Column = 11,
                Row = 6,
                Units = 1,
                ProductionUnits = 1,
                Icon = iconBarracks
            };

            pieces.Add(b);
            playerPieces.Add(b);
            RangedPiece ra = new RangedPiece()
            {
                Column = 10,
                Row = 7,
                Units = 1,
                Icon = iconArrow
            };

            pieces.Add(ra);
            playerPieces.Add(ra);

            AttackPiece a = new AttackPiece()
            {
                Column = 10,
                Row = 5,
                Units = 2,
                Icon = iconSword
            };

            pieces.Add(a);
            playerPieces.Add(a);

            DefendPiece d = new DefendPiece()
            {
                Column = 10,
                Row = 4,
                Units = 3,
                Icon = iconShield
            };

            pieces.Add(d);
            playerPieces.Add(d);
        }

        void FillPiecesLevelThree()
        {
            int fillRows = (int)Rows;
            int fillColumns = 2;
            int enemyUnitsPerTile = 3;
            int totalEnemyUnits = enemyUnitsPerTile * (fillColumns * fillRows);

            for (uint row = 0; row < fillRows; row++) {
                for (uint column = 0; column < fillColumns; column++) {
                    EnemyPiece wallPiece = new EnemyPiece()
                    {
                        Column = column,
                        Row = row,
                        Units = enemyUnitsPerTile,
                        Icon = iconWall
                    };

                    pieces.Add(wallPiece);
                    enemyPieces.Add(wallPiece);
                }
            }

            BarracksPiece b = new BarracksPiece()
            {
                Column = 11,
                Row = 6,
                Units = 1,
                ProductionUnits = 2,
                Icon = iconBarracks
            };

            pieces.Add(b);
            playerPieces.Add(b);

            RangedPiece ra = new RangedPiece()
            {
                Column = 10,
                Row = 7,
                Units = 1,
                Icon = iconArrow
            };

            pieces.Add(ra);
            playerPieces.Add(ra);

            AttackPiece a = new AttackPiece()
            {
                Column = 10,
                Row = 5,
                Units = 2,
                Icon = iconSword
            };

            pieces.Add(a);
            playerPieces.Add(a);

            DefendPiece d = new DefendPiece()
            {
                Column = 10,
                Row = 4,
                Units = 3,
                Icon = iconShield
            };

            pieces.Add(d);
            playerPieces.Add(d);
        }

        void FillPiecesLevelFour()
        {
            int fillRows = (int)Rows;
            int fillColumns = 2;
            int enemyUnitsPerTile = 4;
            int totalEnemyUnits = enemyUnitsPerTile * (fillColumns * fillRows);

            for (uint row = 0; row < fillRows; row++) {
                for (uint column = 0; column < fillColumns; column++) {
                    EnemyPiece wallPiece = new EnemyPiece()
                    {
                        Column = column,
                        Row = row,
                        Units = enemyUnitsPerTile,
                        Icon = iconWall
                    };

                    pieces.Add(wallPiece);
                    enemyPieces.Add(wallPiece);
                }
            }

            BarracksPiece b = new BarracksPiece()
            {
                Column = 11,
                Row = 6,
                Units = 1,
                ProductionUnits = 2,
                Icon = iconBarracks
            };

            pieces.Add(b);
            playerPieces.Add(b);

            RangedPiece ra = new RangedPiece()
            {
                Column = 10,
                Row = 7,
                Units = 1,
                Icon = iconArrow
            };

            pieces.Add(ra);
            playerPieces.Add(ra);

            AttackPiece a = new AttackPiece()
            {
                Column = 10,
                Row = 5,
                Units = 2,
                Icon = iconSword
            };

            pieces.Add(a);
            playerPieces.Add(a);

            AttackPiece a2 = new AttackPiece()
            {
                Column = 10,
                Row = 6,
                Units = 2,
                Icon = iconSword
            };

            pieces.Add(a2);
            playerPieces.Add(a2);

            DefendPiece d = new DefendPiece()
            {
                Column = 10,
                Row = 4,
                Units = 5,
                Icon = iconShield
            };

            pieces.Add(d);
            playerPieces.Add(d);
        }

        void FillPiecesLevelFive()
        {
            int fillRows = (int)Rows;
            int fillColumns = 3;
            int enemyUnitsPerTile = 6;
            int totalEnemyUnits = enemyUnitsPerTile * (fillColumns * fillRows);

            for (uint row = 0; row < fillRows; row++) {
                for (uint column = 0; column < fillColumns; column++) {
                    EnemyPiece wallPiece = new EnemyPiece()
                    {
                        Column = column,
                        Row = row,
                        Units = enemyUnitsPerTile,
                        Icon = iconWall
                    };

                    pieces.Add(wallPiece);
                    enemyPieces.Add(wallPiece);
                }
            }

            BarracksPiece b = new BarracksPiece()
            {
                Column = 11,
                Row = 6,
                Units = 1,
                ProductionUnits = 3,
                Icon = iconBarracks
            };

            pieces.Add(b);
            playerPieces.Add(b);

            RangedPiece ra = new RangedPiece()
            {
                Column = 10,
                Row = 7,
                Units = 3,
                Icon = iconArrow
            };

            pieces.Add(ra);
            playerPieces.Add(ra);

            RangedPiece ra2 = new RangedPiece()
            {
                Column = 10,
                Row = 2,
                Units = 3,
                Icon = iconArrow
            };

            pieces.Add(ra2);
            playerPieces.Add(ra2);

            AttackPiece a = new AttackPiece()
            {
                Column = 10,
                Row = 5,
                Units = 3,
                Icon = iconSword
            };

            pieces.Add(a);
            playerPieces.Add(a);

            AttackPiece a2 = new AttackPiece()
            {
                Column = 10,
                Row = 6,
                Units = 3,
                Icon = iconSword
            };

            pieces.Add(a2);
            playerPieces.Add(a2);

            DefendPiece d = new DefendPiece()
            {
                Column = 10,
                Row = 4,
                Units = 5,
                Icon = iconShield
            };

            pieces.Add(d);
            playerPieces.Add(d);
        }
    }
}
