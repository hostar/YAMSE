using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SWM = System.Windows.Media;

namespace YAMSE.Helpers
{
    public class MafiaHighlight : IHighlightingDefinition
    {
        public string Name => "mafia";

        public HighlightingRuleSet MainRuleSet { get; private set; }

        private Dictionary<string, HighlightingColor> colorDict = new Dictionary<string, HighlightingColor>();

        public IEnumerable<HighlightingColor> NamedHighlightingColors
        {
            get
            {
                return colorDict.Values;
            }
        }

        private HighlightingColor red = new HighlightingColor() { Foreground = new SimpleHighlightingBrush((SWM.Color)SWM.ColorConverter.ConvertFromString("Red")) };
        private HighlightingColor blue = new HighlightingColor() { Foreground = new SimpleHighlightingBrush((SWM.Color)SWM.ColorConverter.ConvertFromString("Blue")) };
        private HighlightingColor green = new HighlightingColor() { Foreground = new SimpleHighlightingBrush((SWM.Color)SWM.ColorConverter.ConvertFromString("Green")) };
        private HighlightingColor purple = new HighlightingColor() { Foreground = new SimpleHighlightingBrush((SWM.Color)SWM.ColorConverter.ConvertFromString("#ba2cf2")) };

        private Dictionary<string, string> propDict = new Dictionary<string, string>();

        private HighlightingRuleSet highlightingRuleSet;

        public IDictionary<string, string> Properties
        {
            get
            {
                return propDict;
            }
        }

        public HighlightingColor GetNamedColor(string name)
        {
            return red;
        }

        public HighlightingRuleSet GetNamedRuleSet(string name)
        {
            return highlightingRuleSet;
        }

        public MafiaHighlight()
        {
            HighlightingRule highlightingRuleDefs = new HighlightingRule() { Color = red, Regex = new Regex("dim.*", RegexOptions.IgnoreCase) };
            HighlightingRule highlightingRuleCommands = new HighlightingRule() { Color = blue, Regex = new Regex(string.Join('|', MafiaKeywords.Commands) + "|" + string.Join('|', MafiaKeywords.Keywords), RegexOptions.IgnoreCase) };
            HighlightingRule highlightingRuleComments = new HighlightingRule() { Color = green, Regex = new Regex("//.*") };
            HighlightingRule highlightingRuleQuotes = new HighlightingRule() { Color = purple, Regex = new Regex("\".*\"") };

            highlightingRuleSet = new HighlightingRuleSet() { Name = "mafia" };

            highlightingRuleSet.Rules.Add(highlightingRuleDefs);
            highlightingRuleSet.Rules.Add(highlightingRuleCommands);
            highlightingRuleSet.Rules.Add(highlightingRuleComments);
            highlightingRuleSet.Rules.Add(highlightingRuleQuotes);

            MainRuleSet = highlightingRuleSet;
        }
    }
}
