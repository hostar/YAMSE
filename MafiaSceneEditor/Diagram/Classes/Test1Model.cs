using System;
using System.Collections.Generic;
using System.Text;

namespace MafiaSceneEditor.Diagram.Classes
{
    public class Test1Model
    {
        public Root root;
    }


    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Root
    {

        private RootDesignerItem[] designerItemsField;

        private RootConnection[] connectionsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("DesignerItem", IsNullable = false)]
        public RootDesignerItem[] DesignerItems
        {
            get
            {
                return this.designerItemsField;
            }
            set
            {
                this.designerItemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Connection", IsNullable = false)]
        public RootConnection[] Connections
        {
            get
            {
                return this.connectionsField;
            }
            set
            {
                this.connectionsField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootDesignerItem
    {

        private int leftField;

        private int topField;

        private decimal widthField;

        private decimal heightField;

        private string idField;

        private byte zIndexField;

        private bool isGroupField;

        private string parentIDField;

        private string contentField;

        /// <remarks/>
        public int Left
        {
            get
            {
                return this.leftField;
            }
            set
            {
                this.leftField = value;
            }
        }

        /// <remarks/>
        public int Top
        {
            get
            {
                return this.topField;
            }
            set
            {
                this.topField = value;
            }
        }

        /// <remarks/>
        public decimal Width
        {
            get
            {
                return this.widthField;
            }
            set
            {
                this.widthField = value;
            }
        }

        /// <remarks/>
        public decimal Height
        {
            get
            {
                return this.heightField;
            }
            set
            {
                this.heightField = value;
            }
        }

        /// <remarks/>
        public string ID
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        public byte zIndex
        {
            get
            {
                return this.zIndexField;
            }
            set
            {
                this.zIndexField = value;
            }
        }

        /// <remarks/>
        public bool IsGroup
        {
            get
            {
                return this.isGroupField;
            }
            set
            {
                this.isGroupField = value;
            }
        }

        /// <remarks/>
        public string ParentID
        {
            get
            {
                return this.parentIDField;
            }
            set
            {
                this.parentIDField = value;
            }
        }

        /// <remarks/>
        public string Content
        {
            get
            {
                return this.contentField;
            }
            set
            {
                this.contentField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RootConnection
    {

        private string sourceIDField;

        private string sinkIDField;

        private string sourceConnectorNameField;

        private string sinkConnectorNameField;

        private string sourceArrowSymbolField;

        private string sinkArrowSymbolField;

        private string pathFinderField;

        private string colorField;

        private byte strokeThicknessField;

        private byte zIndexField;

        /// <remarks/>
        public string SourceID
        {
            get
            {
                return this.sourceIDField;
            }
            set
            {
                this.sourceIDField = value;
            }
        }

        /// <remarks/>
        public string SinkID
        {
            get
            {
                return this.sinkIDField;
            }
            set
            {
                this.sinkIDField = value;
            }
        }

        /// <remarks/>
        public string SourceConnectorName
        {
            get
            {
                return this.sourceConnectorNameField;
            }
            set
            {
                this.sourceConnectorNameField = value;
            }
        }

        /// <remarks/>
        public string SinkConnectorName
        {
            get
            {
                return this.sinkConnectorNameField;
            }
            set
            {
                this.sinkConnectorNameField = value;
            }
        }

        /// <remarks/>
        public string SourceArrowSymbol
        {
            get
            {
                return this.sourceArrowSymbolField;
            }
            set
            {
                this.sourceArrowSymbolField = value;
            }
        }

        /// <remarks/>
        public string SinkArrowSymbol
        {
            get
            {
                return this.sinkArrowSymbolField;
            }
            set
            {
                this.sinkArrowSymbolField = value;
            }
        }

        /// <remarks/>
        public string PathFinder
        {
            get
            {
                return this.pathFinderField;
            }
            set
            {
                this.pathFinderField = value;
            }
        }

        /// <remarks/>
        public string Color
        {
            get
            {
                return this.colorField;
            }
            set
            {
                this.colorField = value;
            }
        }

        /// <remarks/>
        public byte StrokeThickness
        {
            get
            {
                return this.strokeThicknessField;
            }
            set
            {
                this.strokeThicknessField = value;
            }
        }

        /// <remarks/>
        public byte zIndex
        {
            get
            {
                return this.zIndexField;
            }
            set
            {
                this.zIndexField = value;
            }
        }
    }


}
