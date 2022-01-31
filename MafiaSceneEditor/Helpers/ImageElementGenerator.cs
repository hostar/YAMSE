using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using SWM = System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YAMSE.Helpers
{
    public class ImageElementGenerator : VisualLineElementGenerator
    {
        // To use this class:
        // textEditor.TextArea.TextView.ElementGenerators.Add(new ImageElementGenerator(basePath));

        readonly static Regex hintRegex = new Regex(@"(detector_waitforuse|console_addtext|endofmission|human_talk[ ]*[0-9]+|subtitle_add|event use_ab|mission_objectives)[ ]*,?[ ]*([0-9]+)", RegexOptions.IgnoreCase);
        //readonly static Regex intRegex = new Regex(@"([0-9]+)", RegexOptions.IgnoreCase);

        readonly DefLoader defLoader;

        public ImageElementGenerator(string defFilePath)
        {
            //this.basePath = basePath;
            defLoader = new DefLoader(defFilePath);
        }

        Match FindMatch(int startOffset)
        {
            // fetch the end offset of the VisualLine being generated
            int endOffset = CurrentContext.VisualLine.LastDocumentLine.EndOffset;
            TextDocument document = CurrentContext.Document;
            string relevantText = document.GetText(startOffset, endOffset - startOffset);
            return hintRegex.Match(relevantText);
        }

        /// Gets the first offset >= startOffset where the generator wants to construct
        /// an element.
        /// Return -1 to signal no interest.
        public override int GetFirstInterestedOffset(int startOffset)
        {
            Match match = FindMatch(startOffset);
            var offset = startOffset + match.Length; // returned value cannot be longer than length of the line
            //return match.Success ? offset : -1;
            if (match.Success)
            {
                return offset;
            }
            else
            {
                return -1;
            }
        }

        /// Constructs an element at the specified offset.
        /// May return null if no element should be constructed.
        public override VisualLineElement ConstructElement(int offset)
        {
            var line = CurrentContext.Document.GetLineByOffset(offset);
            var text = CurrentContext.Document.GetText(line.Offset, line.EndOffset - line.Offset);
            if (text.StartsWith("//"))
            {
                return null;
            }

            string stringID = hintRegex.Match(text).Groups[2].Value;

            string toShow = defLoader.GetRecordById(Convert.ToInt32(stringID));

            SWM.Color color;
            if (toShow == string.Empty)
            {
                color = (SWM.Color)SWM.ColorConverter.ConvertFromString("Red");
                toShow = "  Record does not exist!";
            }
            else
            {
                toShow = "   " + toShow;
                color = SWM.Color.FromArgb(255, 172, 181, 174);
                //color = (SWM.Color)SWM.ColorConverter.ConvertFromString("Gray");
            }

            // check whether there's a match exactly at offset
            TextBlock textBlock = new TextBlock()
            {
                Text = toShow, 
                Background = new SWM.SolidColorBrush(color),
            };

            var inlineObject = new InlineObjectElement(0, textBlock);
            return inlineObject;
        }
    }
}
