namespace DeepConfig
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// Describes an exception in the configuration framework
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "This construction is by design, a code is always required.")]
    [Serializable]
    public sealed class ConfigException : Exception, ISerializable
    {
        /// <summary>
        /// Backing field for <see cref="Errors"/>
        /// </summary>
        private IEnumerable<ConfigError> _errorList;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigException"/> class.
        /// </summary>
        /// <param name="message">A custom message</param>
        /// <param name="code">The error code of the Exception</param>
        public ConfigException(string message, ConfigErrorCode code)
        {
            _errorList = new ConfigError[] { new ConfigError(code, message) };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigException"/> class.
        /// </summary>
        /// <param name="message">A custom message</param>
        /// <param name="code">The error code of the Exception</param>
        /// <param name="innerException">An inner exception to be passed along with this</param>
        public ConfigException(string message, ConfigErrorCode code, Exception innerException)
            : base(message, innerException)
        {
            _errorList = new ConfigError[] { new ConfigError(code, message, innerException) };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigException"/> class.
        /// </summary>
        /// <param name="errors">A list of errors that caused this exception.</param>
        public ConfigException(IEnumerable<ConfigError> errors)
        {
            _errorList = errors ?? new ConfigError[0];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
        [ExcludeFromCodeCoverage]
        private ConfigException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _errorList = (IEnumerable<ConfigError>)info.GetValue("ErrorList", typeof(IEnumerable<ConfigError>));
        }

        /// <summary>
        /// Gets the cause of this Exception
        /// </summary>
        public IEnumerable<ConfigError> Errors
        {
            get { return _errorList; }
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value></value>
        /// <returns>The error message that explains the reason for the exception, or an empty string("").</returns>
        public override string Message
        {
            get
            {
                return string.Join(Environment.NewLine, from e in _errorList select e.Message);
            }
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is a null reference (Nothing in Visual Basic). </exception>
        /// <PermissionSet>
        /// 	<IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*"/>
        /// 	<IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter"/>
        /// </PermissionSet>
        [ExcludeFromCodeCoverage]
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info != null)
            {
                info.AddValue("ErrorList", _errorList);
            }

            base.GetObjectData(info, context);
        }
    }
}
