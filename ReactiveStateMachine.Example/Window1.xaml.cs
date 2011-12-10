using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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
            StateMachine = new TouchStateMachine<VisibilityStates>(VisibilityStates.Collapsed, new SurfaceTouchTracker());
            InitializeComponent();

            Loaded += new RoutedEventHandler(Window1_Loaded);
        }

        public TouchStateMachine<VisibilityStates> StateMachine { get; set; }


        private EventTrigger<ContactEventArgs> _contactDownTrigger;
        private EventTrigger<ContactEventArgs> _contactUpTrigger;

        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            _contactDownTrigger = new EventTrigger<ContactEventArgs>(this, "ContactDown");
            
            _contactUpTrigger = new EventTrigger<ContactEventArgs>(this, "ContactUp");

            StateMachine.AddTransition(VisibilityStates.Visible, VisibilityStates.Collapsed, _contactUpTrigger, args => StateMachine.TouchCount == 0, args => Contacts.ReleaseContactCapture(args.Contact));
            

            //StateMachine.AddTransition(VisibilityStates.Collapsed, VisibilityStates.Visible, _contactDownTrigger, null);
            
            StateMachine.Start();
        }
    }
}
