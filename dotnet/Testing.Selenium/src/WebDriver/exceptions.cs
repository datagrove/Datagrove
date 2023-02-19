using System.Runtime.Serialization;

namespace Datagrove.Testing.Selenium;

    public class NotFoundException : WebDriverException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class.
        /// </summary>
        public NotFoundException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class with
        /// a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public NotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class with
        /// a specified error message and a reference to the inner exception that is the
        /// cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception,
        /// or <see langword="null"/> if no inner exception is specified.</param>
        public NotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected NotFoundException(string info, StreamingContext context)
        {
        }
    }


    [Serializable]
    public class NoAlertPresentException : NotFoundException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoAlertPresentException"/> class.
        /// </summary>
        public NoAlertPresentException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoAlertPresentException"/> class with
        /// a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public NoAlertPresentException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoAlertPresentException"/> class with
        /// a specified error message and a reference to the inner exception that is the
        /// cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception,
        /// or <see langword="null"/> if no inner exception is specified.</param>
        public NoAlertPresentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoAlertPresentException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        /// information about the source or destination.</param>
        protected NoAlertPresentException(SerializationInfo info, StreamingContext context)
 
        {
        }
    }
        [Serializable]
    public class StaleElementReferenceException : Exception
    {
        public StaleElementReferenceException()
        {
        }

        public StaleElementReferenceException(string? message) : base(message)
        {
        }

        public StaleElementReferenceException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected StaleElementReferenceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
    [Serializable]
    public class NoSuchFrameException : Exception
    {
        public NoSuchFrameException()
        {
        }

        public NoSuchFrameException(string? message) : base(message)
        {
        }

        public NoSuchFrameException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected NoSuchFrameException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    public  class NoSuchElementException : Exception
    {
        public NoSuchElementException()
        {
        }

        public NoSuchElementException(string? message) : base(message)
        {
        }

        public NoSuchElementException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected NoSuchElementException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }


