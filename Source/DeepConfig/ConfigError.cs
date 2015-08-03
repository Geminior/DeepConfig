namespace DeepConfig
{
    using System;

    /// <summary>
    /// A class that represents an error encountered during processing of a configuration.
    /// </summary>
    [Serializable]
    public class ConfigError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigError"/> class.
        /// </summary>
        public ConfigError()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigError"/> class.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="message">The error message.</param>
        public ConfigError(ConfigErrorCode code, string message)
            : this(code, message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigError"/> class.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="message">The error message.</param>
        /// <param name="ex">An exception containing additional info about the error.</param>
        public ConfigError(ConfigErrorCode code, string message, Exception ex)
        {
            this.Code = code;
            this.Message = message;
            this.Exception = ex;
        }

        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        /// <value>The error code.</value>
        public ConfigErrorCode Code { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>The error message.</value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets an exception containing additional info about the error.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Exception { get; set; }
    }
}
