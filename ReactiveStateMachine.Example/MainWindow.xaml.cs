using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Surface.Presentation;
using ReactiveStateMachine;
using TouchStateMachine;

namespace Example
{

    public enum VisibilityStates
    {
        Collapsed,
        FadingIn,
        Visible,
        FadingOut
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        private ReactiveStateMachine<VisibilityStates> _stateMachine;

        private IObservable<MouseEventArgs> _mouseDownTrigger;

        private ObservableTrigger<MouseEventArgs> _mouseUpTrigger;



        void MainWindow_Loaded(object sender, RoutedEventArgs args)
        {
            _stateMachine = new ReactiveStateMachine<VisibilityStates>(VisibilityStates.Collapsed);
            _mouseDownTrigger = Observable.FromEventPattern<MouseEventArgs>(this, "MouseDown").Select(evt => evt.EventArgs);
            _mouseUpTrigger = new ObservableTrigger<MouseEventArgs>(Observable.FromEventPattern<MouseEventArgs>(this, "MouseUp").Select(evt => evt.EventArgs));

            


            _stateMachine.AddTransition(VisibilityStates.Collapsed, VisibilityStates.FadingIn, _mouseDownTrigger, e =>
            {
                Console.WriteLine("MouseDown");
            });

            _stateMachine.AddTransition(VisibilityStates.FadingIn, VisibilityStates.Visible, _mouseUpTrigger, e =>
            {
                Console.WriteLine("MouseUp");
            });

            _stateMachine.AddTransition(VisibilityStates.Visible, VisibilityStates.Collapsed, _mouseDownTrigger, e =>
            {
                Console.WriteLine("MouseDown");
            });

            _stateMachine.AddEntryAction(VisibilityStates.Visible, () => Console.WriteLine("Entering Visible State"));
            _stateMachine.AddExitAction(VisibilityStates.Visible, () => Console.WriteLine("Exiting Visible State"));

            _stateMachine.Start();
        }
    }
}
