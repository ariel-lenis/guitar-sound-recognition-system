using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TsExtraControls
{
    public class Extra
    {
        public enum AdaptedSymbolType
        {
            // Summary:
            //     Square-shaped ZedGraph.Symbol
            Square = 0,
            //
            // Summary:
            //     Rhombus-shaped ZedGraph.Symbol
            Diamond = 1,
            //
            // Summary:
            //     Equilateral triangle ZedGraph.Symbol
            Triangle = 2,
            //
            // Summary:
            //     Uniform circle ZedGraph.Symbol
            Circle = 3,
            //
            // Summary:
            //     "X" shaped ZedGraph.Symbol. This symbol cannot be filled since it has no
            //     outline.
            XCross = 4,
            //
            // Summary:
            //     "+" shaped ZedGraph.Symbol. This symbol cannot be filled since it has no
            //     outline.
            Plus = 5,
            //
            // Summary:
            //     Asterisk-shaped ZedGraph.Symbol. This symbol cannot be filled since it has
            //     no outline.
            Star = 6,
            //
            // Summary:
            //     Unilateral triangle ZedGraph.Symbol, pointing down.
            TriangleDown = 7,
            //
            // Summary:
            //     Horizontal dash ZedGraph.Symbol. This symbol cannot be filled since it has
            //     no outline.
            HDash = 8,
            //
            // Summary:
            //     Vertical dash ZedGraph.Symbol. This symbol cannot be filled since it has
            //     no outline.
            VDash = 9,
            //
            // Summary:
            //     A symbol defined by the ZedGraph.Symbol.UserSymbol propery.  If no symbol
            //     is defined, the ZedGraph.Symbol.Default.Type. symbol will be used.
            UserDefined = 10,
            //
            // Summary:
            //     A Default symbol type (the symbol type will be obtained from ZedGraph.Symbol.Default.Type.
            Default = 11,
            //
            // Summary:
            //     No symbol is shown (this is equivalent to using ZedGraph.Symbol.IsVisible
            //     = false.
            None = 12,
        }
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        [ComVisible(true)]
        public struct AxisConfig
        {
            public double MinorMarks { get; set; }
            public double Mayormarks { get; set; }
            public string AxisTitle { get; set; }
            public override string ToString()
            {
                return AxisTitle + "[" + MinorMarks + "," + Mayormarks + "]";
            }
        }
    }
}
