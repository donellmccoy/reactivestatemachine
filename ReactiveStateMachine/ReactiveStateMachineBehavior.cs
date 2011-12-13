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

            foreach (var mapping in Mappings.Where(mapping => !String.IsNullOrEmpty(mapping.GroupName) && mapping.StateMachine != null))
            {
                _vsm.AddMapping(mapping.GroupName, mapping.StateMachine);
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
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the StateMachine property. This dependency property 
        /// indicates ....
        /// </summary>
        public IReactiveStateMachine StateMachine
        {
            get { return (IReactiveStateMachine)GetValue(StateMachineProperty); }
            set { SetValue(StateMachineProperty, value); }
        }

        #endregion

        #endregion
    }
}
