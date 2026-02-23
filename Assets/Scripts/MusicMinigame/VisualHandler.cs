using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MusicMinigame
{
    internal enum Symbol
    {
        Empty = -1,
        Q = 0,
        W = 1,
        E = 2,
        R = 3,
        T = 4
    }

    /// <summary>
    /// Contains a symbol and represents a space within the visual grid.
    /// </summary>
    internal class SymbolSpace
    {
        internal Vector2Int Position;
        private Symbol _symbol;
        private Symbol[] _validSymbols = new Symbol[2];
        
        internal SymbolSpace(Vector2Int position)
        {
            Init(position);
        }

        private void Init(Vector2Int position)
        {
            Position = position;
            _symbol = default;
            SetValidSymbols(Position);
        }

        internal bool CanContainSymbol(Symbol symbol)
        {
            return _symbol == Symbol.Empty && _validSymbols.Contains(symbol);
        }

        internal void SetSymbol(Symbol symbol)
        {
            _symbol = symbol;
        }

        private void SetValidSymbols(Vector2Int position)
        {
            var validSymbols = new Symbol[2] { default, default };
            validSymbols[1] = position.y switch
            {
                0 => Symbol.Q,
                1 => Symbol.W,
                2 => Symbol.E,
                3 => Symbol.R,
                4 => Symbol.T,
                _ => throw new ArgumentOutOfRangeException()
            };
            _validSymbols = validSymbols;
        }
    }

    /// <summary>
    /// Handles the display logic for a grid to visualise
    /// which notes are played and in what order.
    /// </summary>
    internal class VisualGrid
    {
        internal readonly SymbolSpace[] Symbols;
        private readonly Vector2Int _dimensions;
        
        internal VisualGrid(Vector2Int dimensions)
        {
            _dimensions = dimensions;
            Symbols = new SymbolSpace[(_dimensions.x * _dimensions.y)];

            var posX = 0;
            var posY = 0;
            
            for (var symIndex = 0; symIndex < Symbols.Length; symIndex++)
            {
                if (posX < _dimensions.x)
                    posX++;
                else
                {
                    posX = 0;
                    posY++;
                }
                var pos = new Vector2Int(posX, posY);
                Symbols[symIndex] = new SymbolSpace(pos);
            }
        }

        internal void Reset()
        {
            foreach (var symbolSpace in Symbols)
            {
                symbolSpace.SetSymbol(default);
            }
        }
    }
    
    /// <summary>
    /// Handles the high-level logic of the visual system.
    /// </summary>
    public class VisualHandler : MonoBehaviour
    {
        [SerializeField]
        private Vector2Int gridDimensions;
        
        private VisualGrid _computersGrid;
        private VisualGrid _playersGrid;
        
        // Start is called before the first frame update
        private void Start()
        {
            BuildGrids();
        }

        // Update is called once per frame
        private void Update()
        {
            DisplayGrids();
        }

        private static void DisplayGrids()
        {
            
        }

        private void BuildGrids()
        {
            _computersGrid = new VisualGrid(gridDimensions);
            _playersGrid = new VisualGrid(gridDimensions);
        }
        
        /// <summary>
        /// Adds a symbol to either the computer's or player's
        /// visual grid received from the game logic handler
        /// </summary>
        private void AddSymbol(string symbolInput, bool isPlayersGrid)
        {
            var gridToModify = isPlayersGrid ? _playersGrid : _computersGrid;
            var symbol = Enum.Parse<Symbol>(symbolInput);

            foreach (var symbolSpace in gridToModify.Symbols)
            {
                if (!symbolSpace.CanContainSymbol(symbol)) continue;
                symbolSpace.SetSymbol(symbol);
                break;
            }
        }

        /// <summary>
        /// Resets either the player's or computers grid
        /// to contain empty symbols in its spaces.
        /// </summary>
        private void ResetGrid(bool isPlayersGrid)
        {
            var gridToModify = isPlayersGrid ? _computersGrid : _playersGrid;
            gridToModify.Reset();
        }
    }
}
