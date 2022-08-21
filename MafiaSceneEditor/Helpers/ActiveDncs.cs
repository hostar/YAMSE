using ComponentFactory.Krypton.Navigator;
using ComponentFactory.Krypton.Toolkit;
using ComponentFactory.Krypton.Workspace;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YAMSE.DataLayer;

namespace YAMSE.Helpers
{
    public static class ActiveDncs
    {
        private static List<string> activeDncs = new List<string>();
        private static ImageElementGenerator imageElementGenerator;
        private static KryptonWorkspace kryptonWorkspace;
        private static Form _this;

        private static Color defaultColor = Color.FromArgb(221, 234, 247);

        public static void Add(Dnc dnc)
        {
            if (!Exists(dnc))
            {
                activeDncs.Add(DncMethods.CreatePageID(dnc));
                CreatePage(dnc, DncToPanelKind(dnc));
            }
        }

        public static bool Exists(Dnc dnc)
        {
            string pageId = DncMethods.CreatePageID(dnc);
            return activeDncs.Contains(pageId);
        }

        public static void RemoveAll()
        {
            activeDncs.Clear();
            CloseAllPages();
        }

        public static void Remove(Dnc dnc)
        {
            activeDncs.Remove(DncMethods.CreatePageID(dnc));
            ClosePage(dnc);
        }

        private static PanelKind DncToPanelKind(Dnc dnc)
        {
            switch (dnc.dncKind)
            {
                case NodeType.Object:
                    switch (dnc.dncType)
                    {
                        case DncType.Standard:
                            return PanelKind.Standard;
                        case DncType.Model:
                            return PanelKind.Model;
                        default:
                            return PanelKind.Hex;
                    }
                case NodeType.Definition:
                    switch (dnc.dncType)
                    {
                        case DncType.Script:
                            return PanelKind.Script;
                        case DncType.Enemy:
                            return PanelKind.Enemy;
                        case DncType.PhysicalObject:
                        case DncType.Door:
                        case DncType.Tram:
                        case DncType.GasStation:
                        case DncType.PedestrianSetup:
                        case DncType.Plane:
                        case DncType.Player:
                        case DncType.TrafficSetup:
                        case DncType.Unknown:
                        case DncType.MovableBridge:
                        case DncType.Car:
                        default:
                            return PanelKind.Hex;
                    }
                case NodeType.InitScript:
                    return PanelKind.Script;
                case NodeType.Header:
                    return PanelKind.Header;
                case NodeType.Unknown:
                    return PanelKind.Hex;
                default:
                    return PanelKind.Hex;
            }
        }

        private static KryptonPage CreatePage(Dnc dnc, PanelKind panelKind)
        {
            string pageName = $"{dnc.Name}";

            var pageId = new KryptonPageId { Dnc = dnc, PanelKind = panelKind };

            List<KryptonPageContainer> kryptonPageContainer = new List<KryptonPageContainer>();

            TableLayoutPanel tableLayoutPanel;
            TextEditorWrapper textEditorWrapper;
            KryptonPage page;

            switch (panelKind)
            {
                case PanelKind.Script:
                    textEditorWrapper = CreateAvalonEdit(Scene2Parser.GetScriptFromDnc(dnc), pageId, _this);

                    if (imageElementGenerator != null)
                    {
                        textEditorWrapper.SetElementGenerator(imageElementGenerator);
                    }

                    pageId.TextEditor = textEditorWrapper;

                    kryptonPageContainer.Add(
                        new KryptonPageContainer
                        {
                            Column = 0,
                            ColumnSpan = 4,
                            Component = textEditorWrapper.ElementHost,
                            ComponentType = ComponentType.TextEditor,
                            RowSpan = 2
                        });

                    pageId.KryptonPageContainer = kryptonPageContainer;

                    page = CreatePageOnly(pageName, pageId);
                    pageId.KryptonPage = page;
                    return CreatePageInternal(page, pageId, kryptonPageContainer);

                case PanelKind.Enemy:
                    textEditorWrapper = CreateAvalonEdit(Scene2Parser.GetScriptFromDnc(dnc), pageId, _this);

                    if (imageElementGenerator != null)
                    {
                        textEditorWrapper.SetElementGenerator(imageElementGenerator);
                    }

                    pageId.TextEditor = textEditorWrapper;

                    EnemyProps enemyProps = dnc.DncProps as EnemyProps;

                    PropertyGrid propertyGrid = new PropertyGrid() { SelectedObject = enemyProps, Dock = DockStyle.Fill };

                    kryptonPageContainer.Add(
                        new KryptonPageContainer
                        {
                            Column = 2,
                            ColumnSpan = 1,
                            Component = propertyGrid,
                            ComponentType = ComponentType.PropertyGrid,
                            RowSpan = 1
                        });
                    kryptonPageContainer.Add(
                        new KryptonPageContainer
                        {
                            Column = 0,
                            ColumnSpan = 2,
                            Component = textEditorWrapper.ElementHost,
                            ComponentType = ComponentType.TextEditor,
                            RowSpan = 1
                        });

                    pageId.KryptonPageContainer = kryptonPageContainer;

                    page = CreatePageOnly(pageName, pageId);
                    pageId.KryptonPage = page;
                    return CreatePageInternal(page, pageId, kryptonPageContainer);

                case PanelKind.Hex:

                    var hexEditor = new WpfHexaEditor.HexEditor
                    {
                        ForegroundSecondColor = System.Windows.Media.Brushes.Blue,
                        TypeOfCharacterTable = WpfHexaEditor.Core.CharacterTableType.Ascii
                    };

                    var elementHostHexEditor = new System.Windows.Forms.Integration.ElementHost
                    {
                        Location = new Point(250, 50),
                        Size = new Size(1000, 500),
                        Dock = DockStyle.Fill
                    };
                    elementHostHexEditor.Name = nameof(elementHostHexEditor);
                    elementHostHexEditor.Child = hexEditor;

                    var tmpStream = new MemoryStream();
                    new MemoryStream(dnc.RawData).CopyTo(tmpStream); // needed in order to allow expanding
                    hexEditor.Stream = tmpStream;

                    pageId.HexEditor = hexEditor;

                    kryptonPageContainer.Add(
                        new KryptonPageContainer
                        {
                            Column = 0,
                            ColumnSpan = 3,
                            Component = elementHostHexEditor,
                            ComponentType = ComponentType.HexEditor,
                            RowSpan = 1
                        });

                    page = CreatePageOnly(pageName, pageId);
                    pageId.KryptonPage = page;
                    return CreatePageInternal(page, pageId, kryptonPageContainer);

                case PanelKind.Standard:
                    CreateDefaultTextBoxes(kryptonPageContainer, pageId, dnc);

                    tableLayoutPanel = new TableLayoutPanel
                    {
                        BackColor = Color.FromArgb(187, 206, 230), //defaultColor,
                        ColumnCount = 6,
                        RowCount = 3
                    };
                    tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
                    tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
                    tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));

                    page = CreatePageOnly(pageName, pageId);
                    pageId.KryptonPage = page;
                    return CreatePageInternal(page, pageId, kryptonPageContainer, tableLayoutPanel);

                case PanelKind.Model:
                    CreateDefaultTextBoxes(kryptonPageContainer, pageId, dnc);

                    ModelProps modelProps = dnc.DncProps as ModelProps;

                    KryptonPanel kryptonPanel = new KryptonPanel { Top = 500, Left = 0, Size = new Size(500, 200) };

                    List<KryptonPageContainer> kryptonPageContainer2 = new List<KryptonPageContainer>();
                    int row = 0;
                    int col = 0;
                    CreateLabel(kryptonPageContainer2, col, row, 1, "Sector");

                    col++;
                    CreateCheckBox(kryptonPageContainer2, col, row, string.Empty, modelProps.HaveSector,
                        (o, control) => { modelProps.HaveSector = (bool)o; }, (prop, control) => { (control as CheckBox).Checked = (prop as ModelProps).HaveSector; }, width: 16);

                    col++;
                    CreateTextBox(kryptonPageContainer2, col, row, modelProps.Sector,
                        (o, control) => { modelProps.Sector = o.ToString(); }, (prop, control) => { control.Text = (prop as ModelProps).Sector.ToString(); }, 3, 278);

                    row++;
                    col = 0;
                    CreateLabel(kryptonPageContainer2, col, row, 1, "Model");

                    col++;
                    CreateTextBox(kryptonPageContainer2, col, row, modelProps.Model,
                        (o, control) => { modelProps.Model = o.ToString(); }, (prop, control) => { control.Text = (prop as ModelProps).Model.ToString(); }, 3, 300);

                    TableLayoutPanel tableLayoutOptionalPanel = new TableLayoutPanel
                    {
                        BackColor = Color.FromArgb(187, 206, 230),
                        ColumnCount = 8,
                        RowCount = 2,
                        Dock = DockStyle.Fill
                    };

                    PutOnTableLayout(kryptonPageContainer2, tableLayoutOptionalPanel);
                    kryptonPanel.Controls.Add(tableLayoutOptionalPanel);

                    tableLayoutPanel = new TableLayoutPanel
                    {
                        BackColor = Color.FromArgb(187, 206, 230),
                        ColumnCount = 6,
                        RowCount = 5
                    };
                    tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
                    tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
                    tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));

                    page = CreatePageOnly(pageName, pageId);
                    pageId.KryptonPage = page;
                    return CreatePageInternal(page, pageId, kryptonPageContainer, tableLayoutPanel, kryptonPanel);
                case PanelKind.Header:
                    HeaderProps headerProps = dnc.DncProps as HeaderProps;

                    row = 0;
                    col = 0;
                    CreateLabel(kryptonPageContainer, col, row, 1, "View distance");

                    col++;
                   
                    CreateTextBox(kryptonPageContainer, col, row, headerProps.ViewDistance.ToString(),
                    (o, control) =>
                    {
                        headerProps.ViewDistance = CheckFloatAndSet(o, headerProps.ViewDistance, pageId);
                        AddAsterisk(pageId.KryptonPage);
                    },
                    (prop, control) => {
                        control.Text = (prop as HeaderProps).ViewDistance.ToString();
                    }, colSpan: 1);

                    row++;
                    col = 0;

                    CreateLabel(kryptonPageContainer, col, row, 1, "Camera distance");

                    col++;

                    CreateTextBox(kryptonPageContainer, col, row, headerProps.CameraDistance.ToString(),
                    (o, control) =>
                    {
                        headerProps.CameraDistance = CheckFloatAndSet(o, headerProps.CameraDistance, pageId);
                        AddAsterisk(pageId.KryptonPage);
                    },
                    (prop, control) => {
                        control.Text = (prop as HeaderProps).ViewDistance.ToString();
                    }, colSpan: 1);

                    row++;
                    col = 0;

                    CreateLabel(kryptonPageContainer, col, row, 1, "Near clipping");

                    col++;

                    CreateTextBox(kryptonPageContainer, col, row, headerProps.NearClipping.ToString(),
                    (o, control) =>
                    {
                        headerProps.NearClipping = CheckFloatAndSet(o, headerProps.NearClipping, pageId);
                        AddAsterisk(pageId.KryptonPage);
                    },
                    (prop, control) => {
                        control.Text = (prop as HeaderProps).ViewDistance.ToString();
                    }, colSpan: 1);

                    row++;
                    col = 0;

                    CreateLabel(kryptonPageContainer, col, row, 1, "Far clipping");

                    col++;

                    CreateTextBox(kryptonPageContainer, col, row, headerProps.FarClipping.ToString(),
                    (o, control) =>
                    {
                        headerProps.FarClipping = CheckFloatAndSet(o, headerProps.FarClipping, pageId);
                        AddAsterisk(pageId.KryptonPage);
                    },
                    (prop, control) => {
                        control.Text = (prop as HeaderProps).ViewDistance.ToString();
                    }, colSpan: 1);

                    KryptonPanel kryptonPanelHeader = new KryptonPanel { Top = 0, Left = 0, Size = new Size(1000, 200) };

                    row++;
                    col = 0;
                    CreateLabel(kryptonPageContainer, col, row, 1, "Description and signature");

                    col++;
                    CreateTextBox(kryptonPageContainer, col, row, headerProps.Text,
                        (o, control) => { headerProps.Text = o.ToString(); }, 
                        (prop, control) => { control.Text = (prop as HeaderProps).Text; }, 1, 1000);

                    tableLayoutPanel = new TableLayoutPanel
                    {
                        BackColor = Color.FromArgb(187, 206, 230),
                        ColumnCount = 2,
                        RowCount = 2
                    };
                    tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
                    tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
                    //tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 400));

                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 1000));

                    page = CreatePageOnly(pageName, pageId);
                    pageId.KryptonPage = page;
                    return CreatePageInternal(page, pageId, kryptonPageContainer, tableLayoutPanel, kryptonPanelHeader);
                default:
                    throw new InvalidOperationException(nameof(CreatePage));
            }

            static void CreateLabel(List<KryptonPageContainer> kryptonPageContainer, int col, int row, int colSpan, string text)
            {
                kryptonPageContainer.Add(
                                        new KryptonPageContainer
                                        {
                                            Column = col,
                                            ColumnSpan = colSpan,
                                            Component = new KryptonLabel() { Text = text },
                                            ComponentType = ComponentType.Label,
                                            RowSpan = 1,
                                            Row = row
                                        });
            }

            static void CreateTextBox(List<KryptonPageContainer> kryptonPageContainer, int col, int row, string init, CallbackSetPropValue setterFunction, CallbackSetComponentValue componentValueFunction, int colSpan = 1, int width = 100)
            {
                var textBox = new KryptonTextBox() { Text = init, Width = width };
                textBox.TextChanged += (sender, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        setterFunction(textBox.Text, textBox);
                    }
                };

                kryptonPageContainer.Add(
                                        new KryptonPageContainer
                                        {
                                            Column = col,
                                            ColumnSpan = colSpan,
                                            Component = textBox,
                                            ComponentType = ComponentType.CheckBox,
                                            SetComponentValue = componentValueFunction,
                                            RowSpan = 1,
                                            Row = row
                                        });
            }

            static void CreateCheckBox(List<KryptonPageContainer> kryptonPageContainer, int col, int row, string initText, bool isChecked, CallbackSetPropValue setterFunction, CallbackSetComponentValue componentValueFunction, int colSpan = 1, int width = 100)
            {
                var checkBox = new CheckBox() { Text = initText, Checked = isChecked, Width = width };
                checkBox.CheckedChanged += (sender, e) =>
                {
                    setterFunction(checkBox.Checked, checkBox);
                };

                kryptonPageContainer.Add(
                                        new KryptonPageContainer
                                        {
                                            Column = col,
                                            ColumnSpan = colSpan,
                                            Component = checkBox,
                                            ComponentType = ComponentType.CheckBox,
                                            SetComponentValue = componentValueFunction,
                                            RowSpan = 1,
                                            Row = row
                                        });
            }

            // Marks tab as modified
            static void AddAsterisk(Control control)
            {
                if (!control.Text.EndsWith('*'))
                {
                    control.Text += '*';
                }
            }

            static void CreateDefaultTextBoxes(List<KryptonPageContainer> kryptonPageContainer, KryptonPageId pageId, Dnc dnc)
            {
                int col = 0;
                int row = 0;

                StandardProps standardProps = dnc.DncProps as StandardProps;

                // position
                CreateLabel(kryptonPageContainer, col, row, 2, "Position");

                row++;
                CreateLabel(kryptonPageContainer, col, row, 1, "X");

                row++;

                CreateLabel(kryptonPageContainer, col, row, 1, "Y");

                row++;

                CreateLabel(kryptonPageContainer, col, row, 1, "Z");

                col++;
                row = 1;

                CreateTextBox(kryptonPageContainer, col, row, standardProps.PositionX.ToString(), 
                (o, control) =>
                {
                    standardProps.PositionX = CheckFloatAndSet(o, standardProps.PositionX, pageId);
                    AddAsterisk(pageId.KryptonPage);
                }, 
                (prop, control) => {
                    control.Text = (prop as StandardProps).PositionX.ToString(); 
                });

                row++;

                CreateTextBox(kryptonPageContainer, col, row, standardProps.PositionY.ToString(),
                (o, control) => 
                {
                    standardProps.PositionY = CheckFloatAndSet(o, standardProps.PositionY, pageId);
                    AddAsterisk(pageId.KryptonPage);
                },
                (prop, control) => 
                {
                    control.Text = (prop as StandardProps).PositionY.ToString();
                });

                row++;

                CreateTextBox(kryptonPageContainer, col, row, standardProps.PositionZ.ToString(),
                (o, control) => 
                {
                    standardProps.PositionZ = CheckFloatAndSet(o, standardProps.PositionZ, pageId);
                    AddAsterisk(pageId.KryptonPage);
                },
                (prop, control) => 
                {
                    control.Text = (prop as StandardProps).PositionZ.ToString();
                });

                col++;
                row = 0;

                // rotation
                CreateLabel(kryptonPageContainer, col, row, 2, "Rotation");

                row++;
                CreateLabel(kryptonPageContainer, col, row, 1, "X");

                row++;
                CreateLabel(kryptonPageContainer, col, row, 1, "Y");

                row++;
                CreateLabel(kryptonPageContainer, col, row, 1, "Z");

                col++;

                row = 1;
                CreateTextBox(kryptonPageContainer, col, row, standardProps.RotationX.ToString(),
                (o, control) => 
                {
                    standardProps.RotationX = CheckFloatAndSet(o, standardProps.RotationX, pageId);
                    AddAsterisk(pageId.KryptonPage);
                },
                (prop, control) => 
                {
                    control.Text = (prop as StandardProps).RotationX.ToString();
                });

                row++;
                CreateTextBox(kryptonPageContainer, col, row, standardProps.RotationY.ToString(),
                (o, control) => 
                {
                    standardProps.RotationY = CheckFloatAndSet(o, standardProps.RotationY, pageId);
                    AddAsterisk(pageId.KryptonPage);
                },
                (prop, control) => 
                {
                    control.Text = (prop as StandardProps).RotationY.ToString();
                });

                row++;
                CreateTextBox(kryptonPageContainer, col, row, standardProps.RotationZ.ToString(),
                (o, control) => 
                {
                    standardProps.RotationZ = CheckFloatAndSet(o, standardProps.RotationZ, pageId);
                    AddAsterisk(pageId.KryptonPage);
                },
                (prop, control) => 
                {
                    control.Text = (prop as StandardProps).RotationZ.ToString();
                });

                col++;
                row = 0;

                // scaling
                CreateLabel(kryptonPageContainer, col, row, 2, "Scaling");

                row++;
                CreateLabel(kryptonPageContainer, col, row, 1, "X");

                row++;
                CreateLabel(kryptonPageContainer, col, row, 1, "Y");

                row++;
                CreateLabel(kryptonPageContainer, col, row, 1, "Z");

                col++;

                row = 1;
                CreateTextBox(kryptonPageContainer, col, row, standardProps.ScalingX.ToString(),
                (o, control) => 
                {
                    standardProps.ScalingX = CheckFloatAndSet(o, standardProps.ScalingX, pageId);
                    AddAsterisk(pageId.KryptonPage);
                },
                (prop, control) => 
                {
                    control.Text = (prop as StandardProps).ScalingX.ToString();
                });

                row++;
                CreateTextBox(kryptonPageContainer, col, row, standardProps.ScalingY.ToString(),
                (o, control) => 
                {
                    standardProps.ScalingY = CheckFloatAndSet(o, standardProps.ScalingY, pageId);
                    AddAsterisk(pageId.KryptonPage);
                },
                (prop, control) => 
                {
                    control.Text = (prop as StandardProps).ScalingY.ToString();
                });

                row++;
                CreateTextBox(kryptonPageContainer, col, row, standardProps.ScalingZ.ToString(),
                (o, control) => 
                {
                    standardProps.ScalingZ = CheckFloatAndSet(o, standardProps.ScalingZ, pageId);
                    AddAsterisk(pageId.KryptonPage);
                },
                (prop, control) => 
                {
                    control.Text = (prop as StandardProps).ScalingZ.ToString();
                });
            }
        }

        private static float CheckFloatAndSet(object obj, float originalValue, KryptonPageId pageId)
        {
            float parsed;
            if (float.TryParse(obj.ToString(), out parsed))
            {
                pageId.IsDirty = true;
                return Convert.ToSingle(parsed);
            }
            return originalValue;
        }

        private static TextEditorWrapper CreateAvalonEdit(string text, KryptonPageId pageId, Form parentForm)
        {
            ICSharpCode.AvalonEdit.TextEditor avalonEdit = new ICSharpCode.AvalonEdit.TextEditor();
            avalonEdit.Text = text;

            var avalonEditElementHost = new System.Windows.Forms.Integration.ElementHost
            {
                Dock = DockStyle.Fill,
                Child = avalonEdit,
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Right
            };
            avalonEditElementHost.Name = nameof(avalonEditElementHost);
            avalonEditElementHost.Parent = parentForm;

            RichTextModel richTextModel = new RichTextModel();
            RichTextColorizer richTextColorizer = new RichTextColorizer(richTextModel);

            avalonEdit.TextArea.TextView.LineTransformers.Add(richTextColorizer);
            avalonEdit.ShowLineNumbers = true;

            SearchPanel.Install(avalonEdit);

            //HighlightingEngine highlightingEngine = new HighlightingEngine(highlightingRuleSet);


            //avalonEdit.SyntaxHighlighting.MainRuleSet.Rules.Add(new HighlightingRule() { Color = red, Regex = new System.Text.RegularExpressions.Regex("dim_flt") });

            avalonEdit.TextChanged += (sender, eargs) =>
            {
                if (!pageId.IsDirty)
                {
                    pageId.KryptonPage.Text += "*";
                    pageId.IsDirty = true;
                }

                //richTextModel.ApplyHighlighting(0, 20, new HighlightingColor() { Foreground = new SimpleHighlightingBrush((SWM.Color)SWM.ColorConverter.ConvertFromString("Red")) });
                //avalonEdit.TextArea.TextView.Redraw(0, 20);

                //avalonEdit.TextArea.TextView.InvalidateLayer(ICSharpCode.AvalonEdit.Rendering.KnownLayer.Caret);
                //avalonEdit.Document.Lines

                //DncMethods.ScintillaTextHighlight(scintillaTextEditor.Lines[scintillaTextEditor.LineFromPosition(scintillaTextEditor.CurrentPosition)].Text, scintillaTextEditor.CurrentPosition, scintillaTextEditor);

            };

            //avalonEdit.Document.

            //HighlightingManager.Instance.RegisterHighlighting("mafiascript", new string[] { ".bin" }, new MafiaHighlight());
            avalonEdit.SyntaxHighlighting = new MafiaHighlight();

            //avalonEdit.TextArea.TextView.BackgroundRenderers
            //avalonEdit.TextArea.TextView.ElementGenerators.
            //avalonEdit.

            //parentForm.Controls.Add(avalonEditElementHost);

            //avalonEditElementHost.Show();
            //avalonEditElementHost.BringToFront();
            return new TextEditorWrapper { Editor = avalonEdit, ElementHost = avalonEditElementHost };
        }

        private static KryptonPage CreatePageInternal(KryptonPage page, KryptonPageId pageId, IEnumerable<KryptonPageContainer> mainComponents, TableLayoutPanel tableLayoutPanel = null, KryptonPanel optionalPanel = null)
        {
            TableLayoutPanel kryptonBasePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(187, 206, 230), //defaultColor,
                ColumnCount = 1,
                RowCount = 3
            };

            bool createdDefault = false;
            if (tableLayoutPanel == null)
            {
                createdDefault = true;
                tableLayoutPanel = CreateDefaultLayout();
            }

            if (optionalPanel == null)
            {
                //tableLayoutPanel.Size = new Size(1200, 500);

                if (createdDefault)
                {
                    kryptonBasePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 80));
                    kryptonBasePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
                }
                else
                {
                    kryptonBasePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
                    kryptonBasePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40));
                }
            }
            else
            {
                //tableLayoutPanel.Size = new Size(800, 300);
                kryptonBasePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
                kryptonBasePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
                kryptonBasePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
            }

            kryptonBasePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            PutOnTableLayout(mainComponents, tableLayoutPanel);

            tableLayoutPanel.RowCount++;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.Location = new Point(0, 0);

            tableLayoutPanel.Name = nameof(tableLayoutPanel);
            tableLayoutPanel.TabIndex = 0;

            // Add rich text box as the contents of the page
            kryptonBasePanel.Padding = new Padding(5);

            kryptonBasePanel.Controls.Add(tableLayoutPanel, 0, 0);

            int kryptonButtonPanelRow = 2;
            if (optionalPanel != null)
            {
                kryptonBasePanel.Controls.Add(optionalPanel, 0, 1);
            }
            else
            {
                kryptonButtonPanelRow = 1;
            }

            KryptonPanel kryptonButtonPanel = new KryptonPanel() { Dock = DockStyle.Fill, BackColor = defaultColor, ForeColor = defaultColor };
            var btnSave = CreateButton(pageId, DncMethods.BtnSaveClick, "Save", 0, 50);
            kryptonButtonPanel.Controls.Add(btnSave);

            var btnRevert = CreateButton(pageId, DncMethods.BtnRevertClick, "Revert", 150, 50);
            kryptonButtonPanel.Controls.Add(btnRevert);

            kryptonBasePanel.Controls.Add(kryptonButtonPanel, 0, kryptonButtonPanelRow);

            page.Controls.Add(kryptonBasePanel);

            // Create a close button for the page
            ButtonSpecAny bsa = new ButtonSpecAny
            {
                Tag = pageId,
                Type = PaletteButtonSpecStyle.Close
            };
            bsa.Click += PageClose;
            page.ButtonSpecs.Add(bsa);

            kryptonWorkspace.FirstCell().Pages.Add(page);
            return page;
        }

        private static KryptonPage CreatePageOnly(string pageName, KryptonPageId pageId)
        {
            // Create a new page and give it a name and image
            KryptonPage page = new KryptonPage
            {
                Text = pageName,
                TextTitle = pageName,
                TextDescription = pageName,
                Tag = pageId,
                ImageSmall = DncMethods.PageKindToGlyph(pageId.PanelKind),
                //page.ImageSmall = imageList.Images[_count % imageList.Images.Count];
                MinimumSize = new Size(200, 250)
            };
            return page;
        }

        private static TableLayoutPanel CreateDefaultLayout()
        {
            TableLayoutPanel tableLayoutPanel1 = new TableLayoutPanel
            {
                BackColor = defaultColor,
                ColumnCount = 3,
                RowCount = 1
            };
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 114F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300F));

            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            return tableLayoutPanel1;
        }

        private static void PutOnTableLayout(IEnumerable<KryptonPageContainer> mainComponents, TableLayoutPanel tableLayoutPanel)
        {
            if (mainComponents == null)
            {
                return;
            }
            foreach (var pageContainer in mainComponents)
            {
                tableLayoutPanel.Controls.Add(pageContainer.Component, pageContainer.Column, pageContainer.Row);

                tableLayoutPanel.SetColumnSpan(pageContainer.Component, pageContainer.ColumnSpan);
                tableLayoutPanel.SetRowSpan(pageContainer.Component, pageContainer.RowSpan);
            }
        }

        private static Control CreateButton(KryptonPageId kryptonPageId, EventHandler eventHandler, string btnName, int x, int y)
        {
            KryptonButton btnObj = new KryptonButton
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Location = new Point(x, y),
                Name = $"btn{btnName}",
                Size = new Size(108, 44),
                TabIndex = 1,
                Text = btnName,
                Tag = kryptonPageId,
                PaletteMode = PaletteMode.Office2010Blue
            };
            btnObj.Click += (sender, e) => { if (Exists(((KryptonPageId)((KryptonButton)sender).Tag).Dnc)) eventHandler(sender, e); };

            return btnObj;
        }

        private static void PageClose(object sender, EventArgs e)
        {
            var pageId = (sender as ButtonSpecAny).Tag as KryptonPageId;

            if (pageId.IsDirty)
            {
                if (KryptonMessageBox.Show("Close without saving?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }
            }

            var page = pageId.KryptonPage;

            activeDncs.Remove(page.Tag.ToString());
            try
            {
                KryptonWorkspaceCell currCell = kryptonWorkspace.FirstCell();
                while (!currCell.Pages.Contains(page))
                {
                    currCell = kryptonWorkspace.NextCell(currCell);
                }
                currCell.Pages.Remove(page);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private static void CloseAllPages()
        {
            KryptonWorkspaceCell currCell = kryptonWorkspace.FirstCell();
            while (kryptonWorkspace.NextCell(currCell) != null)
            {
                currCell.Pages.Clear();
                currCell = kryptonWorkspace.NextCell(currCell);
            }
            currCell.Pages.Clear();

            activeDncs.Clear();
        }

        private static void ClosePage(Dnc dnc)
        {
            KryptonWorkspaceCell currCell = kryptonWorkspace.FirstCell();
            if (currCell == null)
            {
                return;
            }

            var page = currCell.Pages.FirstOrDefault(page => ((KryptonPageId)page.Tag).PageId == DncMethods.CreatePageID(dnc));
            while (page == null)
            {
                currCell = kryptonWorkspace.NextCell(currCell);
                if (currCell != null)
                {
                    page = currCell.Pages.FirstOrDefault(page => ((KryptonPageId)page.Tag).PageId == DncMethods.CreatePageID(dnc));
                    if (page != null)
                    {
                        currCell.Pages.Remove(page);
                        return;
                    }
                }
            }
            currCell.Pages.Remove(page);
        }

        public static void SetElementGenerator(string defFilePath)
        {
            imageElementGenerator = new ImageElementGenerator(defFilePath);
        }

        public static void SetObjects(Form form, KryptonWorkspace _kryptonWorkspace)
        {
            kryptonWorkspace = _kryptonWorkspace;
            _this = form;
        }
    }
}
