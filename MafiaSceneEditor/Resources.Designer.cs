﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace YAMSE {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("YAMSE.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;Test1 Visibility=&quot;Visible&quot; IsHitTestVisible=&quot;False&quot; xmlns=&quot;clr-namespace:MafiaSceneEditor.Diagram;assembly=MafiaSceneEditor&quot; 
        ///xmlns:av=&quot;http://schemas.microsoft.com/winfx/2006/xaml/presentation&quot; xmlns:dd=&quot;urn:diagram-designer-ns&quot; xmlns:ddc=&quot;clr-namespace:DiagramDesigner.Controls;assembly=DiagramDesigner&quot;&gt;
        ///	&lt;dd:DesignerItem.DragThumbTemplate&gt;
        ///		&lt;av:ControlTemplate&gt;
        ///			&lt;av:Grid Background=&quot;#FFF0F8FF&quot;&gt;
        ///				&lt;av:Path Data=&quot;M0,0L60,0 60,40 0,40z&quot; Stretch=&quot;Fill&quot; Fill=&quot;#00FFFFFF&quot; Stroke=&quot;#00FFFFFF&quot; IsHitTestV [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Test1Content {
            get {
                return ResourceManager.GetString("Test1Content", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap undo {
            get {
                object obj = ResourceManager.GetObject("undo", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
    }
}
