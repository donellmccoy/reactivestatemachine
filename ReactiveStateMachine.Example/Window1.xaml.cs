using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using ReactiveStateMachine;
using ReactiveStateMachine.Triggers;
using TouchStateMachine;

namespace Example
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : SurfaceWindow
    {
        public Window1()
        {
            StateMachine = new SurfaceTouchStateMachine<VisibilityStates>("VisibilityStates", VisibilityStates.Collapsed);
            TouchDownTrigger = new Subject<TouchEventArgs>();
            TouchUpTrigger = new Subject<TouchEventArgs>();
            InitializeComponent();

            Loaded += new RoutedEventHandler(Window1_Loaded);
        }

        public SurfaceTouchStateMachine<VisibilityStates> StateMachine { get; set; }


        private Trigger<TouchEventArgs> _touchDownTrigger;
        private Trigger<TouchEventArgs> _touchUpTrigger;

        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            _touchDownTrigger = new Trigger<TouchEventArgs>(TouchDownTrigger.Do(args => Console.WriteLine("TouchDown")));
            _touchUpTrigger = new Trigger<TouchEventArgs>(TouchUpTrigger.Do(args => Console.WriteLine("TouchUp")));

            StateMachine.AddTransition(_touchDownTrigger).From(VisibilityStates.Collapsed).To(VisibilityStates.FadingIn);
            StateMachine.AddAutomaticTransition(VisibilityStates.FadingIn, VisibilityStates.Visible);
            StateMachine.AddTransition(_touchUpTrigger).From(VisibilityStates.Visible).To(VisibilityStates.FadingOut);
            StateMachine.AddAutomaticTransition(VisibilityStates.FadingOut, VisibilityStates.Collapsed);

            var rsm = new WindowsTouchStateMachine<VisibilityStates>("", VisibilityStates.Collapsed);

            rsm.AddTransition(Observable.Return<TouchDevice>(null)).Where(point => rsm.First(point) && rsm.Count == 2);

            StateMachine.Start();
        }

        public Subject<TouchEventArgs> TouchDownTrigger { get; set; }
        public Subject<TouchEventArgs> TouchUpTrigger { get; set; }
    }
}
