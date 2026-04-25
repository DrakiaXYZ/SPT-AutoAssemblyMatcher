using ScintillaNET;
using System.Threading.Tasks;
using BorderStyle = System.Windows.Forms.BorderStyle;
using TabDrawMode = System.Windows.Forms.TabDrawMode;

namespace AutoAssemblyMatcher
{
    internal class AISlopStyler
    {
        public void ApplyScintillaStyle(Scintilla scintilla)
        {
            scintilla.LexerName = "cpp";

            scintilla.StyleResetDefault();
            scintilla.Styles[Style.Default].BackColor = Color.FromArgb(30, 30, 30);
            scintilla.Styles[Style.Default].ForeColor = Color.FromArgb(220, 220, 220);
            scintilla.Styles[Style.Default].Font = "Consolas";
            scintilla.Styles[Style.Default].Size = 11;
            scintilla.StyleClearAll();

            scintilla.Styles[Style.Cpp.Comment].ForeColor = Color.FromArgb(87, 166, 74);
            scintilla.Styles[Style.Cpp.CommentLine].ForeColor = Color.FromArgb(87, 166, 74);
            scintilla.Styles[Style.Cpp.CommentDoc].ForeColor = Color.FromArgb(87, 166, 74);
            scintilla.Styles[Style.Cpp.CommentLineDoc].ForeColor = Color.FromArgb(87, 166, 74);
            scintilla.Styles[Style.Cpp.Word].ForeColor = Color.FromArgb(86, 156, 214);
            scintilla.Styles[Style.Cpp.Word2].ForeColor = Color.FromArgb(78, 201, 176);
            scintilla.Styles[Style.Cpp.String].ForeColor = Color.FromArgb(214, 157, 133);
            scintilla.Styles[Style.Cpp.StringEol].ForeColor = Color.FromArgb(214, 157, 133);
            scintilla.Styles[Style.Cpp.Character].ForeColor = Color.FromArgb(214, 157, 133);
            scintilla.Styles[Style.Cpp.Verbatim].ForeColor = Color.FromArgb(214, 157, 133);
            scintilla.Styles[Style.Cpp.Number].ForeColor = Color.FromArgb(181, 206, 168);
            scintilla.Styles[Style.Cpp.Operator].ForeColor = Color.FromArgb(220, 220, 220);
            scintilla.Styles[Style.Cpp.Preprocessor].ForeColor = Color.FromArgb(155, 155, 155);
            scintilla.Styles[Style.Cpp.Identifier].ForeColor = Color.FromArgb(220, 220, 220);

            scintilla.SetKeywords(0,
                "abstract as base bool break byte case catch char checked " +
                "class const continue decimal default delegate do double else " +
                "enum event explicit extern false finally fixed float for " +
                "foreach goto if implicit in int interface internal is lock " +
                "long namespace new null object operator out override params " +
                "private protected public readonly ref return sbyte sealed " +
                "short sizeof stackalloc static string struct switch this " +
                "throw true try typeof uint ulong unchecked unsafe ushort " +
                "using virtual void volatile while async await var dynamic");

            scintilla.SetKeywords(1,
                "bool byte char decimal double float int long object sbyte " +
                "short string uint ulong ushort var void dynamic get set " +
                "value add remove yield partial where select from let join " +
                "on equals into orderby ascending descending group by");

            scintilla.Margins[0].Width = 40;
            scintilla.Styles[Style.LineNumber].BackColor = Color.FromArgb(30, 30, 30);
            scintilla.Styles[Style.LineNumber].ForeColor = Color.FromArgb(43, 145, 175);

            scintilla.CaretForeColor = Color.White;
            scintilla.CaretWidth = 2;

            scintilla.SelectionBackColor = Color.FromArgb(38, 79, 120);
            scintilla.CaretLineBackColor = Color.FromArgb(40, 40, 40);

            scintilla.WhitespaceSize = 1;
            scintilla.ViewWhitespace = WhitespaceMode.Invisible;

            scintilla.IndentationGuides = IndentView.LookBoth;
            scintilla.Styles[Style.IndentGuide].ForeColor = Color.FromArgb(60, 60, 60);
            scintilla.Styles[Style.IndentGuide].BackColor = Color.FromArgb(30, 30, 30);

            scintilla.Margins[2].Width = 20;
            scintilla.Margins[2].BackColor = Color.FromArgb(30, 30, 30);
        }
    }
}
