using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MusicMinigame
{
    internal enum SymbolCode
    {
        Empty = -1,
        Q = 0,
        W = 1,
        E = 2,
        R = 3,
        T = 4
    }

    /// <summary>
    /// Represents a symbol to be displayed in a symbol space
    /// </summary>
    internal class Symbol
    {
        internal Color Colour;
        internal SymbolCode Code;
        internal Symbol(SymbolCode code)
        {
            Code = code;
            SetColor();
        }

        private void SetColor()
        {
            Colour = Code switch
            {
                SymbolCode.Q => Color.blue,
                SymbolCode.W => Color.red,
                SymbolCode.E => Color.green,
                SymbolCode.R => Color.yellow,
                SymbolCode.T => Color.magenta,
                SymbolCode.Empty => Color.clear,
                _ => Colour
            };
        }

        internal static Symbol EmptySymbol()
        {
            return new Symbol(SymbolCode.Empty);
        }
    }

    /// <summary>
    /// Contains a symbol and represents a space within the visual grid.
    /// </summary>
    internal class SymbolSpace
    {
        internal Vector2Int Position;
        internal Symbol ContainedSymbol;
        private SymbolCode[] _validSymbols = new SymbolCode[2];
        internal GameObject SymbolSpaceGameObject;
        
        internal SymbolSpace(Vector2Int position)
        {
            Init(position);
        }

        private void Init(Vector2Int position)
        {
            Position = position;
            ContainedSymbol = Symbol.EmptySymbol();
            SetValidSymbols(Position);
        }

        internal bool CanContainSymbol(Symbol symbol)
        {
            return ContainedSymbol.Code == SymbolCode.Empty && _validSymbols.Contains(symbol.Code);
        }

        internal void SetSymbol(Symbol symbol)
        {
            ContainedSymbol = symbol;
            SymbolSpaceGameObject.GetComponent<Image>().color = symbol.Colour;
        }

        private void SetValidSymbols(Vector2Int position)
        {
            var validSymbols = new SymbolCode[2] { default, default };
            validSymbols[1] = position.y switch
            {
                0 => SymbolCode.Q,
                1 => SymbolCode.W,
                2 => SymbolCode.E,
                3 => SymbolCode.R,
                4 => SymbolCode.T,
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
        internal readonly Vector2Int Dimensions;
        internal GameObject GridGameObject;
        internal int LastSymbolIndex;
        internal VisualGrid(Vector2Int dimensions)
        {
            Dimensions = dimensions;
            Symbols = new SymbolSpace[(Dimensions.x * Dimensions.y)];
            LastSymbolIndex = -1;
            var posX = 0;
            var posY = 0;
            
            for (var symIndex = 0; symIndex < Symbols.Length; symIndex++)
            {
                if (posX < Dimensions.x)
                    posX++;
                else
                {
                    posX = 1;
                    posY++;
                }
                var pos = new Vector2Int(posX, posY);
                Symbols[symIndex] = new SymbolSpace(pos);
                Debug.Log(Symbols[symIndex].Position.ToString());
            }
        }

        internal void Reset()
        {
            LastSymbolIndex = -1;
            
            foreach (var symbolSpace in Symbols)
            {
                var resetSymbol = Symbol.EmptySymbol();
                symbolSpace.SetSymbol(resetSymbol);
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
        [SerializeField] 
        private GameObject uiContainerGameObject;
        [SerializeField] 
        private GameObject symbolPrefab;
        
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
            DebugAddSymbol();
            DebugResetGrid();
            DisplayGrids();
        }

        private static void DisplayGrids()
        {

        }

        private void BuildGrids()
        {
            var playersGridGameObject = uiContainerGameObject.transform.GetChild(0).gameObject;
            var computersGridGameObject = uiContainerGameObject.transform.GetChild(1).gameObject;
            
            _computersGrid = new VisualGrid(gridDimensions);
            _playersGrid = new VisualGrid(gridDimensions);
            
            BuildGrid(_playersGrid, playersGridGameObject);
            BuildGrid(_computersGrid, computersGridGameObject);
        }

        private void BuildGrid(VisualGrid gridToBuild, GameObject gridGameObject)
        {
            gridGameObject.GetComponent<GridLayoutGroup>().constraintCount = gridToBuild.Dimensions.x;
            gridToBuild.GridGameObject = gridGameObject;
            
            foreach (var symbol in gridToBuild.Symbols)
            {
                var symbolGameObject = Instantiate(symbolPrefab, gridGameObject.transform, false); 
                symbolGameObject.GetComponent<Image>().color = Symbol.EmptySymbol().Colour;
                symbol.SymbolSpaceGameObject = symbolGameObject;
            }
        }

        /// <summary>
        /// Adds a symbol to either the computer's or player's
        /// visual grid received from the game logic handler
        /// </summary>
        private void AddSymbol(int symbolInput, bool isPlayersGrid)
        {
            var gridToModify = isPlayersGrid ? _playersGrid : _computersGrid;
            var symbolCode = (SymbolCode)symbolInput;
            var symbol = new Symbol(symbolCode);

            foreach (var symbolSpace in gridToModify.Symbols)
            {
                if (gridToModify.LastSymbolIndex >= symbolSpace.Position.x || 
                    !symbolSpace.CanContainSymbol(symbol)) continue;
                symbolSpace.SetSymbol(symbol);
                gridToModify.LastSymbolIndex = symbolSpace.Position.x;
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
        
        // ------------------------------Debug Methods (REMOVE BEFORE SUBMITTING GAME) ---------------------------------
        private void DebugAddSymbol()
        { 
            var gridToModify = Input.GetKey(KeyCode.LeftShift) ? _computersGrid : _playersGrid;
            if (Input.GetKeyDown(KeyCode.F))
            {
                AddSymbol(0, gridToModify != _playersGrid);
            }
            else if (Input.GetKeyDown(KeyCode.G))
            {
                AddSymbol(1, gridToModify != _playersGrid);
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                AddSymbol(2, gridToModify != _playersGrid);
            }
            else if (Input.GetKeyDown(KeyCode.J))
            {
                AddSymbol(3, gridToModify != _playersGrid);
            }
            else if (Input.GetKeyDown(KeyCode.K))
            {
                AddSymbol(4, gridToModify != _playersGrid);
            }
        }

        private void DebugResetGrid()
        {
            var gridToModify = Input.GetKey(KeyCode.LeftShift) ? _computersGrid : _playersGrid;
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetGrid(gridToModify == _playersGrid);
            }
        }
    }
}
