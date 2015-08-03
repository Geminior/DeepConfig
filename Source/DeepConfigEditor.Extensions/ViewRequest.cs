namespace DeepConfigEditor.Extensions
{
    using System;

    /// <summary>
    /// A message that can be posted through the <see cref="Caliburn.Micro.IEventAggregator"/> to request a view in the main view port of the editor. See remarks.
    /// </summary>
    /// <remarks>
    /// The request can be either a <see cref="Type"/> or an instance of a viewmodel type. Remember to mark all types that are intended as plugins with <see cref="IPlugin"/>.
    /// The static property <see cref="MainView"/> can be used to request a return to the main view.
    /// </remarks>
    public class ViewRequest
    {
        /// <summary>
        /// A request to revert to the main (configuration edit) view.
        /// </summary>
        public static readonly ViewRequest MainView = new ViewRequest(null);

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewRequest"/> class.
        /// </summary>
        /// <param name="instanceOrType">Either an instance of a viewmodel or the type of a viewmodel. In the latter case the type is instantiated and injected by the ioc container.</param>
        public ViewRequest(object instanceOrType)
        {
            this.RequestedViewModel = instanceOrType;
        }

        /// <summary>
        /// Gets or sets the requested view model. 
        /// </summary>
        /// <value>
        /// Either an instance of a viewmodel or the type of a viewmodel. In the latter case the type is instantiated and injected by the ioc container.
        /// </value>
        public object RequestedViewModel
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a request for a viewmodel type.
        /// </summary>
        /// <typeparam name="T">The type of viewmodel to request</typeparam>
        /// <returns>A view request for a specific type</returns>
        public static ViewRequest CreateForType<T>()
        {
            return new ViewRequest(typeof(T));
        }
    }
}
