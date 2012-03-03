using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ReactiveStateMachine;
using ReactiveStateMachine.Triggers;
using System.Reactive.Linq;


namespace StateMachine_Test_001
{
    //Definition der Enums mit dem Gleichen namen wie die States im Xaml
    public enum VisibilityStates
    {
        Deaktiv,
        Aktiv
        //    Collapsed,
        //FadingIn,
        //Visible,
        //FadingOut
    }

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow2 : Window
    {

        public MainWindow2()
        {
            //Wir initialisieren die StateMachine mit dem Namen der VisualStateGroup und dem StartState
            StateMachine = new ReactiveStateMachine<VisibilityStates>("VisibilityGroup", VisibilityStates.Deaktiv);

            InitializeComponent();
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        //Die StateMachine als Property damit man vom xaml drauf zugreifen kann
        public ReactiveStateMachine<VisibilityStates> StateMachine {get;set;}

        //Der Trigger muss als IObservabal gemacht werden sonnst hat man keine Zeitlichen abstände
        private IObservable<MouseEventArgs> _mouseDownTrigger;

        private IObservable<MouseEventArgs> _mouseDownTrigger_2;

        private Trigger<MouseEventArgs> _mouseUpTrigger;

        private IObservable<EventArgs> _eventTrigger;


        public delegate void TestDelegate(object sender, EventArgs args);
        public event TestDelegate TestDelegateEvent;

        void MainWindow_Loaded(object sender, RoutedEventArgs args)
        {
            //Initzialisieren des Triggers (Test ist hier das Ziel Taget MouseDown das event auf dem man Hören soll und Select gibt an welche werte Weitergeben werden bei den Events )
            _mouseDownTrigger = Observable.FromEventPattern<MouseEventArgs>(Test, "MouseDown").Select(evt => evt.EventArgs);
            _mouseDownTrigger_2 = Observable.FromEventPattern<MouseEventArgs>(Test2, "MouseDown").Select(evt => evt.EventArgs);
            _mouseUpTrigger = new Trigger<MouseEventArgs>(Observable.FromEventPattern<MouseEventArgs>(Test, "MouseUp").Select(evt => evt.EventArgs));
            _eventTrigger = (Observable.FromEventPattern<EventArgs>(this, "TestDelegateEvent").Select(evt => evt.EventArgs));


            //Hier werden die Übergänge definiert  Erst der Trigger dann vom State zum State  zusätzlich kann man nach ein DO einfügen über dem man eine Aktion ausführen kann
            StateMachine.AddTransition(_mouseDownTrigger).From(VisibilityStates.Deaktiv).To(VisibilityStates.Aktiv).Do(e=>{
                Console.WriteLine("MouseDown2");
            });
            StateMachine.AddTransition(_mouseDownTrigger).From(VisibilityStates.Aktiv).To(VisibilityStates.Deaktiv).Do(e =>
            {
                Console.WriteLine("MouseDown3");
            });

            //StateMachine.AddTransition(_mouseDownTrigger_2).Do(e =>
            //{
            //    if (TestDelegateEvent != null)
            //    {
            //        TestDelegateEvent(null);
            //        Console.WriteLine("Event gefeuert");
            //    }
            //});

            //StateMachine.AddTransition(_eventTrigger).From(VisibilityState s.Deaktiv).To(VisibilityStates.Aktiv).Do(e =>
            //{
            //    Console.WriteLine("MouseDown4");
            //});

            //Starten der StateMachine
            StateMachine.Start();
        }

        private void Test_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("Test");
        }
    }
}
