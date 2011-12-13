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
            StateMachine = new TouchStateMachine<VisibilityStates>("VisibilityStates", VisibilityStates.Collapsed, new SurfaceTouchTracker());
            ContactDownTrigger = new Subject<ContactEventArgs>();
            ContactUpTrigger = new Subject<ContactEventArgs>();
            InitializeComponent();

            Loaded += new RoutedEventHandler(Window1_Loaded);
        }

        public TouchStateMachine<VisibilityStates> StateMachine { get; set; }


        private Trigger<ContactEventArgs> _contactDownTrigger;
        private Trigger<ContactEventArgs> _contactUpTrigger;

        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            _contactDownTrigger = new Trigger<ContactEventArgs>(ContactDownTrigger.Do( args => Console.WriteLine("ContactDown")));
            _contactUpTrigger = new Trigger<ContactEventArgs>(ContactUpTrigger.Do(args => Console.WriteLine("ContactUp")));

            StateMachine.AddTransition(_contactDownTrigger).From(VisibilityStates.Collapsed).To(VisibilityStates.FadingIn);
            StateMachine.AddAutomaticTransition(VisibilityStates.FadingIn, VisibilityStates.Visible);
            StateMachine.AddTransition(_contactUpTrigger).From(VisibilityStates.Visible).To(VisibilityStates.FadingOut);
            StateMachine.AddAutomaticTransition(VisibilityStates.FadingOut, VisibilityStates.Collapsed);

            StateMachine.Start();
        }

        public Subject<ContactEventArgs> ContactDownTrigger { get; set; }
        public Subject<ContactEventArgs> ContactUpTrigger { get; set; }
    }
}
