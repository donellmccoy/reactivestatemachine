using System;
using System.Linq;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Markup;

namespace ReactiveStateMachine
{
    /// <summary>
    /// Trick to make databinding work on Mapping: http://blogs.msdn.com/b/mikehillberg/archive/2008/05/21/model-see_2c00_-model-do.aspx
    /// </summary>
    [ContentProperty("Mappings")]
    public class ReactiveStateMachineBehavior : Behavior<FrameworkElement>
    {
        #region private fields

        private ReactiveVisualStateManager _vsm;

        #endregion

        #region ctor

        public ReactiveStateMachineBehavior()
        {
            SetValue(MappingsProperty, new FreezableCollection<Mapping>());
        }

        #endregion
        
        #region overrides

        protected override void OnAttached()
        {
            _vsm = new ReactiveVisualStateManager(AssociatedObject);

            VisualStateManager.SetCustomVisualStateManager(AssociatedObject, _vsm);

            foreach (var mapping in Mappings)
            {
                if(mapping.StateMachine != null && !String.IsNullOrWhiteSpace(mapping.GroupName))
                    _vsm.AddMapping(mapping.GroupName, mapping.StateMachine);
                else
                    mapping.StateMachineChanged += (sender, args) =>
                    {
                        var m = sender as Mapping;
                        if (m.StateMachine != null && !String.IsNullOrWhiteSpace(m.GroupName))
                            _vsm.AddMapping(m.GroupName, m.StateMachine);
                    };
            }
        }

        #endregion
        
        #region dp

        #region Mappings

        /// <summary>
        /// Provide a FreezableCollection to enable the inheritance of the DataContext into the Mappings instances to allow DataBinding
        /// </summary>
        public FreezableCollection<Mapping> Mappings
        {
            get { return (FreezableCollection<Mapping>)GetValue(MappingsProperty); }
        }

        public static readonly DependencyProperty MappingsProperty = DependencyProperty.Register("Mappings", typeof(FreezableCollection<Mapping>), typeof(ReactiveStateMachineBehavior), null);

        #endregion

        #endregion

    }

    public class Mapping : Freezable
    {
        /// <summary>
        /// Not needed, but must be implemented
        /// </summary>
        /// <returns></returns>
        protected override Freezable CreateInstanceCore()
        {
            throw new NotImplementedException();
        }

        #region dp

        #region GroupName

        /// <summary>
        /// GroupName Dependency Property
        /// </summary>
        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register("GroupName", typeof(String), typeof(Mapping),
                new FrameworkPropertyMetadata((String)null));

        /// <summary>
        /// Gets or sets the GroupName property. This dependency property 
        /// indicates ....
        /// </summary>
        public String GroupName
        {
            get { return (String)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }

        #endregion

        #region StateMachine

        /// <summary>
        /// StateMachine Dependency Property
        /// </summary>
        public static readonly DependencyProperty StateMachineProperty =
            DependencyProperty.Register("StateMachine", typeof(IReactiveStateMachine), typeof(Mapping),
                new FrameworkPropertyMetadata((IReactiveStateMachine)null,
                    new PropertyChangedCallback(OnStateMachineChanged)));

        /// <summary>
        /// Gets or sets the StateMachine property. This dependency property 
        /// indicates ....
        /// </summary>
        public IReactiveStateMachine StateMachine
        {
            get { return (IReactiveStateMachine)GetValue(StateMachineProperty); }
            set { SetValue(StateMachineProperty, value); }
        }

        /// <summary>
        /// Handles changes to the StateMachine property.
        /// </summary>
        private static void OnStateMachineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Mapping target = (Mapping)d;
            IReactiveStateMachine oldStateMachine = (IReactiveStateMachine)e.OldValue;
            IReactiveStateMachine newStateMachine = target.StateMachine;
            target.OnStateMachineChanged(oldStateMachine, newStateMachine);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the StateMachine property.
        /// </summary>
        protected virtual void OnStateMachineChanged(IReactiveStateMachine oldStateMachine, IReactiveStateMachine newStateMachine)
        {
            OnStateMachineChanged(EventArgs.Empty);
        }

        internal event EventHandler StateMachineChanged;

        private void OnStateMachineChanged(EventArgs e)
        {
            EventHandler handler = StateMachineChanged;
            if (handler != null) handler(this, e);
        }

        #endregion



        #endregion
    }
}
