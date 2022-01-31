using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms.Integration;
using ICSharpCode.AvalonEdit;

namespace YAMSE.Helpers
{
    public class TextEditorWrapper
    {
        ElementHost elementHost;
        TextEditor editor;

        public TextEditor Editor { get => editor; set => editor = value; }
        public ElementHost ElementHost { get => elementHost; set => elementHost = value; }

        public string GetText()
        {
            return Editor.Text;
        }

        public string SetText(string text)
        {
            return Editor.Text = text;
        }

        public void SetElementGenerator(ImageElementGenerator imageElementGenerator)
        {
            Editor.TextArea.TextView.ElementGenerators.Add(imageElementGenerator);
        }
    }
}
