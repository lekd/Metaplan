﻿#pragma checksum "..\..\..\Presentation\BrainstormCanvas.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "6834601A0E0F443138220D21BFCB1328"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Controls.Primitives;
using Microsoft.Surface.Presentation.Controls.TouchVisualizations;
using Microsoft.Surface.Presentation.Input;
using Microsoft.Surface.Presentation.Palettes;
using PieInTheSky;
using PostIt_Prototype_1.Presentation;
using PostIt_Prototype_1.Utilities;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace PostIt_Prototype_1.Presentation {
    
    
    /// <summary>
    /// BrainstormCanvas
    /// </summary>
    public partial class BrainstormCanvas : Microsoft.Surface.Presentation.Controls.SurfaceWindow, System.Windows.Markup.IComponentConnector {
        
        
        #line 17 "..\..\..\Presentation\BrainstormCanvas.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid mainGrid;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\..\Presentation\BrainstormCanvas.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid canvasesContainer;
        
        #line default
        #line hidden
        
        
        #line 19 "..\..\..\Presentation\BrainstormCanvas.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Microsoft.Surface.Presentation.Controls.SurfaceInkCanvas drawingCanvas;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\..\Presentation\BrainstormCanvas.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Microsoft.Surface.Presentation.Controls.ScatterView sv_MainCanvas;
        
        #line default
        #line hidden
        
        
        #line 39 "..\..\..\Presentation\BrainstormCanvas.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal PostIt_Prototype_1.Presentation.BrainstormingTimelineUI timelineView;
        
        #line default
        #line hidden
        
        
        #line 53 "..\..\..\Presentation\BrainstormCanvas.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal PostIt_Prototype_1.Presentation.RecycleBinUI recycleBin;
        
        #line default
        #line hidden
        
        
        #line 67 "..\..\..\Presentation\BrainstormCanvas.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal PieInTheSky.PieMenu MainMenu;
        
        #line default
        #line hidden
        
        
        #line 76 "..\..\..\Presentation\BrainstormCanvas.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal PieInTheSky.PieMenuItem menuItem_RecycleBin;
        
        #line default
        #line hidden
        
        
        #line 77 "..\..\..\Presentation\BrainstormCanvas.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal PieInTheSky.PieMenuItem menuItem_Timeline;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/PostIt_Prototype_1;component/presentation/brainstormcanvas.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Presentation\BrainstormCanvas.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal System.Delegate _CreateDelegate(System.Type delegateType, string handler) {
            return System.Delegate.CreateDelegate(delegateType, this, handler);
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.mainGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 2:
            this.canvasesContainer = ((System.Windows.Controls.Grid)(target));
            return;
            case 3:
            this.drawingCanvas = ((Microsoft.Surface.Presentation.Controls.SurfaceInkCanvas)(target));
            
            #line 22 "..\..\..\Presentation\BrainstormCanvas.xaml"
            this.drawingCanvas.StrokeCollected += new System.Windows.Controls.InkCanvasStrokeCollectedEventHandler(this.drawingCanvas_StrokeCollected);
            
            #line default
            #line hidden
            
            #line 23 "..\..\..\Presentation\BrainstormCanvas.xaml"
            this.drawingCanvas.StrokeErasing += new Microsoft.Surface.Presentation.Controls.SurfaceInkCanvasStrokeErasingEventHandler(this.drawingCanvas_StrokeErasing);
            
            #line default
            #line hidden
            
            #line 24 "..\..\..\Presentation\BrainstormCanvas.xaml"
            this.drawingCanvas.StrokeErased += new System.Windows.RoutedEventHandler(this.drawingCanvas_StrokeErased);
            
            #line default
            #line hidden
            return;
            case 4:
            this.sv_MainCanvas = ((Microsoft.Surface.Presentation.Controls.ScatterView)(target));
            
            #line 30 "..\..\..\Presentation\BrainstormCanvas.xaml"
            this.sv_MainCanvas.TouchDown += new System.EventHandler<System.Windows.Input.TouchEventArgs>(this.sv_MainCanvas_TouchDown);
            
            #line default
            #line hidden
            
            #line 31 "..\..\..\Presentation\BrainstormCanvas.xaml"
            this.sv_MainCanvas.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.sv_MainCanvas_MouseDown);
            
            #line default
            #line hidden
            return;
            case 5:
            this.timelineView = ((PostIt_Prototype_1.Presentation.BrainstormingTimelineUI)(target));
            return;
            case 6:
            this.recycleBin = ((PostIt_Prototype_1.Presentation.RecycleBinUI)(target));
            return;
            case 7:
            this.MainMenu = ((PieInTheSky.PieMenu)(target));
            return;
            case 8:
            this.menuItem_RecycleBin = ((PieInTheSky.PieMenuItem)(target));
            
            #line 76 "..\..\..\Presentation\BrainstormCanvas.xaml"
            this.menuItem_RecycleBin.Click += new System.Windows.RoutedEventHandler(this.menuItem_RecycleBin_Click);
            
            #line default
            #line hidden
            return;
            case 9:
            this.menuItem_Timeline = ((PieInTheSky.PieMenuItem)(target));
            
            #line 77 "..\..\..\Presentation\BrainstormCanvas.xaml"
            this.menuItem_Timeline.Click += new System.Windows.RoutedEventHandler(this.menuItem_Timeline_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

